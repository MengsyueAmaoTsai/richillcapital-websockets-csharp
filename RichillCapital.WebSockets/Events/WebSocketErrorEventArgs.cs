namespace RichillCapital.WebSockets.Events
{
    public class WebSocketErrorEventArgs : EventArgs
    {
        public WebSocketErrorEventArgs(string message, Exception exception) 
        { 
            Message = message;
            Exception = exception;
        }

        public string Message { get; }

        public Exception Exception { get; }
    }
}
