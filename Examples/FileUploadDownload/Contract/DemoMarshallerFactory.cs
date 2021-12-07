using System;
using System.Buffers;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;

namespace Contract
{
    public sealed class DemoMarshallerFactory : IMarshallerFactory
    {
        public static readonly IMarshallerFactory Default = new DemoMarshallerFactory();

        public Marshaller<T> CreateMarshaller<T>()
        {
            // do not user serializer, write/read RentedArray directly
            if (typeof(T) == typeof(Message<RentedArray>))
            {
                return (Marshaller<T>)RentedArrayMarshaller.Default;
            }

            // do not user serializer, write/read byte[] directly
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
            var array = message.Value1;
            context.Complete(array);
        }

        private static Message<byte[]> Deserialize(DeserializationContext context)
        {
            var array = context.PayloadAsNewBuffer();
            return new Message<byte[]>(array);
        }
    }

    internal sealed class RentedArrayMarshaller
    {
        public static readonly object Default = new Marshaller<Message<RentedArray>>(Serialize, Deserialize);

        private static void Serialize(Message<RentedArray> message, SerializationContext context)
        {
            var rentedArray = message.Value1;

            context.SetPayloadLength(rentedArray.Length);
            
            var writer = context.GetBufferWriter();
            var span = writer.GetSpan(rentedArray.Length);
            rentedArray.Array.AsSpan(0, rentedArray.Length).CopyTo(span);
            writer.Advance(rentedArray.Length);

            context.Complete();
        }

        private static Message<RentedArray> Deserialize(DeserializationContext context)
        {
            var sequence = context.PayloadAsReadOnlySequence();
            var length = context.PayloadLength;
            var source = sequence.Slice(0, length);
            
            var rentedArray = RentedArray.Rent(length);
            source.CopyTo(rentedArray.Array.AsSpan(0, length));
            
            return new Message<RentedArray>(rentedArray);
        }
    }
}
