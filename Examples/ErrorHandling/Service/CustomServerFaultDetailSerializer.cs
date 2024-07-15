using System;
using Contract;
using ServiceModel.Grpc.Interceptors;

namespace Service;

public sealed class CustomServerFaultDetailSerializer : DefaultServerFaultDetailSerializer
{
    public override string SerializeDetailType(Type detailType)
    {
        if (detailType == typeof(UnexpectedErrorDetail))
        {
            return ErrorDetailTypes.UnexpectedErrorDetailTypeName;
        }

        return base.SerializeDetailType(detailType);
    }
}