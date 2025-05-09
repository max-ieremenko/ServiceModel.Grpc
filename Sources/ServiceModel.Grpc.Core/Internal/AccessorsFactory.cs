﻿// <copyright>
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
public static class AccessorsFactory
{
    public static IStreamAccessor CreateStreamAccessor<TItem>() => StreamAccessor<TItem>.Instance;

    public static IMessageAccessor CreateMessageAccessor() => new MessageAccessor([]);

    public static IMessageAccessor CreateMessageAccessor<T1>(string[] names) => new MessageAccessor<T1>(names);

    public static IMessageAccessor CreateMessageAccessor<T1, T2>(string[] names) => new MessageAccessor<T1, T2>(names);

    public static IMessageAccessor CreateMessageAccessor<T1, T2, T3>(string[] names) => new MessageAccessor<T1, T2, T3>(names);
}