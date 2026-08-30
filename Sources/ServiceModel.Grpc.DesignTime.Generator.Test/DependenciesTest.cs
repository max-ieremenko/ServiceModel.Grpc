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

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using NUnit.Framework;

namespace ServiceModel.Grpc.DesignTime.Generator.Test;

// https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/10.0/asyncenumerable
[TestFixture]
public class DependenciesTest
{
    [Test(Description = "#423: compile with VS2022")]
    public void System_Memory()
    {
        var versions = Collect("System.Memory");
        versions.ShouldBe([new("4.0.1.1")]);
    }

    private static HashSet<Version> Collect(string referenceName)
    {
        const string prefix = "ServiceModel.Grpc.DesignTime";

        var queue = new Queue<string>();
        queue.Enqueue($"{prefix}.Generators");

        var result = new HashSet<Version>();
        var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (queue.TryDequeue(out var next))
        {
            if (!processed.Add(next))
            {
                continue;
            }

            foreach (var reference in GetReferences($"{next}.dll"))
            {
                if (reference.Name.Equals(referenceName, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(reference.Version);
                    continue;
                }

                if (reference.Name.Contains(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    queue.Enqueue(reference.Name);
                }
            }
        }

        return result;
    }

    private static IEnumerable<(string Name, Version Version)> GetReferences(string peFile)
    {
        var path = Path.Combine(AppContext.BaseDirectory, peFile);
        if (!File.Exists(path))
        {
            yield break;
        }

        using var stream = File.OpenRead(path);
        using var reader = new PEReader(stream);

        var metadataReader = reader.GetMetadataReader();
        foreach (var item in metadataReader.AssemblyReferences)
        {
            var reference = metadataReader.GetAssemblyReference(item);
            var name = metadataReader.GetString(reference.Name);
            yield return (name, reference.Version);
        }
    }
}