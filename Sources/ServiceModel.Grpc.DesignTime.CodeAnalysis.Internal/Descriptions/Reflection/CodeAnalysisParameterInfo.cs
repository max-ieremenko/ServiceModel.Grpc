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
using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions.Reflection;

internal sealed class CodeAnalysisParameterInfo : IParameterInfo<ITypeSymbol>
{
    public CodeAnalysisParameterInfo(IParameterSymbol source)
    {
        Source = source;
    }

    public IParameterSymbol Source { get; }

    public string Name => Source.Name;

    public ITypeSymbol Type => Source.Type;

    public bool IsRefOrOut() => Source.IsOut() || Source.IsRef();
}