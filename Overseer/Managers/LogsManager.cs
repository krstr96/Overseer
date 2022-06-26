using System.Text;
using Overseer.Queues;
using Overseer.Services;

namespace Overseer.Managers;

public class LogsManager : BackgroundService
{
    private readonly ILogsService _logsService;
    private readonly ILogsQueue _logsQueue;

    public LogsManager(ILogsService logsService, ILogsQueue logsQueue)
    {
        _logsService = logsService;
        _logsQueue = logsQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var (folderInfoModel, taskInfoModel, log) = await _logsQueue.DequeueAsync();

            var data = new Lazy<byte[]>(Encoding.UTF8.GetBytes(log));

            foreach (var (_, (canExecuteHandler, executeHandler)) in _logsService.Handlers)
            {
                if (canExecuteHandler(folderInfoModel.Id, taskInfoModel.Id))
                {
                    await executeHandler(data.Value);
                }
            }
        }
    }
}
