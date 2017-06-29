// Copyright (c) GZNB. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#if !NET35
using CodeProject.ObjectPool.Logging;
#endif


namespace CodeProject.ObjectPool
{
#if !NETSTD10
    public class EvictorTimer : IEvictionTimer, IDisposable
    {
#if !NET35
        private static readonly ILog Log = LogProvider.GetLogger(typeof(EvictorTimer));
#endif
        private Dictionary<Action, Timer> _actionMap;
        private volatile bool _disposed;

        public EvictorTimer()
        {
            this._actionMap = new Dictionary<Action, Timer>();
        }

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Add Eviction action
        /// </summary>
        /// <param name="action">eviction action</param>
        /// <param name="delay">delay time</param>
        /// <param name="period">period</param>
        public void Schedule(Action action, TimeSpan delay, TimeSpan period)
        {
            this.ThrowIfDisposed();
            if (action == null)
            {
                return;
            }
            lock (typeof(EvictorTimer))
            {
#if!NET35
                Action piplineAction = () => Log.Debug("Begin Schedule Evictor");
                piplineAction += action;
                piplineAction += () => Log.Debug("End Schedule Evictor");
                action = piplineAction;
#endif
                if (this._actionMap.TryGetValue(action, out Timer timer))
                {
                    timer.Change(delay, period);
                }
                else
                {
                    var t = new Timer(state => action(), null, delay, period);
                    this._actionMap[action] = t;
                }
            }
        }

        /// <summary>
        ///     Cancle Action
        /// </summary>
        /// <param name="task"></param>
        public void Cancel(Action task)
        {
            this.ThrowIfDisposed();
            lock (typeof(EvictorTimer))
            {
                if (this._actionMap.TryGetValue(task, out Timer timer))
                {
                    this._actionMap.Remove(task);
                    timer.Dispose();
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        ~EvictorTimer()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                this._disposed = true;
                if (disposing)
                {
                    Timer[] timers = this._actionMap?.Values.ToArray() ?? new Timer[0];
                    this._actionMap.Clear();
                    this._actionMap = null;
                    foreach (Timer t in timers)
                    {
                        t.Dispose();
                    }
                }
            }
        }
    }
#endif
}