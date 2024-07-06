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

internal sealed class CodeAnalysisMethodInfo : IMethodInfo<ITypeSymbol>
{
    public CodeAnalysisMethodInfo(IMethodSymbol source)
    {
        Source = source;

        var parameters = source.Parameters;
        Parameters = new IParameterInfo<ITypeSymbol>[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            Parameters[i] = new CodeAnalysisParameterInfo(parameters[i]);
        }
    }

    public IMethodSymbol Source { get; }

    public string Name => Source.Name;

    public IParameterInfo<ITypeSymbol>[] Parameters { get; }

    public ITypeSymbol ReturnType => Source.ReturnType;

    public bool HasGenericArguments() => Source.TypeArguments.Length > 0;

    public bool TryGetCustomAttribute(string attributeTypeFullName, [NotNullWhen(true)] out IAttributeInfo? attribute)
    {
        var source = SyntaxTools.GetCustomAttribute(Source, attributeTypeFullName);
        if (source == null)
        {
            attribute = null;
            return false;
        }

        attribute = new CodeAnalysisAttributeInfo(source);
        return true;
    }

    public bool TryGetReturnParameterCustomAttribute(string attributeTypeFullName, [NotNullWhen(true)] out IAttributeInfo? attribute)
    {
        throw new NotSupportedException();
    }
}