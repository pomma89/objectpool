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

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   A parameterized version of the ObjectPool interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the pool parameter.</typeparam>
    /// <typeparam name="TValue">The type of the objects stored in the pool.</typeparam>
    public interface IParameterizedObjectPool<in TKey, out TValue>
    {
        /// <summary>
        ///   Gets the Diagnostics class for the current Object Pool, whose goal is to record data
        ///   about how the pool operates. By default, however, an object pool records anything; you
        ///   have to enable it through the <see cref="ObjectPoolDiagnostics.Enabled"/> property.
        /// </summary>
        [Pure]
        ObjectPoolDiagnostics Diagnostics { get; }

        /// <summary>
        ///   Gets the Factory method that will be used for creating new objects.
        /// </summary>
        [Pure]
        Func<TKey, TValue> FactoryMethod { get; }

        /// <summary>
        ///   Gets or sets the maximum number of objects that could be available at the same time in
        ///   the pool.
        /// </summary>
        [Pure]
        int MaximumPoolSize { get; set; }

        /// <summary>
        ///   Gets or sets the minimum number of objects in the pool.
        /// </summary>
        [Pure]
        int MinimumPoolSize { get; set; }

        /// <summary>
        ///   Gets the count of the objects currently in the pool.
        /// </summary>
        [Pure]
        int ObjectsInPoolCount { get; }

        /// <summary>
        ///   Gets an object linked to given key.
        /// </summary>
        /// <param name="key">The key linked to the object.</param>
        /// <returns>The objects linked to given key.</returns>
        TValue GetObject(TKey key);
    }
}
