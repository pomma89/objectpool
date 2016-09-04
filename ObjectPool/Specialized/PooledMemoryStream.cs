// File name: PooledMemoryStream.cs
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
using System.IO;

namespace CodeProject.ObjectPool.Specialized
{
    /// <summary>
    ///   Pooled object prepared to work with <see cref="System.IO.MemoryStream"/> instances.
    /// </summary>
    public class PooledMemoryStream : PooledObject
    {
        /// <summary>
        ///   The pool to which this object belongs.
        /// </summary>
        private readonly IMemoryStreamPool _pool;

        /// <summary>
        ///   The memory stream.
        /// </summary>
        private readonly TrackedMemoryStream _trackedMemoryStream;

        /// <summary>
        ///   Builds a pooled memory stream.
        /// </summary>
        /// <param name="pool">The pool to which this object belongs.</param>
        public PooledMemoryStream(IMemoryStreamPool pool)
            : this(pool, new TrackedMemoryStream(pool.MinimumMemoryStreamCapacity))
        {
        }

        /// <summary>
        ///   Builds a pooled memory stream using given buffer.
        /// </summary>
        /// <param name="pool">The pool to which this object belongs.</param>
        /// <param name="buffer">The buffer.</param>
        public PooledMemoryStream(IMemoryStreamPool pool, byte[] buffer)
            : this(pool, new TrackedMemoryStream(buffer))
        {
        }

        /// <summary>
        ///   Builds a pooled memory stream using given stream.
        /// </summary>
        /// <param name="pool">The pool to which this object belongs.</param>
        /// <param name="trackedMemoryStream">The backing stream.</param>
        private PooledMemoryStream(IMemoryStreamPool pool, TrackedMemoryStream trackedMemoryStream)
        {
            _pool = pool;
            _trackedMemoryStream = trackedMemoryStream;
            _trackedMemoryStream.Parent = this;
        }

        /// <summary>
        ///   The memory stream.
        /// </summary>
        public MemoryStream MemoryStream => _trackedMemoryStream;

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
        public override string ToString() => _trackedMemoryStream.ToString();

        /// <summary>
        ///   Reset the object state to allow this object to be re-used by other parts of the application.
        /// </summary>
        protected override void OnResetState()
        {
            if (!_trackedMemoryStream.CanRead || !_trackedMemoryStream.CanWrite || !_trackedMemoryStream.CanSeek)
            {
                throw new CannotResetStateException($"Memory stream has already been disposed");
            }
            if (_trackedMemoryStream.Capacity < _pool.MinimumMemoryStreamCapacity)
            {
                throw new CannotResetStateException($"Memory stream capacity is {_trackedMemoryStream.Capacity}, while minimum required capacity is {_pool.MinimumMemoryStreamCapacity}");
            }
            if (_trackedMemoryStream.Capacity > _pool.MaximumMemoryStreamCapacity)
            {
                throw new CannotResetStateException($"Memory stream capacity is {_trackedMemoryStream.Capacity}, while maximum allowed capacity is {_pool.MaximumMemoryStreamCapacity}");
            }
            _trackedMemoryStream.Position = 0L;
            _trackedMemoryStream.SetLength(0L);
            base.OnResetState();
        }

        /// <summary>
        ///   Releases the object's resources.
        /// </summary>
        protected override void OnReleaseResources()
        {
            _trackedMemoryStream.Parent = null;
            _trackedMemoryStream.Dispose();
            base.OnReleaseResources();
        }

        private sealed class TrackedMemoryStream : MemoryStream
        {
            public TrackedMemoryStream(int capacity)
                : base(capacity)
            {
            }

            public TrackedMemoryStream(byte[] buffer)
                : base(buffer)
            {
            }

            public PooledMemoryStream Parent { get; set; }

            protected override void Dispose(bool disposing)
            {
                if (disposing && Parent != null)
                {
                    Parent.Dispose();
                }
                else
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}