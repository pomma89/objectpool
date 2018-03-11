// File name: ObjectPoolAdapterTests.cs
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

#if HAS_EXTENSIONS

using CodeProject.ObjectPool.MicrosoftExtensionsAdapter;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;

namespace CodeProject.ObjectPool.UnitTests.MicrosoftExtensionsAdapter
{
    [TestFixture]
    internal sealed class ObjectPoolAdapterTests
    {
        [Test]
        public void ObjectPoolAdapter_GetAndReturnObject_SameInstance()
        {
            // Arrange
            var pool = ObjectPoolAdapter.Create(new ObjectPool<PooledObjectWrapper<List<int>>>(() => PooledObjectWrapper.Create(new List<int>())));

            var list1 = pool.Get();
            pool.Return(list1);

            // Act
            var list2 = pool.Get();

            // Assert
            list1.ShouldBe(list2);
        }

        [Test]
        public void ObjectPoolAdapterForPooledObject_GetAndReturnObject_SameInstance()
        {
            // Arrange
            var pool = ObjectPoolAdapter.CreateForPooledObject(new ObjectPool<MyPooledObject>());

            var pooledObject1 = pool.Get();
            pool.Return(pooledObject1);

            // Act
            var pooledObject2 = pool.Get();

            // Assert
            pooledObject1.ShouldBe(pooledObject2);
        }
    }
}

#endif