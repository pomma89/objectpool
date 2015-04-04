// File name: PairAndTriple.cs
// 
// Author(s): Alessio Parma <alessio.parma@gmail.com>
// 
// The MIT License (MIT)
// 
// Copyright (c) 2014-2016 Alessio Parma <alessio.parma@gmail.com>
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
using Newtonsoft.Json;
using CodeProject.ObjectPool.Core;
using CodeProject.ObjectPool.Extensions;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   A simple pair.
    /// </summary>
    /// <typeparam name="T1">The type of the first element.</typeparam>
    /// <typeparam name="T2">The type of the second element.</typeparam>
    [Serializable, JsonObject(MemberSerialization.OptIn)]
    internal sealed class GPair<T1, T2> : EquatableObject<GPair<T1, T2>>, IList<object>
    {
        /// <summary>
        ///   Gets or sets the first element of the pair.
        /// </summary>
        /// <value>The first element of the pair.</value>
        [JsonProperty(Order = 0)]
        public T1 First { get; set; }

        /// <summary>
        ///   Gets or sets the second element of the pair.
        /// </summary>
        /// <value>The second element of the pair.</value>
        [JsonProperty(Order = 1)]
        public T2 Second { get; set; }

        #region EquatableObject<GPair<T1,T2>> Members

        protected override IEnumerable<GKeyValuePair<string, string>> GetFormattingMembers()
        {
            yield return GKeyValuePair.Create("First", First.SafeToString());
            yield return GKeyValuePair.Create("Second", Second.SafeToString());
        }

        protected override IEnumerable<object> GetIdentifyingMembers()
        {
            yield return First;
            yield return Second;
        }

        #endregion EquatableObject<GPair<T1,T2>> Members

        #region IList<object> Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///   Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate
        ///   through the collection.
        /// </returns>
        public IEnumerator<object> GetEnumerator()
        {
            yield return First;
            yield return Second;
        }

        /// <summary>
        ///   Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Add(object item)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Pair_NotFullCollection);
        }

        /// <summary>
        ///   Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Clear()
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Pair_NotFullCollection);
        }

        /// <summary>
        ///   Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/>
        ///   contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        ///   true if <paramref name="item"/> is found in the <see
        ///   cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(object item)
        {
            return Equals(First, item) || Equals(Second, item);
        }

        /// <summary>
        ///   Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(object[] array, int arrayIndex)
        {
            array[arrayIndex] = First;
            array[arrayIndex + 1] = Second;
        }

        /// <summary>
        ///   Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        ///   true if <paramref name="item"/> was successfully removed from the <see
        ///   cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method
        ///   also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Remove(object item)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Pair_NotFullCollection);
        }

        /// <summary>
        ///   Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        public int Count
        {
            get { return 2; }
        }

        /// <summary>
        ///   Gets a value indicating whether the <see
        ///   cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///   Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <returns>
        ///   The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(object item)
        {
            return Equals(First, item) ? 0 : Equals(Second, item) ? 1 : -1;
        }

        /// <summary>
        ///   Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the
        ///   specified index.
        /// </summary>
        /// <param name="index">
        ///   The zero-based index at which <paramref name="item"/> should be inserted.
        /// </param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Insert(int index, object item)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Pair_NotFullCollection);
        }

        /// <summary>
        ///   Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void RemoveAt(int index)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Pair_NotFullCollection);
        }

        /// <summary>
        ///   Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public object this[int index]
        {
            get
            {
                return (index == 0) ? First : (index == 1) ? Second : (object) null;
            }
            set
            {
                switch (index)
                {
                    case 0:
                        First = (T1) value;
                        break;

                    case 1:
                        Second = (T2) value;
                        break;
                }
            }
        }

        #endregion IList<object> Members
    }

    internal static class GPair
    {
        public static GPair<T1, T2> Create<T1, T2>(T1 first, T2 second)
        {
            return new GPair<T1, T2> { First = first, Second = second };
        }
    }

    /// <summary>
    ///   A simple key value pair.
    /// </summary>
    /// <typeparam name="T1">The type of the key.</typeparam>
    /// <typeparam name="T2">The type of the value.</typeparam>
    [Serializable, JsonObject(MemberSerialization.OptIn)]
    internal sealed class GKeyValuePair<T1, T2> : EquatableObject<GKeyValuePair<T1, T2>>, IList<object>
    {
        /// <summary>
        ///   Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        [JsonProperty(Order = 0)]
        public T1 Key { get; set; }

        /// <summary>
        ///   Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [JsonProperty(Order = 1)]
        public T2 Value { get; set; }

        #region EquatableObject<GKeyValuePair<T1,T2>> Members

        protected override IEnumerable<GKeyValuePair<string, string>> GetFormattingMembers()
        {
            yield return GKeyValuePair.Create("Key", Key.SafeToString());
            yield return GKeyValuePair.Create("Value", Value.SafeToString());
        }

        protected override IEnumerable<object> GetIdentifyingMembers()
        {
            yield return Key;
        }

        #endregion EquatableObject<GKeyValuePair<T1,T2>> Members

        #region IList<object> Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///   Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate
        ///   through the collection.
        /// </returns>
        public IEnumerator<object> GetEnumerator()
        {
            yield return Key;
            yield return Value;
        }

        /// <summary>
        ///   Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Add(object item)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Pair_NotFullCollection);
        }

        /// <summary>
        ///   Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Clear()
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Pair_NotFullCollection);
        }

        /// <summary>
        ///   Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/>
        ///   contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        ///   true if <paramref name="item"/> is found in the <see
        ///   cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(object item)
        {
            return Equals(Key, item) || Equals(Value, item);
        }

        /// <summary>
        ///   Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(object[] array, int arrayIndex)
        {
            array[arrayIndex] = Key;
            array[arrayIndex + 1] = Value;
        }

        /// <summary>
        ///   Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        ///   true if <paramref name="item"/> was successfully removed from the <see
        ///   cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method
        ///   also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Remove(object item)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Pair_NotFullCollection);
        }

        /// <summary>
        ///   Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        public int Count
        {
            get { return 2; }
        }

        /// <summary>
        ///   Gets a value indicating whether the <see
        ///   cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///   Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <returns>
        ///   The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(object item)
        {
            return Equals(Key, item) ? 0 : Equals(Value, item) ? 1 : -1;
        }

        /// <summary>
        ///   Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the
        ///   specified index.
        /// </summary>
        /// <param name="index">
        ///   The zero-based index at which <paramref name="item"/> should be inserted.
        /// </param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Insert(int index, object item)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Pair_NotFullCollection);
        }

        /// <summary>
        ///   Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void RemoveAt(int index)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Pair_NotFullCollection);
        }

        /// <summary>
        ///   Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public object this[int index]
        {
            get
            {
                return (index == 0) ? Key : (index == 1) ? Value : (object) null;
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Key = (T1) value;
                        break;

                    case 1:
                        Value = (T2) value;
                        break;
                }
            }
        }

        #endregion IList<object> Members
    }

    internal static class GKeyValuePair
    {
        public static GKeyValuePair<T1, T2> Create<T1, T2>(T1 key, T2 value)
        {
            return new GKeyValuePair<T1, T2> { Key = key, Value = value };
        }
    }

    /// <summary>
    ///   A simple triple.
    /// </summary>
    /// <typeparam name="T1">The type of the first element.</typeparam>
    /// <typeparam name="T2">The type of the second element.</typeparam>
    /// <typeparam name="T3">The type of the third element.</typeparam>
    [Serializable, JsonObject(MemberSerialization.OptIn)]
    internal sealed class GTriple<T1, T2, T3> : EquatableObject<GTriple<T1, T2, T3>>, IList<object>
    {
        /// <summary>
        ///   Gets or sets the first element of the triple.
        /// </summary>
        /// <value>The first element of the triple.</value>
        [JsonProperty(Order = 0)]
        public T1 First { get; set; }

        /// <summary>
        ///   Gets or sets the second element of the triple.
        /// </summary>
        /// <value>The second element of the triple.</value>
        [JsonProperty(Order = 1)]
        public T2 Second { get; set; }

        /// <summary>
        ///   Gets or sets the third element of the triple.
        /// </summary>
        /// <value>The third element of the triple.</value>
        [JsonProperty(Order = 2)]
        public T3 Third { get; set; }

        #region EquatableObject<GTriple<T1, T2, T3>> Members

        protected override IEnumerable<GKeyValuePair<string, string>> GetFormattingMembers()
        {
            yield return GKeyValuePair.Create("First", First.SafeToString());
            yield return GKeyValuePair.Create("Second", Second.SafeToString());
            yield return GKeyValuePair.Create("Third", Third.SafeToString());
        }

        protected override IEnumerable<object> GetIdentifyingMembers()
        {
            yield return First;
            yield return Second;
            yield return Third;
        }

        #endregion EquatableObject<GTriple<T1, T2, T3>> Members

        #region IList<object> Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///   Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate
        ///   through the collection.
        /// </returns>
        public IEnumerator<object> GetEnumerator()
        {
            yield return First;
            yield return Second;
            yield return Third;
        }

        /// <summary>
        ///   Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Add(object item)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Triple_NotFullCollection);
        }

        /// <summary>
        ///   Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Clear()
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Triple_NotFullCollection);
        }

        /// <summary>
        ///   Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/>
        ///   contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        ///   true if <paramref name="item"/> is found in the <see
        ///   cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(object item)
        {
            return Equals(First, item) || Equals(Second, item) || Equals(Third, item);
        }

        /// <summary>
        ///   Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(object[] array, int arrayIndex)
        {
            array[arrayIndex] = First;
            array[arrayIndex + 1] = Second;
            array[arrayIndex + 2] = Third;
        }

        /// <summary>
        ///   Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        ///   true if <paramref name="item"/> was successfully removed from the <see
        ///   cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method
        ///   also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Remove(object item)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Triple_NotFullCollection);
        }

        /// <summary>
        ///   Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        public int Count
        {
            get { return 3; }
        }

        /// <summary>
        ///   Gets a value indicating whether the <see
        ///   cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///   Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <returns>
        ///   The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(object item)
        {
            return Equals(First, item) ? 0 : Equals(Second, item) ? 1 : Equals(Third, item) ? 2 : -1;
        }

        /// <summary>
        ///   Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the
        ///   specified index.
        /// </summary>
        /// <param name="index">
        ///   The zero-based index at which <paramref name="item"/> should be inserted.
        /// </param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Insert(int index, object item)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Triple_NotFullCollection);
        }

        /// <summary>
        ///   Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void RemoveAt(int index)
        {
            throw new NotImplementedException(ErrorMessages.TopLevel_Triple_NotFullCollection);
        }

        /// <summary>
        ///   Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public object this[int index]
        {
            get
            {
                return (index == 0) ? First : (index == 1) ? Second : (index == 2) ? Third : (object) null;
            }
            set
            {
                switch (index)
                {
                    case 0:
                        First = (T1) value;
                        break;

                    case 1:
                        Second = (T2) value;
                        break;

                    case 2:
                        Third = (T3) value;
                        break;
                }
            }
        }

        #endregion IList<object> Members
    }

    internal static class GTriple
    {
        public static GTriple<T1, T2, T3> Create<T1, T2, T3>(T1 first, T2 second, T3 third)
        {
            return new GTriple<T1, T2, T3> { First = first, Second = second, Third = third };
        }
    }
}
