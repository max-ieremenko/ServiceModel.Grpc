// <copyright>
// Copyright 2024 Max Ieremenko
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
using System.Text;
using Grpc.Core;
using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

internal readonly ref struct ContractDescriptionBuilder
{
    private readonly INamedTypeSymbol _serviceType;
    private readonly List<InterfaceDescriptionBuilder> _interfaces;
    private readonly List<InterfaceDescriptionBuilder> _services;

    public ContractDescriptionBuilder(INamedTypeSymbol serviceType)
    {
        _serviceType = serviceType;
        _interfaces = new();
        _services = new();
    }

    public IContractDescription Build()
    {
        AnalyzeServiceAndInterfaces();
        FindDuplicates();
        FindSyncOverAsync();
        SortAll();

        return new ContractDescription(
            _serviceType,
            GetBaseClassName(_serviceType),
            InterfaceDescriptionBuilder.ToArray(_interfaces),
            InterfaceDescriptionBuilder.ToArray(_services));
    }

    private static NotSupportedMethodDescription CreateNonServiceOperation(INamedTypeSymbol interfaceType, IMethodSymbol method)
    {
        var error = $"Method {SyntaxTools.GetNamespace(interfaceType)}.{interfaceType.Name}.{method.Name} is not service operation.";
        return new NotSupportedMethodDescription(method, error);
    }

    private static bool TryFindAsyncOperation(
        IList<IOperationDescription> operations,
        OperationDescription syncOperation,
        [NotNullWhen(true)] out OperationDescription? asyncOperation)
    {
        var asyncMethodName = syncOperation.Method.Name + "Async";
        for (var i = 0; i < operations.Count; i++)
        {
            var operation = (OperationDescription)operations[i];
            if (operation.IsAsync
                && operation.OperationType == MethodType.Unary
                && operation.Method.Name.Equals(asyncMethodName, StringComparison.OrdinalIgnoreCase)
                && operation.IsCompatibleWith(syncOperation))
            {
                asyncOperation = operation;
                return true;
            }
        }

        asyncOperation = null;
        return false;
    }

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

    private static int Compare(InterfaceDescriptionBuilder x, InterfaceDescriptionBuilder y) =>
        StringComparer.Ordinal.Compare(x.InterfaceType.Name, y.InterfaceType.Name);

    private static int Compare(INotSupportedMethodDescription x, INotSupportedMethodDescription y) =>
        StringComparer.Ordinal.Compare(x.Method.Name, y.Method.Name);

    private static int Compare(IOperationDescription x, IOperationDescription y) =>
        StringComparer.Ordinal.Compare(x.Method.Name, y.Method.Name);

    private void AnalyzeServiceAndInterfaces()
    {
        var tree = new InterfaceTree(_serviceType);

        for (var i = 0; i < tree.Services.Count; i++)
        {
            var service = tree.Services[i];
            var interfaceDescription = new InterfaceDescriptionBuilder(service.ServiceType);
            _services.Add(interfaceDescription);

            foreach (var method in SyntaxTools.GetInstanceMethods(service.ServiceType))
            {
                if (!ServiceContract.IsServiceOperation(method))
                {
                    interfaceDescription.Methods.Add(CreateNonServiceOperation(service.ServiceType, method));
                    continue;
                }

                if (new OperationDescriptionBuilder(method, service.ServiceName, ServiceContract.GetServiceOperationName(method)).TryBuild(out var operation, out var error))
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
            var interfaceDescription = new InterfaceDescriptionBuilder(interfaceType);
            _interfaces.Add(interfaceDescription);

            foreach (var method in SyntaxTools.GetInstanceMethods(interfaceType))
            {
                interfaceDescription.Methods.Add(CreateNonServiceOperation(interfaceType, method));
            }
        }
    }

    private void FindDuplicates()
    {
        var duplicates = _services
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

                text.AppendFormat(SyntaxTools.GetSignature(operation.Method));
            }

            var error = text.Append(".").ToString();

            foreach (var (service, operation) in entries)
            {
                service.Operations.Remove(operation);
                service.NotSupportedOperations.Add(new NotSupportedMethodDescription(operation.Method, error));
            }
        }
    }

    private void FindSyncOverAsync()
    {
        for (var i = 0; i < _services.Count; i++)
        {
            var service = _services[i];

            for (var j = 0; j < service.Methods.Count; j++)
            {
                var syncMethod = service.Methods[j];

                if (!new OperationDescriptionBuilder(syncMethod.Method, string.Empty, string.Empty).TryBuild(out var syncOperation, out _)
                    || syncOperation.OperationType != MethodType.Unary
                    || syncOperation.IsAsync)
                {
                    continue;
                }

                if (TryFindAsyncOperation(service.Operations, syncOperation, out var asyncOperation))
                {
                    service.Methods.Remove(syncMethod);
                    syncOperation.ClrDefinitionMethodName = asyncOperation.ClrDefinitionMethodName + "Sync";
                    service.SyncOverAsync.Add((syncOperation, asyncOperation));
                    j--;
                }
            }
        }
    }

    private void SortAll()
    {
        _interfaces.Sort(Compare);
        _services.Sort(Compare);

        foreach (var description in _interfaces.Concat(_services))
        {
            description.Methods.Sort(Compare);
            description.NotSupportedOperations.Sort(Compare);
            description.Operations.Sort(Compare);
        }
    }
}