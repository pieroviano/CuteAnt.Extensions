// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/* This file contains multiple namespace/class definitions used to test the namespace walking
 * capabilities of the namespace reader.
 * 
 * Note that type definitions inside of the namespaces (with the exception of the global namespace)
 * are basically generated by:
 * foreach (var part in @namespace.Name.Split('.')
 *     MakeClassNamed(part);
 * ...For easy verification.
 * 
 * There are no type forwarders in any of these namespaces except the Forwarder namespace.
 */

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(Forwarder.FwdType))]
[assembly: TypeForwardedTo(typeof(Forwarder.NoDefs.FwdType))]

class GlobalClassA
{ }

class GlobalClassB
{ }

namespace NSTests
{
    public class NSTests
    { }
}

namespace NSTests.WithNestedType
{
    public class NSTests
    { }

    public class WithNestedType
    {
        // This SHOULD NOT show up in enumerating a namespace's types.
        public class NestedType
        { }
    }
}

namespace NSTests.Nested
{
    public class NSTests
    { }

    public class Nested
    { }
}

namespace NSTests.Nested.AndAgain
{
    public class NSTests
    { }

    public class Nested
    { }

    public class AndAgain
    { }
}

namespace NSTests.Nested.Multiple
{
    public class NSTests
    { }

    public class Nested
    { }

    public class Multiple
    { }
}

// Varying case
namespace Nstests
{
    public class Nstests 
    { }
}

namespace Nstests.Nested
{
    public class Nstests
    { }

    public class Nested
    { }
}

namespace NSTests.nested
{
    public class NSTests
    { }

    public class nested
    { }
}

namespace SkipFirst.Namespace
{
    public class SkipFirst
    { }

    public class Namespace
    { }
}

namespace SkipFirst.AndSecond.Namespace
{
    public class SkipFirst
    { }

    public class AndSecond
    { }

    public class Namespace
    { }
}

namespace SkipFirstOnce.Namespace
{
    public class SkipFirstOnce
    { }

    public class Namespace
    { }
}

// Same suffix shared by different namespaces
namespace FxResources.Microsoft.CSharp
{
    public class FxResources
    { }

    public class Microsoft
    { }

    public class CSharp
    { }
}

namespace Microsoft.CSharp
{
    public class Microsoft
    { }

    public class CSharp
    { }
}

// EXCEPTION: Forwarder namespace has a type forwarder: Forwarder.FwdType. This can be found in NamespaceForwardedCS.cs.
namespace Forwarder
{
    public class Forwarder
    { }
}

// EXCEPTION: Forwarder.NoDefs namespace has a type forwarder: Forwarder.NoDefs.FwdType. This can be found in NamespaceForwardedCS.cs.
// EXCEPTION: Forwarder.NoDefs namespace has no type definitions.
namespace Forwarder.NoDefs
{
}
