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
using Grpc.Core;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client;

public partial class ClientFactoryTest
{
    public interface ISomeContract;

    internal interface IInternalContract;

    private sealed class ManualClientBuilder : IClientBuilder<ISomeContract>
    {
        public IMarshallerFactory MarshallerFactory { get; private set; } = null!;

        public Func<CallOptions>? DefaultCallOptionsFactory { get; private set; }

        public Func<CallInvoker, ISomeContract> OnBuild { get; set; } = null!;

        public void Initialize(IClientMethodBinder methodBinder)
        {
            MarshallerFactory = methodBinder.MarshallerFactory;
            DefaultCallOptionsFactory = ((ClientMethodBinder)methodBinder).DefaultCallOptionsFactory;
        }

        public ISomeContract Build(CallInvoker callInvoker) => OnBuild(callInvoker);
    }
}