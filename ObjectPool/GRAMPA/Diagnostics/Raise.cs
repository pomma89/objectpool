// Raise.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CodeProject.ObjectPool.Portability;

namespace CodeProject.ObjectPool.Diagnostics
{
    /// <summary>
    ///   Stores items shared by various <see cref="Raise{TEx}"/> instances.
    /// </summary>
    internal abstract class RaiseBase
    {
        /// <summary>
        ///   The define used to enable method compilation.
        /// </summary>
        public const string UseThrowerDefine = "USETHROWER";

        /// <summary>
        ///   Stores an empty array of <see cref="Type"/> used to seek constructors without parameters.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        protected static readonly Type[] NoCtorTypes = new Type[0];

        /// <summary>
        ///   Stores the types needed to seek the constructor which takes a string and an exception
        ///   as parameters to instance the exception.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        protected static readonly Type[] StrExCtorTypes = { typeof(string), typeof(Exception) };

        /// <summary>
        ///   Stores the type needed to seek the constructor which takes a string as parameter to
        ///   instance the exception.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        protected static readonly Type[] StrCtorType = { typeof(string) };

        /// <summary>
        ///   This class should not be extended by external classes: therefore, we have an empty
        ///   internal constructor.
        /// </summary>
        internal RaiseBase()
        {
            // It should not be extended by external classes.
        }
    }

    /// <summary>
    ///   Contains methods that throw specified exception <typeparamref name="TEx"/> if given
    ///   conditions will be verified. All methods in this class require that <see
    ///   cref="RaiseBase.UseThrowerDefine"/> is specified as conditional compilation symbol.
    /// </summary>
    /// <typeparam name="TEx">The type of the exceptions thrown if conditions will be satisfied.</typeparam>
    /// <remarks>
    ///   In order to achieve a good speed, the class caches an instance of the constructors found
    ///   via reflection; therefore, constructors are looked for only once.
    /// </remarks>
    internal sealed class Raise<TEx> : RaiseBase where TEx : Exception
    {
        /// <summary>
        ///   Stores wheter the exception type is abstract or not. We do this both to provide better
        ///   error messages for the end user and to avoid calling wrong constructors.
        /// </summary>
        private static readonly bool ExTypeIsAbstract = GTypeInfo.IsAbstract(typeof(TEx));

        /// <summary>
        ///   Caches an instance of the constructor which takes no arguments. If it does not exist,
        ///   then this field will be null. There must be an instance for each type associated with
        ///   <see cref="Raise{TEx}"/>.
        /// </summary>
        private static readonly ConstructorInfo NoArgsCtor = GetCtor(NoCtorTypes);

        /// <summary>
        ///   Caches an instance of the constructor which creates an exception with a message. If it
        ///   does not exist, then this field will be null. There must be an instance for each type
        ///   associated with <see cref="Raise{TEx}"/>.
        /// </summary>
        /// <remarks>
        ///   At first, we look for constructors which take a string and an inner exception, because
        ///   some standard exceptions (like ArgumentException or ArgumentNullException) have a
        ///   constructor which takes a string as a "parameter name", not as a message. If a
        ///   constructor with that signature is not found, then we look for a constructor with a
        ///   string as the only argument.
        /// </remarks>
        private static readonly ConstructorInfo MsgCtor = GetCtor(StrExCtorTypes) ?? GetCtor(StrCtorType);

        /// <summary>
        ///   Keeps the number of arguments required by the constructor who creates the exception
        ///   with a message.
        /// </summary>
        private static readonly int MsgArgCount = (MsgCtor == null) ? 0 : MsgCtor.GetParameters().Length;

