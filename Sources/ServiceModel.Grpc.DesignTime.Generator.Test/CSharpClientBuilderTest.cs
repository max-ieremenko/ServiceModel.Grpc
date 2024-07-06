﻿// <copyright>
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

using System.Reflection;
using NUnit.Framework;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.DesignTime.Generator.Test;

[TestFixture]
public class CSharpClientBuilderTest : ClientBuilderTestBase
{
    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        var builder = new GrpcServices.ContractClientBuilder();
        builder.Initialize(new ClientMethodBinder(null, DataContractMarshallerFactory.Default, null));

        Factory = () => builder.Build(CallInvoker.Object);
    }

    protected override MethodInfo GetClientInstanceMethod(string name)
    {
        return base.GetClientInstanceMethod("global::" + typeof(IContract).FullName + "." + name);
    }

    protected override MethodInfo GetClientInstanceMethod(string name, params Type[] parameters)
    {
        return base.GetClientInstanceMethod("global::" + typeof(IContract).FullName + "." + name, parameters);
    }
}