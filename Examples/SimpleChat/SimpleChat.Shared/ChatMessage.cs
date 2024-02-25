using System;
using System.Runtime.Serialization;

namespace SimpleChat.Shared;

[DataContract]
public class ChatMessage
{
    // uniquely identifies a chart client application
    [DataMember(Order = 0)]
    public string? ConnectionId { get; set; }

    // UTC
    [DataMember(Order = 1)]
    public DateTime Time { get; set; }

    // the author of the message
    [DataMember(Order = 2)]
    public string Author { get; set; } = null!;

    // the text of the message
    [DataMember(Order = 3)]
    public string Content { get; set; } = null!;
}