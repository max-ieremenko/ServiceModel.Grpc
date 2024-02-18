using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public class Address
{
    [DataMember]
    public string? Country { get; set; }

    [DataMember]
    public string? City { get; set; }

    [DataMember]
    public string? PostCode { get; set; }

    [DataMember]
    public string? Street { get; set; }
}