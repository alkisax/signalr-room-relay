using System;
using Microsoft.AspNetCore.SignalR;

namespace backend_dotnet;

public class MorseHub : Hub
{
  private static readonly Dictionary<string, HashSet<string>> Rooms = new();
  private static readonly Dictionary<string, string> ConnectionRooms = new();

  public override async Task OnConnectedAsync()
  {
    Console.WriteLine($"📡 Morse connected: {Context.ConnectionId}");
    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    Console.WriteLine($"❌ Morse disconnected: {Context.ConnectionId}");
    if (
      ConnectionRooms.TryGetValue(
        Context.ConnectionId,
        out string? roomId
      )
    )
    {
      Console.WriteLine($"client was in morse room: {roomId}");
      Rooms[roomId].Remove(Context.ConnectionId);
      int count = Rooms[roomId].Count;

      Console.WriteLine($"morse room {roomId} users left: {count}");

      await Clients.Group(roomId).SendAsync("RoomUsers", count);
      ConnectionRooms.Remove(Context.ConnectionId);
      if (count == 0)
      {
        Rooms.Remove(roomId);
      }
    }
    await base.OnDisconnectedAsync(exception);
  }

  public async Task JoinRoom(string roomId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

    if (!Rooms.ContainsKey(roomId))
    {
      Rooms[roomId] = new HashSet<string>();
    }

    Rooms[roomId].Add(Context.ConnectionId);
    ConnectionRooms[Context.ConnectionId] = roomId;
    int count = Rooms[roomId].Count;
    Console.WriteLine($"🫂 {Context.ConnectionId} joined morse room {roomId}. Users: {count}");

    await Clients.Group(roomId).SendAsync("RoomUsers", count);
  }

  public async Task SendMorse(MorsePayloadDto payload)
  {
    Console.WriteLine($"📡 morse room: {payload.RoomId}");
    Console.WriteLine($"📡 morse letters: {string.Join("", payload.MorseLetters)}");
    Console.WriteLine($"📡 translated text: {payload.TranslatedText}");
    await Clients.OthersInGroup(payload.RoomId).SendAsync("ReceiveMorse", payload);
  }
}
