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
using MessagePack;
using ProtoBuf;

namespace ServiceModel.Grpc.Benchmarks.Domain;

[DataContract]
[ProtoContract]
[MessagePackObject]
public class SomeObject
{
    [DataMember]
    [ProtoMember(1)]
    [Key(1)]
    public string? StringScalar { get; set; }

    [DataMember]
    [ProtoMember(2)]
    [Key(2)]
    public DateTime DateScalar { get; set; }

    [DataMember]
    [ProtoMember(3)]
    [Key(3)]
    public float SingleScalar { get; set; }

    [DataMember]
    [ProtoMember(4)]
    [Key(4)]
    public int Int32Scalar { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [ProtoMember(5, IsPacked = true)]
    [Key(5)]
    public float[]? SingleArray { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [ProtoMember(6, IsPacked = true)]
    [Key(6)]
    public int[]? Int32Array { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [ProtoMember(7, IsPacked = true)]
    [Key(7)]
    public double[]? DoubleArray { get; set; }
}