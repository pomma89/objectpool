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
using System.Diagnostics.Contracts;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   A parameterized version of the ObjectPool class.
    /// </summary>
    /// <typeparam name="TKey">The type of the pool parameter.</typeparam>
    /// <typeparam name="TValue">The type of the objects stored in the pool.</typeparam>
    public sealed class ParameterizedObjectPool<TKey, TValue> : ObjectPool where TValue : PooledObject
    {
        private FSharpMap<TKey, ObjectPool<TValue>> _pools = MapModule.Empty<TKey, ObjectPool<TValue>>();

        private int _minimumPoolSize;
        private int _maximumPoolSize;

        #region Public Properties

        /// <summary>
        ///   Gets or sets the minimum number of objects in the pool.
        /// </summary>
        [Pure]
        public int MinimumPoolSize
        {
            get
            {
                return _minimumPoolSize;
            }
            set
            {
                // Validating pool limits, exception is thrown if invalid
                ValidatePoolLimits(value, _maximumPoolSize);
                _minimumPoolSize = value;
            }
        }

        /// <summary>
        ///   Gets or sets the maximum number of objects that could be available at the same time in
        ///   the pool.
        /// </summary>
        [Pure]
        public int MaximumPoolSize
        {
            get
            {
                return _maximumPoolSize;
            }
            set
            {
                // Validating pool limits, exception is thrown if invalid
                ValidatePoolLimits(_minimumPoolSize, value);
                _maximumPoolSize = value;
            }
        }

        /// <summary>
        ///   Gets the Factory method that will be used for creating new objects.
        /// </summary>
        [Pure]
        public Func<TKey, TValue> FactoryMethod { get; private set; }

        #endregion Public Properties

        #region C'tor and Initialization code

        /// <summary>
        ///   Initializes a new pool with default settings.
        /// </summary>
        public ParameterizedObjectPool()
            : this(DefaultPoolMinimumSize, DefaultPoolMaximumSize, null)
        {
        }

        /// <summary>
        ///   Initializes a new pool with specified minimum pool size and maximum pool size
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
            : this(DefaultPoolMinimumSize, DefaultPoolMaximumSize, factoryMethod)
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
            ValidatePoolLimits(minimumPoolSize, maximumPoolSize);

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
            var localPools = _pools;
            var pool = localPools.TryFind(key);
            if (pool == null)
            {
                // Initialize the new pool.
                var objPool = new ObjectPool<TValue>(MinimumPoolSize, MaximumPoolSize, PrepareFactoryMethod(key));
                pool = FSharpOption<ObjectPool<TValue>>.Some(objPool);
                _pools = MapModule.Add(key, objPool, localPools);
            }
            Debug.Assert(pool != null);
            return pool.Value.GetObject();
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