using System.Collections.Concurrent;

namespace Overseer.Services;

public class LogsService : ILogsService
{
    public LogsService()
    {
        Handlers = new ConcurrentDictionary<Guid, (Func<Guid, Guid, bool>, Func<byte[], Task>)>();
    }

    public ConcurrentDictionary<Guid, (Func<Guid, Guid, bool>, Func<byte[], Task>)> Handlers { get; }

    public async Task TemporarilyListenAsync(Guid folderId, Guid taskId, CancellationToken cancellationToken, Func<byte[], Task> handler)
    {
        var id = Guid.NewGuid();

        try
        {
            var wrappedHandler = WrapHandler(folderId, taskId, handler);

            Handlers.AddOrUpdate(id, wrappedHandler, (_, _) => wrappedHandler);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
        finally
        {
            Handlers.Remove(id, out _);
        }
    }

    private static (Func<Guid, Guid, bool>, Func<byte[], Task>) WrapHandler(Guid folderId, Guid taskId, Func<byte[], Task> handler)
    {
        return ((logFolderId, logTaskId) => folderId == logFolderId && taskId == logTaskId, handler);
    }
}
