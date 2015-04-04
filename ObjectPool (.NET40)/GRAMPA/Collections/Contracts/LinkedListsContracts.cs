// File name: Contracts.cs
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
using CodeProject.ObjectPool.Core;

namespace CodeProject.ObjectPool.Collections.Contracts
{
    [ContractClassFor(typeof(IThinLinkedList<>))]
    internal abstract class ThinLinkedListContract<T> : IThinLinkedList<T>
    {
        public T First
        {
            get
            {
                Contract.Requires<InvalidOperationException>(Count > 0, ErrorMessages.EmptyList);
                return default(T);
            }
        }

        public IEqualityComparer<T> EqualityComparer
        {
            get
            {
                Contract.Ensures(Contract.Result<IEqualityComparer<T>>() != null);
                return EqualityComparer<T>.Default;
            }
        }

        public void AddFirst(T item)
        {
            Contract.Ensures(Count == Contract.OldValue(Count) + 1);
            Contract.Ensures(EqualityComparer.Equals(First, item));
            Contract.Ensures(Contains(item));
        }

        public T RemoveFirst()
        {
            Contract.Requires<InvalidOperationException>(Count > 0, ErrorMessages.EmptyList);
            Contract.Ensures(Count == Contract.OldValue(Count) - 1);
            Contract.Ensures(EqualityComparer.Equals(Contract.Result<T>(), Contract.OldValue(First)));
            return default(T);
        }

        #region ICollection Members

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract void Add(T item);

        public abstract void Clear();

        public abstract bool Contains(T item);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract IEnumerator<T> GetEnumerator();

        public abstract bool Remove(T item);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion ICollection Members
    }

    [ContractClassFor(typeof(ILinkedList<>))]
    internal abstract class LinkedListContract<T> : ILinkedList<T>
    {
        public T Last
        {
            get
            {
                Contract.Requires<InvalidOperationException>(Count > 0, ErrorMessages.EmptyList);
                return default(T);
            }
        }

        public void AddLast(T item)
        {
            Contract.Ensures(Count == Contract.OldValue(Count) + 1);
            Contract.Ensures(EqualityComparer.Equals(Last, item));
            Contract.Ensures(Contains(item));
        }

        public void Append(ILinkedList<T> list)
        {
            Contract.Requires<ArgumentNullException>(list != null, ErrorMessages.NullList);
            Contract.Ensures(Count == Contract.OldValue(Count) + Contract.OldValue(list.Count));
            Contract.Ensures(list.Count == 0);
            Contract.Ensures(EqualityComparer.Equals(Last, Contract.OldValue(list.Last)));
        }

        #region IThinLinkedList Members

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract void Add(T item);

        public abstract void Clear();

        public abstract bool Contains(T item);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract IEnumerator<T> GetEnumerator();

        public abstract bool Remove(T item);

        public abstract IEqualityComparer<T> EqualityComparer { get; }

        public abstract T First { get; }

        public abstract void AddFirst(T item);

        public abstract T RemoveFirst();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IThinLinkedList Members
    }

    [ContractClassFor(typeof(IDoublyLinkedList<>))]
    internal abstract class DoublyLinkedListContract<T> : IDoublyLinkedList<T>
    {
        public void Append(IDoublyLinkedList<T> list)
        {
            Contract.Requires<ArgumentNullException>(list != null, ErrorMessages.NullList);
            Contract.Ensures(Count == Contract.OldValue(Count) + Contract.OldValue(list.Count));
            Contract.Ensures(list.Count == 0);
            Contract.Ensures(EqualityComparer.Equals(Last, Contract.OldValue(list.Last)));
        }

        public IEnumerator<T> GetReversedEnumerator()
        {
            Contract.Ensures(Contract.Result<IEnumerator<T>>() != null);
            return new List<T>().GetEnumerator();
        }

        public T RemoveLast()
        {
            Contract.Requires<InvalidOperationException>(Count > 0, ErrorMessages.EmptyList);
            Contract.Ensures(Count == Contract.OldValue(Count) - 1);
            Contract.Ensures(EqualityComparer.Equals(Contract.Result<T>(), Contract.OldValue(Last)));
            return default(T);
        }

        public void Reverse()
        {
            Contract.Ensures(Count == Contract.OldValue(Count));
        }

        #region ILinkedList Members

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract void Add(T item);

        public abstract void Clear();

        public abstract bool Contains(T item);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract IEnumerator<T> GetEnumerator();

        public abstract bool Remove(T item);

        public abstract IEqualityComparer<T> EqualityComparer { get; }

        public abstract T First { get; }

        public abstract void AddFirst(T item);

        public abstract T RemoveFirst();

        public abstract T Last { get; }

        public abstract void AddLast(T item);

        public abstract void Append(ILinkedList<T> list);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion ILinkedList Members
    }

    [ContractClassFor(typeof(IHashLinkedList<>))]
    internal abstract class HashLinkedListContract<T> : IHashLinkedList<T>
    {
        public IEqualityComparer<T> EqualityComparer
        {
            get
            {
                Contract.Ensures(Contract.Result<IEqualityComparer<T>>() != null);
                return EqualityComparer<T>.Default;
            }
        }

        public T First
        {
            get
            {
                Contract.Requires<InvalidOperationException>(Count > 0, ErrorMessages.EmptyList);
                return default(T);
            }
        }

