﻿// <copyright>
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
using System.Threading.Tasks;
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed class ClientCallFilterHandler : CallFilterHandlerBase<IClientFilterContext, IClientFilter>, IClientCallFilterHandler
{
    public ClientCallFilterHandler(IClientFilterContext context, IClientFilter[] filters)
        : base(context, filters)
    {
    }

    protected override ValueTask HandleAsync(IClientFilter filter, Func<ValueTask> next) => filter.InvokeAsync(Context, next);

    protected override void Handle(IClientFilter filter, Action next) => filter.Invoke(Context, next);
}