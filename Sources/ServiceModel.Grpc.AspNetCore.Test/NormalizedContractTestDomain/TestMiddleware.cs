using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ServiceModel.Grpc.AspNetCore.NormalizedContractTestDomain
{
    internal sealed class TestMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
