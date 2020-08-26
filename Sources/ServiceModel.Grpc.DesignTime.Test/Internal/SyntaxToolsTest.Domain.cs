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
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel;

namespace ServiceModel.Grpc.DesignTime.Internal
{
    public partial class SyntaxToolsTest
    {
        [ServiceContract]
        private interface I1 : IDisposable
        {
            [OperationContract]
            string Overload();

            string Overload(int x);
        }

        private sealed class FullNameCases
        {
            [DisplayName("int")]
            public int C1() => throw new NotImplementedException();

            [DisplayName("string")]
            public string C2() => throw new NotImplementedException();

            [DisplayName("int?")]
            public int? C3() => throw new NotImplementedException();

            [DisplayName("IAsyncEnumerable<int>")]
            public IAsyncEnumerable<int> C4() => throw new NotImplementedException();

            [DisplayName("int[]")]
            public int[] C5() => throw new NotImplementedException();

            [DisplayName("int?[]")]
            public int?[] C6() => throw new NotImplementedException();

            [DisplayName("ServiceModel.Grpc.DesignTime.Internal.SyntaxToolsTest.I1")]
            public I1 C7() => throw new NotImplementedException();

            [DisplayName("void")]
            public void C8() => throw new NotImplementedException();

            [DisplayName("T2")]
            public T2 Generic<T1, T2>(T1 value) => throw new NotImplementedException();
        }
    }
}
