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

using System;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed class MessageProxy
{
    private readonly IMessageAccessor _messageAccessor;

    public MessageProxy(IMessageAccessor messageAccessor)
    {
        _messageAccessor = messageAccessor;
    }

    public string[] Names => _messageAccessor.Names;

    public int GetPropertyIndex(string name)
    {
        for (var i = 0; i < Names.Length; i++)
        {
            if (string.Equals(name, Names[i], StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(name), $"Parameter [{name}] not found.");
    }

    public object? GetValue(object message, int property)
    {
        ValidateIndex(property);
        return _messageAccessor.GetValue(message, property);
    }

    public void SetValue(object message, int property, object? value)
    {
        ValidateIndex(property);
        _messageAccessor.SetValue(message, property, value);
    }

    public object CreateDefault() => _messageAccessor.CreateNew();

    private void ValidateIndex(int property)
    {
        if (property < 0 || property >= Names.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(property));
        }
    }
}