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

using System.Diagnostics;

namespace ServiceModel.Grpc.Internal
{
    [DebuggerDisplay("{ServiceName}/{OperationName}")]
    internal sealed class OperationDescription
    {
        public OperationDescription(string serviceName, string operationName, MessageAssembler message)
        {
            ServiceName = serviceName;
            OperationName = operationName;
            Message = message;
        }

        public string ServiceName { get; }

        public string OperationName { get; }

        public MessageAssembler Message { get; }
    }
}