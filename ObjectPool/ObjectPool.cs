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
using System.Threading;

namespace CodeProject.ObjectPool
{
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
        #region Fields

        /// <summary>
        ///   Backing field for <see cref="MaximumPoolSize"/>.
        /// </summary>
        private int _maximumPoolSize;

        /// <summary>
        ///   Backing field for <see cref="MinimumPoolSize"/>.
        /// </summary>
        private int _minimumPoolSize;

        #endregion Fields

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
                return _maximumPoolSize;
            }
            set
            {
                ObjectPoolConstants.ValidatePoolLimits(MinimumPoolSize, value);
                _maximumPoolSize = value;
                AdjustPoolSizeToBounds(AdjustMode.Maximum);
            }
        }

        /// <summary>
        ///   Gets or sets the minimum number of objects in the pool.
        /// </summary>
        public int MinimumPoolSize
        {
            get
            {
                return _minimumPoolSize;
            }
            set
            {
                ObjectPoolConstants.ValidatePoolLimits(value, MaximumPoolSize);
                _minimumPoolSize = value;
                AdjustPoolSizeToBounds(AdjustMode.Minimum);
            }
        }

        /// <summary>
        ///   Gets the count of the objects currently in the pool.
        /// </summary>
        public int ObjectsInPoolCount => Math.Min(_poolSize, MaximumPoolSize); // We do this because the queue might be slightly larger, for performance reasons.

        #endregion Public Properties

        #region C'tor and Initialization code

        /// <summary>
        ///   Initializes a new pool with default settings.
        /// </summary>
        public ObjectPool()
            : this(ObjectPoolConstants.DefaultPoolMinimumSize, ObjectPoolConstants.DefaultPoolMaximumSize, null, true)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified minimum pool size and maximum pool size.
        /// </summary>
        /// <param name="minimumPoolSize">The minimum pool size limit.</param>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="minimumPoolSize"/> is less than zero,
        ///   <paramref name="maximumPoolSize"/> is less than or equal to zero, or
        ///   <paramref name="minimumPoolSize"/> is greater than <paramref name="maximumPoolSize"/>.
        /// </exception>
        public ObjectPool(int minimumPoolSize, int maximumPoolSize)
            : this(minimumPoolSize, maximumPoolSize, null, true)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method.
        /// </summary>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ObjectPool(Func<T> factoryMethod)
            : this(ObjectPoolConstants.DefaultPoolMinimumSize, ObjectPoolConstants.DefaultPoolMaximumSize, factoryMethod, true)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method and minimum and maximum size.
        /// </summary>
        /// <param name="minimumPoolSize">The minimum pool size limit.</param>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="minimumPoolSize"/> is less than zero,
        ///   <paramref name="maximumPoolSize"/> is less than or equal to zero, or
        ///   <paramref name="minimumPoolSize"/> is greater than <paramref name="maximumPoolSize"/>.
        /// </exception>
        public ObjectPool(int minimumPoolSize, int maximumPoolSize, Func<T> factoryMethod)
            : this(minimumPoolSize, maximumPoolSize, factoryMethod, true)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method and minimum and maximum size.
        /// </summary>
        /// <param name="minimumPoolSize">The minimum pool size limit.</param>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        /// <param name="adjustPoolSizeToBounds">
        ///   True if this constructor should perform initial pool adjustment, false if it will be
        ///   performed by another constructor.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="minimumPoolSize"/> is less than zero,
        ///   <paramref name="maximumPoolSize"/> is less than or equal to zero, or
        ///   <paramref name="minimumPoolSize"/> is greater than <paramref name="maximumPoolSize"/>.
        /// </exception>
        protected ObjectPool(int minimumPoolSize, int maximumPoolSize, Func<T> factoryMethod, bool adjustPoolSizeToBounds)
        {
            // Validating pool limits, exception is thrown if invalid.
            ObjectPoolConstants.ValidatePoolLimits(minimumPoolSize, maximumPoolSize);

            // Assigning properties.
            FactoryMethod = factoryMethod;
            _maximumPoolSize = maximumPoolSize;
            _minimumPoolSize = minimumPoolSize;

            // Creating a new instance for the Diagnostics class.
            Diagnostics = new ObjectPoolDiagnostics();

            // Initilizing objects in pool.
            if (adjustPoolSizeToBounds)
            {
                AdjustPoolSizeToBounds(AdjustMode.Minimum | AdjustMode.Maximum);
            }
        }

        #endregion C'tor and Initialization code

        #region Finalizer

        /// <summary>
        ///   ObjectPool destructor.
        /// </summary>
        ~ObjectPool()
        {
            // The pool is going down, releasing the resources for all objects in pool.
            ClearQueue();
        }

        #endregion Finalizer

        #region Pool Operations

        /// <summary>
        ///   Clears the pool and destroys each object stored inside it.
        /// </summary>
        public void Clear()
        {
            // Destroy all objects.
            ClearQueue();

            // Restore pool bounds.
            AdjustPoolSizeToBounds(AdjustMode.Minimum);
        }

        /// <summary>
        ///   Gets a monitored object from the pool.
        /// </summary>
        /// <returns>A monitored object from the pool.</returns>
        public T GetObject()
        {
            T pooledObject;
            if (TryDequeue(out pooledObject))
            {
                // Object found in pool.
                if (Diagnostics.Enabled)
                {
                    Diagnostics.IncrementPoolObjectHitCount();
                }
                return pooledObject;
            }

            // This should not happen normally, but could be happening when there is stress on the
            // pool. No available objects in pool, create a new one and return it to the caller.
            if (Diagnostics.Enabled)
            {
                Diagnostics.IncrementPoolObjectMissCount();
            }
            return CreatePooledObject();
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
            if (TryEnqueue(returnedObject))
            {
                if (Diagnostics.Enabled)
                {
                    Diagnostics.IncrementReturnedToPoolCount();
                }
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

        #region Low-level Pooling

#if (NET35 || PORTABLE)

        /// <summary>
        ///   The concurrent buffer containing pooled objects.
        /// </summary>
        private readonly System.Collections.Generic.Queue<T> _pooledObjects = new System.Collections.Generic.Queue<T>();

        /// <summary>
        ///   Local copy of the pool size.
        /// </summary>
        private int _poolSize;

        private void ClearQueue()
        {
            if (_pooledObjects == null)
            {
                return;
            }
            lock (_pooledObjects)
            {
                foreach (var pooledObject in _pooledObjects)
                {
                    DestroyPooledObject(pooledObject);
                }
                _pooledObjects.Clear();
                _poolSize = 0;
            }
        }

        private bool TryDequeue(out T pooledObject)
        {
            lock (_pooledObjects)
            {
                if (_pooledObjects.Count == 0)
                {
                    pooledObject = default(T);
                    return false;
                }
                pooledObject = _pooledObjects.Dequeue();
                _poolSize--;
                return true;
            }
        }

        private bool TryEnqueue(T pooledObject)
        {
            lock (_pooledObjects)
            {
                if (_pooledObjects.Count == MaximumPoolSize)
                {
                    return false;
                }
                _pooledObjects.Enqueue(pooledObject);
                _poolSize++;
                return true;
            }
        }

#else

        /// <summary>
        ///   The concurrent buffer containing pooled objects.
        /// </summary>
        private readonly System.Collections.Concurrent.ConcurrentQueue<T> _pooledObjects = new System.Collections.Concurrent.ConcurrentQueue<T>();

        /// <summary>
        ///   Local copy of the pool size. It seems that accessing the property
        ///   <see cref="System.Collections.Concurrent.ConcurrentQueue{T}.Count"/> is far slower than
        ///   keeping a local copy.
        /// </summary>
        private int _poolSize;

        private void ClearQueue()
        {
            if (_pooledObjects == null)
            {
                return;
            }
            T dequeuedObjectToDestroy;
            while (TryDequeue(out dequeuedObjectToDestroy))
            {
                DestroyPooledObject(dequeuedObjectToDestroy);
            }
        }

        private bool TryDequeue(out T pooledObject)
        {
            if (_pooledObjects.TryDequeue(out pooledObject))
            {
                Interlocked.Decrement(ref _poolSize);
                return true;
            }
            return false;
        }

        private bool TryEnqueue(T pooledObject)
        {
            if (_poolSize >= MaximumPoolSize)
            {
                return false;
            }
            _pooledObjects.Enqueue(pooledObject);
            Interlocked.Increment(ref _poolSize);
            return true;
        }

#endif

        #endregion Low-level Pooling

        #region Private Methods

        /// <summary>
        ///   Ensures that the pool respects the bounds described by <see cref="MinimumPoolSize"/>
        ///   and <see cref="MaximumPoolSize"/>.
        /// </summary>
        /// <param name="adjustMode"></param>
        protected internal void AdjustPoolSizeToBounds(AdjustMode adjustMode)
        {
            // Adjusting lower bound.
            if (((adjustMode & AdjustMode.Minimum) == AdjustMode.Minimum))
            {
                while (_poolSize < MinimumPoolSize && TryEnqueue(CreatePooledObject()))
                {
                }
            }

            // Adjusting upper bound.
            if (((adjustMode & AdjustMode.Maximum) == AdjustMode.Maximum))
            {
                T dequeuedObjectToDestroy;
                while (_poolSize > MaximumPoolSize && TryDequeue(out dequeuedObjectToDestroy))
                {
                    if (Diagnostics.Enabled)
                    {
                        Diagnostics.IncrementPoolOverflowCount();
                    }
                    DestroyPooledObject(dequeuedObjectToDestroy);
                }
            }
        }

        private T CreatePooledObject()
        {
            if (Diagnostics.Enabled)
            {
                Diagnostics.IncrementObjectsCreatedCount();
            }

            // Throws an exception if the type doesn't have default ctor - on purpose! I've could've
            // add a generic constraint with new (), but I didn't want to limit the user and force a
            // parameterless c'tor.
            var newObject = FactoryMethod?.Invoke() ?? Activator.CreateInstance<T>();

            // Setting the 'return to pool' action in the newly created pooled object.
            newObject.Handle = this;
            return newObject;
        }

        private void DestroyPooledObject(PooledObject objectToDestroy)
        {
            // Making sure that the object is only disposed once (in case of application shutting
            // down and we don't control the order of the finalization).
            if (!objectToDestroy.Disposed)
            {
                if (Diagnostics.Enabled)
                {
                    Diagnostics.IncrementObjectsDestroyedCount();
                }

                // Deterministically release object resources, nevermind the result, we are
                // destroying the object.
                objectToDestroy.ReleaseResources();
                objectToDestroy.Disposed = true;
            }

            // The object is being destroyed, resources have been already released deterministically,
            // so we di no need the finalizer to fire.
            GC.SuppressFinalize(objectToDestroy);
        }

        #endregion Private Methods
    }
}