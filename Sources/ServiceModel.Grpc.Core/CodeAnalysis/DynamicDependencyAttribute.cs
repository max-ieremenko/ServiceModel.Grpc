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

/*
 * copy from:
 * https://github.com/dotnet/runtime/blob/7e429c2393a002065b641c3817fff62145c926db/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/DynamicDependencyAttribute.cs
 */
#if !NET6_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

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
#endif