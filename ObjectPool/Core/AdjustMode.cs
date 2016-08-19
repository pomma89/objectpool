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

namespace CodeProject.ObjectPool.Core
{
    /// <summary>
    ///   Determines which bounds should be checked when adjusting pool size.
    /// </summary>
    [Flags]
    internal enum AdjustMode
    {
        Minimum = 1,
        Maximum = 2
    }
}