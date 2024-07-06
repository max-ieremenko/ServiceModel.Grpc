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

using System;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed partial class ClientMethodMetadata
{
    public sealed class Metadata
    {
        private MessageProxy? _requestMessageProxy;
        private MessageProxy? _responseMessageProxy;
        private IStreamAccessor? _requestStreamProxy;
        private IStreamAccessor? _responseStreamProxy;

        public Metadata(IOperationDescriptor operation)
        {
            Operation = operation;
            FilterFactories = [];
        }

        public IOperationDescriptor Operation { get; }

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

            _requestMessageProxy = new MessageProxy(Operation.GetRequestAccessor());
            _requestStreamProxy = Operation.GetRequestStreamAccessor();
            _responseMessageProxy = new MessageProxy(Operation.GetResponseAccessor());
            _responseStreamProxy = Operation.GetResponseStreamAccessor();
        }
    }
}