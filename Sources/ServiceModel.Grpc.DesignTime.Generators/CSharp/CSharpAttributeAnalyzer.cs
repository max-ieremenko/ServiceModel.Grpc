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

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp;

namespace ServiceModel.Grpc.DesignTime.Generators.CSharp;

internal static class CSharpAttributeAnalyzer
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool DoesLookLikeExpandable(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax owner)
        {
            return false;
        }

        var attributeLists = owner.AttributeLists;
        for (var i = 0; i < attributeLists.Count; i++)
        {
            var attributes = attributeLists[i].Attributes;
            for (var j = 0; j < attributes.Count; j++)
            {
                var name = attributes[j].Name.ToString();
                if (name.IndexOf(AttributeAnalyzer.ImportAttributeName, StringComparison.Ordinal) >= 0
                    || name.IndexOf(AttributeAnalyzer.ExportAttributeName, StringComparison.Ordinal) >= 0
                    || name.IndexOf(AttributeAnalyzer.ExtensionAttributeName, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}