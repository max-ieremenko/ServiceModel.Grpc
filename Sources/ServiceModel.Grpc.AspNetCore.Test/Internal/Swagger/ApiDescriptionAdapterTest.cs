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
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger;

[TestFixture]
public class ApiDescriptionAdapterTest
{
    private HttpContext _httpContext = null!;
    private EndpointMetadataCollection _metadata = null!;
    private ApiDescriptionAdapter _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _metadata = new EndpointMetadataCollection();

        var feature = new Mock<IEndpointFeature>(MockBehavior.Strict);
        feature
            .SetupGet(f => f.Endpoint)
            .Returns(() => new Endpoint(null!, _metadata, "dummy"));

        var features = new Mock<IFeatureCollection>(MockBehavior.Strict);
        features
            .Setup(f => f.Get<IEndpointFeature>())
            .Returns(feature.Object);

        var context = new Mock<HttpContext>(MockBehavior.Strict);
        context
            .SetupGet(c => c.Features)
            .Returns(features.Object);

        _httpContext = context.Object;

        _sut = new ApiDescriptionAdapter();
    }

    [Test]
    public void GetMethod()
    {
        _sut.GetMethod(_httpContext).ShouldBeNull();

        var method = new Mock<IMethod>(MockBehavior.Strict);

        _metadata = new EndpointMetadataCollection(new GrpcMethodMetadata(typeof(IDisposable), method.Object));

        _sut.GetMethod(_httpContext).ShouldBe(method.Object);
    }
}