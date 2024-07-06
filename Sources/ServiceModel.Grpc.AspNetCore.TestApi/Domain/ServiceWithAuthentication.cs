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

using Microsoft.AspNetCore.Authorization;

namespace ServiceModel.Grpc.AspNetCore.TestApi.Domain;

[Authorize]
public sealed class ServiceWithAuthentication : IServiceWithAuthentication
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ServiceWithAuthentication(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public string? GetCurrentUserName(CallContext? context)
    {
        return _contextAccessor.HttpContext?.User.Identity?.Name;
    }

    [AllowAnonymous]
    public string? TryGetCurrentUserName(CallContext? context)
    {
        return _contextAccessor.HttpContext?.User.Identity?.Name;
    }
}