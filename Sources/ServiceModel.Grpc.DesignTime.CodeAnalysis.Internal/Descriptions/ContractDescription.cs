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

[DebuggerDisplay("{ContractInterface.ToString()}")]
internal sealed class ContractDescription : IContractDescription
{
    public ContractDescription(
        INamedTypeSymbol contractInterface,
        string baseClassName,
        IInterfaceDescription[] interfaces,
        IInterfaceDescription[] services)
    {
        ContractInterface = contractInterface;
        BaseClassName = baseClassName;
        Interfaces = interfaces;
        Services = services;
    }

    public INamedTypeSymbol ContractInterface { get; }

    public string BaseClassName { get; }

    public IInterfaceDescription[] Interfaces { get; }

    public IInterfaceDescription[] Services { get; }
}