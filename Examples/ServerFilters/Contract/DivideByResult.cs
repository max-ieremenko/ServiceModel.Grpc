using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public sealed class DivideByResult
{
    [DataMember]
    public bool IsSuccess { get; set; }

    [DataMember]
    public List<string> ErrorMessages { get; set; } = new List<string>();

    [DataMember]
    public int Result { get; set; }

    public override string ToString()
    {
        if (IsSuccess)
        {
            return Result.ToString();
        }

        return string.Join("; ", ErrorMessages);
    }
}