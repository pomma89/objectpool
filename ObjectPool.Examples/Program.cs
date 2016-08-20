// File name: Program.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2013-2016 Alessio Parma <alessio.parma@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace CodeProject.ObjectPool.Examples
{
    /// <summary>
    ///   Example usages of ObjectPool.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///   Example usages of ObjectPool.
        /// </summary>
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
            // External Resource reset state code.
        }

        public static void ExternalResourceReleaseResource(ExternalExpensiveResource resource)
        {
            // External Resource release code.
        }
    }

    internal sealed class ExpensiveResource : PooledObject
    {
        public void DoStuff()
        {
            // Do some work here, for example.
        }

        protected override void OnReleaseResources()
        {
            // Override if the resource needs to be manually cleaned before the memory is reclaimed.
        }

        protected override void OnResetState()
        {
            // Override if the resource needs resetting before it is getting back into the pool.
        }
    }

    internal sealed class ExternalExpensiveResource
    {
        public void DoOtherStuff()
        {
            // Do some work here, for example.
        }
    }
}
