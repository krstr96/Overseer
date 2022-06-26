using Microsoft.AspNetCore.Mvc.RazorPages;
using Overseer.Models;
using Overseer.Services;

namespace Overseer.Pages;

public class MetricsModel : PageModel
{
    private readonly IInfoService _infoService;
    private readonly IStateService _stateService;

    public MetricsModel(IInfoService infoService, IStateService stateService)
    {
        _infoService = infoService;
        _stateService = stateService;
    }

    public int StartedTasks { get; private set; }

    public int StoppedTasks { get; private set; }

    public Dictionary<string, (int, int)> States { get; private set; }

    public void OnGet()
    {
        StartedTasks = 0;
        StoppedTasks = 0;
        States = new Dictionary<string, (int, int)>();

        var rootInfoModel = _infoService.GetRootInfo();

        foreach (var folderInfoModel in rootInfoModel.FolderInfoModels)
        {
            var startedApps = 0;
            var stoppedApps = 0;

            foreach (var taskInfoModel in folderInfoModel.TaskInfoModels)
            {
                var appState = _stateService.GetState(folderInfoModel.Id, taskInfoModel.Id);

                if (appState == TaskState.Started)
                {
                    StartedTasks++;
                    startedApps++;
                }
                else
                {
                    StoppedTasks++;
                    stoppedApps++;
                }
            }

            States.Add(folderInfoModel.Name, (startedApps, stoppedApps));
        }
    }
}
