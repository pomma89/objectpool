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
using System.Diagnostics;
using Finsa.CodeServices.Common.Collections.Concurrent;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   A parameterized version of the ObjectPool class.
    /// </summary>
    /// <typeparam name="TKey">The type of the pool parameter.</typeparam>
    /// <typeparam name="TValue">The type of the objects stored in the pool.</typeparam>
    public sealed class ParameterizedObjectPool<TKey, TValue> : IParameterizedObjectPool<TKey, TValue> where TValue : PooledObject
    {
        private readonly ConcurrentDictionary<TKey, ObjectPool<TValue>> _pools = new ConcurrentDictionary<TKey, ObjectPool<TValue>>();

        private int _minimumPoolSize;
        private int _maximumPoolSize;
        private ObjectPoolDiagnostics _diagnostics;

        #region Public Properties

        /// <summary>
        /// Gets or sets the Diagnostics class for the current Object Pool, whose goal is to
        /// record data about how the pool operates. By default, however, an object pool records
        /// anything, in order to be most efficient; in any case, you can enable it through the
        /// <see cref="ObjectPoolDiagnostics.Enabled" /> property.
        /// </summary>
        public ObjectPoolDiagnostics Diagnostics
        {
            get { return _diagnostics; }
            set
            {
                _diagnostics = value;
                foreach (var p in _pools)
                {
                    p.Value.Diagnostics = _diagnostics;
                }
            }
        }

        /// <summary>
        ///   Gets or sets the maximum number of objects that could be available at the same time in
        ///   the pool.
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public int MaximumPoolSize
        {
            get
            {
                return _maximumPoolSize;
            }
            set
            {
                _maximumPoolSize = value;
            }
        }

        /// <summary>
        ///   Gets or sets the minimum number of objects in the pool.
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public int MinimumPoolSize
        {
            get
            {
                return _minimumPoolSize;
            }
            set
            {
                _minimumPoolSize = value;
            }
        }

        /// <summary>
        ///   Gets the Factory method that will be used for creating new objects.
        /// </summary>
        public Func<TKey, TValue> FactoryMethod { get; private set; }

        /// <summary>
        ///   Gets the count of the keys currently handled by the pool.
        /// </summary>
        public int KeysInPoolCount
        {
            get { return _pools.Count; }
        }

        #endregion Public Properties

        #region C'tor and Initialization code

        /// <summary>
        ///   Initializes a new pool with default settings.
        /// </summary>
        public ParameterizedObjectPool()
            : this(ObjectPoolConstants.DefaultPoolMinimumSize, ObjectPoolConstants.DefaultPoolMaximumSize, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified minimum pool size and maximum pool size.
        /// </summary>
        /// <param name="minimumPoolSize">The minimum pool size limit.</param>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        public ParameterizedObjectPool(int minimumPoolSize, int maximumPoolSize)
            : this(minimumPoolSize, maximumPoolSize, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method.
        /// </summary>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ParameterizedObjectPool(Func<TKey, TValue> factoryMethod)
            : this(ObjectPoolConstants.DefaultPoolMinimumSize, ObjectPoolConstants.DefaultPoolMaximumSize, factoryMethod)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified factory method and minimum and maximum size.
        /// </summary>
        /// <param name="minimumPoolSize">The minimum pool size limit.</param>
        /// <param name="maximumPoolSize">The maximum pool size limit</param>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        public ParameterizedObjectPool(int minimumPoolSize, int maximumPoolSize, Func<TKey, TValue> factoryMethod)
        {
            // Validating pool limits, exception is thrown if invalid
            ObjectPoolConstants.ValidatePoolLimits(minimumPoolSize, maximumPoolSize);

            // Assigning properties
            FactoryMethod = factoryMethod;
            _maximumPoolSize = maximumPoolSize;
            _minimumPoolSize = minimumPoolSize;
        }

        #endregion C'tor and Initialization code

        /// <summary>
        ///   Gets an object linked to given key.
        /// </summary>
        /// <param name="key">The key linked to the object.</param>
        /// <returns>The objects linked to given key.</returns>
        public TValue GetObject(TKey key)
        {
            ObjectPool<TValue> pool;

            if (!_pools.TryGetValue(key, out pool))
            {
                // Initialize the new pool.
                pool = new ObjectPool<TValue>(MinimumPoolSize, MaximumPoolSize, PrepareFactoryMethod(key));
                ObjectPool<TValue> foundPool;
                if (!_pools.TryAdd(key, pool, out foundPool))
                {
                    // Someone added the pool in the meantime!
                    pool = foundPool;
                }
                else
                {
                    // The new pool has been added, now we have to configure it.
                    pool.Diagnostics = _diagnostics;
                }
            }

            Debug.Assert(pool != null);
            return pool.GetObject();
        }

        private Func<TValue> PrepareFactoryMethod(TKey key)
        {
            var factory = FactoryMethod;
            if (factory == null)
            {
                // Use the default parameterless constructor.
                return null;
            }
            return () => factory(key);
        }
    }
}
