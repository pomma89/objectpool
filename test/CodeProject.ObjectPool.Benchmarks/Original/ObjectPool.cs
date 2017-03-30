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

namespace Original
{
    /// <summary>
    ///   Generic object pool
    /// </summary>
    /// <typeparam name="T">
    ///   The type of the object that which will be managed by the pool. The pooled object have to be
    ///   a sub-class of PooledObject.
    /// </typeparam>
    public class ObjectPool<T> where T : PooledObject
    {
        public class ObjectPoolDiagnostics
        {
            #region Public Properties and backing fields

            /// <summary>
            ///   gets the total count of live instances, both in the pool and in use.
            /// </summary>
            public int TotalLiveInstancesCount
            {
                get { return _TotalInstancesCreated - _TotalInstancesDestroyed; }
            }

            internal int _ObjectResetFailedCount;

            /// <summary>
            ///   gets the count of object reset failures occured while the pool tried to re-add the
            ///   object into the pool.
            /// </summary>
            public int ObjectResetFailedCount
            {
                get { return _ObjectResetFailedCount; }
            }

            internal int _ReturnedToPoolByRessurectionCount;

            /// <summary>
            ///   gets the total count of object that has been picked up by the GC, and returned to pool.
            /// </summary>
            public int ReturnedToPoolByRessurectionCount
            {
                get { return _ReturnedToPoolByRessurectionCount; }
            }

            internal int _PoolObjectHitCount;

            /// <summary>
            ///   gets the total count of successful accesses. The pool had a spare object to provide
            ///   to the user without creating it on demand.
            /// </summary>
            public int PoolObjectHitCount
            {
                get { return _PoolObjectHitCount; }
            }

            internal int _PoolObjectMissCount;

            /// <summary>
            ///   gets the total count of unsuccessful accesses. The pool had to create an object in
            ///   order to satisfy the user request. If the number is high, consider increasing the
            ///   object minimum limit.
            /// </summary>
            public int PoolObjectMissCount
            {
                get { return _PoolObjectMissCount; }
            }

            internal int _TotalInstancesCreated;

            /// <summary>
            ///   gets the total number of pooled objected created
            /// </summary>
            public int TotalInstancesCreated
            {
                get { return _TotalInstancesCreated; }
            }

            internal int _TotalInstancesDestroyed;

            /// <summary>
            ///   gets the total number of objects destroyes, both in case of an pool overflow, and
            ///   state corruption.
            /// </summary>
            public int TotalInstancesDestroyed
            {
                get { return _TotalInstancesDestroyed; }
            }

            internal int _PoolOverflowCount;

            /// <summary>
            ///   gets the number of objects been destroyed because the pool was full at the time of
            ///   returning the object to the pool.
            /// </summary>
            public int PoolOverflowCount
            {
                get { return _PoolOverflowCount; }
            }

            internal int _ReturnedToPoolCount;

            /// <summary>
            ///   gets the total count of objects that been successfully returned to the pool
            /// </summary>
            public int ReturnedToPoolCount
            {
                get { return _ReturnedToPoolCount; }
            }

            #endregion Public Properties and backing fields

            #region Internal Methods for incrementing the counters

            internal void IncrementObjectsCreatedCount()
            {
                Interlocked.Increment(ref _TotalInstancesCreated);
            }

            internal void IncrementObjectsDestroyedCount()
            {
                Interlocked.Increment(ref _TotalInstancesDestroyed);
            }

            internal void IncrementPoolObjectHitCount()
            {
                Interlocked.Increment(ref _PoolObjectHitCount);
            }

            internal void IncrementPoolObjectMissCount()
            {
                Interlocked.Increment(ref _PoolObjectMissCount);
            }

            internal void IncrementPoolOverflowCount()
            {
                Interlocked.Increment(ref _PoolOverflowCount);
            }

            internal void IncrementResetStateFailedCount()
            {
                Interlocked.Increment(ref _ObjectResetFailedCount);
            }

            internal void IncrementObjectRessurectionCount()
            {
                Interlocked.Increment(ref _ReturnedToPoolByRessurectionCount);
            }

            internal void IncrementReturnedToPoolCount()
            {
                Interlocked.Increment(ref _ReturnedToPoolCount);
            }

            #endregion Internal Methods for incrementing the counters
        }

        #region Consts

        private const int DefaultPoolMinimumSize = 5;
        private const int DefaultPoolMaximumSize = 100;

        #endregion Consts

        #region Private Members

        // Pool internal data structure
        private ConcurrentQueue<T> PooledObjects { get; set; }

