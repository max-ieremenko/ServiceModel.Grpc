using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public class InvalidOperationExceptionFaultDetail
{
    [DataMember]
    public string Message { get; set; } = null!;

    [DataMember]
    public string StackTrace { get; set; } = null!;
}