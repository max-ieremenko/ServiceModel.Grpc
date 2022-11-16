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
using System.Collections.Generic;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger;

[TestFixture]
public class ApiDescriptionAdapterTest
{
    private Mock<IApiDescriptionGroupCollectionProvider> _apiDescriptionProvider = null!;
    private ApiDescriptionAdapter _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _apiDescriptionProvider = new Mock<IApiDescriptionGroupCollectionProvider>(MockBehavior.Strict);
        _sut = new ApiDescriptionAdapter(_apiDescriptionProvider.Object);
    }

    [Test]
    [TestCase("/path1", false)]
    [TestCase("/path2", false)]
    [TestCase("/path3", true)]
    public void FindApiDescription(string path, bool expected)
    {
        var items = new List<ApiDescription>
        {
            new ApiDescription
            {
                RelativePath = "path1"
            },
            new ApiDescription
            {
                RelativePath = "path2",
                ActionDescriptor = new ActionDescriptor()
            },
            new ApiDescription
            {
                RelativePath = "path3",
                ActionDescriptor = new GrpcActionDescriptor()
            },
        };

        _apiDescriptionProvider
            .SetupGet(p => p.ApiDescriptionGroups)
            .Returns(new ApiDescriptionGroupCollection(new[] { new ApiDescriptionGroup("n", items) }, 1));

        var actual = _sut.FindApiDescription(path);

        expected.ShouldBe(actual != null);
    }

    [Test]
    public void GetMethod()
    {
        var features = new Mock<IFeatureCollection>(MockBehavior.Strict);
        features
            .Setup(f => f.Get<IEndpointFeature>())
            .Returns((IEndpointFeature)null!);

        var context = new Mock<HttpContext>(MockBehavior.Strict);
        context
            .SetupGet(c => c.Features)
            .Returns(features.Object);

        _sut.GetMethod(context.Object).ShouldBeNull();

        var feature = new Mock<IEndpointFeature>(MockBehavior.Strict);
        feature
            .SetupGet(f => f.Endpoint)
            .Returns((Endpoint)null!);

        features
            .Setup(f => f.Get<IEndpointFeature>())
            .Returns(feature.Object);

        _sut.GetMethod(context.Object).ShouldBeNull();

        var endpoint = new Endpoint(null!, new EndpointMetadataCollection(), "s");

        feature
            .SetupGet(f => f.Endpoint)
            .Returns(endpoint);

        _sut.GetMethod(context.Object).ShouldBeNull();

        var method = new Mock<IMethod>(MockBehavior.Strict);
        var metadata = new GrpcMethodMetadata(typeof(IDisposable), method.Object);
        endpoint = new Endpoint(null!, new EndpointMetadataCollection(metadata), "s");

        feature
            .SetupGet(f => f.Endpoint)
            .Returns(endpoint);

        _sut.GetMethod(context.Object).ShouldBe(method.Object);
    }
}