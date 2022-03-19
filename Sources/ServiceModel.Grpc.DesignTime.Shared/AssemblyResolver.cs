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

using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    // workaround: custom dependencies of DesignTime.nupkg
    internal sealed class AssemblyResolver : IDisposable
    {
        private readonly string _dependenciesLocation;
        private readonly bool _canLockFile;

        public AssemblyResolver(AnalyzerConfigOptions globalOptions)
        {
            _dependenciesLocation = GetDependenciesLocation(globalOptions);
            _canLockFile = !IsLocalBuild(globalOptions);
        }

        public void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
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
                throw new InvalidOperationException(string.Format("Dependencies not found in [{0}].", dependencies));
            }

            return dependencies!;
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