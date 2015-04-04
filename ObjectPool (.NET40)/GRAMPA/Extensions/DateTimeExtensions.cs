// DateTimeExtensions.cs
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

using System;
using System.Diagnostics.Contracts;

namespace CodeProject.ObjectPool.Extensions
{
    /// <summary>
    ///   Extension methods for <see cref="DateTime"/>.
    /// </summary>
    internal static class DateTimeExtensions
    {
        /// <summary>
        ///   The UNIX time start.
        /// </summary>
        public static readonly DateTime UnixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///   Gets the Unix time (https://en.wikipedia.org/wiki/Unix_time) associated with given date.
        /// </summary>
        /// <value>
        ///   The Unix time (https://en.wikipedia.org/wiki/Unix_time) associated with given date.
        /// </value>
        public static long ToUnixTime(this DateTime dateTime)
        {
            Contract.Requires<ArgumentOutOfRangeException>(dateTime.ToUniversalTime() >= UnixTimeStart);
            Contract.Ensures(Contract.Result<long>() >= 0);

            return (long) (dateTime.ToUniversalTime() - UnixTimeStart).TotalSeconds;
        }
    }
}
