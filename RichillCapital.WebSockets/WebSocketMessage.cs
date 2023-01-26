using System.Net.WebSockets;

namespace RichillCapital.WebSockets
{
    public abstract class WebSocketMessage
    {
        public WebSocketMessageType MessageType { get; set; }
    }
}
