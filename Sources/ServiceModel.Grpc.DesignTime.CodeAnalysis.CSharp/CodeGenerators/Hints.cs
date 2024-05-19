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

using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal static class Hints
{
    public const string Messages = "Messages";

    public static string Contracts(string baseClassName) => NamingConventions.Contract.Class(baseClassName);

    public static string Clients(string baseClassName) => NamingConventions.Client.Class(baseClassName);

    public static string ClientBuilders(string baseClassName) => NamingConventions.Client.Class(baseClassName);

    public static string ClientDiExtensions(string baseClassName) => NamingConventions.Client.Class(baseClassName);

    public static string ClientFactoryExtensions(string baseClassName) => NamingConventions.Client.Class(baseClassName);

    public static string Endpoints(string baseClassName) => NamingConventions.Endpoint.Class(baseClassName);

    public static string EndpointBinders(string baseClassName) => NamingConventions.Endpoint.Class(baseClassName);

    public static string EndpointAspNetAddOptions(string baseClassName) => NamingConventions.Endpoint.Class(baseClassName);

    public static string EndpointAspNetMapGrpc(string baseClassName) => NamingConventions.Endpoint.Class(baseClassName);

    public static string EndpointSelfHostAddSingleton(string baseClassName) => NamingConventions.Endpoint.Class(baseClassName);

    public static string EndpointSelfHostAddTransient(string baseClassName) => NamingConventions.Endpoint.Class(baseClassName);

    public static string EndpointSelfHostAddProvider(string baseClassName) => NamingConventions.Endpoint.Class(baseClassName);

    public static string EndpointSelfHostBinderBaseBind(string baseClassName) => NamingConventions.Endpoint.Class(baseClassName);

    public static string EndpointSelfHostBinderBaseBindTransient(string baseClassName) => NamingConventions.Endpoint.Class(baseClassName);
}
