// <copyright>
// Copyright 2020-2024 Max Ieremenko
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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

public static class ServiceContract
{
    private const string ServiceContractAttribute = "System.ServiceModel.ServiceContractAttribute";
    private const string OperationContractAttribute = "System.ServiceModel.OperationContractAttribute";
    private const string DataContractAttribute = "System.Runtime.Serialization.DataContractAttribute";

    public static bool IsServiceContractInterface(INamedTypeSymbol type) =>
        SyntaxTools.IsInterface(type)
        && !type.IsUnboundGenericType
        && GetServiceContractAttribute(type) != null;

    public static bool IsServiceOperation(IMethodSymbol method) => GetOperationContractAttribute(method) != null;

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

        var name = attribute.GetNamedArgumentValue("Name", (string?)null);
        return string.IsNullOrWhiteSpace(name) ? method.Name : name!;
    }

    private static AttributeData? GetServiceContractAttribute(ITypeSymbol type) =>
        SyntaxTools.GetCustomAttribute(type, ServiceContractAttribute);

    private static AttributeData? GetOperationContractAttribute(IMethodSymbol method) =>
        SyntaxTools.GetCustomAttribute(method, OperationContractAttribute);

    private static void GetServiceGenericEnding(ITypeSymbol serviceType, List<string> result)
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

        var name = attribute.GetNamedArgumentValue("Name", (string?)null);
        var @namespace = attribute.GetNamedArgumentValue("Namespace", (string?)null);

        return (serviceType.Name, @namespace, name);
    }

    private static string GetDataContainerName(ITypeSymbol type)
    {
        var attribute = SyntaxTools.GetCustomAttribute(type, DataContractAttribute);
        var attributeName = attribute?.GetNamedArgumentValue("Name", (string?)null);
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