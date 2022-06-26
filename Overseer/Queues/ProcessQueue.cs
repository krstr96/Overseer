using System.Threading.Channels;
using Overseer.Models;
using Overseer.Services;

namespace Overseer.Queues;

public class ProcessQueue : IProcessQueue
{
    private readonly IInfoService _infoService;
    private readonly IStateService _stateService;
    private readonly Channel<(FolderInfoModel, TaskInfoModel, ProcessAction)> _queue;

    public ProcessQueue(IInfoService infoService, IStateService stateService)
    {
        _infoService = infoService;
        _stateService = stateService;
        _queue = Channel.CreateUnbounded<(FolderInfoModel, TaskInfoModel, ProcessAction)>();
    }

    public async Task QueueStartAllAsync()
    {
        await QueueAllAsync(ProcessAction.Start);
    }

    public async Task QueueStartAsync(Guid folderId, Guid taskId)
    {
        await QueueAsync(folderId, taskId, ProcessAction.Start);
    }

    public async Task QueueResetAllAsync()
    {
        var tasks = new List<Task>();

        var rootInfoModel = _infoService.GetRootInfo();

        foreach (var folderInfoModel in rootInfoModel.FolderInfoModels)
        {
            foreach (var taskInfoModel in folderInfoModel.TaskInfoModels)
            {
                if (_stateService.GetState(folderInfoModel.Id, taskInfoModel.Id) == TaskState.Started)
                {
                    tasks.Add(QueueAsync(folderInfoModel, taskInfoModel, ProcessAction.Reset));
                }
            }
        }

        await Task.WhenAll(tasks);
    }

    public async Task QueueResetAsync(Guid folderId, Guid taskId)
    {
        await QueueAsync(folderId, taskId, ProcessAction.Reset);
    }

    public async Task QueueStopAllAsync()
    {
        await QueueAllAsync(ProcessAction.Stop);
    }

    public async Task QueueStopAsync(Guid folderId, Guid taskId)
    {
        await QueueAsync(folderId, taskId, ProcessAction.Stop);
    }

    public async Task<(FolderInfoModel, TaskInfoModel, ProcessAction)> DequeueAsync()
    {
        return await _queue.Reader.ReadAsync();
    }

    private async Task QueueAllAsync(ProcessAction processAction)
    {
        var tasks = new List<Task>();

        var rootInfoModel = _infoService.GetRootInfo();

        foreach (var folderInfoModel in rootInfoModel.FolderInfoModels)
        {
            foreach (var taskInfoModel in folderInfoModel.TaskInfoModels)
            {
                tasks.Add(QueueAsync(folderInfoModel, taskInfoModel, processAction));
            }
        }

        await Task.WhenAll(tasks);
    }

    private async Task QueueAsync(Guid folderId, Guid taskId, ProcessAction processAction)
    {
        var folderInfoModel = _infoService.GetFolderInfo(folderId);
        var taskInfoModel = _infoService.GetTaskInfo(folderId, taskId);

        await QueueAsync(folderInfoModel, taskInfoModel, processAction);
    }

    private async Task QueueAsync(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel, ProcessAction processAction)
    {
        await _queue.Writer.WriteAsync((folderInfoModel, taskInfoModel, processAction));
    }
}
