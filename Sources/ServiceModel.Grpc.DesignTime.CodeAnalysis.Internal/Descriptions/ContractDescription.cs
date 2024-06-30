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

using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

internal sealed class ContractDescription : IContractDescription
{
    public ContractDescription(ContractDescription<ITypeSymbol> source)
    {
        ContractInterface = source.ContractInterface;
        BaseClassName = source.BaseClassName;

        Interfaces = new IInterfaceDescription[source.Interfaces.Length];
        for (var i = 0; i < source.Interfaces.Length; i++)
        {
            Interfaces[i] = new InterfaceDescription(source.Interfaces[i]);
        }

        Services = new IInterfaceDescription[source.Services.Length];
        for (var i = 0; i < source.Services.Length; i++)
        {
            Services[i] = new InterfaceDescription(source.Services[i]);
        }
    }

    public string BaseClassName { get; }

    public ITypeSymbol ContractInterface { get; }

    public IInterfaceDescription[] Interfaces { get; }

    public IInterfaceDescription[] Services { get; }
}