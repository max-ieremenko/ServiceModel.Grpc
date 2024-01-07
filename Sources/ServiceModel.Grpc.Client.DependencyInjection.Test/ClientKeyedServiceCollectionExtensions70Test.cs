// <copyright>
// Copyright 2024 Max Ieremenko
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

#if NET7_0
using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Client.DependencyInjection;

[TestFixture]
public class ClientKeyedServiceCollectionExtensions70Test
{
    [Test]
    public void NotSupported()
    {
        var services = new ServiceCollection();
        services.AddKeyedServiceModelGrpcClientFactory(new object());

        // InvalidOperationException : This service descriptor is keyed. Your service provider may not support keyed services.
        Should.Throw<InvalidOperationException>(services.BuildServiceProvider);
    }
}
#endif