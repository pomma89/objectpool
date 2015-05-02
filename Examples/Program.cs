/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

using CodeProject.ObjectPool;

namespace ObjectPoolTester
{
    internal static class Program
    {
        private static void Main()
        {
            // Creating a pool with minimum size of 5 and maximum size of 25, using custom Factory
            // method to create and instance of ExpensiveResource.
            var pool = new ObjectPool<ExpensiveResource>(5, 25, () => new ExpensiveResource(/* resource specific initialization */));

            using (var resource = pool.GetObject())
            {
                // Using the resource...
                resource.DoStuff();
            } // Exiting the using scope will return the object back to the pool.

            // Creating a pool with wrapper object for managing external resources, that is, classes
            // which cannot inherit from PooledObject.
            var newPool = new ObjectPool<PooledObjectWrapper<ExternalExpensiveResource>>(() =>
                new PooledObjectWrapper<ExternalExpensiveResource>(CreateNewResource())
                {
                    WrapperReleaseResourcesAction = r => ExternalResourceReleaseResource(r),
                    WrapperResetStateAction = r => ExternalResourceResetState(r)
                });

            using (var wrapper = newPool.GetObject())
            {
                // wrapper.InternalResource contains the object that you pooled.
                wrapper.InternalResource.DoOtherStuff();
            } // Exiting the using scope will return the object back to the pool.
        }

        private static ExternalExpensiveResource CreateNewResource()
        {
            return new ExternalExpensiveResource();
        }

        public static void ExternalResourceResetState(ExternalExpensiveResource resource)
        {
            // External Resource reset state code
        }

        public static void ExternalResourceReleaseResource(ExternalExpensiveResource resource)
        {
            // External Resource release code
        }
    }

    public class ExpensiveResource : PooledObject
    {
        public void DoStuff()
        {
            // Do some work here, for example
        }

        protected override void OnReleaseResources()
        {
            // Override if the resource needs to be manually cleaned before the memory is reclaimed
        }

        protected override void OnResetState()
        {
            // Override if the resource needs resetting before it is getting back into the pool
        }
    }

    public class ExternalExpensiveResource
    {
        public void DoOtherStuff()
        {
            // Do some work here, for example
        }
    }
}
