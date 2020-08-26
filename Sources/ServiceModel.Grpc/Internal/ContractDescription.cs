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
using System.Reflection;
using System.Text;

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

            ClientClassName = GetClientClassName(serviceType);
            ContractClassName = GetContractClassName(serviceType);
            ClientBuilderClassName = GetClientBuilderClassName(serviceType);
            ServiceClassName = GetServiceClassName(serviceType);
        }

        public string ClientClassName { get; }

        public string ContractClassName { get; }

        public string ClientBuilderClassName { get; }

        public string ServiceClassName { get; }

        public Type ServiceType { get; }

        public IList<InterfaceDescription> Interfaces { get; }

        public IList<InterfaceDescription> Services { get; }

        public static string GetClientClassName(Type serviceType) => GetClassName(serviceType, "Client");

        public static string GetContractClassName(Type serviceType) => GetClassName(serviceType, "Contract");

        public static string GetClientBuilderClassName(Type serviceType) => GetClassName(serviceType, "ClientBuilder");

        public static string GetServiceClassName(Type serviceType) => GetClassName(serviceType, "Service");

        private static string GetClassName(Type serviceType, string suffix)
        {
            return "{0}.{1}.{2}{3}".FormatWith(
                serviceType.Assembly.GetName().Name,
                ReflectionTools.GetNamespace(serviceType),
                serviceType.Name,
                suffix);
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
                return false;
            }
        }

        private void AnalyzeServiceAndInterfaces(Type serviceType)
        {
            foreach (var interfaceType in ReflectionTools.ExpandInterface(serviceType))
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

                foreach (var method in ReflectionTools.GetMethods(interfaceType))
                {
                    string? error;

                    if (serviceName == null || !ServiceContract.IsServiceOperation(method))
                    {
                        error = "Method {0}.{1}.{2} is not service operation.".FormatWith(
                            ReflectionTools.GetNamespace(interfaceType),
                            interfaceType.Name,
                            method.Name);

                        interfaceDescription.Methods.Add(new MethodDescription(method, error));
                        continue;
                    }

                    if (TryCreateMessage(method, out var message, out error))
                    {
                        interfaceDescription.Operations.Add(new OperationDescription(
                            serviceName,
                            ServiceContract.GetServiceOperationName(method),
                            message));
                    }
                    else
                    {
                        interfaceDescription.NotSupportedOperations.Add(new MethodDescription(method, error));
                    }
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
