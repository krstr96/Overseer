using System.Text;
using Overseer.Queues;
using Overseer.Services;

namespace Overseer.Managers;

public class StateManager : BackgroundService
{
    private readonly IStateService _stateService;
    private readonly IStateQueue _stateQueue;

    public StateManager(IStateService stateService, IStateQueue stateQueue)
    {
        _stateService = stateService;
        _stateQueue = stateQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var (folderInfoModel, taskInfoModel, appState) = await _stateQueue.DequeueAsync();

            var data = new Lazy<byte[]>(Encoding.UTF8.GetBytes(appState.ToString()));

            foreach (var (_, (canExecuteHandler, executeHandler)) in _stateService.Handlers)
            {
                if (canExecuteHandler(folderInfoModel.Id, taskInfoModel.Id))
                {
                    await executeHandler(data.Value);
                }
            }
        }
    }
}
