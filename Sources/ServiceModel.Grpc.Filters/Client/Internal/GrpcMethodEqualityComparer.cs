// <copyright>
// Copyright 2023 Max Ieremenko
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
using System.Collections.Generic;
using Grpc.Core;

namespace ServiceModel.Grpc.Client.Internal;

internal sealed class GrpcMethodEqualityComparer : IEqualityComparer<IMethod>
{
    public static readonly IEqualityComparer<IMethod> Instance = new GrpcMethodEqualityComparer();

    private GrpcMethodEqualityComparer()
    {
    }

    public bool Equals(IMethod? x, IMethod? y)
    {
        if (x == null)
        {
            return y == null;
        }

        if (y == null)
        {
            return false;
        }

        return ReferenceEquals(x, y) || StringComparer.OrdinalIgnoreCase.Equals(x.FullName, y.FullName);
    }

    public int GetHashCode(IMethod? method)
    {
        if (method == null)
        {
            return 0;
        }

        return StringComparer.OrdinalIgnoreCase.GetHashCode(method.FullName);
    }
}