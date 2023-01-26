using RichillCapital.WebSockets.Events;

namespace RichillCapital.WebSockets
{
    public interface IWebSocketClient
    {
        event EventHandler OnOpen;
        event EventHandler<WebSocketCloseEventArgs> OnClose;
        event EventHandler<WebSocketErrorEventArgs> OnError;
        event EventHandler<WebSocketMessageEventArgs> OnMessage;

        void Connect();
        void Close();
        void Send(string data);
    }
}
