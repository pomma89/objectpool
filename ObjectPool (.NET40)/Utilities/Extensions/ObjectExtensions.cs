// File name: ObjectExtensions.cs
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

#if !PORTABLE

using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

#endif

namespace CodeProject.ObjectPool.Utilities.Extensions
{
    /// <summary>
    ///   TODO
    /// </summary>
    internal static partial class ObjectExtensions
    {
        /// <summary>
        ///   Converts the specified object using the "as" operator.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>The specified object cast to <typeparamref name="TResult"/>.</returns>
        public static TResult As<TResult>(this object obj) where TResult : class
        {
            return obj as TResult;
        }

        /// <summary>
        ///   An empty string if <paramref name="obj"/> is null, otherwise it simply calls ToString.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   An empty string if <paramref name="obj"/> is null, otherwise it simply calls ToString.
        /// </returns>
        public static string SafeToString<T>(this T obj)
        {
            return ReferenceEquals(obj, null) ? Constants.EmptyString : obj.ToString();
        }

        #region JavaScript utilities

        /// <summary>
        ///   Converts given object into a valid JavaScript string.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>Given object converted into a valid JavaScript string.</returns>
        public static string ToJavaScriptString<T>(this T obj)
        {
            return ReferenceEquals(obj, null) ? null : obj.ToString().ToJavaScriptString();
        }

        #endregion JavaScript utilities
    }
}

#if !PORTABLE

namespace CodeProject.ObjectPool.Utilities.Extensions
{
    /// <summary>
    ///   TODO
    /// </summary>
    internal static partial class ObjectExtensions
    {
#region Hashing

        /// <summary>
        ///   TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToMD5Bytes<T>(this T obj)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 16);

            byte[] bytes;

            var maybeBytes = obj as byte[];
            if (maybeBytes != null)
            {
                bytes = maybeBytes;
                goto computeHash;
            }

            var maybeString = obj as string;
            if (maybeString != null)
            {
                bytes = Encoding.Default.GetBytes(maybeString);
                goto computeHash;
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    using (var jsonWriter = new JsonTextWriter(streamWriter))
                    {
                        var serializer = new JsonSerializer
                        {
                            Formatting = Formatting.None,
                            NullValueHandling = NullValueHandling.Ignore,
                        };
                        serializer.Serialize(jsonWriter, obj);
                    }
                }
                bytes = memoryStream.GetBuffer();
            }

        computeHash:
            return MD5.Create().ComputeHash(bytes);
        }

        /// <summary>
        ///   TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToMD5String<T>(this T obj)
        {
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(Contract.Result<string>().Length == 32);

            var hash = obj.ToMD5Bytes();
            var sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

#endregion Hashing
    }
}

#endif