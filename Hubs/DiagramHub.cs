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
        try
        {
            Console.WriteLine($"JoinRoom called with roomCode: {roomCode}");

            if (RoomCodeIsInvalid(roomCode))
            {
                Console.WriteLine($"Room code invalid: {roomCode}");
                return;
            }

            Console.WriteLine($"Adding user to group: {roomCode}");
            await AddCurrentUserToRoomGroup(roomCode);

            Console.WriteLine($"Sending diagram to user for room: {roomCode}");
            await SendCurrentDiagramToUser(roomCode);

            Console.WriteLine($"JoinRoom completed for: {roomCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in JoinRoom: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
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
        Console.WriteLine($"Getting room from database: {roomCode}");
        var room = await _roomService.GetRoomAsync(roomCode);

        if (RoomExists(room))
        {
            Console.WriteLine($"Room found, sending diagram content. Length: {room!.DiagramContent.Length}");
            await Clients.Caller.SendAsync(ApplicationConstants.SignalR.LoadDiagram, room!.DiagramContent);
            Console.WriteLine($"LoadDiagram message sent");
        }
        else
        {
            Console.WriteLine($"Room NOT found in database: {roomCode}");
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
