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

using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Interceptors;

/// <summary>
/// The default implementation of <see cref="IClientFaultDetailDeserializer"/>.
/// </summary>
public class DefaultClientFaultDetailDeserializer : IClientFaultDetailDeserializer
{
    /// <summary>
    /// The default implementation. <code>Type.GetType(typePayload, true, false)</code>
    /// </summary>
    /// <param name="typePayload">A value provided by <see cref="IServerFaultDetailSerializer"/>.<see cref="IServerFaultDetailSerializer.SerializeDetailType"/>.</param>
    /// <returns>A <see cref="Type"/> of <see cref="ClientFaultDetail.Detail"/>.</returns>
    public virtual Type DeserializeDetailType(string typePayload) => DeserializeType(typePayload);

    /// <summary>
    /// The default implementation. <code>marshallerFactory.Marshaller[detailType].Deserialize(detailPayload)</code>
    /// </summary>
    /// <param name="marshallerFactory">The current <see cref="IMarshallerFactory"/>.</param>
    /// <param name="detailType"><see cref="Type"/> of instance, provided by <see cref="DeserializeDetailType"/>.</param>
    /// <param name="detailPayload">A value provided by <see cref="IServerFaultDetailSerializer"/>.<see cref="IServerFaultDetailSerializer.SerializeDetail"/>.</param>
    /// <returns>An instance of details.</returns>
    public virtual object DeserializeDetail(IMarshallerFactory marshallerFactory, Type detailType, byte[] detailPayload) =>
        Deserialize(marshallerFactory, detailType, detailPayload);

    internal static Type DeserializeType(string typePayload) => Type.GetType(typePayload, true, false)!;

    internal static object Deserialize(IMarshallerFactory marshallerFactory, Type detailType, byte[] detailPayload) =>
        MarshallerExtensions.DeserializeObject(marshallerFactory, detailType, detailPayload);
}