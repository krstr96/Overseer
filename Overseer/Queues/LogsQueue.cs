using System.Threading.Channels;
using Overseer.Models;

namespace Overseer.Queues;

public class LogsQueue : ILogsQueue
{
    private readonly Channel<(FolderInfoModel, TaskInfoModel, string)> _queue;

    public LogsQueue()
    {
        _queue = Channel.CreateUnbounded<(FolderInfoModel, TaskInfoModel, string)>();
    }

    public async Task QueueAsync(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel, string log)
    {
        await _queue.Writer.WriteAsync((folderInfoModel, taskInfoModel, log));
    }

    public async Task<(FolderInfoModel, TaskInfoModel, string)> DequeueAsync()
    {
        return await _queue.Reader.ReadAsync();
    }
}