        /// <summary>
        ///   <see cref="Raise{TEx}"/> must not be instanced.
        /// </summary>
        private Raise()
        {
            throw new InvalidOperationException("This class should not be instantiated");
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   condition is true.
        /// </summary>
        /// <param name="cond">The condition that determines whether an exception will be thrown.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="cond"/> is true, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void If(bool cond)
        {
            if (cond)
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified condition is true.
        /// </summary>
        /// <param name="cond">The condition that determines whether an exception will be thrown.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="cond"/> is true, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void If(bool cond, string message)
        {
            if (cond)
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   condition is false.
        /// </summary>
        /// <param name="cond">The condition that determines whether an exception will be thrown.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="cond"/> is false, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfNot(bool cond)
        {
            if (!cond)
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified condition is false.
        /// </summary>
        /// <param name="cond">The condition that determines whether an exception will be thrown.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="cond"/> is false, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfNot(bool cond, string message)
        {
            if (!cond)
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   arguments are equal.
        /// </summary>
        /// <param name="arg1">First argument to test for equality.</param>
        /// <param name="arg2">Second argument to test for equality.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If arguments are equal, then an exception of type <typeparamref name="TEx"/> will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have a constructor
        ///   which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfAreEqual<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
        {
            if (Equals(arg1, arg2))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified arguments are equal.
        /// </summary>
        /// <param name="arg1">First argument to test for equality.</param>
        /// <param name="arg2">Second argument to test for equality.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If arguments are equal, then an exception of type <typeparamref name="TEx"/>, with the
        ///   message specified by <paramref name="message"/>, will be thrown. <br/> In order to do
        ///   that, <typeparamref name="TEx"/> must have either a constructor which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> as arguments, or a constructor which
        ///   takes a <see cref="string"/> as only parameter. <br/> If both constructors are
        ///   available, then the one which takes a <see cref="string"/> and an <see
        ///   cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfAreEqual<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, string message)
        {
            if (Equals(arg1, arg2))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   arguments are not equal.
        /// </summary>
        /// <param name="arg1">First argument to test for equality.</param>
        /// <param name="arg2">Second argument to test for equality.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If arguments are not equal, then an exception of type <typeparamref name="TEx"/> will
        ///   be thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have a
        ///   constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfAreNotEqual<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
        {
            if (!Equals(arg1, arg2))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified arguments are not equal.
        /// </summary>
        /// <param name="arg1">First argument to test for equality.</param>
        /// <param name="arg2">Second argument to test for equality.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If arguments are not equal, then an exception of type <typeparamref name="TEx"/>, with
        ///   the message specified by <paramref name="message"/>, will be thrown. <br/> In order to
        ///   do that, <typeparamref name="TEx"/> must have either a constructor which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> as arguments, or a constructor which
        ///   takes a <see cref="string"/> as only parameter. <br/> If both constructors are
        ///   available, then the one which takes a <see cref="string"/> and an <see
        ///   cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfAreNotEqual<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, string message)
        {
            if (!Equals(arg1, arg2))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   arguments point to the same object.
        /// </summary>
        /// <param name="arg1">First argument to test for reference equality.</param>
        /// <param name="arg2">Second argument to test for reference equality.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If arguments point to the same object, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfAreSame<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
        {
            if (ReferenceEquals(arg1, arg2))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified arguments point to the same object.
        /// </summary>
        /// <param name="arg1">First argument to test for reference equality.</param>
        /// <param name="arg2">Second argument to test for reference equality.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If arguments point to the same object, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfAreSame<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, string message)
        {
            if (ReferenceEquals(arg1, arg2))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   arguments do not point to the same object.
        /// </summary>
        /// <param name="arg1">First argument to test for reference equality.</param>
        /// <param name="arg2">Second argument to test for reference equality.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If arguments do not point to the same object, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfAreNotSame<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
        {
            if (!ReferenceEquals(arg1, arg2))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified arguments do not point to the same object.
        /// </summary>
        /// <param name="arg1">First argument to test for reference equality.</param>
        /// <param name="arg2">Second argument to test for reference equality.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If arguments do not point to the same object, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfAreNotSame<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, string message)
        {
            if (!ReferenceEquals(arg1, arg2))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if an instance of
        ///   given type can be assigned to specified object.
        /// </summary>
        /// <param name="instance">The object to test.</param>
        /// <param name="type">The type whose instance must be assigned to given object.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If an instance of given type can be assigned to specified object, then an exception of
        ///   type <typeparamref name="TEx"/> will be thrown. <br/> In order to do that,
        ///   <typeparamref name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsAssignableFrom(object instance, Type type)
        {
            if (GTypeInfo.IsAssignableFrom(instance, type))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if an instance of given type can be assigned to specified object.
        /// </summary>
        /// <param name="instance">The object to test.</param>
        /// <param name="type">The type whose instance must be assigned to given object.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If an instance of given type can be assigned to specified object, then an exception of
        ///   type <typeparamref name="TEx"/>, with the message specified by <paramref
        ///   name="message"/>, will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have either a constructor which takes a <see cref="string"/> and an
        ///   <see cref="Exception"/> as arguments, or a constructor which takes a <see
        ///   cref="string"/> as only parameter. <br/> If both constructors are available, then the
        ///   one which takes a <see cref="string"/> and an <see cref="Exception"/> will be used to
        ///   throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsAssignableFrom(object instance, Type type, string message)
        {
            if (ReferenceEquals(instance, null) || GTypeInfo.IsAssignableFrom(instance, type))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if an instance of
        ///   given type can be assigned to specified object.
        /// </summary>
        /// <typeparam name="TType">The type whose instance must be assigned to given object.</typeparam>
        /// <param name="instance">The object to test.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If an instance of given type can be assigned to specified object, then an exception of
        ///   type <typeparamref name="TEx"/> will be thrown. <br/> In order to do that,
        ///   <typeparamref name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void IfIsAssignableFrom<TType>(object instance)
        {
            if (ReferenceEquals(instance, null) || GTypeInfo.IsAssignableFrom(instance, typeof(TType)))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if an instance of given type can be assigned to specified object.
        /// </summary>
        /// <typeparam name="TType">The type whose instance must be assigned to given object.</typeparam>
        /// <param name="instance">The object to test.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If an instance of given type can be assigned to specified object, then an exception of
        ///   type <typeparamref name="TEx"/>, with the message specified by <paramref
        ///   name="message"/>, will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have either a constructor which takes a <see cref="string"/> and an
        ///   <see cref="Exception"/> as arguments, or a constructor which takes a <see
        ///   cref="string"/> as only parameter. <br/> If both constructors are available, then the
        ///   one which takes a <see cref="string"/> and an <see cref="Exception"/> will be used to
        ///   throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void IfIsAssignableFrom<TType>(object instance, string message)
        {
            if (ReferenceEquals(instance, null) || GTypeInfo.IsAssignableFrom(instance, typeof(TType)))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if an instance of
        ///   given type cannot be assigned to specified object.
        /// </summary>
        /// <param name="instance">The object to test.</param>
        /// <param name="type">The type whose instance must not be assigned to given object.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If an instance of given type cannot be assigned to specified object, then an exception
        ///   of type <typeparamref name="TEx"/> will be thrown. <br/> In order to do that,
        ///   <typeparamref name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotAssignableFrom(object instance, Type type)
        {
            if (ReferenceEquals(instance, null) || !GTypeInfo.IsAssignableFrom(instance, type))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if an instance of given type cannot be assigned to
        ///   specified object.
        /// </summary>
        /// <param name="instance">The object to test.</param>
        /// <param name="type">The type whose instance must not be assigned to given object.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If an instance of given type cannot be assigned to specified object, then an exception
        ///   of type <typeparamref name="TEx"/>, with the message specified by <paramref
        ///   name="message"/>, will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have either a constructor which takes a <see cref="string"/> and an
        ///   <see cref="Exception"/> as arguments, or a constructor which takes a <see
        ///   cref="string"/> as only parameter. <br/> If both constructors are available, then the
        ///   one which takes a <see cref="string"/> and an <see cref="Exception"/> will be used to
        ///   throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotAssignableFrom(object instance, Type type, string message)
        {
            if (ReferenceEquals(instance, null) || !GTypeInfo.IsAssignableFrom(instance, type))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if an instance of
        ///   given type cannot be assigned to specified object.
        /// </summary>
        /// <typeparam name="TType">
        ///   The type whose instance must not be assigned to given object.
        /// </typeparam>
        /// <param name="instance">The object to test.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If an instance of given type cannot be assigned to specified object, then an exception
        ///   of type <typeparamref name="TEx"/> will be thrown. <br/> In order to do that,
        ///   <typeparamref name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void IfIsNotAssignableFrom<TType>(object instance)
        {
            if (!GTypeInfo.IsAssignableFrom(instance, typeof(TType)))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if an instance of given type cannot be assigned to
        ///   specified object.
        /// </summary>
        /// <typeparam name="TType">
        ///   The type whose instance must not be assigned to given object.
        /// </typeparam>
        /// <param name="instance">The object to test.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If an instance of given type cannot be assigned to specified object, then an exception
        ///   of type <typeparamref name="TEx"/>, with the message specified by <paramref
        ///   name="message"/>, will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have either a constructor which takes a <see cref="string"/> and an
        ///   <see cref="Exception"/> as arguments, or a constructor which takes a <see
        ///   cref="string"/> as only parameter. <br/> If both constructors are available, then the
        ///   one which takes a <see cref="string"/> and an <see cref="Exception"/> will be used to
        ///   throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void IfIsNotAssignableFrom<TType>(object instance, string message)
        {
            if (ReferenceEquals(instance, null) || !GTypeInfo.IsAssignableFrom(instance, typeof(TType)))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   argument is contained in given collection.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <param name="collection">The collection that must not contain given argument.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="argument"/> is contained, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsContainedIn(object argument, IList collection)
        {
            if (ReferenceEquals(collection, null) || collection.Contains(argument))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified argument is contained in given collection.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <param name="collection">The collection that must not contain given argument.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="argument"/> is contained, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsContainedIn(object argument, IList collection, string message)
        {
            if (ReferenceEquals(collection, null) || collection.Contains(argument))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   argument is not contained in given collection.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <param name="collection">The collection that must contain given argument.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="argument"/> is not contained, then an exception of type
        ///   <typeparamref name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotContainedIn(object argument, IList collection)
        {
            if (ReferenceEquals(collection, null) || !collection.Contains(argument))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified argument is not contained in given collection.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <param name="collection">The collection that must contain given argument.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="argument"/> is not contained, then an exception of type
        ///   <typeparamref name="TEx"/>, with the message specified by <paramref name="message"/>,
        ///   will be thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either
        ///   a constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotContainedIn(object argument, IList collection, string message)
        {
            if (ReferenceEquals(collection, null) || !collection.Contains(argument))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   argument is contained in given collection.
        /// </summary>
        /// <param name="arg">The argument to check.</param>
        /// <param name="collection">The collection that must not contain given argument.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is contained, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsContainedIn<TArg>(TArg arg, IEnumerable<TArg> collection)
        {
            if (ReferenceEquals(collection, null) || collection.Contains(arg))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified argument is contained in given collection.
        /// </summary>
        /// <param name="arg">The argument to check.</param>
        /// <param name="collection">The collection that must not contain given argument.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is contained, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsContainedIn<TArg>(TArg arg, IEnumerable<TArg> collection, string message)
        {
            if (ReferenceEquals(collection, null) || collection.Contains(arg))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   argument is not contained in given collection.
        /// </summary>
        /// <param name="arg">The argument to check.</param>
        /// <param name="collection">The collection that must contain given argument.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is not contained, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotContainedIn<TArg>(TArg arg, IEnumerable<TArg> collection)
        {
            if (ReferenceEquals(collection, null) || !collection.Contains(arg))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified argument is not contained in given collection.
        /// </summary>
        /// <param name="arg">The argument to check.</param>
        /// <param name="collection">The collection that must contain given argument.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is not contained, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotContainedIn<TArg>(TArg arg, IEnumerable<TArg> collection, string message)
        {
            if (ReferenceEquals(collection, null) || !collection.Contains(arg))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   argument is contained in given dictionary keys.
        /// </summary>
        /// <param name="arg">The argument to check.</param>
        /// <param name="dictionary">The dictionary that must not contain given argument.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is contained, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsContainedIn<TArg>(TArg arg, IDictionary dictionary)
        {
            if (ReferenceEquals(dictionary, null) || dictionary.Contains(arg))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified argument is contained in given dictionary keys.
        /// </summary>
        /// <param name="arg">The argument to check.</param>
        /// <param name="dictionary">The dictionary that must not contain given argument.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is contained, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsContainedIn<TArg>(TArg arg, IDictionary dictionary, string message)
        {
            if (ReferenceEquals(dictionary, null) || dictionary.Contains(arg))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   argument is not contained in given dictionary keys.
        /// </summary>
        /// <param name="arg">The argument to check.</param>
        /// <param name="dictionary">The dictionary that must contain given argument.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is not contained, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotContainedIn<TArg>(TArg arg, IDictionary dictionary)
        {
            if (ReferenceEquals(dictionary, null) || !dictionary.Contains(arg))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified argument is not contained in given
        ///   dictionary keys.
        /// </summary>
        /// <param name="arg">The argument to check.</param>
        /// <param name="dictionary">The dictionary that must contain given argument.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is not contained, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotContainedIn<TArg>(TArg arg, IDictionary dictionary, string message)
        {
            if (ReferenceEquals(dictionary, null) || !dictionary.Contains(arg))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   arguments are contained in given dictionary pairs.
        /// </summary>
        /// <param name="arg1">The key argument to check.</param>
        /// <param name="arg2">The value argument to check.</param>
        /// <param name="dictionary">The dictionary that must not contain given arguments.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg1"/> and <paramref name="arg2"/> are contained, then an
        ///   exception of type <typeparamref name="TEx"/> will be thrown. <br/> In order to do
        ///   that, <typeparamref name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsContainedIn<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, IDictionary<TArg1, TArg2> dictionary)
        {
            if (ReferenceEquals(dictionary, null) || dictionary.Contains(new KeyValuePair<TArg1, TArg2>(arg1, arg2)))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified arguments are contained in given dictionary pairs.
        /// </summary>
        /// <param name="arg1">The key argument to check.</param>
        /// <param name="arg2">The value argument to check.</param>
        /// <param name="dictionary">The dictionary that must not contain given argument.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg1"/> and <paramref name="arg2"/> are contained, then an
        ///   exception of type <typeparamref name="TEx"/>, with the message specified by <paramref
        ///   name="message"/>, will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have either a constructor which takes a <see cref="string"/> and an
        ///   <see cref="Exception"/> as arguments, or a constructor which takes a <see
        ///   cref="string"/> as only parameter. <br/> If both constructors are available, then the
        ///   one which takes a <see cref="string"/> and an <see cref="Exception"/> will be used to
        ///   throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsContainedIn<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, IDictionary<TArg1, TArg2> dictionary,
            string message)
        {
            if (ReferenceEquals(dictionary, null) || dictionary.Contains(new KeyValuePair<TArg1, TArg2>(arg1, arg2)))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   arguments are not contained in given dictionary pairs.
        /// </summary>
        /// <param name="arg1">The key argument to check.</param>
        /// <param name="arg2">The value argument to check.</param>
        /// <param name="dictionary">The dictionary that must contain given argument.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg1"/> and <paramref name="arg2"/> are not contained, then an
        ///   exception of type <typeparamref name="TEx"/> will be thrown. <br/> In order to do
        ///   that, <typeparamref name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotContainedIn<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, IDictionary<TArg1, TArg2> dictionary)
        {
            if (ReferenceEquals(dictionary, null) || !dictionary.Contains(new KeyValuePair<TArg1, TArg2>(arg1, arg2)))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified arguments are not contained in given
        ///   dictionary pairs.
        /// </summary>
        /// <param name="arg1">The key argument to check.</param>
        /// <param name="arg2">The value argument to check.</param>
        /// <param name="dictionary">The dictionary that must contain given argument.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg1"/> and <paramref name="arg2"/> are not contained, then an
        ///   exception of type <typeparamref name="TEx"/>, with the message specified by <paramref
        ///   name="message"/>, will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have either a constructor which takes a <see cref="string"/> and an
        ///   <see cref="Exception"/> as arguments, or a constructor which takes a <see
        ///   cref="string"/> as only parameter. <br/> If both constructors are available, then the
        ///   one which takes a <see cref="string"/> and an <see cref="Exception"/> will be used to
        ///   throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotContainedIn<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, IDictionary<TArg1, TArg2> dictionary,
            string message)
        {
            if (ReferenceEquals(dictionary, null) || !dictionary.Contains(new KeyValuePair<TArg1, TArg2>(arg1, arg2)))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified string
        ///   is is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="valueToCheck">The string to check for emptiness.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="valueToCheck"/> is empty, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsEmpty(string valueToCheck)
        {
            if (IsNullOrWhiteSpace(valueToCheck))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified string is is null, empty, or consists only
        ///   of white-space characters.
        /// </summary>
        /// <param name="valueToCheck">The string to check for emptiness.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="valueToCheck"/> is empty, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsEmpty(string valueToCheck, string message)
        {
            if (IsNullOrWhiteSpace(valueToCheck))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified string
        ///   is not null, empty, or does not consist only of white-space characters.
        /// </summary>
        /// <param name="valueToCheck">The string to check for emptiness.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="valueToCheck"/> is not empty, then an exception of type
        ///   <typeparamref name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotEmpty(string valueToCheck)
        {
            if (!IsNullOrWhiteSpace(valueToCheck))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified string is not null, empty, or does not
        ///   consist only of white-space characters.
        /// </summary>
        /// <param name="valueToCheck">The string to check for emptiness.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="valueToCheck"/> is not empty, then an exception of type
        ///   <typeparamref name="TEx"/>, with the message specified by <paramref name="message"/>,
        ///   will be thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either
        ///   a constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotEmpty(string valueToCheck, string message)
        {
            if (!IsNullOrWhiteSpace(valueToCheck))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   collection is null or empty.
        /// </summary>
        /// <param name="collection">The collection to check for emptiness.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="collection"/> is null or empty, then an exception of type
        ///   <typeparamref name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsEmpty(ICollection collection)
        {
            if (ReferenceEquals(collection, null) || collection.Count == 0)
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified collection is null or empty.
        /// </summary>
        /// <param name="collection">The collection to check for emptiness.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="collection"/> is null or empty, then an exception of type
        ///   <typeparamref name="TEx"/>, with the message specified by <paramref name="message"/>,
        ///   will be thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either
        ///   a constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsEmpty(ICollection collection, string message)
        {
            if (ReferenceEquals(collection, null) || collection.Count == 0)
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   collection is null or not empty.
        /// </summary>
        /// <param name="collection">The collection to check for emptiness.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="collection"/> is null or not empty, then an exception of type
        ///   <typeparamref name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotEmpty(ICollection collection)
        {
            if (ReferenceEquals(collection, null) || collection.Count > 0)
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified collection is null or not empty.
        /// </summary>
        /// <param name="collection">The collection to check for emptiness.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="collection"/> is null or not empty, then an exception of type
        ///   <typeparamref name="TEx"/>, with the message specified by <paramref name="message"/>,
        ///   will be thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either
        ///   a constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotEmpty(ICollection collection, string message)
        {
            if (ReferenceEquals(collection, null) || collection.Count > 0)
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   collection is null or empty.
        /// </summary>
        /// <param name="collection">The collection to check for emptiness.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="collection"/> is null or empty, then an exception of type
        ///   <typeparamref name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsEmpty<TArg>(IEnumerable<TArg> collection)
        {
            if (ReferenceEquals(collection, null) || !collection.Any())
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified collection is null or empty.
        /// </summary>
        /// <param name="collection">The collection to check for emptiness.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="collection"/> is null or empty, then an exception of type
        ///   <typeparamref name="TEx"/>, with the message specified by <paramref name="message"/>,
        ///   will be thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either
        ///   a constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsEmpty<TArg>(IEnumerable<TArg> collection, string message)
        {
            if (ReferenceEquals(collection, null) || !collection.Any())
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   collection is null or not empty.
        /// </summary>
        /// <param name="collection">The collection to check for emptiness.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="collection"/> is null or not empty, then an exception of type
        ///   <typeparamref name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotEmpty<TArg>(IEnumerable<TArg> collection)
        {
            if (ReferenceEquals(collection, null) || collection.Any())
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified collection is null or not empty.
        /// </summary>
        /// <param name="collection">The collection to check for emptiness.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="collection"/> is null or not empty, then an exception of type
        ///   <typeparamref name="TEx"/>, with the message specified by <paramref name="message"/>,
        ///   will be thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either
        ///   a constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotEmpty<TArg>(IEnumerable<TArg> collection, string message)
        {
            if (ReferenceEquals(collection, null) || collection.Any())
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified object
        ///   has given type.
        /// </summary>
        /// <param name="instance">The object to test.</param>
        /// <param name="type">The type the object must have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="instance"/> has given type, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsInstanceOf(object instance, Type type)
        {
            if (GTypeInfo.IsInstanceOf(instance, type))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified object has given type.
        /// </summary>
        /// <param name="instance">The object to test.</param>
        /// <param name="type">The type the object must have.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="instance"/> has given type, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsInstanceOf(object instance, Type type, string message)
        {
            if (GTypeInfo.IsInstanceOf(instance, type))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified object
        ///   has given type.
        /// </summary>
        /// <typeparam name="TType">The type the object must have.</typeparam>
        /// <param name="instance">The object to test.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="instance"/> has given type, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void IfIsInstanceOf<TType>(object instance)
        {
            if (instance is TType)
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified object has given type.
        /// </summary>
        /// <typeparam name="TType">The type the object must have.</typeparam>
        /// <param name="instance">The object to test.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="instance"/> has given type, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void IfIsInstanceOf<TType>(object instance, string message)
        {
            if (instance is TType)
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified object
        ///   has not given type.
        /// </summary>
        /// <param name="instance">The object to test.</param>
        /// <param name="type">The type the object must not have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="instance"/> has not given type, then an exception of type
        ///   <typeparamref name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotInstanceOf(object instance, Type type)
        {
            if (!GTypeInfo.IsInstanceOf(instance, type))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified object has not given type.
        /// </summary>
        /// <param name="instance">The object to test.</param>
        /// <param name="type">The type the object must not have.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="instance"/> has not given type, then an exception of type
        ///   <typeparamref name="TEx"/>, with the message specified by <paramref name="message"/>,
        ///   will be thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either
        ///   a constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotInstanceOf(object instance, Type type, string message)
        {
            if (!GTypeInfo.IsInstanceOf(instance, type))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified object
        ///   has not given type.
        /// </summary>
        /// <typeparam name="TType">The type the object must not have.</typeparam>
        /// <param name="instance">The object to test.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="instance"/> has not given type, then an exception of type
        ///   <typeparamref name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void IfIsNotInstanceOf<TType>(object instance)
        {
            if (!(instance is TType))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified object has not given type.
        /// </summary>
        /// <typeparam name="TType">The type the object must not have.</typeparam>
        /// <param name="instance">The object to test.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="instance"/> has not given type, then an exception of type
        ///   <typeparamref name="TEx"/>, with the message specified by <paramref name="message"/>,
        ///   will be thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either
        ///   a constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void IfIsNotInstanceOf<TType>(object instance, string message)
        {
            if (!(instance is TType))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified double
        ///   is <see cref="double.NaN"/>.
        /// </summary>
        /// <param name="number">The double to test for <see cref="double.NaN"/> equality.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="number"/> is <see cref="double.NaN"/>, then an exception of type
        ///   <typeparamref name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNaN(double number)
        {
            if (double.IsNaN(number))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified double is <see cref="double.NaN"/>.
        /// </summary>
        /// <param name="number">The double to test for <see cref="double.NaN"/> equality.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="number"/> is <see cref="double.NaN"/>, then an exception of type
        ///   <typeparamref name="TEx"/>, with the message specified by <paramref name="message"/>,
        ///   will be thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either
        ///   a constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNaN(double number, string message)
        {
            if (double.IsNaN(number))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified double
        ///   is not <see cref="double.NaN"/>.
        /// </summary>
        /// <param name="number">The double to test for <see cref="double.NaN"/> equality.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="number"/> is not <see cref="double.NaN"/>, then an exception of
        ///   type <typeparamref name="TEx"/> will be thrown. <br/> In order to do that,
        ///   <typeparamref name="TEx"/> must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotNaN(double number)
        {
            if (!double.IsNaN(number))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified double is not <see cref="double.NaN"/>.
        /// </summary>
        /// <param name="number">The double to test for <see cref="double.NaN"/> equality.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="number"/> is not <see cref="double.NaN"/>, then an exception of
        ///   type <typeparamref name="TEx"/>, with the message specified by <paramref
        ///   name="message"/>, will be thrown. <br/> In order to do that, <typeparamref
        ///   name="TEx"/> must have either a constructor which takes a <see cref="string"/> and an
        ///   <see cref="Exception"/> as arguments, or a constructor which takes a <see
        ///   cref="string"/> as only parameter. <br/> If both constructors are available, then the
        ///   one which takes a <see cref="string"/> and an <see cref="Exception"/> will be used to
        ///   throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotNaN(double number, string message)
        {
            if (!double.IsNaN(number))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   argument is null.
        /// </summary>
        /// <param name="arg">The argument to test for nullity.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is null, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNull<TArg>(TArg arg)
        {
            if (ReferenceEquals(arg, null))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified argument is null.
        /// </summary>
        /// <param name="arg">The argument to test for nullity.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is null, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNull<TArg>(TArg arg, string message)
        {
            if (ReferenceEquals(arg, null))
            {
                DoThrow(message);
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> if and only if specified
        ///   argument is not null.
        /// </summary>
        /// <param name="arg">The argument to test for nullity.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor with no
        ///   parameters, or <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is null, then an exception of type <typeparamref
        ///   name="TEx"/> will be thrown. <br/> In order to do that, <typeparamref name="TEx"/>
        ///   must have a constructor which doesn't take any arguments.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotNull<TArg>(TArg arg)
        {
            if (!ReferenceEquals(arg, null))
            {
                DoThrow();
            }
        }

        /// <summary>
        ///   Throws an exception of type <typeparamref name="TEx"/> with given message <paramref
        ///   name="message"/> if and only if specified argument is not null.
        /// </summary>
        /// <param name="arg">The argument to test for nullity.</param>
        /// <param name="message">The message the thrown exception will have.</param>
        /// <exception cref="ThrowerException">
        ///   <typeparamref name="TEx"/> has not a public or internal constructor which takes, as
        ///   parameters, either a <see cref="string"/> or a <see cref="string"/> and an <see
        ///   cref="Exception"/>. The same exception is thrown when <typeparamref name="TEx"/> is abstract.
        /// </exception>
        /// <remarks>
        ///   If <paramref name="arg"/> is not null, then an exception of type <typeparamref
        ///   name="TEx"/>, with the message specified by <paramref name="message"/>, will be
        ///   thrown. <br/> In order to do that, <typeparamref name="TEx"/> must have either a
        ///   constructor which takes a <see cref="string"/> and an <see cref="Exception"/> as
        ///   arguments, or a constructor which takes a <see cref="string"/> as only parameter.
        ///   <br/> If both constructors are available, then the one which takes a <see
        ///   cref="string"/> and an <see cref="Exception"/> will be used to throw the exception.
        /// </remarks>
        [Conditional(UseThrowerDefine)]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static void IfIsNotNull<TArg>(TArg arg, string message)
        {
            if (!ReferenceEquals(arg, null))
            {
                DoThrow(message);
            }
        }

        private static ConstructorInfo GetCtor(IList<Type> ctorTypes)
        {
            return (from c in GTypeInfo.GetConstructors(typeof(TEx))
                    let args = c.GetParameters()
                    let zipArgs = args.Zip(ctorTypes, (argType, ctorType) => new { argType, ctorType })
                    where args.Length == ctorTypes.Count &&
                          (c.IsPublic || c.IsAssembly) &&
                          zipArgs.All(t => ReferenceEquals(t.argType.ParameterType, t.ctorType))
                    select c).FirstOrDefault();
        }

        private static void DoThrow()
        {
            // Checks whether the proper constructor exists. If not, then we produce an internal exception.
            if (ExTypeIsAbstract)
            {
                throw ThrowerException.AbstractEx;
            }
            if (NoArgsCtor == null)
            {
                throw ThrowerException.MissingNoArgsCtor;
            }
            // A proper constrctor exists: therefore, we can throw the exception.
            throw (TEx) NoArgsCtor.Invoke(new object[0]);
        }

        private static void DoThrow(string message)
        {
            // Checks whether the proper constructor exists. If not, then we produce an internal exception.
            if (ExTypeIsAbstract)
            {
                throw ThrowerException.AbstractEx;
            }
            if (MsgCtor == null)
            {
                throw ExTypeIsAbstract ? ThrowerException.AbstractEx : ThrowerException.MissingMsgCtor;
            }
            // A proper constrctor exists: therefore, we can throw the exception.
            var messageArgs = new object[MsgArgCount];
            messageArgs[0] = message;
            throw (TEx) MsgCtor.Invoke(messageArgs);
        }

        private static bool IsNullOrWhiteSpace(string value)
        {
            return value == null || string.IsNullOrEmpty(value.Trim());
        }
    }

    /// <summary>
    ///   Exception thrown by <see cref="Raise{TEx}"/> when the type parameter passed to that class
    ///   has something invalid (missing constructors, etc).
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    internal sealed class ThrowerException : Exception
    {
        [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
        private ThrowerException(string message) : base(message) { }

        internal static ThrowerException AbstractEx
        {
            get { return new ThrowerException("Given exception type is abstract"); }
        }

        internal static ThrowerException MissingNoArgsCtor
        {
            get { return new ThrowerException("Given exception type has no parameterless constructor"); }
        }

        internal static ThrowerException MissingMsgCtor
        {
            get { return new ThrowerException("Given exception type has not a valid message constructor"); }
        }
    }
}
