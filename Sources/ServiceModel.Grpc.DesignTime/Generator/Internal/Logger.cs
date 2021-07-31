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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal
{
    internal sealed class Logger
    {
        private readonly GeneratorContext _context;
        private readonly Location _location;

        public Logger(GeneratorContext context, ClassDeclarationSyntax root, AttributeData attribute)
        {
            _context = context;

            var location = attribute.ApplicationSyntaxReference!.GetSyntax().GetLocation();
            _location = Location.Create(root.SyntaxTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
        }

        public void Error(string message) => Log(DiagnosticSeverity.Error, message);

        public void ErrorFormat(string message, params object[] args) => Log(DiagnosticSeverity.Error, message.FormatWith(args));

        public void Warn(string message) => Log(DiagnosticSeverity.Warning, message);

        public void WarnFormat(string message, params object[] args) => Log(DiagnosticSeverity.Warning, message.FormatWith(args));

        public void Info(string message) => Log(DiagnosticSeverity.Info, message);

        public void InfoFormat(string message, params object[] args) => Log(DiagnosticSeverity.Info, message.FormatWith(args));

        private void Log(DiagnosticSeverity severity, string message)
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor("GrpcDesignTime", "GrpcDesignTime", message, "GrpcDesignTime", severity, false),
                _location);

            _context.ReportDiagnostic(diagnostic);
        }
    }
}
