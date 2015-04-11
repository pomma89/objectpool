// File name: LinkedQueue.cs
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

namespace CodeProject.ObjectPool.Utilities.Collections
{
    /// <typeparam name="T">The type of the items the queue will contain.</typeparam>
    internal sealed class LinkedQueue<T> : ILinkedQueue<T>
    {
        #region Fields

        private Core.SinglyNode<T> _firstNode;
        private Core.SinglyNode<T> _lastNode;

        #endregion Fields

        #region Construction

        /// <summary>
        ///   Returns a queue implemented using an <see cref="ILinkedList{TItem}"/>.
        /// </summary>
        /// <returns>A queue implemented using an <see cref="ILinkedList{TItem}"/>.</returns>
        public LinkedQueue()
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == 0);
        }

        #endregion Construction

        #region IEnumerable Members

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            for (var n = _firstNode; n != null; n = n.Next)
            {
                yield return n.Item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Members

        #region ILinkedQueue Members

        public int Count { get; private set; }

        public T Dequeue()
        {
            var first = _firstNode.Item;
            _firstNode = _firstNode.Next;
            if (--Count == 0)
            {
                _lastNode = null;
            }
            return first;
        }

        public void Enqueue(T item)
        {
            var node = new Core.SinglyNode<T>(item, null);
            if (Count++ == 0)
            {
                _firstNode = node;
            }
            else
            {
                _lastNode.Next = node;
            }
            _lastNode = node;
        }

        public T Peek()
        {
            return _firstNode.Item;
        }

        #endregion ILinkedQueue Members
    }
}