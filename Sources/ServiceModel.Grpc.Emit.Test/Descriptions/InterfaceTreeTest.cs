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
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions.Reflection;

namespace ServiceModel.Grpc.Emit.Descriptions;

[TestFixture]
public partial class InterfaceTreeTest
{
    [Test]
    public void NonServiceContractTree()
    {
        var sut = new InterfaceTree<Type>(typeof(NonServiceContract.IService), new ReflectType());

        sut.Interfaces.Count.ShouldBe(1);
        sut.Interfaces[0].ShouldBe(typeof(NonServiceContract.IService));

        sut.Services.ShouldBeEmpty();
    }

    [Test]
    [TestCase(typeof(OneContractRoot.IContract))]
    [TestCase(typeof(OneContractRoot.Contract))]
    public void OneContractRootTree(Type rootType)
    {
        var sut = new InterfaceTree<Type>(rootType, new ReflectType());

        sut.Interfaces.Count.ShouldBe(1);
        sut.Interfaces[0].ShouldBe(typeof(IDisposable));

        sut.Services.Count.ShouldBe(3);

        sut.Services[0].ServiceName.ShouldBe(nameof(OneContractRoot.IContract));
        sut.Services[0].ServiceType.ShouldBe(typeof(OneContractRoot.IContract));

        sut.Services[1].ServiceName.ShouldBe(nameof(OneContractRoot.IContract));
        sut.Services[1].ServiceType.ShouldBe(typeof(OneContractRoot.IService1));

        sut.Services[2].ServiceName.ShouldBe(nameof(OneContractRoot.IContract));
        sut.Services[2].ServiceType.ShouldBe(typeof(OneContractRoot.IService2));
    }

    [Test]
    public void AttachToMostTopContractTree()
    {
        var sut = new InterfaceTree<Type>(typeof(AttachToMostTopContract.IContract2), new ReflectType());

        sut.Interfaces.ShouldBeEmpty();

        sut.Services.Count.ShouldBe(4);

        sut.Services[0].ServiceName.ShouldBe(nameof(AttachToMostTopContract.IContract2));
        sut.Services[0].ServiceType.ShouldBe(typeof(AttachToMostTopContract.IContract2));

        sut.Services[1].ServiceName.ShouldBe(nameof(AttachToMostTopContract.IContract1));
        sut.Services[1].ServiceType.ShouldBe(typeof(AttachToMostTopContract.IContract1));

        sut.Services[2].ServiceName.ShouldBe(nameof(AttachToMostTopContract.IContract2));
        sut.Services[2].ServiceType.ShouldBe(typeof(AttachToMostTopContract.IService1));

        sut.Services[3].ServiceName.ShouldBe(nameof(AttachToMostTopContract.IContract2));
        sut.Services[3].ServiceType.ShouldBe(typeof(AttachToMostTopContract.IService2));
    }

    [Test]
    public void RootNotFoundTree()
    {
        var sut = new InterfaceTree<Type>(typeof(RootNotFound.Contract), new ReflectType());

        sut.Interfaces.ShouldBeEmpty();

        sut.Services.Count.ShouldBe(3);

        sut.Services[0].ServiceName.ShouldBe(nameof(RootNotFound.IContract1));
        sut.Services[0].ServiceType.ShouldBe(typeof(RootNotFound.IContract1));

        sut.Services[1].ServiceName.ShouldBe(nameof(RootNotFound.IContract2));
        sut.Services[1].ServiceType.ShouldBe(typeof(RootNotFound.IContract2));

        sut.Services[2].ServiceName.ShouldBe(nameof(RootNotFound.IContract1));
        sut.Services[2].ServiceType.ShouldBe(typeof(RootNotFound.IService));
    }

    [Test]
    public void TransientInterfaceTree()
    {
        var sut = new InterfaceTree<Type>(typeof(TransientInterface.IContract), new ReflectType());

        sut.Interfaces.Count.ShouldBe(1);
        sut.Interfaces[0].ShouldBe(typeof(TransientInterface.IService2));

        sut.Services.Count.ShouldBe(2);

        sut.Services[0].ServiceName.ShouldBe(nameof(TransientInterface.IContract));
        sut.Services[0].ServiceType.ShouldBe(typeof(TransientInterface.IContract));

        sut.Services[1].ServiceName.ShouldBe(nameof(TransientInterface.IContract));
        sut.Services[1].ServiceType.ShouldBe(typeof(TransientInterface.IService1));
    }
}