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

using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.Descriptions;

public static class NamingContract
{
    public static string GetBaseClassName<TType>(IReflect<TType> reflect, TType serviceType, string? @namespace)
        => ServiceContract.GetBaseClassName(reflect, serviceType, @namespace);

    public static class Contract
    {
        public static string Class(string baseClassName) => $"{baseClassName}Contract";

        public static string GrpcMethod(string operationName) => $"Method{operationName}";

        // ClrDefinitionMethodName = "Get" + OperationName + "Definition";
        //   syncOperation.ClrDefinitionMethodName = asyncOperation.ClrDefinitionMethodName + "Sync";
        //   ClrDefinitionMethodNameSyncVersion => ClrDefinitionMethodName + "Sync";
        public static string ClrDefinitionMethod(string operationName) => $"Get{operationName}Definition";

        public static string ClrDefinitionMethodSync(string asyncOperationName) => $"{ClrDefinitionMethod(asyncOperationName)}Sync";
    }

    public static class Client
    {
        public static string Class(string baseClassName) => $"{baseClassName}Client";
    }

    public static class ClientBuilder
    {
        public static string Class(string baseClassName) => $"{baseClassName}ClientBuilder";
    }

    public static class Endpoint
    {
        public static string Class(string baseClassName) => $"{baseClassName}Endpoint";
    }

    public static class EndpointBinder
    {
        public static string Class(string baseClassName) => $"{baseClassName}EndpointBinder";
    }
}