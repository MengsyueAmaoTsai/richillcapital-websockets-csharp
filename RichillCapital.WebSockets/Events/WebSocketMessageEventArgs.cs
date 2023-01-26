namespace RichillCapital.WebSockets.Events
{
    public class WebSocketMessageEventArgs : EventArgs
    {
        public WebSocketMessageEventArgs(IWebSocketClient websocket, WebSocketMessage data) 
        {
            WebSocket = websocket;
            Data = data;
        }

        public IWebSocketClient WebSocket { get; }
        public WebSocketMessage Data { get; }
    }
}
