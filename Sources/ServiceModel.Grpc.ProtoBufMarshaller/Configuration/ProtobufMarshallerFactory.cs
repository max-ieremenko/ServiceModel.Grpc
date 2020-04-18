using System;
using System.IO;
using Grpc.Core;
using ProtoBuf.Meta;

namespace ServiceModel.Grpc.Configuration
{
    public sealed class ProtobufMarshallerFactory : IMarshallerFactory
    {
        public static readonly IMarshallerFactory Default = new ProtobufMarshallerFactory();

        private readonly RuntimeTypeModel _runtimeTypeModel;

        public ProtobufMarshallerFactory()
        {
        }

        public ProtobufMarshallerFactory(RuntimeTypeModel runtimeTypeModel)
        {
            if (runtimeTypeModel == null)
            {
                throw new ArgumentNullException(nameof(runtimeTypeModel));
            }

            _runtimeTypeModel = runtimeTypeModel;
        }

        public Marshaller<T> CreateMarshaller<T>()
        {
            if (_runtimeTypeModel == null)
            {
                return ProtobufMarshaller<T>.Default;
            }

            return new Marshaller<T>(Serialize, Deserialize<T>);
        }

        internal static byte[] Serialize<T>(T value, RuntimeTypeModel runtimeTypeModel)
        {
            using (var buffer = new MemoryStream())
            {
                runtimeTypeModel.Serialize(buffer, value);
                return buffer.ToArray();
            }
        }

        internal static T Deserialize<T>(byte[] value, RuntimeTypeModel runtimeTypeModel)
        {
            if (value == null)
            {
                return default;
            }

            using (var buffer = new MemoryStream(value))
            {
                return (T)runtimeTypeModel.Deserialize(buffer, null, typeof(T));
            }
        }

        private byte[] Serialize<T>(T value) => Serialize(value, _runtimeTypeModel);

        private T Deserialize<T>(byte[] value) => Deserialize<T>(value, _runtimeTypeModel);
    }
}
