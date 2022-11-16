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

using System.Linq;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal;

internal sealed class MethodDescription
{
    public MethodDescription(IMethodSymbol source)
    {
        Source = source;
        Name = source.Name;
        ReturnType = SyntaxTools.GetFullName(source.ReturnType);
        ReturnTypeSymbol = source.ReturnType;
        TypeArguments = source.TypeArguments.Select(SyntaxTools.GetFullName).ToArray();
        Parameters = source.Parameters.Select(i => new ParameterDescription(i)).ToArray();
    }

    public IMethodSymbol Source { get; }

    public string Name { get; }

    public string ReturnType { get; }

    public ITypeSymbol ReturnTypeSymbol { get; }

    public string[] TypeArguments { get; }

    public ParameterDescription[] Parameters { get; }
}