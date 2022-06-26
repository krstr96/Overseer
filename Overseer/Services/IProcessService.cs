using System.Diagnostics;

namespace Overseer.Services;

public interface IProcessService
{
    bool IsRunning(Guid folderId, Guid taskId);

    void SetProcess(Guid folderId, Guid taskId, Process? process);

    Process? GetProcess(Guid folderId, Guid taskId);
}
