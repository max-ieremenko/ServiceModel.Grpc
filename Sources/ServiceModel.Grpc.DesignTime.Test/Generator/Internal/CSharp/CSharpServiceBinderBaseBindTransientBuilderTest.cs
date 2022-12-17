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

using NUnit.Framework;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

[TestFixture]
public class CSharpServiceBinderBaseBindTransientBuilderTest
{
    private ContractDescription _contract = null!;

    [OneTimeSetUp]
    public void BeforeAll()
    {
        _contract = ContractDescriptionFactory.Create(typeof(IContract));
    }

    [Test]
    public void GenerateMemberDeclaration()
    {
        var sut = new CSharpServiceBinderBaseBindTransientBuilder(_contract, true);

        var code = new CodeStringBuilder();
        sut.GenerateMemberDeclaration(code);
        var actual = code.AsStringBuilder().ToString();
        TestOutput.WriteLine(actual);

        actual.ShouldContain("IContract> serviceFactory,");
        actual.ShouldContain("new " + _contract.EndpointBinderClassName + "()");
    }
}