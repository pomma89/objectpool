// File name: SinglyLinkedList.cs
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
    internal sealed class SinglyLinkedList<T> : ListBase<SinglyNode<T>, T>, ILinkedList<T>
    {
        #region Construction

        /// <summary>
        ///   Returns the default implementation of the <see cref="ILinkedList{TItem}"/> interface.
        /// </summary>
        /// <returns>An implementation of the <see cref="ILinkedList{TItem}"/> interface.</returns>
        public SinglyLinkedList()
            : base(EqualityComparer<T>.Default)
        {
            Contract.Ensures(Count == 0);
            Contract.Ensures(ReferenceEquals(EqualityComparer, EqualityComparer<T>.Default));
        }

        /// <summary>
        ///   Returns the default implementation of the <see cref="ILinkedList{TItem}"/> interface,
        ///   using specified equality comparer.
        /// </summary>
        /// <param name="equalityComparer">
        ///   The equality comparer that it will be used to determine whether two items are equal.
        /// </param>
        /// <returns>
        ///   An implementation of the <see cref="ILinkedList{TItem}"/> interface using specified
        ///   equality comparer.
        /// </returns>
        public SinglyLinkedList(IEqualityComparer<T> equalityComparer)
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

        #region IThinLinkedList Members

        public void AddFirst(T item)
        {
            var node = new SinglyNode<T>(item, FirstNode);
            FirstNode = node;
            if (Count++ == 0)
            {
                LastNode = node;
            }
        }

        public bool Remove(T item)
        {
            SinglyNode<T> prev = null;
            SinglyNode<T> node = null;
            for (var n = FirstNode; n != null; prev = n, n = n.Next)
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
            else
            {
                Debug.Assert(prev != null);
                prev.Next = node.Next;
                if (node == LastNode)
                {
                    LastNode = prev;
                }
                Count--;
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

        #endregion IThinLinkedList Members

        #region ILinkedList Members

        public void AddLast(T item)
        {
            var node = new SinglyNode<T>(item, null);
            if (Count++ == 0)
            {
                FirstNode = node;
            }
            else
            {
                LastNode.Next = node;
            }
            LastNode = node;
        }

        public void Append(ILinkedList<T> list)
        {
            if (list.Count == 0)
            {
                return;
            }
            var ll = list as SinglyLinkedList<T>;
            if (ll == null)
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
                LastNode.Next = ll.FirstNode;
            }
            else
            {
                FirstNode = ll.FirstNode;
            }
            LastNode = ll.LastNode;
            Count += ll.Count;
            list.Clear();
        }

        #endregion ILinkedList Members
    }
}
