using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace SwashbuckleWebApplication.Configuration;

public static class SwaggerTools
{
    public static IEnumerable<Type> GetDataContractKnownTypes(Type type)
    {
        var result = new HashSet<Type>(1);
        AddType(result, type);

        foreach (var attribute in type.GetCustomAttributes<KnownTypeAttribute>(inherit: false))
        {
            var knownTypes = ResolveKnownTypes(type, attribute);
            foreach (var knownType in knownTypes)
            {
                AddType(result, knownType);
            }
        }

        return result;
    }

    private static void AddType(HashSet<Type> types, Type type)
    {
        if (!type.IsAbstract)
        {
            types.Add(type);
        }
    }

    private static IEnumerable<Type> ResolveKnownTypes(Type type, KnownTypeAttribute attribute)
    {
        if (attribute.Type != null)
        {
            return [attribute.Type];
        }

        if (attribute.MethodName == null)
        {
            return Array.Empty<Type>();
        }

        var types = (IEnumerable<Type>)type
            .GetMethod(attribute.MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!
            .Invoke(null, Array.Empty<object>())!;

        return types;
    }
}