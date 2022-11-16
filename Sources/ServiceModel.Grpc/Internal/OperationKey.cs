// <copyright>
// Copyright 2020-2021 Max Ieremenko
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

namespace ServiceModel.Grpc.Internal;

internal readonly struct OperationKey : IEquatable<OperationKey>
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