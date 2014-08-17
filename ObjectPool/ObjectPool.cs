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
using System.Threading;
using Thrower;

namespace CodeProject.ObjectPool
{
    public abstract class ObjectPool
    {
        internal ObjectPool() {}

        #region Validation

        [Conditional(RaiseBase.UseThrowerDefine)]
        protected static void ValidatePoolLimits(int minimumPoolSize, int maximumPoolSize)
        {
            Raise<ArgumentOutOfRangeException>.If(minimumPoolSize < 0, ErrorMessages.NegativeMinimumPoolSize);
            Raise<ArgumentOutOfRangeException>.If(maximumPoolSize < 1, ErrorMessages.NegativeOrZeroMaximumPoolSize);
            Raise<ArgumentOutOfRangeException>.If(minimumPoolSize > maximumPoolSize, ErrorMessages.WrongCacheBounds);
        }

        #endregion

        #region Consts

        protected const int DefaultPoolMinimumSize = 5;
        protected const int DefaultPoolMaximumSize = 100;

        #endregion
    }

    /// <summary>
    /// Generic object pool
    /// </summary>
    /// <typeparam name="T">The type of the object that which will be managed by the pool. The pooled object have to be a sub-class of PooledObject.</typeparam>
    public sealed class ObjectPool<T> : ObjectPool where T : PooledObject
    {
        // Indication flag that states whether Adjusting operating is in progress.
        // The type is Int, altought it looks like it should be bool - this was done for Interlocked CAS operation (CompareExchange)
        private int _adjustPoolSizeIsInProgressCasFlag; // 0 state false

        private Action<PooledObject, bool> _returnToPoolAction;

        #region Public Properties

        private int _maximumPoolSize;
        private int _minimumPoolSize;

        /// <summary>
        ///   Gets or sets whether the pool should record data about how it operates.
        /// </summary>
        public bool DiagnosticsEnabled
        {
            get { return Diagnostics.Enabled; }
            set { Diagnostics.Enabled = value; }
        }

        /// <summary>
        /// Gets the Diagnostics class for the current Object Pool.
        /// </summary>
        public ObjectPoolDiagnostics Diagnostics { get; private set; }

        /// <summary>
        /// Gets the count of the objects currently in the pool.
        /// </summary>
        public int ObjectsInPoolCount
        {
            get { return PooledObjects.Count; }
        }

