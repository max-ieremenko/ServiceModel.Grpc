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

using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.TestApi;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

[TestFixture]
public partial class InterfaceTreeTest
{
    private readonly CSharpCompilation _compilation = CSharpCompilationExtensions.CreateDefault();

    [Test]
    public void NonServiceContractTree()
    {
        var symbol = _compilation.ResolveTypeSymbol(typeof(NonServiceContract.IService));
        var actual = ContractDescriptionBuilder.Build(symbol);

        actual.Interfaces.Length.ShouldBe(1);
        actual.Interfaces[0].InterfaceType.ShouldBe(symbol);

        actual.Services.ShouldBeEmpty();
    }

    [Test]
    [TestCase(typeof(OneContractRoot.IContract))]
    [TestCase(typeof(OneContractRoot.Contract))]
    public void OneContractRootTree(Type rootType)
    {
        var rootTypeSymbol = _compilation.ResolveTypeSymbol(rootType);
        var actual = ContractDescriptionBuilder.Build(rootTypeSymbol);

        actual.Interfaces.Length.ShouldBe(1);
        actual.Interfaces[0].InterfaceType.Name.ShouldBe(nameof(IDisposable));

        actual.Services.Length.ShouldBe(3);

        actual.Services[0].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(OneContractRoot.IContract)));
        actual.Services[0].Operations.ShouldBeEmpty();
        actual.Services[0].Methods.ShouldBeEmpty();
        actual.Services[0].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[1].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(OneContractRoot.IService1)));
        actual.Services[1].Operations.Length.ShouldBe(1);
        actual.Services[1].Operations[0].ServiceName.ShouldBe(nameof(OneContractRoot.IContract));
        actual.Services[1].Methods.ShouldBeEmpty();
        actual.Services[1].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[2].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(OneContractRoot.IService2)));
        actual.Services[1].Operations.Length.ShouldBe(1);
        actual.Services[1].Operations[0].ServiceName.ShouldBe(nameof(OneContractRoot.IContract));
        actual.Services[1].Methods.ShouldBeEmpty();
        actual.Services[1].NotSupportedOperations.ShouldBeEmpty();
    }

    [Test]
    public void AttachToMostTopContractTree()
    {
        var rootType = _compilation.ResolveTypeSymbol(typeof(AttachToMostTopContract.IContract2));
        var actual = ContractDescriptionBuilder.Build(rootType);

        actual.Interfaces.ShouldBeEmpty();

        actual.Services.Length.ShouldBe(4);

        actual.Services[0].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(AttachToMostTopContract.IContract1)));
        actual.Services[0].Operations.ShouldBeEmpty();
        actual.Services[0].Methods.ShouldBeEmpty();
        actual.Services[0].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[1].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(AttachToMostTopContract.IContract2)));
        actual.Services[1].Operations.ShouldBeEmpty();
        actual.Services[1].Methods.ShouldBeEmpty();
        actual.Services[1].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[2].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(AttachToMostTopContract.IService1)));
        actual.Services[2].Operations.Length.ShouldBe(1);
        actual.Services[2].Operations[0].ServiceName.ShouldBe(nameof(AttachToMostTopContract.IContract2));
        actual.Services[2].Methods.ShouldBeEmpty();
        actual.Services[2].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[3].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(AttachToMostTopContract.IService2)));
        actual.Services[2].Operations.Length.ShouldBe(1);
        actual.Services[2].Operations[0].ServiceName.ShouldBe(nameof(AttachToMostTopContract.IContract2));
        actual.Services[3].Methods.ShouldBeEmpty();
        actual.Services[3].NotSupportedOperations.ShouldBeEmpty();
    }

    [Test]
    public void RootNotFoundTree()
    {
        var rootType = _compilation.ResolveTypeSymbol(typeof(RootNotFound.Contract));
        var actual = ContractDescriptionBuilder.Build(rootType);

        actual.Interfaces.ShouldBeEmpty();

        actual.Services.Length.ShouldBe(3);

        actual.Services[0].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(RootNotFound.IContract1)));
        actual.Services[0].Operations.ShouldBeEmpty();
        actual.Services[0].Methods.ShouldBeEmpty();
        actual.Services[0].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[1].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(RootNotFound.IContract2)));
        actual.Services[1].Operations.ShouldBeEmpty();
        actual.Services[1].Methods.ShouldBeEmpty();
        actual.Services[1].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[2].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(RootNotFound.IService)));
        actual.Services[2].Operations.Length.ShouldBe(1);
        actual.Services[2].Operations[0].ServiceName.ShouldBe(nameof(RootNotFound.IContract1));
        actual.Services[2].Methods.ShouldBeEmpty();
        actual.Services[2].NotSupportedOperations.ShouldBeEmpty();
    }

    [Test]
    public void TransientInterfaceTree()
    {
        var rootType = _compilation.ResolveTypeSymbol(typeof(TransientInterface.IContract));
        var actual = ContractDescriptionBuilder.Build(rootType);

        actual.Interfaces.Length.ShouldBe(1);
        actual.Interfaces[0].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(TransientInterface.IService2)));

        actual.Services.Length.ShouldBe(2);

        actual.Services[0].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(TransientInterface.IContract)));
        actual.Services[0].Operations.ShouldBeEmpty();
        actual.Services[0].Methods.ShouldBeEmpty();
        actual.Services[0].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[1].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(TransientInterface.IService1)));
        actual.Services[1].Operations.Length.ShouldBe(1);
        actual.Services[1].Operations[0].ServiceName.ShouldBe(nameof(TransientInterface.IContract));
        actual.Services[1].Methods.ShouldBeEmpty();
        actual.Services[1].NotSupportedOperations.ShouldBeEmpty();
    }

    [Test]
    public void TransientGenericInterfaceTree()
    {
        var rootType = _compilation.ResolveTypeSymbol(typeof(TransientGenericInterface.IContract<int>));
        var actual = ContractDescriptionBuilder.Build(rootType);

        actual.Interfaces.ShouldBeEmpty();
        actual.Services.Length.ShouldBe(3);

        actual.Services[0].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(TransientGenericInterface.IContract<int>)));
        actual.Services[0].Operations.ShouldBeEmpty();
        actual.Services[0].Methods.ShouldBeEmpty();
        actual.Services[0].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[1].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(TransientGenericInterface.IService1)));
        actual.Services[1].Operations.Length.ShouldBe(1);
        actual.Services[1].Operations[0].ServiceName.ShouldBe("IContract-Int32");
        actual.Services[1].Methods.ShouldBeEmpty();
        actual.Services[1].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[2].InterfaceType.ShouldBe(_compilation.ResolveTypeSymbol(typeof(TransientGenericInterface.IService2<int>)));
        actual.Services[1].Operations.Length.ShouldBe(1);
        actual.Services[1].Operations[0].ServiceName.ShouldBe("IContract-Int32");
        actual.Services[2].Methods.ShouldBeEmpty();
        actual.Services[2].NotSupportedOperations.ShouldBeEmpty();
    }
}