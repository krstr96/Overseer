using System.Diagnostics;
using System.Text;
using Overseer.Models;
using Overseer.Queues;
using Overseer.Services;

namespace Overseer.Desktop;

public static class DesktopApplicationEvents
{

    public static List<FolderInfoModel> HandleFindFolders(DesktopApplicationContext desktopApplicationContext)
    {
        var infoService = desktopApplicationContext.GetRequiredService<IInfoService>();

        var rootInfoModel = infoService.GetRootInfo();

        return rootInfoModel.FolderInfoModels;
    }

    public static void HandleBuildFolder(FolderInfoModel folderInfoModel,  DesktopNotifyItemBuilder desktopNotifyItemBuilder)
    {
        desktopNotifyItemBuilder.WithLabel(folderInfoModel.Name);
    }

    public static List<TaskInfoModel> HandleFindTasks(FolderInfoModel folderInfoModel, DesktopApplicationContext desktopApplicationContext)
    {
        return folderInfoModel.TaskInfoModels;
    }

    public static void HandleBuildTask(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel, DesktopNotifyItemBuilder desktopNotifyItemBuilder)
    {
        desktopNotifyItemBuilder.WithLabel(taskInfoModel.Name);
        desktopNotifyItemBuilder.WithTooltip("Stopped");
        desktopNotifyItemBuilder.WithIcon("wwwroot/stopped.ico");
        desktopNotifyItemBuilder.WithClick((toolStripMenuItem, desktopApplicationContext) => QueueStateChange(folderInfoModel, taskInfoModel, toolStripMenuItem, desktopApplicationContext));
        desktopNotifyItemBuilder.WithSetup((toolStripMenuItem, desktopApplicationContext) => HandleStateListen(folderInfoModel, taskInfoModel, toolStripMenuItem, desktopApplicationContext));
    }

    private static void QueueStateChange(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel, ToolStripMenuItem toolStripMenuItem, DesktopApplicationContext desktopApplicationContext)
    {
        MethodInvoker method = delegate
        {
            var stateService = desktopApplicationContext.GetRequiredService<IStateService>();
            var processQueue = desktopApplicationContext.GetRequiredService<IProcessQueue>();

            var taskState = stateService.GetState(folderInfoModel.Id, taskInfoModel.Id);

            if (taskState == TaskState.Started)
            {
                processQueue.QueueStopAsync(folderInfoModel.Id, taskInfoModel.Id);
            }
            else
            {
                processQueue.QueueStartAsync(folderInfoModel.Id, taskInfoModel.Id);
            }

            toolStripMenuItem.Enabled = false;
        };

        if (desktopApplicationContext.NotifyIcon.ContextMenuStrip.InvokeRequired)
        {
            desktopApplicationContext.NotifyIcon.ContextMenuStrip.BeginInvoke(method);
        }
        else
        {
            method.Invoke();
        }
    }

    private static void HandleStateListen(FolderInfoModel folderInfoModel, TaskInfoModel taskInfoModel, ToolStripMenuItem toolStripMenuItem, DesktopApplicationContext desktopApplicationContext)
    {
        var stateService = desktopApplicationContext.GetRequiredService<IStateService>();

        stateService.PermanentlyListen(folderInfoModel.Id, taskInfoModel.Id, data => HandleStateChange(data, toolStripMenuItem, desktopApplicationContext));
    }

    private static Task HandleStateChange(byte[] data, ToolStripMenuItem toolStripMenuItem, DesktopApplicationContext desktopApplicationContext)
    {
        MethodInvoker method = delegate
        {
            var value = Encoding.UTF8.GetString(data);

            var taskState = Enum.Parse<TaskState>(value);

            if (taskState == TaskState.Started)
            {
                toolStripMenuItem.ToolTipText = "Started";
                toolStripMenuItem.Image = Image.FromFile("wwwroot/started.ico");
            }
            else
            {
                toolStripMenuItem.ToolTipText = "Stopped";
                toolStripMenuItem.Image = Image.FromFile("wwwroot/stopped.ico");
            }

            toolStripMenuItem.Enabled = true;
        };

        if (desktopApplicationContext.NotifyIcon.ContextMenuStrip.InvokeRequired)
        {
            desktopApplicationContext.NotifyIcon.ContextMenuStrip.BeginInvoke(method);
        }
        else
        {
            method.Invoke();
        }

        return Task.CompletedTask;
    }

    public static bool HandleSeparate(DesktopApplicationContext desktopApplicationContext)
    {
        return desktopApplicationContext.NotifyIcon.ContextMenuStrip.Items.Count > 0;
    }

    public static void HandleDashboardClick(ToolStripMenuItem toolStripMenuItem, DesktopApplicationContext desktopApplicationContext)
    {
        var address = desktopApplicationContext.WebApp.Urls.First();

        Process.Start("explorer", address);
    }

    public static void HandleQuitClick(ToolStripMenuItem toolStripMenuItem, DesktopApplicationContext desktopApplicationContext)
    {
        desktopApplicationContext.WebApp.Lifetime.StopApplication();

        desktopApplicationContext.WebAppEvent.WaitOne();

        Application.Exit();
    }
}
