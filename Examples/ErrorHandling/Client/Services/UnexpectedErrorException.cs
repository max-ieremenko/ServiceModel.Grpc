using System;
using Contract;

namespace Client.Services;

internal class UnexpectedErrorException : SystemException
{
    public UnexpectedErrorException(UnexpectedErrorDetail detail)
        : base(detail.Message)
    {
        Detail = detail;
    }

    public UnexpectedErrorDetail Detail { get; }
}