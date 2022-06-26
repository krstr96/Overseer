using System.Collections.Concurrent;
using Overseer.Models;

namespace Overseer.Services;

public class StateService : IStateService
{
    private readonly IProcessService _processService;

    public StateService(IProcessService processService)
    {
        _processService = processService;
        Handlers = new ConcurrentDictionary<Guid, (Func<Guid, Guid, bool>, Func<byte[], Task>)>();
    }

    public TaskState GetState(Guid folderId, Guid taskId)
    {
        return _processService.IsRunning(folderId, taskId) ? TaskState.Started : TaskState.Stopped;
    }

    public ConcurrentDictionary<Guid, (Func<Guid, Guid, bool>, Func<byte[], Task>)> Handlers { get; }

    public void PermanentlyListen(Guid folderId, Guid taskId, Func<byte[], Task> handler)
    {
        var id = Guid.NewGuid();

        var wrappedHandler = WrapHandler(folderId, taskId, handler);

        Handlers.AddOrUpdate(id, wrappedHandler, (_, _) => wrappedHandler);
    }

    public async Task TemporarilyListenAsync(Guid folderId, Guid taskId, CancellationToken cancellationToken, Func<byte[], Task> handler)
    {
        var id = Guid.NewGuid();

        try
        {
            var wrappedHandler = WrapHandler(folderId, taskId, handler);

            Handlers.AddOrUpdate(id, wrappedHandler, (_, _) => wrappedHandler);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
        finally
        {
            Handlers.Remove(id, out _);
        }
    }

    private static (Func<Guid, Guid, bool>, Func<byte[], Task>) WrapHandler(Guid folderId, Guid taskId, Func<byte[], Task> handler)
    {
        return ((statefolderId, statetaskId) => folderId == statefolderId && taskId == statetaskId, handler);
    }
}
