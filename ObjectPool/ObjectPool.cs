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
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using CodeProject.ObjectPool.Contracts;
using Finsa.CodeServices.Common.Collections.Concurrent;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Generic object pool.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of the object that which will be managed by the pool. The pooled object have to
    ///   be a sub-class of PooledObject.
    /// </typeparam>
    public sealed class ObjectPool<T> : IObjectPool<T> where T : PooledObject
    {
        /// <summary>
        ///   Indication flag that states whether Adjusting operating is in progress. The type is
        ///   Int, altought it looks like it should be bool - this was done for Interlocked CAS
        ///   operation (CompareExchange).
        /// </summary>
        private int _adjustPoolSizeIsInProgressCasFlag; // 0 state false

        /// <summary>
        ///   The action performed when an object returns to the pool.
        /// </summary>
        private readonly Action<PooledObject, bool> _returnToPoolAction;

        /// <summary>
        ///   The concurrent queue containing pooled objects.
        /// </summary>
        private readonly ConcurrentQueue<T> _pooledObjects = new ConcurrentQueue<T>();

        private int _maximumPoolSize;
        private int _minimumPoolSize;

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
        public Func<T> FactoryMethod { get; private set; }

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
                _maximumPoolSize = value;
                AdjustPoolSizeToBounds();
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
                _minimumPoolSize = value;
                AdjustPoolSizeToBounds();
            }
        }

        /// <summary>
        ///   Gets the count of the objects currently in the pool.
        /// </summary>
        public int ObjectsInPoolCount
        {
            get { return _pooledObjects.Count; }
        }

        #endregion Public Properties

        #region C'tor and Initialization code

        /// <summary>
        ///   Initializes a new pool with default settings.
        /// </summary>
        public ObjectPool()
            : this(ObjectPoolConstants.DefaultPoolMinimumSize, ObjectPoolConstants.DefaultPoolMaximumSize, null)
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
            : this(minimumPoolSize, maximumPoolSize, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method.
        /// </summary>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ObjectPool(Func<T> factoryMethod)
            : this(ObjectPoolConstants.DefaultPoolMinimumSize, ObjectPoolConstants.DefaultPoolMaximumSize, factoryMethod)
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
        {
            // Validating pool limits, exception is thrown if invalid
            ObjectPoolConstants.ValidatePoolLimits(minimumPoolSize, maximumPoolSize);

            // Assigning properties
            FactoryMethod = factoryMethod;
            _maximumPoolSize = maximumPoolSize;
            _minimumPoolSize = minimumPoolSize;

            // Creating a new instance for the Diagnostics class
            Diagnostics = new ObjectPoolDiagnostics();

            // Setting the action for returning to the pool to be integrated in the pooled objects
            _returnToPoolAction = ReturnObjectToPool;

            // Initilizing objects in pool
            AdjustPoolSizeToBounds();
        }

        #endregion C'tor and Initialization code

        #region Private Methods

        internal void AdjustPoolSizeToBounds()
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
        /// <returns>A monitored object from the pool.</returns>
        public T GetObject()
        {
            T dequeuedObject;

            if (_pooledObjects.TryDequeue(out dequeuedObject))
            {
                // Invokes AdjustPoolSize asynchronously.
                Task.Factory.StartNew(AdjustPoolSizeToBounds);

                // Diagnostics update.
                Diagnostics.IncrementPoolObjectHitCount();

                return dequeuedObject;
            }

            // This should not happen normally, but could be happening when there is stress on the
            // pool. No available objects in pool, create a new one and return it to the caller.
            Diagnostics.IncrementPoolObjectMissCount();
            return CreatePooledObject();
        }

        internal void ReturnObjectToPool(PooledObject objectToReturnToPool, bool reRegisterForFinalization)
        {
            var returnedObject = objectToReturnToPool as T;

            // Diagnostics update.
            if (reRegisterForFinalization)
            {
                Diagnostics.IncrementObjectResurrectionCount();
            }

            // Checking that the pool is not full.
            if (ObjectsInPoolCount < MaximumPoolSize)
            {
                // Reset the object state (if implemented) before returning it to the pool. If
                // reseting the object have failed, destroy the object.
                if (returnedObject != null && !returnedObject.ResetState())
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
    }
}
