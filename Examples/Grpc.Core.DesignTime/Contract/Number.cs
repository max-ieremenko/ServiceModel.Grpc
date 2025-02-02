using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public record struct Number
{
    public Number(int value)
    {
        Value = value;
    }

    [DataMember]
    public int Value { get; set; }
}