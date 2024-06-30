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

using System.Diagnostics.CodeAnalysis;

namespace ServiceModel.Grpc.Descriptions.Reflection;

internal interface IMethodInfo<TType>
{
    string Name { get; }

    IParameterInfo<TType>[] Parameters { get; }

    TType ReturnType { get; }

    bool HasGenericArguments();

    bool TryGetCustomAttribute(string attributeTypeFullName, [NotNullWhen(true)] out IAttributeInfo? attribute);

    bool TryGetReturnParameterCustomAttribute(string attributeTypeFullName, [NotNullWhen(true)] out IAttributeInfo? attribute);
}