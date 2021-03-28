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
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    // workaround custom dependencies of DesignTime.nupkg
    internal sealed class AssemblyResolver : IDisposable
    {
        private readonly string _dependenciesLocation;
        private readonly bool _canLockFile;

        public AssemblyResolver(GeneratorExecutionContext context)
        {
            (_dependenciesLocation, _canLockFile) = ResolveDependenciesLocation(context);
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
        }

        private static (string Location, bool CanLockFile) ResolveDependenciesLocation(GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.servicemodelgrpcdesigntime_dependencies", out var dependencies);
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.servicemodelgrpcdesigntime_localbuild", out var localBuild);

            if (string.IsNullOrEmpty(dependencies) || !Directory.Exists(dependencies))
            {
                throw new InvalidOperationException(string.Format("Dependencies not found in [{0}].", dependencies));
            }

            var canLockFile = string.IsNullOrWhiteSpace(localBuild) || !bool.Parse(localBuild!);
            return (dependencies!, canLockFile);
        }

        private Assembly? AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            return ResolveDependency(name.Name + ".dll");
        }

        private Assembly? ResolveDependency(string fileName)
        {
            var location = Path.Combine(_dependenciesLocation, fileName);
            if (!File.Exists(location))
            {
                return null;
            }

            if (_canLockFile)
            {
                return Assembly.LoadFrom(location);
            }

            return Assembly.Load(File.ReadAllBytes(location));
        }
    }
}