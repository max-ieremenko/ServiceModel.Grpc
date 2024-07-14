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
using System.Reflection;
using System.Reflection.Emit;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

internal static class EmitMessageAccessorBuilder
{
    public static Type GetMessageAccessorGenericType(int propertiesCount)
    {
        if (propertiesCount <= 3)
        {
            throw new ArgumentOutOfRangeException(nameof(propertiesCount));
        }

        var messageGenericType = MessageBuilder.GetMessageGenericType(propertiesCount);
        return GetMessageAccessorGenericType(messageGenericType);
    }

    internal static Type GetMessageAccessorGenericType(Type messageGenericType)
    {
        var typeName = "Accessor" + messageGenericType.FullName;

        Type? result;
        lock (ProxyAssembly.SyncRoot)
        {
            result = ProxyAssembly.DefaultModule.GetType(typeName, false, false);
            if (result == null)
            {
                result = Build(ProxyAssembly.DefaultModule, messageGenericType, typeName);
            }
        }

        return result;
    }

    private static Type Build(ModuleBuilder moduleBuilder, Type messageGenericType, string typeName)
    {
        var typeBuilder = moduleBuilder
            .DefineType(
                typeName,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);
        typeBuilder.AddInterfaceImplementation(typeof(IMessageAccessor));

        var genericArgs = typeBuilder
            .DefineGenericParameters(messageGenericType.GetTypeInfo().GenericTypeParameters.Select(i => i.Name).ToArray());

        var messageType = messageGenericType.MakeGenericType(genericArgs);

        // private string[] _names;
        var fieldNames = typeBuilder.DefineField("_names", typeof(string[]), FieldAttributes.Private | FieldAttributes.InitOnly);

        BuildCtor(typeBuilder, fieldNames, genericArgs.Length);
        BuildNames(typeBuilder, fieldNames);
        BuildCreateNew(typeBuilder, messageGenericType, messageType);
        BuildGetInstanceType(typeBuilder, messageType);
        BuildGetValue(typeBuilder, messageGenericType, messageType);
        BuildGetValueType(typeBuilder, messageType);
        BuildSetValue(typeBuilder, messageGenericType, messageType);

        return typeBuilder.CreateTypeInfo()!;
    }

