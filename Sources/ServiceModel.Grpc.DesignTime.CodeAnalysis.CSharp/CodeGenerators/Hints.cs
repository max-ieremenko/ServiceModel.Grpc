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

using ServiceModel.Grpc.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal static class Hints
{
    public const string Messages = "Messages";

    public static string Contracts(string baseClassName) => NamingContract.Contract.Class(baseClassName);

    public static string Clients(string baseClassName) => NamingContract.Client.Class(baseClassName);

    public static string ClientBuilders(string baseClassName) => NamingContract.Client.Class(baseClassName);

    public static string ClientDiExtensions(string baseClassName) => NamingContract.Client.Class(baseClassName);

    public static string ClientFactoryExtensions(string baseClassName) => NamingContract.Client.Class(baseClassName);

    public static string Endpoints(string baseClassName) => NamingContract.Endpoint.Class(baseClassName);

    public static string EndpointBinders(string baseClassName) => NamingContract.Endpoint.Class(baseClassName);

    public static string EndpointAspNetAddOptions(string baseClassName) => NamingContract.Endpoint.Class(baseClassName);

    public static string EndpointAspNetMapGrpc(string baseClassName) => NamingContract.Endpoint.Class(baseClassName);

    public static string EndpointSelfHostAddSingleton(string baseClassName) => NamingContract.Endpoint.Class(baseClassName);

    public static string EndpointSelfHostAddTransient(string baseClassName) => NamingContract.Endpoint.Class(baseClassName);

    public static string EndpointSelfHostAddProvider(string baseClassName) => NamingContract.Endpoint.Class(baseClassName);

    public static string EndpointSelfHostBinderBaseBind(string baseClassName) => NamingContract.Endpoint.Class(baseClassName);

    public static string EndpointSelfHostBinderBaseBindTransient(string baseClassName) => NamingContract.Endpoint.Class(baseClassName);
}
