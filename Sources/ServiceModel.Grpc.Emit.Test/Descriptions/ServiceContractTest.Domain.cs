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

using System.Runtime.Serialization;
using System.ServiceModel;
using Grpc.Core;

namespace ServiceModel.Grpc.Emit.Descriptions;

public partial class ServiceContractTest
{
    [ServiceContract]
    public interface IServiceContract
    {
        [OperationContract]
        void Empty();

        void Ignore();
    }

    [ServiceContract]
    public interface IGenericServiceContract<T>
    {
        [OperationContract]
        void Invoke(T value);
    }

    [BindServiceMethod(typeof(NativeGrpcService), nameof(BindService))]
    public abstract class NativeGrpcService
    {
        public static void BindService() => throw new NotImplementedException();
    }

    [DataContract(Name = "Some-Data")]
    public sealed class SomeData;
}