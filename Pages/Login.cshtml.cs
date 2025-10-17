using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarkdownCollab.Common;

namespace MarkdownCollab.Pages;

public class LoginModel : PageModel
{
    private readonly IConfiguration _configuration;

    public string? ErrorMessage { get; set; }

    public LoginModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnGet()
    {
    }

    public IActionResult OnPost(string password)
    {
        if (PasswordIsValid(password))
        {
            MarkUserAsAuthenticated();
            return RedirectToHomePage();
        }

        DisplayInvalidPasswordError();
        return Page();
    }

    private bool PasswordIsValid(string providedPassword)
    {
        var expectedPassword = GetExpectedPassword();
        return providedPassword == expectedPassword;
    }

    private string GetExpectedPassword()
    {
        return _configuration[ApplicationConstants.Authentication.PasswordConfigKey]
            ?? Environment.GetEnvironmentVariable(ApplicationConstants.Authentication.PasswordConfigKey)
            ?? throw new InvalidOperationException(ApplicationConstants.ErrorMessages.PasswordNotConfigured);
    }

    private void MarkUserAsAuthenticated()
    {
        HttpContext.Session.SetString(
            ApplicationConstants.Authentication.SessionKey,
            ApplicationConstants.Authentication.SessionValue
        );
    }

    private IActionResult RedirectToHomePage()
    {
        return RedirectToPage(ApplicationConstants.Routes.Index);
    }

    private void DisplayInvalidPasswordError()
    {
        ErrorMessage = ApplicationConstants.ErrorMessages.InvalidPassword;
    }
}
