using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarkdownCollab.Common;

namespace MarkdownCollab.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        ClearUserSession();
        return RedirectToLoginPage();
    }

    private void ClearUserSession()
    {
        HttpContext.Session.Clear();
    }

    private IActionResult RedirectToLoginPage()
    {
        return RedirectToPage(ApplicationConstants.Routes.Login);
    }
}
