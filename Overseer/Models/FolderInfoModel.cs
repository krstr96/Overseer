namespace Overseer.Models;

public class FolderInfoModel
{
    public FolderInfoModel(Guid id, string name, List<TaskInfoModel> taskInfoModels)
    {
        Id = id;
        Name = name;
        TaskInfoModels = taskInfoModels;
    }

    public Guid Id { get; }

    public string Name { get; }

    public List<TaskInfoModel> TaskInfoModels { get; }
}
