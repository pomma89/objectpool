/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

using CodeProject.ObjectPool.Core;
using System;
using System.Linq;
using System.Threading;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Constants for all Object Pools.
    /// </summary>
    public static class ObjectPool
    {
        /// <summary>
        ///   The default maximum size for the pool. It is set to 16.
        /// </summary>
        public const int DefaultPoolMaximumSize = 16;
    }

    /// <summary>
    ///   Generic object pool.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of the object that which will be managed by the pool. The pooled object have to be
    ///   a sub-class of PooledObject.
    /// </typeparam>
    public class ObjectPool<T> : IObjectPool<T>, IObjectPoolHandle
        where T : PooledObject
    {
        #region C'tor and Initialization code

        /// <summary>
        ///   Initializes a new pool with default settings.
        /// </summary>
        public ObjectPool()
            : this(ObjectPool.DefaultPoolMaximumSize, null, null, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified maximum pool size.
        /// </summary>
        /// <param name="maximumPoolSize">The maximum pool size limit.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="maximumPoolSize"/> is less than or equal to zero.
        /// </exception>
        public ObjectPool(int maximumPoolSize)
            : this(maximumPoolSize, null, null, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method.
        /// </summary>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ObjectPool(Func<T> factoryMethod)
            : this(ObjectPool.DefaultPoolMaximumSize, factoryMethod, null, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method and maximum size.
        /// </summary>
        /// <param name="maximumPoolSize">The maximum pool size limit.</param>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="maximumPoolSize"/> is less than or equal to zero.
        /// </exception>
        public ObjectPool(int maximumPoolSize, Func<T> factoryMethod)
            : this(maximumPoolSize, factoryMethod, null, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified eviction settings.
        /// </summary>
        /// <param name="evictionSettings">Settings for the validation and eviction job.</param>
        public ObjectPool(EvictionSettings evictionSettings)
            : this(ObjectPool.DefaultPoolMaximumSize, null, evictionSettings, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method, maximum size, eviction timer and settings.
        /// </summary>
        /// <param name="maximumPoolSize">The maximum pool size limit.</param>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        /// <param name="evictionSettings">Settings for the validation and eviction job.</param>
        /// <param name="evictionTimer">
        ///   The eviction timer used to schedule an async validation and eviction job.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="maximumPoolSize"/> is less than or equal to zero.
        /// </exception>
        public ObjectPool(int maximumPoolSize, Func<T> factoryMethod, EvictionSettings evictionSettings, IEvictionTimer evictionTimer)
        {
            // Preconditions
            if (maximumPoolSize <= 0) throw new ArgumentOutOfRangeException(nameof(maximumPoolSize), ErrorMessages.NegativeOrZeroMaximumPoolSize);

            // Throws an exception if the type does not have default constructor - on purpose! We
            // could have added a generic constraint with new (), but we did not want to limit the
            // user and force a parameterless constructor.
            FactoryMethod = factoryMethod ?? Activator.CreateInstance<T>;

            // Max pool size.
            MaximumPoolSize = maximumPoolSize;

            // Creating a new instance for the Diagnostics class.
            Diagnostics = new ObjectPoolDiagnostics();

            // Use specified timer or create a new one if missing.
            _evictionTimer = evictionTimer ?? new EvictionTimer();
            StartEvictor(evictionSettings ?? EvictionSettings.Default);
        }

        #endregion C'tor and Initialization code

        #region Public Properties

        /// <summary>
        ///   Gets the Diagnostics class for the current Object Pool, whose goal is to record data
        ///   about how the pool operates. By default, however, an object pool records anything; you
        ///   have to enable it through the <see cref="ObjectPoolDiagnostics.Enabled"/> property.
        /// </summary>
        public ObjectPoolDiagnostics Diagnostics { get; set; }

        /// <summary>
        ///   Gets the Factory method that will be used for creating new objects.
        /// </summary>
        public Func<T> FactoryMethod { get; protected set; }

        /// <summary>
        ///   Gets or sets the maximum number of objects that could be available at the same time in
        ///   the pool.
        /// </summary>
        public int MaximumPoolSize
        {
            get
            {
                return PooledObjects.Capacity;
            }
            set
            {
                // Preconditions
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), ErrorMessages.NegativeOrZeroMaximumPoolSize);

                // Resize the pool and destroy exceeding items, if any.
                foreach (var exceedingItem in PooledObjects.Resize(value))
                {
                    DestroyPooledObject(exceedingItem);
                }
            }
        }

        /// <summary>
        ///   Gets the count of the objects currently in the pool.
        /// </summary>
        public int ObjectsInPoolCount => PooledObjects.Count;

        /// <summary>
        ///   The concurrent buffer containing pooled objects.
        /// </summary>
        protected PooledObjectBuffer<T> PooledObjects { get; } = new PooledObjectBuffer<T>();

        #endregion Public Properties

        #region Finalizer

        /// <summary>
        ///   ObjectPool destructor.
        /// </summary>
        ~ObjectPool()
        {
            // The pool is going down, releasing the resources for all objects in pool.
            Clear();

            // Dispose the eviction timer, if any.
            _evictionTimer?.Dispose();
        }

        #endregion Finalizer

        #region Pool Operations

        /// <summary>
        ///   Clears the pool and destroys each object stored inside it.
        /// </summary>
        public void Clear()
        {
            // Destroy all objects, one by one.
            while (PooledObjects.TryDequeue(out T dequeuedObjectToDestroy))
            {
                DestroyPooledObject(dequeuedObjectToDestroy);
            }
        }

        /// <summary>
        ///   Gets a monitored object from the pool.
        /// </summary>
        /// <returns>A monitored object from the pool.</returns>
        public T GetObject()
        {
            while (true)
            {
                if (PooledObjects.TryDequeue(out T pooledObject))
                {
                    // Object found in pool.
                    if (Diagnostics.Enabled) Diagnostics.IncrementPoolObjectHitCount();
                }
                else
                {
                    // This should not happen normally, but could be happening when there is stress
                    // on the pool. No available objects in pool, create a new one and return it to
                    // the caller.
                    if (Diagnostics.Enabled) Diagnostics.IncrementPoolObjectMissCount();
                    pooledObject = CreatePooledObject();
                }

                if (!pooledObject.ValidateObject(PooledObjectValidationContext.Outbound(pooledObject)))
                {
                    DestroyPooledObject(pooledObject);
                    continue;
                }

                // Change the state of the pooled object, marking it as reserved. We will mark it as
                // available as soon as the object will return to the pool.
                pooledObject.PooledObjectInfo.State = PooledObjectState.Reserved;
                return pooledObject;
            }
        }

        void IObjectPoolHandle.ReturnObjectToPool(PooledObject objectToReturnToPool, bool reRegisterForFinalization)
        {
            var returnedObject = objectToReturnToPool as T;

            if (reRegisterForFinalization && Diagnostics.Enabled)
            {
                Diagnostics.IncrementObjectResurrectionCount();
            }

            // Reset the object state (if implemented) before returning it to the pool. If resetting
            // the object have failed, destroy the object.
            if (returnedObject != null && !returnedObject.ResetState())
            {
                if (Diagnostics.Enabled)
                {
                    Diagnostics.IncrementResetStateFailedCount();
                }
                DestroyPooledObject(returnedObject);
                return;
            }

            // Re-registering for finalization - in case of resurrection (called from Finalize method).
            if (reRegisterForFinalization)
            {
                GC.ReRegisterForFinalize(returnedObject);
            }

            // Trying to add the object back to the pool.
            if (PooledObjects.TryEnqueue(returnedObject))
            {
                if (Diagnostics.Enabled)
                {
                    Diagnostics.IncrementReturnedToPoolCount();
                }

                // While adding the object back to the pool, we mark it as available.
                returnedObject.PooledObjectInfo.State = PooledObjectState.Available;
            }
            else
            {
                // The Pool's upper limit has exceeded, there is no need to add this object back into
                // the pool and we can destroy it.
                if (Diagnostics.Enabled)
                {
                    Diagnostics.IncrementPoolOverflowCount();
                }
                DestroyPooledObject(returnedObject);
            }
        }

        #endregion Pool Operations

        #region Protected Methods

        /// <summary>
        ///   Used to schedule an async validation and eviction job.
        /// </summary>
        private readonly IEvictionTimer _evictionTimer;

        /// <summary>
        ///   Keeps track of last pooled object ID.
        /// </summary>
        private int _lastPooledObjectId;

        /// <summary>
        ///   Stores the ticket returned by
        ///   <see cref="IEvictionTimer.Schedule(Action, TimeSpan, TimeSpan)"/>, in order to be able
        ///   to cancel the scheduled eviction action, if needed.
        /// </summary>
        private Guid _evictionActionTicket;

        /// <summary>
        ///   Creates a new pooled object, initializing its info.
        /// </summary>
        /// <returns>A new pooled object.</returns>
        protected virtual T CreatePooledObject()
        {
            if (Diagnostics.Enabled)
            {
                Diagnostics.IncrementObjectsCreatedCount();
            }

            if (FactoryMethod == null)
            {
                // A child class has deleted our factory method. Therefore, we can only return null.
                return null;
            }

            var newObject = FactoryMethod();

            // Setting the 'return to pool' action and other properties in the newly created pooled object.
            newObject.PooledObjectInfo.Id = Interlocked.Increment(ref _lastPooledObjectId);
            newObject.PooledObjectInfo.State = PooledObjectState.Available;
            newObject.PooledObjectInfo.Handle = this;

            return newObject;
        }

        /// <summary>
        ///   Destroys given pooled object, disposing its resources.
        /// </summary>
        /// <param name="objectToDestroy">The pooled object that should be destroyed.</param>
        protected void DestroyPooledObject(PooledObject objectToDestroy)
        {
            // Making sure that the object is only disposed once (in case of application shutting
            // down and we don't control the order of the finalization).
            if (objectToDestroy.PooledObjectInfo.State != PooledObjectState.Disposed)
            {
                if (Diagnostics.Enabled)
                {
                    Diagnostics.IncrementObjectsDestroyedCount();
                }

                // Deterministically release object resources, nevermind the result, we are
                // destroying the object.
                objectToDestroy.ReleaseResources();
                objectToDestroy.PooledObjectInfo.State = PooledObjectState.Disposed;
            }

            // The object is being destroyed, resources have been already released deterministically,
            // so we di no need the finalizer to fire.
            GC.SuppressFinalize(objectToDestroy);
        }

        /// <summary>
        ///   Starts the evictor process, if enabled.
        /// </summary>
        /// <param name="settings">Eviction settings.</param>
        protected void StartEvictor(EvictionSettings settings)
        {
            if (settings.Enabled)
            {
                lock (this)
                {
                    if (_evictionActionTicket != Guid.Empty)
                    {
                        // Cancel previous eviction action.
                        _evictionTimer.Cancel(_evictionActionTicket);
                    }
                    _evictionActionTicket = _evictionTimer.Schedule(() =>
                    {
                        // Local copy, since the buffer might change.
                        var pooledObjects = PooledObjects.ToArray();

                        // All items which are not valid will be destroyed.
                        foreach (var pooledObject in pooledObjects)
                        {
                            if (!pooledObject.ValidateObject(PooledObjectValidationContext.Outbound(pooledObject)) && PooledObjects.TryRemove(pooledObject))
                            {
                                DestroyPooledObject(pooledObject);
                            }
                        }
                    }, settings.Delay, settings.Period);
                }
            }
        }

        #endregion Protected Methods
    }
}