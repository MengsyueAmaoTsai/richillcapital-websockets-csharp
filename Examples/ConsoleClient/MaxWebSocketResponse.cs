using Newtonsoft.Json;

namespace ConsoleClient
{
    public class MaxWebSocketResponse
    {
        [JsonProperty("e")]
        public MaxWebSocketEventType EventType { get; }


    }
}
