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
using System.Reflection;

#pragma warning disable SA1411 // Attribute constructor should not use unnecessary parenthesis
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

public partial class CSharpServiceEndpointBinderBuilderTest
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
        [My()]
        public string Case1() => FinishCode("()");

        [My]
        public string Case2() => FinishCode("()");

        [My]
        public string Case3() => FinishCode("()");

        [My("abc")]
        public string Case4() => FinishCode("(\"abc\")");

        [My("abc")]
        public string Case5() => FinishCode("(\"abc\")");

        [My(null)]
        public string Case6() => FinishCode("(null)");

        [My(BindingFlags.Instance)]
        public string Case7() => FinishCode("(System.Reflection.BindingFlags.Instance)");

        [My(Value2 = BindingFlags.Instance)]
        public string Case8() => FinishCode(" { Value2 = System.Reflection.BindingFlags.Instance }");

        [My("abc", BindingFlags.Instance, Value1 = null, Value2 = BindingFlags.Instance)]
        public string Case9() => FinishCode("(\"abc\", System.Reflection.BindingFlags.Instance) { Value1 = null, Value2 = System.Reflection.BindingFlags.Instance }");

        private static string FinishCode(string template)
        {
            return string.Format(
                "new global::{0}.{1}{2}",
                typeof(CSharpServiceEndpointBinderBuilderTest).FullName,
                nameof(MyAttribute),
                template);
        }
    }
}