// File name: ReadOnlyDictionary.cs
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

namespace CodeProject.ObjectPool.Utilities.Collections.ReadOnly
{
    /// <summary>
    ///   Provides the base class for a generic read-only dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <remarks>
    ///   <para>
    ///     An instance of the <b>ReadOnlyDictionary</b> generic class is always read-only. A
    ///     dictionary that is read-only is simply a dictionary with a wrapper that prevents
    ///     modifying the dictionary; therefore, if changes are made to the underlying dictionary,
    ///     the read-only dictionary reflects those changes. See
    ///     <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/> for a modifiable
    ///     version of this class.
    ///   </para>
    ///   <para>
    ///     <b>Notes to Implementers</b> This base class is provided to make it easier for
    ///     implementers to create a generic read-only custom dictionary. Implementers are
    ///     encouraged to extend this base class instead of creating their own.
    ///   </para>
    /// </remarks>
    [System.Serializable]
    [System.Diagnostics.DebuggerDisplay("Count = {Count}")]
    [System.Diagnostics.DebuggerTypeProxy(typeof(ReadOnlyDictionaryDebugView<,>))]
    internal class ReadOnlyDictionary<TKey, TValue> : System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.ICollection
    {
        private readonly System.Collections.Generic.IDictionary<TKey, TValue> source;
        private object syncRoot;

        /// <summary>
        ///   Initializes a new instance of the <see cref="T:ReadOnlyDictionary`2"/> class that
        ///   wraps the supplied <paramref name="dictionaryToWrap"/>.
        /// </summary>
        /// <param name="dictionaryToWrap">The <see cref="T:IDictionary`2"/> that will be wrapped.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   Thrown when the dictionary is null.
        /// </exception>
        public ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue> dictionaryToWrap)
        {
            if (dictionaryToWrap == null)
            {
                throw new System.ArgumentNullException("dictionaryToWrap");
            }

            source = dictionaryToWrap;
        }

        #region ICollection Members

        /// <summary>
        ///   Gets a value indicating whether access to the dictionary is synchronized (thread safe).
        /// </summary>
        bool System.Collections.ICollection.IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        ///   Gets an object that can be used to synchronize access to dictionary.
        /// </summary>
        object System.Collections.ICollection.SyncRoot
        {
            get
            {
                if (syncRoot == null)
                {
                    var collection = source as System.Collections.ICollection;

                    if (collection != null)
                    {
                        syncRoot = collection.SyncRoot;
                    }
                    else
                    {
                        System.Threading.Interlocked.CompareExchange(ref syncRoot, new object(), null);
                    }
                }

                return syncRoot;
            }
        }

        /// <summary>
        ///   For a description of this member, see <see cref="System.Collections.ICollection.CopyTo"/>.
        /// </summary>
        /// <param name="array">
        ///   The one-dimensional Array that is the destination of the elements copied from
        ///   ICollection. The Array must have zero-based indexing.
        /// </param>
        /// <param name="index">The zero-based index in Array at which copying begins.</param>
        void System.Collections.ICollection.CopyTo(System.Array array, int index)
        {
            System.Collections.ICollection collection =
                new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<TKey, TValue>>(source);

            collection.CopyTo(array, index);
        }

        #endregion ICollection Members

        #region IDictionary<TKey,TValue> Members

        /// <summary>
        ///   Gets the number of key/value pairs contained in the <see cref="T:ReadOnlyDictionary`2"></see>.
        /// </summary>
        /// <value>The number of key/value pairs.</value>
        /// <returns>The number of key/value pairs contained in the <see cref="T:ReadOnlyDictionary`2"></see>.</returns>
        public int Count
        {
            get { return source.Count; }
        }

        /// <summary>
        ///   Gets a collection containing the keys in the <see cref="T:ReadOnlyDictionary{TKey,TValue}"></see>.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Collections.Generic.Dictionary{TKey,TValue}.KeyCollection"/>
        ///   containing the keys.
        /// </value>
        /// <returns>
        ///   A <see cref="System.Collections.Generic.Dictionary{TKey,TValue}.KeyCollection"/>
        ///   containing the keys in the <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"></see>.
        /// </returns>
        public System.Collections.Generic.ICollection<TKey> Keys
        {
            get { return source.Keys; }
        }

        /// <summary>
        ///   Gets a collection containing the values of the <see cref="T:ReadOnlyDictionary`2"/>.
        /// </summary>
        /// <value>The collection of values.</value>
        public System.Collections.Generic.ICollection<TValue> Values
        {
            get { return source.Values; }
        }

