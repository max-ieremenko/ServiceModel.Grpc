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

using System.ServiceModel;

namespace ServiceModel.Grpc.TestApi.Domain;

[ServiceContract]
public interface IErrorService
{
    [OperationContract]
    void ThrowApplicationException(string message);

    [OperationContract]
    Task ThrowApplicationExceptionAsync(string message);

    [OperationContract]
    void ThrowOperationCanceledException();

    [OperationContract]
    void PassSerializationFail(DomainObjectSerializationFail fail);

    [OperationContract]
    DomainObjectSerializationFail ReturnSerializationFail(string? onDeserializedError = null, string? onSerializedError = null);

    [OperationContract]
    void CancelOperation(CancellationToken token);

    [OperationContract]
    Task CancelOperationAsync(CancellationToken token);

    [OperationContract]
    void ThrowApplicationExceptionAfterCancel(string message, CancellationToken token);

    [OperationContract]
    Task ThrowApplicationExceptionAfterCancelAsync(string message, CancellationToken token);

    [OperationContract]
    Task ThrowApplicationExceptionClientStreaming(IAsyncEnumerable<int> data, int readsCount, string message, CallContext context);

    [OperationContract]
    Task<IAsyncEnumerable<int>> ThrowApplicationExceptionServerStreaming(int writesCount, string message);

    [OperationContract]
    ValueTask<(IAsyncEnumerable<int> Stream, string Message)> ThrowApplicationExceptionServerStreamingHeader(string message);

    [OperationContract]
    IAsyncEnumerable<int> ThrowApplicationExceptionDuplexStreaming(IAsyncEnumerable<int> data, string message, int readsCount, CallContext context);

    [OperationContract]
    Task<(IAsyncEnumerable<int> Stream, string Message)> ThrowApplicationExceptionDuplexStreamingHeader(IAsyncEnumerable<int> data, string message, int readsCount, CallContext context);
}