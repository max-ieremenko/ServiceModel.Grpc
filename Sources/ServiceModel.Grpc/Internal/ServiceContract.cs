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
using System.Reflection;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal;

internal static class ServiceContract
{
    public static bool IsNativeGrpcService(Type type)
    {
        return type.IsClass
               && type.GetCustomAttribute<BindServiceMethodAttribute>() != null;
    }

    public static bool IsServiceInstanceType(Type type)
    {
        return !type.IsInterface && !type.IsAbstract;
    }

    public static bool IsServiceContractInterface(Type type)
    {
        return ReflectionTools.IsPublicInterface(type)
               && !type.IsGenericTypeDefinition
               && GetServiceContractAttribute(type) != null;
    }

    public static bool IsServiceOperation(MethodInfo method)
    {
        return method.IsPublic
               && GetOperationContractAttribute(method) != null;
    }

    public static string GetServiceName(Type serviceType)
    {
        var attribute = GetServiceContractAttribute(serviceType);
        if (attribute == null)
        {
            throw new ArgumentOutOfRangeException(nameof(serviceType));
        }

        var (typeName, attributeNamespace, attributeName) = GetServiceNonGenericName(serviceType, attribute);
        var genericEnding = GetServiceGenericEnding(serviceType);

        return NamingContract.GetServiceName(typeName, attributeNamespace, attributeName, genericEnding);
    }

    public static string GetServiceOperationName(MethodInfo method)
    {
        var attribute = GetOperationContractAttribute(method);
        if (attribute == null)
        {
            throw new ArgumentOutOfRangeException(nameof(method));
        }

        return GetServiceOperationName(method.Name, attribute);
    }

    public static IList<string> GetServiceGenericEnding(Type serviceType)
    {
        var result = new List<string>();
        GetServiceGenericEnding(serviceType, result);

        return result;
    }

    internal static (string TypeName, string? AttributeNamespace, string? AttributeName) GetServiceNonGenericName(Type serviceType, Attribute serviceContractAttribute)
    {
        var attributeType = serviceContractAttribute.GetType();

        var @namespace = (string?)attributeType.TryInstanceProperty("Namespace")?.GetValue(serviceContractAttribute);
        var name = (string?)attributeType.TryInstanceProperty("Name")?.GetValue(serviceContractAttribute);

        return (ReflectionTools.GetNonGenericName(serviceType), @namespace, name);
    }

    internal static string GetServiceOperationName(string methodName, Attribute operationContractAttribute)
    {
        var name = (string?)operationContractAttribute
            .GetType()
            .TryInstanceProperty("Name")
            ?.GetValue(operationContractAttribute);
        return string.IsNullOrWhiteSpace(name) ? methodName : name!;
    }

    private static Attribute? GetServiceContractAttribute(Type type)
    {
        return ReflectionTools.GetCustomAttribute(type, "System.ServiceModel.ServiceContractAttribute");
    }

    private static Attribute? GetOperationContractAttribute(MethodInfo method)
    {
        return ReflectionTools.GetCustomAttribute(method, "System.ServiceModel.OperationContractAttribute");
    }

    private static void GetServiceGenericEnding(Type serviceType, IList<string> result)
    {
        if (!serviceType.IsGenericType)
        {
            return;
        }

        var args = serviceType.GetGenericArguments();
        for (var i = 0; i < args.Length; i++)
        {
            var type = args[i];

            if (type.IsArray)
            {
                var (elementType, prefix) = ExpandArray(type);
                if (type.GetArrayRank() > 1)
                {
                    prefix += type.GetArrayRank().ToString(CultureInfo.InvariantCulture);
                }

                type = elementType;
                result.Add(prefix + GetDataContainerName(type));
            }
            else
            {
                result.Add(GetDataContainerName(type));
            }

            GetServiceGenericEnding(type, result);
        }
    }

    private static string GetDataContainerName(Type type)
    {
        var attribute = ReflectionTools.GetCustomAttribute(type, "System.Runtime.Serialization.DataContractAttribute");
        string? attributeName = null;

        if (attribute != null)
        {
            attributeName = (string?)attribute.GetType().TryInstanceProperty("Name")?.GetValue(attribute);
        }

        return string.IsNullOrWhiteSpace(attributeName) ? ReflectionTools.GetNonGenericName(type) : attributeName!;
    }

    private static (Type ElementType, string Prefix) ExpandArray(Type array)
    {
        var prefix = "Array";
        var elementType = array.GetElementType()!;
        while (elementType.IsArray)
        {
            prefix += "Array";
            elementType = elementType.GetElementType()!;
        }

        return (elementType, prefix);
    }
}