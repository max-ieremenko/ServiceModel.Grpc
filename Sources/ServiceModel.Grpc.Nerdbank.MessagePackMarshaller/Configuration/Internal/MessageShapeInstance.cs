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

using PolyType;
using PolyType.Abstractions;
using PolyType.SourceGenModel;

namespace ServiceModel.Grpc.Configuration.Internal;

internal sealed class MessageShapeInstance<TMessage>
    where TMessage : new()
{
    private readonly ITypeShapeProvider _provider;
    private readonly IMessagePropertyShape<TMessage>[] _properties;
    private SourceGenObjectTypeShape<TMessage>? _value;

    public MessageShapeInstance(ITypeShapeProvider provider, IMessagePropertyShape<TMessage>[] properties)
    {
        _provider = provider;
        _properties = properties;
    }

    public ITypeShape ToShape()
    {
        _value = new SourceGenObjectTypeShape<TMessage>
        {
            Provider = _provider,
            IsRecordType = false,
            IsTupleType = false,
            CreatePropertiesFunc = CreateProperties,
            CreateConstructorFunc = CreateConstructor
        };

        return _value;
    }

    private static object? GetReserved(ref TMessage message) => null;

    private static void SetReserved(ref TMessage message, object? value)
    {
    }

    private IConstructorShape CreateConstructor() => new SourceGenConstructorShape<TMessage, EmptyArgumentState>
    {
        DeclaringType = _value!,
        DefaultConstructorFunc = static () => new TMessage(),
        IsPublic = true
    };

    private IPropertyShape[] CreateProperties()
    {
        if (_properties.Length == 0)
        {
            return Array.Empty<IPropertyShape>();
        }

        var result = new IPropertyShape[_properties.Length + 1];
        result[0] = new SourceGenPropertyShape<TMessage, object?>
        {
            Position = 0,
            Name = "Reserved",
            DeclaringType = _value!,
            PropertyType = new SourceGenObjectTypeShape<object?>
            {
                IsRecordType = false,
                IsTupleType = false,
                Provider = _provider
            },
            Getter = GetReserved,
            Setter = SetReserved,
            IsField = false,
            IsGetterPublic = true,
            IsSetterPublic = true,
            IsGetterNonNullable = false,
            IsSetterNonNullable = false,
        };

        for (var i = 0; i < _properties.Length; i++)
        {
            result[i + 1] = _properties[i].ToShape(_provider, _value!);
        }

        return result;
    }
}