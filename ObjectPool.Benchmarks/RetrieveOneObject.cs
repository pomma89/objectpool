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

namespace CodeProject.ObjectPool.Benchmarks
{
    [Config(typeof(Config))]
    public class RetrieveOneObject
    {
        private readonly ObjectPool<PooledObjectWrapper<string>> _objectPool = new ObjectPool<PooledObjectWrapper<string>>(9, 21, () => new PooledObjectWrapper<string>(DateTime.UtcNow.ToString()));
        private readonly ParameterizedObjectPool<int, PooledObjectWrapper<string>> _paramObjectPool = new ParameterizedObjectPool<int, PooledObjectWrapper<string>>(9, 21, x => new PooledObjectWrapper<string>(DateTime.UtcNow + "#" + x));

        private class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.Default);
                Add(GetColumns().ToArray());
                Add(CsvExporter.Default, HtmlExporter.Default, MarkdownExporter.GitHub, PlainExporter.Default);
                Add(new MemoryDiagnoser());
                Add(EnvironmentAnalyser.Default);
            }
        }

        [Benchmark]
        public string SimpleObjectPool()
        {
            string str;
            using (var x = _objectPool.GetObject())
            {
                str = x.InternalResource;
            }
            return str;
        }

        [Benchmark]
        public string ParameterizedObjectPool()
        {
            string str;
            using (var x = _paramObjectPool.GetObject(21))
            {
                str = x.InternalResource;
            }
            return str;
        }
    }
}