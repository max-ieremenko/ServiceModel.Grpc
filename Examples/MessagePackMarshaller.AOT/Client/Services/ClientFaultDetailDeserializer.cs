using System;
using Contract;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors;

namespace Client.Services;

/// <summary>
/// Error handling: AOT compatible deserialization of <see cref="InvalidRectangleError"/>
/// </summary>
internal sealed class ClientFaultDetailDeserializer : IClientFaultDetailDeserializer
{
    public Type DeserializeDetailType(string typePayload)
    {
        if (typePayload == nameof(InvalidRectangleError))
        {
            return typeof(InvalidRectangleError);
        }

        throw new NotSupportedException();
    }

    public object DeserializeDetail(IMarshallerFactory marshallerFactory, Type detailType, byte[] detailPayload)
    {
        if (detailType != typeof(InvalidRectangleError))
        {
            throw new NotSupportedException();
        }

        return MarshallerExtensions.Deserialize(marshallerFactory.CreateMarshaller<InvalidRectangleError>(), detailPayload);
    }
}