using System;
using System.Runtime.Serialization;

namespace Contract
{
    [DataContract]
    public class Person
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public DateTime BirthDay { get; set; }

        [DataMember(Order = 3)]
        public string CreatedBy { get; set; }
    }
}
