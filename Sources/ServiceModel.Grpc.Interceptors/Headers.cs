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

using Grpc.Core;

namespace ServiceModel.Grpc.Interceptors;

internal static class Headers
{
    internal const string HeaderNameErrorDetail = $"smgrpc-error-detail{Metadata.BinaryHeaderSuffix}";
    internal const string HeaderNameErrorDetailType = "smgrpc-error-detail-type";

    public static string GetShortAssemblyQualifiedName(this Type type) => new TypeFullNameBuilder(type).Build();

    private readonly ref struct TypeFullNameBuilder
    {
        private readonly Type _type;
        private readonly StringBuilder _result;

        public TypeFullNameBuilder(Type type)
        {
            _type = type;
            _result = new StringBuilder();
        }

        public string Build()
        {
            WriteShortAssemblyQualifiedName(_type);
            return _result.ToString();
        }

        private void WriteShortAssemblyQualifiedName(Type type)
        {
            var isArray = type.IsArray;
            if (isArray)
            {
                type = type.GetElementType()!;
            }

            WriteTypeFullName(type);

            // System.Tuple`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib
            if (type.IsGenericType)
            {
                _result.Append("[");

                var args = type.GetGenericArguments();
                for (var i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                    {
                        _result.Append(", ");
                    }

                    _result.Append("[");
                    WriteShortAssemblyQualifiedName(args[i]);
                    _result.Append("]");
                }

                _result.Append("]");
            }

            // System.Private.CoreLib, mscorlib
            if (isArray)
            {
                _result.Append("[]");
            }

            WriteAssemblyName(type);
        }

        private void WriteTypeFullName(Type type)
        {
            if (type.IsNested)
            {
                WriteTypeFullName(type.DeclaringType);
                _result
                    .Append("+")
                    .Append(type.Name);
            }
            else
            {
                _result
                    .Append(type.Namespace)
                    .Append(".")
                    .Append(type.Name);
            }
        }

        private void WriteAssemblyName(Type type)
        {
            if (type.IsPrimitive)
            {
                return;
            }

            var assemblyName = type.Assembly.GetName().Name;
            if ("System.Private.CoreLib".Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if ("mscorlib".Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _result
                .Append(", ")
                .Append(assemblyName);
        }
    }
}