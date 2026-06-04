using System.Net.WebSockets;
using System.Text;

using var ws = new ClientWebSocket();

// Підключаємось — тут відбувається Handshake
await ws.ConnectAsync(new Uri("ws://localhost:8181/"), CancellationToken.None);
Console.WriteLine($"Conected! State: {ws.State}\n");

var buffer = new byte[4096];


while (ws.State == WebSocketState.Open)
{
    var msg = Console.ReadLine();
    var bytes = Encoding.UTF8.GetBytes(msg);
    await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    Console.WriteLine($"→ Sent:  {msg}\n");

    // Отримати відповідь
    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    var reply = Encoding.UTF8.GetString(buffer, 0, result.Count);
    Console.WriteLine($"← Get:   {reply}\n");
    Console.WriteLine();
}


// Надіслати Close Frame — коректне закриття
await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
Console.WriteLine($"Conection closed. State: {ws.State}\n");