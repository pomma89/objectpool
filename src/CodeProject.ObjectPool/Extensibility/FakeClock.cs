// File name: FakeClock.cs
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

using System;

namespace CodeProject.ObjectPool.Extensibility
{
    /// <summary>
    ///   A fake clock which can be used for unit testing.
    /// </summary>
    public sealed class FakeClock : IClock
    {
        /// <summary>
        ///   Fixed UTC date and time.
        /// </summary>
        private DateTime _utcNow;

        /// <summary>
        ///   Builds a fake clock and resets it to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public FakeClock()
        {
            Reset();
        }

        /// <summary>
        ///   Builds a fake clock and resets it to <paramref name="utcNow"/>.
        /// </summary>
        /// <param name="utcNow">UTC date and time.</param>
        public FakeClock(DateTime utcNow)
        {
            Reset(utcNow);
        }

        /// <summary>
        ///   Thread safe singleton.
        /// </summary>
        public static FakeClock Instance { get; } = new FakeClock();

        /// <summary>
        ///   Current UTC date and time.
        /// </summary>
        public DateTime UtcNow => _utcNow;

        /// <summary>
        ///   Move time forward by given interval.
        /// </summary>
        /// <param name="interval">The interval.</param>
        public void Advance(TimeSpan interval)
        {
            _utcNow = _utcNow.Add(interval);
        }

        /// <summary>
        ///   Resets clock time using given UTC date and time.
        /// </summary>
        /// <param name="utcNow">UTC date and time.</param>
        public void Reset(DateTime? utcNow = null)
        {
            _utcNow = utcNow?.ToUniversalTime() ?? DateTime.UtcNow;
        }
    }
}