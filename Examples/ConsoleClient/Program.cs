using RichillCapital.WebSockets;

WebSocketClient ws = new WebSocketClient("wss://max-stream.maicoin.com/ws");
ws.OnOpen += Ws_OnOpen;
ws.OnClose += Ws_OnClose;
ws.OnError += Ws_OnError;
ws.OnMessage += Ws_OnMessage;

ws.Connect();


void Ws_OnMessage(object? sender, RichillCapital.WebSockets.Events.WebSocketMessageEventArgs e)
{
    Console.WriteLine($"On message -> WebSocket: {e.WebSocket} MessageType: {e.Data.MessageType}");
}

void Ws_OnError(object? sender, RichillCapital.WebSockets.Events.WebSocketErrorEventArgs e)
{
    Console.WriteLine($"On error -> Exception: {e.Exception} Message: {e.Message}");
}

void Ws_OnClose(object? sender, RichillCapital.WebSockets.Events.WebSocketCloseEventArgs e)
{
    Console.WriteLine($"On Close -> reason: {e.Reason} code: {e.Code} wasClean: {e.WasClean}");
}

void Ws_OnOpen(object? sender, EventArgs e)
{
    Console.WriteLine($"On opened");
}

Console.ReadKey();