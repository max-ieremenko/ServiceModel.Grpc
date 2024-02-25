using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleChat.Shared;

[ServiceContract]
public interface IChatService
{
    [OperationContract]
    Task MessageAsync(string connectionId, string content, CancellationToken cancellationToken);

    [OperationContract]
    IAsyncEnumerable<ChatMessage> JoinAsync(CancellationToken cancellationToken);
}