// <copyright>
// Copyright 2021 Max Ieremenko
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
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceModel.Grpc.Filters;
using Shouldly;

namespace ServiceModel.Grpc.TestApi.Domain;

public sealed class TrackingServerFilter : IServerFilter
{
    public TrackingServerFilter(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public async ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
    {
        context.ServiceMethodInfo.ShouldNotBeNull();

        var input = (IList<string>)context.Request[0]!;
        context.Request["input"] = new List<string>(input)
        {
            Name + "-before"
        };

        await next().ConfigureAwait(false);

        var result = (IList<string>)context.Response[0]!;
        context.Response[0] = new List<string>(result)
        {
            Name + "-after"
        };
    }
}