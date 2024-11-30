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

using System.Net;

namespace ServiceModel.Grpc.AspNetCore.TestApi.Domain;

public sealed class CustomResponseMiddleware : IMiddleware
{
    public const string HeaderResponseStatusCode = "CustomStatusCode";
    public const string HeaderContentType = "CustomContentType";
    public const string HeaderResponseBody = "CustomBody";

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var statusCode = context.Request.Headers[HeaderResponseStatusCode];
        if (statusCode.Count == 0)
        {
            return next(context);
        }

        context.Response.StatusCode = (int)Enum.Parse<HttpStatusCode>(statusCode.ToString());
        context.Response.ContentType = context.Request.Headers[HeaderContentType].ToString();

        var body = context.Request.Headers[HeaderResponseBody].ToString();
        return context.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(body)).AsTask();
    }
}