using System;
using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public class Person
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public DateTime BirthDay { get; set; }
}