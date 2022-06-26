namespace Overseer.Desktop;

public class DesktopApplication : ApplicationContext
{
    private DesktopApplicationContext _desktopApplicationContext;

    public static DesktopApplicationBuilder CreateBuilder()
    {
        return new DesktopApplicationBuilder();
    }

    public DesktopApplication(DesktopApplicationContext desktopApplicationContext)
    {
        _desktopApplicationContext = desktopApplicationContext;
    }

    public void Run()
    {
        _desktopApplicationContext.WebAppWorker.RunWorkerAsync();

        _desktopApplicationContext.NotifyIcon.ContextMenuStrip.Show();
        _desktopApplicationContext.NotifyIcon.ContextMenuStrip.Hide();

        Application.Run();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_desktopApplicationContext != null!)
            {
                _desktopApplicationContext.Dispose();
                _desktopApplicationContext = null!;
            }
        }

        base.Dispose(disposing);
    }
}
