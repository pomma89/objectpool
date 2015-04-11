/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

namespace CodeProject.ObjectPool.Core
{
    /// <summary>
    ///   Static class containing all error messages used by ObjectPool.
    /// </summary>
    internal static partial class ErrorMessages
    {
        /// <summary>
        ///   An error message.
        /// </summary>
        public const string NegativeMinimumPoolSize = "Minimum pool size must be greater or equals to zero.";

        /// <summary>
        ///   An error message.
        /// </summary>
        public const string NegativeOrZeroMaximumPoolSize = "Maximum pool size must be greater than zero.";

        /// <summary>
        ///   An error message.
        /// </summary>
        public const string NullResource = "Resource cannot be null.";

        /// <summary>
        ///   An error message.
        /// </summary>
        public const string WrongCacheBounds = "Maximum pool size must be greater than the maximum pool size.";
    }
}