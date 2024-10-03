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
 * https://github.com/dotnet/wcf/blob/ea7d491072cd598073173b372361a52b0f7aa5fc/src/System.ServiceModel.Primitives/src/System/ServiceModel/ServiceContractAttribute.cs
 */
namespace System.ServiceModel;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ServiceContractAttribute : Attribute
{
    public string? Name { get; set; }

    public string? Namespace { get; set; }
}