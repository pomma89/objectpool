/*
 * Generic Object Pool Implementation
 *
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 *
 */

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following set of attributes.
// Change these attribute values to modify the information associated with an assembly.
[assembly: AssemblyTitle("CodeProject.ObjectPool")]
[assembly: AssemblyDescription("A generic, concurrent, portable and flexible Object Pool for the .NET Framework.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Ofir Makmal")]
[assembly: AssemblyProduct("CodeProject.ObjectPool")]
[assembly: AssemblyCopyright("Copyright © Ofir Makmal 2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible to COM components. If
// you need to access a type in this assembly from COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
// 
// Major Version Minor Version Build Number Revision
// 
// You can specify all the values or you can default the Build and Revision Numbers by using the '*'
// as shown below: [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2.0.0")]
[assembly: AssemblyFileVersion("2.0.5")]

// Common Language Specification (CLS) compliance generally refers to the claim that CLS rules and
// restrictions are being followed.
[assembly: CLSCompliant(true)]

// To allow simpler unit testing.
[assembly: InternalsVisibleTo("CodeProject.ObjectPool.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100efe22a8787b348e219d6b501b425d79f31502681d5a9f7d11030852e44e7ef2b29ddbc7dfcd6461fc3e67a6d7a186dea40535dad461679209a079a45440a99440bd292498b623e9a7fc5161c519c8d45b79fdfbd95a6e0ac5e211d5438d47b8635662d108448cb84bf1212983fab0010c68b41d5168bcbe5a59f9149974852c4")]
