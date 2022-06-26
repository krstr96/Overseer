namespace Overseer.Desktop;

public class DesktopNotifySeparatorBuilder
{
    private Func<DesktopApplicationContext, bool>? _condition;

    public static DesktopNotifySeparatorBuilder CreateBuilder()
    {
        return new DesktopNotifySeparatorBuilder();
    }

    private DesktopNotifySeparatorBuilder()
    {
    }

    public void When(Func<DesktopApplicationContext, bool>? condition)
    {
        _condition = condition;
    }

    public ToolStripSeparator? Build(DesktopApplicationContext desktopApplicationContext)
    {
        if (_condition?.Invoke(desktopApplicationContext) == true)
        {
            return new ToolStripSeparator();
        }

        return null;
    }
}
