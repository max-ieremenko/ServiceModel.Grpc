using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ServiceModel.Grpc.Internal
{
    internal sealed class ContractDescription
    {
        public ContractDescription(Type serviceType)
        {
            Interfaces = new List<InterfaceDescription>();
            Services = new List<InterfaceDescription>();

            AnalyzeServiceAndInterfaces(serviceType);
            FindDuplicates();
        }

        public IList<InterfaceDescription> Interfaces { get; }

        public IList<InterfaceDescription> Services { get; }

        private static bool TryCreateMessage(MethodInfo method, out MessageAssembler message, out string error)
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
            }

            return false;
        }

        private void AnalyzeServiceAndInterfaces(Type serviceType)
        {
            foreach (var interfaceType in ReflectionTools.ExpandInterface(serviceType))
            {
                var interfaceDescription = new InterfaceDescription { InterfaceType = interfaceType };

                string serviceName = null;
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
                    string error;

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

                foreach (var (service, operation) in entries)
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
