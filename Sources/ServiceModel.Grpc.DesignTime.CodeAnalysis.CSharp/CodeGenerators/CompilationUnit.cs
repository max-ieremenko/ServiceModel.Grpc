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

using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

public sealed class CompilationUnit : ICompilationUnit
{
    private readonly List<IDisposable> _indentation = new();

    public string FileExtension => ".g.cs";

    public void AddFileHeader(ICodeStringBuilder output)
    {
        AddComment(output);
        AddUsing(output);
        AddPragma(output);
    }

    public void BeginDeclaration(ICodeStringBuilder output, INamedTypeSymbol holder)
    {
        var stack = new List<ISymbol>(1);
        ISymbol? parent = holder;
        while (parent != null && parent.Kind == SymbolKind.NamedType)
        {
            stack.Add(parent);
            parent = parent.ContainingSymbol;
        }

        if (holder.ContainingNamespace != null && !holder.ContainingNamespace.IsGlobalNamespace)
        {
            output
                .Append("namespace ")
                .AppendLine(holder.ContainingNamespace.ToDisplayString())
                .AppendLine("{");
            _indentation.Add(output.Indent());
        }

        for (var i = stack.Count - 1; i >= 0; i--)
        {
            var symbol = stack[i];
            if (symbol.IsStatic)
            {
                output.Append("static ");
            }

            output
                .Append("partial class ")
                .AppendLine(symbol.Name)
                .AppendLine("{");
            _indentation.Add(output.Indent());
        }
    }

    public void EndDeclaration(ICodeStringBuilder output)
    {
        for (var i = 0; i < _indentation.Count; i++)
        {
            _indentation[i].Dispose();
            output.AppendLine("}");
        }

        _indentation.Clear();
    }

    private static void AddComment(ICodeStringBuilder output)
    {
        var comment = @"// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

"
            .Replace("\r\n", "\n")
            .Replace("\n", Environment.NewLine);

        output.Append(comment);
    }

    private static void AddUsing(ICodeStringBuilder output)
    {
        output.AppendLine("using System;");
        output.AppendLine("using System.Collections.Generic;");
        output.AppendLine("using System.Threading;");
        output.AppendLine("using System.Threading.Tasks;");
        output.AppendLine();
    }

    private static void AddPragma(ICodeStringBuilder output)
    {
        output
            .AppendLine("#pragma warning disable ServiceModelGrpcInternalAPI")
            .AppendLine();
    }
}