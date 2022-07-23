using System.Runtime.Serialization;

namespace Contract
{
    [DataContract]
    public class UnexpectedErrorDetail
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }

        [DataMember(Order = 2)]
        public string MethodName { get; set; }

        [DataMember(Order = 3)]
        public string ExceptionType { get; set; }

        [DataMember(Order = 4)]
        public string FullException { get; set; }
    }
}
