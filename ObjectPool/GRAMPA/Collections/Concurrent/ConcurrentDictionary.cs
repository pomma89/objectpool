// File name: ConcurrentDictionary.cs
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeProject.ObjectPool.Extensions;
using CodeProject.ObjectPool.Threading;

namespace CodeProject.ObjectPool.Collections.Concurrent
{
    internal sealed class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Fields

        private readonly Dictionary<TKey, TValue> _dict;

        private readonly ConcurrentWorkQueue _workQueue = new ConcurrentWorkQueue();

        #endregion Fields

        #region Construction

        public ConcurrentDictionary()
            : this(new Dictionary<TKey, TValue>())
        {
        }

        public ConcurrentDictionary(IDictionary<TKey, TValue> source)
            : this(new Dictionary<TKey, TValue>(source))
        {
        }

        public ConcurrentDictionary(IEqualityComparer<TKey> equalityComparer)
            : this(new Dictionary<TKey, TValue>(equalityComparer))
        {
        }

        public ConcurrentDictionary(int capacity)
            : this(new Dictionary<TKey, TValue>(capacity))
        {
        }

        public ConcurrentDictionary(IDictionary<TKey, TValue> source, IEqualityComparer<TKey> equalityComparer)
            : this(new Dictionary<TKey, TValue>(source, equalityComparer))
        {
        }

        public ConcurrentDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
            : this(new Dictionary<TKey, TValue>(capacity, equalityComparer))
        {
        }

        private ConcurrentDictionary(Dictionary<TKey, TValue> dict)
        {
            _dict = dict;
        }

        #endregion Construction

        public void Add(TKey key, TValue value)
        {
            using (_workQueue.EnqueueWrite())
            {
                _dict.Add(key, value);
            }
        }

        public bool TryAdd(TKey key, TValue value, out TValue foundValue)
        {
            using (_workQueue.EnqueueWrite())
            {
                if (_dict.TryGetValue(key, out foundValue))
                {
                    return false;
                }
                foundValue = default(TValue);
                _dict.Add(key, value);
                return true;
            }
        }

        public bool ContainsKey(TKey key)
        {
            using (_workQueue.EnqueueRead())
            {
                return _dict.ContainsKey(key);
            }
        }

        public bool Remove(TKey key)
        {
            using (_workQueue.EnqueueWrite())
            {
                return _dict.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            using (_workQueue.EnqueueRead())
            {
                return _dict.TryGetValue(key, out value);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                using (_workQueue.EnqueueRead())
                {
                    return _dict[key];
                }
            }
            set
            {
                using (_workQueue.EnqueueWrite())
                {
                    _dict[key] = value;
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                using (_workQueue.EnqueueRead())
                {
                    return _dict.Keys;
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                using (_workQueue.EnqueueRead())
                {
                    return _dict.Values;
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            List<KeyValuePair<TKey, TValue>> copy;
            using (_workQueue.EnqueueRead())
            {
                copy = _dict.ToList();
            }
            return copy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            using (_workQueue.EnqueueWrite())
            {
                _dict.As<IDictionary<TKey, TValue>>().Add(item);
            }
        }

        public void Clear()
        {
            using (_workQueue.EnqueueWrite())
            {
                _dict.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            using (_workQueue.EnqueueRead())
            {
                return (_dict as IDictionary<TKey, TValue>).Contains(item);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            using (_workQueue.EnqueueRead())
            {
                (_dict as IDictionary<TKey, TValue>).CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            using (_workQueue.EnqueueWrite())
            {
                return (_dict as IDictionary<TKey, TValue>).Remove(item);
            }
        }

        public int Count
        {
            get { return _dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}
