// File name: MemoryStreamPool.cs
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

using System.IO;

namespace CodeProject.ObjectPool.Specialized
{
    /// <summary>
    ///   An <see cref="IObjectPool{PooledMemoryStream}"/> ready to be used.
    ///   <see cref="MemoryStream"/> management can be further configured using the
    ///   <see cref="MinimumMemoryStreamCapacity"/> and <see cref="MaximumMemoryStreamCapacity"/> properties.
    /// </summary>
    public sealed class MemoryStreamPool : ObjectPool<PooledMemoryStream>, IMemoryStreamPool
    {
        /// <summary>
        ///   Default minimum memory stream capacity. Shared by all <see cref="IMemoryStreamPool"/>
        ///   instances, defaults to 4KB.
        /// </summary>
        public static int DefaultMinimumMemoryStreamCapacity { get; set; } = 4 * 1024;

        /// <summary>
        ///   Default maximum memory stream capacity. Shared by all <see cref="IMemoryStreamPool"/>
        ///   instances, defaults to 512KB.
        /// </summary>
        public static int DefaultMaximumMemoryStreamCapacity { get; set; } = 512 * 1024;

        /// <summary>
        ///   Thread-safe pool instance.
        /// </summary>
        public static IMemoryStreamPool Instance { get; } = new MemoryStreamPool();

        /// <summary>
        ///   Builds the specialized pool.
        /// </summary>
        public MemoryStreamPool()
        {
            FactoryMethod = () => new PooledMemoryStream(MinimumMemoryStreamCapacity);
        }

        /// <summary>
        ///   Minimum capacity a <see cref="MemoryStream"/> should have when created and this is the
        ///   minimum capacity of all streams stored in the pool. Defaults to <see cref="DefaultMinimumMemoryStreamCapacity"/>.
        /// </summary>
        public int MinimumMemoryStreamCapacity { get; set; } = DefaultMinimumMemoryStreamCapacity;

        /// <summary>
        ///   Maximum capacity a <see cref="MemoryStream"/> might have in order to be able to return
        ///   to pool. Defaults to <see cref="DefaultMaximumMemoryStreamCapacity"/>.
        /// </summary>
        public int MaximumMemoryStreamCapacity { get; set; } = DefaultMaximumMemoryStreamCapacity;

#pragma warning disable CC0022 // Should dispose object

        /// <summary>
        ///   Returns a pooled memory stream using given byte array as stream buffer. Once the object
        ///   is returned to the pool, the pool itself might take ownership of given buffer.
        /// </summary>
        /// <param name="buffer">The byte array that will be used as stream buffer.</param>
        /// <returns>A pooled memory stream.</returns>
        /// <remarks>
        ///   When you pass a buffer to this method, you lose the ownership of the buffer, since it
        ///   might be claimed by the pool itself.
        /// </remarks>
        public PooledMemoryStream GetObject(byte[] buffer) => new PooledMemoryStream(buffer)
        {
            Handle = this
        };

#pragma warning restore CC0022 // Should dispose object
    }
}