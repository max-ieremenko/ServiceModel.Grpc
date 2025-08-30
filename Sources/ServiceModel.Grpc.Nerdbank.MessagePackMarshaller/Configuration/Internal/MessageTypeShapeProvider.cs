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

using System.Collections.Concurrent;
using PolyType;
using PolyType.Abstractions;

namespace ServiceModel.Grpc.Configuration.Internal;

internal sealed class MessageTypeShapeProvider
{
    private readonly ConcurrentDictionary<Type, ITypeShape> _shapeByType;
    private readonly MessageTypeShapeCache _cache;

    public MessageTypeShapeProvider(MessageTypeShapeCache cache, ITypeShapeProvider userProvider)
    {
        _shapeByType = new();
        _cache = cache;
        UserProvider = userProvider;
    }

    public ITypeShapeProvider UserProvider { get; }

    public ITypeShape<T> GetShape<T>()
    {
        var result = GetShape(typeof(T));
        if (result == null)
        {
            throw new NotSupportedException($"This provider '{UserProvider.GetType()}' had no type shape for '{typeof(T)}'.");
        }

        return (ITypeShape<T>)result;
    }

    private ITypeShape? GetShape(Type type)
    {
        if (_shapeByType.TryGetValue(type, out var result))
        {
            return result;
        }

        if (!_cache.TryGetFactory(type, out var factory))
        {
            return UserProvider.GetShape(type);
        }

        return _shapeByType.GetOrAdd(type, factory.CreateShape(UserProvider));
    }
}