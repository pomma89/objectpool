/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

using BenchmarkDotNet.Running;
using PommaLabs.Thrower;

namespace CodeProject.ObjectPool.Benchmarks
{
    public static class Program
    {   
        public static void Main()
        {
            var p = new RetrieveOneObject();
            for (var i = 0; i < 1000000; ++i)
            {
                var x = p.SimpleObjectPool();
                Raise.ArgumentException.IfIsNullOrWhiteSpace(x);
            }

            //BenchmarkRunner.Run<RetrieveOneObject>();
        }
    }
}