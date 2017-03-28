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

#if !NET35

using PommaLabs.Thrower.Logging;

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

#if !NET35
        private static readonly ILog Log = LogProvider.GetLogger(typeof(PooledObject));
#endif

        #endregion Logging

        #region Properties

        /// <summary>
        ///   Unique identifier.
        /// </summary>
        public Guid PooledObjectId { get; } = Guid.NewGuid();

        /// <summary>
        ///   Enumeration that is being managed by the pool to describe the object state - primary
        ///   used to void cases where the resources are being releases twice.
        /// </summary>
        /// <remarks>Default value for pooled object state is <see cref="Core.PooledObjectState.Available"/>.</remarks>
        public PooledObjectState PooledObjectState { get; internal set; } = PooledObjectState.Available;

        /// <summary>
        ///   Internal action that is initialized by the pool while creating the object, this allows
        ///   that object to re-add itself back to the pool.
        /// </summary>
        internal IObjectPoolHandle Handle { get; set; }

        #endregion Properties

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
#if !NET35
                if (Log.IsWarnEnabled())
                {
                    Log.WarnException("[ObjectPool] An unexpected error occurred while releasing resources", ex);
                }
#else
                System.Diagnostics.Debug.Assert(ex != null); // Placeholder to avoid warnings
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
#if !NET35
                if (Log.IsDebugEnabled())
                {
                    Log.DebugException("[ObjectPool] Object state could not be reset", crsex);
                }
#else
                System.Diagnostics.Debug.Assert(crsex != null); // Placeholder to avoid warnings
#endif
                successFlag = false;
            }
            catch (Exception ex)
            {
#if !NET35
                if (Log.IsWarnEnabled())
                {
                    Log.WarnException("[ObjectPool] An unexpected error occurred while resetting state", ex);
                }
#else
                System.Diagnostics.Debug.Assert(ex != null); // Placeholder to avoid warnings
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
            if (PooledObjectState == Core.PooledObjectState.Disposed || PooledObjectState == Core.PooledObjectState.Available)
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
#if !NET35
                if (Log.IsWarnEnabled())
                {
                    Log.WarnException("[ObjectPool] An error occurred while re-adding to pool", ex);
                }
#else
                System.Diagnostics.Debug.Assert(ex != null); // Placeholder to avoid warnings
#endif
                PooledObjectState = Core.PooledObjectState.Disposed;
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
}