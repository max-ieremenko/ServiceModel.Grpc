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

using Grpc.Core;
using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.Descriptions;

internal static class ContractDescriptionBuilder<TType>
{
    public static bool IsServiceContractInterface(TType serviceType, IReflect<TType> reflect) => reflect.TryGetServiceName(serviceType, out _);

    public static ContractDescription<TType> Build(TType serviceType, string? @namespace, IReflect<TType> reflect)
    {
        var interfaces = new List<InterfaceDescriptionBuilder<TType>>();
        var services = new List<InterfaceDescriptionBuilder<TType>>();

        AnalyzeServiceAndInterfaces(serviceType, reflect, interfaces, services);
        FindDuplicates(reflect, services);
        FindSyncOverAsync(reflect, services);

        var baseClassName = NamingContract.GetBaseClassName(reflect, serviceType, @namespace);

        return new ContractDescription<TType>(
            serviceType,
            baseClassName,
            InterfaceDescriptionBuilder<TType>.ToArray(interfaces),
            InterfaceDescriptionBuilder<TType>.ToArray(services));
    }

    public static bool TryBuildOperation(
        IMethodInfo<TType> method,
        string serviceName,
        string operationName,
        IReflect<TType> reflect,
        [NotNullWhen(true)] out OperationDescription<TType>? operation,
        [NotNullWhen(false)] out string? error) =>
        new OperationDescriptionBuilder<TType>(method, serviceName, operationName, reflect).TryBuild(out operation, out error);

    private static void AnalyzeServiceAndInterfaces(
        TType serviceType,
        IReflect<TType> reflect,
        List<InterfaceDescriptionBuilder<TType>> interfaces,
        List<InterfaceDescriptionBuilder<TType>> services)
    {
        var tree = new InterfaceTree<TType>(serviceType, reflect);

        for (var i = 0; i < tree.Services.Count; i++)
        {
            var service = tree.Services[i];
            var interfaceDescription = new InterfaceDescriptionBuilder<TType>(service.ServiceType);
            services.Add(interfaceDescription);

            foreach (var method in reflect.GetMethods(service.ServiceType))
            {
                if (!method.TryGetOperationName(out var operationName))
                {
                    interfaceDescription.Methods.Add(CreateNonServiceOperation(reflect, service.ServiceType, method));
                    continue;
                }

                if (TryBuildOperation(method, service.ServiceName, operationName, reflect, out var operation, out var error))
                {
                    interfaceDescription.Operations.Add(operation);
                }
                else
                {
                    interfaceDescription.NotSupportedOperations.Add(new NotSupportedMethodDescription<TType>(method, error));
                }
            }
        }

        for (var i = 0; i < tree.Interfaces.Count; i++)
        {
            var interfaceType = tree.Interfaces[i];
            var interfaceDescription = new InterfaceDescriptionBuilder<TType>(interfaceType);
            interfaces.Add(interfaceDescription);

            foreach (var method in reflect.GetMethods(interfaceType))
            {
                interfaceDescription.Methods.Add(CreateNonServiceOperation(reflect, interfaceType, method));
            }
        }
    }

    private static NotSupportedMethodDescription<TType> CreateNonServiceOperation(IReflect<TType> reflect, TType interfaceType, IMethodInfo<TType> method)
    {
        var error = $"Method {reflect.GetFullName(interfaceType)}.{method.Name} is not service operation.";
        return new NotSupportedMethodDescription<TType>(method, error);
    }

    private static void FindDuplicates(IReflect<TType> reflect, List<InterfaceDescriptionBuilder<TType>> services)
    {
        var duplicates = services
            .SelectMany(s => s.Operations.Select(m => (s, m)))
            .GroupBy(i => new OperationKey(i.m.ServiceName, i.m.OperationName))
            .Where(i => i.Count() > 1);

        foreach (var entries in duplicates)
        {
            var text = new StringBuilder();

            foreach (var (_, operation) in entries)
            {
                if (text.Length == 0)
                {
                    text.AppendFormat("Operations have naming conflict [{0}/{1}]: ", operation.ServiceName, operation.OperationName);
                }
                else
                {
                    text.Append(" and ");
                }

                text.AppendFormat(reflect.GetSignature(operation.Method));
            }

            var error = text.Append(".").ToString();

            foreach (var (service, operation) in entries)
            {
                service.Operations.Remove(operation);
                service.NotSupportedOperations.Add(new NotSupportedMethodDescription<TType>(operation.Method, error));
            }
        }
    }

    private static void FindSyncOverAsync(IReflect<TType> reflect, List<InterfaceDescriptionBuilder<TType>> services)
    {
        for (var i = 0; i < services.Count; i++)
        {
            var service = services[i];

            for (var j = 0; j < service.Methods.Count; j++)
            {
                var syncMethod = service.Methods[j];

                if (!new OperationDescriptionBuilder<TType>(syncMethod.Method, string.Empty, string.Empty, reflect).TryBuild(out var syncOperation, out _)
                    || syncOperation.OperationType != MethodType.Unary
                    || syncOperation.IsAsync)
                {
                    continue;
                }

                if (TryFindAsyncOperation(service.Operations, syncOperation, reflect, out var asyncOperation))
                {
                    service.Methods.Remove(syncMethod);
                    service.SyncOverAsync.Add((syncOperation, asyncOperation));
                    j--;
                }
            }
        }
    }

    private static bool TryFindAsyncOperation(
        List<OperationDescription<TType>> operations,
        OperationDescription<TType> syncOperation,
        IReflect<TType> reflect,
        [NotNullWhen(true)] out OperationDescription<TType>? asyncOperation)
    {
        var asyncMethodName = syncOperation.Method.Name + "Async";
        for (var i = 0; i < operations.Count; i++)
        {
            var operation = operations[i];
            if (operation.IsAsync
                && operation.OperationType == MethodType.Unary
                && operation.Method.Name.Equals(asyncMethodName, StringComparison.OrdinalIgnoreCase)
                && operation.IsCompatibleWith(syncOperation, reflect))
            {
                asyncOperation = operation;
                return true;
            }
        }

        asyncOperation = null;
        return false;
    }
}