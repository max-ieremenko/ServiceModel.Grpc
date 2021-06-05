// <copyright>
// Copyright 2021 Max Ieremenko
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
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Swashbuckle.Internal
{
    internal sealed class ContractDescriptionCache
    {
        private readonly IDictionary<OperationKey, OperationDescription> _operationByFullName = new Dictionary<OperationKey, OperationDescription>();

        public bool TryFindOperation(
            Type serviceType,
            string serviceName,
            string operationName,
            [NotNullWhen(true)] out OperationDescription? operation)
        {
            if (TryGetOperation(serviceName, operationName, out operation))
            {
                return true;
            }

            AddService(serviceType);

            return TryGetOperation(serviceName, operationName, out operation);
        }

        private void AddService(Type serviceType)
        {
            var contractDescription = new ContractDescription(serviceType);
            for (var i = 0; i < contractDescription.Services.Count; i++)
            {
                var service = contractDescription.Services[i];
                for (var j = 0; j < service.Operations.Count; j++)
                {
                    var operation = service.Operations[j];
                    var key = new OperationKey(operation.ServiceName, operation.OperationName);
                    if (!_operationByFullName.ContainsKey(key))
                    {
                        _operationByFullName.Add(key, operation);
                    }
                }
            }
        }

        private bool TryGetOperation(string serviceName, string operationName, [NotNullWhen(true)] out OperationDescription? operation)
        {
            var key = new OperationKey(serviceName, operationName);
            return _operationByFullName.TryGetValue(key, out operation);
        }
    }
}
