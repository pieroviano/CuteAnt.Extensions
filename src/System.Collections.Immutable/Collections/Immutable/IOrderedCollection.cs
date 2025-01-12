﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#if NET40 || NET35 || NET30 || NET20
using System.Collections.Generic;

namespace System.Collections.Immutable
{
    /// <summary>
    /// Describes an ordered collection of elements.
    /// </summary>
    /// <typeparam name="T">The type of element in the collection.</typeparam>
    internal interface IOrderedCollection<
#if !NET35 && !NET30 && !NET20
        out
#endif
        T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the element in the collection at a given index.
        /// </summary>
        T this[int index] { get; }
    }
}
#endif