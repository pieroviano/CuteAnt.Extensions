﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
#if NET40 || NET35 || NET30 || NET20
using Microsoft.Extensions.Internal;
#endif

namespace Microsoft.AspNetCore.JsonPatch.Converters
{
    public class TypedJsonPatchDocumentConverter : JsonPatchDocumentConverter
    {
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            try
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

#if NET40 || NET35 || NET30 || NET20
                var genericType = objectType.GetTypeGenericArguments()[0];
#else
                var genericType = objectType.GetTypeInfo().GenericTypeArguments[0];
#endif

                // load jObject
                var jObject = JArray.Load(reader);

                // Create target object for Json => list of operations, typed to genericType
                var genericOperation = typeof(Operation<>);
                var concreteOperationType = genericOperation.MakeGenericType(genericType);

                var genericList = typeof(List<>);
                var concreteList = genericList.MakeGenericType(concreteOperationType);

                var targetOperations = Activator.CreateInstance(concreteList);

                //Create a new reader for this jObject, and set all properties to match the original reader.
                var jObjectReader = jObject.CreateReader();
                jObjectReader.Culture = reader.Culture;
                jObjectReader.DateParseHandling = reader.DateParseHandling;
                jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                jObjectReader.FloatParseHandling = reader.FloatParseHandling;

                // Populate the object properties
                serializer.Populate(jObjectReader, targetOperations);

                // container target: the typed JsonPatchDocument.
                var container = Activator.CreateInstance(objectType, targetOperations, new DefaultContractResolver());

                return container;
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException(Resources.InvalidJsonPatchDocument, ex);
            }
        }
    }
}