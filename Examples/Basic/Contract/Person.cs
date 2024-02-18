using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public class Person
{
    [DataMember]
    public string? FullName { get; set; }

    [DataMember]
    public DateTime DateOfBirth { get; set; }

    [DataMember]
    public IList<Address>? Addresses { get; set; }
}