// <copyright>
// Copyright Max Ieremenko
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

using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

public sealed class MessageDescriptionComparer : IEqualityComparer<IMessageDescription>
{
    public static readonly MessageDescriptionComparer Default = new();

    private readonly SymbolEqualityComparer _symbolComparer;

    public MessageDescriptionComparer(SymbolEqualityComparer symbolComparer)
    {
        _symbolComparer = symbolComparer;
    }

    private MessageDescriptionComparer()
        : this(SymbolEqualityComparer.Default)
    {
    }

    public bool Equals(IMessageDescription x, IMessageDescription y)
    {
        if (x.Properties.Length != y.Properties.Length)
        {
            return false;
        }

        for (var i = 0; i < x.Properties.Length; i++)
        {
            if (!_symbolComparer.Equals(x.Properties[i], y.Properties[i]))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(IMessageDescription obj)
    {
        if (obj.Properties.Length == 0)
        {
            return 0;
        }

        var result = _symbolComparer.GetHashCode(obj.Properties[0]);
        for (var i = 1; i < obj.Properties.Length; i++)
        {
            result = Combine(result, _symbolComparer.GetHashCode(obj.Properties[i]));
        }

        return result;
    }

    private static int Combine(int x, int y)
    {
        var hash = 17;
        unchecked
        {
            hash = (hash * 31) + x;
            hash = (hash * 31) + y;
        }

        return hash;
    }
}