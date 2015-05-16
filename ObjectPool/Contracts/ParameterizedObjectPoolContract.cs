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
using CodeProject.ObjectPool.Core;

namespace CodeProject.ObjectPool.Contracts
{
    /// <summary>
    ///   Contract class for <see cref="IParameterizedObjectPool{TKey,TValue}"/>.
    /// </summary>
    [ContractClassFor(typeof(IParameterizedObjectPool<,>))]
    public abstract class ParameterizedObjectPoolContract<TKey, TValue> : IParameterizedObjectPool<TKey, TValue>
    {
        /// <summary>
        ///   Gets or sets the Diagnostics class for the current Object Pool, whose goal is to
        ///   record data about how the pool operates. By default, however, an object pool records
        ///   anything, in order to be most efficient; in any case, you can enable it through the
        ///   <see cref="ObjectPoolDiagnostics.Enabled"/> property.
        /// </summary>
        public ObjectPoolDiagnostics Diagnostics
        {
            get
            {
                Contract.Ensures(Contract.Result<ObjectPoolDiagnostics>() != null);
                return default(ObjectPoolDiagnostics);
            }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null, ErrorMessages.NullDiagnostics);
            }
        }

        /// <summary>
        ///   Gets the Factory method that will be used for creating new objects.
        /// </summary>
        public abstract Func<TKey, TValue> FactoryMethod { get; }

        /// <summary>
        ///   Gets or sets the maximum number of objects that could be available at the same time in
        ///   the pool.
        /// </summary>
        public int MaximumPoolSize
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 1 && Contract.Result<int>() >= MinimumPoolSize);
                return default(int);
            }
            set
            {
                ObjectPoolConstants.ValidatePoolLimits(MinimumPoolSize, value);
            }
        }

        /// <summary>
        ///   Gets or sets the minimum number of objects in the pool.
        /// </summary>
        public int MinimumPoolSize
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return default(int);
            }
            set
            {
                ObjectPoolConstants.ValidatePoolLimits(value, MaximumPoolSize);
            }
        }

        /// <summary>
        ///   Gets the count of the keys currently handled by the pool.
        /// </summary>
        public int KeysInPoolCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return default(int);
            }
        }

        /// <summary>
        ///   Gets an object linked to given key.
        /// </summary>
        /// <param name="key">The key linked to the object.</param>
        /// <returns>The objects linked to given key.</returns>
        public TValue GetObject(TKey key)
        {
            Contract.Ensures(Contract.Result<TValue>() != null);
            return default(TValue);
        }
    }
}
