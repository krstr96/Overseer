using Overseer.Models;

namespace Overseer.Queues;

public interface IStateQueue
{
    Task QueueAsync(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel, TaskState taskState);

    Task<(FolderInfoModel, TaskInfoModel, TaskState)> DequeueAsync();
}
