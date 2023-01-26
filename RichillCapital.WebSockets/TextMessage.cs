using System.Net.WebSockets;


namespace RichillCapital.WebSockets
{
    public class TextMessage : WebSocketMessage 
    {
        public TextMessage()
        {
            MessageType = WebSocketMessageType.Text;
        }

        public string Message { get; set; }
    }
}
