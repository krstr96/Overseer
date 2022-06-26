namespace Overseer.Desktop;

public class DesktopNotifyFoldersBuilder<TFolder, TItem> where TFolder : notnull
{
    private Func<DesktopApplicationContext, List<TFolder>>? _folders;
    private Action<TFolder, DesktopNotifyItemBuilder>? _folderBuilder;
    private Func<TFolder, DesktopApplicationContext, List<TItem>>? _tasks;
    private Action<TFolder, TItem, DesktopNotifyItemBuilder>? _taskBuilder;

    public static DesktopNotifyFoldersBuilder<TFolder, TItem> CreateBuilder()
    {
        return new DesktopNotifyFoldersBuilder<TFolder, TItem>();
    }

    private DesktopNotifyFoldersBuilder()
    {

    }

    public void WithFolders(Func<DesktopApplicationContext, List<TFolder>> folders)
    {
        _folders = folders;
    }

    public void WithFolderBuilder(Action<TFolder, DesktopNotifyItemBuilder> builder)
    {
        _folderBuilder = builder;
    }

    public void WithTasks(Func<TFolder, DesktopApplicationContext, List<TItem>> tasks)
    {
        _tasks = tasks;
    }

    public void WithTaskBuilder(Action<TFolder, TItem, DesktopNotifyItemBuilder> builder)
    {
        _taskBuilder = builder;
    }

    public List<ToolStripMenuItem>? Build(DesktopApplicationContext desktopApplicationContext)
    {
        if (_folders == null || _tasks == null)
        {
            return null;
        }

        if (_folderBuilder == null)
        {
            throw new InvalidOperationException("Folder builder not set up");
        }

        if (_taskBuilder == null)
        {
            throw new InvalidOperationException("Task builder not set up");
        }

        var toolStripMenuItems = new List<ToolStripMenuItem>();

        var folders = _folders(desktopApplicationContext);

        foreach (var folder in folders)
        {
            var folderBuilder = DesktopNotifyItemBuilder.CreateBuilder();

            _folderBuilder(folder, folderBuilder);

            var folderToolStripMenuItem = folderBuilder.Build(desktopApplicationContext);

            var tasks = _tasks(folder, desktopApplicationContext);

            foreach (var task in tasks)
            {
                var itemBuilder = DesktopNotifyItemBuilder.CreateBuilder();

                _taskBuilder(folder, task, itemBuilder);

                var appToolStripMenuItem = itemBuilder.Build(desktopApplicationContext);

                folderToolStripMenuItem.DropDownItems.Add(appToolStripMenuItem);
            }

            toolStripMenuItems.Add(folderToolStripMenuItem);
        }

        return toolStripMenuItems;
    }
}
