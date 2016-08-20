/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

using CodeProject.ObjectPool.Core;
using PommaLabs.Thrower;
using System;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Constants for Object Pools.
    /// </summary>
    public static class ObjectPoolConstants
    {
        #region Constants

        /// <summary>
        ///   The default minimum size for the pool. It is set to 4.
        /// </summary>
        public const int DefaultPoolMinimumSize = 4;

        /// <summary>
        ///   The default maximum size for the pool. It is set to 32.
        /// </summary>
        public const int DefaultPoolMaximumSize = 32;

        #endregion Constants

        #region Validation

        /// <summary>
        ///   Checks the lower and upper bounds for the pool size.
        /// </summary>
        /// <param name="minimumPoolSize">The lower bound.</param>
        /// <param name="maximumPoolSize">The upper bound.</param>
        public static void ValidatePoolLimits(int minimumPoolSize, int maximumPoolSize)
        {
            Raise<ArgumentOutOfRangeException>.If(minimumPoolSize < 0, ErrorMessages.NegativeMinimumPoolSize);
            Raise<ArgumentOutOfRangeException>.If(maximumPoolSize < 1, ErrorMessages.NegativeOrZeroMaximumPoolSize);
            Raise<ArgumentOutOfRangeException>.If(minimumPoolSize > maximumPoolSize, ErrorMessages.WrongCacheBounds);
        }

        #endregion Validation

    }
}