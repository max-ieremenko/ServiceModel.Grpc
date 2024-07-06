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

using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.Emit.Descriptions.Reflection;

internal sealed class ReflectType : IReflect<Type>
{
    public ICollection<Type> GetInterfaces(Type type) => ReflectionTools.ExpandInterface(type);

    public bool IsPublicNonGenericInterface(Type type) => !type.IsGenericTypeDefinition && ReflectionTools.IsPublicInterface(type);

    public bool TryGetCustomAttribute(Type owner, string attributeTypeFullName, [NotNullWhen(true)] out IAttributeInfo? attribute)
    {
        if (!ReflectionTools.TryGetCustomAttribute(owner, attributeTypeFullName, out var source))
        {
            attribute = null;
            return false;
        }

        attribute = new ReflectionAttributeInfo(source);
        return true;
    }

    public IMethodInfo<Type>[] GetMethods(Type type)
    {
        var methods = ReflectionTools.GetInstanceMethods(type);
        var result = new IMethodInfo<Type>[methods.Length];
        for (var i = 0; i < methods.Length; i++)
        {
            result[i] = new ReflectionMethodInfo(methods[i]);
        }

        return result;
    }

    public bool IsAssignableFrom(Type type, Type assignedTo) => type.IsAssignableFrom(assignedTo);

    public bool Equals(Type x, Type y) => x == y;

    public bool IsTaskOrValueTask(Type type) =>
        typeof(Task).IsAssignableFrom(type)
        || typeof(ValueTask) == type
        || (type.FullName ?? string.Empty).StartsWith(typeof(ValueTask<>).FullName, StringComparison.Ordinal);

    public Type[] GenericTypeArguments(Type type) => type.GenericTypeArguments;

    public bool IsAsyncEnumerable(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        return string.Equals(type.Namespace, typeof(IAsyncEnumerable<>).Namespace, StringComparison.Ordinal)
               && type.Name.Equals("IAsyncEnumerable`1", StringComparison.Ordinal);
    }

    public bool IsValueTuple(Type type) =>
        type.IsGenericType
        && type.Name.StartsWith(nameof(ValueTuple) + "`", StringComparison.Ordinal)
        && type.GenericTypeArguments.Length > 0
        && "System".Equals(type.Namespace, StringComparison.Ordinal);

    public string GetFullName(Type type)
    {
        var ns = ReflectionTools.GetNamespace(type);
        return string.IsNullOrEmpty(ns) ? type.Name : $"{ns}.{type.Name}";
    }

    public string GetName(Type type) => type.Name;

    public string GetSignature(IMethodInfo<Type> method) => ReflectionTools.GetSignature(((ReflectionMethodInfo)method).Source);

    public bool TryGetArrayInfo(Type type, [NotNullWhen(true)] out Type? elementType, out int rank)
    {
        if (!type.IsArray)
        {
            elementType = null;
            rank = 0;
            return false;
        }

        elementType = type.GetElementType();
        rank = type.GetArrayRank();
        return true;
    }
}