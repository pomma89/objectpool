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
    ///   PooledObject wrapper, for classes which cannot inherit from that class.
    /// </summary>
    [Serializable]
    public sealed class PooledObjectWrapper<T> : PooledObject where T : class
    {
        /// <summary>
        ///   Wraps a given resource so that it can be put in the pool.
        /// </summary>
        /// <param name="resource">The resource to be wrapped.</param>
        /// <exception cref="ArgumentNullException">Given resource is null.</exception>
        public PooledObjectWrapper(T resource)
        {
            // Preconditions
            Raise.ArgumentNullException.IfIsNull(resource, nameof(resource), ErrorMessages.NullResource);

            InternalResource = resource;
        }

        /// <summary>
        ///   Triggered by the pool manager when there is no need for this object anymore.
        /// </summary>
        public Action<T> WrapperReleaseResourcesAction { get; set; }

        /// <summary>
        ///   Triggered by the pool manager just before the object is being returned to the pool.
        /// </summary>
        public Action<T> WrapperResetStateAction { get; set; }

        /// <summary>
        ///   The resource wrapped inside this class.
        /// </summary>
        public T InternalResource { get; }

        /// <summary>
        ///   Triggers the <see cref="WrapperReleaseResourcesAction"/>, if any.
        /// </summary>
        protected override void OnReleaseResources()
        {
            WrapperReleaseResourcesAction?.Invoke(InternalResource);
        }

        /// <summary>
        ///   Triggers the <see cref="WrapperResetStateAction"/>, if any.
        /// </summary>
        protected override void OnResetState()
        {
            WrapperResetStateAction?.Invoke(InternalResource);
        }
    }
}