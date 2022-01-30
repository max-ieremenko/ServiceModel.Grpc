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
using System.Linq;
using System.Text;
using Grpc.Core;
using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal
{
    internal sealed class ContractDescription
    {
        public ContractDescription(INamedTypeSymbol serviceType)
        {
            Interfaces = new List<InterfaceDescription>();
            Services = new List<InterfaceDescription>();

            AnalyzeServiceAndInterfaces(serviceType);
            FindDuplicates();
            FindSyncOverAsync();

            BaseClassName = GetBaseClassName(serviceType);
            ClientClassName = BaseClassName + "Client";
            ClientBuilderClassName = BaseClassName + "ClientBuilder";
            ContractClassName = BaseClassName + "Contract";
            EndpointClassName = BaseClassName + "Endpoint";
            EndpointBinderClassName = BaseClassName + "EndpointBinder";

            ContractInterfaceName = SyntaxTools.GetFullName(serviceType);
            ContractInterface = serviceType;
            SortAll();
        }

        public string BaseClassName { get; }

        public string ContractInterfaceName { get; }

        public INamedTypeSymbol ContractInterface { get; }

        public string ClientClassName { get; }

        public string ClientBuilderClassName { get; }

        public string ContractClassName { get; }

        public string EndpointClassName { get; }

        public string EndpointBinderClassName { get; }

        public List<InterfaceDescription> Interfaces { get; }

        public List<InterfaceDescription> Services { get; }

        private static string GetBaseClassName(INamedTypeSymbol serviceType)
        {
            var result = new StringBuilder(serviceType.Name);
            if (result.Length > 0 && result[0] == 'I')
            {
                result.Remove(0, 1);
            }

            var serviceGenericEnding = ServiceContract.GetServiceGenericEnding(serviceType);
            for (var i = 0; i < serviceGenericEnding.Count; i++)
            {
                result.Append(serviceGenericEnding[i]);
            }

            for (var i = 0; i < result.Length; i++)
            {
                var c = result[i];
                if (c == '-' || c == '.' || c == '/' || c == '\\' || c == '`')
                {
                    result[i] = '_';
                }
            }

            return result.ToString();
        }

        private static bool TryCreateOperation(
            IMethodSymbol method,
            string serviceName,
            string operationName,
            out OperationDescription operation,
            out string error)
        {
            operation = default!;
            error = default!;

            try
            {
                operation = new OperationDescription(method, serviceName, operationName);
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
            }

            return false;
        }

        private static OperationDescription? TryFindAsyncOperation(IList<OperationDescription> operations, OperationDescription syncOperation)
        {
            var asyncMethodName = syncOperation.Method.Name + "Async";
            for (var i = 0; i < operations.Count; i++)
            {
                var operation = operations[i];
                if (operation.IsAsync
                    && operation.OperationType == MethodType.Unary
                    && operation.Method.Name.Equals(asyncMethodName, StringComparison.OrdinalIgnoreCase)
                    && operation.IsCompatibleWith(syncOperation))
                {
                    return operation;
                }
            }

            return null;
        }

        private static NotSupportedMethodDescription CreateNonServiceOperation(INamedTypeSymbol interfaceType, IMethodSymbol method)
        {
            var error = "Method {0}.{1}.{2} is not service operation.".FormatWith(
                SyntaxTools.GetNamespace(interfaceType),
                interfaceType.Name,
                method.Name);

            return new NotSupportedMethodDescription(method, error);
        }

        private void AnalyzeServiceAndInterfaces(INamedTypeSymbol serviceType)
        {
            var tree = new InterfaceTree(serviceType);

            for (var i = 0; i < tree.Services.Count; i++)
            {
                var service = tree.Services[i];
                var interfaceDescription = new InterfaceDescription(service.ServiceType);
                Services.Add(interfaceDescription);

                foreach (var method in SyntaxTools.GetInstanceMethods(service.ServiceType))
                {
                    if (!ServiceContract.IsServiceOperation(method))
                    {
                        interfaceDescription.Methods.Add(CreateNonServiceOperation(service.ServiceType, method));
                        continue;
                    }

                    if (TryCreateOperation(method, service.ServiceName, ServiceContract.GetServiceOperationName(method), out var operation, out var error))
                    {
                        interfaceDescription.Operations.Add(operation);
                    }
                    else
                    {
                        interfaceDescription.NotSupportedOperations.Add(new NotSupportedMethodDescription(method, error));
                    }
                }
            }

            for (var i = 0; i < tree.Interfaces.Count; i++)
            {
                var interfaceType = tree.Interfaces[i];
                var interfaceDescription = new InterfaceDescription(interfaceType);
                Interfaces.Add(interfaceDescription);

                foreach (var method in SyntaxTools.GetInstanceMethods(interfaceType))
                {
                    interfaceDescription.Methods.Add(CreateNonServiceOperation(interfaceType, method));
                }
            }
        }

        private void SortAll()
        {
            Interfaces.Sort((x, y) => StringComparer.Ordinal.Compare(x.InterfaceTypeName, y.InterfaceTypeName));
            Services.Sort((x, y) => StringComparer.Ordinal.Compare(x.InterfaceTypeName, y.InterfaceTypeName));

            foreach (var description in Interfaces.Concat(Services))
            {
                description.Methods.Sort((x, y) => StringComparer.Ordinal.Compare(x.Method.Name, y.Method.Name));
                description.NotSupportedOperations.Sort((x, y) => StringComparer.Ordinal.Compare(x.Method.Name, y.Method.Name));
                description.Operations.Sort((x, y) => StringComparer.Ordinal.Compare(x.Method.Name, y.Method.Name));
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

                    text.AppendFormat(SyntaxTools.GetSignature(operation.Method.Source));
                }

                var error = text.Append(".").ToString();

                foreach (var (service, operation) in entries)
                {
                    service.Operations.Remove(operation);
                    service.NotSupportedOperations.Add(new NotSupportedMethodDescription(operation.Method.Source, error));
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
                    if (!TryCreateOperation(syncMethod.Method.Source, "dummy", "dummy", out var syncOperation, out _)
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
