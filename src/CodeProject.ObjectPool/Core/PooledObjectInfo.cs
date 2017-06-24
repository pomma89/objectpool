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

using System;

namespace CodeProject.ObjectPool.Core
{
    /// <summary>
    ///   Core information about a specific <see cref="PooledObject"/>.
    /// </summary>
    public sealed class PooledObjectInfo : IEquatable<PooledObjectInfo>
    {
        /// <summary>
        ///   An identifier which is unique inside the pool to which this object belongs. Moreover,
        ///   this identifier increases monotonically as new objects are created.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        ///   Payload which can be used to add custom information to a pooled object.
        /// </summary>
        public object Payload { get; set; }

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

        /// <summary>
        ///   Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{nameof(Id)}: {Id}, {nameof(Payload)}: {Payload}";

        /// <summary>
        ///   Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        ///   true if the current object is equal to the <paramref name="other"/> parameter;
        ///   otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(PooledObjectInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        /// <summary>
        ///   Determines whether the specified <see cref="object"/> is equal to the current <see cref="PooledObjectInfo"/>.
        /// </summary>
        /// <returns>
        ///   true if the specified <see cref="object"/> is equal to the current
        ///   <see cref="PooledObjectInfo"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="PooledObjectInfo"/>.</param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var info = obj as PooledObjectInfo;
            return info != null && Equals(info);
        }

        /// <summary>
        ///   Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="PooledObjectInfo"/>.</returns>
        public override int GetHashCode() => Id;

        /// <summary>
        ///   Compares to pooled objects info by <see cref="Id"/>.
        /// </summary>
        /// <param name="left">Left object.</param>
        /// <param name="right">Right object.</param>
        /// <returns>True if given pooled objects info are equal, false otherwise.</returns>
        public static bool operator ==(PooledObjectInfo left, PooledObjectInfo right) => Equals(left, right);

        /// <summary>
        ///   Compares to pooled objects info by <see cref="Id"/>.
        /// </summary>
        /// <param name="left">Left object.</param>
        /// <param name="right">Right object.</param>
        /// <returns>True if given pooled objects info are not equal, false otherwise.</returns>
        public static bool operator !=(PooledObjectInfo left, PooledObjectInfo right) => !Equals(left, right);
    }
}