/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

using PommaLabs.Thrower;
using System;
using System.Collections.Generic;

namespace CodeProject.ObjectPool.Core
{
    internal sealed class ResizableBuffer<T>
    {
        /// <summary>
        ///   An empty list.
        /// </summary>
        private static readonly IList<T> EmptyList = new T[0];

        /// <summary>
        ///   Used as global lock.
        /// </summary>
        private readonly object _token = new object();

        /// <summary>
        ///   Resizable buffer.
        /// </summary>
        private T[] _buffer;

        /// <summary>
        ///   Buffer size.
        /// </summary>
        private int _bufferSize;

        public ResizableBuffer(int maxLength)
        {
            _buffer = new T[maxLength];
            Resize(maxLength);
        }

        public int Count => _bufferSize;

        public IList<T> Clear(out int oldBufferSize)
        {
            lock (_token)
            {
                var oldBuffer = _buffer;
                oldBufferSize = _bufferSize;

                _buffer = new T[oldBuffer.Length];
                _bufferSize = 0;

                return oldBuffer;
            }
        }

        public IList<T> Resize(int maxLength)
        {
            Raise.ArgumentOutOfRangeException.IfIsLess(maxLength, 1, nameof(maxLength));

            if (_buffer.Length == maxLength)
            {
                return EmptyList;
            }

            lock (_token)
            {
                if (_buffer.Length < maxLength)
                {
                    Array.Resize(ref _buffer, maxLength);
                    return EmptyList;
                }

                T item;
                var lostItems = new List<T>();
                for (var i = maxLength; i < _buffer.Length && !Equals((item = _buffer[i]), default(T)); ++i)
                {
                    lostItems.Add(item);
                }

                Array.Resize(ref _buffer, maxLength);
                _bufferSize = maxLength;

                return lostItems;
            }
        }

        public bool TryPush(T item)
        {
            // Fast check to avoid unnecessary locking.
            if (_bufferSize == _buffer.Length)
            {
                return false;
            }

            // Proper check and push.
            lock (_token)
            {
                if (_bufferSize == _buffer.Length)
                {
                    return false;
                }

                _buffer[_bufferSize++] = item;
                return true;
            }
        }

        public bool TryPop(out T item)
        {
            // Fast check to avoid unnecessary locking.
            if (_bufferSize == 0)
            {
                item = default(T);
                return false;
            }

            // Proper check and push.
            lock (_token)
            {
                if (_bufferSize == 0)
                {
                    item = default(T);
                    return false;
                }

                item = _buffer[--_bufferSize];
                _buffer[_bufferSize] = default(T);
                return true;
            }
        }
    }
}
