using System.Collections.Concurrent;

namespace Overseer.Services;

public interface ILogsService
{
    ConcurrentDictionary<Guid, (Func<Guid, Guid, bool>, Func<byte[], Task>)> Handlers { get; }

    Task TemporarilyListenAsync(Guid folderId, Guid taskId, CancellationToken cancellationToken, Func<byte[], Task> handler);
}
