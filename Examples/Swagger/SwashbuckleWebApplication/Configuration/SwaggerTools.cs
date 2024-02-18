using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace SwashbuckleWebApplication.Configuration;

public static class SwaggerTools
{
    public static IEnumerable<Type> GetDataContractKnownTypes(Type type)
    {
        foreach (var attribute in type.GetCustomAttributes<KnownTypeAttribute>())
        {
            foreach (var knownType in ResolveKnownTypes(type, attribute))
            {
                yield return knownType;
            }
        }
    }

    private static IEnumerable<Type> ResolveKnownTypes(Type type, KnownTypeAttribute attribute)
    {
        if (attribute.Type != null)
        {
            return new[] { attribute.Type };
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