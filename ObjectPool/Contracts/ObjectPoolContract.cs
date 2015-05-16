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

namespace CodeProject.ObjectPool.Contracts
{
    /// <summary>
    ///   Contract class for <see cref="IObjectPool{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ContractClassFor(typeof(IObjectPool<>))]
    public abstract class ObjectPoolContract<T> : IObjectPool<T> where T : PooledObject
    {
        /// <summary>
        /// Gets the Diagnostics class for the current Object Pool, whose goal is to record data
        /// about how the pool operates. By default, however, an object pool records anything; you
        /// have to enable it through the <see cref="ObjectPoolDiagnostics.Enabled" /> property.
        /// </summary>
        public ObjectPoolDiagnostics Diagnostics
        {
            get
            {
                Contract.Ensures(Contract.Result<ObjectPoolDiagnostics>() != null);
                return default(ObjectPoolDiagnostics);
            }
        }

        /// <summary>
        ///   Gets the Factory method that will be used for creating new objects.
        /// </summary>
        public Func<T> FactoryMethod
        {
            get
            {
                Contract.Ensures(Contract.Result<Func<T>>() != null);
                return default(Func<T>);
            }
        }

        public int MaximumPoolSize { get; set; }

        public int MinimumPoolSize { get; set; }

        /// <summary>
        ///   Gets the count of the objects currently in the pool.
        /// </summary>
        public int ObjectsInPoolCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return default(int);
            }
        }

        /// <summary>
        ///   Gets a monitored object from the pool.
        /// </summary>
        /// <returns>A monitored object from the pool.</returns>
        public T GetObject()
        {
            Contract.Ensures(Contract.Result<T>() != null);
            return default(T);
        }
    }
}
