using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarkdownCollab.Common;
using System.Security.Cryptography;

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

    [ValidateAntiForgeryToken]
    public IActionResult OnPost(string password)
    {
        if (PasswordIsValid(password))
        {
            RegenerateSessionIdentifier();
            MarkUserAsAuthenticated();
            return RedirectToHomePage();
        }

        DisplayInvalidPasswordError();
        return Page();
    }

    private void RegenerateSessionIdentifier()
    {
        var existingSessionData = ExtractExistingSessionData();
        HttpContext.Session.Clear();
        RestoreSessionData(existingSessionData);
    }

    private Dictionary<string, string> ExtractExistingSessionData()
    {
        var sessionData = new Dictionary<string, string>();
        foreach (var key in HttpContext.Session.Keys)
        {
            var value = HttpContext.Session.GetString(key);
            if (value != null)
            {
                sessionData[key] = value;
            }
        }
        return sessionData;
    }

    private void RestoreSessionData(Dictionary<string, string> sessionData)
    {
        foreach (var kvp in sessionData)
        {
            HttpContext.Session.SetString(kvp.Key, kvp.Value);
        }
    }

    private bool PasswordIsValid(string providedPassword)
    {
        var expectedPassword = GetExpectedPassword();
        return PasswordsMatchInConstantTime(providedPassword, expectedPassword);
    }

    private static bool PasswordsMatchInConstantTime(string providedPassword, string expectedPassword)
    {
        if (providedPassword == null || expectedPassword == null)
        {
            return false;
        }

        var providedBytes = System.Text.Encoding.UTF8.GetBytes(providedPassword);
        var expectedBytes = System.Text.Encoding.UTF8.GetBytes(expectedPassword);

        return CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
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
