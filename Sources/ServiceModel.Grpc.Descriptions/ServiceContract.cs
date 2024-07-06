// <copyright>
// Copyright Max Ieremenko
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.Descriptions;

internal static class ServiceContract
{
    public const string ServiceContractAttribute = "System.ServiceModel.ServiceContractAttribute";
    public const string OperationContractAttribute = "System.ServiceModel.OperationContractAttribute";
    public const string DataContractAttribute = "System.Runtime.Serialization.DataContractAttribute";

    public static bool TryGetServiceName<TType>(this IReflect<TType> reflect, TType serviceType, [NotNullWhen(true)] out string? name)
    {
        if (!reflect.IsPublicNonGenericInterface(serviceType)
            || !reflect.TryGetCustomAttribute(serviceType, ServiceContractAttribute, out var attribute))
        {
            name = null;
            return false;
        }

        var typeName = GetNonGenericName(reflect, serviceType);
        var attributeNamespace = attribute.GetPropertyValue("Namespace") as string;
        var attributeName = attribute.GetPropertyValue("Name") as string;

        var genericEnding = new List<string>();
        GetServiceGenericEnding(reflect, serviceType, genericEnding);

        name = BuildServiceName(typeName, attributeNamespace, attributeName, genericEnding);
        return true;
    }

    public static bool TryGetOperationName<TType>(this IMethodInfo<TType> method, [NotNullWhen(true)] out string? name)
    {
        if (!method.TryGetCustomAttribute(OperationContractAttribute, out var attribute))
        {
            name = null;
            return false;
        }

        name = attribute.GetPropertyValue("Name") as string;
        name = string.IsNullOrWhiteSpace(name) ? method.Name : name!;
        return true;
    }

    public static string GetBaseClassName<TType>(IReflect<TType> reflect, TType serviceType, string? @namespace)
    {
        var result = new StringBuilder(@namespace);
        if (result.Length > 0)
        {
            result.Append('.');
        }

        var name = GetNonGenericName(reflect, serviceType);
        if (name.Length > 1 && name[0] == 'I')
        {
            result.Append(name, 1, name.Length - 1);
        }
        else
        {
            result.Append(name);
        }

        var genericEnding = new List<string>();
        GetServiceGenericEnding(reflect, serviceType, genericEnding);
        for (var i = 0; i < genericEnding.Count; i++)
        {
            var ending = genericEnding[i];
            for (var j = 0; j < ending.Length; j++)
            {
                var c = ending[j];
                if (c == '-' || c == '.' || c == '/' || c == '\\' || c == '`')
                {
                    result.Append('_');
                }
                else
                {
                    result.Append(c);
                }
            }
        }

        return result.ToString();
    }

    private static string GetNonGenericName<TType>(IReflect<TType> reflect, TType type)
    {
        var name = reflect.GetName(type);
        var index = name.IndexOf('`');
        return index <= 0 ? name : name.Substring(0, index);
    }

    private static void GetServiceGenericEnding<TType>(IReflect<TType> reflect, TType serviceType, IList<string> result)
    {
        var args = reflect.GenericTypeArguments(serviceType);
        for (var i = 0; i < args.Length; i++)
        {
            var type = args[i];

            if (TryExpandArray(reflect, type, out var elementType, out var prefix, out var arrayRank))
            {
                if (arrayRank > 1)
                {
                    prefix += arrayRank.ToString(CultureInfo.InvariantCulture);
                }

                type = elementType;
                result.Add(prefix + GetDataContainerName(reflect, type));
            }
            else
            {
                result.Add(GetDataContainerName(reflect, type));
            }

            GetServiceGenericEnding(reflect, type, result);
        }
    }

    private static string GetDataContainerName<TType>(IReflect<TType> reflect, TType type)
    {
        var attributeName = reflect.TryGetCustomAttribute(type, DataContractAttribute, out var attribute)
            ? attribute.GetPropertyValue("Name") as string
            : null;

        return string.IsNullOrWhiteSpace(attributeName) ? GetNonGenericName(reflect, type) : attributeName!;
    }

    private static bool TryExpandArray<TType>(
        IReflect<TType> reflect,
        TType type,
        [NotNullWhen(true)] out TType? elementType,
        [NotNullWhen(true)] out string? prefix,
        out int rank)
    {
        if (!reflect.TryGetArrayInfo(type, out var itemType, out rank))
        {
            prefix = null;
            elementType = default;
            return false;
        }

        prefix = "Array";
        elementType = itemType;
        while (reflect.TryGetArrayInfo(itemType, out itemType, out _))
        {
            prefix += "Array";
            elementType = itemType;
        }

        return true;
    }

    private static string BuildServiceName(
        string serviceTypeName,
        string? serviceContractAttributeNamespace,
        string? serviceContractAttributeName,
        List<string> serviceGenericEnding)
    {
        var result = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(serviceContractAttributeNamespace))
        {
            result
                .Append(serviceContractAttributeNamespace)
                .Append('.');
        }

        if (string.IsNullOrWhiteSpace(serviceContractAttributeName))
        {
            result.Append(serviceTypeName);
        }
        else
        {
            result.Append(serviceContractAttributeName);
        }

        for (var i = 0; i < serviceGenericEnding.Count; i++)
        {
            result
                .Append('-')
                .Append(serviceGenericEnding[i]);
        }

        return result.ToString();
    }
}