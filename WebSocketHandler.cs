namespace MyPos
{
    using System.Net.WebSockets;
    using System.Text;

    public class WebSocketHandler
    {
        private readonly RequestDelegate _next;

        public WebSocketHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await Echo(context, webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                }
                else
                {
                    var serverMsg = Encoding.UTF8.GetBytes($"Server received: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");
                    await webSocket.SendAsync(new ArraySegment<byte>(serverMsg), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }

}
