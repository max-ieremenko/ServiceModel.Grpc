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

using System.Reflection;
using System.Reflection.Emit;

namespace ServiceModel.Grpc.Emit;

internal static partial class ReflectionTools
{
    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetInterfaces")]
    public static ICollection<Type> ExpandInterface(Type type)
    {
        var result = new HashSet<Type>();

        if (IsPublicInterface(type))
        {
            result.Add(type);
        }

        foreach (var i in type.GetInterfaces().Where(IsPublicInterface))
        {
            result.Add(i);
        }

        return result;
    }

    public static string? GetNamespace(Type type)
    {
        var @namespace = type.Namespace;
        if (type.IsNested && type.DeclaringType != null)
        {
            if (string.IsNullOrEmpty(@namespace))
            {
                @namespace = type.DeclaringType.Name;
            }
            else
            {
                @namespace += "." + type.DeclaringType.Name;
            }
        }

        return @namespace;
    }

    public static bool IsTask(Type type) => typeof(Task).IsAssignableFrom(type) || IsValueTask(type);

    public static bool IsValueTask(this Type type) =>
        typeof(ValueTask) == type
        || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>));

    public static bool IsAsyncEnumerable(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        return string.Equals(type.Namespace, typeof(IAsyncEnumerable<>).Namespace, StringComparison.Ordinal)
               && type.Name.Equals("IAsyncEnumerable`1", StringComparison.Ordinal);
    }

    public static bool IsValueTuple(Type type)
    {
        return type.IsGenericType
               && type.Name.StartsWith(nameof(ValueTuple) + "`", StringComparison.Ordinal)
               && type.GenericTypeArguments.Length > 0
               && "System".Equals(type.Namespace, StringComparison.Ordinal);
    }

    public static bool IsOut(this ParameterInfo parameter) => parameter.IsOut;

    public static bool IsRef(this ParameterInfo parameter) => parameter.ParameterType.Name.EndsWith("&", StringComparison.Ordinal);

    public static bool TryGetCustomAttribute(MemberInfo owner, string attributeTypeFullName, [NotNullWhen(true)] out Attribute? attribute)
    {
        foreach (var candidate in owner.GetCustomAttributes())
        {
            if (string.Equals(attributeTypeFullName, candidate.GetType().FullName, StringComparison.Ordinal))
            {
                attribute = candidate;
                return true;
            }
        }

        attribute = null;
        return false;
    }

    public static bool TryGetCustomAttribute(ParameterInfo owner, string attributeTypeFullName, [NotNullWhen(true)] out Attribute? attribute)
    {
        foreach (var candidate in owner.GetCustomAttributes())
        {
            if (string.Equals(attributeTypeFullName, candidate.GetType().FullName, StringComparison.Ordinal))
            {
                attribute = candidate;
                return true;
            }
        }

        attribute = null;
        return false;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetConstructor")]
    public static ConstructorInfo Constructor(this Type type, params Type[] parameters)
    {
        var result = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            parameters,
            null);

        if (result == null)
        {
            var ctor = string.Join(",", parameters.Select(i => i.Name));
            throw new ArgumentOutOfRangeException($"{type.Name} has no .ctor ({ctor}).");
        }

        return result;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetConstructors")]
    public static ConstructorInfo Constructor(this Type type, int parametersCount)
    {
        var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        ConstructorInfo? result = null;
        for (var i = 0; i < constructors.Length; i++)
        {
            var ctor = constructors[i];
            if (ctor.GetParameters().Length == parametersCount)
            {
                if (result != null)
                {
                    throw new ArgumentException($"{type.Name} contains too many ctors with {parametersCount} parameters.");
                }

                result = ctor;
            }
        }

        if (result == null)
        {
            throw new ArgumentException($"{type.Name} does not contain ctor with {parametersCount} parameters.");
        }

        return result;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetProperty")]
    public static PropertyInfo? TryInstanceProperty(this Type type, string name)
    {
        return type.GetProperty(
            name,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }

    public static PropertyInfo InstanceProperty(this Type type, string name) =>
        TryInstanceProperty(type, name) ?? throw new ArgumentOutOfRangeException($"{type.Name} does not implement instance property {name}.");

    public static MethodInfo SafeGetGetMethod(this PropertyInfo property) =>
        property.GetMethod ?? throw new ArgumentOutOfRangeException($"{property.Name} does not implement get method.");

    public static MethodInfo SafeGetSetMethod(this PropertyInfo property) =>
        property.SetMethod ?? throw new ArgumentOutOfRangeException($"{property.Name} does not implement set method.");

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetMethod")]
    public static MethodInfo InstanceMethod(this Type type, string name)
    {
        var result = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (result == null)
        {
            throw new ArgumentOutOfRangeException($"{type.Name} does not implement method {name}.");
        }

        return result;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetMethod")]
    public static MethodInfo InstanceMethod(this Type type, string name, params Type[] parameters)
    {
        var result = type.GetMethod(
            name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
            null,
            parameters,
            null);

        if (result == null)
        {
            throw new ArgumentOutOfRangeException($"{type.Name} does not implement method {name}.");
        }

        return result;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetMethods")]
    public static MethodInfo[] GetInstanceMethods(Type type) =>
        type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetField")]
    public static FieldInfo InstanceFiled(this Type type, string name)
    {
        var result = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (result == null)
        {
            throw new ArgumentOutOfRangeException($"{type.Name} does not have instance field {name}.");
        }

        return result;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetField")]
    public static FieldInfo StaticFiled(this Type type, string name)
    {
        var result = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

        if (result == null)
        {
            throw new ArgumentOutOfRangeException($"{type.Name} does not have static field {name}.");
        }

        return result;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetMethod")]
    public static MethodInfo StaticMethod(this Type type, string name)
    {
        var result = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

        if (result == null)
        {
            throw new ArgumentOutOfRangeException($"{type.Name} does not implement static method {name}.");
        }

        return result;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetMethod")]
    public static MethodInfo StaticMethod(this Type type, string name, params Type[] parameters)
    {
        var result = type.GetMethod(
            name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly,
            null,
            parameters,
            null);

        if (result == null)
        {
            throw new ArgumentOutOfRangeException($"{type.Name} does not implement static method {name}.");
        }

        return result;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:Type.GetMethods")]
    public static MethodInfo StaticMethodByReturnType(this Type type, string nameStartWith, Type returnType)
    {
        var result = type
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(i => i.Name.StartsWith(nameStartWith, StringComparison.Ordinal))
            .FirstOrDefault(i => i.ReturnType == returnType);

        if (result == null)
        {
            throw new ArgumentOutOfRangeException($"{type.Name} does not implement method {nameStartWith}*.");
        }

        return result;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2067:Type.GetInterfaceMap")]
    public static MethodInfo ImplementationOfMethod(Type instance, Type methodDeclaringType, MethodInfo method)
    {
        var map = instance.GetInterfaceMap(methodDeclaringType);
        for (var i = 0; i < map.InterfaceMethods.Length; i++)
        {
            if (map.InterfaceMethods[i].Equals(method))
            {
                return map.TargetMethods[i];
            }
        }

        throw new ArgumentOutOfRangeException($"Implementation of method {methodDeclaringType.Name}.{method.Name} not found in {instance.FullName}.");
    }

    public static string GetSignature(MethodInfo method)
    {
        var result = new StringBuilder()
            .Append(method.ReturnType.GetUserFriendlyName())
            .Append(' ');

        var ns = method.DeclaringType == null ? null : GetNamespace(method.DeclaringType);
        if (!string.IsNullOrEmpty(ns))
        {
            result.Append(ns).Append('.');
        }

        result.Append(method.Name).Append('(');

        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
            {
                result.Append(", ");
            }

            var p = parameters[i];

            if (p.IsOut())
            {
                result.Append("out ");
            }
            else if (p.IsRef())
            {
                result.Append("ref ");
            }

            result.Append(p.ParameterType.GetUserFriendlyName());
        }

        result.Append(')');
        return result.ToString();
    }

    public static bool IsPublicInterface(Type type) => type.IsInterface && (type.IsPublic || type.IsNestedPublic);

    public static Attribute? GetCustomAttribute(MemberInfo owner, string attributeTypeFullName)
    {
        return owner
            .GetCustomAttributes()
            .FirstOrDefault(i => string.Equals(attributeTypeFullName, i.GetType().FullName, StringComparison.Ordinal));
    }

    public static TDelegate CreateDelegate<TDelegate>(this MethodInfo method, object? target = default)
        where TDelegate : Delegate
    {
        var result = target == null ? method.CreateDelegate(typeof(TDelegate)) : method.CreateDelegate(typeof(TDelegate), target);
        return (TDelegate)result;
    }

    public static string GetUserFriendlyName(this Type type)
    {
        return new TypeUserFriendlyBuilder(type).Build();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:System.Reflection.Assembly.GetType")]
    public static Type SafeGetType(this Assembly assembly, string name) =>
        assembly.GetType(name, true, false) ?? throw new ArgumentOutOfRangeException($"Type {name} not found in {assembly.FullName}.");

    [UnconditionalSuppressMessage("Trimming", "IL2026:System.Reflection.Module.GetType")]
    public static bool TryGetType(this ModuleBuilder moduleBuilder, string name, [NotNullWhen(true)] out Type? type)
    {
        type = moduleBuilder.GetType(name, false, false);
        return type != null;
    }

    public static Type SafeGetArrayElementType(this Type arrayType)
    {
        if (!arrayType.IsArray)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayType));
        }

        return arrayType.GetElementType() ?? throw new InvalidOperationException($"Array {arrayType.FullName ?? arrayType.Name}.GetElementType() is null.");
    }

    [UnconditionalSuppressMessage("Trimming", "IL2060:MethodInfo.MakeGenericMethod")]
    [UnconditionalSuppressMessage("AOT", "IL3050:MethodInfo.MakeGenericMethod")]
    public static MethodInfo MakeConstructedGeneric(this MethodInfo method, params Type[] typeArguments) => method.MakeGenericMethod(typeArguments);

    [UnconditionalSuppressMessage("Trimming", "IL2055:Type.MakeGenericType")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Type.MakeGenericType")]
    public static Type MakeConstructedGeneric(this Type type, params Type[] typeArguments) => type.MakeGenericType(typeArguments);
}