using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarkdownCollab.Services;

namespace MarkdownCollab.Pages.Api.Rooms;

public class CreateModel : PageModel
{
    private readonly RoomService _roomService;

    public CreateModel(RoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var room = await _roomService.CreateRoomAsync();
        return new JsonResult(new { roomCode = room.RoomCode });
    }
}
