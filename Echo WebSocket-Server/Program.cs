using System.Net;
using System.Net.WebSockets;
using System.Text;

var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8181/");
listener.Start();
Console.WriteLine("Websocket started: ws://localhost:8181/");
Console.WriteLine("Waiting for clients...\n");

while (true)
{
    var httpContext = await listener.GetContextAsync();
    
    if (!httpContext.Request.IsWebSocketRequest)
    {
        httpContext.Response.StatusCode = 400;
        httpContext.Response.Close();
        Console.WriteLine("Rejected not Websokcet");
        continue;
    }

    Console.WriteLine($"New client connected: {httpContext.Request.RemoteEndPoint}");

    // HttpListener сам виконує WebSocket Handshake (101 Switching Protocols)
    var wsContext = await httpContext.AcceptWebSocketAsync(subProtocol: null);
    var ws = wsContext.WebSocket;

    var buffer = new byte[4096];
    
    while (ws.State == WebSocketState.Open)
    {
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", CancellationToken.None);
            Console.WriteLine("Cleint closed \n");
            break;
        }

        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine($"  ← Get: {message}\n");
        
        var reply = $"[Echo]: {message}\n";
        var replyBytes = Encoding.UTF8.GetBytes(reply);
        await ws.SendAsync(
            new ArraySegment<byte>(replyBytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            CancellationToken.None
        );
        Console.WriteLine($"  → Sent: {reply}\n");
    }
}