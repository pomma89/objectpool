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

#if !NETSTD10

using CodeProject.ObjectPool.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Default implementation of <see cref="IEvictionTimer"/>.
    /// </summary>
    public sealed class EvictionTimer : IEvictionTimer, IDisposable
    {
        private static readonly ILog Log = LogProvider.GetLogger(typeof(EvictionTimer));

        private readonly Dictionary<Guid, Timer> _actionMap = new Dictionary<Guid, Timer>();
        private volatile bool _disposed;

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
        /// <returns>
        ///   A ticket which identifies the scheduled eviction action, it can be used to cancel the
        ///   scheduled action via <see cref="Cancel(Guid)"/> method.
        /// </returns>
        public Guid Schedule(Action action, TimeSpan delay, TimeSpan period)
        {
            ThrowIfDisposed();
            if (action == null)
            {
                return Guid.Empty;
            }
            lock (_actionMap)
            {
                TimerCallback timerCallback = _ =>
                {
                    Log.Debug("Begin scheduled evictor");
                    action();
                    Log.Debug("End scheduled evictor");
                };
                var actionTicket = Guid.NewGuid();
                _actionMap[actionTicket] = new Timer(timerCallback, null, delay, period);
                return actionTicket;
            }
        }

        /// <summary>
        ///   Cancels a scheduled evicton action using a ticket returned by <see cref="Schedule(Action, TimeSpan, TimeSpan)"/>.
        /// </summary>
        /// <param name="actionTicket">
        ///   An eviction action ticket, which has been returned by <see cref="Schedule(Action, TimeSpan, TimeSpan)"/>.
        /// </param>
        public void Cancel(Guid actionTicket)
        {
            ThrowIfDisposed();
            lock (_actionMap)
            {
                if (_actionMap.TryGetValue(actionTicket, out Timer timer))
                {
                    _actionMap.Remove(actionTicket);
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
        private void Dispose(bool disposing)
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

#else

using System;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Dummy implementation of <see cref="IEvictionTimer"/>, .NET Standard 1.0 does not implement
    ///   System.Timer class.
    /// </summary>
    public sealed class EvictionTimer : IEvictionTimer, IDisposable
    {
        /// <summary>
        ///   Performs application-defined tasks associated with freeing, releasing, or resetting
        ///   unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do nothing.
        }

        /// <summary>
        ///   Schedules an eviction action.
        /// </summary>
        /// <param name="action">Eviction action.</param>
        /// <param name="delay">Start delay.</param>
        /// <param name="period">Schedule period.</param>
        /// <returns>
        ///   A ticket which identifies the scheduled eviction action, it can be used to cancel the
        ///   scheduled action via <see cref="Cancel(Guid)"/> method.
        /// </returns>
        public Guid Schedule(Action action, TimeSpan delay, TimeSpan period) => Guid.NewGuid();

        /// <summary>
        ///   Cancels a scheduled evicton action using a ticket returned by <see cref="Schedule(Action,TimeSpan,TimeSpan)"/>.
        /// </summary>
        /// <param name="actionTicket">
        ///   An eviction action ticket, which has been returned by <see cref="Schedule(Action,TimeSpan,TimeSpan)"/>.
        /// </param>
        public void Cancel(Guid actionTicket)
        {
            // Do nothing.
        }
    }
}

#endif