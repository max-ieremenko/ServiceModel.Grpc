using System;
using MessagePack;

namespace Contract
{
    [MessagePackObject]
    public class Person
    {
        [Key(1)]
        public string Name { get; set; }

        [Key(2)]
        public DateTime BirthDay { get; set; }

        [Key(3)]
        public string CreatedBy { get; set; }
    }
}
