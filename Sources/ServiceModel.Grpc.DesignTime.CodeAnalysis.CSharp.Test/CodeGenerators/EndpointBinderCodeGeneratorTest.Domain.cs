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

using System.Reflection;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

public partial class EndpointBinderCodeGeneratorTest
{
    [AttributeUsage(AttributeTargets.All)]
    public class MyAttribute : Attribute
    {
        public MyAttribute()
        {
        }

        public MyAttribute(string? value1)
        {
            Value1 = value1;
        }

        public MyAttribute(BindingFlags value2)
        {
            Value2 = value2;
        }

        public MyAttribute(string? value1, BindingFlags value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        public string? Value1 { get; set; }

        public BindingFlags Value2 { get; set; }
    }

    public class WriteNewAttributeCases
    {
        [My]
        public string Case1() => FinishCode("()");

        [My("abc")]
        public string Case2() => FinishCode("(\"abc\")");

        [My(null)]
        public string Case3() => FinishCode("(null)");

        [My(BindingFlags.Instance)]
        public string Case4() => FinishCode("(System.Reflection.BindingFlags.Instance)");

        [My(Value2 = BindingFlags.Instance)]
        public string Case5() => FinishCode(" { Value2 = System.Reflection.BindingFlags.Instance }");

        [My("abc", BindingFlags.Instance, Value1 = null, Value2 = BindingFlags.Instance)]
        public string Case6() => FinishCode("(\"abc\", System.Reflection.BindingFlags.Instance) { Value1 = null, Value2 = System.Reflection.BindingFlags.Instance }");

        private static string FinishCode(string template) =>
            $"new global::{typeof(EndpointBinderCodeGeneratorTest).FullName}.{nameof(MyAttribute)}{template}";
    }
}