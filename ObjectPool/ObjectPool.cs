/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Base class for Object Pools.
    /// </summary>
    [Serializable]
    public abstract class ObjectPool
    {
        internal ObjectPool()
        {
        }

        #region Validation

        /// <summary>
        ///   Checks the lower and upper bounds for the pool size.
        /// </summary>
        /// <param name="minimumPoolSize">The lower bound.</param>
        /// <param name="maximumPoolSize">The upper bound.</param>
        protected static void ValidatePoolLimits(int minimumPoolSize, int maximumPoolSize)
        {
            Contract.Requires<ArgumentOutOfRangeException>(minimumPoolSize >= 0, ErrorMessages.NegativeMinimumPoolSize);
            Contract.Requires<ArgumentOutOfRangeException>(maximumPoolSize >= 1, ErrorMessages.NegativeOrZeroMaximumPoolSize);
            Contract.Requires<ArgumentOutOfRangeException>(minimumPoolSize <= maximumPoolSize, ErrorMessages.WrongCacheBounds);
        }

        #endregion Validation

        #region Constants

        /// <summary>
        ///   The default minimum size for the pool.
        /// </summary>
        protected const int DefaultPoolMinimumSize = 5;

        /// <summary>
        ///   The default maximum size for the pool.
        /// </summary>
        protected const int DefaultPoolMaximumSize = 100;

        #endregion Constants
    }

    /// <summary>
    ///   Generic object pool.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of the object that which will be managed by the pool. The pooled object have to
    ///   be a sub-class of PooledObject.
    /// </typeparam>
    [Serializable]
    public sealed class ObjectPool<T> : ObjectPool where T : PooledObject
    {
        /// <summary>
        ///   Indication flag that states whether Adjusting operating is in progress. The type is
        ///   Int, altought it looks like it should be bool - this was done for Interlocked CAS
        ///   operation (CompareExchange).
        /// </summary>
        private int _adjustPoolSizeIsInProgressCasFlag; // 0 state false

        private readonly ConcurrentQueue<T> _pooledObjects;
        private readonly Action<PooledObject, bool> _returnToPoolAction;

        private int _maximumPoolSize;
        private int _minimumPoolSize;

        #region Public Properties

        /// <summary>
        ///   Gets or sets whether the pool should record data about how it operates.
        /// </summary>
        public bool DiagnosticsEnabled
        {
            get { return Diagnostics.Enabled; }
            set { Diagnostics.Enabled = value; }
        }

        /// <summary>
        ///   Gets the Diagnostics class for the current Object Pool.
        /// </summary>
        public ObjectPoolDiagnostics Diagnostics { get; private set; }

        /// <summary>
        ///   Gets the count of the objects currently in the pool.
        /// </summary>
        public int ObjectsInPoolCount
        {
            get { return _pooledObjects.Count; }
        }

        /// <summary>
        ///   Gets or sets the minimum number of objects in the pool.
        /// </summary>
        public int MinimumPoolSize
        {
            get { return _minimumPoolSize; }
            set
            {
                // Validating pool limits, exception is thrown if invalid.
                ValidatePoolLimits(value, _maximumPoolSize);

                _minimumPoolSize = value;

                AdjustPoolSizeToBounds();
            }
        }

        /// <summary>
        ///   Gets or sets the maximum number of objects that could be available at the same time in
        ///   the pool.
        /// </summary>
        public int MaximumPoolSize
        {
            get { return _maximumPoolSize; }
            set
            {
                // Validating pool limits, exception is thrown if invalid.
                ValidatePoolLimits(_minimumPoolSize, value);

                _maximumPoolSize = value;

                AdjustPoolSizeToBounds();
            }
        }

        /// <summary>
        ///   Gets the Factory method that will be used for creating new objects.
        /// </summary>
        public Func<T> FactoryMethod { get; private set; }

        #endregion Public Properties

        #region C'tor and Initialization code

        /// <summary>
        ///   Initializes a new pool with default settings.
        /// </summary>
        public ObjectPool()
            : this(DefaultPoolMinimumSize, DefaultPoolMaximumSize, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified minimum pool size and maximum pool size
        /// </summary>
        /// <param name="minimumPoolSize">The minimum pool size limit.</param>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        public ObjectPool(int minimumPoolSize, int maximumPoolSize)
            : this(minimumPoolSize, maximumPoolSize, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method.
        /// </summary>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ObjectPool(Func<T> factoryMethod)
            : this(DefaultPoolMinimumSize, DefaultPoolMaximumSize, factoryMethod)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method and minimum and maximum size.
        /// </summary>
        /// <param name="minimumPoolSize">The minimum pool size limit.</param>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ObjectPool(int minimumPoolSize, int maximumPoolSize, Func<T> factoryMethod)
        {
            // Validating pool limits, exception is thrown if invalid
            ValidatePoolLimits(minimumPoolSize, maximumPoolSize);

            // Assigning properties
            FactoryMethod = factoryMethod;
            _maximumPoolSize = maximumPoolSize;
            _minimumPoolSize = minimumPoolSize;

            // Initializing the internal pool data structure
            _pooledObjects = new ConcurrentQueue<T>();

            // Creating a new instance for the Diagnostics class
            Diagnostics = new ObjectPoolDiagnostics();

            // Setting the action for returning to the pool to be integrated in the pooled objects
            _returnToPoolAction = ReturnObjectToPool;

            // Initilizing objects in pool
            AdjustPoolSizeToBounds();
        }

        #endregion C'tor and Initialization code

        #region Private Methods

        private void AdjustPoolSizeToBounds()
        {
            // If there is an Adjusting operation in progress, skip and return.
            if (Interlocked.CompareExchange(ref _adjustPoolSizeIsInProgressCasFlag, 1, 0) != 0)
            {
                return;
            }

            // If we reached this point, we've set the AdjustPoolSizeIsInProgressCASFlag to 1 (true)
            // using the above CAS function. We can now safely adjust the pool size without
            // interferences :)

            // Adjusting...
            while (ObjectsInPoolCount < MinimumPoolSize)
            {
                _pooledObjects.Enqueue(CreatePooledObject());
            }

            while (ObjectsInPoolCount > MaximumPoolSize)
            {
                T dequeuedObjectToDestroy;
                if (_pooledObjects.TryDequeue(out dequeuedObjectToDestroy))
                {
                    // Diagnostics update.
                    Diagnostics.IncrementPoolOverflowCount();

                    DestroyPooledObject(dequeuedObjectToDestroy);
                }
            }

            // Finished adjusting, allowing additional callers to enter when needed.
            _adjustPoolSizeIsInProgressCasFlag = 0;
        }

        private T CreatePooledObject()
        {
            // Throws an exception if the type doesn't have default ctor - on purpose! I've could've
            // add a generic constraint with new (), but I didn't want to limit the user and force a
            // parameterless c'tor.
            var safeFactory = FactoryMethod;
            var newObject = (safeFactory != null) ? safeFactory() : Activator.CreateInstance<T>();

            // Diagnostics update.
            Diagnostics.IncrementObjectsCreatedCount();

            // Setting the 'return to pool' action in the newly created pooled object.
            newObject.ReturnToPool = _returnToPoolAction;
            return newObject;
        }

        private void DestroyPooledObject(PooledObject objectToDestroy)
        {
            // Making sure that the object is only disposed once (in case of application shutting
            // down and we don't control the order of the finalization).
            if (!objectToDestroy.Disposed)
            {
                // Deterministically release object resources, nevermind the result, we are
                // destroying the object.
                objectToDestroy.ReleaseResources();
                objectToDestroy.Disposed = true;

                // Diagnostics update.
                Diagnostics.IncrementObjectsDestroyedCount();
            }

            // The object is being destroyed, resources have been already released
            // deterministically, so we di no need the finalizer to fire.
            GC.SuppressFinalize(objectToDestroy);
        }

        #endregion Private Methods

        #region Pool Operations

        /// <summary>
        ///   Gets a monitored object from the pool.
        /// </summary>
        /// <returns></returns>
        public T GetObject()
        {
            T dequeuedObject;

            if (_pooledObjects.TryDequeue(out dequeuedObject))
            {
                // Invokes AdjustPoolSize asynchronously.
                Task.Run((Action) AdjustPoolSizeToBounds);

                // Diagnostics update.
                Diagnostics.IncrementPoolObjectHitCount();

                return dequeuedObject;
            }

            // This should not happen normally, but could be happening when there is stress on the
            // pool. No available objects in pool, create a new one and return it to the caller.
            Debug.WriteLine("Object pool failed to return a pooled object. pool is empty. consider increasing the number of minimum pooled objects.");

            // Diagnostics update.
            Diagnostics.IncrementPoolObjectMissCount();

            return CreatePooledObject();
        }

        internal void ReturnObjectToPool(PooledObject objectToReturnToPool, bool reRegisterForFinalization)
        {
            Debug.Assert(objectToReturnToPool is T);
            var returnedObject = objectToReturnToPool as T;

            // Diagnostics update.
            if (reRegisterForFinalization)
            {
                Diagnostics.IncrementObjectRessurectionCount();
            }

            // Checking that the pool is not full.
            if (ObjectsInPoolCount < MaximumPoolSize)
            {
                // Reset the object state (if implemented) before returning it to the pool. If
                // reseting the object have failed, destroy the object.
                if (!returnedObject.ResetState())
                {
                    // Diagnostics update.
                    Diagnostics.IncrementResetStateFailedCount();

                    DestroyPooledObject(returnedObject);
                    return;
                }

                // Re-registering for finalization - in case of resurrection (called from Finalize method).
                if (reRegisterForFinalization)
                {
                    GC.ReRegisterForFinalize(returnedObject);
                }

                // Diagnostics update.
                Diagnostics.IncrementReturnedToPoolCount();

                // Adding the object back to the pool.
                _pooledObjects.Enqueue(returnedObject);
            }
            else
            {
                // Diagnostics update.
                Diagnostics.IncrementPoolOverflowCount();

                // The Pool's upper limit has exceeded, there is no need to add this object back
                // into the pool and we can destroy it.
                DestroyPooledObject(returnedObject);
            }
        }

        #endregion Pool Operations

        #region Finalizer

        /// <summary>
        ///   ObjectPool destructor.
        /// </summary>
        ~ObjectPool()
        {
            // The pool is going down, releasing the resources for all objects in pool.
            foreach (var item in _pooledObjects)
            {
                DestroyPooledObject(item);
            }
        }

        #endregion Finalizer

        #region Nested type: ObjectPoolDiagnostics

        /// <summary>
        ///   A simple class to track stats during execution. By default, this class does not record anything.
        /// </summary>
        public sealed class ObjectPoolDiagnostics
        {
            #region C'tor and Initialization code

            /// <summary>
            ///   Creates a new diagnostics object, ready to record Object Pool main events.
            /// </summary>
            public ObjectPoolDiagnostics()
            {
                // By default, diagnostics are disabled.
                Enabled = false;
            }

            #endregion C'tor and Initialization code

            #region Public Properties and backing fields

            private long _objectResetFailedCount;
            private long _poolObjectHitCount;
            private long _poolObjectMissCount;
            private long _poolOverflowCount;

            private long _returnedToPoolByRessurectionCount;
            private long _ReturnedToPoolCount;
            private long _totalInstancesCreated;
            private long _totalInstancesDestroyed;

            /// <summary>
            ///   Gets or sets whether this object can record data about how the Pool operates.
            /// </summary>
            public bool Enabled { get; set; }

            /// <summary>
            ///   Gets the total count of live instances, both in the pool and in use.
            /// </summary>
            public long TotalLiveInstancesCount
            {
                get { return _totalInstancesCreated - _totalInstancesDestroyed; }
            }

            /// <summary>
            ///   Gets the count of object reset failures occured while the pool tried to re-add the
            ///   object into the pool.
            /// </summary>
            public long ObjectResetFailedCount
            {
                get { return _objectResetFailedCount; }
            }

            /// <summary>
            ///   Gets the total count of object that has been picked up by the GC, and returned to pool.
            /// </summary>
            public long ReturnedToPoolByRessurectionCount
            {
                get { return _returnedToPoolByRessurectionCount; }
            }

            /// <summary>
            ///   Gets the total count of successful accesses. The pool had a spare object to
            ///   provide to the user without creating it on demand.
            /// </summary>
            public long PoolObjectHitCount
            {
                get { return _poolObjectHitCount; }
            }

            /// <summary>
            ///   Gets the total count of unsuccessful accesses. The pool had to create an object in
            ///   order to satisfy the user request. If the number is high, consider increasing the
            ///   object minimum limit.
            /// </summary>
            public long PoolObjectMissCount
            {
                get { return _poolObjectMissCount; }
            }

            /// <summary>
            ///   Gets the total number of pooled objected created.
            /// </summary>
            public long TotalInstancesCreated
            {
                get { return _totalInstancesCreated; }
            }

            /// <summary>
            ///   Gets the total number of objects destroyes, both in case of an pool overflow, and
            ///   state corruption.
            /// </summary>
            public long TotalInstancesDestroyed
            {
                get { return _totalInstancesDestroyed; }
            }

            /// <summary>
            ///   Gets the number of objects been destroyed because the pool was full at the time of
            ///   returning the object to the pool.
            /// </summary>
            public long PoolOverflowCount
            {
                get { return _poolOverflowCount; }
            }

            /// <summary>
            ///   Gets the total count of objects that been successfully returned to the pool.
            /// </summary>
            public long ReturnedToPoolCount
            {
                get { return _ReturnedToPoolCount; }
            }

            #endregion Public Properties and backing fields

            #region Internal Methods for incrementing the counters

            internal void IncrementObjectsCreatedCount()
            {
                if (Enabled)
                {
                    Interlocked.Increment(ref _totalInstancesCreated);
                }
            }

            internal void IncrementObjectsDestroyedCount()
            {
                if (Enabled)
                {
                    Interlocked.Increment(ref _totalInstancesDestroyed);
                }
            }

            internal void IncrementPoolObjectHitCount()
            {
                if (Enabled)
                {
                    Interlocked.Increment(ref _poolObjectHitCount);
                }
            }

            internal void IncrementPoolObjectMissCount()
            {
                if (Enabled)
                {
                    Interlocked.Increment(ref _poolObjectMissCount);
                }
            }

            internal void IncrementPoolOverflowCount()
            {
                if (Enabled)
                {
                    Interlocked.Increment(ref _poolOverflowCount);
                }
            }

            internal void IncrementResetStateFailedCount()
            {
                if (Enabled)
                {
                    Interlocked.Increment(ref _objectResetFailedCount);
                }
            }

            internal void IncrementObjectRessurectionCount()
            {
                if (Enabled)
                {
                    Interlocked.Increment(ref _returnedToPoolByRessurectionCount);
                }
            }

            internal void IncrementReturnedToPoolCount()
            {
                if (Enabled)
                {
                    Interlocked.Increment(ref _ReturnedToPoolCount);
                }
            }

            #endregion Internal Methods for incrementing the counters
        }

        #endregion Nested type: ObjectPoolDiagnostics
    }
}