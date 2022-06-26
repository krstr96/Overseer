using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Overseer.Queues;

namespace Overseer.Pages;

public class StopAllModel : PageModel
{
    private readonly IProcessQueue _processQueue;

    public StopAllModel(IProcessQueue processQueue)
    {
        _processQueue = processQueue;
    }

    public IActionResult OnGet()
    {
        return Redirect("/");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _processQueue.QueueStopAllAsync();

        var referer = Request.Headers.Referer;

        return string.IsNullOrWhiteSpace(referer) ? Redirect("/") : Redirect(referer);
    }
}
