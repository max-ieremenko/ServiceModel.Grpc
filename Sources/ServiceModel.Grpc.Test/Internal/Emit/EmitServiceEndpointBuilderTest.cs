﻿// <copyright>
// Copyright 2020-2021 Max Ieremenko
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

using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.Internal.Emit;

[TestFixture]
public class EmitServiceEndpointBuilderTest : ServiceEndpointBuilderTestBase
{
    [OneTimeSetUp]
    public void BeforeAllTest()
    {
        var description = ContractDescriptionBuilder.Build(typeof(IContract));
        var contractType = new EmitContractBuilder(description).Build(ProxyAssembly.DefaultModule, nameof(EmitServiceEndpointBuilderTest) + "Contract");

        var sut = new EmitServiceEndpointBuilder(description);
        ChannelType = sut.Build(ProxyAssembly.DefaultModule, className: nameof(EmitServiceEndpointBuilderTest) + "Channel");

        var contract = EmitContractBuilder.CreateFactory(contractType)(DataContractMarshallerFactory.Default);
        Channel = EmitServiceEndpointBuilder.CreateFactory(ChannelType)();
    }
}