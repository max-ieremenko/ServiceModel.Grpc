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

using PolyType.Abstractions;

#pragma warning disable ServiceModelGrpcInternalAPI

namespace ServiceModel.Grpc.Configuration.Internal;

internal sealed class MessageShapeBuilder<TMessage> : IMessageShapeBuilder<TMessage>
    where TMessage : new()
{
    private readonly MessageTypeShapeCache _cache;
    private readonly IMessagePropertyShape<TMessage>?[] _properties;

    public MessageShapeBuilder(int propertiesCount, MessageTypeShapeCache cache)
    {
        if (propertiesCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(propertiesCount));
        }

        _cache = cache;

        _properties = propertiesCount == 0 ? Array.Empty<IMessagePropertyShape<TMessage>>() : new IMessagePropertyShape<TMessage>?[propertiesCount];
    }

    public IMessageShapeBuilder<TMessage> AddProperty<TProperty>(Getter<TMessage, TProperty> getter, Setter<TMessage, TProperty> setter)
    {
        var index = -1;
        for (var i = 0; i < _properties.Length; i++)
        {
            if (_properties[i] == null)
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            throw new InvalidOperationException();
        }

        _properties[index] = new MessagePropertyShape<TMessage, TProperty>(index + 1, getter, setter);
        return this;
    }

    public void Register()
    {
        for (var i = 0; i < _properties.Length; i++)
        {
            if (_properties[i] == null)
            {
                throw new InvalidOperationException();
            }
        }

        var properties = (IMessagePropertyShape<TMessage>[])_properties.Clone();
        _cache.Register(properties);
    }
}