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

using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServiceModel.Grpc.DesignTime.Internal
{
    public partial class ServiceContractTest
    {
        [ServiceContract]
        private interface I1
        {
            [OperationContract]
            string Operation();
        }

        [ServiceContract(Name = "Service2")]
        private interface I2
        {
            [OperationContract(Name = "Method")]
            string Operation();
        }

        [ServiceContract(Name = "Service2", Namespace = "Test")]
        private interface I3
        {
            [OperationContract]
            string Operation();
        }

        [ServiceContract]
        private interface IGeneric1<TValue>
        {
        }

        [ServiceContract(Name = "Service2")]
        private interface IGeneric2<TValue1, TValue2>
        {
        }

        [DataContract(Name = "Some-Data")]
        private sealed class SomeData
        {
        }
    }
}
