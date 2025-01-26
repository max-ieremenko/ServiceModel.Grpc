﻿using System;
using Contract;

namespace Server.Services.Filters;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public abstract class ValidateParameterAttribute : Attribute
{
    public abstract void Validate(object? parameterValue, string? parameterName, DivideByResult result);
}

public sealed class NotAttribute : ValidateParameterAttribute
{
    public NotAttribute(object value)
    {
        Value = value;
    }

    public object? Value { get; }

    public override void Validate(object? parameterValue, string? parameterName, DivideByResult result)
    {
        if (Equals(Value, parameterValue))
        {
            result.IsSuccess = false;
            result.ErrorMessages.Add($"{parameterName} cannot be {Value}.");
        }
    }
}

public sealed class GreaterThanAttribute : ValidateParameterAttribute
{
    public GreaterThanAttribute(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public override void Validate(object? parameterValue, string? parameterName, DivideByResult result)
    {
        if (parameterValue is int typedValue)
        {
            if (typedValue <= Value)
            {
                result.IsSuccess = false;
                result.ErrorMessages.Add($"{parameterName} must be greater than {Value}.");
            }
        }
        else
        {
            result.IsSuccess = false;
            result.ErrorMessages.Add($"{parameterName} must be Int32.");
        }
    }
}