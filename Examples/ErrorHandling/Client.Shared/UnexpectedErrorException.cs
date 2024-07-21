using System;
using Contract;

namespace Client.Shared;

public class UnexpectedErrorException : SystemException
{
    public UnexpectedErrorException(UnexpectedErrorDetail detail)
        : base(detail.Message)
    {
        Detail = detail;
    }

    public UnexpectedErrorDetail Detail { get; }
}