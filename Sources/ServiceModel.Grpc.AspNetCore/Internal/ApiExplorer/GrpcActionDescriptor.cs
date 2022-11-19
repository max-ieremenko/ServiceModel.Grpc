// <copyright>
// Copyright 2021-2022 Max Ieremenko
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

using System.Diagnostics;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer;

[DebuggerDisplay("{ControllerName}.{ActionName}")]
internal sealed class GrpcActionDescriptor : ControllerActionDescriptor
{
    public MethodType MethodType { get; set; }

    public string MethodSignature { get; set; } = null!;
}