using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Overseer.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? Message { get; private set; }

    public void OnGet()
    {
        HandleError();
    }

    public void OnPost()
    {
        HandleError();
    }

    private void HandleError()
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature == null)
        {
            Message = "No Message Provided";
        }
        else
        {
            Message = exceptionHandlerPathFeature.Error.Message;
        }
    }
}
