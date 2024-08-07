﻿using System;
using Contract;
using ServiceModel.Grpc.Interceptors;

namespace Client.Shared;

public sealed class CustomClientFaultDetailDeserializer : DefaultClientFaultDetailDeserializer
{
    public override Type DeserializeDetailType(string typePayload)
    {
        if (typePayload == ErrorDetailTypes.UnexpectedErrorDetailTypeName)
        {
            return typeof(UnexpectedErrorDetail);
        }

        return base.DeserializeDetailType(typePayload);
    }
}