using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;

namespace Contract;

// the only purpose is to skip serialization and write/read directly
public sealed class DemoMarshallerFactory : IMarshallerFactory
{
    public static readonly IMarshallerFactory Default = new DemoMarshallerFactory();

    public Marshaller<T> CreateMarshaller<T>()
    {
        // skip serializer, write/read byte[] directly
        if (typeof(T) == typeof(Message<byte[]>))
        {
            return (Marshaller<T>)ByteArrayMarshaller.Default;
        }

        return DataContractMarshallerFactory.Default.CreateMarshaller<T>();
    }
}

internal sealed class ByteArrayMarshaller
{
    public static readonly object Default = new Marshaller<Message<byte[]>>(Serialize, Deserialize);

    private static void Serialize(Message<byte[]> message, SerializationContext context)
    {
        var array = message.Value1!;
        context.Complete(array);
    }

    private static Message<byte[]> Deserialize(DeserializationContext context)
    {
        var array = context.PayloadAsNewBuffer();
        return new Message<byte[]>(array);
    }
}