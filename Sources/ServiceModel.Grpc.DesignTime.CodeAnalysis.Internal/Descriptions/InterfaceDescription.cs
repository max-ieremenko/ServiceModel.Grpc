// <copyright>
// Copyright 2020-2024 Max Ieremenko
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
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

[DebuggerDisplay("{InterfaceType.ToString()}")]
internal sealed class InterfaceDescription : IInterfaceDescription
{
    public InterfaceDescription(
        INamedTypeSymbol interfaceType,
        INotSupportedMethodDescription[] methods,
        IOperationDescription[] operations,
        INotSupportedMethodDescription[] notSupportedOperations,
        (IOperationDescription Sync, IOperationDescription Async)[] syncOverAsync)
    {
        InterfaceType = interfaceType;
        Methods = methods;
        Operations = operations;
        NotSupportedOperations = notSupportedOperations;
        SyncOverAsync = syncOverAsync;
    }

    public INamedTypeSymbol InterfaceType { get; }

    public INotSupportedMethodDescription[] Methods { get; }

    public IOperationDescription[] Operations { get; }

    public INotSupportedMethodDescription[] NotSupportedOperations { get; }

    public (IOperationDescription Sync, IOperationDescription Async)[] SyncOverAsync { get; }
}