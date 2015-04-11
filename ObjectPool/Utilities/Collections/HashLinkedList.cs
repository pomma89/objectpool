// File name: HashLinkedList.cs
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
    internal sealed class HashLinkedList<T> : Core.ListBase<Core.DoublyNode<T>, T>, IHashLinkedList<T>
    {
        #region Fields

        private readonly System.Collections.Generic.Dictionary<T, Core.DoublyNode<T>> _nodes;

        #endregion Fields

        #region Construction

        /// <summary>
        ///   Returns the default implementation of the <see cref="IHashLinkedList{TItem}"/> interface.
        /// </summary>
        /// <returns>An implementation of the <see cref="IHashLinkedList{TItem}"/> interface.</returns>
        public HashLinkedList()
            : base(System.Collections.Generic.EqualityComparer<T>.Default)
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == 0);
            System.Diagnostics.Contracts.Contract.Ensures(ReferenceEquals(EqualityComparer, System.Collections.Generic.EqualityComparer<T>.Default));
            _nodes = new System.Collections.Generic.Dictionary<T, Core.DoublyNode<T>>(System.Collections.Generic.EqualityComparer<T>.Default);
        }

        /// <summary>
        ///   Returns the default implementation of the <see cref="IHashLinkedList{TItem}"/>
        ///   interface, using specified equality comparer.
        /// </summary>
        /// <param name="equalityComparer">
        ///   The equality comparer that it will be used to determine whether two items are equal.
        /// </param>
        /// <returns>
        ///   An implementation of the <see cref="IHashLinkedList{TItem}"/> interface using
        ///   specified equality comparer.
        /// </returns>
        public HashLinkedList(System.Collections.Generic.IEqualityComparer<T> equalityComparer)
            : base(equalityComparer)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentNullException>(equalityComparer != null);
            System.Diagnostics.Contracts.Contract.Ensures(Count == 0);
            System.Diagnostics.Contracts.Contract.Ensures(ReferenceEquals(EqualityComparer, equalityComparer));
            _nodes = new System.Collections.Generic.Dictionary<T, Core.DoublyNode<T>>(equalityComparer);
        }

        #endregion Construction

        #region ICollection Members

        public void Add(T item)
        {
            AddLast(item);
        }

        #endregion ICollection Members

        #region IHashLinkedList Members

        public void AddAfter(T after, T toAdd)
        {
            if (after.Equals(LastNode.Item))
            {
                AddLast(toAdd);
                return;
            }
            var afterNode = _nodes[after];
            var node = new Core.DoublyNode<T>(toAdd, afterNode.Next, afterNode);
            _nodes.Add(toAdd, node);
            afterNode.Next.Prev = node;
            afterNode.Next = node;
            Count++;
        }

        public void AddBefore(T before, T toAdd)
        {
            if (before.Equals(FirstNode.Item))
            {
                AddFirst(toAdd);
                return;
            }
            var beforeNode = _nodes[before];
            var node = new Core.DoublyNode<T>(toAdd, beforeNode, beforeNode.Prev);
            _nodes.Add(toAdd, node);
            beforeNode.Prev.Next = node;
            beforeNode.Prev = node;
            Count++;
        }

        public void AddFirst(T item)
        {
            var node = new Core.DoublyNode<T>(item, FirstNode, null);
            _nodes.Add(item, node);

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
            var node = new Core.DoublyNode<T>(item, null, LastNode);
            _nodes.Add(item, node);

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

        public new void Clear()
        {
            base.Clear();
            _nodes.Clear();
        }

        public new bool Contains(T item)
        {
            return _nodes.ContainsKey(item);
        }

        public System.Collections.Generic.IEnumerator<T> GetReversedEnumerator()
        {
            Core.DoublyNode<T> prev;
            for (var curr = LastNode; curr != null; curr = prev)
            {
                prev = curr.Prev;
                yield return curr.Item;
            }
        }

        public bool Remove(T item)
        {
            Core.DoublyNode<T> node;
            if (!_nodes.TryGetValue(item, out node))
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
            System.Diagnostics.Debug.Assert(Count == _nodes.Count);
            return true;
        }

        public T RemoveAfter(T after)
        {
            var afterNode = _nodes[after];
            if (ReferenceEquals(afterNode.Next, LastNode))
            {
                return RemoveLast();
            }
            var afterItem = afterNode.Next.Item;
            afterNode.Next = afterNode.Next.Next;
            afterNode.Next.Prev = afterNode;
            Count--;
            return afterItem;
        }

        public T RemoveBefore(T before)
        {
            var beforeNode = _nodes[before];
            if (ReferenceEquals(beforeNode.Prev, FirstNode))
            {
                return RemoveFirst();
            }
            var beforeItem = beforeNode.Prev.Item;
            beforeNode.Prev = beforeNode.Prev.Prev;
            beforeNode.Prev.Next = beforeNode;
            Count--;
            return beforeItem;
        }

        public T RemoveFirst()
        {
            var first = FirstNode.Item;
            _nodes.Remove(first);
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
            _nodes.Remove(last);
            LastNode = LastNode.Prev;
            if (--Count == 0)
            {
                FirstNode = null;
            }
            return last;
        }

        public void Reverse()
        {
            Core.DoublyNode<T> next;
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

        #endregion IHashLinkedList Members

        #region Private Methods

        private void RemoveInnerNode(Core.DoublyNode<T> node)
        {
            System.Diagnostics.Debug.Assert(node != null && node.Next != null && node.Prev != null);
            _nodes.Remove(node.Item);
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
            Count--;
        }

        #endregion Private Methods
    }
}