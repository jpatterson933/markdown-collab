using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarkdownCollab.Common;
using MarkdownCollab.Services;

namespace MarkdownCollab.Pages;

public class RoomModel : PageModel
{
    private readonly RoomService _roomService;

    public string RoomCode { get; set; } = string.Empty;

    public RoomModel(RoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task<IActionResult> OnGetAsync(string code)
    {
        if (RoomCodeIsMissing(code))
        {
            return RedirectToHomePage();
        }

        RoomCode = NormalizeRoomCode(code);

        if (await RoomDoesNotExist(RoomCode))
        {
            return RedirectToHomePage();
        }

        return Page();
    }

    private static bool RoomCodeIsMissing(string roomCode)
    {
        return string.IsNullOrEmpty(roomCode);
    }

    private static string NormalizeRoomCode(string roomCode)
    {
        return roomCode.ToUpper();
    }

    private async Task<bool> RoomDoesNotExist(string roomCode)
    {
        var room = await _roomService.GetRoomAsync(roomCode);
        return room == null;
    }

    private IActionResult RedirectToHomePage()
    {
        return RedirectToPage(ApplicationConstants.Routes.Index);
    }
}
