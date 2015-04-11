// File name: ConcurrentSinglyLinkedList.cs
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

namespace CodeProject.ObjectPool.Utilities.Collections.Concurrent
{
    /// <typeparam name="T">The type of the items the list will contain.</typeparam>
    internal sealed class ConcurrentSinglyLinkedList<T> : ILinkedList<T>
    {
        #region Fields

        private readonly SinglyLinkedList<T> _list;

        private readonly Threading.ConcurrentWorkQueue _workQueue = Threading.ConcurrentWorkQueue.Create();

        #endregion Fields

        #region Construction

        /// <summary>
        ///   Returns the thread safe implementation of the <see cref="ILinkedList{TItem}"/> interface.
        /// </summary>
        /// <returns>A thread safe of the <see cref="ILinkedList{TItem}"/> interface.</returns>
        public ConcurrentSinglyLinkedList()
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == 0);
            System.Diagnostics.Contracts.Contract.Ensures(ReferenceEquals(EqualityComparer, System.Collections.Generic.EqualityComparer<T>.Default));
            _list = new SinglyLinkedList<T>(System.Collections.Generic.EqualityComparer<T>.Default);
        }

        /// <summary>
        ///   Returns the thread safe implementation of the <see cref="ILinkedList{TItem}"/>
        ///   interface, using specified equality comparer.
        /// </summary>
        /// <param name="equalityComparer">
        ///   The equality comparer that it will be used to determine whether two items are equal.
        /// </param>
        /// <returns>
        ///   A thread safe of the <see cref="ILinkedList{TItem}"/> interface using specified
        ///   equality comparer.
        /// </returns>
        public ConcurrentSinglyLinkedList(System.Collections.Generic.IEqualityComparer<T> equalityComparer)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentNullException>(equalityComparer != null);
            System.Diagnostics.Contracts.Contract.Ensures(Count == 0);
            System.Diagnostics.Contracts.Contract.Ensures(ReferenceEquals(EqualityComparer, equalityComparer));
            _list = new SinglyLinkedList<T>(equalityComparer);
        }

        #endregion Construction

        public int Count
        {
            get { return _workQueue.EnqueueReadFunc(() => _list.Count); }
        }

        public System.Collections.Generic.IEqualityComparer<T> EqualityComparer
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

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            var result = Core.ListUtilities<T>.EmptyList;
            _workQueue.EnqueueReadFunc(() => result = Core.ListUtilities<T>.ToList(_list.GetEnumerator()));
            return result.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(T item)
        {
            return _workQueue.EnqueueWriteFunc(_list.Remove, item);
        }

        public T RemoveFirst()
        {
            return _workQueue.EnqueueWriteFunc(_list.RemoveFirst);
        }
    }
}