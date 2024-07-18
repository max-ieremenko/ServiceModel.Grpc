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

using System.ComponentModel;
using System.Reflection;
using Grpc.Core.Utils;
using ServiceModel.Grpc.Internal.Descriptors;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace ServiceModel.Grpc.Internal;

/// <summary>
/// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
[Experimental("ServiceModelGrpcInternalAPI")]
public readonly ref struct OperationDescriptorBuilder
{
    private readonly bool _isAsync;
    private readonly object?[] _args;

    public OperationDescriptorBuilder(Func<MethodInfo> getContractMethod, bool isAsync)
    {
        GrpcPreconditions.CheckNotNull(getContractMethod, nameof(getContractMethod));

        _isAsync = isAsync;
        _args = [getContractMethod, null, null, null, null, null, null];
    }

    public OperationDescriptorBuilder WithRequestHeaderParameters(int[] parameters)
    {
        _args[1] = GrpcPreconditions.CheckNotNull(parameters, nameof(parameters));
        return this;
    }

    public OperationDescriptorBuilder WithRequestParameters(int[] parameters)
    {
        _args[2] = GrpcPreconditions.CheckNotNull(parameters, nameof(parameters));
        return this;
    }

    public OperationDescriptorBuilder WithRequest(IMessageAccessor accessor)
    {
        _args[3] = GrpcPreconditions.CheckNotNull(accessor, nameof(accessor));
        return this;
    }

    public OperationDescriptorBuilder WithRequestStream(IStreamAccessor accessor)
    {
        _args[4] = GrpcPreconditions.CheckNotNull(accessor, nameof(accessor));
        return this;
    }

    public OperationDescriptorBuilder WithResponse(IMessageAccessor accessor)
    {
        _args[5] = GrpcPreconditions.CheckNotNull(accessor, nameof(accessor));
        return this;
    }

    public OperationDescriptorBuilder WithResponseStream(IStreamAccessor accessor)
    {
        _args[6] = GrpcPreconditions.CheckNotNull(accessor, nameof(accessor));
        return this;
    }

    public IOperationDescriptor Build() => new OperationDescriptor(
        (Func<MethodInfo>)_args[0]!,
        _isAsync,
        (int[]?)_args[1] ?? [],
        (int[]?)_args[2] ?? [],
        (IMessageAccessor?)_args[3] ?? throw new InvalidOperationException(),
        (IStreamAccessor?)_args[4],
        (IMessageAccessor?)_args[5] ?? throw new InvalidOperationException(),
        (IStreamAccessor?)_args[6]);
}