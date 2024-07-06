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

using System.Collections.Generic;

namespace ServiceModel.Grpc.Descriptions;

internal readonly struct InterfaceDescriptionBuilder<TType>
{
    public InterfaceDescriptionBuilder(TType interfaceType)
    {
        InterfaceType = interfaceType;
    }

    public TType InterfaceType { get; }

    public List<NotSupportedMethodDescription<TType>> Methods { get; } = new();

    public List<OperationDescription<TType>> Operations { get; } = new();

    public List<NotSupportedMethodDescription<TType>> NotSupportedOperations { get; } = new();

    public List<(OperationDescription<TType> Sync, OperationDescription<TType> Async)> SyncOverAsync { get; } = new();

    public static InterfaceDescription<TType>[] ToArray(List<InterfaceDescriptionBuilder<TType>> builders)
    {
        var result = new InterfaceDescription<TType>[builders.Count];
        for (var i = 0; i < builders.Count; i++)
        {
            result[i] = builders[i].Build();
        }

        return result;
    }

    private InterfaceDescription<TType> Build() => new(
        InterfaceType,
        Operations.ToArray(),
        Methods.ToArray(),
        NotSupportedOperations.ToArray(),
        SyncOverAsync.ToArray());
}