        /// <summary>
        ///   Gets a value indicating whether the dictionary is read-only. This value will always be true.
        /// </summary>
        bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        ///   Gets or sets the value associated with the specified key.
        /// </summary>
        /// <returns>
        ///   The value associated with the specified key. If the specified key is not found, a get
        ///   operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException"/>,
        ///   and a set operation creates a new element with the specified key.
        /// </returns>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown when the key is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        ///   The property is retrieved and key does not exist in the collection.
        /// </exception>
        public TValue this[TKey key]
        {
            get { return source[key]; }
            set { ThrowNotSupportedException(); }
        }

        /// <summary>
        ///   This method is not supported by the <see cref="T:ReadOnlyDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        void System.Collections.Generic.IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            ThrowNotSupportedException();
        }

        /// <summary>
        ///   Determines whether the <see cref="T:ReadOnlyDictionary`2"/> contains the specified key.
        /// </summary>
        /// <returns>
        ///   True if the <see cref="T:ReadOnlyDictionary`2"/> contains an element with the
        ///   specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:ReadOnlyDictionary`2"></see>.</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown when the key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return source.ContainsKey(key);
        }

        /// <summary>
        ///   This method is not supported by the <see cref="T:ReadOnlyDictionary`2"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>True if the element is successfully removed; otherwise, false.</returns>
        bool System.Collections.Generic.IDictionary<TKey, TValue>.Remove(TKey key)
        {
            ThrowNotSupportedException();
            return false;
        }

        /// <summary>
        ///   Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///   When this method returns, contains the value associated with the specified key, if the
        ///   key is found; otherwise, the default value for the type of the value parameter. This
        ///   parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///   <b>true</b> if the <see cref="T:ReadOnlyDictionary`2"/> contains an element with the
        ///   specified key; otherwise, <b>false</b>.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return source.TryGetValue(key, out value);
        }

        /// <summary>
        ///   This method is not supported by the <see cref="T:ReadOnlyDictionary`2"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:ICollection`1"/>.</param>
        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.Add(System.Collections.Generic.KeyValuePair<TKey, TValue> item)
        {
            ThrowNotSupportedException();
        }

        /// <summary>
        ///   This method is not supported by the <see cref="T:ReadOnlyDictionary`2"/>.
        /// </summary>
        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.Clear()
        {
            ThrowNotSupportedException();
        }

        /// <summary>
        ///   Determines whether the <see cref="T:ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:ICollection`1"/>.</param>
        /// <returns><b>true</b> if item is found in the <b>ICollection</b>; otherwise, <b>false</b>.</returns>
        bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.Contains(System.Collections.Generic.KeyValuePair<TKey, TValue> item)
        {
            System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>> collection = source;

            return collection.Contains(item);
        }

        /// <summary>
        ///   Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">
        ///   The one-dimensional Array that is the destination of the elements copied from
        ///   ICollection. The Array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.CopyTo(System.Collections.Generic.KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>> collection = source;
            collection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///   This method is not supported by the <see cref="T:ReadOnlyDictionary`2"/>.
        /// </summary>
        /// <param name="item">The object to remove from the ICollection.</param>
        /// <returns>Will never return a value.</returns>
        bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.Remove(System.Collections.Generic.KeyValuePair<TKey, TValue> item)
        {
            ThrowNotSupportedException();
            return false;
        }

        /// <summary>
        ///   Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
        System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>> enumerator = source;

            return enumerator.GetEnumerator();
        }

        /// <summary>
        ///   Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return source.GetEnumerator();
        }

        #endregion IDictionary<TKey,TValue> Members

        private static void ThrowNotSupportedException()
        {
            throw new System.NotSupportedException("This Dictionary is read-only");
        }
    }

    internal sealed class ReadOnlyDictionaryDebugView<TKey, TValue>
    {
        private readonly System.Collections.Generic.IDictionary<TKey, TValue> dict;

        public ReadOnlyDictionaryDebugView(
            ReadOnlyDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                throw new System.ArgumentNullException("dictionary");
            }

            dict = dictionary;
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public System.Collections.Generic.KeyValuePair<TKey, TValue>[] Items
        {
            get
            {
                var array =
                    new System.Collections.Generic.KeyValuePair<TKey, TValue>[dict.Count];
                dict.CopyTo(array, 0);
                return array;
            }
        }
    }
}