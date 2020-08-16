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

namespace ServiceModel.Grpc.DesignTime.Internal
{
    internal static class DiagnosticTools
    {
        public static void Error(
            this IProgress<Diagnostic> progress,
            CSharpSyntaxNode anchor,
            string message)
        {
            var location = anchor.GetLocation();
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor("GrpcDesignTime", "GrpcDesignTime", message, "GrpcDesignTime", DiagnosticSeverity.Error, false),
                Location.Create(anchor.SyntaxTree.FilePath, location.SourceSpan, location.GetLineSpan().Span));

            progress.Report(diagnostic);
        }
    }
}
