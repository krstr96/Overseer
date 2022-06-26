using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Overseer.Queues;

namespace Overseer.Pages;

public class ResetAllModel : PageModel
{
    private readonly IProcessQueue _processQueue;

    public ResetAllModel(IProcessQueue processQueue)
    {
        _processQueue = processQueue;
    }

    public IActionResult OnGet()
    {
        return Redirect("/");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _processQueue.QueueResetAllAsync();

        var referer = Request.Headers.Referer;

        return string.IsNullOrWhiteSpace(referer) ? Redirect("/") : Redirect(referer);
    }
}
