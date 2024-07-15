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
/// The default implementation of <see cref="IServerFaultDetailSerializer"/>.
/// </summary>
public class DefaultServerFaultDetailSerializer : IServerFaultDetailSerializer
{
    /// <summary>
    /// The default implementation. <code>detailType.AssemblyQualifiedName()</code>.
    /// </summary>
    /// <param name="detailType">A type of value, provided by <see cref="ServerFaultDetail"/>.<see cref="ServerFaultDetail.Detail"/>.</param>
    /// <returns>A string representation of <paramref name="detailType"/>.</returns>
    public virtual string SerializeDetailType(Type detailType) => SerializeType(detailType);

    /// <summary>
    /// The default implementation. <code>marshallerFactory.Marshaller[detail.GetType()].Serialize(detail)</code>.
    /// </summary>
    /// <param name="marshallerFactory">The current <see cref="IMarshallerFactory"/>.</param>
    /// <param name="detail">A value, provided by <see cref="ServerFaultDetail"/>.<see cref="ServerFaultDetail.Detail"/>.</param>
    /// <returns>A byte array representation of <paramref name="detail"/>.</returns>
    public virtual byte[] SerializeDetail(IMarshallerFactory marshallerFactory, object detail) => Serialize(marshallerFactory, detail);

    internal static string SerializeType(Type detailType) => detailType.GetShortAssemblyQualifiedName();

    internal static byte[] Serialize(IMarshallerFactory marshallerFactory, object detail) => MarshallerExtensions.SerializeObject(marshallerFactory, detail);
}