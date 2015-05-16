using System;
using System.Diagnostics.Contracts;
using CodeProject.ObjectPool.Core;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Constants for Object Pools.
    /// </summary>
    public static class ObjectPoolConstants
    {
        #region Constants

        /// <summary>
        ///   The default minimum size for the pool.
        /// </summary>
        public const int DefaultPoolMinimumSize = 5;

        /// <summary>
        ///   The default maximum size for the pool.
        /// </summary>
        public const int DefaultPoolMaximumSize = 100;

        #endregion Constants

        #region Validation

        /// <summary>
        ///   Checks the lower and upper bounds for the pool size.
        /// </summary>
        /// <param name="minimumPoolSize">The lower bound.</param>
        /// <param name="maximumPoolSize">The upper bound.</param>
        [ContractAbbreviator]
        public static void ValidatePoolLimits(int minimumPoolSize, int maximumPoolSize)
        {
            Contract.Requires<ArgumentOutOfRangeException>(minimumPoolSize >= 0, ErrorMessages.NegativeMinimumPoolSize);
            Contract.Requires<ArgumentOutOfRangeException>(maximumPoolSize >= 1, ErrorMessages.NegativeOrZeroMaximumPoolSize);
            Contract.Requires<ArgumentOutOfRangeException>(minimumPoolSize <= maximumPoolSize, ErrorMessages.WrongCacheBounds);
        }

        #endregion
    }

#if !PORTABLE

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [global::System.Diagnostics.Conditional("CONTRACTS_FULL")]
    internal sealed class ContractAbbreviatorAttribute : global::System.Attribute
    {
    }

#endif
}