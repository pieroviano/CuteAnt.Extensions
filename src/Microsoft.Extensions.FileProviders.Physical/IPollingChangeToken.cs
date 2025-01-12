﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.FileProviders
{
    public interface IPollingChangeToken : IChangeToken
    {
        CancellationTokenSource CancellationTokenSource { get; }
    }
}
