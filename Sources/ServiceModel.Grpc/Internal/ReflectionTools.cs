// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.Internal
{
    internal static partial class ReflectionTools
    {
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

        public static string GetNonGenericName(Type type)
        {
            var name = type.Name;
            if (!type.IsGenericType)
            {
                return name;
            }

            var index = name.IndexOf('`');
            return name.Substring(0, index);
        }

        public static string GetNamespace(Type type)
        {
            var @namespace = type.Namespace;
            if (type.IsNested && type.DeclaringType != null)
            {
                @namespace += "." + type.DeclaringType.Name;
            }

            return @namespace;
        }

        public static IList<MethodInfo> GetMethods(Type type)
        {
            return type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public static bool IsTask(Type type)
        {
            return typeof(Task).IsAssignableFrom(type) || IsValueTask(type);
        }

        public static bool IsValueTask(this Type type)
        {
            return typeof(ValueTask) == type
                   || (type.FullName ?? string.Empty).StartsWith(typeof(ValueTask<>).FullName, StringComparison.Ordinal);
        }

        public static bool IsStream(Type type) => typeof(Stream).IsAssignableFrom(type);

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

        public static ConstructorInfo Constructor(this Type type, params Type[] parameters)
        {
            var result = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                parameters,
                null);

            if (result == null)
            {
                throw new ArgumentOutOfRangeException("{0} has no .ctor ({1}).".FormatWith(
                    type.Name,
                    string.Join(",", parameters.Select(i => i.Name))));
            }

            return result;
        }

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
                        throw new ArgumentException("{0} contains too many ctors with {1} parameters.".FormatWith(type.Name, parametersCount));
                    }

                    result = ctor;
                }
            }

            if (result == null)
            {
                throw new ArgumentException("{0} does not contain ctor with {1} parameters.".FormatWith(type.Name, parametersCount));
            }

            return result;
        }

        public static PropertyInfo? TryInstanceProperty(this Type type, string name)
        {
            return type.GetProperty(
                name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public static PropertyInfo InstanceProperty(this Type type, string name)
        {
            var result = TryInstanceProperty(type, name);

            if (result == null)
            {
                throw new ArgumentOutOfRangeException("{0} does not implement instance property {1}.".FormatWith(type.Name, name));
            }

            return result;
        }

        public static MethodInfo InstanceMethod(this Type type, string name)
        {
            var result = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (result == null)
            {
                throw new ArgumentOutOfRangeException("{0} does not implement method {1}.".FormatWith(type.Name, name));
            }

            return result;
        }

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
                throw new ArgumentOutOfRangeException("{0} does not implement method {1}.".FormatWith(type.Name, name));
            }

            return result;
        }

        public static MethodInfo InstanceGenericMethod(this Type type, string name, int genericArgsCount)
        {
            var candidates = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            MethodInfo? result = null;
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (!name.Equals(candidate.Name, StringComparison.Ordinal)
                    || candidate.GetGenericArguments().Length != genericArgsCount)
                {
                    continue;
                }

                if (result != null)
                {
                    throw new ArgumentOutOfRangeException("{0} implements too many methods {1} with {2} generic arguments.".FormatWith(type.Name, name, genericArgsCount));
                }

                result = candidate;
            }

            if (result == null)
            {
                throw new ArgumentOutOfRangeException("{0} does not implement method {1} with {2} generic arguments.".FormatWith(type.Name, name, genericArgsCount));
            }

            return result;
        }

        public static FieldInfo InstanceFiled(this Type type, string name)
        {
            var result = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (result == null)
            {
                throw new ArgumentOutOfRangeException("{0} does not have instance field {1}.".FormatWith(type.Name, name));
            }

            return result;
        }

        public static MethodInfo StaticMethod(this Type type, string name)
        {
            var result = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

            if (result == null)
            {
                throw new ArgumentOutOfRangeException("{0} does not implement static method {1}.".FormatWith(type.Name, name));
            }

            return result;
        }

        public static MethodInfo StaticMethodByReturnType(this Type type, string nameStartWith, Type returnType)
        {
            var result = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(i => i.Name.StartsWith(nameStartWith, StringComparison.Ordinal))
                .FirstOrDefault(i => i.ReturnType == returnType);

            if (result == null)
            {
                throw new ArgumentOutOfRangeException("{0} does not implement method {1}*.".FormatWith(type.Name, nameStartWith));
            }

            return result;
        }

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

            throw new ArgumentOutOfRangeException("Implementation of method {0}.{1} not found in {2}.".FormatWith(methodDeclaringType.Name, method.Name, instance.FullName));
        }

        public static string GetSignature(MethodInfo method)
        {
            var result = new StringBuilder()
                .Append(typeof(void) == method.ReturnType ? "void" : method.ReturnType.Name)
                .Append(" ")
                .Append(GetNamespace(method.DeclaringType))
                .Append(".")
                .Append(method.Name)
                .Append("(");

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

                result.Append(p.ParameterType.Name);
            }

            result.Append(")");
            return result.ToString();
        }

        public static bool IsPublicInterface(Type type)
        {
            return type.IsInterface
                   && (type.IsPublic || type.IsNestedPublic);
        }

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

        public static string GetShortAssemblyQualifiedName(this Type type)
        {
            return new TypeFullNameBuilder(type).Build();
        }
    }
}
