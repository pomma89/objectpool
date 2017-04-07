// File name: TimedObjectPool.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2013-2018 Alessio Parma <alessio.parma@gmail.com>
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
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if !(NETSTD10 || NETSTD11)

using System;
using System.Linq;
using System.Threading;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   A pool where objects are automatically removed after a period of inactivity.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of the object that which will be managed by the pool. The pooled object have to be
    ///   a sub-class of PooledObject.
    /// </typeparam>
    internal class TimedObjectPool<T> : ObjectPool<T>, ITimedObjectPool<T>
        where T : PooledObject
    {
        private TimeSpan _timeout;
        private Timer _timer;

        public TimedObjectPool()
        {
        }

        /// <summary>
        ///   When pooled objects have not been used for a time greater than <see cref="Timeout"/>,
        ///   then they will be destroyed by a cleaning task.
        /// </summary>
        public TimeSpan Timeout
        {
            get => _timeout;
            set
            {
                _timeout = value;
                UpdateTimeout();
            }
        }

        /// <summary>
        ///   Updates the timer according to a new timeout.
        /// </summary>
        private void UpdateTimeout()
        {
            lock (this)
            {
                if (_timer != null)
                {
                    // A timer already exists, simply change its period.
                    _timer.Change(_timeout, _timeout);
                    return;
                }

                _timer = new Timer(_ =>
                {
                    // Local copy, since the buffer might change.
                    var items = PooledObjects.ToArray();

                    // All items which have been last used before following threshold will be destroyed.
                    var threshold = DateTime.UtcNow - _timeout;

                    foreach (var item in items)
                    {
                        if (item.PooledObjectInfo.Payload is DateTime lastUsage && lastUsage < threshold && PooledObjects.TryRemove(item))
                        {
                            DestroyPooledObject(item);
                        }
                    }
                }, null, _timeout, _timeout);
            }
        }
    }
}

#endif