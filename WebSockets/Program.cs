using System.Net.WebSockets;

using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use HTTP/2
builder.WebHost.ConfigureKestrel(options => {
    options.ListenAnyIP(5001, listenOptions => {
        listenOptions.Protocols = HttpProtocols.Http2;
        listenOptions.UseHttps(); // Ensure HTTPS is used
    });
});

var app = builder.Build();

app.UseWebSockets();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.Map("/ws", async (HttpContext context) => {
    if (context.WebSockets.IsWebSocketRequest) {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await Echo(context, webSocket);
    } else {
        context.Response.StatusCode = 400;
    }
});

app.Run();

async Task Echo(HttpContext context, WebSocket webSocket) {
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    while (!result.CloseStatus.HasValue) {
        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}
