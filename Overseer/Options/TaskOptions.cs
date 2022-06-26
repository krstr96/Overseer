namespace Overseer.Options;

public class TaskOptions
{
    public string? Name { get; set; }

    public string? Command { get; set; }

    public string? Arguments { get; set; }

    public string? WorkingDirectory { get; set; }

    public bool? OnFailRestart { get; set; }
}
