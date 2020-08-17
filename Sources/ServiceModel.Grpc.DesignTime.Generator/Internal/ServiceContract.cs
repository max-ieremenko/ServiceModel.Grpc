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
using Microsoft.CodeAnalysis;

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

            if (string.IsNullOrWhiteSpace(name))
            {
                name = serviceType.Name;
            }

            if (string.IsNullOrWhiteSpace(@namespace))
            {
                return name!;
            }

            return @namespace + "." + name;
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

        private static AttributeData? GetServiceContractAttribute(INamedTypeSymbol type)
        {
            return SyntaxTools.GetCustomAttribute(type, "System.ServiceModel.ServiceContractAttribute");
        }

        private static AttributeData? GetOperationContractAttribute(IMethodSymbol method)
        {
            return SyntaxTools.GetCustomAttribute(method, "System.ServiceModel.OperationContractAttribute");
        }
    }
}
