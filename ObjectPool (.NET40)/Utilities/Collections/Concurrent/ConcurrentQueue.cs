// File name: ConcurrentQueue.cs
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

using Enumerable = System.Linq.Enumerable;

namespace CodeProject.ObjectPool.Utilities.Collections.Concurrent
{
    internal sealed class ConcurrentQueue<T> : System.Collections.Generic.ICollection<T>, ILinkedQueue<T>
    {
        #region Fields

        private readonly System.Collections.Generic.Queue<T> _queue;

        private readonly Threading.ConcurrentWorkQueue _workQueue = Threading.ConcurrentWorkQueue.Create();

        #endregion Fields

        #region Construction

        public ConcurrentQueue()
            : this(new System.Collections.Generic.Queue<T>())
        {
        }

        public ConcurrentQueue(System.Collections.Generic.IEnumerable<T> items)
            : this(new System.Collections.Generic.Queue<T>(items))
        {
        }

        public ConcurrentQueue(int capacity)
            : this(new System.Collections.Generic.Queue<T>(capacity))
        {
        }

        private ConcurrentQueue(System.Collections.Generic.Queue<T> queue)
        {
            _queue = queue;
        }

        #endregion Construction

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            System.Collections.Generic.List<T> copy;
            using (_workQueue.EnqueueRead())
            {
                copy = Enumerable.ToList(_queue);
            }
            return copy.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            Enqueue(item);
        }

        public void Clear()
        {
            using (_workQueue.EnqueueWrite())
            {
                _queue.Clear();
            }
        }

        public bool Contains(T item)
        {
            using (_workQueue.EnqueueRead())
            {
                return _queue.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            using (_workQueue.EnqueueRead())
            {
                _queue.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            throw new System.NotImplementedException();
        }

        public int Count
        {
            get { return _queue.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Enqueue(T item)
        {
            using (_workQueue.EnqueueWrite())
            {
                _queue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            using (_workQueue.EnqueueWrite())
            {
                return _queue.Dequeue();
            }
        }

        public T Peek()
        {
            using (_workQueue.EnqueueRead())
            {
                return _queue.Peek();
            }
        }

        public bool TryDequeue(out T item)
        {
            using (_workQueue.EnqueueWrite())
            {
                if (_queue.Count == 0)
                {
                    item = default(T);
                    return false;
                }
                item = _queue.Dequeue();
                return true;
            }
        }

        public bool TryPeek(out T item)
        {
            using (_workQueue.EnqueueRead())
            {
                if (_queue.Count == 0)
                {
                    item = default(T);
                    return false;
                }
                item = _queue.Peek();
                return true;
            }
        }
    }
}