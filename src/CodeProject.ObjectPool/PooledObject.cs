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
using PommaLabs.Thrower.Goodies;
using System;
using System.Collections.Generic;

#if !NET35

using PommaLabs.Thrower.Logging;

#endif

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   PooledObject base class.
    /// </summary>
    [Serializable]
    public abstract class PooledObject : EquatableObject<PooledObject>, IDisposable
    {
        #region Logging

#if !NET35
        private static readonly ILog Log = LogProvider.GetLogger(typeof(PooledObject));
#endif

        #endregion Logging

        #region Properties

        /// <summary>
        ///   Core information about this <see cref="PooledObject"/>.
        /// </summary>
        public PooledObjectInfo PooledObjectInfo { get; } = new PooledObjectInfo();

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

        /// <summary>
        ///   See <see cref="IDisposable"/> docs.
        /// </summary>
        public void Dispose()
        {
            // Returning to pool
            HandleReAddingToPool(false);
        }

        private void HandleReAddingToPool(bool reRegisterForFinalization)
        {
            // Only when the object is reserved it can be readded to the pool.
            if (PooledObjectInfo.State == PooledObjectState.Disposed || PooledObjectInfo.State == PooledObjectState.Available)
            {
                return;
            }
            // If there is any case that the re-adding to the pool failes, release the resources and
            // set the internal Disposed flag to true
            try
            {
                // Notifying the pool that this object is ready for re-adding to the pool.
                PooledObjectInfo.Handle.ReturnObjectToPool(this, reRegisterForFinalization);
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
                PooledObjectInfo.State = PooledObjectState.Disposed;
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

        #region Formatting and equality

        /// <summary>
        ///   Returns all property (or field) values, along with their names, so that they can be
        ///   used to produce a meaningful <see cref="object.ToString"/>.
        /// </summary>
        /// <returns>
        ///   All property (or field) values, along with their names, so that they can be used to
        ///   produce a meaningful <see cref="object.ToString"/>.
        /// </returns>
        protected override IEnumerable<KeyValuePair<string, object>> GetFormattingMembers()
        {
            yield return new KeyValuePair<string, object>(nameof(PooledObjectInfo.Id), PooledObjectInfo.Id);
            if (PooledObjectInfo.Payload != null)
            {
                yield return new KeyValuePair<string, object>(nameof(PooledObjectInfo.Payload), PooledObjectInfo.Payload);
            }
        }

        /// <summary>
        ///   Returns all property (or field) values that should be used inside
        ///   <see cref="IEquatable{T}.Equals(T)"/> or <see cref="object.GetHashCode"/>.
        /// </summary>
        /// <returns>
        ///   All property (or field) values that should be used inside
        ///   <see cref="IEquatable{T}.Equals(T)"/> or <see cref="object.GetHashCode"/>.
        /// </returns>
        protected override IEnumerable<object> GetIdentifyingMembers()
        {
            yield return PooledObjectInfo.Id;
        }

        #endregion Formatting and equality
    }
}