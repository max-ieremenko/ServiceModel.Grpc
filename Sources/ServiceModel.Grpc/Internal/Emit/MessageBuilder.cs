// <copyright>
// Copyright 2020 Max Ieremenko
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Internal.Emit;

internal static class MessageBuilder
{
    public static Type GetMessageType(params Type[] typeArguments)
    {
        if (typeArguments.Length == 0)
        {
            return typeof(Message);
        }

        var messageTypeName = typeof(Message).FullName + "`" + typeArguments.Length.ToString(CultureInfo.InvariantCulture);

        Type messageType;
        if (typeArguments.Length <= 3)
        {
            messageType = typeof(Message).Assembly.GetType(messageTypeName, true, false);
        }
        else
        {
            messageType = ResolveMessageType(typeArguments.Length, messageTypeName);
        }

        return messageType.MakeGenericType(typeArguments);
    }

    private static Type ResolveMessageType(int propertiesCount, string typeName)
    {
        Type? result;
        lock (ProxyAssembly.SyncRoot)
        {
            result = ProxyAssembly.DefaultModule.GetType(typeName, false, false);
            if (result == null)
            {
                result = BuildNewMessageType(propertiesCount, typeName);
            }
        }

        return result;
    }

    private static Type BuildNewMessageType(int propertiesCount, string typeName)
    {
        // internal sealed class Message<T1, T2, T3>
        var typeBuilder = ProxyAssembly
            .DefaultModule
            .DefineType(
                typeName,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

        var genericArgs = typeBuilder.DefineGenericParameters(Enumerable.Range(1, propertiesCount).Select(i => "T" + i.ToString(CultureInfo.InvariantCulture)).ToArray());
        typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(SerializableAttribute).Constructor(), Array.Empty<object>()));
        typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(
            typeof(DataContractAttribute).Constructor(),
            Array.Empty<object>(),
            new[]
            {
                typeof(DataContractAttribute).InstanceProperty(nameof(DataContractAttribute.Name)),
                typeof(DataContractAttribute).InstanceProperty(nameof(DataContractAttribute.Namespace))
            },
            new object[]
            {
                "m",
                "s"
            }));

        // new ()
        typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

        var ctorBody = typeBuilder
            .DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                typeBuilder.GetGenericArguments())
            .GetILGenerator();

        ctorBody.Emit(OpCodes.Ldarg_0);
        ctorBody.Emit(OpCodes.Call, typeof(object).Constructor());

        for (var i = 0; i < propertiesCount; i++)
        {
            // private T1 _value1;
            var field = typeBuilder.DefineField(
                "_value" + (i + 1).ToString(CultureInfo.InvariantCulture),
                genericArgs[i],
                FieldAttributes.Private);

            // public T1 Value1 { get; set; }
            var property = typeBuilder.DefineProperty(
                "Value" + (i + 1).ToString(CultureInfo.InvariantCulture),
                PropertyAttributes.HasDefault,
                genericArgs[i],
                null);

            property.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(DataMemberAttribute).Constructor(),
                Array.Empty<object>(),
                new[]
                {
                    typeof(DataMemberAttribute).InstanceProperty(nameof(DataMemberAttribute.Name)),
                    typeof(DataMemberAttribute).InstanceProperty(nameof(DataMemberAttribute.Order))
                },
                new object[]
                {
                    "v" + (i + 1).ToString(CultureInfo.InvariantCulture),
                    i + 1
                }));

            // T1 get_Value1
            var getter = typeBuilder.DefineMethod(
                "get_" + property.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                genericArgs[i],
                Type.EmptyTypes);

            var getterBody = getter.GetILGenerator();
            getterBody.Emit(OpCodes.Ldarg_0);
            getterBody.Emit(OpCodes.Ldfld, field);
            getterBody.Emit(OpCodes.Ret);

            // set_Value1
            var setter = typeBuilder.DefineMethod(
                "set_" + property.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new Type[] { genericArgs[i] });

            var setterBody = setter.GetILGenerator();
            setterBody.Emit(OpCodes.Ldarg_0);
            setterBody.Emit(OpCodes.Ldarg_1);
            setterBody.Emit(OpCodes.Stfld, field);
            setterBody.Emit(OpCodes.Ret);

            property.SetGetMethod(getter);
            property.SetSetMethod(setter);

            // _valueI = pi
            ctorBody.Emit(OpCodes.Ldarg_0);
            ctorBody.EmitLdarg(i + 1);
            ctorBody.Emit(OpCodes.Stfld, field);
        }

        ctorBody.Emit(OpCodes.Ret);
        return typeBuilder.CreateTypeInfo()!;
    }
}