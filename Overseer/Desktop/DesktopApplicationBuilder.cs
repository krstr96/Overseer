using System.ComponentModel;

namespace Overseer.Desktop;

public class DesktopApplicationBuilder
{
    private WebApplication? _webApp;
    private AutoResetEvent? _webAppEvent;
    private BackgroundWorker? _webAppWorker;
    private NotifyIcon? _notifyIcon;
    private List<Func<DesktopApplicationContext, List<ToolStripItem>?>>? _toolStripItemBuilders;

    public DesktopApplicationBuilder()
    {
        ApplicationConfiguration.Initialize();
    }

    public void UseWebApp(WebApplication webApp)
    {
        _webApp = webApp;
        _webAppEvent = new AutoResetEvent(false);
        _webAppWorker = new BackgroundWorker();
        _webAppWorker.DoWork += (_, _) =>
        {
            try
            {
                _webApp.Run();
            }
            finally
            {
                _webAppEvent.Set();
            }
        };
    }

    public void UseNotify()
    {
        if (_toolStripItemBuilders == null)
        {
            _toolStripItemBuilders = new List<Func<DesktopApplicationContext, List<ToolStripItem>?>>();
        }

        if (_notifyIcon == null)
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
        }

        _notifyIcon.Visible = true;
        _notifyIcon.Icon = new Icon("wwwroot/favicon.ico");
    }

    public DesktopNotifyFoldersBuilder<TFolder, TItem> UseNotifyFolders<TFolder, TItem>() where TFolder : notnull
    {
        if (_toolStripItemBuilders == null)
        {
            _toolStripItemBuilders = new List<Func<DesktopApplicationContext, List<ToolStripItem>?>>();
        }

        if (_notifyIcon == null)
        {
            _notifyIcon = new NotifyIcon();
        }

        var notifyItemBuilder = DesktopNotifyFoldersBuilder<TFolder, TItem>.CreateBuilder();

        _toolStripItemBuilders.Add(desktopApplicationContext =>
        {
            var toolStripItems = new List<ToolStripItem>();

            var toolStripMenuItems = notifyItemBuilder.Build(desktopApplicationContext);

            if (toolStripMenuItems != null)
            {
                toolStripItems.AddRange(toolStripMenuItems);
            }

            return toolStripItems;
        });

        return notifyItemBuilder;
    }

    public DesktopNotifyItemBuilder UseNotifyItem()
    {
        if (_toolStripItemBuilders == null)
        {
            _toolStripItemBuilders = new List<Func<DesktopApplicationContext, List<ToolStripItem>?>>();
        }

        if (_notifyIcon == null)
        {
            _notifyIcon = new NotifyIcon();
        }

        var notifyItemBuilder = DesktopNotifyItemBuilder.CreateBuilder();

        _toolStripItemBuilders.Add(desktopApplicationContext =>
        {
            var toolStripItem = notifyItemBuilder.Build(desktopApplicationContext);

            var toolStripItems = new List<ToolStripItem>();

            toolStripItems.Add(toolStripItem);

            return toolStripItems;
        });

        return notifyItemBuilder;
    }

    public DesktopNotifySeparatorBuilder UseNotifySeparator()
    {
        if (_toolStripItemBuilders == null)
        {
            _toolStripItemBuilders = new List<Func<DesktopApplicationContext, List<ToolStripItem>?>>();
        }

        if (_notifyIcon == null)
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
        }

        var notifyItemBuilder = DesktopNotifySeparatorBuilder.CreateBuilder();

        _toolStripItemBuilders.Add(desktopApplicationContext =>
        {
            var toolStripItem = notifyItemBuilder.Build(desktopApplicationContext);

            var toolStripItems = new List<ToolStripItem>();

            if (toolStripItem != null)
            {
                toolStripItems.Add(toolStripItem);
            }

            return toolStripItems;
        });

        return notifyItemBuilder;
    }

    public DesktopApplication Build()
    {
        if (_webApp == null || _webAppEvent == null || _webAppWorker == null)
        {
            throw new InvalidOperationException("Web app not set up");
        }

        if (_notifyIcon == null || _notifyIcon.Visible == false || _notifyIcon.Icon == null)
        {
            throw new InvalidOperationException("Notify not set up");
        }

        var desktopApplicationContext = new DesktopApplicationContext
        (
            _webApp,
            _webAppEvent,
            _webAppWorker,
            _notifyIcon
        );

        _notifyIcon.ContextMenuStrip = new ContextMenuStrip();

        if (_toolStripItemBuilders != null)
        {
            foreach (var toolStripItemBuilder in _toolStripItemBuilders)
            {
                var toolStripItems = toolStripItemBuilder(desktopApplicationContext);

                if (toolStripItems != null)
                {
                    foreach (var toolStripItem in toolStripItems)
                    {
                        _notifyIcon.ContextMenuStrip.Items.Add(toolStripItem);
                    }
                }
            }
        }

        return new DesktopApplication(desktopApplicationContext);
    }
}
