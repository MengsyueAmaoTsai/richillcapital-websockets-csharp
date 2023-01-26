using System.Net.WebSockets;


namespace RichillCapital.WebSockets
{
    public class BinaryMessage : WebSocketMessage
    {
        public BinaryMessage()
        {
            MessageType = WebSocketMessageType.Binary;
        }
        
        public byte[] Data { get; set; }

        public int Count { get; set; }
    }
}
