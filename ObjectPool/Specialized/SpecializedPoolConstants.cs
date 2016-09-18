// File name: SpecializedPoolConstants.cs
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

namespace CodeProject.ObjectPool.Specialized
{
    /// <summary>
    ///   Constants for specialized Object Pools.
    /// </summary>
    public static class SpecializedPoolConstants
    {
        /// <summary>
        ///   Default minimum memory stream capacity. Shared by all <see cref="IMemoryStreamPool"/>
        ///   instances, defaults to 4KB.
        /// </summary>
        public const int DefaultMinimumMemoryStreamCapacity = 4 * 1024;

        /// <summary>
        ///   Default maximum memory stream capacity. Shared by all <see cref="IMemoryStreamPool"/>
        ///   instances, defaults to 512KB.
        /// </summary>
        public const int DefaultMaximumMemoryStreamCapacity = 512 * 1024;

        /// <summary>
        ///   Default minimum string builder capacity. Shared by all <see cref="IStringBuilderPool"/>
        ///   instances, defaults to 4096 characters.
        /// </summary>
        public const int DefaultMinimumStringBuilderCapacity = 4 * 1024;

        /// <summary>
        ///   Default maximum string builder capacity. Shared by all <see cref="IStringBuilderPool"/>
        ///   instances, defaults to 524288 characters.
        /// </summary>
        public const int DefaultMaximumStringBuilderCapacity = 512 * 1024;
    }
}
