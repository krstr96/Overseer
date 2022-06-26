using System.Collections.Concurrent;
using Overseer.Models;

namespace Overseer.Services;

public interface IStateService
{
    TaskState GetState(Guid folderId, Guid taskId);

    ConcurrentDictionary<Guid, (Func<Guid, Guid, bool>, Func<byte[], Task>)> Handlers { get; }

    void PermanentlyListen(Guid folderId, Guid taskId, Func<byte[], Task> handler);

    Task TemporarilyListenAsync(Guid folderId, Guid taskId, CancellationToken cancellationToken, Func<byte[], Task> handler);
}
