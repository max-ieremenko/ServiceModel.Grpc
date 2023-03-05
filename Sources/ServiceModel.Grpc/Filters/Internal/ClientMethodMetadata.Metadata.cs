// <copyright>
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
using System.Reflection;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed partial class ClientMethodMetadata
{
    public sealed class Metadata
    {
        private MessageProxy? _requestMessageProxy;
        private MessageProxy? _responseMessageProxy;
        private StreamProxy? _requestStreamProxy;
        private StreamProxy? _responseStreamProxy;

        public Metadata(Func<MethodInfo> contractMethodDefinition)
        {
            ContractMethodDefinition = contractMethodDefinition;
            FilterFactories = Array.Empty<Func<IServiceProvider, IClientFilter>>();
        }

        public Func<MethodInfo> ContractMethodDefinition { get; }

        public Func<IServiceProvider, IClientFilter>[] FilterFactories { get; set; }

        public IRequestContextInternal CreateRequestContext()
        {
            InitProxies();
            return new RequestContext(_requestMessageProxy!, _requestStreamProxy);
        }

        public IResponseContextInternal CreateResponseContext()
        {
            InitProxies();
            return new ResponseContext(_responseMessageProxy!, _responseStreamProxy);
        }

        private void InitProxies()
        {
            if (_requestMessageProxy != null)
            {
                return;
            }

            var factory = new ProxyFactory(ContractMethodDefinition());
            _requestMessageProxy = factory.RequestProxy;
            _requestStreamProxy = factory.RequestStreamProxy;
            _responseMessageProxy = factory.ResponseProxy;
            _responseStreamProxy = factory.ResponseStreamProxy;
        }
    }
}