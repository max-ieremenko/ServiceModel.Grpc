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
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

[TestFixture]
public partial class EmitExtremelyLarge
{
    private Func<IOperationDescriptor> _getTestDescriptor = null!;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        var description = ContractDescriptionBuilder.Build(typeof(ILarge));
        var contractType = EmitContractBuilder.Build(ProxyAssembly.DefaultModule, description);

        _getTestDescriptor = contractType
            .StaticMethod(NamingContract.Contract.DescriptorMethod(nameof(ILarge.Test)))
            .CreateDelegate<Func<IOperationDescriptor>>();
    }

    [Test]
    public void ContractBuilder()
    {
        var descriptor = _getTestDescriptor();

        descriptor.GetContractMethod().ShouldBe(typeof(ILarge).InstanceMethod(nameof(ILarge.Test)));
        descriptor.GetRequestParameters().ShouldBe(Enumerable.Range(0, descriptor.GetContractMethod().GetParameters().Length).ToArray());
    }

    [Test]
    public void MessageAccessor()
    {
        var descriptor = _getTestDescriptor();

        var names = descriptor.GetContractMethod().GetParameters().Select(i => i.Name).ToArray();
        var genericArgs = descriptor.GetContractMethod().GetParameters().Select(i => i.ParameterType).ToArray();

        var sut = descriptor.GetRequestAccessor();

        sut.Names.ShouldBe(names);
        sut.GetInstanceType().GenericTypeArguments.ShouldBe(genericArgs);

        var message = sut.CreateNew();
        message.GetType().GenericTypeArguments.ShouldBe(genericArgs);

        for (var i = 0; i < names.Length; i++)
        {
            sut.GetValueType(i).ShouldBe(typeof(int));
            sut.SetValue(message, i, i + 1);
            sut.GetValue(message, i).ShouldBe(i + 1);
        }
    }
}