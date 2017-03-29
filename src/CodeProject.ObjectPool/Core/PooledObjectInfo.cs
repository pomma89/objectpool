// File name: PooledObjectInfo.cs
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

namespace CodeProject.ObjectPool.Core
{
    /// <summary>
    ///   Core information about a specific <see cref="PooledObject"/>.
    /// </summary>
    public sealed class PooledObjectInfo
    {
        /// <summary>
        ///   An identifier which is unique inside the pool to which this object belongs. Moreover,
        ///   this identifier increases monotonically as new objects are created.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        ///   Enumeration that is being managed by the pool to describe the object state - primary
        ///   used to void cases where the resources are being releases twice.
        /// </summary>
        public PooledObjectState State { get; internal set; }

        /// <summary>
        ///   Internal action that is initialized by the pool while creating the object, this allows
        ///   that object to re-add itself back to the pool.
        /// </summary>
        internal IObjectPoolHandle Handle { get; set; }
    }
}