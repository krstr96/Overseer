using Overseer.Desktop;
using Overseer.Managers;
using Overseer.Middleware;
using Overseer.Models;
using Overseer.Options;
using Overseer.Queues;
using Overseer.Services;

namespace Overseer;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        var webAppBuilder = WebApplication.CreateBuilder();

        webAppBuilder.Logging.AddConsole();

        webAppBuilder.Services.AddRazorPages();

        webAppBuilder.Services.Configure<RootOptions>(webAppBuilder.Configuration.GetRequiredSection("RootOptions"));

        webAppBuilder.Services.AddHostedService<ProcessManager>();
        webAppBuilder.Services.AddHostedService<StateManager>();
        webAppBuilder.Services.AddHostedService<LogsManager>();

        webAppBuilder.Services.AddScoped<LogsMiddleware>();
        webAppBuilder.Services.AddScoped<StateMiddleware>();

        webAppBuilder.Services.AddSingleton<IProcessQueue, ProcessQueue>();
        webAppBuilder.Services.AddSingleton<IStateQueue, StateQueue>();
        webAppBuilder.Services.AddSingleton<ILogsQueue, LogsQueue>();

        webAppBuilder.Services.AddSingleton<IProcessService, ProcessService>();
        webAppBuilder.Services.AddSingleton<IInfoService, InfoService>();
        webAppBuilder.Services.AddSingleton<IStateService, StateService>();
        webAppBuilder.Services.AddSingleton<ILogsService, LogsService>();

        var webApp = webAppBuilder.Build();

        webApp.UseExceptionHandler("/Error");
        webApp.UseStaticFiles();
        webApp.UseWebSockets();
        webApp.UseMiddleware<LogsMiddleware>();
        webApp.UseMiddleware<StateMiddleware>();
        webApp.UseRouting();
        webApp.MapRazorPages();

        var desktopAppBuilder = DesktopApplication.CreateBuilder();

        desktopAppBuilder.UseWebApp(webApp);
        desktopAppBuilder.UseNotify();

        var foldersBuilder = desktopAppBuilder.UseNotifyFolders<FolderInfoModel, TaskInfoModel>();
        foldersBuilder.WithFolders(DesktopApplicationEvents.HandleFindFolders);
        foldersBuilder.WithFolderBuilder(DesktopApplicationEvents.HandleBuildFolder);
        foldersBuilder.WithTasks(DesktopApplicationEvents.HandleFindTasks);
        foldersBuilder.WithTaskBuilder(DesktopApplicationEvents.HandleBuildTask);

        var separatorBuilder = desktopAppBuilder.UseNotifySeparator();
        separatorBuilder.When(DesktopApplicationEvents.HandleSeparate);

        var dashboardItemBuilder = desktopAppBuilder.UseNotifyItem();
        dashboardItemBuilder.WithLabel("Dashboard");
        dashboardItemBuilder.WithClick(DesktopApplicationEvents.HandleDashboardClick);

        var quitItemBuilder = desktopAppBuilder.UseNotifyItem();
        quitItemBuilder.WithLabel("Quit");
        quitItemBuilder.WithClick(DesktopApplicationEvents.HandleQuitClick);

        var desktopApp = desktopAppBuilder.Build();

        desktopApp.Run();
    }
}
