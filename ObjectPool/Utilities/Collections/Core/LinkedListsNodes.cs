// File name: LinkedListsNodes.cs
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

using ObjectExtensions = CodeProject.ObjectPool.Utilities.Extensions.ObjectExtensions;

namespace CodeProject.ObjectPool.Utilities.Collections.Core
{
    internal abstract class NodeBase<TN, TI> : FormattableObject where TN : NodeBase<TN, TI>
    {
        public readonly TI Item;
        public TN Next;

        internal NodeBase(TI item, TN next)
        {
            Item = item;
            Next = next;
        }
    }

    internal sealed class SinglyNode<T> : NodeBase<SinglyNode<T>, T>
    {
        internal SinglyNode(T item, SinglyNode<T> next)
            : base(item, next)
        {
        }

        protected override System.Collections.Generic.IEnumerable<GKeyValuePair<string, string>> GetFormattingMembers()
        {
            yield return GKeyValuePair.Create("Item", ObjectExtensions.SafeToString(Item));
            yield return GKeyValuePair.Create("Next", ObjectExtensions.SafeToString(Next.Item));
        }
    }

    internal sealed class DoublyNode<T> : NodeBase<DoublyNode<T>, T>
    {
        public DoublyNode<T> Prev;

        internal DoublyNode(T item, DoublyNode<T> next, DoublyNode<T> prev)
            : base(item, next)
        {
            Prev = prev;
        }

        protected override System.Collections.Generic.IEnumerable<GKeyValuePair<string, string>> GetFormattingMembers()
        {
            yield return GKeyValuePair.Create("Item", ObjectExtensions.SafeToString(Item));
            yield return GKeyValuePair.Create("Next", ObjectExtensions.SafeToString(Next.Item));
            yield return GKeyValuePair.Create("Prev", ObjectExtensions.SafeToString(Prev.Item));
        }
    }
}