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

namespace CodeProject.ObjectPool.Utilities.Collections.Contracts
{
    [System.Diagnostics.Contracts.ContractClassFor(typeof(IThinLinkedList<>))]
    internal abstract class ThinLinkedListContract<T> : IThinLinkedList<T>
    {
        public T First
        {
            get
            {
                System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(Count > 0, Utilities.Core.ErrorMessages.EmptyList);
                return default(T);
            }
        }

        public System.Collections.Generic.IEqualityComparer<T> EqualityComparer
        {
            get
            {
                System.Diagnostics.Contracts.Contract.Ensures(System.Diagnostics.Contracts.Contract.Result<System.Collections.Generic.IEqualityComparer<T>>() != null);
                return System.Collections.Generic.EqualityComparer<T>.Default;
            }
        }

        public void AddFirst(T item)
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) + 1);
            System.Diagnostics.Contracts.Contract.Ensures(EqualityComparer.Equals(First, item));
            System.Diagnostics.Contracts.Contract.Ensures(Contains(item));
        }

        public T RemoveFirst()
        {
            System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(Count > 0, Utilities.Core.ErrorMessages.EmptyList);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) - 1);
            System.Diagnostics.Contracts.Contract.Ensures(EqualityComparer.Equals(System.Diagnostics.Contracts.Contract.Result<T>(), System.Diagnostics.Contracts.Contract.OldValue(First)));
            return default(T);
        }

        #region ICollection Members

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract void Add(T item);

        public abstract void Clear();

        public abstract bool Contains(T item);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract System.Collections.Generic.IEnumerator<T> GetEnumerator();

        public abstract bool Remove(T item);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion ICollection Members
    }

    [System.Diagnostics.Contracts.ContractClassFor(typeof(ILinkedList<>))]
    internal abstract class LinkedListContract<T> : ILinkedList<T>
    {
        public T Last
        {
            get
            {
                System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(Count > 0, Utilities.Core.ErrorMessages.EmptyList);
                return default(T);
            }
        }

        public void AddLast(T item)
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) + 1);
            System.Diagnostics.Contracts.Contract.Ensures(EqualityComparer.Equals(Last, item));
            System.Diagnostics.Contracts.Contract.Ensures(Contains(item));
        }

        public void Append(ILinkedList<T> list)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentNullException>(list != null, Utilities.Core.ErrorMessages.NullList);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) + System.Diagnostics.Contracts.Contract.OldValue(list.Count));
            System.Diagnostics.Contracts.Contract.Ensures(list.Count == 0);
            System.Diagnostics.Contracts.Contract.Ensures(System.Diagnostics.Contracts.Contract.OldValue(list.Count) == 0 || EqualityComparer.Equals(Last, System.Diagnostics.Contracts.Contract.OldValue(list.Last)));
        }

        #region IThinLinkedList Members

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract void Add(T item);

        public abstract void Clear();

        public abstract bool Contains(T item);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract System.Collections.Generic.IEnumerator<T> GetEnumerator();

        public abstract bool Remove(T item);

        public abstract System.Collections.Generic.IEqualityComparer<T> EqualityComparer { get; }

        public abstract T First { get; }

        public abstract void AddFirst(T item);

        public abstract T RemoveFirst();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IThinLinkedList Members
    }

    [System.Diagnostics.Contracts.ContractClassFor(typeof(IDoublyLinkedList<>))]
    internal abstract class DoublyLinkedListContract<T> : IDoublyLinkedList<T>
    {
        public void Append(IDoublyLinkedList<T> list)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentNullException>(list != null, Utilities.Core.ErrorMessages.NullList);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) + System.Diagnostics.Contracts.Contract.OldValue(list.Count));
            System.Diagnostics.Contracts.Contract.Ensures(list.Count == 0);
            System.Diagnostics.Contracts.Contract.Ensures(EqualityComparer.Equals(Last, System.Diagnostics.Contracts.Contract.OldValue(list.Last)));
        }

        public System.Collections.Generic.IEnumerator<T> GetReversedEnumerator()
        {
            System.Diagnostics.Contracts.Contract.Ensures(System.Diagnostics.Contracts.Contract.Result<System.Collections.Generic.IEnumerator<T>>() != null);
            return new System.Collections.Generic.List<T>().GetEnumerator();
        }

        public T RemoveLast()
        {
            System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(Count > 0, Utilities.Core.ErrorMessages.EmptyList);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) - 1);
            System.Diagnostics.Contracts.Contract.Ensures(EqualityComparer.Equals(System.Diagnostics.Contracts.Contract.Result<T>(), System.Diagnostics.Contracts.Contract.OldValue(Last)));
            return default(T);
        }

        public void Reverse()
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count));
        }

        #region ILinkedList Members

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract void Add(T item);

        public abstract void Clear();

        public abstract bool Contains(T item);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract System.Collections.Generic.IEnumerator<T> GetEnumerator();

        public abstract bool Remove(T item);

        public abstract System.Collections.Generic.IEqualityComparer<T> EqualityComparer { get; }

        public abstract T First { get; }

        public abstract void AddFirst(T item);

        public abstract T RemoveFirst();

        public abstract T Last { get; }

        public abstract void AddLast(T item);

        public abstract void Append(ILinkedList<T> list);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion ILinkedList Members
    }

    [System.Diagnostics.Contracts.ContractClassFor(typeof(IHashLinkedList<>))]
    internal abstract class HashLinkedListContract<T> : IHashLinkedList<T>
    {
        public System.Collections.Generic.IEqualityComparer<T> EqualityComparer
        {
            get
            {
                System.Diagnostics.Contracts.Contract.Ensures(System.Diagnostics.Contracts.Contract.Result<System.Collections.Generic.IEqualityComparer<T>>() != null);
                return System.Collections.Generic.EqualityComparer<T>.Default;
            }
        }

        public T First
        {
            get
            {
                System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(Count > 0, Utilities.Core.ErrorMessages.EmptyList);
                return default(T);
            }
        }

        public T Last
        {
            get
            {
                System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(Count > 0, Utilities.Core.ErrorMessages.EmptyList);
                return default(T);
            }
        }

        public void AddAfter(T after, T toAdd)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentException>(Contains(after), Utilities.Core.ErrorMessages.NotContainedItem);
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentException>(!Contains(toAdd), Utilities.Core.ErrorMessages.ContainedItem);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) + 1);
            System.Diagnostics.Contracts.Contract.Ensures(Contains(toAdd));
        }

        public void AddBefore(T before, T toAdd)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentException>(Contains(before), Utilities.Core.ErrorMessages.NotContainedItem);
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentException>(!Contains(toAdd), Utilities.Core.ErrorMessages.ContainedItem);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) + 1);
            System.Diagnostics.Contracts.Contract.Ensures(Contains(toAdd));
        }

        public void AddFirst(T item)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentException>(!Contains(item), Utilities.Core.ErrorMessages.ContainedItem);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) + 1);
            System.Diagnostics.Contracts.Contract.Ensures(EqualityComparer.Equals(First, item));
            System.Diagnostics.Contracts.Contract.Ensures(Contains(item));
        }

        public void AddLast(T item)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentException>(!Contains(item), Utilities.Core.ErrorMessages.ContainedItem);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) + 1);
            System.Diagnostics.Contracts.Contract.Ensures(EqualityComparer.Equals(Last, item));
            System.Diagnostics.Contracts.Contract.Ensures(Contains(item));
        }

        public System.Collections.Generic.IEnumerator<T> GetReversedEnumerator()
        {
            System.Diagnostics.Contracts.Contract.Ensures(System.Diagnostics.Contracts.Contract.Result<System.Collections.Generic.IEnumerator<T>>() != null);
            return new System.Collections.Generic.List<T>().GetEnumerator();
        }

        public T RemoveAfter(T after)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentException>(Contains(after), Utilities.Core.ErrorMessages.NotContainedItem);
            System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(!EqualityComparer.Equals(after, Last));
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) - 1);
            System.Diagnostics.Contracts.Contract.Ensures(Contains(after));
            return default(T);
        }

        public T RemoveBefore(T before)
        {
            System.Diagnostics.Contracts.Contract.Requires<System.ArgumentException>(Contains(before), Utilities.Core.ErrorMessages.NotContainedItem);
            System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(!EqualityComparer.Equals(before, First));
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) - 1);
            System.Diagnostics.Contracts.Contract.Ensures(Contains(before));
            return default(T);
        }

        public T RemoveFirst()
        {
            System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(Count > 0, Utilities.Core.ErrorMessages.EmptyList);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) - 1);
            System.Diagnostics.Contracts.Contract.Ensures(EqualityComparer.Equals(System.Diagnostics.Contracts.Contract.Result<T>(), System.Diagnostics.Contracts.Contract.OldValue(First)));
            return default(T);
        }

        public T RemoveLast()
        {
            System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(Count > 0, Utilities.Core.ErrorMessages.EmptyList);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) - 1);
            System.Diagnostics.Contracts.Contract.Ensures(EqualityComparer.Equals(System.Diagnostics.Contracts.Contract.Result<T>(), System.Diagnostics.Contracts.Contract.OldValue(Last)));
            return default(T);
        }

        public void Reverse()
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count));
        }

        #region ICollection Members

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract void Add(T item);

        public abstract void Clear();

        public abstract bool Contains(T item);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract System.Collections.Generic.IEnumerator<T> GetEnumerator();

        public abstract bool Remove(T item);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion ICollection Members
    }

    [System.Diagnostics.Contracts.ContractClassFor(typeof(ILinkedQueue<>))]
    internal abstract class LinkedQueueContract<T> : ILinkedQueue<T>
    {
        public int Count
        {
            get
            {
                System.Diagnostics.Contracts.Contract.Ensures(Count >= 0);
                return default(int);
            }
        }

        public T Dequeue()
        {
            System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(Count > 0, Utilities.Core.ErrorMessages.EmptyQueue);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) - 1);
            System.Diagnostics.Contracts.Contract.Ensures(Equals(System.Diagnostics.Contracts.Contract.Result<T>(), System.Diagnostics.Contracts.Contract.OldValue(Peek())));
            return default(T);
        }

        public void Enqueue(T item)
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) + 1);
        }

        public T Peek()
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count));
            return default(T);
        }

        #region IEnumerable Members

        public abstract System.Collections.Generic.IEnumerator<T> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Members
    }

    [System.Diagnostics.Contracts.ContractClassFor(typeof(ILinkedStack<>))]
    internal abstract class LinkedStackContract<T> : ILinkedStack<T>
    {
        public int Count
        {
            get
            {
                System.Diagnostics.Contracts.Contract.Ensures(Count >= 0);
                return default(int);
            }
        }

        public T Pop()
        {
            System.Diagnostics.Contracts.Contract.Requires<System.InvalidOperationException>(Count > 0, Utilities.Core.ErrorMessages.EmptyStack);
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) - 1);
            System.Diagnostics.Contracts.Contract.Ensures(Equals(System.Diagnostics.Contracts.Contract.Result<T>(), System.Diagnostics.Contracts.Contract.OldValue(Top())));
            return default(T);
        }

        public void Push(T item)
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count) + 1);
            System.Diagnostics.Contracts.Contract.Ensures(Equals(Top(), item));
        }

        public T Top()
        {
            System.Diagnostics.Contracts.Contract.Ensures(Count == System.Diagnostics.Contracts.Contract.OldValue(Count));
            return default(T);
        }

        #region IEnumerable Members

        public abstract System.Collections.Generic.IEnumerator<T> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Members
    }
}