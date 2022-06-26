using Overseer.Models;

namespace Overseer.Queues;

public interface ILogsQueue
{
    Task QueueAsync(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel, string log);

    Task<(FolderInfoModel, TaskInfoModel, string)> DequeueAsync();
}
