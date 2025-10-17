using MarkdownCollab.Common;
using MarkdownCollab.Data;
using MarkdownCollab.Models;
using Microsoft.EntityFrameworkCore;

namespace MarkdownCollab.Services;

public class RoomService
{
    private readonly ApplicationDbContext _context;

    public RoomService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DiagramRoom> CreateRoomAsync()
    {
        var uniqueRoomCode = await GenerateUniqueRoomCodeAsync();
        var newRoom = CreateNewRoomWithCode(uniqueRoomCode);

        _context.DiagramRooms.Add(newRoom);
        await _context.SaveChangesAsync();

        return newRoom;
    }

    public async Task<DiagramRoom?> GetRoomAsync(string roomCode)
    {
        return await _context.DiagramRooms
            .FirstOrDefaultAsync(room => room.RoomCode == roomCode);
    }

    public async Task<bool> UpdateDiagramAsync(string roomCode, string content)
    {
        var room = await GetRoomAsync(roomCode);

        if (RoomDoesNotExist(room))
        {
            return false;
        }

        UpdateDiagramContent(room!, content);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ResetDiagramAsync(string roomCode)
    {
        var room = await GetRoomAsync(roomCode);

        if (RoomDoesNotExist(room))
        {
            return false;
        }

        ResetDiagramToDefault(room!);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<string> GenerateUniqueRoomCodeAsync()
    {
        var roomCode = GenerateRandomRoomCode();

        while (await RoomCodeAlreadyExists(roomCode))
        {
            roomCode = GenerateRandomRoomCode();
        }

        return roomCode;
    }

    private async Task<bool> RoomCodeAlreadyExists(string roomCode)
    {
        return await _context.DiagramRooms.AnyAsync(room => room.RoomCode == roomCode);
    }

    private static DiagramRoom CreateNewRoomWithCode(string roomCode)
    {
        return new DiagramRoom
        {
            RoomCode = roomCode,
            DiagramContent = ApplicationConstants.Diagrams.DefaultContent
        };
    }

    private static void UpdateDiagramContent(DiagramRoom room, string content)
    {
        room.DiagramContent = content;
        room.LastModified = DateTime.UtcNow;
    }

    private static void ResetDiagramToDefault(DiagramRoom room)
    {
        room.DiagramContent = ApplicationConstants.Diagrams.DefaultContent;
        room.LastModified = DateTime.UtcNow;
    }

    private static bool RoomDoesNotExist(DiagramRoom? room)
    {
        return room == null;
    }

    private static string GenerateRandomRoomCode()
    {
        return new string(
            Enumerable.Repeat(ApplicationConstants.RoomCode.AllowedCharacters, ApplicationConstants.RoomCode.Length)
                .Select(characters => characters[Random.Shared.Next(characters.Length)])
                .ToArray()
        );
    }
}
