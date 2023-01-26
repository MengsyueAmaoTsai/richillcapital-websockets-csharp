using ConsoleClient;

MaxWebSocketClient ws = new MaxWebSocketClient();
ws.Connect();

Thread.Sleep(2000);
string market = "btctwd";

ws.SubscribeTrade(market);

Console.ReadKey();