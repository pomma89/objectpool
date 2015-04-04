// File name: LinkedListsBases.cs
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

using System.Collections;
using System.Collections.Generic;

namespace CodeProject.ObjectPool.Collections.Core
{
    internal abstract class ThinListBase<TN, TI> : IEnumerable<TI> where TN : NodeBase<TN, TI>
    {
        private readonly IEqualityComparer<TI> _equalityComparer;
        protected TN FirstNode;

        internal ThinListBase(IEqualityComparer<TI> equalityComparer)
        {
            _equalityComparer = equalityComparer;
        }

        #region IEnumerable Members

        public IEnumerator<TI> GetEnumerator()
        {
            for (var n = FirstNode; n != null; n = n.Next)
            {
                yield return n.Item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Members

        #region ICollection Members

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int Count { get; protected set; }

        public bool Contains(TI item)
        {
            if (Count == 0)
            {
                return false;
            }
            if (Count == 1)
            {
                return _equalityComparer.Equals(FirstNode.Item, item);
            }
            for (var n = FirstNode; n != null; n = n.Next)
            {
                if (_equalityComparer.Equals(n.Item, item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(TI[] array, int idx)
        {
            var listEn = GetEnumerator();
            for (var i = 0; i < Count; ++i)
            {
                listEn.MoveNext();
                array[idx + i] = listEn.Current;
            }
        }

        #endregion ICollection Members

        #region IThinLinkedList Members

        public IEqualityComparer<TI> EqualityComparer
        {
            get { return _equalityComparer; }
        }

        public TI First
        {
            get { return FirstNode.Item; }
        }

        #endregion IThinLinkedList Members
    }

    internal abstract class ListBase<TN, TI> : ThinListBase<TN, TI> where TN : NodeBase<TN, TI>
    {
        protected TN LastNode;

        internal ListBase(IEqualityComparer<TI> equalityComparer)
            : base(equalityComparer)
        {
        }

        #region ICollection Members

        public void Clear()
        {
            FirstNode = LastNode = null;
            Count = 0;
        }

        #endregion ICollection Members

        #region ILinkedList Members

        public TI Last
        {
            get { return LastNode.Item; }
        }

        #endregion ILinkedList Members
    }

    internal static class ListUtilities<T>
    {
        public static readonly List<T> EmptyList = new List<T>();

        public static List<T> ToList(IEnumerator<T> en)
        {
            var result = new List<T>();
            while (en.MoveNext())
            {
                result.Add(en.Current);
            }
            return result;
        }
    }
}
