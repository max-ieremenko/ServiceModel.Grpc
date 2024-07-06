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

namespace ServiceModel.Grpc.Emit.CodeGenerators;

internal sealed class ReflectStreamAccessor : IStreamAccessor
{
    private readonly Type _itemType;
    private readonly Func<object, object> _cast;
    private readonly Func<object> _factory;

    public ReflectStreamAccessor(Type itemType)
    {
        _itemType = itemType;
        (_factory, _cast) = ReflectOperationDescriptorCompiler.GetStreamAccessors(itemType);
    }

    public void Validate(object stream) => _cast(stream);

    public object CreateEmpty() => _factory();

    public Type GetInstanceType() => typeof(IAsyncEnumerable<>).MakeGenericType(_itemType);
}