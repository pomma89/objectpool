// File name: ObjectPoolTests.cs
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

using System;
using System.Collections.Generic;
using CodeProject.ObjectPool.Core;
using NUnit.Framework;
using Shouldly;

#if !NET40

using System.Threading.Tasks;

#endif

namespace CodeProject.ObjectPool.UnitTests
{
    [TestFixture]
    internal sealed class ObjectPoolTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        public void ShouldThrowOnMaximumSizeEqualToZeroOrNegative(int maxSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObjectPool<MyPooledObject>(maxSize));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-5)]
        [TestCase(-10)]
        public void ShouldThrowOnMaximumSizeEqualToZeroOrNegativeOnProperty(int maxSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObjectPool<MyPooledObject> { MaximumPoolSize = maxSize });
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void ShouldFillUntilMaximumSize(int maxSize)
        {
            var pool = new ObjectPool<MyPooledObject>(maxSize);
            var objects = new List<MyPooledObject>();
            for (var i = 0; i < maxSize * 2; ++i)
            {
                var obj = pool.GetObject();
                objects.Add(obj);
            }
            foreach (var obj in objects)
            {
                obj.Dispose();
            }
            Assert.AreEqual(maxSize, pool.ObjectsInPoolCount);
        }

#if !NET40

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public async Task ShouldFillUntilMaximumSize_Async(int maxSize)
        {
            var pool = new ObjectPool<MyPooledObject>(maxSize);
            var objectCount = maxSize * 4;
            var objects = new MyPooledObject[objectCount];
            Parallel.For(0, objectCount, i =>
            {
                objects[i] = pool.GetObject();
            });
            Parallel.For(0, objectCount, i =>
            {
                objects[i].Dispose();
            });

            await Task.Delay(1000);

            Assert.AreEqual(maxSize, pool.ObjectsInPoolCount);
        }

#endif

        [Test]
        public void ShouldNotResetPooledObjectIdsWhenCleared()
        {
            var pool = new ObjectPool<MyPooledObject>();

            var poA = pool.GetObject();
            var poB = pool.GetObject();

            poA.PooledObjectInfo.Id.ShouldBe(1);
            poB.PooledObjectInfo.Id.ShouldBe(2);

            poA.Dispose();
            poB.Dispose();

            pool.ObjectsInPoolCount.ShouldBe(2);
            pool.Clear();
            pool.ObjectsInPoolCount.ShouldBe(0);

            var poC = pool.GetObject();

            poC.PooledObjectInfo.Id.ShouldBe(3);
        }

        [Test]
        public void ShouldChangePoolLimitsIfCorrect()
        {
            var pool = new ObjectPool<MyPooledObject>();
            Assert.AreEqual(ObjectPool.DefaultPoolMaximumSize, pool.MaximumPoolSize);

            pool.MaximumPoolSize = pool.MaximumPoolSize * 2;
            Assert.AreEqual(ObjectPool.DefaultPoolMaximumSize * 2, pool.MaximumPoolSize);

            pool.MaximumPoolSize = 2;
            pool.MaximumPoolSize.ShouldBe(2);
        }

        [Test]
        public void ShouldHandleClearAfterNoUsage()
        {
            var pool = new ObjectPool<MyPooledObject>();

            pool.Clear();

            pool.ObjectsInPoolCount.ShouldBe(0);
        }

        [Test]
        public void ShouldHandleClearAfterSomeUsage()
        {
            var pool = new ObjectPool<MyPooledObject>();

            using (var obj = pool.GetObject())
            {
            }

            pool.Clear();

            pool.ObjectsInPoolCount.ShouldBe(0);
        }

        [Test]
        public void ShouldHandleClearAndThenPoolCanBeUsedAgain()
        {
            var pool = new ObjectPool<MyPooledObject>();

            using (var obj = pool.GetObject())
            {
            }

            pool.Clear();

            using (var obj = pool.GetObject())
            {
            }

            pool.ObjectsInPoolCount.ShouldBe(1);
        }

        [Test]
        public void ShouldHandleClearAndThenReachCorrectSizeAtLaterUsage()
        {
            var pool = new ObjectPool<MyPooledObject>();

            using (var obj = pool.GetObject())
            {
            }

            pool.Clear();

            // Usage #A
            using (var obj = pool.GetObject())
            {
            }

            // Usages #B
            using (var obj = pool.GetObject())
            {
            }
            using (var obj = pool.GetObject())
            {
            }
            using (var obj = pool.GetObject())
            {
            }

            // Despite usage #B, count should always be fixed.
            pool.ObjectsInPoolCount.ShouldBe(1);
        }

        [Test]
        public void ShouldEnforceLowerBoundOnPoolConstruction()
        {
            var pool = new ObjectPool<MyPooledObject>();
            pool.ObjectsInPoolCount.ShouldBe(0);
        }

        #region Pooled object state

        [Test]
        public void PooledObjectStateShouldBecomeReservedWhenRetrievedFromThePool()
        {
            var pool = new ObjectPool<MyPooledObject>();

            using (var obj = pool.GetObject())
            {
                obj.PooledObjectInfo.State.ShouldBe(PooledObjectState.Reserved);
            }
        }

        [Test]
        public void PooledObjectStateShouldBecomeAvailableWhenReturnedToThePool()
        {
            var pool = new ObjectPool<MyPooledObject>();

            MyPooledObject obj;
            using (obj = pool.GetObject())
            {
            }

            obj.PooledObjectInfo.State.ShouldBe(PooledObjectState.Available);
        }

        [Test]
        public void PooledObjectStateShouldBecomeDisposedWhenCannotReturnToThePool()
        {
            var pool = new ObjectPool<MyPooledObject>(2);

            var obj = pool.GetObject();

            using (var tmp1 = pool.GetObject())
            using (var tmp2 = pool.GetObject())
            {
            }

            obj.Dispose();
            obj.PooledObjectInfo.State.ShouldBe(PooledObjectState.Disposed);
        }

        #endregion Pooled object state
    }
}