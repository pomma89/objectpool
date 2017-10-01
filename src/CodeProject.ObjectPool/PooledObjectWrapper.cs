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
using System;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Helper class for building pooled object wrappers.
    /// </summary>
    public static class PooledObjectWrapper
    {
        /// <summary>
        ///   Wraps a given resource so that it can be put in the pool.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="resource">The resource to be wrapped.</param>
        /// <exception cref="ArgumentNullException">Given resource is null.</exception>
        /// <returns>A wrapper for given resource.</returns>
        public static PooledObjectWrapper<T> Create<T>(T resource) where T : class => new PooledObjectWrapper<T>(resource);
    }

    /// <summary>
    ///   PooledObject wrapper, for classes which cannot inherit from that class.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
#if HAS_SERIALIZABLE
    [Serializable]
#endif

    public sealed class PooledObjectWrapper<T> : PooledObject where T : class
    {
        /// <summary>
        ///   Wraps a given resource so that it can be put in the pool.
        /// </summary>
        /// <param name="resource">The resource to be wrapped.</param>
        /// <exception cref="ArgumentNullException">Given resource is null.</exception>
        public PooledObjectWrapper(T resource)
        {
            InternalResource = resource ?? throw new ArgumentNullException(nameof(resource), ErrorMessages.NullResource);

            base.OnReleaseResources += () => OnReleaseResources?.Invoke(InternalResource);
            base.OnResetState += () => OnResetState?.Invoke(InternalResource);
        }

        /// <summary>
        ///   The resource wrapped inside this class.
        /// </summary>
        public T InternalResource { get; }

        /// <summary>
        ///   Triggered by the pool manager when there is no need for this object anymore.
        /// </summary>
        public new Action<T> OnReleaseResources { get; set; }

        /// <summary>
        ///   Triggered by the pool manager just before the object is being returned to the pool.
        /// </summary>
        public new Action<T> OnResetState { get; set; }
    }
}