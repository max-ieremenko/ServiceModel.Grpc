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

using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ServiceModel.Grpc.DesignTime.Generators;

// workaround: custom dependencies of DesignTime.nupkg
internal sealed class AssemblyResolver : IDisposable
{
    private readonly DebugLogger? _logger;
    private readonly Dictionary<string, Assembly> _loadedByName;
    private readonly string _dependenciesLocation;
    private readonly bool _canLockFile;

    public AssemblyResolver(AnalyzerConfigOptions globalOptions, DebugLogger? logger)
    {
        _logger = logger;
        _dependenciesLocation = GetDependenciesLocation(globalOptions);
        _canLockFile = !IsLocalBuild(globalOptions);

        _loadedByName = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
    }

    public void Dispose() => AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;

    public Assembly Resolve(string assemblyName, string location)
    {
        if (_loadedByName.TryGetValue(assemblyName, out var result))
        {
            return result;
        }

        try
        {
            result = Load(location);
        }
        catch (Exception ex)
        {
            _logger?.Log($"AssemblyResolver: fail to load assembly {assemblyName} from {location}: {ex}");
            throw;
        }

        _logger?.Log($"AssemblyResolver: assembly {assemblyName} loaded from {location}");
        _loadedByName.Add(assemblyName, result);
        return result;
    }

    private static bool IsLocalBuild(AnalyzerConfigOptions globalOptions)
    {
        globalOptions.TryGetValue("build_property.servicemodelgrpcdesigntime_localbuild", out var localBuild);
        return string.IsNullOrWhiteSpace(localBuild) || bool.Parse(localBuild);
    }

    private static string GetDependenciesLocation(AnalyzerConfigOptions globalOptions)
    {
        globalOptions.TryGetValue("build_property.servicemodelgrpcdesigntime_dependencies", out var dependencies);

        if (string.IsNullOrEmpty(dependencies) || !Directory.Exists(dependencies))
        {
            throw new InvalidOperationException($"Dependencies not found in [{dependencies}].");
        }

        return dependencies!;
    }

    private static Assembly? FindInAppDomain(string assemblyName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (var i = 0; i < assemblies.Length; i++)
        {
            var assembly = assemblies[i];
            if (!assembly.IsDynamic
                && !assembly.ReflectionOnly
                && string.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                return assembly;
            }
        }

        return null;
    }

    private static bool CanSubstitute(AssemblyName assembly)
    {
        // System.IO.FileNotFoundException: Could not load file or assembly Microsoft.CodeAnalysis, Version=4.0.0.0
        // AppDomain contains Microsoft.CodeAnalysis, Version=4.9.0.0
        return string.Equals(assembly.Name, "Microsoft.CodeAnalysis", StringComparison.OrdinalIgnoreCase)
               && assembly.Version.Major >= 4;
    }

    private Assembly? AssemblyResolve(object sender, ResolveEventArgs args) => ResolveDependency(new AssemblyName(args.Name).Name);

    private Assembly? ResolveDependency(string assemblyName)
    {
        var location = Path.Combine(_dependenciesLocation, assemblyName + ".dll");
        if (File.Exists(location))
        {
            return Resolve(assemblyName, location);
        }

        var loaded = FindInAppDomain(assemblyName);
        if (loaded == null)
        {
            _logger?.Log($"AssemblyResolver: {assemblyName} not found in the dependencies folder and in the AppDomain");
            return null;
        }

        var name = loaded.GetName();
        if (CanSubstitute(name))
        {
            _logger?.Log($"AssemblyResolver: substitute {assemblyName} with {name}");
            return loaded;
        }

        _logger?.Log($"AssemblyResolver: request for {assemblyName}, ignore loaded in the AppDomain {name}");
        return null;
    }

    private Assembly Load(string location) => _canLockFile ? Assembly.LoadFrom(location) : Assembly.Load(File.ReadAllBytes(location));
}