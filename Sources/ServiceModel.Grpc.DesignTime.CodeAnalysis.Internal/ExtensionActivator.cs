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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

internal sealed class ExtensionActivator
{
    private readonly IExtensionContext _context;
    private readonly Dictionary<ITypeSymbol, Type> _typeBySymbol;

    public ExtensionActivator(IExtensionContext context, TypeHandler typeHandler)
    {
        _context = context;
        TypeHandler = typeHandler;
        _typeBySymbol = SyntaxTools.CreateTypeSymbolDictionary<Type>();
    }

    public TypeHandler TypeHandler { get; }

    public bool TryActivate(Type type, AttributeData attribute, [NotNullWhen(true)] out IExtensionProvider? provider)
    {
        try
        {
            provider = (IExtensionProvider)Activator.CreateInstance(type);
            return true;
        }
        catch (Exception ex)
        {
            _context.DebugLogger?.Log($"Try activate {type.FullName}: {ex}");
            _context.ReportExtensionActivationError(attribute, type, ex);

            provider = null;
            return false;
        }
    }

    public Type? TryResolveType(ITypeSymbol typeSymbol, AttributeData attribute)
    {
        if (_typeBySymbol.TryGetValue(typeSymbol, out var result))
        {
            return result;
        }

        var typeName = SyntaxTools.GetFullName(typeSymbol);
        try
        {
            var location = ResolveAssemblyPath(typeSymbol, _context.Compilation);
            var assembly = TypeHandler.GetAssembly(typeSymbol.ContainingAssembly.Name, location);
            result = assembly.GetType(typeName, throwOnError: true, ignoreCase: false);
        }
        catch (Exception ex)
        {
            _context.DebugLogger?.Log($"Try resolve type {typeName} from {typeSymbol.ContainingAssembly.Name}: {ex}");
            _context.ReportExtensionTypeError(attribute, typeName + ", " + typeSymbol.ContainingAssembly.Name, ex);
        }

        if (result != null)
        {
            _typeBySymbol.Add(typeSymbol, result);
        }

        return result;
    }

    internal static string ResolveAssemblyPath(ITypeSymbol typeSymbol, Compilation compilation)
    {
        var locations = typeSymbol.Locations;
        for (var i = 0; i < locations.Length; i++)
        {
            var location = locations[i];
            if (location.IsInSource)
            {
                throw new NotSupportedException("An extension must be defined in a separate project or package.");
            }
        }

        var reference = compilation.GetMetadataReference(typeSymbol.ContainingAssembly);
        if (reference is not PortableExecutableReference portable || portable.Properties.Kind != MetadataImageKind.Assembly)
        {
            throw new NotSupportedException("Metadata reference not found.");
        }

        if (string.IsNullOrEmpty(portable.FilePath) || !File.Exists(portable.FilePath))
        {
            throw new NotSupportedException($"Metadata reference file '{portable.FilePath}' not found.");
        }

        return portable.FilePath!;
    }
}