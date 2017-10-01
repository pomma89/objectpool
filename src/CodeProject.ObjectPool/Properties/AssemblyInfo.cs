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
using System.Runtime.InteropServices;
using System.Security;

// Setting ComVisible to false makes the types in this assembly not visible to COM components. If you
// need to access a type in this assembly from COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Common Language Specification (CLS) compliance generally refers to the claim that CLS rules and
// restrictions are being followed.
[assembly: CLSCompliant(true)]

// Allows an assembly to be called by partially trusted code. Without this declaration, only fully
// trusted callers are able to use the assembly.
[assembly: AllowPartiallyTrustedCallers]