using System.Threading.Channels;
using Overseer.Models;

namespace Overseer.Queues;

public class StateQueue : IStateQueue
{
    private readonly Channel<(FolderInfoModel, TaskInfoModel, TaskState)> _queue;

    public StateQueue()
    {
        _queue = Channel.CreateUnbounded<(FolderInfoModel, TaskInfoModel, TaskState)>();
    }

    public async Task QueueAsync(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel, TaskState taskState)
    {
        await _queue.Writer.WriteAsync((folderInfoModel, taskInfoModel, taskState));
    }

    public async Task<(FolderInfoModel, TaskInfoModel, TaskState)> DequeueAsync()
    {
        return await _queue.Reader.ReadAsync();
    }
}
