using System;
using System.Runtime.CompilerServices;

namespace ServiceModel.Grpc
{
    internal static class ArgumentAssert
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertNotNull(this object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertNotNull(this string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AssertIsInstanceOf<T>(this object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }

            if (argument is T result)
            {
                return result;
            }

            throw new ArgumentOutOfRangeException(argumentName, "Expected {0} is {1}".FormatWith(argumentName, typeof(T)));
        }
    }
}
