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

using System.Globalization;
using PolyType;
using PolyType.Abstractions;
using PolyType.SourceGenModel;

namespace ServiceModel.Grpc.Configuration.Internal;

internal sealed class MessagePropertyShape<TMessage, TProperty> : IMessagePropertyShape<TMessage>
{
    private readonly int _index;
    private readonly Getter<TMessage, TProperty> _getter;
    private readonly Setter<TMessage, TProperty> _setter;

    public MessagePropertyShape(int index, Getter<TMessage, TProperty> getter, Setter<TMessage, TProperty> setter)
    {
        _index = index;
        _getter = getter;
        _setter = setter;
    }

    public IPropertyShape ToShape(ITypeShapeProvider provider, IObjectTypeShape<TMessage> declaringType)
    {
        var propertyType = provider.GetTypeShape<TProperty>();
        if (propertyType == null)
        {
            throw new NotSupportedException($"This provider '{provider.GetType()}' had no type shape for '{typeof(TProperty)}'.");
        }

        return new SourceGenPropertyShape<TMessage, TProperty>
        {
            Position = _index,
            Name = "Value" + _index.ToString(CultureInfo.InvariantCulture),
            DeclaringType = declaringType,
            PropertyType = propertyType,
            Getter = _getter,
            Setter = _setter,
            IsField = false,
            IsGetterPublic = true,
            IsSetterPublic = true,
            IsGetterNonNullable = false,
            IsSetterNonNullable = false,
        };
    }
}