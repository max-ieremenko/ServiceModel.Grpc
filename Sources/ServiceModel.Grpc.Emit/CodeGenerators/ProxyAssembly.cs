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

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

internal static class ProxyAssembly
{
    public static readonly object SyncRoot = new object();
    public static readonly ModuleBuilder DefaultModule = CreateModule("ServiceModel.Grpc.Proxy");

    internal static ModuleBuilder CreateModule(string name)
    {
        var assemblyName = new AssemblyName(name);
        assemblyName.SetPublicKey(Convert.FromBase64String("ACQAAASAAACUAAAABgIAAAAkAABSU0ExAAQAAAEAAQDZAJwN9Rn4Om4Qwz1i4sVhZ0Mf9p6t9OEuXu8D/s9N1N4FKL8eh1REUxAnFNhK6lmIxEb5UAMWXBtJ3WQ4kVCo3FKIJAXX+MYemcRD+YLKkK/ZEHb3m3KwbQ7La456yxhoXh1+5XlA7StUhj3dW3jfDkWzOHhKlxSeOznDqBjb6g=="));

        return AssemblyBuilder
            .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
            .DefineDynamicModule(name);
    }
}