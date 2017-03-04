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
using System.Diagnostics;

#if !(NETSTD10 || NETSTD13 || NET35)

using CodeProject.ObjectPool.Logging;

#endif

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   PooledObject base class.
    /// </summary>
    [Serializable]
    public abstract class PooledObject : IDisposable
    {
        #region Logging

#if !(NETSTD10 || NETSTD13 || NET35)
        private static readonly ILog Log = LogProvider.GetLogger(typeof(PooledObject));
#endif

        #endregion Logging

        #region Internal Properties

        /// <summary>
        ///   Internal Action that is initialized by the pool while creating the object, this allow
        ///   that object to re-add itself back to the pool.
        /// </summary>
        internal IObjectPoolHandle Handle { get; set; }

        /// <summary>
        ///   Enumeration that is being managed by the pool to describe the object state - primary
        ///   used to void cases where the resources are being releases twice.
        /// </summary>
        /// <remarks>Default value for pooled object state is <see cref="PooledObjectState.Available"/>.</remarks>
        public PooledObjectState State { get; internal set; } = PooledObjectState.Available;

        #endregion Internal Properties

        #region Internal Methods - resource and state management

        /// <summary>
        ///   Releases the object resources. This method will be called by the pool manager when
        ///   there is no need for this object anymore (decreasing pooled objects count, pool is
        ///   being destroyed).
        /// </summary>
        internal bool ReleaseResources()
        {
            var successFlag = true;

            try
            {
                OnReleaseResources();
            }
            catch (Exception ex)
            {
#if !(NETSTD10 || NETSTD13 || NET35)
                if (Log.IsWarnEnabled())
                {
                    Log.WarnException("[ObjectPool] An unexpected error occurred while releasing resources", ex);
                }
#else
                Debug.Assert(ex != null); // Placeholder to avoid warnings
#endif
                successFlag = false;
            }

            return successFlag;
        }

        /// <summary>
        ///   Reset the object state. This method will be called by the pool manager just before the
        ///   object is being returned to the pool.
        /// </summary>
        internal bool ResetState()
        {
            var successFlag = true;

            try
            {
                OnResetState();
            }
            catch (CannotResetStateException crsex)
            {
#if !(NETSTD10 || NETSTD13 || NET35)
                if (Log.IsDebugEnabled())
                {
                    Log.DebugException("[ObjectPool] Object state could not be reset", crsex);
                }
#else
                Debug.Assert(crsex != null); // Placeholder to avoid warnings
#endif
                successFlag = false;
            }
            catch (Exception ex)
            {
#if !(NETSTD10 || NETSTD13 || NET35)
                if (Log.IsWarnEnabled())
                {
                    Log.WarnException("[ObjectPool] An unexpected error occurred while resetting state", ex);
                }
#else
                Debug.Assert(ex != null); // Placeholder to avoid warnings
#endif
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
        ///   Releases the object's resources.
        /// </summary>
        protected virtual void OnReleaseResources()
        {
        }

        #endregion Virtual Template Methods - extending resource and state management

        #region Returning object to pool - Dispose and Finalizer

#pragma warning disable CC0029 // Disposables Should Call Suppress Finalize

        /// <summary>
        ///   See <see cref="IDisposable"/> docs.
        /// </summary>
        public void Dispose()
#pragma warning restore CC0029 // Disposables Should Call Suppress Finalize
        {
            // Returning to pool
            HandleReAddingToPool(false);
        }

        private void HandleReAddingToPool(bool reRegisterForFinalization)
        {
            // Only when the object is reserved it can be readded to the pool.
            if (State == PooledObjectState.Disposed || State == PooledObjectState.Available)
            {
                return;
            }
            // If there is any case that the re-adding to the pool failes, release the resources and
            // set the internal Disposed flag to true
            try
            {
                // Notifying the pool that this object is ready for re-adding to the pool.
                Handle.ReturnObjectToPool(this, reRegisterForFinalization);
            }
            catch (Exception ex)
            {
#if !(NETSTD10 || NETSTD13 || NET35)
                if (Log.IsWarnEnabled())
                {
                    Log.WarnException("[ObjectPool] An error occurred while re-adding to pool", ex);
                }
#else
                Debug.Assert(ex != null); // Placeholder to avoid warnings
#endif
                State = PooledObjectState.Disposed;
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
        /// <summary>
        ///   Wraps a given resource so that it can be put in the pool.
        /// </summary>
        /// <param name="resource">The resource to be wrapped.</param>
        /// <exception cref="ArgumentNullException">Given resource is null.</exception>
        public PooledObjectWrapper(T resource)
        {
            Raise.ArgumentNullException.IfIsNull(resource, nameof(resource), ErrorMessages.NullResource);
            // Setting the internal resource
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