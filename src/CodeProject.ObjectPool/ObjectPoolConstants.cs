/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Constants for Object Pools.
    /// </summary>
    public static class ObjectPoolConstants
    {
        #region Constants

        /// <summary>
        ///   The default minimum size for the pool. It is set to 1.
        /// </summary>
        public const int DefaultPoolMinimumSize = 1;

        /// <summary>
        ///   The default maximum size for the pool. It is set to 10.
        /// </summary>
        public const int DefaultPoolMaximumSize = 10;

        #endregion Constants
    }
}