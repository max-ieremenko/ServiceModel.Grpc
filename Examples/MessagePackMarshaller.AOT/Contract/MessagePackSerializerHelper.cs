using Contract.Resolvers;
using MessagePack;
using MessagePack.Resolvers;

namespace Contract;

public static class MessagePackSerializerHelper
{
    /// <summary>
    /// Create options with MessagePack generated formatters (see generated <see cref="SharedMessagePackResolver"/>)
    /// </summary>
    public static MessagePackSerializerOptions CreateApplicationOptions()
    {
        var resolver = CompositeResolver.Create(SharedMessagePackResolver.Instance, StandardResolver.Instance);
        return MessagePackSerializerOptions.Standard.WithResolver(resolver);
    }
}