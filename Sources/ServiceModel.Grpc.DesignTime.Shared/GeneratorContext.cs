// <copyright>
// Copyright 2020-2021 Max Ieremenko
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    internal sealed class GeneratorContext
    {
        private readonly IExecutionContext _executionContext;
        private readonly HashSet<string> _generatedNames;

        public GeneratorContext(
            Compilation compilation,
            IExecutionContext executionContext)
        {
            _executionContext = executionContext;
            Compilation = compilation;
            _generatedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public CancellationToken CancellationToken => _executionContext.CancellationToken;

        public Compilation Compilation { get; }

        public void ReportDiagnostic(Diagnostic diagnostic) => _executionContext.ReportDiagnostic(diagnostic);

        public void AddOutput(ClassDeclarationSyntax node, string hintName, string source)
        {
            var fileName = GetOutputFileName(node, hintName);
            var sourceText = SourceText.From(source, Encoding.UTF8);
            _executionContext.AddSource(fileName, sourceText);
        }

        internal string GetOutputFileName(ClassDeclarationSyntax node, string hintName)
        {
            var name = new StringBuilder()
                .Append(node.Identifier.WithoutTrivia().ToString())
                .Append(".")
                .Append(hintName)
                .Replace('=', '-')
                .Replace('+', '-')
                .Replace('/', '-')
                .Replace('\\', '-')
                .Replace('<', '-')
                .Replace('>', '-')
                .Replace('{', '-')
                .Replace('}', '-')
                .ToString();

            var result = name;
            var index = 0;
            while (!_generatedNames.Add(result))
            {
                index++;
                result = name + index.ToString(CultureInfo.InvariantCulture);
            }

            return result + ".g.cs";
        }
    }
}
