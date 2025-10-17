using Microsoft.AspNetCore.SignalR;
using MarkdownCollab.Common;
using MarkdownCollab.Services;

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
        await AddCurrentUserToRoomGroup(roomCode);
        await SendCurrentDiagramToUser(roomCode);
    }

    public async Task UpdateDiagram(string roomCode, string content)
    {
        var wasUpdated = await _roomService.UpdateDiagramAsync(roomCode, content);

        if (wasUpdated)
        {
            await BroadcastDiagramUpdateToOthersInRoom(roomCode, content);
        }
    }

    public async Task ResetDiagram(string roomCode)
    {
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

    private static bool RoomExists(object? room)
    {
        return room != null;
    }
}
