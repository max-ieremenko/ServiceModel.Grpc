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
using System.ComponentModel;

namespace ServiceModel.Grpc.Emit;

public partial class ReflectionToolsTest
{
    private interface I1
    {
        string Overload();

        string Overload(int x);
    }

    private interface I2
    {
        string Overload(int x);
    }

    private sealed class Implementation : I1, I2
    {
        [Description("I1.Overload")]
        public string Overload() => throw new NotImplementedException();

        [Description("I1.Overload(int)")]
        public string Overload(int x) => throw new NotImplementedException();

        [Description("I2.Overload(int)")]
        string I2.Overload(int x) => throw new NotImplementedException();
    }
}