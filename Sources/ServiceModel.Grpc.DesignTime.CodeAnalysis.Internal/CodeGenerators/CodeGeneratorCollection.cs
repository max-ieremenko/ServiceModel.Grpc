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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

internal sealed class CodeGeneratorCollection : List<ICodeGenerator>, ICodeGeneratorCollection
{
    private readonly List<IMetadataExtension> _metadataExtensions = new();

    public void AddMetadata(IMetadataExtension metadata) => _metadataExtensions.Add(metadata);

    public TExtension? TryGetMetadata<TExtension>()
        where TExtension : IMetadataExtension
    {
        for (var i = 0; i < _metadataExtensions.Count; i++)
        {
            if (_metadataExtensions[i] is TExtension result)
            {
                return result;
            }
        }

        return default;
    }
}