using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Overseer.Queues;

namespace Overseer.Pages;

public class ResetModel : PageModel
{
    private readonly IProcessQueue _processQueue;

    public ResetModel(IProcessQueue processQueue)
    {
        _processQueue = processQueue;
    }

    public IActionResult OnGet()
    {
        return Redirect("/");
    }

    public async Task<IActionResult> OnPostAsync(Guid folderId, Guid taskId)
    {
        await _processQueue.QueueResetAsync(folderId, taskId);

        var referer = Request.Headers.Referer;

        return string.IsNullOrWhiteSpace(referer) ? Redirect("/") : Redirect(referer);
    }
}
