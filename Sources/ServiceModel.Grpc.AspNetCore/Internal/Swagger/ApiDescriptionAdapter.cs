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
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger
{
    internal sealed class ApiDescriptionAdapter : IApiDescriptionAdapter
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionProvider;

        public ApiDescriptionAdapter(IApiDescriptionGroupCollectionProvider apiDescriptionProvider)
        {
            _apiDescriptionProvider = apiDescriptionProvider;
        }

        public ApiDescription? FindApiDescription(string requestPath)
        {
            var path = ProtocolConstants.NormalizeRelativePath(requestPath);

            var groups = _apiDescriptionProvider.ApiDescriptionGroups.Items;

            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                for (var j = 0; j < group.Items.Count; j++)
                {
                    var item = group.Items[j];
                    if (string.Equals(item.RelativePath, path, StringComparison.OrdinalIgnoreCase))
                    {
                        return item.ActionDescriptor is GrpcActionDescriptor ? item : null;
                    }
                }
            }

            return null;
        }

        public IMethod? GetMethod(HttpContext context)
        {
            return context.GetEndpoint()?.Metadata.GetMetadata<GrpcMethodMetadata>()?.Method;
        }
    }
}
