// File name: ConcurrentDoublyLinkedList.cs
// 
// Author(s): Alessio Parma <alessio.parma@gmail.com>
// 
// Copyright (c) 2013-2014 Alessio Parma <alessio.parma@gmail.com>
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
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CodeProject.ObjectPool.Collections.Core;
using CodeProject.ObjectPool.Threading;

namespace CodeProject.ObjectPool.Collections.Concurrent
{
    /// <typeparam name="T">The type of the items the list will contain.</typeparam>
    internal sealed class ConcurrentDoublyLinkedList<T> : IDoublyLinkedList<T>
    {
        #region Fields

        private readonly DoublyLinkedList<T> _list;

        private readonly ConcurrentWorkQueue _workQueue = new ConcurrentWorkQueue();

        #endregion Fields

        #region Construction

        /// <summary>
        ///   Returns the thread safe implementation of the <see cref="IDoublyLinkedList{TItem}"/> interface.
        /// </summary>
        /// <returns>A thread safe of the <see cref="IDoublyLinkedList{TItem}"/> interface.</returns>
        public ConcurrentDoublyLinkedList()
        {
            Contract.Ensures(Count == 0);
            Contract.Ensures(ReferenceEquals(EqualityComparer, EqualityComparer<T>.Default));
            _list = new DoublyLinkedList<T>(EqualityComparer<T>.Default);
        }

        /// <summary>
        ///   Returns the thread safe implementation of the <see cref="IDoublyLinkedList{TItem}"/>
        ///   interface, using specified equality comparer.
        /// </summary>
        /// <param name="equalityComparer">
        ///   The equality comparer that it will be used to determine whether two items are equal.
        /// </param>
        /// <returns>
        ///   A thread safe of the <see cref="IDoublyLinkedList{TItem}"/> interface using specified
        ///   equality comparer.
        /// </returns>
        public ConcurrentDoublyLinkedList(IEqualityComparer<T> equalityComparer)
        {
            Contract.Requires<ArgumentNullException>(equalityComparer != null);
            Contract.Ensures(Count == 0);
            Contract.Ensures(ReferenceEquals(EqualityComparer, equalityComparer));
            _list = new DoublyLinkedList<T>(equalityComparer);
        }

        #endregion Construction

        public int Count
        {
            get { return _workQueue.EnqueueReadFunc(() => _list.Count); }
        }

        public IEqualityComparer<T> EqualityComparer
        {
            get { return _list.EqualityComparer; }
        }

        public T First
        {
            get { return _workQueue.EnqueueReadFunc(() => _list.First); }
        }

        public bool IsReadOnly
        {
            get { return _list.IsReadOnly; }
        }

        public T Last
        {
            get { return _workQueue.EnqueueReadFunc(() => _list.Last); }
        }

        public void Add(T item)
        {
            _workQueue.EnqueueWriteAction(_list.Add, item);
        }

        public void AddFirst(T item)
        {
            _workQueue.EnqueueWriteAction(_list.AddFirst, item);
        }

        public void AddLast(T item)
        {
            _workQueue.EnqueueWriteAction(_list.AddLast, item);
        }

        public void Append(ILinkedList<T> list)
        {
            _workQueue.EnqueueWriteAction(_list.Append, list);
        }

        public void Append(IDoublyLinkedList<T> list)
        {
            _workQueue.EnqueueWriteAction(_list.Append, list);
        }

        public void Clear()
        {
            _workQueue.EnqueueWriteAction(_list.Clear);
        }

        public bool Contains(T item)
        {
            return _workQueue.EnqueueReadFunc(_list.Contains, item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _workQueue.EnqueueWriteAction(_list.CopyTo, array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var result = ListUtilities<T>.EmptyList;
            _workQueue.EnqueueReadFunc(() => result = ListUtilities<T>.ToList(_list.GetEnumerator()));
            return result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetReversedEnumerator()
        {
            var result = ListUtilities<T>.EmptyList;
            _workQueue.EnqueueReadFunc(() => result = ListUtilities<T>.ToList(_list.GetReversedEnumerator()));
            return result.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return _workQueue.EnqueueWriteFunc(_list.Remove, item);
        }

        public T RemoveFirst()
        {
            return _workQueue.EnqueueWriteFunc(_list.RemoveFirst);
        }

        public T RemoveLast()
        {
            return _workQueue.EnqueueWriteFunc(_list.RemoveLast);
        }

        public void Reverse()
        {
            _workQueue.EnqueueWriteAction(_list.Reverse);
        }
    }
}
