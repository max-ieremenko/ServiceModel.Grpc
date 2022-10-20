using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public class Person
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    public string FirstName { get; set; }

    [DataMember]
    public string SecondName { get; set; }

    public override string ToString()
    {
        return $"Id:{Id}, {FirstName} {SecondName}";
    }
}