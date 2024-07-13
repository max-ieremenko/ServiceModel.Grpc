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

using System.Reflection;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

internal sealed class ReflectDescriptor
{
    private readonly Dictionary<int, object> _createMessageAccessor;

    public ReflectDescriptor()
    {
        FuncMethodInfoCtor = typeof(Func<MethodInfo>).Constructor(2);

        BuilderCtor = typeof(OperationDescriptorBuilder).Constructor(2);
        BuilderBuild = typeof(OperationDescriptorBuilder).InstanceMethod(nameof(OperationDescriptorBuilder.Build));
        BuilderWithRequest = typeof(OperationDescriptorBuilder).InstanceMethod(nameof(OperationDescriptorBuilder.WithRequest));
        BuilderWithResponse = typeof(OperationDescriptorBuilder).InstanceMethod(nameof(OperationDescriptorBuilder.WithResponse));
        BuilderWithRequestHeaderParameters = typeof(OperationDescriptorBuilder).InstanceMethod(nameof(OperationDescriptorBuilder.WithRequestHeaderParameters));
        BuilderWithRequestParameters = typeof(OperationDescriptorBuilder).InstanceMethod(nameof(OperationDescriptorBuilder.WithRequestParameters));
        BuilderWithRequestStream = typeof(OperationDescriptorBuilder).InstanceMethod(nameof(OperationDescriptorBuilder.WithRequestStream));
        BuilderWithResponseStream = typeof(OperationDescriptorBuilder).InstanceMethod(nameof(OperationDescriptorBuilder.WithResponseStream));

        _createMessageAccessor = new Dictionary<int, object>();
        CreateStreamAccessor = null!;
        foreach (var method in typeof(AccessorsFactory).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (method.Name == nameof(AccessorsFactory.CreateMessageAccessor))
            {
                var args = method.GetGenericArguments();
                _createMessageAccessor.Add(args.Length, method);
            }
            else if (method.Name == nameof(AccessorsFactory.CreateStreamAccessor))
            {
                CreateStreamAccessor = method;
            }
        }
    }

    public ConstructorInfo FuncMethodInfoCtor { get; }

    public ConstructorInfo BuilderCtor { get; }

    public MethodInfo BuilderBuild { get; }

    public MethodInfo BuilderWithRequest { get; }

    public MethodInfo BuilderWithResponse { get; }

    public MethodInfo BuilderWithRequestHeaderParameters { get; }

    public MethodInfo BuilderWithRequestParameters { get; }

    public MethodInfo BuilderWithRequestStream { get; }

    public MethodInfo BuilderWithResponseStream { get; }

    public MethodInfo CreateStreamAccessor { get; }

    public object CreateMessageAccessor(int length)
    {
        if (_createMessageAccessor.TryGetValue(length, out var result))
        {
            return result;
        }

        result = EmitMessageAccessorBuilder.GetMessageAccessorGenericType(length);
        _createMessageAccessor.Add(length, result);
        return result;
    }
}