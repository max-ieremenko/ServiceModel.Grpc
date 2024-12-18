﻿// <copyright>
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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.Extensions;

internal sealed class MemoryPackMarshaller : IExtensionProvider
{
    public void ProvideExtensions(ExtensionProviderDeclaration declaration, IExtensionCollection extensions, IExtensionContext context)
    {
        extensions.TryAdd<MPackCodeGeneratorExtension>().MemoryPack = true;
        extensions.TryAdd<ContractCodeGeneratorMetadata>();
    }
}