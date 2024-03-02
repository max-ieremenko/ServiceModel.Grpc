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

using System.Collections.Generic;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Generator.Test;

[TestFixture]
[ExportGrpcService(typeof(TrackedFilteredService))]
public partial class CSharpServiceEndpointBinderBuilderTest
{
    [Test]
    public void ServiceGetMetadataOverrideTest()
    {
        var actual = new TrackedFilteredServiceEndpointBinder().InvokeServiceGetMetadata();

        actual.Count.ShouldBe(2);
        actual[0].ShouldBeOfType<TrackingServerFilterAttribute>().Order.ShouldBe(3);
        actual[1].ShouldBe("service-marker");
    }

    [Test]
    public void MethodGetMetadataOverrideTest()
    {
        var actual = new TrackedFilteredServiceEndpointBinder().InvokeMethodClientStreamAsyncGetMetadata();

        actual.Count.ShouldBe(4);
        actual[0].ShouldBeOfType<TrackingServerFilterAttribute>().Order.ShouldBe(3);
        actual[1].ShouldBe("service-marker");
        actual[2].ShouldBeOfType<TrackingServerFilterAttribute>().Order.ShouldBe(4);
        actual[3].ShouldBe("method-marker");
    }

    internal partial class TrackedFilteredServiceEndpointBinder
    {
        public IList<object> InvokeServiceGetMetadata()
        {
            var result = new List<object>();
            ServiceGetMetadata(result);
            return result;
        }

        public IList<object> InvokeMethodClientStreamAsyncGetMetadata() => MethodClientStreamAsyncGetMetadata();

        partial void ServiceGetMetadataOverride(IList<object> metadata)
        {
            metadata.Add("service-marker");
        }

        partial void MethodClientStreamAsyncGetMetadataOverride(IList<object> metadata)
        {
            metadata.Add("method-marker");
        }
    }
}