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
using System.Threading;

namespace Original
{
    /// <summary>
    ///   PooledObject base calss
    /// </summary>
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
        ///   Releases the object resources This method will be called by the pool manager when there
        ///   is no need for this object anymore (decreasing pooled objects count, pool is being destroyed)
        /// </summary>
        /// <returns></returns>
        internal bool ReleaseResources()
        {
            bool successFlag = true;

            try
            {
                OnReleaseResources();
            }
            catch (Exception)
            {
                successFlag = false;
            }

            return successFlag;
        }

        /// <summary>
        ///   Reset the object state This method will be called by the pool manager just before the
        ///   object is being returned to the pool
        /// </summary>
        /// <returns></returns>
        internal bool ResetState()
        {
            bool successFlag = true;

            try
            {
                OnResetState();
            }
            catch (Exception)
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

        private void HandleReAddingToPool(bool reRegisterForFinalization)
        {
            if (!Disposed)
            {
                // If there is any case that the re-adding to the pool failes, release the resources
                // and set the internal Disposed flag to true
                try
                {
                    // Notifying the pool that this object is ready for re-adding to the pool.
                    ReturnToPool(this, reRegisterForFinalization);
                }
                catch (Exception)
                {
                    Disposed = true;
                    this.ReleaseResources();
                }
            }
        }

        ~PooledObject()
        {
            // Resurrecting the object
            HandleReAddingToPool(true);
        }

        public void Dispose()
        {
            // Returning to pool
            ThreadPool.QueueUserWorkItem(new WaitCallback((o) => HandleReAddingToPool(false)));
        }

        #endregion Returning object to pool - Dispose and Finalizer
    }

    public class PooledObjectWrapper<T> : PooledObject
    {
        public Action<T> WrapperReleaseResourcesAction { get; set; }
        public Action<T> WrapperResetStateAction { get; set; }

        public T InternalResource { get; private set; }

        public PooledObjectWrapper(T resource)
        {
            if (resource == null)
            {
                throw new ArgumentException("resource cannot be null");
            }

            // Setting the internal resource
            InternalResource = resource;
        }

        protected override void OnReleaseResources()
        {
            if (WrapperReleaseResourcesAction != null)
            {
                WrapperReleaseResourcesAction(InternalResource);
            }
        }

        protected override void OnResetState()
        {
            if (WrapperResetStateAction != null)
            {
                WrapperResetStateAction(InternalResource);
            }
        }
    }
}