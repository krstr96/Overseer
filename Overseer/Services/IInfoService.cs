using Overseer.Models;

namespace Overseer.Services;

public interface IInfoService
{
    RootInfoModel GetRootInfo();

    FolderInfoModel GetFolderInfo(Guid? folderId);

    TaskInfoModel GetTaskInfo(Guid? folderId, Guid? taskId);
}
