// <copyright>
// Copyright 2020 Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Runtime.CompilerServices;

namespace ServiceModel.Grpc;

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