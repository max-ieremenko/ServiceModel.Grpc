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
 * https://github.com/dotnet/runtime/blob/7e429c2393a002065b641c3817fff62145c926db/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/ExperimentalAttribute.cs
 */
#if !NET8_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(
    AttributeTargets.Assembly |
    AttributeTargets.Module |
    AttributeTargets.Class |
    AttributeTargets.Struct |
    AttributeTargets.Enum |
    AttributeTargets.Constructor |
    AttributeTargets.Method |
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Event |
    AttributeTargets.Interface |
    AttributeTargets.Delegate,
    Inherited = false)]
internal sealed class ExperimentalAttribute : Attribute
{
    public ExperimentalAttribute(string diagnosticId)
    {
        DiagnosticId = diagnosticId;
    }

    public string DiagnosticId { get; }

    public string? UrlFormat { get; set; }
}
#endif