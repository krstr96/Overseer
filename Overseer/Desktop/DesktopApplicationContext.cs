using System.ComponentModel;

namespace Overseer.Desktop;

public class DesktopApplicationContext : IDisposable
{
    public DesktopApplicationContext
    (
        WebApplication webApp,
        AutoResetEvent webAppEvent,
        BackgroundWorker webAppWorker,
        NotifyIcon notifyIcon
    )
    {
        WebApp = webApp;
        WebAppEvent = webAppEvent;
        WebAppWorker = webAppWorker;
        NotifyIcon = notifyIcon;
    }

    public WebApplication WebApp { get; private set; }

    public AutoResetEvent WebAppEvent { get; private set; }

    public BackgroundWorker WebAppWorker { get; private set; }

    public NotifyIcon NotifyIcon { get; private set; }

    public T GetRequiredService<T>() where T : notnull
    {
        return WebApp.Services.GetRequiredService<T>();
    }

    public void Dispose()
    {
        if (NotifyIcon != null!)
        {
            NotifyIcon.Dispose();
            NotifyIcon = null!;
        }

        if (WebApp != null!)
        {
            var disposeWebAppTask = WebApp.DisposeAsync();
            while (!disposeWebAppTask.IsCompleted)
            {
                Thread.Sleep(100);
            }
            WebApp = null!;
        }

        if (WebAppEvent != null!)
        {
            WebAppEvent.Dispose();
            WebAppEvent = null!;
        }

        if (WebAppWorker != null!)
        {
            WebAppWorker.Dispose();
            WebAppWorker = null!;
        }

        GC.SuppressFinalize(this);
    }
}
