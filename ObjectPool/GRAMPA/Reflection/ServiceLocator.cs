// File name: ServiceLocator.cs
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
using CodeProject.ObjectPool.Collections.Concurrent;
using CodeProject.ObjectPool.Core;
using CodeProject.ObjectPool.Diagnostics;
using CodeProject.ObjectPool.Extensions;
using CodeProject.ObjectPool.Portability;

namespace CodeProject.ObjectPool.Reflection
{
    /// <summary>
    ///   A quick and dirty service locator.
    /// </summary>
    internal static class ServiceLocator
    {
        private static readonly ConcurrentDictionary<GPair<string, Type>, Type> TypeCache = new ConcurrentDictionary<GPair<string, Type>, Type>();

        /// <summary>
        ///   Creates a new instance for specified type, which must implement given <typeparamref name="TInterface"/>.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name.</param>
        /// <returns>A new instance for specified type.</returns>
        /// <exception cref="System.ArgumentException">
        ///   Something bad happened while loading specified type, which may be related to given
        ///   type name.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   Specified type argument is not an interface or specified type name is a class without
        ///   an empty constructor.
        /// </exception>
        public static TInterface Load<TInterface>(string fullyQualifiedTypeName) where TInterface : class
        {
            Raise<ArgumentException>.IfIsEmpty(fullyQualifiedTypeName, ErrorMessages.Reflection_ServiceLocator_ErrorOnLoading);

            // If type cache contains an entry for given type name, then we create a new instance
            // and we return it.
            var @interface = typeof(TInterface);
            var cacheKey = GPair.Create(fullyQualifiedTypeName, @interface);
            Type type;
            if (TypeCache.TryGetValue(cacheKey, out type))
            {
                return CreateInstance<TInterface>(type);
            }

            // We make sure that TInterface is really an interface.
            Raise<InvalidOperationException>.IfNot(GTypeInfo.IsInterface(@interface), ErrorMessages.Reflection_ServiceLocator_NotAnInterface);

            // We load the type and we make sure it implements TInterface.
            try
            {
                type = Type.GetType(fullyQualifiedTypeName, true);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ErrorMessages.Reflection_ServiceLocator_ErrorOnLoading, "fullyQualifiedTypeName", ex);
            }

            // The type we just found must implement given interface.
            Raise<ArgumentException>.IfIsNotContainedIn(@interface, GTypeInfo.GetInterfaces(type), ErrorMessages.Reflection_ServiceLocator_InterfaceNotImplemented);

            // We store the type inside the cache, so that future calls will be faster.
            TypeCache.Add(cacheKey, type);

            // At last, we create an instance and we return it to the caller.
            return CreateInstance<TInterface>(type);
        }

        private static TInterface CreateInstance<TInterface>(Type type) where TInterface : class
        {
            try
            {
                // For now, Activator.CreateInstance is more than enough for us. Please see:
                // * http://ayende.com/blog/3167/creating-objects-perf-implications
                // * http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/
                // * http://vagif.bloggingabout.net/2010/04/02/dont-use-activator-createinstance-or-constructorinfo-invoke-use-compiled-lambda-expressions/
                return Activator.CreateInstance(type).As<TInterface>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ErrorMessages.Reflection_ServiceLocator_ErrorOnLoading, ex);
            }
        }
    }
}
