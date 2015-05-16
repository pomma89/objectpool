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

namespace CodeProject.ObjectPool
{
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
            get { return _returnedToPoolCount; }
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
                Interlocked.Increment(ref _returnedToPoolCount);
            }
        }

        #endregion Internal Methods for incrementing the counters
    }
}