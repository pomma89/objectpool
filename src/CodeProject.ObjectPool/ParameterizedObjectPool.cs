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
using System.Linq;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   A parameterized version of the ObjectPool class.
    /// </summary>
    /// <typeparam name="TKey">The type of the pool parameter.</typeparam>
    /// <typeparam name="TValue">The type of the objects stored in the pool.</typeparam>
    public sealed class ParameterizedObjectPool<TKey, TValue> : IParameterizedObjectPool<TKey, TValue>
        where TValue : PooledObject
    {
        #region Fields

        /// <summary>
        ///   Backing field for <see cref="Diagnostics"/>.
        /// </summary>
        private ObjectPoolDiagnostics _diagnostics;

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
        ///   Gets or sets the Diagnostics class for the current Object Pool, whose goal is to record
        ///   data about how the pool operates. By default, however, an object pool records anything,
        ///   in order to be most efficient; in any case, you can enable it through the
        ///   <see cref="ObjectPoolDiagnostics.Enabled"/> property.
        /// </summary>
        public ObjectPoolDiagnostics Diagnostics
        {
            get { return _diagnostics; }
            set
            {
                // Safe copy of the current pools.
                var innerPools = _pools.Values.Cast<ObjectPool<TValue>>().ToArray();

                _diagnostics = value;
                foreach (var p in innerPools)
                {
                    p.Diagnostics = value;
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
                ObjectPoolConstants.ValidatePoolLimits(MinimumPoolSize, value);
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
                ObjectPoolConstants.ValidatePoolLimits(value, MaximumPoolSize);
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
        public int KeysInPoolCount => _pools.Count;

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
            Diagnostics = new ObjectPoolDiagnostics();
            FactoryMethod = factoryMethod;
            _maximumPoolSize = maximumPoolSize;
            _minimumPoolSize = minimumPoolSize;
        }

        #endregion C'tor and Initialization code

        #region Pool Operations

        /// <summary>
        ///   Clears the parameterized pool and each inner pool stored inside it.
        /// </summary>
        public void Clear()
        {
            ClearPools();
        }

        /// <summary>
        ///   Gets an object linked to given key.
        /// </summary>
        /// <param name="key">The key linked to the object.</param>
        /// <returns>The objects linked to given key.</returns>
        public TValue GetObject(TKey key)
        {
            ObjectPool<TValue> pool;
            if (!TryGetPool(key, out pool))
            {
                // Initialize and insert the new pool.
                pool = AddPool(key);
            }

            Debug.Assert(pool != null);
            return pool.GetObject();
        }

        #endregion Pool Operations

        #region Low-level Pooling

#if (PORTABLE || NETSTD10)
        private readonly System.Collections.Generic.Dictionary<TKey, ObjectPool<TValue>> _pools = new System.Collections.Generic.Dictionary<TKey, ObjectPool<TValue>>();
#else
        private readonly System.Collections.Hashtable _pools = new System.Collections.Hashtable();
#endif

        private void ClearPools()
        {
            // Safe copy of the current pools.
            var innerPools = _pools.Values.Cast<ObjectPool<TValue>>().ToArray();

            // Clear the main pool.
            lock (_pools)
            {
                _pools.Clear();
            }

            // Then clear each pool, taking it from the safe copy.
            foreach (var innerPool in innerPools)
            {
                innerPool.Clear();
            }
        }

        private bool TryGetPool(TKey key, out ObjectPool<TValue> objectPool)
        {
#if (PORTABLE || NETSTD10)
            // Dictionary requires locking even for readers.
            lock (_pools)
            {
                return _pools.TryGetValue(key, out objectPool);
            }
#else
            // Hashtable requires no locking for readers.
            objectPool = _pools[key] as ObjectPool<TValue>;
            return objectPool != null;
#endif
        }

        private ObjectPool<TValue> AddPool(TKey key)
        {
            // We are going to write, so we need full locking.
            lock (_pools)
            {
                ObjectPool<TValue> objectPool;
                if (!_pools.ContainsKey(key))
                {
                    _pools.Add(key, objectPool = new ObjectPool<TValue>(MinimumPoolSize, MaximumPoolSize, PrepareFactoryMethod(key))
                    {
                        Diagnostics = _diagnostics
                    });
                }
                else
                {
                    // Someone added the same pool in the meanwhile.
                    objectPool = _pools[key] as ObjectPool<TValue>;
                }
                return objectPool;
            }
        }

        #endregion Low-level Pooling

        #region Private Methods

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

        #endregion Private Methods
    }
}