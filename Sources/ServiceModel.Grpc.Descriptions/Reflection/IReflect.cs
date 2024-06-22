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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ServiceModel.Grpc.Descriptions.Reflection;

public interface IReflect<TType>
{
    ICollection<TType> GetInterfaces(TType type);

    bool IsPublicNonGenericInterface(TType type);

    bool TryGetCustomAttribute(TType owner, string attributeTypeFullName, [NotNullWhen(true)] out IAttributeInfo? attribute);

    IMethodInfo<TType>[] GetMethods(TType type);

    bool IsAssignableFrom(TType type, TType assignedTo);

    bool IsAssignableFrom(TType type, Type assignedTo);

    bool Equals(TType x, TType y);

    bool IsTaskOrValueTask(TType type);

    TType[] GenericTypeArguments(TType type);

    bool IsAsyncEnumerable(TType type);

    bool IsValueTuple(TType type);

    string GetFullName(TType type);

    string GetName(TType type);

    string GetSignature(IMethodInfo<TType> method);

    bool TryGetArrayInfo(TType type, [NotNullWhen(true)] out TType? elementType, out int rank);
}