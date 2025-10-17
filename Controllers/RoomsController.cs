using Microsoft.AspNetCore.Mvc;
using MarkdownCollab.Services;

namespace MarkdownCollab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly RoomService _roomService;

    public RoomsController(RoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create()
    {
        var room = await _roomService.CreateRoomAsync();
        return Ok(new { roomCode = room.RoomCode });
    }
}
