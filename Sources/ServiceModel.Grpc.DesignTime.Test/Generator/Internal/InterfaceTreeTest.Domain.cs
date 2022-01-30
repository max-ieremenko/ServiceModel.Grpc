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
using System.ServiceModel;

//// ReSharper disable OperationContractWithoutServiceContract

namespace ServiceModel.Grpc.DesignTime.Generator.Internal
{
    public partial class InterfaceTreeTest
    {
        public static class NonServiceContract
        {
            public interface IService
            {
                [OperationContract]
                void Method();
            }
        }

        public static class OneContractRoot
        {
            public interface IService1 : IDisposable
            {
                [OperationContract]
                void Method1();
            }

            public interface IService2
            {
                [OperationContract]
                void Method2();
            }

            [ServiceContract]
            public interface IContract : IService1, IService2
            {
            }

            public sealed class Contract : IContract
            {
                public void Method1() => throw new NotImplementedException();

                public void Method2() => throw new NotImplementedException();

                public void Dispose() => throw new NotImplementedException();
            }
        }

        // attach non ServiceContract to the most top: IService1 must be attached to IContract2
        // other behavior does not make sense, the following must work:
        // - call IService1 via IContract2
        // - call IService1 via IContract1
        public static class AttachToMostTopContract
        {
            public interface IService1
            {
                [OperationContract]
                void Method1();
            }

            public interface IService2
            {
                [OperationContract]
                void Method2();
            }

            [ServiceContract]
            public interface IContract1 : IService1, IService2
            {
            }

            [ServiceContract]
            public interface IContract2 : IContract1
            {
            }
        }

        public static class RootNotFound
        {
            public interface IService
            {
                [OperationContract]
                void Method();
            }

            [ServiceContract]
            public interface IContract1 : IService
            {
            }

            [ServiceContract]
            public interface IContract2 : IService
            {
            }

            public sealed class Contract : IContract1, IContract2
            {
                public void Method() => throw new NotImplementedException();
            }
        }

        public static class TransientInterface
        {
            public interface IService1
            {
                [OperationContract]
                void Method();
            }

            public interface IService2 : IService1
            {
            }

            [ServiceContract]
            public interface IContract : IService2
            {
            }
        }
    }
}
