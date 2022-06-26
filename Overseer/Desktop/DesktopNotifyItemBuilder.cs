namespace Overseer.Desktop;

public class DesktopNotifyItemBuilder
{
    private string? _label;
    private string? _tooltip;
    private string? _iconPath;
    private Action<ToolStripMenuItem, DesktopApplicationContext>? _click;
    private Action<ToolStripMenuItem, DesktopApplicationContext>? _setup;

    public static DesktopNotifyItemBuilder CreateBuilder()
    {
        return new DesktopNotifyItemBuilder();
    }

    private DesktopNotifyItemBuilder()
    {
    }

    public void WithLabel(string? label)
    {
        _label = label;
    }

    public void WithTooltip(string? tooltip)
    {
        _tooltip = tooltip;
    }

    public void WithIcon(string? iconPath)
    {
        _iconPath = iconPath;
    }

    public void WithClick(Action<ToolStripMenuItem, DesktopApplicationContext>? click)
    {
        _click = click;
    }

    public void WithSetup(Action<ToolStripMenuItem, DesktopApplicationContext>? setup)
    {
        _setup = setup;
    }

    public ToolStripMenuItem Build(DesktopApplicationContext desktopApplicationContext)
    {
        var toolStripMenuItem = new ToolStripMenuItem();

        if (_label != null)
        {
            toolStripMenuItem.Text = _label;
        }

        if (_tooltip != null)
        {
            toolStripMenuItem.ToolTipText = _tooltip;
        }

        if (_iconPath != null)
        {
            toolStripMenuItem.Image = Image.FromFile(_iconPath);
        }

        if (_click != null)
        {
            toolStripMenuItem.Click += (_, _) => _click(toolStripMenuItem, desktopApplicationContext);
        }

        _setup?.Invoke(toolStripMenuItem, desktopApplicationContext);

        return toolStripMenuItem;
    }
}