        // Action to be passed to the pooled objects to allow them to return to the pool
        private Action<PooledObject, bool> ReturnToPoolAction;

        // Indication flag that states whether Adjusting operating is in progress. The type is Int,
        // altought it looks like it should be bool - this was done for Interlocked CAS operation (CompareExchange)
        private int AdjustPoolSizeIsInProgressCASFlag = 0; // 0 state false

        #endregion Private Members

        #region Public Properties

        /// <summary>
        ///   Gets the Diagnostics class for the current Object Pool.
        /// </summary>
        public ObjectPoolDiagnostics Diagnostics { get; private set; }

        /// <summary>
        ///   Gets the count of the objects currently in the pool.
        /// </summary>
        public int ObjectsInPoolCount
        {
            get { return PooledObjects.Count; }
        }

        private int _MinimumPoolSize;

        /// <summary>
        ///   Gets or sets the minimum number of objects in the pool.
        /// </summary>
        public int MinimumPoolSize
        {
            get { return _MinimumPoolSize; }
            set
            {
                // Validating pool limits, exception is thrown if invalid
                ValidatePoolLimits(value, _MaximumPoolSize);

                _MinimumPoolSize = value;

                AdjustPoolSizeToBounds();
            }
        }

        private int _MaximumPoolSize;

        /// <summary>
        ///   Gets or sets the maximum number of objects that could be available at the same time in
        ///   the pool.
        /// </summary>
        public int MaximumPoolSize
        {
            get { return _MaximumPoolSize; }
            set
            {
                // Validating pool limits, exception is thrown if invalid
                ValidatePoolLimits(_MinimumPoolSize, value);

                _MaximumPoolSize = value;

                AdjustPoolSizeToBounds();
            }
        }

        private Func<T> _FactoryMethod = null;

        /// <summary>
        ///   Gets the Factory method that will be used for creating new objects.
        /// </summary>
        public Func<T> FactoryMethod
        {
            get { return _FactoryMethod; }
            private set { _FactoryMethod = value; }
        }

        #endregion Public Properties

        #region C'tor and Initialization code

        /// <summary>
        ///   Initializes a new pool with default settings.
        /// </summary>
        public ObjectPool()
        {
            InitializePool(DefaultPoolMinimumSize, DefaultPoolMaximumSize, null);
        }

