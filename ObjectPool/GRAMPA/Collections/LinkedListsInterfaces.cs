// File name: LinkedListsInterfaces.cs
// 
// Author(s): Alessio Parma <alessio.parma@gmail.com>
// 
// Copyright (c) 2013-2014 Alessio Parma <alessio.parma@gmail.com>
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CodeProject.ObjectPool.Collections.Contracts;

namespace CodeProject.ObjectPool.Collections
{
    /// <summary>
    ///   Represents a linked list which has a very low memory footprint, but it also exposes very
    ///   few operations.
    /// </summary>
    /// <typeparam name="T">The type of the items of the list.</typeparam>
    [ContractClass(typeof(ThinLinkedListContract<>))]
    internal interface IThinLinkedList<T> : ICollection<T>
    {
        /// <summary>
        ///   The equality comparer used to determine whether two items are equal.
        /// </summary>
        [Pure]
        IEqualityComparer<T> EqualityComparer { get; }

        /// <summary>
        ///   The first item of the list.
        /// </summary>
        /// <exception cref="InvalidOperationException">List is empty.</exception>
        [Pure]
        T First { get; }

        /// <summary>
        ///   Adds given item to the list, so that it is the first item of the list.
        /// </summary>
        /// <param name="item">The item to add to the list.</param>
        void AddFirst(T item);

        /// <summary>
        ///   Removes the first item of the list.
        /// </summary>
        /// <exception cref="InvalidOperationException">List is empty.</exception>
        T RemoveFirst();
    }

    /// <summary>
    ///   </summary>
    /// <typeparam name="T">The type of the items of the list.</typeparam>
    [ContractClass(typeof(LinkedListContract<>))]
    internal interface ILinkedList<T> : IThinLinkedList<T>
    {
        /// <summary>
        ///   The last item of the list.
        /// </summary>
        /// <exception cref="InvalidOperationException">List is empty.</exception>
        [Pure]
        T Last { get; }

        /// <summary>
        ///   Adds given item to the list, so that it is the last item of the list.
        /// </summary>
        /// <param name="item">The item to add to the list.</param>
        void AddLast(T item);

        /// <summary>
        ///   Appends given list to this list; after this operation, <paramref name="list"/> is
        ///   cleared, that is, it will result in an empty collection.
        /// </summary>
        /// <param name="list">The list to append to this list.</param>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
        /// <remarks>
        ///   If <paramref name="list"/> was created using <see cref="SinglyLinkedList{TItem}"/>,
        ///   then this operation is done in constant time.
        /// </remarks>
        void Append(ILinkedList<T> list);
    }

    /// <summary>
    ///   </summary>
    /// <typeparam name="T">The type of the items of the list.</typeparam>
    [ContractClass(typeof(DoublyLinkedListContract<>))]
    internal interface IDoublyLinkedList<T> : ILinkedList<T>
    {
        /// <summary>
        ///   Appends given list to this list; after this operation, <paramref name="list"/> is
        ///   cleared, that is, it will result in an empty collection.
        /// </summary>
        /// <param name="list">The list to append to this list.</param>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
        /// <remarks>
        ///   If <paramref name="list"/> was created using <see cref="DoublyLinkedList{TItem}"/>,
        ///   then this operation is done in constant time.
        /// </remarks>
        void Append(IDoublyLinkedList<T> list);

        /// <summary>
        ///   Returns an enumerator to iterate over this list in reversed order.
        /// </summary>
        /// <returns>An enumerator to iterate over this list in reversed order.</returns>
        [Pure]
        IEnumerator<T> GetReversedEnumerator();

        /// <summary>
        ///   Removes the last item of the list.
        /// </summary>
        /// <exception cref="InvalidOperationException">List is empty.</exception>
        T RemoveLast();

        /// <summary>
        ///   Reverses the contents of the list, that is, list will be ordered as the enumerator
        ///   returned by <see cref="GetReversedEnumerator"/> before calling this method.
        /// </summary>
        void Reverse();
    }

    /// <summary>
    ///   </summary>
    /// <typeparam name="T">The type of the items of the list.</typeparam>
    [ContractClass(typeof(HashLinkedListContract<>))]
    internal interface IHashLinkedList<T> : ICollection<T>
    {
        /// <summary>
        ///   The equality comparer used to determine whether two items are equal.
        /// </summary>
        [Pure]
        IEqualityComparer<T> EqualityComparer { get; }

        /// <summary>
        ///   The first item of the list.
        /// </summary>
        /// <exception cref="InvalidOperationException">List is empty.</exception>
        [Pure]
        T First { get; }

        /// <summary>
        ///   The last item of the list.
        /// </summary>
        /// <exception cref="InvalidOperationException">List is empty.</exception>
        [Pure]
        T Last { get; }

