using System.Net.WebSockets;
using Overseer.Services;

namespace Overseer.Middleware;

public class StateMiddleware : IMiddleware
{
    private readonly IStateService _stateService;

    public StateMiddleware(IStateService stateService)
    {
        _stateService = stateService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/State"))
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                throw new Exception("Only supports web sockets");
            }

            context.Request.Query.TryGetValue("folderId", out var folderIdParameter);
            context.Request.Query.TryGetValue("taskId", out var taskIdParameter);

            if (!Guid.TryParse(folderIdParameter, out var folderId))
            {
                throw new Exception("Unable to parse folder id");
            }

            if (!Guid.TryParse(taskIdParameter, out var taskId))
            {
                throw new Exception("Unable to parse task id");
            }

            using var websocket = await context.WebSockets.AcceptWebSocketAsync();

            await _stateService.TemporarilyListenAsync(folderId, taskId, context.RequestAborted, data => HandleAsync(data, websocket));
        }
        else
        {
            await next(context);
        }
    }

    private static async Task HandleAsync(byte[] data, WebSocket websocket)
    {
        await websocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
