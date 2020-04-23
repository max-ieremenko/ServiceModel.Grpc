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
using System.Reflection;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal
{
    internal static class ServiceContract
    {
        public static bool IsNativeGrpcService(Type type)
        {
            return type.IsClass
                   && type.GetCustomAttribute<BindServiceMethodAttribute>() != null;
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

            return GetServiceName(serviceType, attribute);
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

        internal static string GetServiceName(Type serviceType, Attribute serviceContractAttribute)
        {
            var attributeType = serviceContractAttribute.GetType();

            var @namespace = (string)attributeType.TryInstanceProperty("Namespace")?.GetValue(serviceContractAttribute);
            var name = (string)attributeType.TryInstanceProperty("Name")?.GetValue(serviceContractAttribute);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = serviceType.Name;
            }

            if (string.IsNullOrWhiteSpace(@namespace))
            {
                return name;
            }

            return @namespace + "." + name;
        }

        internal static string GetServiceOperationName(string methodName, Attribute operationContractAttribute)
        {
            var name = (string)operationContractAttribute
                .GetType()
                .TryInstanceProperty("Name")
                ?.GetValue(operationContractAttribute);
            return string.IsNullOrWhiteSpace(name) ? methodName : name;
        }

        private static Attribute GetServiceContractAttribute(Type type)
        {
            return ReflectionTools.GetCustomAttribute(type, "System.ServiceModel.ServiceContractAttribute");
        }

        private static Attribute GetOperationContractAttribute(MethodInfo method)
        {
            return ReflectionTools.GetCustomAttribute(method, "System.ServiceModel.OperationContractAttribute");
        }
    }
}
