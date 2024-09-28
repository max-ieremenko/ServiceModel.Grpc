using System;
using Contract;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors;

namespace Server.Services;

/// <summary>
/// Error handling: AOT compatible serialization of <see cref="InvalidRectangleError"/>
/// </summary>
internal sealed class ServerFaultDetailSerializer : IServerFaultDetailSerializer
{
    public string SerializeDetailType(Type detailType)
    {
        if (detailType == typeof(InvalidRectangleError))
        {
            return nameof(InvalidRectangleError);
        }

        throw new NotSupportedException();
    }

    public byte[] SerializeDetail(IMarshallerFactory marshallerFactory, object detail)
    {
        if (detail is not InvalidRectangleError error)
        {
            throw new NotSupportedException();
        }

        return MarshallerExtensions.Serialize(marshallerFactory.CreateMarshaller<InvalidRectangleError>(), error);
    }
}