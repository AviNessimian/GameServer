using GameServer.Core.Contracts;


namespace GameServer.Infrastructure.WebSockets;

public class WebSocketConn : IConnection
{
    private readonly WebSocket _webSocket;
    private readonly string _connectionId;
    private readonly WebSocketConnsOptions _options;
    private readonly Action<string, byte[]> _onReceive;

    public WebSocketConn(
        WebSocket webSocket,
        WebSocketConnsOptions options,
        Action<string, byte[]> onReceive)
    {
        _connectionId = Guid.NewGuid().ToString();
        _webSocket = webSocket;
        _options = options;
        _onReceive = onReceive;
    }

    public bool IsClosed => _webSocket.State == WebSocketState.Closed;

    public bool IsOpen => _webSocket.State == WebSocketState.Open;


    public string ConnectionId => _connectionId;

    public async Task ReceiveUntilClosed()
    {
        try
        {
            byte[] receivePayloadBuffer = new byte[_options.ReceiveBufferSize];
            WebSocketReceiveResult receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
            while (receiveResult.MessageType != WebSocketMessageType.Close)
            {
                var webSocketMessage = await ReceiveMessagePayloadAsync(receiveResult, receivePayloadBuffer);
                _onReceive.Invoke(ConnectionId, webSocketMessage);
                receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
            }

            await _webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);

        }
        catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
        { }

        async Task<byte[]> ReceiveMessagePayloadAsync(WebSocketReceiveResult webSocketReceiveResult, byte[] receivePayloadBuffer)
        {
            byte[] messagePayload = null;

            if (webSocketReceiveResult.EndOfMessage)
            {
                messagePayload = new byte[webSocketReceiveResult.Count];
                Array.Copy(receivePayloadBuffer, messagePayload, webSocketReceiveResult.Count);
            }
            else
            {
                using (var messagePayloadStream = new MemoryStream())
                {
                    messagePayloadStream.Write(receivePayloadBuffer, 0, webSocketReceiveResult.Count);
                    while (!webSocketReceiveResult.EndOfMessage)
                    {
                        webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                        messagePayloadStream.Write(receivePayloadBuffer, 0, webSocketReceiveResult.Count);
                    }

                    messagePayload = messagePayloadStream.ToArray();
                }
            }

            return messagePayload;
        }
    }

    public async Task Send(
        string message,
        CancellationToken cancellationToken = default)
    {
        var messageType = WebSocketMessageType.Text;
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        if (!IsOpen) return;

        if (_options.SendBufferSize < messageBytes.Length)
        {
            int messageOffset = 0;
            int messageBytesToSend = messageBytes.Length;

            while (messageBytesToSend > 0)
            {
                int messageSegmentSize = Math.Min(_options.SendBufferSize, messageBytesToSend);
                var messageSegment = new ArraySegment<byte>(messageBytes, messageOffset, messageSegmentSize);

                messageOffset += messageSegmentSize;
                messageBytesToSend -= messageSegmentSize;

                var endOfMessage = messageBytesToSend == 0;
                await _webSocket.SendAsync(
                    messageSegment,
                    messageType,
                    BuildMessageFlags(endOfMessage, _options.DisableCompression),
                    cancellationToken);
            }
        }
        else
        {
            await _webSocket.SendAsync(
                new ArraySegment<byte>(messageBytes, 0, messageBytes.Length),
                messageType,
                BuildMessageFlags(true, _options.DisableCompression), cancellationToken);
        }


        static WebSocketMessageFlags BuildMessageFlags(bool endOfMessage, bool disableCompression)
        {
            var messageFlags = WebSocketMessageFlags.None;
            if (endOfMessage) messageFlags |= WebSocketMessageFlags.EndOfMessage;
            if (disableCompression) messageFlags |= WebSocketMessageFlags.DisableCompression;
            return messageFlags;
        }
    }

    private WebSocketMessageFlags BuildMessageFlags(bool endOfMessage, object disableCompression)
    {
        throw new NotImplementedException();
    }
}
