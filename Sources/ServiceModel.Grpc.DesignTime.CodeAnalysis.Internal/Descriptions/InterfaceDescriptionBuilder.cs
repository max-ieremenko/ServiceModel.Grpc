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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

internal readonly struct InterfaceDescriptionBuilder
{
    public InterfaceDescriptionBuilder(INamedTypeSymbol interfaceType)
    {
        InterfaceType = interfaceType;
    }

    public INamedTypeSymbol InterfaceType { get; }

    public List<INotSupportedMethodDescription> Methods { get; } = new();

    public List<IOperationDescription> Operations { get; } = new();

    public List<INotSupportedMethodDescription> NotSupportedOperations { get; } = new();

    public List<(IOperationDescription Sync, IOperationDescription Async)> SyncOverAsync { get; } = new();

    public static IInterfaceDescription[] ToArray(List<InterfaceDescriptionBuilder> builders)
    {
        var result = new IInterfaceDescription[builders.Count];
        for (var i = 0; i < builders.Count; i++)
        {
            result[i] = builders[i].Build();
        }

        return result;
    }

    private IInterfaceDescription Build() => new InterfaceDescription(
        InterfaceType,
        Methods.ToArray(),
        Operations.ToArray(),
        NotSupportedOperations.ToArray(),
        SyncOverAsync.ToArray());
}