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

using System.ServiceModel;

namespace ServiceModel.Grpc.AspNetCore;

public partial class NameConflictsTest
{
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract(Name = "Sum2")]
        int Sum(int x, int y);

        [OperationContract(Name = "Sum3")]
        int Sum(int x, int y, int z);
    }

    private sealed class Calculator : ICalculator
    {
        public int Sum(int x, int y)
        {
            return x + y + 1;
        }

        public int Sum(int x, int y, int z)
        {
            return x + y + z - 1;
        }
    }
}