using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public class ApplicationErrorDetail
{
    [DataMember]
    public string Message { get; set; }
}