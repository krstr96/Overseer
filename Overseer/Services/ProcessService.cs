using System.Collections.Concurrent;
using System.Diagnostics;

namespace Overseer.Services;

public class ProcessService : IProcessService, IDisposable
{
    private readonly IInfoService _infoService;
    private ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Process?>>? _processes;

    public ProcessService(IInfoService infoService)
    {
        _infoService = infoService;
        _processes = BuildProcesses();
    }

    public bool IsRunning(Guid folderId, Guid taskId)
    {
        var process = GetProcess(folderId, taskId);

        if (process == null)
        {
            return false;
        }

        if (process.HasExited)
        {
            return false;
        }

        return true;
    }

    public void SetProcess(Guid folderId, Guid taskId, Process? process)
    {
        if (_processes == null)
        {
            return;
        }

        if (_processes.TryGetValue(folderId, out var folderProcesses))
        {
            if (folderProcesses.TryGetValue(taskId, out var appProcess))
            {
                KillProcess(appProcess);

                folderProcesses.AddOrUpdate(taskId, process, (_, _) => process);
            }
        }
    }

    public Process? GetProcess(Guid folderId, Guid taskId)
    {
        if (_processes == null)
        {
            return null;
        }

        if (_processes.TryGetValue(folderId, out var folderProcesses))
        {
            if (folderProcesses.TryGetValue(taskId, out var appProcess))
            {
                return appProcess;
            }
        }

        return null;
    }

    private ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Process?>> BuildProcesses()
    {
        var rootInfoModel = _infoService.GetRootInfo();

        var folderProcesses = new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Process?>>();

        foreach (var folderInfoModel in rootInfoModel.FolderInfoModels)
        {
            var taskProcesses = new ConcurrentDictionary<Guid, Process?>();

            foreach (var taskInfoModel in folderInfoModel.TaskInfoModels)
            {
                taskProcesses.AddOrUpdate(taskInfoModel.Id, _ => null, (_, _) => null);
            }

            folderProcesses.AddOrUpdate(folderInfoModel.Id, taskProcesses, (_, _) => taskProcesses);
        }

        return folderProcesses;
    }

    public void Dispose()
    {
        if (_processes == null)
        {
            return;
        }

        foreach (var folderProcess in _processes)
        {
            foreach (var taskProcess in folderProcess.Value)
            {
                KillProcess(taskProcess.Value);
            }
        }

        _processes = null;

        GC.SuppressFinalize(this);
    }

    private static void KillProcess(Process? process)
    {
        if (process == null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill();
            }

            process.Dispose();
        }
        catch
        {
            // Eat the exception
        }
    }
}
