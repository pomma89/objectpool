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
using PommaLabs.Thrower;
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
                Raise.ArgumentOutOfRangeException.If(value < 1, nameof(value), ErrorMessages.NegativeOrZeroMaximumPoolSize);

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

        #region C'tor and Initialization code

        /// <summary>
        ///   Initializes a new pool with default settings.
        /// </summary>
        public ObjectPool()
            : this(ObjectPool.DefaultPoolMaximumSize, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified maximum pool size.
        /// </summary>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="maximumPoolSize"/> is less than or equal to zero.
        /// </exception>
        public ObjectPool(int maximumPoolSize)
            : this(maximumPoolSize, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method.
        /// </summary>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ObjectPool(Func<T> factoryMethod)
            : this(ObjectPool.DefaultPoolMaximumSize, factoryMethod)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method and maximum size.
        /// </summary>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="maximumPoolSize"/> is less than or equal to zero.
        /// </exception>
        public ObjectPool(int maximumPoolSize, Func<T> factoryMethod)
        {
            // Preconditions
            Raise.ArgumentOutOfRangeException.If(maximumPoolSize < 1, nameof(maximumPoolSize), ErrorMessages.NegativeOrZeroMaximumPoolSize);

            // Assigning properties.
            FactoryMethod = factoryMethod;
            MaximumPoolSize = maximumPoolSize;

            // Creating a new instance for the Diagnostics class.
            Diagnostics = new ObjectPoolDiagnostics();
        }

        #endregion C'tor and Initialization code

        #region Finalizer

        /// <summary>
        ///   ObjectPool destructor.
        /// </summary>
        ~ObjectPool()
        {
            // The pool is going down, releasing the resources for all objects in pool.
            Clear();
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
            if (PooledObjects.TryDequeue(out T pooledObject))
            {
                // Object found in pool.
                if (Diagnostics.Enabled) Diagnostics.IncrementPoolObjectHitCount();
            }
            else
            {
                // This should not happen normally, but could be happening when there is stress on
                // the pool. No available objects in pool, create a new one and return it to the caller.
                if (Diagnostics.Enabled) Diagnostics.IncrementPoolObjectMissCount();
                pooledObject = CreatePooledObject();
            }

            // Change the state of the pooled object, marking it as reserved. We will mark it as
            // available as soon as the object will return to the pool.
            pooledObject.PooledObjectInfo.State = PooledObjectState.Reserved;

            return pooledObject;
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

        #region Private Methods

        /// <summary>
        ///   Keeps track of last pooled object ID.
        /// </summary>
        private int _lastPooledObjectId;

        private T CreatePooledObject()
        {
            if (Diagnostics.Enabled)
            {
                Diagnostics.IncrementObjectsCreatedCount();
            }

            // Throws an exception if the type does not have default constructor - on purpose! We
            // could have added a generic constraint with new (), but we did not want to limit the
            // user and force a parameterless constructor.
            var newObject = FactoryMethod?.Invoke() ?? Activator.CreateInstance<T>();

            // Setting the 'return to pool' action and other properties in the newly created pooled object.
            newObject.PooledObjectInfo.Id = Interlocked.Increment(ref _lastPooledObjectId);
            newObject.PooledObjectInfo.State = PooledObjectState.Available;
            newObject.PooledObjectInfo.Handle = this;
            return newObject;
        }

        private void DestroyPooledObject(PooledObject objectToDestroy)
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

        #endregion Private Methods
    }
}