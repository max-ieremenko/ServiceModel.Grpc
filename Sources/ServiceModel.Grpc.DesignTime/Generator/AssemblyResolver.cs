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

namespace ServiceModel.Grpc.DesignTime.Generator
{
    // workaround custom dependencies of DesignTime.nupkg
    internal sealed class AssemblyResolver : IDisposable
    {
        private readonly string _dependenciesLocation;

        public AssemblyResolver()
        {
            var root = Path.GetDirectoryName(GetType().Assembly.Location)!;
            var location = Path.Combine(root, "dependencies");
            if (!Directory.Exists(location))
            {
                location = Path.GetFullPath(Path.Combine(
                    root,
                    string.Format("..{0}..{0}..{0}build{0}dependencies", Path.DirectorySeparatorChar)));
            }

            _dependenciesLocation = location;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
        }

        private Assembly? AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            return ResolveDependency(name.Name + ".dll");
        }

        private Assembly? ResolveDependency(string fileName)
        {
            var location = Path.Combine(_dependenciesLocation, fileName);
            if (File.Exists(location))
            {
                return Assembly.LoadFrom(location);
            }

            return null;
        }
    }
}