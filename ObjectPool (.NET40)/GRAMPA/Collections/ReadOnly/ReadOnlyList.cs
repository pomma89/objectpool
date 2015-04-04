// File name: ReadOnlyList.cs
// 
// Author(s): Alessio Parma <alessio.parma@finsa.it>
// 
// The MIT License (MIT)
// 
// Copyright (c) 2014-2024 Finsa S.p.A. <finsa@finsa.it>
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
using System.Linq;
using CodeProject.ObjectPool.Core;
using CodeProject.ObjectPool.Diagnostics;

namespace CodeProject.ObjectPool.Collections.ReadOnly
{
    internal static class ReadOnlyList
    {
        public static ReadOnlyList<T> Create<T>(IEnumerable<T> items)
        {
            Raise<ArgumentNullException>.IfIsNull(items, ErrorMessages.Collections_ReadOnlyList_NullItems);
            return Create(items.ToArray());
        }

        public static ReadOnlyList<T> Create<T>(params T[] items)
        {
            Raise<ArgumentNullException>.IfIsNull(items, ErrorMessages.Collections_ReadOnlyList_NullItems);
            return (items.Length == 0) ? Empty<T>() : new ReadOnlyList<T>(items);
        }

        public static ReadOnlyList<T> Empty<T>()
        {
            return ReadOnlyList<T>.EmptyList;
        }
    }

    internal sealed class ReadOnlyList<T> : IList<T>
    {
        internal static readonly ReadOnlyList<T> EmptyList = new ReadOnlyList<T>(new T[0]);

        private readonly T[] _items;

        internal ReadOnlyList(T[] items)
        {
            _items = items;
        }

        #region IList<T> Members

        public int Count
        {
            get { return _items.Length; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public T this[int x]
        {
            get
            {
                if (x >= _items.Length)
                {
                    ThrowException();
                }
                return _items[x];
            }
            // ReSharper disable ValueParameterNotUsed
            set
            {
                ThrowException();
            }
            // ReSharper restore ValueParameterNotUsed
        }

        public void Add(T item)
        {
            ThrowException();
        }

        public void Clear()
        {
            ThrowException();
        }

        public bool Contains(T item)
        {
            return _items.Any(i => Equals(i, item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ThrowException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (_items as IEnumerable<T>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            const int notFound = -1;
            for (var i = 0; i < _items.Length; ++i)
            {
                if (Equals(_items[i], item))
                {
                    return i;
                }
            }
            return notFound;
        }

        public void Insert(int index, T item)
        {
            ThrowException();
        }

        public bool Remove(T item)
        {
            ThrowException();
            return default(bool);
        }

        public void RemoveAt(int index)
        {
            ThrowException();
        }

        #endregion IList<T> Members

        private static void ThrowException()
        {
            throw new InvalidOperationException(ErrorMessages.Collections_ReadOnlyList_Immutable);
        }
    }
}