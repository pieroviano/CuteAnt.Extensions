﻿#if !NET35 && !NET30 && !NET20
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class DynamicServiceProviderEngine : CompiledServiceProviderEngine
    {
        public DynamicServiceProviderEngine(IEnumerable<ServiceDescriptor> serviceDescriptors, IServiceProviderEngineCallback callback) : base(serviceDescriptors, callback)
        {
        }

        protected override Func<ServiceProviderEngineScope, object> RealizeService(IServiceCallSite callSite)
        {
            var callCount = 0;
            return scope =>
            {
                if (Interlocked.Increment(ref callCount) == 2)
                {
#if NET40 || NET35 || NET30 || NET20
                    ThreadPool.QueueUserWorkItem(state => base.RealizeService(callSite));
#else
                    Task.Run(() => base.RealizeService(callSite));
#endif
                }
                return RuntimeResolver.Resolve(callSite, scope);
            };
        }
    }
}

#endif