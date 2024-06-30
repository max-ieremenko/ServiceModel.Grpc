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

using Grpc.Core;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

public interface IOperationDescription
{
    string ServiceName { get; }

    string OperationName { get; }

    IMethodSymbol Method { get; }

    MethodType OperationType { get; }

    bool IsAsync { get; }

    IMessageDescription ResponseType { get; }

    int ResponseTypeIndex { get; }

    IMessageDescription? HeaderResponseType { get; }

    int[] HeaderResponseTypeInput { get; }

    IMessageDescription RequestType { get; }

    int[] RequestTypeInput { get; }

    IMessageDescription? HeaderRequestType { get; }

    int[] HeaderRequestTypeInput { get; }

    int[] ContextInput { get; }
}