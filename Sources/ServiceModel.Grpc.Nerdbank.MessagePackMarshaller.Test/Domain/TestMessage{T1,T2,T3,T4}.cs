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

using PolyType;

namespace ServiceModel.Grpc.Domain;

internal sealed class TestMessage<T1, T2, T3, T4>
{
    public TestMessage()
    {
    }

    public TestMessage(T1? value1, T2? value2, T3? value3, T4? value4)
    {
        Value1 = value1;
        Value2 = value2;
        Value3 = value3;
        Value4 = value4;
    }

    [PropertyShape(Order = 1)]
    public object? Reserved { get; set; }

    [PropertyShape(Order = 2)]
    public T1? Value1 { get; set; }

    [PropertyShape(Order = 3)]
    public T2? Value2 { get; set; }

    [PropertyShape(Order = 4)]
    public T3? Value3 { get; set; }

    [PropertyShape(Order = 5)]
    public T4? Value4 { get; set; }

    internal static T1? Get1(ref TestMessage<T1, T2, T3, T4> message) => message.Value1;

    internal static void Set1(ref TestMessage<T1, T2, T3, T4> message, T1? value) => message.Value1 = value;

    internal static T2? Get2(ref TestMessage<T1, T2, T3, T4> message) => message.Value2;

    internal static void Set2(ref TestMessage<T1, T2, T3, T4> message, T2? value) => message.Value2 = value;

    internal static T3? Get3(ref TestMessage<T1, T2, T3, T4> message) => message.Value3;

    internal static void Set3(ref TestMessage<T1, T2, T3, T4> message, T3? value) => message.Value3 = value;

    internal static T4? Get4(ref TestMessage<T1, T2, T3, T4> message) => message.Value4;

    internal static void Set4(ref TestMessage<T1, T2, T3, T4> message, T4? value) => message.Value4 = value;
}