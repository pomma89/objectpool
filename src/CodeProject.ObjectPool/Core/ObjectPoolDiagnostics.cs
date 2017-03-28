/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

using System.Threading;

namespace CodeProject.ObjectPool.Core
{
    /// <summary>
    ///   A simple class to track stats during execution. By default, this class does not record
    ///   anything, since its <see cref="Enabled"/> property is set to <c>false</c>.
    /// </summary>
    public class ObjectPoolDiagnostics
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

        private long _returnedToPoolByResurrectionCount;
        private long _returnedToPoolCount;
        private long _totalInstancesCreated;
        private long _totalInstancesDestroyed;

        /// <summary>
        ///   Gets or sets whether this object can record data about how the Pool operates.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///   Gets the total count of live instances, both in the pool and in use.
        /// </summary>
        public long TotalLiveInstancesCount => _totalInstancesCreated - _totalInstancesDestroyed;

        /// <summary>
        ///   Gets the count of object reset failures occured while the pool tried to re-add the
        ///   object into the pool.
        /// </summary>
        public long ObjectResetFailedCount => _objectResetFailedCount;

        /// <summary>
        ///   Gets the total count of object that has been picked up by the GC, and returned to pool.
        /// </summary>
        public long ReturnedToPoolByResurrectionCount => _returnedToPoolByResurrectionCount;

        /// <summary>
        ///   Gets the total count of successful accesses. The pool had a spare object to provide to
        ///   the user without creating it on demand.
        /// </summary>
        public long PoolObjectHitCount => _poolObjectHitCount;

        /// <summary>
        ///   Gets the total count of unsuccessful accesses. The pool had to create an object in
        ///   order to satisfy the user request.
        /// </summary>
        public long PoolObjectMissCount => _poolObjectMissCount;

        /// <summary>
        ///   Gets the total number of pooled objected created.
        /// </summary>
        public long TotalInstancesCreated => _totalInstancesCreated;

        /// <summary>
        ///   Gets the total number of objects destroyes, both in case of an pool overflow, and state corruption.
        /// </summary>
        public long TotalInstancesDestroyed => _totalInstancesDestroyed;

        /// <summary>
        ///   Gets the number of objects been destroyed because the pool was full at the time of
        ///   returning the object to the pool.
        /// </summary>
        public long PoolOverflowCount => _poolOverflowCount;

        /// <summary>
        ///   Gets the total count of objects that been successfully returned to the pool.
        /// </summary>
        public long ReturnedToPoolCount => _returnedToPoolCount;

        #endregion Public Properties and backing fields

        #region Protected Methods for incrementing the counters

        /// <summary>
        ///   Increments the objects created count.
        /// </summary>
        protected internal virtual void IncrementObjectsCreatedCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref _totalInstancesCreated);
            }
        }

        /// <summary>
        ///   Increments the objects destroyed count.
        /// </summary>
        protected internal virtual void IncrementObjectsDestroyedCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref _totalInstancesDestroyed);
            }
        }

        /// <summary>
        ///   Increments the pool object hit count.
        /// </summary>
        protected internal virtual void IncrementPoolObjectHitCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref _poolObjectHitCount);
            }
        }

        /// <summary>
        ///   Increments the pool object miss count.
        /// </summary>
        protected internal virtual void IncrementPoolObjectMissCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref _poolObjectMissCount);
            }
        }

        /// <summary>
        ///   Increments the pool overflow count.
        /// </summary>
        protected internal virtual void IncrementPoolOverflowCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref _poolOverflowCount);
            }
        }

        /// <summary>
        ///   Increments the reset state failed count.
        /// </summary>
        protected internal virtual void IncrementResetStateFailedCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref _objectResetFailedCount);
            }
        }

        /// <summary>
        ///   Increments the count of objects returned to pool by resurrection.
        /// </summary>
        protected internal virtual void IncrementObjectResurrectionCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref _returnedToPoolByResurrectionCount);
            }
        }

        /// <summary>
        ///   Increments the returned to pool count.
        /// </summary>
        protected internal virtual void IncrementReturnedToPoolCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref _returnedToPoolCount);
            }
        }

        #endregion Protected Methods for incrementing the counters
    }
}