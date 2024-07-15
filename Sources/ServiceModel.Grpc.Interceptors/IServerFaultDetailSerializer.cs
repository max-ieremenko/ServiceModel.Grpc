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
/// Allows an implementer to perform custom error details <see cref="ServerFaultDetail"/>.<see cref="ServerFaultDetail.Detail"/> serialization on server call handler.
/// </summary>
public interface IServerFaultDetailSerializer
{
    /// <summary>
    /// Serialize <paramref name="detailType"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="detailType">A type of value, provided by <see cref="ServerFaultDetail"/>.<see cref="ServerFaultDetail.Detail"/>.</param>
    /// <returns>A string representation of <paramref name="detailType"/>.</returns>
    string SerializeDetailType(Type detailType);

    /// <summary>
    /// Serialize <paramref name="detail"/> to byte array.
    /// </summary>
    /// <param name="marshallerFactory">The current <see cref="IMarshallerFactory"/>.</param>
    /// <param name="detail">A value, provided by <see cref="ServerFaultDetail"/>.<see cref="ServerFaultDetail.Detail"/>.</param>
    /// <returns>A byte array representation of <paramref name="detail"/>.</returns>
    byte[] SerializeDetail(IMarshallerFactory marshallerFactory, object detail);
}