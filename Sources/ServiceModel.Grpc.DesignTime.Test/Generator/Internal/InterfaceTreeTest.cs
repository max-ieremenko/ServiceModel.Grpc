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

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal
{
    [TestFixture]
    public partial class InterfaceTreeTest
    {
        private static readonly Compilation Compilation = CSharpCompilation
            .Create(
                nameof(InterfaceTreeTest),
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(int).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(InterfaceTreeTest).Assembly.Location)
                });

        [Test]
        public void NonServiceContractTree()
        {
            var rootType = Compilation.GetTypeByMetadataName(typeof(NonServiceContract.IService));
            var sut = new InterfaceTree(rootType);

            sut.Interfaces.Count.ShouldBe(1);
            sut.Interfaces[0].ShouldBe(rootType);

            sut.Services.ShouldBeEmpty();
        }

        [Test]
        [TestCase(typeof(OneContractRoot.IContract))]
        [TestCase(typeof(OneContractRoot.Contract))]
        public void OneContractRootTree(Type rootType)
        {
            var rootTypeSymbol = Compilation.GetTypeByMetadataName(rootType);
            var sut = new InterfaceTree(rootTypeSymbol);

            sut.Interfaces.Count.ShouldBe(1);
            sut.Interfaces[0].Name.ShouldBe(nameof(IDisposable));

            sut.Services.Count.ShouldBe(3);

            sut.Services[0].ServiceName.ShouldBe(nameof(OneContractRoot.IContract));
            sut.Services[0].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(OneContractRoot.IContract)));

            sut.Services[1].ServiceName.ShouldBe(nameof(OneContractRoot.IContract));
            sut.Services[1].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(OneContractRoot.IService1)));

            sut.Services[2].ServiceName.ShouldBe(nameof(OneContractRoot.IContract));
            sut.Services[2].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(OneContractRoot.IService2)));
        }

        [Test]
        public void AttachToMostTopContractTree()
        {
            var rootType = Compilation.GetTypeByMetadataName(typeof(AttachToMostTopContract.IContract2));
            var sut = new InterfaceTree(rootType);

            sut.Interfaces.ShouldBeEmpty();

            sut.Services.Count.ShouldBe(4);

            sut.Services[0].ServiceName.ShouldBe(nameof(AttachToMostTopContract.IContract2));
            sut.Services[0].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(AttachToMostTopContract.IContract2)));

            sut.Services[1].ServiceName.ShouldBe(nameof(AttachToMostTopContract.IContract1));
            sut.Services[1].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(AttachToMostTopContract.IContract1)));

            sut.Services[2].ServiceName.ShouldBe(nameof(AttachToMostTopContract.IContract2));
            sut.Services[2].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(AttachToMostTopContract.IService1)));

            sut.Services[3].ServiceName.ShouldBe(nameof(AttachToMostTopContract.IContract2));
            sut.Services[3].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(AttachToMostTopContract.IService2)));
        }

        [Test]
        public void RootNotFoundTree()
        {
            var rootType = Compilation.GetTypeByMetadataName(typeof(RootNotFound.Contract));
            var sut = new InterfaceTree(rootType);

            sut.Interfaces.ShouldBeEmpty();

            sut.Services.Count.ShouldBe(3);

            sut.Services[0].ServiceName.ShouldBe(nameof(RootNotFound.IContract1));
            sut.Services[0].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(RootNotFound.IContract1)));

            sut.Services[1].ServiceName.ShouldBe(nameof(RootNotFound.IContract2));
            sut.Services[1].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(RootNotFound.IContract2)));

            sut.Services[2].ServiceName.ShouldBe(nameof(RootNotFound.IContract1));
            sut.Services[2].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(RootNotFound.IService)));
        }

        [Test]
        public void TransientInterfaceTree()
        {
            var rootType = Compilation.GetTypeByMetadataName(typeof(TransientInterface.IContract));
            var sut = new InterfaceTree(rootType);

            sut.Interfaces.Count.ShouldBe(1);
            sut.Interfaces[0].ShouldBe(Compilation.GetTypeByMetadataName(typeof(TransientInterface.IService2)));

            sut.Services.Count.ShouldBe(2);

            sut.Services[0].ServiceName.ShouldBe(nameof(TransientInterface.IContract));
            sut.Services[0].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(TransientInterface.IContract)));

            sut.Services[1].ServiceName.ShouldBe(nameof(TransientInterface.IContract));
            sut.Services[1].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(TransientInterface.IService1)));
        }

        [Test]
        public void TransientGenericInterfaceTree()
        {
            var rootType = Compilation.GetTypeByMetadataName(typeof(TransientGenericInterface.IContract<int>));
            var sut = new InterfaceTree(rootType);

            sut.Interfaces.ShouldBeEmpty();
            sut.Services.Count.ShouldBe(3);

            sut.Services[0].ServiceName.ShouldBe("IContract-Int32");
            sut.Services[0].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(TransientGenericInterface.IContract<int>)));

            sut.Services[1].ServiceName.ShouldBe("IContract-Int32");
            sut.Services[1].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(TransientGenericInterface.IService2<int>)));

            sut.Services[2].ServiceName.ShouldBe("IContract-Int32");
            sut.Services[2].ServiceType.ShouldBe(Compilation.GetTypeByMetadataName(typeof(TransientGenericInterface.IService1)));
        }
    }
}
