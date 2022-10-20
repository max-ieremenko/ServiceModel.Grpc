using System.Runtime.Serialization;

namespace Contract;

[DataContract]
public class FileMetadata
{
    public FileMetadata()
    {
    }

    public FileMetadata(string fileName, long size)
    {
        FileName = fileName;
        Size = size;
    }

    [DataMember]
    public string FileName { get; set; }

    [DataMember]
    public long Size { get; set; }

    public override string ToString()
    {
        return string.Format("{0} {1}", FileName, StreamExtensions.SizeToString(Size));
    }
}