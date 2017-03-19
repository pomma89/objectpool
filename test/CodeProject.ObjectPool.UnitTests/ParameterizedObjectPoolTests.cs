// File name: ParameterizedObjectPoolTests.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2013-2018 Alessio Parma <alessio.parma@gmail.com>
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

using CodeProject.ObjectPool;
using NUnit.Framework;
using System;

#if !(NET35 || NET40)

using System.Threading.Tasks;

#endif

namespace CodeProject.ObjectPool.UnitTests
{
    [TestFixture]
    internal sealed class ParameterizedObjectPoolTests
    {
        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        public void ShouldThrowOnNegativeMinimumSize(int minSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterizedObjectPool<int, MyPooledObject>(minSize, 1));
        }

        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        public void ShouldThrowOnNegativeMinimumSizeOnProperty(int minSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterizedObjectPool<int, MyPooledObject> { MinimumPoolSize = minSize });
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        public void ShouldThrowOnMaximumSizeEqualToZeroOrNegative(int maxSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterizedObjectPool<int, MyPooledObject>(0, maxSize));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        public void ShouldThrowOnMaximumSizeEqualToZeroOrNegativeOnProperty(int maxSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterizedObjectPool<int, MyPooledObject> { MaximumPoolSize = maxSize });
        }

#if !(NET35 || NET40)

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public async Task ShouldSimplyWork(int maxSize)
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

            await Task.Delay(1000);

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