namespace RichillCapital.WebSockets.Events
{
    public class WebSocketCloseEventArgs : EventArgs
    {
        public WebSocketCloseEventArgs(ushort code, string reason, bool wasClean) 
        {
            Code = code;
            Reason = reason;
            WasClean = wasClean;
        }

        public ushort Code { get; }
        public string Reason { get; }
        public bool WasClean { get; }
    }
}
