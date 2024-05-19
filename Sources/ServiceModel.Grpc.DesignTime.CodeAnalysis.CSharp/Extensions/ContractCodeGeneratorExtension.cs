// <copyright>
// Copyright 2024 Max Ieremenko
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
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.Extensions;

internal sealed class ContractCodeGeneratorExtension : ICodeGeneratorExtension
{
    public void ProvideGenerators(ICodeGeneratorCollection generators, IContractDescriptionCollection descriptions, IExtensionContext context)
    {
        // [ImportGrpcService(Interface)]
        // [ExportGrpcService(ImplementationClass : Interface)]
        var distinct = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < descriptions.Count; i++)
        {
            var description = descriptions[i];
            if (distinct.Add(description.BaseClassName))
            {
                generators.Add(new ContractCodeGenerator(description));
            }
        }
    }
}