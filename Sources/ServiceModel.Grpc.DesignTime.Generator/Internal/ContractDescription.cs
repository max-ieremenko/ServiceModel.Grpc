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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.Internal
{
    internal sealed class ContractDescription
    {
        public ContractDescription(INamedTypeSymbol serviceType)
        {
            Interfaces = new List<InterfaceDescription>();
            Services = new List<InterfaceDescription>();

            AnalyzeServiceAndInterfaces(serviceType);
            FindDuplicates();

            BaseClassName = GetBaseName(serviceType.Name);
            ContractInterfaceName = SyntaxTools.GetFullName(serviceType);
            SortAll();
        }

        public string ContractInterfaceName { get; }

        public string BaseClassName { get; }

        public string ClientClassName => BaseClassName + "Client";

        public string ClientBuilderClassName => BaseClassName + "ClientBuilder";

        public string ContractClassName => BaseClassName + "Contract";

        public List<InterfaceDescription> Interfaces { get; }

        public List<InterfaceDescription> Services { get; }

        private static string GetBaseName(string name)
        {
            if (name.StartsWith('I') && name.Length > 1)
            {
                return name.Substring(1);
            }

            return name;
        }

        private static bool TryCreateOperation(
            IMethodSymbol method,
            string serviceName,
            [MaybeNullWhen(false)] out OperationDescription operation,
            [MaybeNullWhen(true)] out string error)
        {
            operation = default;
            error = default;

            try
            {
                operation = new OperationDescription(method, serviceName);
                return true;
            }
            catch (NotSupportedException ex)
            {
                var text = new StringBuilder();
                Exception e = ex;
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

        private void AnalyzeServiceAndInterfaces(INamedTypeSymbol serviceType)
        {
            foreach (var interfaceType in SyntaxTools.ExpandInterface(serviceType))
            {
                var interfaceDescription = new InterfaceDescription(interfaceType);

                string? serviceName = null;
                if (ServiceContract.IsServiceContractInterface(interfaceType))
                {
                    serviceName = ServiceContract.GetServiceName(interfaceType);
                    Services.Add(interfaceDescription);
                }
                else
                {
                    Interfaces.Add(interfaceDescription);
                }

                foreach (var method in SyntaxTools.GetInstanceMethods(interfaceType))
                {
                    string? error;

                    if (serviceName == null || !ServiceContract.IsServiceOperation(method))
                    {
                        error = "Method {0}.{1}.{2} is not service operation.".FormatWith(
                            SyntaxTools.GetNamespace(interfaceType),
                            interfaceType.Name,
                            method.Name);

                        interfaceDescription.Methods.Add(new NotSupportedMethodDescription(method, error));
                        continue;
                    }

                    if (TryCreateOperation(method, serviceName, out var operation, out error))
                    {
                        interfaceDescription.Operations.Add(operation);
                    }
                    else
                    {
                        interfaceDescription.NotSupportedOperations.Add(new NotSupportedMethodDescription(method, error));
                    }
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

        private readonly struct OperationKey : IEquatable<OperationKey>
        {
            private readonly string _serviceName;
            private readonly string _operationName;

            public OperationKey(string serviceName, string operationName)
            {
                _serviceName = serviceName;
                _operationName = operationName;
            }

            public bool Equals(OperationKey other)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(_serviceName, _serviceName)
                    && StringComparer.OrdinalIgnoreCase.Equals(_operationName, _operationName);
            }

            public override bool Equals(object obj) => throw new NotSupportedException();

            public override int GetHashCode()
            {
                var h1 = StringComparer.OrdinalIgnoreCase.GetHashCode(_serviceName);
                var h2 = StringComparer.OrdinalIgnoreCase.GetHashCode(_operationName);
                return ((h1 << 5) + h1) ^ h2;
            }
        }
    }
}
