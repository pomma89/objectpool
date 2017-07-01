// File name: EvictionTimer.cs
//
// Author(s): ULiiAn <yncxcxz@gmail.com>, Alessio Parma <alessio.parma@gmail.com>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#if !NET35

using CodeProject.ObjectPool.Logging;

#endif

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Default implementation of <see cref="IEvictionTimer"/>.
    /// </summary>
    public class EvictionTimer : IEvictionTimer, IDisposable
    {
#if !NET35
        private static readonly ILog Log = LogProvider.GetLogger(typeof(EvictionTimer));
#endif

        private readonly Dictionary<Action, Timer> _actionMap;
        private volatile bool _disposed;

        /// <summary>
        ///   Constructor for <see cref="EvictionTimer"/>.
        /// </summary>
        public EvictionTimer()
        {
            _actionMap = new Dictionary<Action, Timer>();
        }

        /// <summary>
        ///   Finalizer for <see cref="EvictionTimer"/>.
        /// </summary>
        ~EvictionTimer()
        {
            Dispose(false);
        }

        /// <summary>
        ///   Disposes the eviction timer, making it unusable.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Schedules an eviction action.
        /// </summary>
        /// <param name="action">Eviction action.</param>
        /// <param name="delay">Start delay.</param>
        /// <param name="period">Schedule period.</param>
        public void Schedule(Action action, TimeSpan delay, TimeSpan period)
        {
            ThrowIfDisposed();
            if (action == null)
            {
                return;
            }
            lock (typeof(EvictionTimer))
            {
#if!NET35
                Action piplineAction = () => Log.Debug("Begin Schedule Evictor");
                piplineAction += action;
                piplineAction += () => Log.Debug("End Schedule Evictor");
                action = piplineAction;
#endif
                if (_actionMap.TryGetValue(action, out Timer timer))
                {
                    timer.Change(delay, period);
                }
                else
                {
                    var t = new Timer(state => action(), null, delay, period);
                    _actionMap[action] = t;
                }
            }
        }

        /// <summary>
        ///   Cancels a scheduled task.
        /// </summary>
        /// <param name="task">Scheduled task.</param>
        public void Cancel(Action task)
        {
            ThrowIfDisposed();
            lock (typeof(EvictionTimer))
            {
                if (_actionMap.TryGetValue(task, out Timer timer))
                {
                    _actionMap.Remove(task);
                    timer.Dispose();
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>
        ///   Disposes all action timers.
        /// </summary>
        /// <param name="disposing">False if called by the finalizer, true otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Mark this object as completely disposed.
                _disposed = true;

                if (disposing && _actionMap != null)
                {
                    var timers = _actionMap.Values.ToArray() ?? Enumerable.Empty<Timer>();
                    _actionMap.Clear();
                    foreach (Timer t in timers)
                    {
                        t.Dispose();
                    }
                }
            }
        }
    }
}