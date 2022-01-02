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
using System.Reflection;
using System.Reflection.Emit;
using Grpc.Core;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class EmitClientBuilderBuilder
    {
        private readonly ContractDescription _description;
        private readonly Type _contractType;
        private readonly Type _clientType;
        private readonly Type _clientBuilderType;

        private FieldBuilder _contractField = null!;
        private FieldBuilder _callOptionsFactoryField = null!;

        public EmitClientBuilderBuilder(ContractDescription description, Type contractType, Type clientType)
        {
            _description = description;
            _contractType = contractType;
            _clientType = clientType;
            _clientBuilderType = typeof(IClientBuilder<>).MakeGenericType(_description.ServiceType);
        }

        public TypeInfo Build(ModuleBuilder moduleBuilder)
        {
            var typeBuilder = moduleBuilder.DefineType(
                _description.ClientBuilderClassName,
                TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            typeBuilder.AddInterfaceImplementation(_clientBuilderType);

            BuildFields(typeBuilder);
            BuildInitializeMethod(typeBuilder);
            BuildBuildMethod(typeBuilder);

            return typeBuilder.CreateTypeInfo()!;
        }

        private void BuildFields(TypeBuilder typeBuilder)
        {
            _contractField = typeBuilder.DefineField("_contract", _contractType, FieldAttributes.Private);
            _callOptionsFactoryField = typeBuilder.DefineField("_defaultCallOptionsFactory", typeof(Func<CallOptions>), FieldAttributes.Private);
        }

        private void BuildInitializeMethod(TypeBuilder typeBuilder)
        {
            var method = typeBuilder
                .DefineMethod(
                    "Initialize",
                    MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                    typeof(void),
                    new[] { typeof(IMarshallerFactory), typeof(Func<CallOptions>) });

            typeBuilder.DefineMethodOverride(
                method,
                _clientBuilderType.InstanceMethod("Initialize"));

            var body = method.GetILGenerator();

            // _contract = new (marshallerFactory);
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Newobj, _contractType.Constructor(typeof(IMarshallerFactory)));
            body.Emit(OpCodes.Stfld, _contractField);

            // _defaultCallOptionsFactory = defaultCallOptionsFactory
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldarg_2);
            body.Emit(OpCodes.Stfld, _callOptionsFactoryField);

            body.Emit(OpCodes.Ret);
        }

        private void BuildBuildMethod(TypeBuilder typeBuilder)
        {
            var method = typeBuilder
                .DefineMethod(
                    "Build",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                    _description.ServiceType,
                    new[] { typeof(CallInvoker) });

            typeBuilder.DefineMethodOverride(
                method,
                _clientBuilderType.InstanceMethod("Build"));

            var body = method.GetILGenerator();

            // new (callInvoker, _contract, _defaultCallOptionsFactory)
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, _contractField);
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, _callOptionsFactoryField);
            body.Emit(OpCodes.Newobj, _clientType.Constructor(typeof(CallInvoker), _contractType, typeof(Func<CallOptions>)));
            body.Emit(OpCodes.Ret);
        }
    }
}