    private static void BuildCtor(TypeBuilder typeBuilder, FieldBuilder fieldNames, int propertiesCount)
    {
        // public MessageAccessor(string[] names)
        var body = typeBuilder
            .DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, [fieldNames.FieldType])
            .GetILGenerator();

        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Call, typeof(object).Constructor());

        var bodyLabel = body.DefineLabel();
        var testLengthLabel = body.DefineLabel();

        // if (names == null) throw new ArgumentNullException("names");
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Ldnull);
        body.Emit(OpCodes.Ceq);
        body.Emit(OpCodes.Brfalse_S, testLengthLabel);
        body.Emit(OpCodes.Ldstr, "names");
        body.Emit(OpCodes.Newobj, typeof(ArgumentNullException).Constructor(typeof(string)));
        body.Emit(OpCodes.Throw);

        // if (names.Length != 5) throw new ArgumentOutOfRangeException("names");
        body.MarkLabel(testLengthLabel);
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Ldlen);
        body.Emit(OpCodes.Conv_I4);
        body.Emit(OpCodes.Ldc_I4, propertiesCount);
        body.Emit(OpCodes.Ceq);
        body.Emit(OpCodes.Brtrue_S, bodyLabel);
        body.Emit(OpCodes.Ldstr, "names");
        body.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).Constructor(typeof(string)));
        body.Emit(OpCodes.Throw);

        // _names = names
        body.MarkLabel(bodyLabel);
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Stfld, fieldNames);

        body.Emit(OpCodes.Ret);
    }

    private static void BuildCreateNew(TypeBuilder typeBuilder, Type messageGenericType, Type messageType)
    {
        var method = typeBuilder
            .DefineMethod(
                nameof(IMessageAccessor.CreateNew),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                typeof(object),
                []);

        typeBuilder.DefineMethodOverride(method, typeof(IMessageAccessor).InstanceMethod(method.Name));

        var body = method.GetILGenerator();
        body.Emit(OpCodes.Newobj, TypeBuilder.GetConstructor(messageType, messageGenericType.Constructor(0)));
        body.Emit(OpCodes.Ret);
    }

    private static void BuildGetInstanceType(TypeBuilder typeBuilder, Type messageType)
    {
        var method = typeBuilder
            .DefineMethod(
                nameof(IMessageAccessor.GetInstanceType),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                typeof(Type),
                []);

        typeBuilder.DefineMethodOverride(method, typeof(IMessageAccessor).InstanceMethod(method.Name));

        var body = method.GetILGenerator();
        body.Emit(OpCodes.Ldtoken, messageType);
        body.Emit(OpCodes.Call, typeof(Type).StaticMethod(nameof(Type.GetTypeFromHandle)));
        body.Emit(OpCodes.Ret);
    }

    private static void BuildGetValue(TypeBuilder typeBuilder, Type messageGenericType, Type messageType)
    {
        var method = typeBuilder
            .DefineMethod(
                nameof(IMessageAccessor.GetValue),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                typeof(object),
                [typeof(object), typeof(int)]);

        typeBuilder.DefineMethodOverride(method, typeof(IMessageAccessor).InstanceMethod(method.Name));

        var body = method.GetILGenerator();
        var bodyLabel = body.DefineLabel();

        // if (message == null) throw new ArgumentNullException("message");
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Ldnull);
        body.Emit(OpCodes.Ceq);
        body.Emit(OpCodes.Brfalse_S, bodyLabel);
        body.Emit(OpCodes.Ldstr, "message");
        body.Emit(OpCodes.Newobj, typeof(ArgumentNullException).Constructor(typeof(string)));
        body.Emit(OpCodes.Throw);

        // (Message<T1, T2, T3, T4, T5>)message
        body.MarkLabel(bodyLabel);
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Castclass, messageType);

        var args = messageType.GenericTypeArguments;
        var switchLabels = new Label[args.Length];
        var defaultLabel = body.DefineLabel();
        for (var i = 0; i < args.Length; i++)
        {
            switchLabels[i] = body.DefineLabel();
        }

        body.Emit(OpCodes.Ldarg_2);
        body.Emit(OpCodes.Switch, switchLabels);
        body.Emit(OpCodes.Br, defaultLabel);

        for (var i = 0; i < args.Length; i++)
        {
            body.MarkLabel(switchLabels[i]);
            var property = messageGenericType.InstanceProperty("Value" + (i + 1).ToString(CultureInfo.InvariantCulture));
            body.Emit(OpCodes.Callvirt, TypeBuilder.GetMethod(messageType, property.GetMethod));
            body.Emit(OpCodes.Box, args[i]);
            body.Emit(OpCodes.Ret);
        }

        body.MarkLabel(defaultLabel);
        body.Emit(OpCodes.Ldstr, "property");
        body.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).Constructor(typeof(string)));
        body.Emit(OpCodes.Throw);
    }

    private static void BuildGetValueType(TypeBuilder typeBuilder, Type messageType)
    {
        var method = typeBuilder
            .DefineMethod(
                nameof(IMessageAccessor.GetValueType),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                typeof(Type),
                [typeof(int)]);

        typeBuilder.DefineMethodOverride(method, typeof(IMessageAccessor).InstanceMethod(method.Name));

        var body = method.GetILGenerator();

        var args = messageType.GenericTypeArguments;
        var switchLabels = new Label[args.Length];
        var defaultLabel = body.DefineLabel();
        for (var i = 0; i < args.Length; i++)
        {
            switchLabels[i] = body.DefineLabel();
        }

        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Switch, switchLabels);
        body.Emit(OpCodes.Br, defaultLabel);

        for (var i = 0; i < args.Length; i++)
        {
            body.MarkLabel(switchLabels[i]);
            body.Emit(OpCodes.Ldtoken, args[i]);
            body.Emit(OpCodes.Call, typeof(Type).StaticMethod(nameof(Type.GetTypeFromHandle)));
            body.Emit(OpCodes.Ret);
        }

        body.MarkLabel(defaultLabel);
        body.Emit(OpCodes.Ldstr, "property");
        body.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).Constructor(typeof(string)));
        body.Emit(OpCodes.Throw);
    }

    private static void BuildSetValue(TypeBuilder typeBuilder, Type messageGenericType, Type messageType)
    {
        var method = typeBuilder
            .DefineMethod(
                nameof(IMessageAccessor.SetValue),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                typeof(void),
                [typeof(object), typeof(int), typeof(object)]);

        typeBuilder.DefineMethodOverride(method, typeof(IMessageAccessor).InstanceMethod(method.Name));

        var body = method.GetILGenerator();
        var bodyLabel = body.DefineLabel();

        // if (message == null) throw new ArgumentNullException("message");
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Ldnull);
        body.Emit(OpCodes.Ceq);
        body.Emit(OpCodes.Brfalse_S, bodyLabel);
        body.Emit(OpCodes.Ldstr, "message");
        body.Emit(OpCodes.Newobj, typeof(ArgumentNullException).Constructor(typeof(string)));
        body.Emit(OpCodes.Throw);

        // (Message<T1, T2, T3, T4, T5>)message
        body.MarkLabel(bodyLabel);
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Castclass, messageType);

        var args = messageType.GenericTypeArguments;
        var switchLabels = new Label[args.Length];
        var defaultLabel = body.DefineLabel();
        for (var i = 0; i < args.Length; i++)
        {
            switchLabels[i] = body.DefineLabel();
        }

        body.Emit(OpCodes.Ldarg_2);
        body.Emit(OpCodes.Switch, switchLabels);
        body.Emit(OpCodes.Br, defaultLabel);

        for (var i = 0; i < args.Length; i++)
        {
            body.MarkLabel(switchLabels[i]);

            // (T3)value
            body.Emit(OpCodes.Ldarg_3);
            body.Emit(OpCodes.Unbox_Any, args[i]);

            var property = messageGenericType.InstanceProperty("Value" + (i + 1).ToString(CultureInfo.InvariantCulture));
            body.Emit(OpCodes.Callvirt, TypeBuilder.GetMethod(messageType, property.SetMethod));
            body.Emit(OpCodes.Ret);
        }

        body.MarkLabel(defaultLabel);
        body.Emit(OpCodes.Ldstr, "property");
        body.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).Constructor(typeof(string)));
        body.Emit(OpCodes.Throw);
    }

    private static void BuildNames(TypeBuilder typeBuilder, FieldBuilder fieldNames)
    {
        // public string[] Names { get; }
        var property = typeBuilder.DefineProperty(nameof(IMessageAccessor.Names), PropertyAttributes.None, fieldNames.FieldType, Type.EmptyTypes);

        // string[] get_Names
        var getter = typeBuilder.DefineMethod(
            "get_" + property.Name,
            MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual,
            fieldNames.FieldType,
            Type.EmptyTypes);

        typeBuilder.DefineMethodOverride(getter, typeof(IMessageAccessor).InstanceProperty(property.Name).GetMethod);

        property.SetGetMethod(getter);

        var body = getter.GetILGenerator();
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldfld, fieldNames);
        body.Emit(OpCodes.Ret);
    }
}