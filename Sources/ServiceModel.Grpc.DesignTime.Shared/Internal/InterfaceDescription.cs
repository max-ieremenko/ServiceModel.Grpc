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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal;

internal sealed class InterfaceDescription
{
    public InterfaceDescription(INamedTypeSymbol interfaceType)
    {
        InterfaceType = interfaceType;
        InterfaceTypeName = SyntaxTools.GetFullName(interfaceType);
    }

    public INamedTypeSymbol InterfaceType { get; }

    public string InterfaceTypeName { get; }

    public List<NotSupportedMethodDescription> Methods { get; } = new List<NotSupportedMethodDescription>();

    public List<OperationDescription> Operations { get; } = new List<OperationDescription>();

    public List<NotSupportedMethodDescription> NotSupportedOperations { get; } = new List<NotSupportedMethodDescription>();

    public List<(OperationDescription Sync, OperationDescription Async)> SyncOverAsync { get; } = new List<(OperationDescription Sync, OperationDescription Async)>();
}