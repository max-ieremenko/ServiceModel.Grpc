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
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

internal sealed class ExtensionContext : IExtensionContext
{
    private readonly SourceProductionContext _context;

    public ExtensionContext(SourceProductionContext context, Compilation compilation, IDebugLogger? debugLogger)
    {
        _context = context;
        Compilation = compilation;
        DebugLogger = debugLogger;
        DescriptionExtensions = new DescriptionExtensions();
    }

    public CancellationToken CancellationToken => _context.CancellationToken;

    public Compilation Compilation { get; }

    public IDebugLogger? DebugLogger { get; }

    public IDescriptionExtensions DescriptionExtensions { get; }

    public void ReportDiagnostic(Diagnostic diagnostic) => _context.ReportDiagnostic(diagnostic);
}