// File name: ConcurrentWorkQueue.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace CodeProject.ObjectPool.Utilities.Threading
{
	/// <summary>
	///   A class whose goal is to help make thread safe those data structures which are not.<br/>
	///   By using two simple methods (<see cref="EnqueueReadAction"/> or <see cref="EnqueueWriteAction"/>, for example),
	///   you can make all read operations concurrent, while write ones are executed in a blocking fashion.<br/>
	///   As a matter of fact, this class is just a wrapper placed upon a <see cref="ReaderWriterLockSlim"/>.
	/// </summary>
	internal sealed class ConcurrentWorkQueue : IDisposable
	{
		private readonly ReaderWriterLockSlim _workQueue = new ReaderWriterLockSlim();

		private ConcurrentWorkQueue()
		{
			// Nothing, for now.
		}

		/// <summary>
		///   Creates an instance of the <see cref="ConcurrentWorkQueue"/> class.
		/// </summary>
		/// <returns>An instance of the <see cref="ConcurrentWorkQueue"/> class.</returns>
		/// <remarks>Instance is not cached.</remarks>
		public static ConcurrentWorkQueue Create()
		{
			return new ConcurrentWorkQueue();
		}

        #region Raw Operations

        /// <summary>
        ///   Starts an user controlled read operation.
        /// </summary>
        /// <returns>An object whose disposal means the end of the operation.</returns>
        public IDisposable EnqueueRead()
        {
            return new OperationHandler(_workQueue, true);
        }

        /// <summary>
        ///   Starts an user controlled write operation.
        /// </summary>
        /// <returns>An object whose disposal means the end of the operation.</returns>
        public IDisposable EnqueueWrite()
        {
            return new OperationHandler(_workQueue, false);
        }

        private sealed class OperationHandler : IDisposable
        {
            private readonly ReaderWriterLockSlim _workQueue;
            private readonly bool _isRead;
            private bool _disposed;

            public OperationHandler(ReaderWriterLockSlim workQueue, bool isRead)
            {
                _workQueue = workQueue;
                _isRead = isRead;

                if (isRead)
                {
                    workQueue.EnterReadLock();
                }
                else
                {
                    workQueue.EnterWriteLock();
                }
            }

            ~OperationHandler()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing && _workQueue != null)
                {
                    if (_isRead)
                    {
                        _workQueue.ExitReadLock();
                    }
                    else
                    {
                        _workQueue.ExitWriteLock();
                    }
                }

                _disposed = true;
            }
        }

        #endregion

		#region Read Actions

        /// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
		public void EnqueueReadAction(Action action)
		{
			StartRead(action);
			try
			{
				action();
			}
			finally
			{
				EndRead();
			}
		}

		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		public void EnqueueReadAction<T1>(Action<T1> action, T1 a1)
        {
            StartRead(action);
            try
            {
                action(a1);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		public void EnqueueReadAction<T1, T2>(Action<T1, T2> action, T1 a1, T2 a2)
        {
            StartRead(action);
            try
            {
                action(a1, a2);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		public void EnqueueReadAction<T1, T2, T3>(Action<T1, T2, T3> action, T1 a1, T2 a2, T3 a3)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		public void EnqueueReadAction<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		public void EnqueueReadAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
        {
            StartRead(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16);
            }
            finally
            {
                EndRead();
            }
        }

		#endregion

		#region Async Read Actions

        /// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
		public Task EnqueueReadActionAsync(Action action)
		{
			return Task.Factory.StartNew(() => EnqueueReadAction(action));
		}

        /// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync(Action action, CancellationToken cancToken)
		{
			return Task.Factory.StartNew(() => EnqueueReadAction(action), cancToken);
		}

		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		public Task EnqueueReadActionAsync<T1>(Action<T1> action, T1 a1)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1>(Action<T1> action, T1 a1, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		public Task EnqueueReadActionAsync<T1, T2>(Action<T1, T2> action, T1 a1, T2 a2)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2>(Action<T1, T2> action, T1 a1, T2 a2, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3>(Action<T1, T2, T3> action, T1 a1, T2 a2, T3 a3)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3>(Action<T1, T2, T3> action, T1 a1, T2 a2, T3 a3, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 a1, T2 a2, T3 a3, T4 a4, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15), cancToken);
        }
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16));
        }
	
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="action">The read action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueReadActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16), cancToken);
        }

		#endregion

		#region Read Functions

        /// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
		public TRet EnqueueReadFunc<TRet>(Func<TRet> func)
		{
			StartRead(func);
			try
			{
				return func();
			}
			finally
			{
				EndRead();
			}
		}

		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		public TRet EnqueueReadFunc<T1, TRet>(Func<T1, TRet> func, T1 a1)
        {
            StartRead(func);
            try
            {
                return func(a1);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		public TRet EnqueueReadFunc<T1, T2, TRet>(Func<T1, T2, TRet> func, T1 a1, T2 a2)
        {
            StartRead(func);
            try
            {
                return func(a1, a2);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> func, T1 a1, T2 a2, T3 a3)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, TRet>(Func<T1, T2, T3, T4, T5, T6, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, T7, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
            }
            finally
            {
                EndRead();
            }
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		public TRet EnqueueReadFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
        {
            StartRead(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16);
            }
            finally
            {
                EndRead();
            }
        }

		#endregion

		#region Async Read Functions

        /// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
		public Task<TRet> EnqueueReadFuncAsync<TRet>(Func<TRet> func)
		{
			return Task.Factory.StartNew(() => EnqueueReadFunc(func));
		}

        /// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<TRet>(Func<TRet> func, CancellationToken cancToken)
		{
			return Task.Factory.StartNew(() => EnqueueReadFunc(func), cancToken);
		}

		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, TRet>(Func<T1, TRet> func, T1 a1)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, TRet>(Func<T1, TRet> func, T1 a1, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, TRet>(Func<T1, T2, TRet> func, T1 a1, T2 a2)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, TRet>(Func<T1, T2, TRet> func, T1 a1, T2 a2, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> func, T1 a1, T2 a2, T3 a3)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> func, T1 a1, T2 a2, T3 a3, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, TRet>(Func<T1, T2, T3, T4, T5, T6, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, TRet>(Func<T1, T2, T3, T4, T5, T6, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15), cancToken);
        }
		
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16));
        }
				
		/// <summary>
		///   Enqueues a read operation. If there are no write operations in the queue,
		///   it will be executed immediately, in parallel with other read operations.<br/>
		///   Otherwise, this operation will be blocked, until the write operation has been processed.
		/// </summary>
		/// <param name="func">The read function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueReadFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueReadFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16), cancToken);
        }

		#endregion

		#region Write Actions

        /// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
		public void EnqueueWriteAction(Action action)
		{
			StartWrite(action);
			try
			{
				action();
			}
			finally
			{
				EndWrite();
			}
        }

	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		public void EnqueueWriteAction<T1>(Action<T1> action, T1 a1)
        {
            StartWrite(action);
            try
            {
                action(a1);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		public void EnqueueWriteAction<T1, T2>(Action<T1, T2> action, T1 a1, T2 a2)
        {
            StartWrite(action);
            try
            {
                action(a1, a2);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		public void EnqueueWriteAction<T1, T2, T3>(Action<T1, T2, T3> action, T1 a1, T2 a2, T3 a3)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		public void EnqueueWriteAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
        {
            StartWrite(action);
            try
            {
                action(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16);
            }
            finally
            {
                EndWrite();
            }
        }

		#endregion

		#region Async Write Actions

        /// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
		public Task EnqueueWriteActionAsync(Action action)
		{
			return Task.Factory.StartNew(() => EnqueueWriteAction(action));
		}

        /// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync(Action action, CancellationToken cancToken)
		{
			return Task.Factory.StartNew(() => EnqueueWriteAction(action), cancToken);
		}

		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		public Task EnqueueWriteActionAsync<T1>(Action<T1> action, T1 a1)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1>(Action<T1> action, T1 a1, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		public Task EnqueueWriteActionAsync<T1, T2>(Action<T1, T2> action, T1 a1, T2 a2)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2>(Action<T1, T2> action, T1 a1, T2 a2, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3>(Action<T1, T2, T3> action, T1 a1, T2 a2, T3 a3)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3>(Action<T1, T2, T3> action, T1 a1, T2 a2, T3 a3, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 a1, T2 a2, T3 a3, T4 a4, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15), cancToken);
        }
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16));
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="action">The write action that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task EnqueueWriteActionAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteAction(action, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16), cancToken);
        }

		#endregion

		#region Write Functions

        /// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
		public TRet EnqueueWriteFunc<TRet>(Func<TRet> func)
		{
			StartWrite(func);
			try
			{
				return func();
			}
			finally
			{
				EndWrite();
			}
        }

	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		public TRet EnqueueWriteFunc<T1, TRet>(Func<T1, TRet> func, T1 a1)
        {
            StartWrite(func);
            try
            {
                return func(a1);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		public TRet EnqueueWriteFunc<T1, T2, TRet>(Func<T1, T2, TRet> func, T1 a1, T2 a2)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> func, T1 a1, T2 a2, T3 a3)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, TRet>(Func<T1, T2, T3, T4, T5, T6, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, T7, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
            }
            finally
            {
                EndWrite();
            }
        }
	
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		public TRet EnqueueWriteFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
        {
            StartWrite(func);
            try
            {
                return func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16);
            }
            finally
            {
                EndWrite();
            }
        }

		#endregion

		#region Async Write Functions

        /// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
		public Task<TRet> EnqueueWriteFuncAsync<TRet>(Func<TRet> func)
		{
			return Task.Factory.StartNew(() => EnqueueWriteFunc(func));
		}

        /// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<TRet>(Func<TRet> func, CancellationToken cancToken)
		{
			return Task.Factory.StartNew(() => EnqueueWriteFunc(func), cancToken);
		}

		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, TRet>(Func<T1, TRet> func, T1 a1)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, TRet>(Func<T1, TRet> func, T1 a1, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, TRet>(Func<T1, T2, TRet> func, T1 a1, T2 a2)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, TRet>(Func<T1, T2, TRet> func, T1 a1, T2 a2, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> func, T1 a1, T2 a2, T3 a3)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> func, T1 a1, T2 a2, T3 a3, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, TRet>(Func<T1, T2, T3, T4, T5, T6, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, TRet>(Func<T1, T2, T3, T4, T5, T6, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15), cancToken);
        }
		
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16));
        }
				
		/// <summary>
		///   Enqueues a write operation. Write operations are never executed in parallel,
		///   both with other write operations and with other read operations.
		/// </summary>
		/// <param name="func">The write function that must be executed.</param>
        /// <param name="a1">Parameter 1.</param>
        /// <param name="a2">Parameter 2.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <param name="a5">Parameter 5.</param>
        /// <param name="a6">Parameter 6.</param>
        /// <param name="a7">Parameter 7.</param>
        /// <param name="a8">Parameter 8.</param>
        /// <param name="a9">Parameter 9.</param>
        /// <param name="a10">Parameter 10.</param>
        /// <param name="a11">Parameter 11.</param>
        /// <param name="a12">Parameter 12.</param>
        /// <param name="a13">Parameter 13.</param>
        /// <param name="a14">Parameter 14.</param>
        /// <param name="a15">Parameter 15.</param>
        /// <param name="a16">Parameter 16.</param>
		/// <param name="cancToken">The cancellation token.</param>
		public Task<TRet> EnqueueWriteFuncAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet> func, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16, CancellationToken cancToken)
        {
            return Task.Factory.StartNew(() => EnqueueWriteFunc(func, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16), cancToken);
        }

		#endregion

		#region IDisposable Members

        /// <summary>
		///   Disposes the underlying instance of <see cref="ReaderWriterLockSlim"/>.
		/// </summary>
		public void Dispose()
		{
		    if (_workQueue != null)
		    {
                _workQueue.Dispose();		        
		    }
			GC.SuppressFinalize(this);
		}

		#endregion

        #region Private Members

	    private void StartRead<TAction>(TAction action) where TAction : class
	    {
            if (ReferenceEquals(action, null))
            {
                throw new ArgumentNullException(CodeProject.ObjectPool.Utilities.Core.ErrorMessages.Threading_ConcurrentWorkQueue_NullAction);
            }
            _workQueue.EnterReadLock();
	    }

	    private void EndRead()
	    {
            _workQueue.ExitReadLock();
	    }

	    private void StartWrite<TAction>(TAction action) where TAction : class
	    {
            if (ReferenceEquals(action, null))
            {
                throw new ArgumentNullException(CodeProject.ObjectPool.Utilities.Core.ErrorMessages.Threading_ConcurrentWorkQueue_NullAction);
            }
            _workQueue.EnterWriteLock();
	    }

	    private void EndWrite()
	    {
            _workQueue.ExitWriteLock();
	    }

        #endregion
    }
}
