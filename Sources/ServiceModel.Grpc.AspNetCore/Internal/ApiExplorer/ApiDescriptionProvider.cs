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

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;

namespace ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer
{
    internal sealed class ApiDescriptionProvider : IApiDescriptionProvider
    {
        private readonly EndpointDataSource _endpointDataSource;

        public ApiDescriptionProvider(EndpointDataSource endpointDataSource)
        {
            _endpointDataSource = endpointDataSource;
        }

        public int Order => 0;

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
            var endpoints = _endpointDataSource.Endpoints;

            for (var i = 0; i < endpoints.Count; i++)
            {
                if (!(endpoints[i] is RouteEndpoint routeEndpoint))
                {
                    continue;
                }

                var apiDescription = ApiDescriptionGenerator.TryCreateApiDescription(routeEndpoint);
                if (apiDescription != null)
                {
                    context.Results.Add(apiDescription);
                }
            }
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
        }
    }
}
