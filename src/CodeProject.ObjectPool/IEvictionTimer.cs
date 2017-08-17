// File name: IEvictionTimer.cs
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

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   Eviction timer interface, used to abstract over eviction jobs.
    /// </summary>
    public interface IEvictionTimer : IDisposable
    {
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
        Guid Schedule(Action action, TimeSpan delay, TimeSpan period);

        /// <summary>
        ///   Cancels a scheduled evicton action using a ticket returned by <see cref="Schedule(Action, TimeSpan, TimeSpan)"/>.
        /// </summary>
        /// <param name="actionTicket">
        ///   An eviction action ticket, which has been returned by <see cref="Schedule(Action, TimeSpan, TimeSpan)"/>.
        /// </param>
        void Cancel(Guid actionTicket);
    }
}