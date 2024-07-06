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

using Grpc.Core;

namespace ServiceModel.Grpc.Interceptors;

/// <summary>
/// Allows an implementer to perform custom error processing on client call.
/// </summary>
public interface IClientErrorHandler
{
    /// <summary>
    /// Handle the exception that was raised by by <see cref="CallInvoker"/>.
    /// </summary>
    /// <param name="context">The current call context.</param>
    /// <param name="detail">The exception details.</param>
    void ThrowOrIgnore(ClientCallInterceptorContext context, ClientFaultDetail detail);
}