﻿// <copyright>
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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

public sealed class TypeHandler
{
    private readonly Func<string, string, Assembly> _assemblyResolver;

    public TypeHandler(Type importGrpcService, Type exportGrpcService, Func<string, string, Assembly> assemblyResolver)
    {
        _assemblyResolver = assemblyResolver;
        ImportGrpcService = importGrpcService;
        ExportGrpcService = exportGrpcService;
    }

    public Type ImportGrpcService { get; }

    public Type ExportGrpcService { get; }

    public Assembly GetAssembly(string assemblyName, string location) => _assemblyResolver(assemblyName, location);
}