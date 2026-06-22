// backend-dotnet\Hub\RoomHub.cs
using System;
using Microsoft.AspNetCore.SignalR;

namespace backend_dotnet;

public class RoomHub : Hub
{
  private static readonly Dictionary<string, HashSet<string>> Rooms = new();
  private static readonly Dictionary<string, string> ConnectionRooms = new();
  public override async Task OnConnectedAsync()
  {
    //  με override αλλάζω default behavior του Hub lifecycle
    Console.WriteLine($"🔌 connected: {Context.ConnectionId}");
    // τρέξε και το normal internal SignalR behavior
    await base.OnConnectedAsync();
  }

  //Αυτό το βήμα είναι σημαντικό γιατί: disconnect δεν είναι "method που καλεί ο client" είναι lifecycle event του hub.
  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    Console.WriteLine($"❌ disconnected: {Context.ConnectionId}");
    // πρέπει όμως να τον αφαιρέσουμε και απο τα Rooms. Αλλά τώρα έχουμε ένα πρόβλημα: Από το disconnect έχουμε μόνο Context.ConnectionId Δεν ξέρουμε αν και σε ποιο room ήταν. Για αυτό δημιουργήθηκε το ConnectionRooms
    if (
      // TryGetValue → "αν υπάρχει αυτό το key"
      ConnectionRooms.TryGetValue(
        Context.ConnectionId,
        // out: method επιστρέφει extra τιμή μέσω parameter
        // εδώ: αν βρεθεί το connectionId, βάλε το room value μέσα στο roomId
        out string? roomId
      )
    )
    {
      Console.WriteLine($"client was in room: {roomId}");
      Rooms[roomId].Remove(Context.ConnectionId);
      int count = Rooms[roomId].Count;
      Console.WriteLine($"room {roomId} users left: {count}");
      // ενημέρωσε realtime όλους στο room για το νέο count
      await Clients.Group(roomId).SendAsync("RoomUsers", count);
      ConnectionRooms.Remove(Context.ConnectionId);
      if (count == 0)
      {
        Rooms.Remove(roomId);
      }
    }

    await base.OnDisconnectedAsync(exception);
  }

  /// <summary>
  /// No rooms. Echo back to self
  /// </summary>
  /// <param name="message"></param>
  /// <returns>Task → promise</returns>
  public async Task Echo(string message)
  {
    Console.WriteLine($"📨 received: {message}");

    // Clients: obj που έχει όλες τις connected συνδεσεις
    // caller: ο client που κάλεσε την μέθοδο
    // στείλε μόνο στον client που έκανε invoke.
    // "ReceiveMessage": custom event name - δικό μου, πρέπει να ταιριάζει frontend/backend
    await Clients.Caller.SendAsync(
      "ReceiveMessage",
      message
    );
  }

  /// <summary>
  /// send to all others except sender
  /// </summary>
  /// <param name="message"></param>
  public async Task Announce(string message)
  {
    Console.WriteLine($"announcement: {message}");
    await Clients.Others.SendAsync(
      "ReceiveAnnouncement",
      message
    );
  }

  /// <summary>
  /// Join realtime room/group
  /// Creates room automatically if it does not exist
  /// </summary>
  /// <param name="roomId"></param>
  public async Task JoinRoom(string roomId)
  {
    // Context.ConnectionId: ποιος client μπαίνει.
    await Groups.AddToGroupAsync(
      Context.ConnectionId,
      roomId
    );

    // αν δεν υπάρχει το room δημιουργησε το
    if (!Rooms.ContainsKey(roomId))
    {
      // roomId -> collection απο unique connectionIds
      Rooms[roomId] = new HashSet<string>();
    }
    // πρόσθεσε τον connected client στο room
    Rooms[roomId].Add(Context.ConnectionId);
    // για να μπορούμε να ξέρουμε πια Rooms ηταν ενεργα όταν θα γίνει το disconnect
    ConnectionRooms[Context.ConnectionId] = roomId;
    int count = Rooms[roomId].Count;

    Console.WriteLine($"🫂{Context.ConnectionId} joined room {roomId}. Users: {count}");

    // ενημέρωσε realtime όλους στο room για το νέο count
    await Clients.Group(roomId).SendAsync("RoomUsers", count);
  }

  /// <summary>
  /// Send message to room 
  /// </summary>
  /// <param name="roomId">string</param>
  /// <param name="message">string</param>
  public async Task SendToRoom(string roomId, string message)
  {
    Console.WriteLine($"room {roomId}: {message}");
    await Clients.OthersInGroup(roomId).SendAsync("ReceiveRoomMessage", message);
  }

  /// <summary>
  /// Leave realtime room/group explicitly
  /// </summary>
  /// <param name="roomId">string</param>
  public async Task LeaveRoom(string roomId)
  {
    if (string.IsNullOrWhiteSpace(roomId))
    {
      return;
    }

    if (
      !ConnectionRooms.TryGetValue(
        Context.ConnectionId,
        out string? currentRoomId
      )
    )
    {
      return;
    }

    if (currentRoomId != roomId)
    {
      return;
    }

    await Groups.RemoveFromGroupAsync(
      Context.ConnectionId,
      roomId
    );

    if (Rooms.ContainsKey(roomId))
    {
      Rooms[roomId].Remove(Context.ConnectionId);
      int count = Rooms[roomId].Count;
      Console.WriteLine($"🚪 {Context.ConnectionId} left room {roomId}. Users: {count}");
      ConnectionRooms.Remove(Context.ConnectionId);

      await Clients.Group(roomId).SendAsync(
        "RoomUsers",
        count
      );

      if (count == 0)
      {
        Rooms.Remove(roomId);
      }
    }
  }

}
