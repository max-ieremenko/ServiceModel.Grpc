using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace ServiceModel.Grpc.Internal
{
    internal static class ServiceContract
    {
        public static ICollection<Type> GetServiceContractInterfaces(Type serviceType)
        {
            return ReflectionTools
                .ExpandInterface(serviceType)
                .Where(IsServiceContractInterface)
                .ToArray();
        }

        public static bool IsServiceContractInterface(Type type)
        {
            return (type.IsPublic || type.IsNestedPublic)
                   && !type.IsGenericType
                   && type.GetCustomAttribute<ServiceContractAttribute>() != null;
        }

        public static bool IsServiceOperation(MethodInfo method)
        {
            return method.IsPublic
                   && !method.IsGenericMethod
                   && !method.IsSpecialName
                   && method.GetCustomAttribute<OperationContractAttribute>() != null;
        }

        public static IList<MethodInfo> GetServiceOperations(Type serviceType)
        {
            if (!IsServiceContractInterface(serviceType))
            {
                throw new ArgumentOutOfRangeException(nameof(serviceType));
            }

            return ReflectionTools
                .GetMethods(serviceType)
                .Where(IsServiceOperation)
                .ToList();
        }

        public static string GetServiceName(Type serviceType)
        {
            var attribute = serviceType.GetCustomAttribute<ServiceContractAttribute>();
            if (attribute == null)
            {
                throw new ArgumentOutOfRangeException(nameof(serviceType));
            }

            return GetServiceName(serviceType, attribute);
        }

        public static string GetServiceOperationName(MethodInfo method)
        {
            var attribute = method.GetCustomAttribute<OperationContractAttribute>();
            if (attribute == null)
            {
                throw new ArgumentOutOfRangeException(nameof(method));
            }

            return GetServiceOperationName(method.Name, attribute);
        }

        internal static string GetServiceName(Type serviceType, ServiceContractAttribute attribute)
        {
            var @namespace = attribute.Namespace;
            if (string.IsNullOrWhiteSpace(@namespace))
            {
                @namespace = ReflectionTools.GetNamespace(serviceType);
            }

            var name = attribute.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = serviceType.Name;
            }

            return @namespace + "." + name;
        }

        internal static string GetServiceOperationName(string methodName, OperationContractAttribute attribute)
        {
            return string.IsNullOrWhiteSpace(attribute.Name) ? methodName : attribute.Name;
        }
    }
}
