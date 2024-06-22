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

using System.Collections.Generic;
using Moq;
using ServiceModel.Grpc.Filters.Internal;

namespace ServiceModel.Grpc.Filters.Api;

internal sealed partial class MessageAccessorMock
{
    private readonly Mock<IMessageAccessor> _messageAccessor;
    private readonly List<string> _names;
    private readonly Dictionary<int, IProperty> _valueByIndex;

    public MessageAccessorMock()
    {
        Message = new object();
        _names = new List<string>();
        _valueByIndex = new();

        _messageAccessor = new(MockBehavior.Strict);
        _messageAccessor
            .SetupGet(a => a.Names)
            .Returns(_names.ToArray);
        _messageAccessor
            .Setup(a => a.GetValue(Message, It.IsAny<int>()))
            .Returns<object, int>((_, property) => _valueByIndex[property].GetValue());
        _messageAccessor
            .Setup(a => a.SetValue(Message, It.IsAny<int>(), It.IsAny<object?>()))
            .Callback<object, int, object?>((_, property, value) => _valueByIndex[property].SetValue(value));
    }

    public IMessageAccessor Accessor => _messageAccessor.Object;

    public object Message { get; }

    public void AddProperty<TValue>(string name, TValue value)
    {
        _names.Add(name);
        _valueByIndex.Add(_valueByIndex.Count, new Property<TValue>(value));
    }

    public void SetupCreateNew()
    {
        _messageAccessor
            .Setup(a => a.CreateNew())
            .Returns(Message);
    }
}