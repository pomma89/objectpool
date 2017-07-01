using System;
using System.Collections.Generic;
using System.Text;

namespace CodeProject.ObjectPool
{
    /// <summary>
    /// Eviction Timer Interface
    /// </summary>
    public interface IEvictionTimer
    {
        /// <summary>
        /// Add Eviction action
        /// </summary>
        /// <param name="action">eviction action</param>
        /// <param name="delay">delay time</param>
        /// <param name="period">period</param>
        void Schedule(Action action, TimeSpan delay, TimeSpan period);

        /// <summary>
        /// Cancle Action
        /// </summary>
        /// <param name="task"></param>
        void Cancel(Action task);
    }
}
