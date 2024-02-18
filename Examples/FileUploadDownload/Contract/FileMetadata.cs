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
    public string FileName { get; set; } = null!;

    [DataMember]
    public long Size { get; set; }

    public override string ToString() => $"{FileName} {StreamExtensions.SizeToString(Size)}";
}