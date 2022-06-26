using System.Diagnostics;
using Overseer.Models;
using Overseer.Queues;
using Overseer.Services;

namespace Overseer.Managers;

public class ProcessManager : BackgroundService
{
    private readonly ILogger<ProcessManager> _logger;
    private readonly IProcessService _processService;
    private readonly IProcessQueue _processQueue;
    private readonly IStateQueue _stateQueue;
    private readonly ILogsQueue _logsQueue;

    public ProcessManager
    (
        ILogger<ProcessManager> logger,
        IProcessService processService,
        IProcessQueue processQueue,
        IStateQueue stateQueue,
        ILogsQueue logsQueue
    )
    {
        _logger = logger;
        _processService = processService;
        _processQueue = processQueue;
        _stateQueue = stateQueue;
        _logsQueue = logsQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var (folderInfoModel, taskInfoModel, processAction) = await _processQueue.DequeueAsync();

            await TryHandleAsync(folderInfoModel, taskInfoModel, processAction);
        }
    }

    private async Task TryHandleAsync(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel, ProcessAction processAction)
    {
        try
        {
            switch (processAction)
            {
                case ProcessAction.Start:
                    await HandleStartAsync(folderInfoModel, taskInfoModel);
                    break;
                case ProcessAction.Reset:
                    await HandleStopAsync(folderInfoModel, taskInfoModel);
                    await HandleStartAsync(folderInfoModel, taskInfoModel);
                    break;
                case ProcessAction.Stop:
                    await HandleStopAsync(folderInfoModel, taskInfoModel);
                    break;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "{folderName} {taskName} {action} failed", folderInfoModel.Name, taskInfoModel.Name, processAction.ToString());
        }
    }

    private async Task HandleStartAsync(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel)
    {
        var process = _processService.GetProcess(folderInfoModel.Id, taskInfoModel.Id);

        if (process != null || process?.HasExited == false)
        {
            return;
        }

        process = new Process();

        process.StartInfo = BuildProcessStartInfo(taskInfoModel);

        process.EnableRaisingEvents = true;

        process.Exited += (_, _) => ProcessExit(process, folderInfoModel, taskInfoModel);
        process.OutputDataReceived += (_, args) => ProcessLog(folderInfoModel, taskInfoModel, args.Data);
        process.ErrorDataReceived += (_, args) => ProcessLog(folderInfoModel, taskInfoModel, args.Data);

        await Task.Delay(TimeSpan.FromSeconds(1));

        var started = process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await Task.Delay(TimeSpan.FromSeconds(1));

        if (!started || process.HasExited)
        {
            _processService.SetProcess(folderInfoModel.Id, taskInfoModel.Id, null);
            await _stateQueue.QueueAsync(folderInfoModel, taskInfoModel, TaskState.Stopped);
        }
        else
        {
            _processService.SetProcess(folderInfoModel.Id, taskInfoModel.Id, process);
            await _stateQueue.QueueAsync(folderInfoModel, taskInfoModel, TaskState.Started);
        }
    }

    private static ProcessStartInfo BuildProcessStartInfo(TaskInfoModel taskInfoModel)
    {
        var processStartInfo = new ProcessStartInfo();

        processStartInfo.FileName = taskInfoModel.Command;
        processStartInfo.Arguments = taskInfoModel.Arguments;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.UseShellExecute = false;
        processStartInfo.CreateNoWindow = true;

        if (taskInfoModel.WorkingDirectory != null)
        {
            processStartInfo.WorkingDirectory = taskInfoModel.WorkingDirectory;
        }

        return processStartInfo;
    }

    private async Task HandleStopAsync(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel)
    {
        _processService.SetProcess(folderInfoModel.Id, taskInfoModel.Id, null);

        await _stateQueue.QueueAsync(folderInfoModel, taskInfoModel, TaskState.Stopped);
    }

    private void ProcessExit(Process process, FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel)
    {
        try
        {
            _processService.SetProcess(folderInfoModel.Id, taskInfoModel.Id, null);

            _stateQueue.QueueAsync(folderInfoModel, taskInfoModel, TaskState.Stopped).GetAwaiter().GetResult();

            if (process.ExitCode != 0 && taskInfoModel.OnFailRestart)
            {
                Task.Delay(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();

                _processQueue.QueueStartAsync(folderInfoModel.Id, taskInfoModel.Id).GetAwaiter().GetResult();
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to handle process exit");
        }
    }

    private void ProcessLog(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel, string? log)
    {
        if (log == null)
        {
            return;
        }

        try
        {
            _logsQueue.QueueAsync(folderInfoModel, taskInfoModel, log).GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to handle process log");
        }
    }
}
