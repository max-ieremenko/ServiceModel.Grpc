using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.Internal
{
    internal static class ReflectionTools
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

        public static FieldInfo StaticFiled(this Type type, string name)
        {
            var result = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

            if (result == null)
            {
                throw new ArgumentOutOfRangeException("{0} does not have static field {1}.".FormatWith(type.Name, name));
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
                .Append(method.DeclaringType?.FullName)
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

        public static Attribute GetCustomAttribute(MemberInfo owner, string attributeTypeFullName)
        {
            return owner
                .GetCustomAttributes()
                .FirstOrDefault(i => string.Equals(attributeTypeFullName, i.GetType().FullName, StringComparison.Ordinal));
        }

        public static TDelegate CreateDelegate<TDelegate>(this MethodInfo method, object target = null)
            where TDelegate : Delegate
        {
            var result = target == null ? method.CreateDelegate(typeof(TDelegate)) : method.CreateDelegate(typeof(TDelegate), target);
            return (TDelegate)result;
        }
    }
}
