// File name: DoublyLinkedList.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using CodeProject.ObjectPool.Collections.Core;

namespace CodeProject.ObjectPool.Collections
{
    /// <typeparam name="T">The type of the items the list will contain.</typeparam>
    internal sealed class DoublyLinkedList<T> : ListBase<DoublyNode<T>, T>, IDoublyLinkedList<T>
    {
        #region Construction

        /// <summary>
        ///   Returns the default implementation of the <see cref="IDoublyLinkedList{TItem}"/> interface.
        /// </summary>
        /// <returns>An implementation of the <see cref="IDoublyLinkedList{TItem}"/> interface.</returns>
        public DoublyLinkedList()
            : base(EqualityComparer<T>.Default)
        {
            Contract.Ensures(Count == 0);
            Contract.Ensures(ReferenceEquals(EqualityComparer, EqualityComparer<T>.Default));
        }

        /// <summary>
        ///   Returns the default implementation of the <see cref="IDoublyLinkedList{TItem}"/>
        ///   interface, using specified equality comparer.
        /// </summary>
        /// <param name="equalityComparer">
        ///   The equality comparer that it will be used to determine whether two items are equal.
        /// </param>
        /// <returns>
        ///   An implementation of the <see cref="IDoublyLinkedList{TItem}"/> interface using
        ///   specified equality comparer.
        /// </returns>
        public DoublyLinkedList(IEqualityComparer<T> equalityComparer)
            : base(equalityComparer)
        {
            Contract.Requires<ArgumentNullException>(equalityComparer != null);
            Contract.Ensures(Count == 0);
            Contract.Ensures(ReferenceEquals(EqualityComparer, equalityComparer));
        }

        #endregion Construction

        #region ICollection Members

        public void Add(T item)
        {
            AddLast(item);
        }

        #endregion ICollection Members

        #region IDoublyLinkedList Members

        public void AddFirst(T item)
        {
            var node = new DoublyNode<T>(item, FirstNode, null);
            if (Count > 0)
            {
                FirstNode.Prev = node;
            }
            else
            {
                LastNode = node;
            }
            FirstNode = node;
            Count++;
        }

        public void AddLast(T item)
        {
            var node = new DoublyNode<T>(item, null, LastNode);
            if (Count > 0)
            {
                LastNode.Next = node;
            }
            else
            {
                FirstNode = node;
            }
            LastNode = node;
            Count++;
        }

        public void Append(ILinkedList<T> list)
        {
            if (list.Count == 0)
            {
                return;
            }
            foreach (var i in list)
            {
                AddLast(i);
            }
            list.Clear();
        }

        public void Append(IDoublyLinkedList<T> list)
        {
            if (list.Count == 0)
            {
                return;
            }
            var rll = list as DoublyLinkedList<T>;
            if (rll == null)
            {
                foreach (var i in list)
                {
                    AddLast(i);
                }
                list.Clear();
                return;
            }
            if (LastNode != null)
            {
                LastNode.Next = rll.FirstNode;
                rll.FirstNode.Prev = LastNode;
            }
            else
            {
                FirstNode = rll.FirstNode;
            }
            LastNode = rll.LastNode;
            Count += rll.Count;
            list.Clear();
        }

        public IEnumerator<T> GetReversedEnumerator()
        {
            DoublyNode<T> prev;
            for (var curr = LastNode; curr != null; curr = prev)
            {
                prev = curr.Prev;
                yield return curr.Item;
            }
        }

        public bool Remove(T item)
        {
            DoublyNode<T> node = null;
            for (var n = FirstNode; n != null; n = n.Next)
            {
                if (!EqualityComparer.Equals(n.Item, item))
                {
                    continue;
                }
                node = n;
                break;
            }
            if (node == null)
            {
                // Node is not contained inside the list.
                return false;
            }

            if (node == FirstNode)
            {
                RemoveFirst();
            }
            else if (node == LastNode)
            {
                RemoveLast();
            }
            else
            {
                RemoveInnerNode(node);
            }
            return true;
        }

        public T RemoveFirst()
        {
            var first = FirstNode.Item;
            FirstNode = FirstNode.Next;
            if (--Count == 0)
            {
                LastNode = null;
            }
            return first;
        }

        public T RemoveLast()
        {
            var last = LastNode.Item;
            LastNode = LastNode.Prev;
            if (--Count == 0)
            {
                FirstNode = null;
            }
            return last;
        }

        public void Reverse()
        {
            DoublyNode<T> next;
            for (var curr = FirstNode; curr != null; curr = next)
            {
                next = curr.Next;
                curr.Next = curr.Prev;
                curr.Prev = next;
            }
            next = FirstNode;
            FirstNode = LastNode;
            LastNode = next;
        }

        #endregion IDoublyLinkedList Members

        #region Private Methods

        private void RemoveInnerNode(DoublyNode<T> node)
        {
            Debug.Assert(node != null && node.Next != null && node.Prev != null);
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
            Count--;
        }

        #endregion Private Methods
    }
}
