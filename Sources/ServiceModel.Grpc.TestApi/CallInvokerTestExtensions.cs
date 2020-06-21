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
using Grpc.Core;
using Grpc.Core.Interceptors;
using Shouldly;

namespace ServiceModel.Grpc.TestApi
{
    public static class CallInvokerTestExtensions
    {
        public static (Interceptor Interceptor, CallInvoker CallInvoker) ShouldBeIntercepted(this CallInvoker callInvoker)
        {
            callInvoker.ShouldNotBeNull();

            Type interceptingCallInvokerType = Type.GetType("Grpc.Core.Interceptors.InterceptingCallInvoker, Grpc.Core.Api", true, false)!;
            callInvoker.ShouldBeOfType(interceptingCallInvokerType);

            var interceptorField = interceptingCallInvokerType
                .GetField("interceptor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            interceptorField.ShouldNotBeNull();

            var invokerField = interceptingCallInvokerType
                .GetField("invoker", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            invokerField.ShouldNotBeNull();

            var interceptor = (Interceptor)interceptorField!.GetValue(callInvoker)!;
            var invoker = (CallInvoker)invokerField!.GetValue(callInvoker)!;

            return (interceptor, invoker);
        }
    }
}
