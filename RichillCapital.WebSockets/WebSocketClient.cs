using System.Text;
using System.Net.WebSockets;
using RichillCapital.WebSockets.Events;

namespace RichillCapital.WebSockets
{
    public class WebSocketClient : IWebSocketClient
    {
        private const int ReceiveBufferSize = 8192;

        private string _url;
        private string _sessionToken;
        private CancellationTokenSource _cancellationTokenSource;
        private ClientWebSocket _client;
        private Task _connectTask;
        private object _connectLock = new object();
        private readonly object _locker = new object();

        public event EventHandler<WebSocketMessageEventArgs> OnMessage;

        public event EventHandler<WebSocketErrorEventArgs> OnError;

        public event EventHandler OnOpen;

        public event EventHandler<WebSocketCloseEventArgs> OnClose;

        public WebSocketClient(string url, string sessionToken = null)
        {
            _url = url;
            _sessionToken = sessionToken;
        }

        public void Send(string data)
        {
            lock (_locker)
            {
                var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
                _client.SendAsync(buffer, WebSocketMessageType.Text, true, _cancellationTokenSource.Token).Wait();
            }
        }

        public void Connect()
        {
            lock (_connectLock)
            {
                lock (_locker)
                {
                    if (_cancellationTokenSource == null)
                    {
                        _cancellationTokenSource = new CancellationTokenSource();

                        _client = null;

                        _connectTask = Task.Factory.StartNew(
                            () =>
                            {
                                Console.WriteLine($"WebSocketClientWrapper connection task started: {_url}");

                                try
                                {
                                    HandleConnection();
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"{e}: Error in WebSocketClientWrapper connection task: {_url}: ");
                                }

                                Console.WriteLine($"WebSocketClientWrapper connection task ended: {_url}");
                            },
                            _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    }
                }

                var count = 0;
                do
                {
                    // wait for _client to be not null, we need to release the '_locker' lock used by 'HandleConnection'
                    if (_client != null || _cancellationTokenSource.Token.WaitHandle.WaitOne(50))
                    {
                        break;
                    }
                }
                while (++count < 100);
            }
        }

        public void Close()
        {
            lock (_locker)
            {
                try
                {
                    try
                    {
                        _client?.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", _cancellationTokenSource.Token).Wait();
                    }
                    catch
                    {
                        // ignored
                    }
                    _cancellationTokenSource?.Cancel();
                    _connectTask?.Wait(TimeSpan.FromSeconds(5));
                    _cancellationTokenSource.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"WebSocketClientWrapper.Close({_url}): {e}");
                }
                _cancellationTokenSource = null;
            }

            if (_client != null)
            {
                RaiseCloseEvent(new WebSocketCloseEventArgs(0, string.Empty, true));
            }
        }

        public bool IsOpen => _client?.State == WebSocketState.Open;

        protected virtual void RaiseMessageEvent(WebSocketMessageEventArgs e)
        {
            OnMessage?.Invoke(this, e);
        }

        protected virtual void RaiseErrorEvent(WebSocketErrorEventArgs e)
        {
            Console.WriteLine($"{e.Exception} WebSocketClientWrapper.OnError(): (IsOpen:{IsOpen}, State:{_client.State}): {_url}: {e.Message}");
            OnError?.Invoke(this, e);
        }

        protected virtual void RaiseOpenEvent()
        {
            Console.WriteLine($"WebSocketClientWrapper.OnOpen(): Connection opened (IsOpen:{IsOpen}, State:{_client.State}): {_url}");
            OnOpen?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void RaiseCloseEvent(WebSocketCloseEventArgs e)
        {
            Console.WriteLine($"WebSocketClientWrapper.OnClose(): Connection closed (IsOpen:{IsOpen}, State:{_client.State}): {_url}");
            OnClose?.Invoke(this, e);
        }

        private void HandleConnection()
        {
            var receiveBuffer = new byte[ReceiveBufferSize];

            while (_cancellationTokenSource is { IsCancellationRequested: false })
            {
                Console.WriteLine($"WebSocketClientWrapper.HandleConnection({_url}): Connecting...");

                const int maximumWaitTimeOnError = 120 * 1000;
                const int minimumWaitTimeOnError = 2 * 1000;
                var waitTimeOnError = minimumWaitTimeOnError;
                using (var connectionCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token))
                {
                    try
                    {
                        lock (_locker)
                        {
                            _client?.Dispose();
                            _client = new ClientWebSocket();
                            if (_sessionToken != null)
                            {
                                _client.Options.SetRequestHeader("x-session-token", _sessionToken);
                            }
                            _client.ConnectAsync(new Uri(_url), connectionCts.Token).Wait();
                        }
                        RaiseOpenEvent();

                        while ((_client.State == WebSocketState.Open || _client.State == WebSocketState.CloseSent) &&
                            !connectionCts.IsCancellationRequested)
                        {
                            var messageData = ReceiveMessage(_client, connectionCts.Token, receiveBuffer);

                            if (messageData == null)
                            {
                                break;
                            }

                            // reset wait time
                            waitTimeOnError = minimumWaitTimeOnError;
                            RaiseMessageEvent(new WebSocketMessageEventArgs(this, messageData));
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (WebSocketException ex)
                    {
                        RaiseErrorEvent(new WebSocketErrorEventArgs(ex.Message, ex));
                        connectionCts.Token.WaitHandle.WaitOne(waitTimeOnError);

                        // increase wait time until a maximum value. This is useful during brokerage down times
                        waitTimeOnError += Math.Min(maximumWaitTimeOnError, waitTimeOnError);
                    }
                    catch (Exception ex)
                    {
                        RaiseErrorEvent(new WebSocketErrorEventArgs(ex.Message, ex));
                    }
                    connectionCts.Cancel();
                }
            }
        }

        private WebSocketMessage ReceiveMessage(WebSocket webSocket, CancellationToken ct, byte[] receiveBuffer, long maxSize = long.MaxValue)
        {
            var buffer = new ArraySegment<byte>(receiveBuffer);

            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;

                do
                {
                    result = webSocket.ReceiveAsync(buffer, ct).Result;
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                    if (ms.Length > maxSize)
                    {
                        throw new InvalidOperationException($"Maximum size of the message was exceeded: {_url}");
                    }
                }
                while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    return new BinaryMessage
                    {
                        Data = ms.ToArray(),
                        Count = result.Count,
                    };
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    return new TextMessage
                    {
                        Message = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length),
                    };
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"WebSocketClientWrapper.HandleConnection({_url}): WebSocketMessageType.Close - Data: {Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length)}");
                    return null;
                }
            }
            return null;
        }
    }
}
