/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

namespace CodeProject.ObjectPool.Core
{
    /// <summary>
    ///   Exposes a way to return objects to the pool.
    /// </summary>
    internal interface IObjectPoolHandle
    {
        void ReturnObjectToPool(PooledObject objectToReturnToPool, bool reRegisterForFinalization);
    }
}