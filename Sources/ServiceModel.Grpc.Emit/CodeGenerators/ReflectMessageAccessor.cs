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

internal sealed class ReflectMessageAccessor : IMessageAccessor
{
    private readonly Type _messageType;
    private readonly Action<object, int, object?> _setter;
    private readonly Func<object, int, object?> _getter;
    private readonly Func<int, Type> _getterType;

    public ReflectMessageAccessor(Type messageType, string[] names)
    {
        _messageType = messageType;
        Names = names;
        (_getter, _setter, _getterType) = ReflectOperationDescriptorCompiler.GetMessageAccessors(messageType);
    }

    public string[] Names { get; }

    public object CreateNew() => Activator.CreateInstance(_messageType);

    public Type GetValueType(int property) => _getterType(property);

    public Type GetInstanceType() => _messageType;

    public object? GetValue(object message, int property) => _getter(message, property);

    public void SetValue(object message, int property, object? value) => _setter(message, property, value);
}