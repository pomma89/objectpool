/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CodeProject.ObjectPool.Benchmarks
{
    [Config(typeof(Config))]
    public class RetrieveObjectsConcurrently
    {
        private readonly ObjectPool<MyResource> _objectPool = new ObjectPool<MyResource>(9, 21, () => new MyResource { Value = DateTime.UtcNow.ToString() });
        private readonly ParameterizedObjectPool<int, MyResource> _paramObjectPool = new ParameterizedObjectPool<int, MyResource>(9, 21, x => new MyResource { Value = (DateTime.UtcNow + "#" + x) });
        private readonly Microsoft.Extensions.ObjectPool.ObjectPool<MyResource> _microsoftObjectPool = new Microsoft.Extensions.ObjectPool.DefaultObjectPoolProvider().Create<MyResource>(new MyResource.Policy());

        private class Config : ManualConfig
        {
            public Config()
            {                
                Add(Job.Default.WithAffinity(new Count(2)));
                Add(GetColumns().ToArray());
                Add(CsvExporter.Default, HtmlExporter.Default, MarkdownExporter.GitHub, PlainExporter.Default);
                Add(new MemoryDiagnoser());
                Add(EnvironmentAnalyser.Default);
            }
        }

        private sealed class MyResource : PooledObject
        {
            public string Value { get; set; }

            public sealed class Policy : Microsoft.Extensions.ObjectPool.IPooledObjectPolicy<MyResource>
            {
#pragma warning disable CC0022 // Should dispose object
                public MyResource Create() => new MyResource { Value = DateTime.UtcNow.ToString() };
#pragma warning restore CC0022 // Should dispose object

                public bool Return(MyResource obj) => true;
            }
        }

        [Params(10, 100, 1000)]
        public int Count { get; set; }

        [Benchmark]
        public ParallelLoopResult SimpleObjectPool() => Parallel.For(0, Count, _ =>
        {
            string str;
            using (var x = _objectPool.GetObject())
            {
                str = x.Value;
            }
        });

        [Benchmark]
        public ParallelLoopResult ParameterizedObjectPool() => Parallel.For(0, Count, _ =>
        {
            string str;
            using (var x = _paramObjectPool.GetObject(21))
            {
                str = x.Value;
            }
        });

        [Benchmark]
        public ParallelLoopResult MicrosoftObjectPool() => Parallel.For(0, Count, _ =>
        {
            MyResource res = null;
            string str;
            try
            {
                res = _microsoftObjectPool.Get();
                str = res.Value;
            }
            finally
            {
                if (res != null)
                {
                    _microsoftObjectPool.Return(res);
                }
            }
        });
    }
}