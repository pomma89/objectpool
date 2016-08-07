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
using System;

namespace CodeProject.ObjectPool.Benchmarks
{
    public class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<RetrieveOneObject>();
            Console.Read();
        }
    }
}