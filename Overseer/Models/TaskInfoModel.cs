namespace Overseer.Models;

public class TaskInfoModel
{
    public TaskInfoModel
    (
        Guid id,
        string name,
        string command,
        string arguments,
        string? workingDirectory,
        bool onFailRestart
    )
    {
        Id = id;
        Name = name;
        Command = command;
        Arguments = arguments;
        WorkingDirectory = workingDirectory;
        OnFailRestart = onFailRestart;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Command { get; }

    public string Arguments { get; }

    public string? WorkingDirectory { get; }

    public bool OnFailRestart { get; }
}
