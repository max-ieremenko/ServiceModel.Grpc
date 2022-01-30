// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal
{
    internal sealed class ContractDescription
    {
        public ContractDescription(Type serviceType)
        {
            ServiceType = serviceType;
            Interfaces = new List<InterfaceDescription>();
            Services = new List<InterfaceDescription>();

            AnalyzeServiceAndInterfaces(serviceType);
            FindDuplicates();
            FindSyncOverAsync();

            var baseClassName = GetClassName(serviceType);
            ClientClassName = baseClassName + "Client";
            ContractClassName = baseClassName + "Contract";
            ClientBuilderClassName = baseClassName + "ClientBuilder";
            EndpointClassName = baseClassName + "Endpoint";
        }

        public string ClientClassName { get; }

        public string ContractClassName { get; }

        public string ClientBuilderClassName { get; }

        public string EndpointClassName { get; }

        public Type ServiceType { get; }

        public IList<InterfaceDescription> Interfaces { get; }

        public IList<InterfaceDescription> Services { get; }

        public static string GetContractClassName(Type serviceType) => GetClassName(serviceType, "Contract");

        public static string GetClientBuilderClassName(Type serviceType) => GetClassName(serviceType, "ClientBuilder");

        public static string GetEndpointClassName(Type serviceType) => GetClassName(serviceType, "Endpoint");

        private static string GetClassName(Type serviceType, string? suffix = null)
        {
            var result = new StringBuilder()
                .Append(serviceType.Assembly.GetName().Name)
                .Append('.')
                .Append(ReflectionTools.GetNamespace(serviceType))
                .Append('.')
                .Append(ReflectionTools.GetNonGenericName(serviceType));

            var serviceGenericEnding = ServiceContract.GetServiceGenericEnding(serviceType);
            for (var i = 0; i < serviceGenericEnding.Count; i++)
            {
                result
                    .Append('-')
                    .Append(serviceGenericEnding[i]);
            }

            result.Append(suffix);
            return result.ToString();
        }

        private static bool TryCreateMessage(MethodInfo method, [MaybeNullWhen(false)] out MessageAssembler message, [MaybeNullWhen(true)] out string error)
        {
            message = default;
            error = default;

            try
            {
                message = new MessageAssembler(method);
                return true;
            }
            catch (NotSupportedException ex)
            {
                var text = new StringBuilder();
                Exception? e = ex;
                while (e != null)
                {
                    if (text.Length > 0)
                    {
                        text.Append(" --> ");
                    }

                    text.Append(ex.Message);
                    e = e.InnerException;
                }

                error = text.ToString();
                return false;
            }
        }

        private static OperationDescription? TryFindAsyncOperation(IList<OperationDescription> operations, MessageAssembler syncOperation)
        {
            var asyncMethodName = syncOperation.Operation.Name + "Async";
            for (var i = 0; i < operations.Count; i++)
            {
                var operation = operations[i];
                if (operation.Message.IsAsync
                    && operation.Message.OperationType == MethodType.Unary
                    && operation.Message.Operation.Name.Equals(asyncMethodName, StringComparison.OrdinalIgnoreCase)
                    && operation.Message.IsCompatibleWith(syncOperation))
                {
                    return operation;
                }
            }

            return null;
        }

        private static MethodDescription CreateNonServiceOperation(Type interfaceType, MethodInfo method)
        {
            var error = "Method {0}.{1}.{2} is not service operation.".FormatWith(
                ReflectionTools.GetNamespace(interfaceType),
                interfaceType.Name,
                method.Name);

            return new MethodDescription(method, error);
        }

        private void AnalyzeServiceAndInterfaces(Type serviceType)
        {
            var tree = new InterfaceTree(serviceType);

            for (var i = 0; i < tree.Services.Count; i++)
            {
                var service = tree.Services[i];
                var interfaceDescription = new InterfaceDescription(service.ServiceType);
                Services.Add(interfaceDescription);

                foreach (var method in ReflectionTools.GetMethods(service.ServiceType))
                {
                    if (!ServiceContract.IsServiceOperation(method))
                    {
                        interfaceDescription.Methods.Add(CreateNonServiceOperation(service.ServiceType, method));
                        continue;
                    }

                    if (TryCreateMessage(method, out var message, out var error))
                    {
                        interfaceDescription.Operations.Add(new OperationDescription(
                            service.ServiceName,
                            ServiceContract.GetServiceOperationName(method),
                            message));
                    }
                    else
                    {
                        interfaceDescription.NotSupportedOperations.Add(new MethodDescription(method, error));
                    }
                }
            }

            for (var i = 0; i < tree.Interfaces.Count; i++)
            {
                var interfaceType = tree.Interfaces[i];
                var interfaceDescription = new InterfaceDescription(interfaceType);
                Interfaces.Add(interfaceDescription);

                foreach (var method in ReflectionTools.GetMethods(interfaceType))
                {
                    interfaceDescription.Methods.Add(CreateNonServiceOperation(interfaceType, method));
                }
            }
        }

        private void FindDuplicates()
        {
            var duplicates = Services
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

                    text.AppendFormat(ReflectionTools.GetSignature(operation.Message.Operation));
                }

                var error = text.Append(".").ToString();

                foreach (var (service, operation) in entries)
                {
                    service.Operations.Remove(operation);
                    service.NotSupportedOperations.Add(new MethodDescription(operation.Message.Operation, error));
                }
            }
        }

        private void FindSyncOverAsync()
        {
            for (var i = 0; i < Services.Count; i++)
            {
                var service = Services[i];

                for (var j = 0; j < service.Methods.Count; j++)
                {
                    var syncMethod = service.Methods[j];
                    if (!TryCreateMessage(syncMethod.Method, out var syncOperation, out _)
                        || syncOperation.OperationType != MethodType.Unary
                        || syncOperation.IsAsync)
                    {
                        continue;
                    }

                    var asyncOperation = TryFindAsyncOperation(service.Operations, syncOperation);
                    if (asyncOperation != null)
                    {
                        service.Methods.Remove(syncMethod);
                        service.SyncOverAsync.Add((syncOperation, asyncOperation));
                        j--;
                    }
                }
            }
        }
    }
}
