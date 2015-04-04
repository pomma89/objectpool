// File name: TypeInfo.cs
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
using System.Collections.Generic;
using System.Reflection;

namespace CodeProject.ObjectPool.Portability
{
    internal static class GTypeInfo
    {
        public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
        {
#if PORTABLE
            return type.GetTypeInfo().DeclaredConstructors;
#else
            return type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#endif
        }

        public static Type GetBaseType(Type type)
        {
#if PORTABLE
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

        public static IEnumerable<Type> GetInterfaces(Type type)
        {
#if PORTABLE
            return type.GetTypeInfo().ImplementedInterfaces;
#else
            return type.GetInterfaces();
#endif
        }

        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
#if PORTABLE
            return type.GetTypeInfo().DeclaredProperties;
#else
            return type.GetProperties();
#endif
        }

        public static bool IsAbstract(Type type)
        {
#if PORTABLE
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        public static bool IsAssignableFrom(object obj, Type type)
        {
            if (ReferenceEquals(obj, null) || ReferenceEquals(type, null))
            {
                return false;
            }

#if PORTABLE
            return obj.GetType().GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
#else
            return obj.GetType().IsAssignableFrom(type);
#endif
        }

        public static bool IsInstanceOf(object obj, Type type)
        {
            if (ReferenceEquals(obj, null) || ReferenceEquals(type, null))
            {
                return false;
            }

#if PORTABLE
            return type.GetTypeInfo().IsAssignableFrom(obj.GetType().GetTypeInfo());
#else
            return type.IsInstanceOfType(obj);
#endif
        }

        public static bool IsInterface(Type type)
        {
#if PORTABLE
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }
    }
}
