// <copyright>
// Copyright 2020 Max Ieremenko
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
using System.ServiceModel;
using Grpc.Core;
using Moq;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Internal.Emit;

public partial class EmitClientBuilderBuilderTest
{
    [ServiceContract]
    public interface ISomeContract
    {
        [OperationContract]
        void SomeOperation();
    }

    public sealed class ContractMock
    {
#pragma warning disable SA1401
        public static RuntimeMethodHandle SomeOperationDefinition;

        public IMethod MethodSomeOperation;
#pragma warning restore SA1401

        public ContractMock(IMarshallerFactory marshallerFactory)
        {
            MarshallerFactory = marshallerFactory;
            MethodSomeOperation = new Mock<IMethod>(MockBehavior.Strict).Object;
            SomeOperationDefinition = typeof(ISomeContract).InstanceMethod(nameof(ISomeContract.SomeOperation)).MethodHandle;
        }

        public IMarshallerFactory MarshallerFactory { get; }
    }

    public sealed class ClientMock : ISomeContract
    {
        public ClientMock(
            CallInvoker callInvoker,
            ContractMock contract,
            Func<CallOptions> defaultCallOptionsFactory,
            IClientCallFilterHandlerFactory? filterHandlerFactory)
        {
            CallInvoker = callInvoker;
            Contract = contract;
            DefaultCallOptionsFactory = defaultCallOptionsFactory;
            FilterHandlerFactory = filterHandlerFactory;
        }

        public CallInvoker CallInvoker { get; }

        public ContractMock Contract { get; }

        public Func<CallOptions> DefaultCallOptionsFactory { get; }

        public IClientCallFilterHandlerFactory? FilterHandlerFactory { get; }

        public void SomeOperation() => throw new NotSupportedException();
    }
}