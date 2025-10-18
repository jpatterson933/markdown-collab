using Microsoft.AspNetCore.SignalR;
using MarkdownCollab.Common;
using MarkdownCollab.Services;
using MarkdownCollab.Models;

namespace MarkdownCollab.Hubs;

public class DiagramHub : Hub
{
    private readonly RoomService _roomService;

    public DiagramHub(RoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task JoinRoom(string roomCode)
    {
        if (RoomCodeIsInvalid(roomCode))
        {
            return;
        }

        await AddCurrentUserToRoomGroup(roomCode);
        await SendCurrentDiagramToUser(roomCode);
    }

    public async Task UpdateDiagram(string roomCode, string content)
    {
        if (RoomCodeIsInvalid(roomCode) || ContentExceedsMaximumLength(content))
        {
            return;
        }

        var wasUpdated = await _roomService.UpdateDiagramAsync(roomCode, content);

        if (wasUpdated)
        {
            await BroadcastDiagramUpdateToOthersInRoom(roomCode, content);
        }
    }

    public async Task ResetDiagram(string roomCode)
    {
        if (RoomCodeIsInvalid(roomCode))
        {
            return;
        }

        var wasReset = await _roomService.ResetDiagramAsync(roomCode);

        if (wasReset)
        {
            await BroadcastDiagramResetToAllInRoom(roomCode);
        }
    }

    private async Task AddCurrentUserToRoomGroup(string roomCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
    }

    private async Task SendCurrentDiagramToUser(string roomCode)
    {
        var room = await _roomService.GetRoomAsync(roomCode);

        if (RoomExists(room))
        {
            await Clients.Caller.SendAsync(ApplicationConstants.SignalR.LoadDiagram, room!.DiagramContent);
        }
    }

    private async Task BroadcastDiagramUpdateToOthersInRoom(string roomCode, string content)
    {
        await Clients.OthersInGroup(roomCode).SendAsync(ApplicationConstants.SignalR.DiagramUpdated, content);
    }

    private async Task BroadcastDiagramResetToAllInRoom(string roomCode)
    {
        var room = await _roomService.GetRoomAsync(roomCode);

        if (RoomExists(room))
        {
            await Clients.Group(roomCode).SendAsync(ApplicationConstants.SignalR.DiagramUpdated, room!.DiagramContent);
        }
    }

    private static bool RoomExists(DiagramRoom? room)
    {
        return room != null;
    }

    private static bool ContentExceedsMaximumLength(string content)
    {
        return content?.Length > ApplicationConstants.Diagrams.MaxContentLength;
    }

    private static bool RoomCodeIsInvalid(string roomCode)
    {
        if (string.IsNullOrEmpty(roomCode))
        {
            return true;
        }

        if (roomCode.Length > ApplicationConstants.RoomCode.MaxLength)
        {
            return true;
        }

        return roomCode.Any(character => !ApplicationConstants.RoomCode.AllowedCharacters.Contains(character));
    }
}
