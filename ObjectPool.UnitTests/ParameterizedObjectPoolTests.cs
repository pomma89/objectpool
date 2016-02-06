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
using System.Threading;
using System.Threading.Tasks;
using CodeProject.ObjectPool;
using CodeProject.ObjectPool.Core;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    internal sealed class ParameterizedObjectPoolTests
    {
        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = ErrorMessages.NegativeMinimumPoolSize, MatchType = MessageMatch.Contains)]
        public void ShouldThrowOnNegativeMinimumSize(int minSize)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ParameterizedObjectPool<int, MyPooledObject>(minSize, 1);
        }

        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = ErrorMessages.NegativeMinimumPoolSize, MatchType = MessageMatch.Contains)]
        public void ShouldThrowOnNegativeMinimumSizeOnProperty(int minSize)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ParameterizedObjectPool<int, MyPooledObject> { MinimumPoolSize = minSize };
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = ErrorMessages.NegativeOrZeroMaximumPoolSize, MatchType = MessageMatch.Contains)]
        public void ShouldThrowOnMaximumSizeEqualToZeroOrNegative(int maxSize)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ParameterizedObjectPool<int, MyPooledObject>(0, maxSize);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = ErrorMessages.NegativeOrZeroMaximumPoolSize, MatchType = MessageMatch.Contains)]
        public void ShouldThrowOnMaximumSizeEqualToZeroOrNegativeOnProperty(int maxSize)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ParameterizedObjectPool<int, MyPooledObject> { MaximumPoolSize = maxSize };
        }

#if !PORTABLE

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void ShouldSimplyWork(int maxSize)
        {
            const int keyCount = 4;
            var pool = new ParameterizedObjectPool<int, MyPooledObject>(0, maxSize);
            var objectCount = maxSize * keyCount;
            var objects = new MyPooledObject[objectCount];
            Parallel.For(0, objectCount, i =>
            {
                objects[i] = pool.GetObject(i % keyCount);
            });
            Parallel.For(0, objectCount, i =>
            {
                objects[i].Dispose();
            });
            Thread.Sleep(1000);
            Assert.AreEqual(keyCount, pool.KeysInPoolCount);
        }

#endif

        [Test]
        public void ShouldChangePoolLimitsIfCorrect()
        {
            var pool = new ParameterizedObjectPool<int, MyPooledObject>();
            Assert.AreEqual(ObjectPoolConstants.DefaultPoolMinimumSize, pool.MinimumPoolSize);
            Assert.AreEqual(ObjectPoolConstants.DefaultPoolMaximumSize, pool.MaximumPoolSize);

            pool.MinimumPoolSize = pool.MaximumPoolSize - 5;
            Assert.AreEqual(ObjectPoolConstants.DefaultPoolMaximumSize - 5, pool.MinimumPoolSize);
            Assert.AreEqual(ObjectPoolConstants.DefaultPoolMaximumSize, pool.MaximumPoolSize);

            pool.MaximumPoolSize = pool.MaximumPoolSize * 2;
            Assert.AreEqual(ObjectPoolConstants.DefaultPoolMaximumSize - 5, pool.MinimumPoolSize);
            Assert.AreEqual(ObjectPoolConstants.DefaultPoolMaximumSize * 2, pool.MaximumPoolSize);

            pool.MinimumPoolSize = 1;
            Assert.AreEqual(1, pool.MinimumPoolSize);
            Assert.AreEqual(ObjectPoolConstants.DefaultPoolMaximumSize * 2, pool.MaximumPoolSize);

            pool.MaximumPoolSize = 2;
            Assert.AreEqual(1, pool.MinimumPoolSize);
            Assert.AreEqual(2, pool.MaximumPoolSize);
        }

        [Test]
        public void ShouldHandleClearAfterNoUsage()
        {
            var pool = new ParameterizedObjectPool<int, MyPooledObject>();

            pool.Clear();

            Assert.That(0, Is.EqualTo(pool.KeysInPoolCount));
        }

        [Test]
        public void ShouldHandleClearAfterSomeUsage()
        {
            var pool = new ParameterizedObjectPool<int, MyPooledObject>();

            using (var obj = pool.GetObject(1))
            {
            }

            pool.Clear();

            Assert.That(0, Is.EqualTo(pool.KeysInPoolCount));
        }

        [Test]
        public void ShouldHandleClearAndThenPoolCanBeUsedAgain()
        {
            var pool = new ParameterizedObjectPool<int, MyPooledObject>();

            using (var obj = pool.GetObject(1))
            {
            }

            pool.Clear();

            using (var obj = pool.GetObject(1))
            {
            }

            Assert.That(1, Is.EqualTo(pool.KeysInPoolCount));
        }
    }
}
