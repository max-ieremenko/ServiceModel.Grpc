// <copyright>
// Copyright 2022 Max Ieremenko
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

using System.Threading.Tasks;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi;

public abstract class SharedContractTestBase
{
    protected IConcreteContract1 DomainService1 { get; set; } = null!;

    protected IConcreteContract2 DomainService2 { get; set; } = null!;

    [Test]
    public async Task InvokeConcreteContract1()
    {
        var name1 = await DomainService1.GetName().ConfigureAwait(false);
        var name2 = await DomainService1.GetConcreteName().ConfigureAwait(false);

        name1.ShouldBe(nameof(ConcreteContract1));
        name1.ShouldBe(name2);
    }

    [Test]
    public async Task InvokeConcreteContract2()
    {
        var name1 = await DomainService2.GetName().ConfigureAwait(false);
        var name2 = await DomainService2.GetConcreteName().ConfigureAwait(false);

        name1.ShouldBe(nameof(ConcreteContract2));
        name1.ShouldBe(name2);
    }
}