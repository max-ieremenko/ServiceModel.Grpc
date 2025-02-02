using MessagePack;
using MessagePack.Resolvers;

namespace Contract;

public static class MessagePackSerializerHelper
{
    /// <summary>
    /// Create options with MessagePack generated formatters (see generated <see cref="GeneratedMessagePackResolver"/>)
    /// </summary>
    public static MessagePackSerializerOptions CreateApplicationOptions()
    {
        var resolver = CompositeResolver.Create(GeneratedMessagePackResolver.Instance, StandardResolver.Instance);
        return MessagePackSerializerOptions.Standard.WithResolver(resolver);
    }
}