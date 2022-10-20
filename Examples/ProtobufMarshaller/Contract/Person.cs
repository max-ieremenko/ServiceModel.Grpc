using System;
using ProtoBuf;

namespace Contract;

[ProtoContract]
public class Person
{
    [ProtoMember(1)]
    public string Name { get; set; }

    [ProtoMember(2)]
    public DateTime BirthDay { get; set; }

    [ProtoMember(3)]
    public string CreatedBy { get; set; }
}