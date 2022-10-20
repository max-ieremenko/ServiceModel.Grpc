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

        var types = (IEnumerable<Type>)type
            .GetMethod(attribute.MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Invoke(null, new object[0]);

        return types;
    }
}