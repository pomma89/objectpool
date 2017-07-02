// File name: PooledObjectValidationContext.cs
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
    ///   Contains additional info which might be useful when performing an object validation step.
    /// </summary>
    public sealed class PooledObjectValidationContext
    {
        /// <summary>
        ///   The pooled object which has to be validated.
        /// </summary>
        public PooledObject PooledObject { get; private set; }

        /// <summary>
        ///   Info about the pooled object which has to be validated.
        /// </summary>
        public PooledObjectInfo PooledObjectInfo => PooledObject.PooledObjectInfo;

        /// <summary>
        ///   Whether an object is going out of the pool or into the pool.
        /// </summary>
        public PooledObjectDirection Direction { get; private set; }

        /// <summary>
        ///   Used when an object is returning to the pool.
        /// </summary>
        /// <param name="pooledObject">The pooled object which has to be validated.</param>
        internal static PooledObjectValidationContext Inbound(PooledObject pooledObject) => new PooledObjectValidationContext
        {
            PooledObject = pooledObject,
            Direction = PooledObjectDirection.Inbound
        };

        /// <summary>
        ///   Used when an object is going out of the pool.
        /// </summary>
        /// <param name="pooledObject">The pooled object which has to be validated.</param>
        internal static PooledObjectValidationContext Outbound(PooledObject pooledObject) => new PooledObjectValidationContext
        {
            PooledObject = pooledObject,
            Direction = PooledObjectDirection.Outbound
        };
    }
}