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

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ServiceModel.Grpc.DesignTime.Internal
{
    internal sealed class Logger
    {
        private readonly IProgress<Diagnostic> _progress;
        private readonly CSharpSyntaxNode _anchor;

        public Logger(IProgress<Diagnostic> progress, ClassDeclarationSyntax root, INamedTypeSymbol interfaceType)
        {
            _progress = progress;
            _anchor = FindAnchor(root, interfaceType);
        }

        public void Error(string message) => Log(DiagnosticSeverity.Error, message);

        public void ErrorFormat(string message, params object[] args) => Log(DiagnosticSeverity.Error, message.FormatWith(args));

        public void Warn(string message) => Log(DiagnosticSeverity.Warning, message);

        public void WarnFormat(string message, params object[] args) => Log(DiagnosticSeverity.Warning, message.FormatWith(args));

        public void Info(string message) => Log(DiagnosticSeverity.Info, message);

        public void InfoFormat(string message, params object[] args) => Log(DiagnosticSeverity.Info, message.FormatWith(args));

        private static CSharpSyntaxNode FindAnchor(ClassDeclarationSyntax root, INamedTypeSymbol interfaceType)
        {
            foreach (var attribute in root.AttributeLists)
            {
                var text = attribute.ToString();
                if (text.Contains("ImportGrpcService", StringComparison.Ordinal) && text.Contains(interfaceType.Name))
                {
                    return attribute;
                }
            }

            return root;
        }

        private void Log(DiagnosticSeverity severity, string message)
        {
            var location = _anchor.GetLocation();
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor("GrpcDesignTime", "GrpcDesignTime", message, "GrpcDesignTime", severity, false),
                Location.Create(_anchor.SyntaxTree.FilePath, location.SourceSpan, location.GetLineSpan().Span));

            _progress.Report(diagnostic);
        }
    }
}
