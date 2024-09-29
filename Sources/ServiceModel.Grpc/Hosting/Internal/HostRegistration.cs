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

using System.Runtime.CompilerServices;
using ServiceModel.Grpc.Emit;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Hosting.Internal;

internal static class HostRegistration
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void BindWithEmit<TService>(IServiceMethodBinder<TService> methodBinder, Type? serviceInstanceType, ILogger? logger)
    {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            throw new PlatformNotSupportedException("Dynamic code generation is not supported on this platform.");
        }
#endif
        if (Features.IsReflectionEmitDisabled)
        {
            throw new NotSupportedException("ServiceModel.Grpc.Emit is disabled within this application.");
        }

        var endpointBinder = EmitGenerator.GenerateServiceEndpointBinder<TService>(serviceInstanceType, logger);
        endpointBinder.Bind(methodBinder);
    }
}