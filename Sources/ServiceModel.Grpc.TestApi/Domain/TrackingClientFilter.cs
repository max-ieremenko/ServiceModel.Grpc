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

using ServiceModel.Grpc.Filters;

namespace ServiceModel.Grpc.TestApi.Domain;

public sealed class TrackingClientFilter : IClientFilter
{
    public TrackingClientFilter(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public void Invoke(IClientFilterContext context, Action next)
    {
        OnRequest(context);
        next();
        OnResponse(context);
    }

    public async ValueTask InvokeAsync(IClientFilterContext context, Func<ValueTask> next)
    {
        OnRequest(context);
        await next().ConfigureAwait(false);
        OnResponse(context);
    }

    private void OnRequest(IClientFilterContext context)
    {
        context.ContractMethodInfo.ShouldNotBeNull();

        var message = Name + "-before";

        if (context.Request.Count != 0)
        {
            var input = context.Request[0].ShouldBeAssignableTo<IList<string>>()!;
            context.ContractMethodInfo.Name.ShouldBe(input[0]);

            context.Request["input"] = new List<string>(input) { message };
        }

        if (context.Request.Stream != null)
        {
            context.Request.Stream = ExtendStream(context.Request.Stream, message);
        }
    }

    private void OnResponse(IClientFilterContext context)
    {
        context.Response.IsProvided.ShouldBeTrue();

        var message = Name + "-after";

        if (context.Response.Count != 0)
        {
            var result = context.Response[0].ShouldBeAssignableTo<IList<string>>()!;
            context.Response[0] = new List<string>(result) { message };
        }

        if (context.Response.Stream != null)
        {
            context.Response.Stream = ExtendStream(context.Response.Stream, message);
        }
    }

    private object ExtendStream(object stream, string message)
    {
        if (stream is IAsyncEnumerable<string> list)
        {
            return ExtendStream(list, message);
        }

        return stream;
    }

    private async IAsyncEnumerable<string> ExtendStream(IAsyncEnumerable<string> stream, string message)
    {
        await foreach (var item in stream)
        {
            yield return item;
        }

        yield return message;
    }
}