using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.Internal
{
    internal static class ReflectionTools
    {
        public static ICollection<Type> ExpandInterface(Type type)
        {
            var result = new HashSet<Type> { type };

            if (type.IsInterface)
            {
                result.Add(type);
            }

            foreach (var i in type.GetInterfaces())
            {
                result.Add(i);
            }

            return result;
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
            if (typeof(Task).IsAssignableFrom(type))
            {
                return true;
            }

            return string.Equals(type.Namespace, typeof(Task).Namespace, StringComparison.Ordinal)
                   && (type.Name.Equals("ValueTask", StringComparison.Ordinal) || type.Name.Equals("ValueTask`1", StringComparison.Ordinal));
        }

        public static bool IsAsyncEnumerable(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            return string.Equals(type.Namespace, typeof(IAsyncEnumerable<>).Namespace, StringComparison.Ordinal)
                   && type.Name.Equals("IAsyncEnumerable`1", StringComparison.Ordinal);
        }

        public static bool IsPureInParameter(ParameterInfo parameter)
        {
            return !parameter.IsOut
                   && !parameter.ParameterType.Name.EndsWith("&", StringComparison.Ordinal);
        }

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

        public static PropertyInfo InstanceProperty(this Type type, string name)
        {
            var result = type.GetProperty(
                name, 
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

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

        public static MethodInfo StaticMethod(this Type type, string name)
        {
            var result = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

            if (result == null)
            {
                throw new ArgumentOutOfRangeException("{0} does not implement method {1}.".FormatWith(type.Name, name));
            }

            return result;
        }

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
                throw new ArgumentOutOfRangeException("{0} does not implement method {1}.".FormatWith(type.Name, name));
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
    }
}
