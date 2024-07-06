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

namespace System.Diagnostics.CodeAnalysis;

[Flags]
internal enum DynamicallyAccessedMemberTypes
{
    None = 0,
    PublicParameterlessConstructor = 1,
    PublicConstructors = 3,
    NonPublicConstructors = 4,
    PublicMethods = 8,
    NonPublicMethods = 16, // 0x00000010
    PublicFields = 32, // 0x00000020
    NonPublicFields = 64, // 0x00000040
    PublicNestedTypes = 128, // 0x00000080
    NonPublicNestedTypes = 256, // 0x00000100
    PublicProperties = 512, // 0x00000200
    NonPublicProperties = 1024, // 0x00000400
    PublicEvents = 2048, // 0x00000800
    NonPublicEvents = 4096, // 0x00001000
    Interfaces = 8192, // 0x00002000
    All = -1, // 0xFFFFFFFF
}

// https://github.com/dotnet/runtime/issues/36656
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
internal sealed class DynamicDependencyAttribute : Attribute
{
    public DynamicDependencyAttribute(string memberSignature)
    {
        MemberSignature = memberSignature;
    }

    public DynamicDependencyAttribute(string memberSignature, Type type)
    {
        MemberSignature = memberSignature;
        Type = type;
    }

    public DynamicDependencyAttribute(string memberSignature, string typeName, string assemblyName)
    {
        MemberSignature = memberSignature;
        TypeName = typeName;
        AssemblyName = assemblyName;
    }

    public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, Type type)
    {
        MemberTypes = memberTypes;
        Type = type;
    }

    public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, string typeName, string assemblyName)
    {
        MemberTypes = memberTypes;
        TypeName = typeName;
        AssemblyName = assemblyName;
    }

    public string? MemberSignature { get; }

    public DynamicallyAccessedMemberTypes MemberTypes { get; }

    public Type? Type { get; }

    public string? TypeName { get; }

    public string? AssemblyName { get; }

    public string? Condition { get; set; }
}