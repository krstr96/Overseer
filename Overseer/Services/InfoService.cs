using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Overseer.Models;
using Overseer.Options;

namespace Overseer.Services;

public class InfoService : IInfoService
{
    private readonly RootInfoModel _rootInfoModel;
    private readonly Dictionary<Guid, FolderInfoModel> _folderInfoModels;
    private readonly Dictionary<Guid, Dictionary<Guid, TaskInfoModel>> _taskInfoModels;
    private readonly IOptions<RootOptions> _options;

    public InfoService(IOptions<RootOptions> options)
    {
        _options = options;
        _rootInfoModel = BuildRootInfo();
        _folderInfoModels = BuildFolderInfo();
        _taskInfoModels = BuildTaskInfo();
    }

    public RootInfoModel GetRootInfo()
    {
        return _rootInfoModel;
    }

    public FolderInfoModel GetFolderInfo(Guid? folderId)
    {
        if (!folderId.HasValue)
        {
            throw new Exception("Folder id is missing");
        }

        if (_folderInfoModels.TryGetValue(folderId.Value, out var folderInfoModel))
        {
            return folderInfoModel;
        }

        throw new Exception("Folder does not exist");
    }

    public TaskInfoModel GetTaskInfo(Guid? folderId, Guid? taskId)
    {
        if (!folderId.HasValue)
        {
            throw new Exception("Task id is missing");
        }

        if (!taskId.HasValue)
        {
            throw new Exception("Task id is missing");
        }

        if (_taskInfoModels.TryGetValue(folderId.Value, out var folderInfoModel))
        {
            if (folderInfoModel.TryGetValue(taskId.Value, out var taskInfoModel))
            {
                return taskInfoModel;
            }
        }

        throw new Exception("Task does not exist");
    }

    private RootInfoModel BuildRootInfo()
    {
        var options = _options.Value;

        var folderInfoModels = new List<FolderInfoModel>();

        var rootInfoModel = new RootInfoModel(folderInfoModels);

        if (options.FolderOptions == null)
        {
            return rootInfoModel;
        }

        foreach (var folderOption in options.FolderOptions)
        {
            if (folderOption.Name == null)
            {
                continue;
            }

            var taskInfoModels = new List<TaskInfoModel>();

            var folderId = ConvertStringsToGuid(folderOption.Name);

            var folderInfoModel = new FolderInfoModel
            (
                folderId,
                folderOption.Name,
                taskInfoModels
            );

            folderInfoModels.Add(folderInfoModel);

            if (folderOption.TaskOptions == null)
            {
                continue;
            }

            foreach (var taskOption in folderOption.TaskOptions)
            {
                if (taskOption.Name == null ||
                    taskOption.Command == null ||
                    taskOption.Arguments == null ||
                    taskOption.OnFailRestart == null)
                {
                    continue;
                }

                var taskId = ConvertStringsToGuid(folderOption.Name, taskOption.Name);

                var taskInfoModel = new TaskInfoModel
                (
                    taskId,
                    taskOption.Name,
                    taskOption.Command,
                    taskOption.Arguments,
                    taskOption.WorkingDirectory,
                    taskOption.OnFailRestart.Value
                );

                folderInfoModel.TaskInfoModels.Add(taskInfoModel);
            }
        }

        return rootInfoModel;
    }

    private Dictionary<Guid, FolderInfoModel> BuildFolderInfo()
    {
        var folderInfoModels = new Dictionary<Guid, FolderInfoModel>();

        foreach (var folderInfoModel in _rootInfoModel.FolderInfoModels)
        {
            folderInfoModels.Add(folderInfoModel.Id, folderInfoModel);
        }

        return folderInfoModels;
    }

    private Dictionary<Guid, Dictionary<Guid, TaskInfoModel>> BuildTaskInfo()
    {
        var outerTaskInfoModels = new Dictionary<Guid, Dictionary<Guid, TaskInfoModel>>();

        foreach (var (folderInfoKey, folderInfoModel) in _folderInfoModels)
        {
            var innerTaskInfoModels = new Dictionary<Guid, TaskInfoModel>();

            foreach (var taskInfoModel in folderInfoModel.TaskInfoModels)
            {
                innerTaskInfoModels.Add(taskInfoModel.Id, taskInfoModel);
            }

            outerTaskInfoModels.Add(folderInfoKey, innerTaskInfoModels);
        }

        return outerTaskInfoModels;
    }

    private static Guid ConvertStringsToGuid(params string[] strings)
    {
        var joinedStrings = string.Join(string.Empty, strings);

        var joinedStringsBytes = Encoding.UTF8.GetBytes(joinedStrings);

        using var md5 = MD5.Create();

        var hash = md5.ComputeHash(joinedStringsBytes);

        return new Guid(hash);
    }
}
