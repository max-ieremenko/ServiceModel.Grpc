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

using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;

namespace ServiceModel.Grpc.AspNetCore;

[TestFixture]
public partial class NameConflictsTest
{
    private KestrelHost _host = null!;
    private ICalculator _calculator = null!;

    [OneTimeSetUp]
    public async Task BeforeAll()
    {
        _host = await new KestrelHost()
            .ConfigureEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<Calculator>();
            })
            .StartAsync()
            .ConfigureAwait(false);

        _calculator = _host.ClientFactory.CreateClient<ICalculator>(_host.Channel);
    }

    [OneTimeTearDown]
    public async Task AfterAll()
    {
        await _host.DisposeAsync().ConfigureAwait(false);
    }

    [Test]
    public void Sum2()
    {
        _calculator.Sum(1, 2).ShouldBe(4);
    }

    [Test]
    public void Sum3()
    {
        _calculator.Sum(1, 2, 3).ShouldBe(5);
    }
}