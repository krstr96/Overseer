namespace Overseer.Models;

public class RootInfoModel
{
    public RootInfoModel(List<FolderInfoModel> folderInfoModels)
    {
        FolderInfoModels = folderInfoModels;
    }

    public List<FolderInfoModel> FolderInfoModels { get; }
}
