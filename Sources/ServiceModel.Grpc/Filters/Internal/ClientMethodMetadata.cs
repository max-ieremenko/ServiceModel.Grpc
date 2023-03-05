// <copyright>
// Copyright 2023 Max Ieremenko
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
using System.Reflection;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed partial class ClientMethodMetadata
{
    public ClientMethodMetadata(
        Func<MethodInfo> contractMethodDefinition,
        Func<MethodInfo>? alternateContractMethodDefinition)
    {
        Method = new Metadata(contractMethodDefinition);
        AlternateMethod = alternateContractMethodDefinition == null ? null : new Metadata(alternateContractMethodDefinition);
    }

    // in case of sync-over-async contains async version
    public Metadata Method { get; }

    public Metadata? AlternateMethod { get; }
}