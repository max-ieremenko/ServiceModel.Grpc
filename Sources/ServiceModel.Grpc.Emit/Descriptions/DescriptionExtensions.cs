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
using System.Reflection;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Descriptions.Reflection;
using ServiceModel.Grpc.Emit.CodeGenerators;
using ServiceModel.Grpc.Emit.Descriptions.Reflection;

namespace ServiceModel.Grpc.Emit.Descriptions;

public static class DescriptionExtensions
{
    public static MethodInfo GetSource(this OperationDescription<Type> operation) => ((ReflectionMethodInfo)operation.Method).Source;

    public static MethodInfo GetSource(this NotSupportedMethodDescription<Type> method) => ((ReflectionMethodInfo)method.Method).Source;

    public static ParameterInfo GetSource(this IParameterInfo<Type> parameter) => ((ReflectionParameterInfo)parameter).Source;

    public static bool IsGenericType(this MessageDescription<Type> message) => message.Properties.Length > 0;

    public static Type GetClrType(this MessageDescription<Type>? message) =>
        message == null ? typeof(Message) : MessageBuilder.GetMessageType(message.Properties);
}