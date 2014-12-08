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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Static class containing all error messages used by ObjectPool.
    /// </summary>
    public static class ErrorMessages
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
