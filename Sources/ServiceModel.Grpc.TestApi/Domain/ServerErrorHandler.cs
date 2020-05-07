// <copyright>
// Copyright 2020 Max Ieremenko
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
using ServiceModel.Grpc.Interceptors;

namespace ServiceModel.Grpc.TestApi.Domain
{
    public sealed class ServerErrorHandler : ServerErrorHandlerBase
    {
        protected override ServerFaultDetail? ProvideFaultOrIgnoreCore(ServerCallInterceptorContext context, Exception error)
        {
            return new ServerFaultDetail
            {
                Message = error.Message,
                Detail = new ExceptionDetail(error)
            };
        }
    }
}
