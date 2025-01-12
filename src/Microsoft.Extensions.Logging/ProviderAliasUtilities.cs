﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
#if NET40 || NET35 || NET30 || NET20
using Microsoft.Extensions.Internal;
#endif

namespace Microsoft.Extensions.Logging
{
    internal class ProviderAliasUtilities
    {
        private const string AliasAttibuteTypeFullName = "Microsoft.Extensions.Logging.ProviderAliasAttribute";
        private const string AliasAttibuteAliasProperty = "Alias";

        internal static string GetAlias(Type providerType)
        {
            foreach (var attribute in providerType
#if !NET40 && !NET35 && !NET30 && !NET20 && !NET30 && !NET20
                .GetTypeInfo()
#endif
                .GetCustomAttributes(inherit: false))
            {
                if (attribute.GetType().FullName == AliasAttibuteTypeFullName)
                {
                    var valueProperty = attribute
                        .GetType()
                        .GetProperty(AliasAttibuteAliasProperty, BindingFlags.Public | BindingFlags.Instance);

                    if (valueProperty != null)
                    {
                        return valueProperty.GetValue(attribute, new object[0]) as string;
                    }
                }
            }

            return null;
        }
    }
}