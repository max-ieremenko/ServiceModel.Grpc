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

using MessagePack;
using PolyType;

namespace ServiceModel.Grpc.Domain;

[MessagePackObject]
public sealed record TestMessage<T1, T2>
{
    public TestMessage()
    {
    }

    public TestMessage(T1? value1, T2? value2)
    {
        Value1 = value1;
        Value2 = value2;
    }

    [PropertyShape(Order = 0)]
    [IgnoreMember]
    public object? Reserved { get; set; }

    [PropertyShape(Order = 1)]
    [Key(1)]
    public T1? Value1 { get; set; }

    [PropertyShape(Order = 2)]
    [Key(2)]
    public T2? Value2 { get; set; }
}