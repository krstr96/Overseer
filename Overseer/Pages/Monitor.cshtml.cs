using Microsoft.AspNetCore.Mvc.RazorPages;
using Overseer.Models;
using Overseer.Services;

namespace Overseer.Pages;

public class MonitorModel : PageModel
{
    private readonly IInfoService _infoService;
    private readonly IStateService _stateService;

    public MonitorModel(IInfoService infoService, IStateService stateService)
    {
        _infoService = infoService;
        _stateService = stateService;
    }

    public FolderInfoModel FolderInfo { get; set; }

    public TaskInfoModel TaskInfo { get; set; }

    public TaskState TaskState { get; set; }

    public void OnGet(Guid? folderId, Guid? taskId)
    {
        FolderInfo = _infoService.GetFolderInfo(folderId);
        TaskInfo = _infoService.GetTaskInfo(folderId, taskId);

        if (folderId.HasValue && taskId.HasValue)
        {
            TaskState = _stateService.GetState(folderId.Value, taskId.Value);
        }
        else
        {
            TaskState = TaskState.Stopped;
        }
    }
}
