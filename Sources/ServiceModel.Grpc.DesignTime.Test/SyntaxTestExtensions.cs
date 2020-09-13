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

using System;
using Microsoft.CodeAnalysis;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime
{
    internal static class SyntaxTestExtensions
    {
        public static INamedTypeSymbol GetTypeByMetadataName(this Compilation compilation, Type metadata)
        {
            return (INamedTypeSymbol)GetTypeByMetadataNameCore(compilation, metadata);
        }

        private static ITypeSymbol GetTypeByMetadataNameCore(Compilation compilation, Type metadata)
        {
            ITypeSymbol? symbol;
            if (metadata.IsGenericType)
            {
                var unbound = compilation.GetTypeByMetadataName(metadata.GetGenericTypeDefinition().FullName);
                var unboundArgs = metadata.GetGenericArguments();

                var args = new ITypeSymbol[unboundArgs.Length];
                for (var i = 0; i < unboundArgs.Length; i++)
                {
                    args[i] = GetTypeByMetadataNameCore(compilation, unboundArgs[i]);
                }

                symbol = unbound.Construct(args);
            }
            else if (metadata.IsArray)
            {
                var unbound = metadata.GetElementType()!;
                var elementType = GetTypeByMetadataNameCore(compilation, unbound);
                symbol = compilation.CreateArrayTypeSymbol(elementType, metadata.GetArrayRank());
            }
            else
            {
                symbol = compilation.GetTypeByMetadataName(metadata.FullName);
            }

            symbol.ShouldNotBeNull();
            return symbol;
        }
    }
}
