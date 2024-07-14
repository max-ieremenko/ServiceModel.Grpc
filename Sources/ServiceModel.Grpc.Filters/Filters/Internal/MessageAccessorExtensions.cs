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

using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

internal static class MessageAccessorExtensions
{
    public static object? GetValue(this IMessageAccessor accessor, object message, string property)
    {
        var index = GetPropertyIndex(accessor, property);
        return accessor.GetValue(message, index);
    }

    public static void SetValue(this IMessageAccessor accessor, object message, string property, object? value)
    {
        var index = GetPropertyIndex(accessor, property);
        accessor.SetValue(message, index, value);
    }

    private static int GetPropertyIndex(IMessageAccessor accessor, string name)
    {
        for (var i = 0; i < accessor.Names.Length; i++)
        {
            if (string.Equals(name, accessor.Names[i], StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(name), $"Parameter [{name}] not found.");
    }
}