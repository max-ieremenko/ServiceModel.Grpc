// <copyright>
// Copyright 2020 Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.DesignTime.Internal
{
    internal static class ServiceContract
    {
        public static bool IsServiceContractInterface(INamedTypeSymbol type)
        {
            return SyntaxTools.IsInterface(type)
                   && !type.IsUnboundGenericType
                   && GetServiceContractAttribute(type) != null;
        }

        public static bool IsServiceOperation(IMethodSymbol method)
        {
            return GetOperationContractAttribute(method) != null;
        }

        public static string GetServiceName(INamedTypeSymbol serviceType)
        {
            var (typeName, attributeNamespace, attributeName) = GetServiceNonGenericName(serviceType);
            var genericEnding = GetServiceGenericEnding(serviceType);

            return NamingContract.GetServiceName(typeName, attributeNamespace, attributeName, genericEnding);
        }

        public static IList<string> GetServiceGenericEnding(INamedTypeSymbol serviceType)
        {
            var result = new List<string>();
            GetServiceGenericEnding(serviceType, result);

            return result;
        }

        public static string GetServiceOperationName(IMethodSymbol method)
        {
            var attribute = GetOperationContractAttribute(method);
            if (attribute == null)
            {
                throw new ArgumentOutOfRangeException(nameof(method));
            }

            string? name = null;

            foreach (var pair in attribute.NamedArguments)
            {
                if ("Name".Equals(pair.Key, StringComparison.Ordinal))
                {
                    name = (string?)pair.Value.Value;
                    break;
                }
            }

            return string.IsNullOrWhiteSpace(name) ? method.Name : name!;
        }

        private static AttributeData? GetServiceContractAttribute(ITypeSymbol type)
        {
            return SyntaxTools.GetCustomAttribute(type, "System.ServiceModel.ServiceContractAttribute");
        }

        private static AttributeData? GetOperationContractAttribute(IMethodSymbol method)
        {
            return SyntaxTools.GetCustomAttribute(method, "System.ServiceModel.OperationContractAttribute");
        }

        private static void GetServiceGenericEnding(ITypeSymbol serviceType, IList<string> result)
        {
            var args = serviceType.GenericTypeArguments();
            for (var i = 0; i < args.Length; i++)
            {
                var type = args[i];

                if (type is IArrayTypeSymbol array)
                {
                    var (elementType, prefix) = ExpandArray(array);
                    type = elementType;
                    if (array.Rank > 1)
                    {
                        prefix += array.Rank.ToString(CultureInfo.InvariantCulture);
                    }

                    result.Add(prefix + GetDataContainerName(type));
                }
                else
                {
                    result.Add(GetDataContainerName(type));
                }

                GetServiceGenericEnding(type, result);
            }
        }

        private static (string TypeName, string? AttributeNamespace, string? AttributeName) GetServiceNonGenericName(ITypeSymbol serviceType)
        {
            var attribute = GetServiceContractAttribute(serviceType);
            if (attribute == null)
            {
                throw new ArgumentOutOfRangeException(nameof(serviceType));
            }

            string? name = null;
            string? @namespace = null;

            foreach (var pair in attribute.NamedArguments)
            {
                if ("Namespace".Equals(pair.Key, StringComparison.Ordinal))
                {
                    @namespace = (string?)pair.Value.Value;
                }
                else if ("Name".Equals(pair.Key, StringComparison.Ordinal))
                {
                    name = (string?)pair.Value.Value;
                }
            }

            return (serviceType.Name, @namespace, name);
        }

        private static string GetDataContainerName(ITypeSymbol type)
        {
            var attribute = SyntaxTools.GetCustomAttribute(type, "System.Runtime.Serialization.DataContractAttribute");
            string? attributeName = null;

            if (attribute != null)
            {
                for (var i = 0; i < attribute.NamedArguments.Length; i++)
                {
                    var pair = attribute.NamedArguments[i];
                    if ("Name".Equals(pair.Key))
                    {
                        attributeName = (string?)pair.Value.Value;
                        break;
                    }
                }
            }

            return string.IsNullOrWhiteSpace(attributeName) ? type.Name : attributeName!;
        }

        private static (ITypeSymbol ElementType, string Prefix) ExpandArray(IArrayTypeSymbol array)
        {
            var prefix = "Array";
            while (array.ElementType is IArrayTypeSymbol subArray)
            {
                prefix += "Array";
                array = subArray;
            }

            return (array.ElementType, prefix);
        }
    }
}
