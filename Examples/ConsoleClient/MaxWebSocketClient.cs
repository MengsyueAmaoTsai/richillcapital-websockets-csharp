using System.Net.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RichillCapital.WebSockets;


namespace ConsoleClient
{
    public class MaxWebSocketClient
    {
        private IWebSocketClient _websocketClient;

        public MaxWebSocketClient()
        {
            string url = "wss://max-stream.maicoin.com/ws";
            _websocketClient = new WebSocketClient(url);
            _websocketClient.OnOpen += _websocketClient_OnOpen;
            _websocketClient.OnClose += _websocketClient_OnClose;
            _websocketClient.OnError += _websocketClient_OnError;
            _websocketClient.OnMessage += _websocketClient_OnMessage;
        }

        public void Connect()
        {
            _websocketClient.Connect();
        }

        public void SubscribeTrade(string market)
        {
            JArray subscriptions = new JArray
            {
                new JObject
                {
                    { "channel", "trade" },
                    { "market", market }
                },
            };

            JObject jsonData = new JObject
            {
                { "id", "websocketTest" },
                { "action", "sub" },
                { "subscriptions", subscriptions }
            };
            Send(jsonData);
        }

        private void Send(JObject json) => _websocketClient.Send(JsonConvert.SerializeObject(json));
        
        private void _websocketClient_OnMessage(object? sender, RichillCapital.WebSockets.Events.WebSocketMessageEventArgs e)
        {
            if (e.Data.MessageType == WebSocketMessageType.Text)
            {
            }
            else if (e.Data.MessageType == System.Net.WebSockets.WebSocketMessageType.Binary)
            {
            }
        }

        private void ProcessBinaryMessage(BinaryMessage message)
        {
            Console.WriteLine($"ProcessBinaryMessage => {message.Data}");
        }

        private void _websocketClient_OnError(object? sender, RichillCapital.WebSockets.Events.WebSocketErrorEventArgs e)
        {
            Console.WriteLine($"On error -> Exception: {e.Exception} Message: {e.Message}");
        }

        private void _websocketClient_OnClose(object? sender, RichillCapital.WebSockets.Events.WebSocketCloseEventArgs e)
        {
            Console.WriteLine($"On Close -> reason: {e.Reason} code: {e.Code} wasClean: {e.WasClean}");
        }

        private void _websocketClient_OnOpen(object? sender, EventArgs e)
        {
            Console.WriteLine($"On opened");
        }
    }
}