        public T Last
        {
            get
            {
                Contract.Requires<InvalidOperationException>(Count > 0, ErrorMessages.EmptyList);
                return default(T);
            }
        }

        public void AddAfter(T after, T toAdd)
        {
            Contract.Requires<ArgumentException>(Contains(after), ErrorMessages.NotContainedItem);
            Contract.Requires<ArgumentException>(!Contains(toAdd), ErrorMessages.ContainedItem);
            Contract.Ensures(Count == Contract.OldValue(Count) + 1);
            Contract.Ensures(Contains(toAdd));
        }

        public void AddBefore(T before, T toAdd)
        {
            Contract.Requires<ArgumentException>(Contains(before), ErrorMessages.NotContainedItem);
            Contract.Requires<ArgumentException>(!Contains(toAdd), ErrorMessages.ContainedItem);
            Contract.Ensures(Count == Contract.OldValue(Count) + 1);
            Contract.Ensures(Contains(toAdd));
        }

        public void AddFirst(T item)
        {
            Contract.Requires<ArgumentException>(!Contains(item), ErrorMessages.ContainedItem);
            Contract.Ensures(Count == Contract.OldValue(Count) + 1);
            Contract.Ensures(EqualityComparer.Equals(First, item));
            Contract.Ensures(Contains(item));
        }

        public void AddLast(T item)
        {
            Contract.Requires<ArgumentException>(!Contains(item), ErrorMessages.ContainedItem);
            Contract.Ensures(Count == Contract.OldValue(Count) + 1);
            Contract.Ensures(EqualityComparer.Equals(Last, item));
            Contract.Ensures(Contains(item));
        }

        public IEnumerator<T> GetReversedEnumerator()
        {
            Contract.Ensures(Contract.Result<IEnumerator<T>>() != null);
            return new List<T>().GetEnumerator();
        }

        public T RemoveAfter(T after)
        {
            Contract.Requires<ArgumentException>(Contains(after), ErrorMessages.NotContainedItem);
            Contract.Requires<InvalidOperationException>(!EqualityComparer.Equals(after, Last));
            Contract.Ensures(Count == Contract.OldValue(Count) - 1);
            Contract.Ensures(Contains(after));
            return default(T);
        }

        public T RemoveBefore(T before)
        {
            Contract.Requires<ArgumentException>(Contains(before), ErrorMessages.NotContainedItem);
            Contract.Requires<InvalidOperationException>(!EqualityComparer.Equals(before, First));
            Contract.Ensures(Count == Contract.OldValue(Count) - 1);
            Contract.Ensures(Contains(before));
            return default(T);
        }

        public T RemoveFirst()
        {
            Contract.Requires<InvalidOperationException>(Count > 0, ErrorMessages.EmptyList);
            Contract.Ensures(Count == Contract.OldValue(Count) - 1);
            Contract.Ensures(EqualityComparer.Equals(Contract.Result<T>(), Contract.OldValue(First)));
            return default(T);
        }

        public T RemoveLast()
        {
            Contract.Requires<InvalidOperationException>(Count > 0, ErrorMessages.EmptyList);
            Contract.Ensures(Count == Contract.OldValue(Count) - 1);
            Contract.Ensures(EqualityComparer.Equals(Contract.Result<T>(), Contract.OldValue(Last)));
            return default(T);
        }

        public void Reverse()
        {
            Contract.Ensures(Count == Contract.OldValue(Count));
        }

        #region ICollection Members

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract void Add(T item);

        public abstract void Clear();

        public abstract bool Contains(T item);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract IEnumerator<T> GetEnumerator();

        public abstract bool Remove(T item);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion ICollection Members
    }

    [ContractClassFor(typeof(ILinkedQueue<>))]
    internal abstract class LinkedQueueContract<T> : ILinkedQueue<T>
    {
        public int Count
        {
            get
            {
                Contract.Ensures(Count >= 0);
                return default(int);
            }
        }

        public T Dequeue()
        {
            Contract.Requires<InvalidOperationException>(Count > 0, ErrorMessages.EmptyQueue);
            Contract.Ensures(Count == Contract.OldValue(Count) - 1);
            Contract.Ensures(Equals(Contract.Result<T>(), Contract.OldValue(Peek())));
            return default(T);
        }

        public void Enqueue(T item)
        {
            Contract.Ensures(Count == Contract.OldValue(Count) + 1);
        }

        public T Peek()
        {
            Contract.Ensures(Count == Contract.OldValue(Count));
            return default(T);
        }

        #region IEnumerable Members

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Members
    }

    [ContractClassFor(typeof(ILinkedStack<>))]
    internal abstract class LinkedStackContract<T> : ILinkedStack<T>
    {
        public int Count
        {
            get
            {
                Contract.Ensures(Count >= 0);
                return default(int);
            }
        }

        public T Pop()
        {
            Contract.Requires<InvalidOperationException>(Count > 0, ErrorMessages.EmptyStack);
            Contract.Ensures(Count == Contract.OldValue(Count) - 1);
            Contract.Ensures(Equals(Contract.Result<T>(), Contract.OldValue(Top())));
            return default(T);
        }

        public void Push(T item)
        {
            Contract.Ensures(Count == Contract.OldValue(Count) + 1);
            Contract.Ensures(Equals(Top(), item));
        }

        public T Top()
        {
            Contract.Ensures(Count == Contract.OldValue(Count));
            return default(T);
        }

        #region IEnumerable Members

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Members
    }
}
