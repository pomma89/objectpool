using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;

// General Information about an assembly is controlled through the following set of attributes.
// Change these attribute values to modify the information associated with an assembly.
[assembly: AssemblyTitle("CodeProject.ObjectPool")]
[assembly: AssemblyDescription("An implementation of a Generic Object Pool.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Ofir Makmal")]
[assembly: AssemblyProduct("Generic Object Pool")]
[assembly: AssemblyCopyright("Copyright © Ofir Makmal 2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Version information for an assembly consists of the following four values:
// 
// Major Version Minor Version Build Number Revision
// 
// You can specify all the values or you can default the Build and Revision Numbers by using the '*'
// as shown below: [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.3.1")]
[assembly: AssemblyFileVersion("1.3.1")]

// Common Language Specification (CLS) compliance generally refers to the claim that CLS rules and
// restrictions are being followed.
[assembly: CLSCompliant(true)]

// Specifies that an assembly cannot cause an elevation of privilege.
[assembly: SecurityTransparent]

// To allow simpler unit testing.
[assembly: InternalsVisibleTo("UnitTests")]