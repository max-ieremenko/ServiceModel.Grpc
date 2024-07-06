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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer;

internal static class MethodSignatureBuilder
{
    public static string Build(
        string name,
        (BindingSource Source, ParameterInfo Parameter)[] requestParameters,
        Type? responseType,
        (Type Type, string Name)[] responseHeaderParameters)
    {
        var result = new StringBuilder();

        if (responseType == null)
        {
            result.Append("void");
        }
        else
        {
            if (responseHeaderParameters.Length > 0)
            {
                result.Append('(');
            }

            result.WriteType(responseType);

            if (responseHeaderParameters.Length > 0)
            {
                for (var i = 0; i < responseHeaderParameters.Length; i++)
                {
                    var header = responseHeaderParameters[i];
                    result
                        .Append(", ")
                        .WriteType(header.Type)
                        .Append(' ')
                        .Append(header.Name);
                }

                result.Append(')');
            }
        }

        result
            .Append(' ')
            .Append(name)
            .Append('(');

        var index = 0;
        foreach (var (_, parameter) in requestParameters)
        {
            if (index > 0)
            {
                result.Append(", ");
            }

            index++;
            result
                .WriteType(parameter.ParameterType)
                .Append(' ')
                .Append(parameter.Name);
        }

        result.Append(')');
        return result.ToString();
    }

    private static StringBuilder WriteType(this StringBuilder result, Type type)
    {
        var isArray = type.IsArray;
        if (isArray)
        {
            type = type.GetElementType()!;
        }

        var nullable = Nullable.GetUnderlyingType(type);
        if (nullable != null)
        {
            type = nullable;
        }

        WriteTypeName(type, result);

        if (type.IsGenericType)
        {
            // System.Tuple`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib
            result.Append('<');

            var args = type.GetGenericArguments();
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(", ");
                }

                result.WriteType(args[i]);
            }

            result.Append('>');
        }

        // System.Private.CoreLib, mscorlib
        if (isArray)
        {
            result.Append("[]");
        }

        if (nullable != null)
        {
            result.Append('?');
        }

        return result;
    }

    private static void WriteTypeName(Type type, StringBuilder result)
    {
        if (type == typeof(void))
        {
            result.Append("void");
            return;
        }

        var index = type.Name.IndexOf('`');
        var count = type.Name.Length;
        if (index > 0)
        {
            count = index;
        }

        if (type.IsNested)
        {
            WriteTypeName(type.DeclaringType!, result);
            result
                .Append('.')
                .Append(type.Name, 0, count);
        }
        else
        {
            result.Append(type.Name, 0, count);
        }
    }
}