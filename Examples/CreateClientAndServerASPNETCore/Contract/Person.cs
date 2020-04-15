using System.Runtime.Serialization;

namespace Contract
{
    [DataContract]
    public class Person
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string SecondName { get; set; }
    }
}
