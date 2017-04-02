// File name: PooledObjectBuffer.cs
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
using System.Threading;

namespace CodeProject.ObjectPool.Core
{
    /// <summary>
    ///   A buffer into which pooled objects are stored. This buffer mostly behaves like a queue,
    ///   even if that behaviour is not stricly respected in order to maximize performance.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of the object that which will be managed by the pool. The pooled object have to be
    ///   a sub-class of PooledObject.
    /// </typeparam>
    public sealed class PooledObjectBuffer<T>
        where T : PooledObject
    {
        /// <summary>
        ///   Used as default value for <see cref="_pooledObjects"/>.
        /// </summary>
        private static readonly T[] NoObjects = new T[0];

        /// <summary>
        ///   The concurrent buffer containing pooled objects.
        /// </summary>
        private T[] _pooledObjects = NoObjects;

        public int Capacity => _pooledObjects.Length;

        public int Count
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _pooledObjects.Length; ++i)
                {
                    if (_pooledObjects[i] != null)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        ///   All objects currently stored inside the pool.
        /// </summary>
        protected IEnumerable<T> PooledObjects
        {
            get
            {
                for (var i = 0; i <= _pooledObjects.Length; ++i)
                {
                    var item = _pooledObjects[i];
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }
        }

        public bool TryDequeue(out T pooledObject)
        {
            for (var i = 0; i < _pooledObjects.Length; i++)
            {
                var item = _pooledObjects[i];
                if (item != null && Interlocked.CompareExchange(ref _pooledObjects[i], null, item) == item)
                {
                    pooledObject = item;
                    return true;
                }
            }
            pooledObject = null;
            return false;
        }

        public bool TryEnqueue(T pooledObject)
        {
            for (var i = 0; i < _pooledObjects.Length; i++)
            {
                ref var item = ref _pooledObjects[i];
                if (item == null && Interlocked.CompareExchange(ref item, pooledObject, null) == null)
                {
                    return true;
                }
            }
            return false;
        }

        public void Resize(int newCapacity)
        {
            if (_pooledObjects == NoObjects)
            {
                _pooledObjects = new T[newCapacity];
                return;
            }

            var currentCapacity = _pooledObjects.Length;
            if (currentCapacity == newCapacity)
            {
                // Nothing to do.
                return;
            }

            if (currentCapacity > newCapacity)
            {
                for (var i = newCapacity; i < currentCapacity; ++i)
                {
                    ref var item = ref _pooledObjects[i];
                    if (item != null)
                    {
                        item.Dispose();
                        item = null;
                    }
                }
            }

            Array.Resize(ref _pooledObjects, newCapacity);
        }
    }
}