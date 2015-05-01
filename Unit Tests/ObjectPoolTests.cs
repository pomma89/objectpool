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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeProject.ObjectPool;
using CodeProject.ObjectPool.Core;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    internal sealed class ObjectPoolTests
    {
        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = ErrorMessages.NegativeMinimumPoolSize, MatchType = MessageMatch.Contains)]
        public void ShouldThrowOnNegativeMinimumSize(int minSize)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ObjectPool<MyPooledObject>(minSize, 1);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = ErrorMessages.NegativeOrZeroMaximumPoolSize, MatchType = MessageMatch.Contains)]
        public void ShouldThrowOnMaximumSizeEqualToZeroOrNegative(int maxSize)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ObjectPool<MyPooledObject>(0, maxSize);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void ShouldSatisfyMinimumSizeRequirement(int minSize)
        {
            var pool = new ObjectPool<MyPooledObject>(minSize, minSize * 2 + 1);
            Assert.AreEqual(minSize, pool.ObjectsInPoolCount);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void ShouldFillUntilMaximumSize(int maxSize)
        {
            var pool = new ObjectPool<MyPooledObject>(0, maxSize);
            var objects = new List<MyPooledObject>();
            for (var i = 0; i < maxSize * 2; ++i)
            {
                var obj = pool.GetObject();
                objects.Add(obj);
            }
            foreach (var obj in objects)
            {
                pool.ReturnObjectToPool(obj, false);
            }
            Assert.AreEqual(maxSize, pool.ObjectsInPoolCount);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void ShouldFillUntilMaximumSize_Async(int maxSize)
        {
            var pool = new ObjectPool<MyPooledObject>(0, maxSize);
            var objectCount = maxSize*4;
            var objects = new MyPooledObject[objectCount];
            Parallel.For(0, objectCount, i =>
            {
                objects[i] = pool.GetObject();
            });
            Parallel.For(0, objectCount, i =>
            {
                objects[i].Dispose();
            });
            Thread.Sleep(1000);
            pool.AdjustPoolSizeToBounds();
            Assert.AreEqual(maxSize, pool.ObjectsInPoolCount);
        }

        private sealed class MyPooledObject : PooledObject
        {
        }
    }
}
