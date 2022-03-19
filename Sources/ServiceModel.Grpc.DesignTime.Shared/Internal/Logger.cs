// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
        private readonly ClassDeclarationSyntax _root;
        private readonly AttributeData _attribute;
        private Location? _location;

        public Logger(GeneratorContext context, ClassDeclarationSyntax root, AttributeData attribute)
        {
            _context = context;
            _root = root;
            _attribute = attribute;
        }

        public void IsNotServiceContract(INamedTypeSymbol serviceType)
        {
            Report(new DiagnosticDescriptor(
                "GrpcDesignTime01",
                "GrpcDesignTime",
                "{0} is not service contract.".FormatWith(serviceType.Name),
                "GrpcDesignTime",
                DiagnosticSeverity.Error,
                true));
        }

        public void InheritsNotServiceContract(INamedTypeSymbol serviceType, INamedTypeSymbol parent)
        {
            Report(new DiagnosticDescriptor(
                "GrpcDesignTime02",
                "GrpcDesignTime",
                "{0}: {1} is not service contract.".FormatWith(serviceType.Name, parent.Name),
                "GrpcDesignTime",
                DiagnosticSeverity.Info,
                true));
        }

        public void IsNotOperationContract(INamedTypeSymbol serviceType, string error)
        {
            Report(new DiagnosticDescriptor(
                "GrpcDesignTime03",
                "GrpcDesignTime",
                "{0}: {1}".FormatWith(serviceType.Name, error),
                "GrpcDesignTime",
                DiagnosticSeverity.Info,
                true));
        }

        public void IsNotSupportedOperation(INamedTypeSymbol serviceType, string error)
        {
            Report(new DiagnosticDescriptor(
                "GrpcDesignTime04",
                "GrpcDesignTime",
                "{0}: {1}".FormatWith(serviceType.Name, error),
                "GrpcDesignTime",
                DiagnosticSeverity.Warning,
                true));
        }

        private void Report(DiagnosticDescriptor descriptor)
        {
            if (_location == null)
            {
                var location = _attribute.ApplicationSyntaxReference!.GetSyntax().GetLocation();
                _location = Location.Create(_root.SyntaxTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
            }

            var diagnostic = Diagnostic.Create(descriptor, _location);

            _context.ReportDiagnostic(diagnostic);
        }
    }
}
