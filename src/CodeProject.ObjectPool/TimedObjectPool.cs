// File name: TimedObjectPool.cs
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
using System;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   A pool where objects are automatically removed after a period of inactivity.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of the object that which will be managed by the pool. The pooled object have to be
    ///   a sub-class of PooledObject.
    /// </typeparam>
    public class TimedObjectPool<T> : ObjectPool<T>, ITimedObjectPool<T>
        where T : PooledObject
    {
        #region Fields

        /// <summary>
        ///   Backing field for <see cref="Timeout"/>.
        /// </summary>
        private TimeSpan _timeout;

        #endregion Fields

        #region C'tor and Initialization code

        /// <summary>
        ///   Initializes a new timed pool with default settings and specified timeout.
        /// </summary>
        /// <param name="timeout">The timeout of each pooled object.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="timeout"/> is less than or equal to <see cref="TimeSpan.Zero"/>.
        /// </exception>
        public TimedObjectPool(TimeSpan timeout)
            : this(ObjectPool.DefaultPoolMaximumSize, null, timeout)
        {
        }

        /// <summary>
        ///   Initializes a new timed pool with specified maximum pool size and timeout.
        /// </summary>
        /// <param name="maximumPoolSize">The maximum pool size limit.</param>
        /// <param name="timeout">The timeout of each pooled object.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="maximumPoolSize"/> is less than or equal to zero.
        ///   <paramref name="timeout"/> is less than or equal to <see cref="TimeSpan.Zero"/>.
        /// </exception>
        public TimedObjectPool(int maximumPoolSize, TimeSpan timeout)
            : this(maximumPoolSize, null, timeout)
        {
        }

        /// <summary>
        ///   Initializes a new timed pool with specified factory method and timeout.
        /// </summary>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        /// <param name="timeout">The timeout of each pooled object.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="timeout"/> is less than or equal to <see cref="TimeSpan.Zero"/>.
        /// </exception>
        public TimedObjectPool(Func<T> factoryMethod, TimeSpan timeout)
            : this(ObjectPool.DefaultPoolMaximumSize, factoryMethod, timeout)
        {
        }

        /// <summary>
        ///   Initializes a new timed pool with specified factory method, maximum size and timeout.
        /// </summary>
        /// <param name="maximumPoolSize">The maximum pool size limit.</param>
        /// <param name="factoryMethod">The factory method that will be used to create new objects.</param>
        /// <param name="timeout">The timeout of each pooled object.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="maximumPoolSize"/> is less than or equal to zero.
        ///   <paramref name="timeout"/> is less than or equal to <see cref="TimeSpan.Zero"/>.
        /// </exception>
        public TimedObjectPool(int maximumPoolSize, Func<T> factoryMethod, TimeSpan timeout)
            : base(maximumPoolSize, factoryMethod, new EvictionSettings { Enabled = true, Delay = timeout, Period = timeout }, null)
        {
            // Preconditions
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout), ErrorMessages.NegativeOrZeroTimeout);

            // Assigning properties.
            _timeout = timeout;
        }

        #endregion C'tor and Initialization code

        #region Public Properties

        /// <summary>
        ///   When pooled objects have not been used for a time greater than <see cref="Timeout"/>,
        ///   then they will be destroyed by a cleaning task.
        /// </summary>
        public TimeSpan Timeout
        {
            get => _timeout;
            set
            {
                StartEvictor(new EvictionSettings { Enabled = true, Delay = value, Period = value });
                _timeout = value;
            }
        }

        #endregion Public Properties

        #region Core Methods

        /// <summary>
        ///   Creates a new pooled object, initializing its info.
        /// </summary>
        /// <returns>A new pooled object.</returns>
        protected override T CreatePooledObject()
        {
            var pooledObject = base.CreatePooledObject();

            // Register an handler which records the time at which the object returned to the pool.
            pooledObject.OnResetState += () =>
            {
                pooledObject.PooledObjectInfo.Payload = DateTime.UtcNow;
            };

            // Register an handler which validates pooled objects timeout.
            pooledObject.OnValidateObject += (ctx) =>
            {
                // An item which have been last used before following threshold will be destroyed.
                var threshold = DateTime.UtcNow - _timeout;
                return !(ctx.PooledObjectInfo.Payload is DateTime lastUsage && lastUsage < threshold);
            };

            return pooledObject;
        }

        #endregion Core Methods
    }
}