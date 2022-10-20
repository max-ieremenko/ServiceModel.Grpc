using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public class ApplicationExceptionFaultDetail
{
    [DataMember]
    public string Message { get; set; }
}