        /// <summary>
        /// Gets or sets the minimum number of objects in the pool.
        /// </summary>
        public int MinimumPoolSize
        {
            get { return _minimumPoolSize; }
            set
            {
                // Validating pool limits, exception is thrown if invalid
                ValidatePoolLimits(value, _maximumPoolSize);

                _minimumPoolSize = value;

                AdjustPoolSizeToBounds();
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of objects that could be available at the same time in the pool.
        /// </summary>
        public int MaximumPoolSize
        {
            get { return _maximumPoolSize; }
            set
            {
                // Validating pool limits, exception is thrown if invalid
                ValidatePoolLimits(_minimumPoolSize, value);

                _maximumPoolSize = value;

                AdjustPoolSizeToBounds();
            }
        }

        /// <summary>
        /// Gets the Factory method that will be used for creating new objects. 
        /// </summary>
        public Func<T> FactoryMethod { get; private set; }

        #endregion

        #region C'tor and Initialization code

        /// <summary>
        /// Initializes a new pool with default settings.
        /// </summary>
        public ObjectPool()
        {
            InitializePool(DefaultPoolMinimumSize, DefaultPoolMaximumSize, null);
        }

        /// <summary>
        /// Initializes a new pool with specified minimum pool size and maximum pool size
        /// </summary>
        /// <param name="minimumPoolSize">The minimum pool size limit.</param>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        public ObjectPool(int minimumPoolSize, int maximumPoolSize)
        {
            InitializePool(minimumPoolSize, maximumPoolSize, null);
        }

        /// <summary>
        /// Initializes a new pool with specified factory method.
        /// </summary>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ObjectPool(Func<T> factoryMethod)
        {
            InitializePool(DefaultPoolMinimumSize, DefaultPoolMaximumSize, factoryMethod);
        }

        /// <summary>
        /// Initializes a new pool with specified factory method and minimum and maximum size.
        /// </summary>
        /// <param name="minimumPoolSize">The minimum pool size limit.</param>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ObjectPool(int minimumPoolSize, int maximumPoolSize, Func<T> factoryMethod)
        {
            InitializePool(minimumPoolSize, maximumPoolSize, factoryMethod);
        }

        private void InitializePool(int minimumPoolSize, int maximumPoolSize, Func<T> factoryMethod)
        {
            // Validating pool limits, exception is thrown if invalid
            ValidatePoolLimits(minimumPoolSize, maximumPoolSize);

            // Assigning properties
            FactoryMethod = factoryMethod;
            _maximumPoolSize = maximumPoolSize;
            _minimumPoolSize = minimumPoolSize;

            // Initializing the internal pool data structure
            PooledObjects = new ConcurrentQueue<T>();

            // Creating a new instnce for the Diagnostics class
            Diagnostics = new ObjectPoolDiagnostics();

            // Setting the action for returning to the pool to be integrated in the pooled objects
            _returnToPoolAction = ReturnObjectToPool;

            // Initilizing objects in pool
            AdjustPoolSizeToBounds();
        }

        #endregion

        #region Private Methods

        private void AdjustPoolSizeToBounds()
        {
            // If there is an Adjusting operation in progress, skip and return.
            if (Interlocked.CompareExchange(ref _adjustPoolSizeIsInProgressCasFlag, 1, 0) != 0) {
                return;
            }
            // If we reached this point, we've set the AdjustPoolSizeIsInProgressCASFlag to 1 (true) - using the above CAS function.
            // We can now safely adjust the pool size without interferences :)

            // Adjusting...
            while (ObjectsInPoolCount < MinimumPoolSize) {
                PooledObjects.Enqueue(CreatePooledObject());
            }

            while (ObjectsInPoolCount > MaximumPoolSize) {
                T dequeuedObjectToDestroy;
                if (PooledObjects.TryDequeue(out dequeuedObjectToDestroy)) {
                    // Diagnostics update
                    Diagnostics.IncrementPoolOverflowCount();

                    DestroyPooledObject(dequeuedObjectToDestroy);
                }
            }

            // Finished adjusting, allowing additional callers to enter when needed
            _adjustPoolSizeIsInProgressCasFlag = 0;
        }

        private T CreatePooledObject()
        {
            T newObject;
            if (FactoryMethod != null) {
                newObject = FactoryMethod();
            } else {
                // Throws an exception if the type doesn't have default ctor - on purpose! I've could've add a generic constraint with new (), but I didn't want to limit the user and force a parameterless c'tor
                newObject = (T) Activator.CreateInstance(typeof(T));
            }

            // Diagnostics update
            Diagnostics.IncrementObjectsCreatedCount();

            // Setting the 'return to pool' action in the newly created pooled object
            newObject.ReturnToPool = _returnToPoolAction;
            return newObject;
        }

        private void DestroyPooledObject(PooledObject objectToDestroy)
        {
            // Making sure that the object is only disposed once (in case of application shutting down and we don't control the order of the finalization)
            if (!objectToDestroy.Disposed) {
                // Deterministically release object resources, nevermind the result, we are destroying the object
                objectToDestroy.ReleaseResources();
                objectToDestroy.Disposed = true;

                // Diagnostics update
                Diagnostics.IncrementObjectsDestroyedCount();
            }

            // The object is being destroyed, resources have been already released deterministically, so we di no need the finalizer to fire
            GC.SuppressFinalize(objectToDestroy);
        }

        #endregion

        #region Pool Operations

        /// <summary>
        /// Get a monitored object from the pool. 
        /// </summary>
        /// <returns></returns>
        public T GetObject()
        {
            T dequeuedObject;

            if (PooledObjects.TryDequeue(out dequeuedObject)) {
                // Invokes AdjustPoolSize asynchronously
                ThreadPool.QueueUserWorkItem(o => AdjustPoolSizeToBounds());

                // Diagnostics update
                Diagnostics.IncrementPoolObjectHitCount();

                return dequeuedObject;
            }

            // This should not happen normally, but could be happening when there is stress on the pool
            // No available objects in pool, create a new one and return it to the caller
            Debug.Print("Object pool failed to return a pooled object. pool is empty. consider increasing the number of minimum pooled objects.");

            // Diagnostics update
            Diagnostics.IncrementPoolObjectMissCount();

            return CreatePooledObject();
        }

        internal void ReturnObjectToPool(PooledObject objectToReturnToPool, bool reRegisterForFinalization)
        {
            var returnedObject = (T) objectToReturnToPool;

            // Diagnostics update
            if (reRegisterForFinalization) {
                Diagnostics.IncrementObjectRessurectionCount();
            }

            // Checking that the pool is not full
            if (ObjectsInPoolCount < MaximumPoolSize) {
                // Reset the object state (if implemented) before returning it to the pool. If reseting the object have failed, destroy the object
                if (!returnedObject.ResetState()) {
                    // Diagnostics update
                    Diagnostics.IncrementResetStateFailedCount();

                    DestroyPooledObject(returnedObject);
                    return;
                }

                // Re-registering for finalization - in case of resurrection (called from Finalize method)
                if (reRegisterForFinalization) {
                    GC.ReRegisterForFinalize(returnedObject);
                }

                // Diagnostics update
                Diagnostics.IncrementReturnedToPoolCount();

                // Adding the object back to the pool 
                PooledObjects.Enqueue(returnedObject);
            } else {
                // Diagnostics update
                Diagnostics.IncrementPoolOverflowCount();

                // The Pool's upper limit has exceeded, there is no need to add this object back into the pool and we can destroy it.
                DestroyPooledObject(returnedObject);
            }
        }

        #endregion

        #region Finalizer

        ~ObjectPool()
        {
            // The pool is going down, releasing the resources for all objects in pool
            foreach (var item in PooledObjects) {
                DestroyPooledObject(item);
            }
        }

        #endregion

        private ConcurrentQueue<T> PooledObjects { get; set; }

        #region Nested type: ObjectPoolDiagnostics

        public sealed class ObjectPoolDiagnostics
        {
            #region C'tor and Initialization code

            public ObjectPoolDiagnostics()
            {
                // By default, diagnostics are disabled.
                Enabled = false;
            }

            #endregion

            #region Public Properties and backing fields

            private int _ObjectResetFailedCount;
            private int _PoolObjectHitCount;
            private int _PoolObjectMissCount;
            private int _PoolOverflowCount;

            private int _ReturnedToPoolByRessurectionCount;
            private int _ReturnedToPoolCount;
            private int _TotalInstancesCreated;
            private int _TotalInstancesDestroyed;

            /// <summary>
            ///   Gets or sets whether this object can record data about how the Pool operates.
            /// </summary>
            public bool Enabled { get; set; }

            /// <summary>
            /// gets the total count of live instances, both in the pool and in use.
            /// </summary>
            public int TotalLiveInstancesCount
            {
                get { return _TotalInstancesCreated - _TotalInstancesDestroyed; }
            }

            /// <summary>
            /// gets the count of object reset failures occured while the pool tried to re-add the object into the pool.
            /// </summary>
            public int ObjectResetFailedCount
            {
                get { return _ObjectResetFailedCount; }
            }

            /// <summary>
            /// gets the total count of object that has been picked up by the GC, and returned to pool. 
            /// </summary>
            public int ReturnedToPoolByRessurectionCount
            {
                get { return _ReturnedToPoolByRessurectionCount; }
            }

            /// <summary>
            /// gets the total count of successful accesses. The pool had a spare object to provide to the user without creating it on demand.
            /// </summary>
            public int PoolObjectHitCount
            {
                get { return _PoolObjectHitCount; }
            }

            /// <summary>
            /// gets the total count of unsuccessful accesses. The pool had to create an object in order to satisfy the user request. If the number is high, consider increasing the object minimum limit.
            /// </summary>
            public int PoolObjectMissCount
            {
                get { return _PoolObjectMissCount; }
            }

            /// <summary>
            /// gets the total number of pooled objected created
            /// </summary>
            public int TotalInstancesCreated
            {
                get { return _TotalInstancesCreated; }
            }

            /// <summary>
            /// gets the total number of objects destroyes, both in case of an pool overflow, and state corruption.
            /// </summary>
            public int TotalInstancesDestroyed
            {
                get { return _TotalInstancesDestroyed; }
            }

            /// <summary>
            /// gets the number of objects been destroyed because the pool was full at the time of returning the object to the pool.
            /// </summary>
            public int PoolOverflowCount
            {
                get { return _PoolOverflowCount; }
            }

            /// <summary>
            /// gets the total count of objects that been successfully returned to the pool
            /// </summary>
            public int ReturnedToPoolCount
            {
                get { return _ReturnedToPoolCount; }
            }

            #endregion

            #region Internal Methods for incrementing the counters

            internal void IncrementObjectsCreatedCount()
            {
                if (Enabled) {
                    Interlocked.Increment(ref _TotalInstancesCreated);
                }
            }

            internal void IncrementObjectsDestroyedCount()
            {
                if (Enabled) {
                    Interlocked.Increment(ref _TotalInstancesDestroyed);
                }
            }

            internal void IncrementPoolObjectHitCount()
            {
                if (Enabled) {
                    Interlocked.Increment(ref _PoolObjectHitCount);
                }
            }

            internal void IncrementPoolObjectMissCount()
            {
                if (Enabled) {
                    Interlocked.Increment(ref _PoolObjectMissCount);
                }
            }

            internal void IncrementPoolOverflowCount()
            {
                if (Enabled) {
                    Interlocked.Increment(ref _PoolOverflowCount);
                }
            }

            internal void IncrementResetStateFailedCount()
            {
                if (Enabled) {
                    Interlocked.Increment(ref _ObjectResetFailedCount);
                }
            }

            internal void IncrementObjectRessurectionCount()
            {
                if (Enabled) {
                    Interlocked.Increment(ref _ReturnedToPoolByRessurectionCount);
                }
            }

            internal void IncrementReturnedToPoolCount()
            {
                if (Enabled) {
                    Interlocked.Increment(ref _ReturnedToPoolCount);
                }
            }

            #endregion
        }

        #endregion
    }
}