        /// <summary>
        ///   Initializes a new pool with specified minimum pool size and maximum pool size
        /// </summary>
        /// <param name="minimumPoolSize">The minimum pool size limit.</param>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        public ObjectPool(int minimumPoolSize, int maximumPoolSize)
        {
            InitializePool(minimumPoolSize, maximumPoolSize, null);
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method.
        /// </summary>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ObjectPool(Func<T> factoryMethod)
        {
            InitializePool(DefaultPoolMinimumSize, DefaultPoolMaximumSize, factoryMethod);
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method and minimum and maximum size.
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
            _MaximumPoolSize = maximumPoolSize;
            _MinimumPoolSize = minimumPoolSize;

            // Initializing the internal pool data structure
            PooledObjects = new ConcurrentQueue<T>();

            // Creating a new instnce for the Diagnostics class
            Diagnostics = new ObjectPoolDiagnostics();

            // Setting the action for returning to the pool to be integrated in the pooled objects
            ReturnToPoolAction = ReturnObjectToPool;

            // Initilizing objects in pool
            AdjustPoolSizeToBounds();
        }

        #endregion C'tor and Initialization code

        #region Private Methods

        private void ValidatePoolLimits(int minimumPoolSize, int maximumPoolSize)
        {
            if (minimumPoolSize < 0)
            {
                throw new ArgumentException("Minimum pool size must be greater or equals to zero.");
            }

            if (maximumPoolSize < 1)
            {
                throw new ArgumentException("Maximum pool size must be greater than zero.");
            }

            if (minimumPoolSize > maximumPoolSize)
            {
                throw new ArgumentException("Maximum pool size must be greater than the maximum pool size.");
            }
        }

        private void AdjustPoolSizeToBounds()
        {
            // If there is an Adjusting operation in progress, skip and return.
            if (Interlocked.CompareExchange(ref AdjustPoolSizeIsInProgressCASFlag, 1, 0) == 0)
            {
                // If we reached this point, we've set the AdjustPoolSizeIsInProgressCASFlag to 1
                // (true) - using the above CAS function We can now safely adjust the pool size
                // without interferences

                // Adjusting...
                while (ObjectsInPoolCount < MinimumPoolSize)
                {
                    PooledObjects.Enqueue(CreatePooledObject());
                }

                while (ObjectsInPoolCount > MaximumPoolSize)
                {
                    T dequeuedObjectToDestroy;
                    if (PooledObjects.TryDequeue(out dequeuedObjectToDestroy))
                    {
                        // Diagnostics update
                        Diagnostics.IncrementPoolOverflowCount();

                        DestroyPooledObject(dequeuedObjectToDestroy);
                    }
                }

                // Finished adjusting, allowing additional callers to enter when needed
                AdjustPoolSizeIsInProgressCASFlag = 0;
            }
        }

        private T CreatePooledObject()
        {
            T newObject;
            if (FactoryMethod != null)
            {
                newObject = FactoryMethod();
            }
            else
            {
                // Throws an exception if the type doesn't have default ctor - on purpose! I've
                // could've add a generic constraint with new (), but I didn't want to limit the user
                // and force a parameterless c'tor
                newObject = (T)Activator.CreateInstance(typeof(T));
            }

            // Diagnostics update
            Diagnostics.IncrementObjectsCreatedCount();

            // Setting the 'return to pool' action in the newly created pooled object
            newObject.ReturnToPool = (Action<PooledObject, bool>)ReturnToPoolAction;
            return newObject;
        }

        private void DestroyPooledObject(PooledObject objectToDestroy)
        {
            // Making sure that the object is only disposed once (in case of application shutting
            // down and we don't control the order of the finalization)
            if (!objectToDestroy.Disposed)
            {
                // Deterministically release object resources, nevermind the result, we are
                // destroying the object
                objectToDestroy.ReleaseResources();
                objectToDestroy.Disposed = true;

                // Diagnostics update
                Diagnostics.IncrementObjectsDestroyedCount();
            }

            // The object is being destroyed, resources have been already released deterministically,
            // so we di no need the finalizer to fire
            GC.SuppressFinalize(objectToDestroy);
        }

        #endregion Private Methods

        #region Pool Operations

        /// <summary>
        ///   Get a monitored object from the pool.
        /// </summary>
        /// <returns></returns>
        public T GetObject()
        {
            T dequeuedObject = null;

            if (PooledObjects.TryDequeue(out dequeuedObject))
            {
                // Invokes AdjustPoolSize asynchronously
                ThreadPool.QueueUserWorkItem(new WaitCallback((o) => AdjustPoolSizeToBounds()));

                // Diagnostics update
                Diagnostics.IncrementPoolObjectHitCount();

                return dequeuedObject;
            }
            else
            {
                // This should not happen normally, but could be happening when there is stress on
                // the pool No available objects in pool, create a new one and return it to the caller
                Debug.Print("Object pool failed to return a pooled object. pool is empty. consider increasing the number of minimum pooled objects.");

                // Diagnostics update
                Diagnostics.IncrementPoolObjectMissCount();

                return CreatePooledObject();
            }
        }

        internal void ReturnObjectToPool(PooledObject objectToReturnToPool, bool reRegisterForFinalization)
        {
            T returnedObject = (T)objectToReturnToPool;

            // Diagnostics update
            if (reRegisterForFinalization) Diagnostics.IncrementObjectRessurectionCount();

            // Checking that the pool is not full
            if (ObjectsInPoolCount < MaximumPoolSize)
            {
                // Reset the object state (if implemented) before returning it to the pool. If
                // reseting the object have failed, destroy the object
                if (!returnedObject.ResetState())
                {
                    // Diagnostics update
                    Diagnostics.IncrementResetStateFailedCount();

                    DestroyPooledObject(returnedObject);
                    return;
                }

                // re-registering for finalization - in case of resurrection (called from Finalize method)
                if (reRegisterForFinalization)
                {
                    GC.ReRegisterForFinalize(returnedObject);
                }

                // Diagnostics update
                Diagnostics.IncrementReturnedToPoolCount();

                // Adding the object back to the pool
                PooledObjects.Enqueue(returnedObject);
            }
            else
            {
                // Diagnostics update
                Diagnostics.IncrementPoolOverflowCount();

                //The Pool's upper limit has exceeded, there is no need to add this object back into the pool and we can destroy it.
                DestroyPooledObject(returnedObject);
            }
        }

        #endregion Pool Operations

        #region Finalizer

        ~ObjectPool()
        {
            // The pool is going down, releasing the resources for all objects in pool
            foreach (var item in PooledObjects)
            {
                DestroyPooledObject(item);
            }
        }

        #endregion Finalizer
    }
}