        /// <summary>
        ///   Adds given item after the item specified.
        /// </summary>
        /// <param name="after">The item after which we have to put the new item.</param>
        /// <param name="toAdd">The item to add to the list.</param>
        /// <exception cref="ArgumentException">
        ///   List does not contain item specified by <paramref name="after"/>, or <paramref
        ///   name="toAdd"/> is already contained in the list.
        /// </exception>
        void AddAfter(T after, T toAdd);

        /// <summary>
        ///   Adds given item before the item specified.
        /// </summary>
        /// <param name="before">The item before which we have to put the new item.</param>
        /// <param name="toAdd">The item to add to the list.</param>
        /// <exception cref="ArgumentException">
        ///   List does not contain item specified by <paramref name="before"/>, or <paramref
        ///   name="toAdd"/> is already contained in the list.
        /// </exception>
        void AddBefore(T before, T toAdd);

        /// <summary>
        ///   Adds given item to the list, so that it is the first item of the list.
        /// </summary>
        /// <param name="item">The item to add to the list.</param>
        /// <exception cref="ArgumentException">
        ///   <paramref name="item"/> is already contained in the list.
        /// </exception>
        void AddFirst(T item);

        /// <summary>
        ///   Adds given item to the list, so that it is the last item of the list.
        /// </summary>
        /// <param name="item">The item to add to the list.</param>
        /// <exception cref="ArgumentException">
        ///   <paramref name="item"/> is already contained in the list.
        /// </exception>
        void AddLast(T item);

        /// <summary>
        ///   Returns an enumerator to iterate over this list in reversed order.
        /// </summary>
        /// <returns>An enumerator to iterate over this list in reversed order.</returns>
        [Pure]
        IEnumerator<T> GetReversedEnumerator();

        /// <summary>
        ///   Removes the item after the item specified by <paramref name="after"/>.
        /// </summary>
        /// <param name="after">The item after which we have to apply the remove operation.</param>
        /// <exception cref="ArgumentException">
        ///   List does not contain item specified by <paramref name="after"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   <paramref name="after"/> is the last element of the list.
        /// </exception>
        T RemoveAfter(T after);

        /// <summary>
        ///   Removes the item before the item specified by <paramref name="before"/>.
        /// </summary>
        /// <param name="before">The item before which we have to apply the remove operation.</param>
        /// <exception cref="ArgumentException">
        ///   List does not contain item specified by <paramref name="before"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   <paramref name="before"/> is the first element of the list.
        /// </exception>
        T RemoveBefore(T before);

        /// <summary>
        ///   Removes the first item of the list.
        /// </summary>
        /// <exception cref="InvalidOperationException">List is empty.</exception>
        T RemoveFirst();

        /// <summary>
        ///   Removes the last item of the list.
        /// </summary>
        /// <exception cref="InvalidOperationException">List is empty.</exception>
        T RemoveLast();

        /// <summary>
        ///   Reverses the contents of the list, that is, list will be ordered as the enumerator
        ///   returned by <see cref="GetReversedEnumerator"/> before calling this method.
        /// </summary>
        void Reverse();
    }

    /// <summary>
    ///   </summary>
    /// <typeparam name="T">The type of the items of the queue.</typeparam>
    [ContractClass(typeof(LinkedQueueContract<>))]
    internal interface ILinkedQueue<T> : IEnumerable<T>
    {
        /// <summary>
        ///   The number of items contained in the queue.
        /// </summary>
        [Pure]
        int Count { get; }

        /// <summary>
        ///   Dequeues the first item.
        /// </summary>
        /// <exception cref="InvalidOperationException">Queue is empty.</exception>
        T Dequeue();

        /// <summary>
        ///   Enqueues given item.
        /// </summary>
        /// <param name="item">The item to add to queue.</param>
        void Enqueue(T item);

        /// <summary>
        ///   The first item of the queue.
        /// </summary>
        /// <exception cref="InvalidOperationException">Queue is empty.</exception>
        [Pure]
        T Peek();
    }

    /// <summary>
    ///   </summary>
    /// <typeparam name="T">The type of the items of the stack.</typeparam>
    [ContractClass(typeof(LinkedStackContract<>))]
    internal interface ILinkedStack<T> : IEnumerable<T>
    {
        /// <summary>
        ///   The number of items contained in the stack.
        /// </summary>
        [Pure]
        int Count { get; }

        /// <summary>
        ///   Pops the first item off the stack.
        /// </summary>
        /// <exception cref="InvalidOperationException">Stack is empty.</exception>
        T Pop();

        /// <summary>
        ///   Pushes given item onto the stack.
        /// </summary>
        /// <param name="item">The item to push onto the stack.</param>
        void Push(T item);

        /// <summary>
        ///   The first item of the stack.
        /// </summary>
        /// <exception cref="InvalidOperationException">Stack is empty.</exception>
        [Pure]
        T Top();
    }
}
