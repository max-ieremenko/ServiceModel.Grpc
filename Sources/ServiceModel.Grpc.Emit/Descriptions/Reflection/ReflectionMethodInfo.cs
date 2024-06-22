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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.Emit.Descriptions.Reflection;

internal sealed class ReflectionMethodInfo : IMethodInfo<Type>
{
    public ReflectionMethodInfo(MethodInfo source)
    {
        Source = source;

        var parameters = source.GetParameters();
        Parameters = new IParameterInfo<Type>[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            Parameters[i] = new ReflectionParameterInfo(parameters[i]);
        }
    }

    public MethodInfo Source { get; }

    public string Name => Source.Name;

    public IParameterInfo<Type>[] Parameters { get; }

    public Type ReturnType => Source.ReturnType;

    public bool HasGenericArguments() => Source.GetGenericArguments().Length > 0;

    public bool TryGetCustomAttribute(string attributeTypeFullName, [NotNullWhen(true)] out IAttributeInfo? attribute)
    {
        if (!ReflectionTools.TryGetCustomAttribute(Source, attributeTypeFullName, out var source))
        {
            attribute = null;
            return false;
        }

        attribute = new ReflectionAttributeInfo(source);
        return true;
    }

    public bool TryGetReturnParameterCustomAttribute(string attributeTypeFullName, [NotNullWhen(true)] out IAttributeInfo? attribute)
    {
        if (!ReflectionTools.TryGetCustomAttribute(Source.ReturnParameter!, attributeTypeFullName, out var source))
        {
            attribute = null;
            return false;
        }

        attribute = new ReflectionAttributeInfo(source);
        return true;
    }
}