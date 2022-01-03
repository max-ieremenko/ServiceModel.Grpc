// <copyright>
// Copyright 2020 Max Ieremenko
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

using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal
{
    internal sealed class ParameterDescription
    {
        public ParameterDescription(IParameterSymbol parameter)
        {
            Name = parameter.Name;
            Type = SyntaxTools.GetFullName(parameter.Type);
            TypeSymbol = parameter.Type;
            IsOut = parameter.IsOut();
            IsRef = parameter.IsRef();
        }

        public string Name { get; }

        public string Type { get; }

        public ITypeSymbol TypeSymbol { get; }

        public bool IsOut { get; }

        public bool IsRef { get; }

        public string GetNonNullableType()
        {
            if (!SyntaxTools.IsNullable(TypeSymbol))
            {
                return Type;
            }

            return SyntaxTools.GetFullName(TypeSymbol.GenericTypeArguments()[0]);
        }
    }
}
