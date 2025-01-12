﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#if NET40 || NET35 || NET30 || NET20
using System;

namespace System.Collections.Immutable
{
    /// <summary>Indicates to Code Analysis that a method validates a particular parameter.</summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class ValidatedNotNullAttribute : Attribute
    {
    }
}
#endif