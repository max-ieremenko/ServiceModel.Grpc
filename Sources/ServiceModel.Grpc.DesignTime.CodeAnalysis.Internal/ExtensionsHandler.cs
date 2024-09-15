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

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

public sealed class ExtensionsHandler
{
    private readonly SourceProductionContext _context;
    private readonly ICompilationUnit _compilationUnit;
    private readonly ExtensionContext _extensionContext;
    private readonly ExtensionActivator _extensionActivator;
    private readonly HashSet<string> _generatedNames;

    public ExtensionsHandler(
        SourceProductionContext context,
        IDebugLogger? logger,
        Compilation compilation,
        TypeHandler typeHandler,
        ICompilationUnit compilationUnit)
    {
        _context = context;
        _compilationUnit = compilationUnit;

        _extensionContext = new ExtensionContext(context, compilation, logger);
        _extensionActivator = new ExtensionActivator(_extensionContext, typeHandler);
        _generatedNames = new(StringComparer.OrdinalIgnoreCase);
    }

    public void Execute(ImmutableArray<SyntaxNode> candidates)
    {
        // same class defined 2 times
        // [attribute1] partial class Holder (candidate1)
        // [attribute2] partial class Holder (candidate2)
        var holders = SyntaxTools.CreateNamedTypeHashSet();

        for (var i = 0; i < candidates.Length; i++)
        {
            var candidate = candidates[i];
            var holder = _extensionContext
                .Compilation
                .GetSemanticModel(candidate.SyntaxTree)
                .GetDeclaredSymbol(candidate, _extensionContext.CancellationToken);

            if (holder is INamedTypeSymbol symbol)
            {
                holders.Add(symbol);
            }
        }

        // the final file name can be non-unique, sort to have same indexes
        foreach (var holder in holders.OrderBy(i => i.Name))
        {
            var extensions = CollectExtensions(holder);
            if (extensions != null)
            {
                ExecuteGroup(holder, extensions);
            }
        }
    }

    private ExtensionCollection? CollectExtensions(INamedTypeSymbol holder)
    {
        var resolved = new List<(AttributeData Attribute, IExtensionProvider Provider)>();

        var attributes = holder.GetAttributes();
        for (var i = 0; i < attributes.Length; i++)
        {
            var attribute = attributes[i];
            if (!_extensionActivator.TypeHandler.TryGetProviderType(attribute, out var typeSymbol, out var type))
            {
                continue;
            }

            if (typeSymbol != null)
            {
                type = _extensionActivator.TryResolveType(typeSymbol, attribute);
            }

            if (type == null || !_extensionActivator.TryActivate(type, attribute, out var provider))
            {
                // something went wrong => do nothing
                return null;
            }

            resolved.Add((attribute, provider));
        }

        if (resolved.Count == 0)
        {
            return null;
        }

        var result = new ExtensionCollection();
        for (var i = 0; i < resolved.Count; i++)
        {
            var item = resolved[i];
            item.Provider.ProvideExtensions(new ExtensionProviderDeclaration(holder, item.Attribute), result, _extensionContext);
        }

        return result.Count == 0 ? null : result;
    }

    private void ExecuteGroup(INamedTypeSymbol holder, List<IExtension> extensions)
    {
        var descriptions = new ContractDescriptionCollection();
        var generators = new CodeGeneratorCollection();
        for (var i = 0; i < extensions.Count; i++)
        {
            if (extensions[i] is IContractDescriptionExtension description)
            {
                description.ProvideContractDescriptions(descriptions, _extensionContext);
            }
            else if (extensions[i] is IMetadataExtension metadata)
            {
                generators.AddMetadata(metadata);
            }
        }

        if (descriptions.Count == 0 || _extensionContext.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        for (var i = 0; i < extensions.Count; i++)
        {
            if (extensions[i] is ICodeGeneratorExtension extension)
            {
                extension.ProvideGenerators(generators, descriptions, _extensionContext);
            }
        }

        if (generators.Count == 0 || _extensionContext.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        // Holder.Client.cs
        // Holder.Server.cs
        var byHintName = generators.GroupBy(i => i.GetHintName(), StringComparer.OrdinalIgnoreCase);
        foreach (var group in byHintName)
        {
            if (_extensionContext.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            Generate(holder, group.Key, group);
        }
    }

    private void Generate(INamedTypeSymbol holder, string hintName, IEnumerable<ICodeGenerator> generators)
    {
        var generateCounter = 0;
        var output = new CodeStringBuilder();

        foreach (var generator in generators)
        {
            if (_extensionContext.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (generateCounter == 0)
            {
                _compilationUnit.AddFileHeader(output);
                _compilationUnit.BeginDeclaration(output, holder);
            }
            else
            {
                output.AppendLine();
            }

            generator.Generate(output);
            generateCounter++;
        }

        _extensionContext.CancellationToken.ThrowIfCancellationRequested();
        if (generateCounter > 0)
        {
            _compilationUnit.EndDeclaration(output);
            var source = output.Clear();
            AddOutput(holder.Name, hintName, source);
        }
    }

    private void AddOutput(string holderName, string hintName, string source)
    {
        var fileName = GetOutputFileName(holderName, hintName);
        var sourceText = SourceText.From(source, Encoding.UTF8);
        _extensionContext.DebugLogger?.LogSource(fileName, source);
        _context.AddSource(fileName, sourceText);
    }

    private string GetOutputFileName(string holderName, string hintName)
    {
        var name = new StringBuilder()
            .Append(holderName)
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

        return result + _compilationUnit.FileExtension;
    }
}