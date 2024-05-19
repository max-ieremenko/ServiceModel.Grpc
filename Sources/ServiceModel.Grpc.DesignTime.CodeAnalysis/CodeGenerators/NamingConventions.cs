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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

public static class NamingConventions
{
    public static class Contract
    {
        public static string Class(string baseClassName) => $"{baseClassName}Contract";

        public static string GrpcMethod(string operationName) => $"Method{operationName}";

        public static string GrpcMethodInputHeader(string operationName) => $"MethodInputHeader{operationName}";

        public static string GrpcMethodOutputHeader(string operationName) => $"MethodOutputHeader{operationName}";
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