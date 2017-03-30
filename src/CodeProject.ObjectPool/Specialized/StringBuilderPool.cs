// File name: StringBuilderPool.cs
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

using CodeProject.ObjectPool.Core;
using System.Text;

namespace CodeProject.ObjectPool.Specialized
{
    /// <summary>
    ///   An <see cref="IObjectPool{PooledStringBuilder}"/> ready to be used.
    ///   <see cref="StringBuilder"/> management can be further configured using the
    ///   <see cref="MinimumStringBuilderCapacity"/> and <see cref="MaximumStringBuilderCapacity"/> properties.
    /// </summary>
    public sealed class StringBuilderPool : ObjectPool<PooledStringBuilder>, IStringBuilderPool
    {
        /// <summary>
        ///   Default minimum string builder capacity. Shared by all <see cref="IStringBuilderPool"/>
        ///   instances, defaults to 4096 characters.
        /// </summary>
        public const int DefaultMinimumStringBuilderCapacity = 4 * 1024;

        /// <summary>
        ///   Default maximum string builder capacity. Shared by all <see cref="IStringBuilderPool"/>
        ///   instances, defaults to 524288 characters.
        /// </summary>
        public const int DefaultMaximumStringBuilderCapacity = 512 * 1024;

        /// <summary>
        ///   Thread-safe pool instance.
        /// </summary>
        public static IStringBuilderPool Instance { get; } = new StringBuilderPool();

        /// <summary>
        ///   Builds the specialized pool.
        /// </summary>
        public StringBuilderPool()
            : base(ObjectPool.DefaultPoolMaximumSize, null)
        {
            FactoryMethod = () => new PooledStringBuilder(MinimumStringBuilderCapacity);
        }

        /// <summary>
        ///   Backing field for <see cref="MinimumStringBuilderCapacity"/>.
        /// </summary>
        private int _minimumItemCapacity = DefaultMinimumStringBuilderCapacity;

        /// <summary>
        ///   Minimum capacity a <see cref="StringBuilder"/> should have when created and this is the
        ///   minimum capacity of all builders stored in the pool. Defaults to <see cref="DefaultMinimumStringBuilderCapacity"/>.
        /// </summary>
        public int MinimumStringBuilderCapacity
        {
            get { return _minimumItemCapacity; }
            set
            {
                var oldValue = _minimumItemCapacity;
                _minimumItemCapacity = value;
                if (oldValue < value)
                {
                    Clear();
                }
            }
        }

        /// <summary>
        ///   Backing field for <see cref="MaximumStringBuilderCapacity"/>.
        /// </summary>
        private int _maximumItemCapacity = DefaultMaximumStringBuilderCapacity;

        /// <summary>
        ///   Maximum capacity a <see cref="StringBuilder"/> might have in order to be able to return
        ///   to pool. Defaults to <see cref="DefaultMaximumStringBuilderCapacity"/>.
        /// </summary>
        public int MaximumStringBuilderCapacity
        {
            get { return _maximumItemCapacity; }
            set
            {
                var oldValue = _maximumItemCapacity;
                _maximumItemCapacity = value;
                if (oldValue > value)
                {
                    Clear();
                }
            }
        }

        /// <summary>
        ///   Returns a pooled string builder using given string as initial value.
        /// </summary>
        /// <param name="value">The string used to initialize the value of the instance.</param>
        /// <returns>A pooled string builder.</returns>
        public PooledStringBuilder GetObject(string value)
        {
            var psb = GetObject();
            psb.StringBuilder.Append(value);
            return psb;
        }
    }
}