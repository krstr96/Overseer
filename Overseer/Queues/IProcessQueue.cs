using Overseer.Models;

namespace Overseer.Queues;

public interface IProcessQueue
{
    Task QueueStartAllAsync();

    Task QueueStartAsync(Guid folderId, Guid taskId);

    Task QueueResetAllAsync();

    Task QueueResetAsync(Guid folderId, Guid taskId);

    Task QueueStopAllAsync();

    Task QueueStopAsync(Guid folderId, Guid taskId);

    Task<(FolderInfoModel, TaskInfoModel, ProcessAction)> DequeueAsync();
}
