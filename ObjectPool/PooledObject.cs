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
using System.Threading.Tasks;
using CodeProject.ObjectPool.Core;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   PooledObject base class.
    /// </summary>
    [Serializable]
    public abstract class PooledObject : IDisposable
    {
        #region Internal Properties

        /// <summary>
        ///   Internal Action that is initialized by the pool while creating the object, this allow
        ///   that object to re-add itself back to the pool.
        /// </summary>
        internal Action<PooledObject, bool> ReturnToPool { get; set; }

        /// <summary>
        ///   Internal flag that is being managed by the pool to describe the object state - primary
        ///   used to void cases where the resources are being releases twice.
        /// </summary>
        internal bool Disposed { get; set; }

        #endregion Internal Properties

        #region Internal Methods - resource and state management

        /// <summary>
        ///   Releases the object resources. This method will be called by the pool manager when
        ///   there is no need for this object anymore (decreasing pooled objects count, pool is
        ///   being destroyed).
        /// </summary>
        /// <returns></returns>
        internal bool ReleaseResources()
        {
            var successFlag = true;

            try
            {
                OnReleaseResources();
            }
            catch
            {
                successFlag = false;
            }

            return successFlag;
        }

        /// <summary>
        ///   Reset the object state. This method will be called by the pool manager just before the
        ///   object is being returned to the pool.
        /// </summary>
        /// <returns></returns>
        internal bool ResetState()
        {
            var successFlag = true;

            try
            {
                OnResetState();
            }
            catch
            {
                successFlag = false;
            }

            return successFlag;
        }

        #endregion Internal Methods - resource and state management

        #region Virtual Template Methods - extending resource and state management

        /// <summary>
        ///   Reset the object state to allow this object to be re-used by other parts of the application.
        /// </summary>
        protected virtual void OnResetState()
        {
        }

        /// <summary>
        ///   Releases the object's resources
        /// </summary>
        protected virtual void OnReleaseResources()
        {
        }

        #endregion Virtual Template Methods - extending resource and state management

        #region Returning object to pool - Dispose and Finalizer

        /// <summary>
        ///   See <see cref="IDisposable"/> docs.
        /// </summary>
        public void Dispose()
        {
            // Returning to pool
            Task.Factory.StartNew(() => HandleReAddingToPool(false));
        }

        private void HandleReAddingToPool(bool reRegisterForFinalization)
        {
            if (Disposed)
            {
                return;
            }
            // If there is any case that the re-adding to the pool failes, release the resources and
            // set the internal Disposed flag to true
            try
            {
                // Notifying the pool that this object is ready for re-adding to the pool.
                ReturnToPool(this, reRegisterForFinalization);
            }
            catch
            {
                Disposed = true;
                ReleaseResources();
            }
        }

        /// <summary>
        ///   PooledObject destructor.
        /// </summary>
        ~PooledObject()
        {
            // Resurrecting the object
            HandleReAddingToPool(true);
        }

        #endregion Returning object to pool - Dispose and Finalizer
    }

    /// <summary>
    ///   PooledObject wrapper, for classes which cannot inherit from that class.
    /// </summary>
    [Serializable]
    public sealed class PooledObjectWrapper<T> : PooledObject where T : class
    {
        private readonly T _internalResource;

        /// <summary>
        ///   Wraps a given resource so that it can be put in the pool.
        /// </summary>
        /// <param name="resource">The resource to be wrapped.</param>
        public PooledObjectWrapper(T resource)
        {
            Contract.Requires<ArgumentNullException>(resource != null, ErrorMessages.NullResource);
            // Setting the internal resource
            _internalResource = resource;
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
        public T InternalResource
        {
            get { return _internalResource; }
        }

        /// <summary>
        ///   Triggers <see cref="WrapperReleaseResourcesAction"/>, if any.
        /// </summary>
        protected override void OnReleaseResources()
        {
            var safeAction = WrapperReleaseResourcesAction;
            if (safeAction != null)
            {
                safeAction(InternalResource);
            }
        }

        /// <summary>
        ///   Triggers <see cref="WrapperResetStateAction"/>, if any.
        /// </summary>
        protected override void OnResetState()
        {
            var safeAction = WrapperResetStateAction;
            if (safeAction != null)
            {
                safeAction(InternalResource);
            }
        }
    }
}