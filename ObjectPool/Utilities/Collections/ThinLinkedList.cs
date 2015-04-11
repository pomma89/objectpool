// File name: ThinLinkedList.cs
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
    /// <typeparam name="T">The type of the items the list will contain.</typeparam>
    internal sealed class ThinLinkedList<T> : Core.ThinListBase<Core.SinglyNode<T>, T>, IThinLinkedList<T>
    {
        #region Construction

        /// <summary>
        ///   Returns the default implementation of the <see cref="IThinLinkedList{TItem}"/> interface.
        /// </summary>
        /// <returns>An implementation of the <see cref="IThinLinkedList{TItem}"/> interface.</returns>
        public ThinLinkedList()
            : base(System.Collections.Generic.EqualityComparer<T>.Default)
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == 0);
            System.Diagnostics.Contracts.Contract.Ensures(ReferenceEquals(EqualityComparer, System.Collections.Generic.EqualityComparer<T>.Default));
        }

        /// <summary>
        ///   Returns the default implementation of the <see cref="IThinLinkedList{TItem}"/>
        ///   interface, using specified equality comparer.
        /// </summary>
        /// <param name="equalityComparer">
        ///   The equality comparer that it will be used to determine whether two items are equal.
        /// </param>
        /// <returns>
        ///   An implementation of the <see cref="IThinLinkedList{TItem}"/> interface using
        ///   specified equality comparer.
        /// </returns>
        public ThinLinkedList(System.Collections.Generic.IEqualityComparer<T> equalityComparer)
            : base(equalityComparer)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentNullException>(equalityComparer != null);
            System.Diagnostics.Contracts.Contract.Ensures(Count == 0);
            System.Diagnostics.Contracts.Contract.Ensures(ReferenceEquals(EqualityComparer, equalityComparer));
        }

        #endregion Construction

        #region ICollection Members

        public void Add(T item)
        {
            AddFirst(item);
        }

        public void Clear()
        {
            FirstNode = null;
            Count = 0;
        }

        #endregion ICollection Members

        #region IThinLinkedList Members

        public void AddFirst(T item)
        {
            FirstNode = new Core.SinglyNode<T>(item, FirstNode);
            Count++;
        }

        public bool Remove(T item)
        {
            Core.SinglyNode<T> prev = null;
            Core.SinglyNode<T> node = null;
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
                System.Diagnostics.Debug.Assert(prev != null);
                prev.Next = node.Next;
                Count--;
            }
            return true;
        }

        public T RemoveFirst()
        {
            var first = FirstNode.Item;
            FirstNode = FirstNode.Next;
            Count--;
            return first;
        }

        #endregion IThinLinkedList Members
    }
}