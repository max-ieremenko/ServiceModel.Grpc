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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

public partial class AttributeAnalyzerTest
{
    [DesignTimeExtension<SomeExtension>("foo", 1)]
    public sealed class ExtensionHolder;

    [ExportGrpcService(typeof(object))]
    public sealed class ExportHolder;

    [ImportGrpcService(typeof(object))]
    public sealed class ImportHolder;

    internal sealed class ImportGrpcServiceExtension;

    internal sealed class ExportGrpcServiceExtension;

    internal sealed class SomeExtension : IExtensionProvider
    {
        public void ProvideExtensions(ExtensionProviderDeclaration declaration, IExtensionCollection extensions, IExtensionContext context)
        {
        }
    }
}