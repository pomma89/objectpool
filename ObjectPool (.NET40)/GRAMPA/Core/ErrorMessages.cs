// File name: ErrorMessages.cs
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

namespace CodeProject.ObjectPool.Core
{
    /// <summary>
    ///   Error messages.
    /// </summary>
    /// <remarks>
    ///   As other classes inside GRAMPA, it needs to be partial to avoid collisions when it will be installed.
    /// </remarks>
    internal static partial class ErrorMessages
    {
        public const string TopLevel_Pair_NotFullCollection = "GPair is not (and cannot be) a full collection, therefore some IList methods are not implemented.";
        public const string TopLevel_Triple_NotFullCollection = "GTriple is not (and cannot be) a full collection, therefore some IList methods are not implemented.";
        public const string TopLevel_Tuple_NotFullCollection = "GTuple is not (and cannot be) a full collection, therefore some IList methods are not implemented.";

        public const string Collections_ReadOnlyList_Immutable = "List is immutable and it cannot be modified.";
        public const string Collections_ReadOnlyList_NullItems = "Given items cannot be null, or Nothing in VB.NET.";

        public const string Extensions_DataTableExtensions_NullDataTable = "Given data table cannot be null, or Nothing in VB.NET.";

        public const string Reflection_ServiceLocator_ErrorOnLoading = "An error occurred while loading specified type. See inner exception for more details.";
        public const string Reflection_ServiceLocator_InterfaceNotImplemented = "Specified type does not implement given interface.";
        public const string Reflection_ServiceLocator_NotAnInterface = "Specified type argument is not an interface.";

        public const string Threading_ConcurrentWorkQueue_NullAction = "Given action cannot be null, or Nothing in VB.NET.";

        public const string ContainedItem = "TODO";
        public const string EmptyList = "List is empty";
        public const string EmptyQueue = "Queue is empty";
        public const string EmptyStack = "Stack is empty";
        public const string NotContainedItem = "TODO";
        public const string NullList = "TODO";
    }
}
