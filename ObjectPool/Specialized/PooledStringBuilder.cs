// File name: PooledStringBuilder.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2013-2016 Alessio Parma <alessio.parma@gmail.com>
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

using CodeProject.ObjectPool.Core;
using System;
using System.Text;

namespace CodeProject.ObjectPool.Specialized
{
    /// <summary>
    ///   Pooled object prepared to work with <see cref="StringBuilder"/> instances.
    /// </summary>
    public class PooledStringBuilder : PooledObject
    {
        /// <summary>
        ///   The string builder.
        /// </summary>
        public StringBuilder StringBuilder { get; }

        /// <summary>
        ///   Builds a pooled string builder.
        /// </summary>
        /// <param name="capacity">The capacity of the string builder.</param>
        public PooledStringBuilder(int capacity)
        {
            StringBuilder = new StringBuilder(capacity);
        }

        /// <summary>
        ///   Unique identifier.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        ///   When the pooled object was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        /// <summary>
        ///   Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => StringBuilder.ToString();

        /// <summary>
        ///   Reset the object state to allow this object to be re-used by other parts of the application.
        /// </summary>
        protected override void OnResetState()
        {
            var stringBuilderPool = Handle as IStringBuilderPool;
            if (StringBuilder.Capacity > stringBuilderPool.MaximumStringBuilderCapacity)
            {
                throw new CannotResetStateException($"String builder capacity is {StringBuilder.Capacity}, while maximum allowed capacity is {stringBuilderPool.MaximumStringBuilderCapacity}");
            }

            ClearStringBuilder();
            base.OnResetState();
        }

        /// <summary>
        ///   Releases the object's resources.
        /// </summary>
        protected override void OnReleaseResources()
        {
            ClearStringBuilder();
            base.OnReleaseResources();
        }

        /// <summary>
        ///   Clears the <see cref="StringBuilder"/> property, using specific methods depending on
        ///   the framework for which ObjectPool has been compiled.
        /// </summary>
        protected void ClearStringBuilder()
        {
#if NET35
            StringBuilder.Remove(0, StringBuilder.Length);
#else
            StringBuilder.Clear();
#endif
        }
    }
}