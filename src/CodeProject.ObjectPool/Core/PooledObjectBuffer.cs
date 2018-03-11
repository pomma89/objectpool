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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public sealed class PooledObjectBuffer<T> : IEnumerable<T>
        where T : PooledObject
    {
#if NET40
        private const MethodImplOptions TryInline = default(MethodImplOptions);
#else
        private const MethodImplOptions TryInline = MethodImplOptions.AggressiveInlining;
#endif

        /// <summary>
        ///   Used as default value for <see cref="_pooledObjects"/>.
        /// </summary>
        private static readonly T[] NoObjects = new T[0];

        /// <summary>
        ///   The concurrent buffer containing pooled objects.
        /// </summary>
        private T[] _pooledObjects = NoObjects;

        /// <summary>
        ///   The maximum capacity of this buffer.
        /// </summary>
        public int Capacity => _pooledObjects.Length;

        /// <summary>
        ///   The number of objects stored in this buffer.
        /// </summary>
        public int Count
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _pooledObjects.Length; ++i)
                {
                    if (_pooledObjects[i] != null) count++;
                }
                return count;
            }
        }

        /// <summary>
        ///   Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _pooledObjects)
            {
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        ///   Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///   An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///   Tries to dequeue an object from the buffer.
        /// </summary>
        /// <param name="pooledObject">Output pooled object.</param>
        /// <returns>True if <paramref name="pooledObject"/> has a value, false otherwise.</returns>
        [MethodImpl(TryInline)]
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

        /// <summary>
        ///   Tries to enqueue given object into the buffer.
        /// </summary>
        /// <param name="pooledObject">Input pooled object.</param>
        /// <returns>True if there was enough space to enqueue given object, false otherwise.</returns>
        [MethodImpl(TryInline)]
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

        /// <summary>
        ///   Resizes the buffer so that it fits to given capacity. If new capacity is smaller than
        ///   current capacity, then exceeding items are returned.
        /// </summary>
        /// <param name="newCapacity">The new capacity of this buffer.</param>
        /// <returns>All exceeding items.</returns>
        public IList<T> Resize(int newCapacity)
        {
            if (_pooledObjects == NoObjects)
            {
                _pooledObjects = new T[newCapacity];
                return NoObjects;
            }

            var currentCapacity = _pooledObjects.Length;
            if (currentCapacity == newCapacity)
            {
                // Nothing to do.
                return NoObjects;
            }

            IList<T> exceedingItems = NoObjects;
            if (currentCapacity > newCapacity)
            {
                for (var i = newCapacity; i < currentCapacity; ++i)
                {
                    ref var item = ref _pooledObjects[i];
                    if (item != null)
                    {
                        if (exceedingItems == NoObjects)
                        {
                            exceedingItems = new List<T> { item };
                        }
                        else
                        {
                            exceedingItems.Add(item);
                        }
                        item = null;
                    }
                }
            }

            Array.Resize(ref _pooledObjects, newCapacity);
            return exceedingItems;
        }

        /// <summary>
        ///   Tries to remove given object from the buffer.
        /// </summary>
        /// <param name="pooledObject">Pooled object to be removed.</param>
        /// <returns>True if <paramref name="pooledObject"/> has been removed, false otherwise.</returns>
        [MethodImpl(TryInline)]
        public bool TryRemove(T pooledObject)
        {
            for (var i = 0; i < _pooledObjects.Length; i++)
            {
                var item = _pooledObjects[i];
                if (item != null && item == pooledObject && Interlocked.CompareExchange(ref _pooledObjects[i], null, item) == item)
                {
                    return true;
                }
            }
            return false;
        }
    }
}