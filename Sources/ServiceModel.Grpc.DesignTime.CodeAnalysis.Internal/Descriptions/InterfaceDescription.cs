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

internal sealed class InterfaceDescription : IInterfaceDescription
{
    public InterfaceDescription(InterfaceDescription<ITypeSymbol> source)
    {
        InterfaceType = source.InterfaceType;

        Operations = new IOperationDescription[source.Operations.Length];
        for (var i = 0; i < source.Operations.Length; i++)
        {
            Operations[i] = new OperationDescription(source.Operations[i]);
        }

        Methods = new INotSupportedMethodDescription[source.Methods.Length];
        for (var i = 0; i < source.Methods.Length; i++)
        {
            Methods[i] = new NotSupportedMethodDescription(source.Methods[i]);
        }

        NotSupportedOperations = new INotSupportedMethodDescription[source.NotSupportedOperations.Length];
        for (var i = 0; i < source.NotSupportedOperations.Length; i++)
        {
            NotSupportedOperations[i] = new NotSupportedMethodDescription(source.NotSupportedOperations[i]);
        }

        SyncOverAsync = new (IOperationDescription Sync, IOperationDescription Async)[source.SyncOverAsync.Length];
        for (var i = 0; i < source.SyncOverAsync.Length; i++)
        {
            var syncOverAsync = source.SyncOverAsync[i];
            SyncOverAsync[i] = (new OperationDescription(syncOverAsync.Sync), new OperationDescription(syncOverAsync.Async));
        }
    }

    public ITypeSymbol InterfaceType { get; }

    public IOperationDescription[] Operations { get; }

    public INotSupportedMethodDescription[] Methods { get; }

    public INotSupportedMethodDescription[] NotSupportedOperations { get; }

    public (IOperationDescription Sync, IOperationDescription Async)[] SyncOverAsync { get; }
}