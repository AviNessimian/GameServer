namespace GameServer.Infrastructure.WebSockets;

public class WebSocketConnsOptions
{
    public HashSet<string> AllowedOrigins { get; set; }
    public int SendBufferSize { get; set; }
    public int ReceiveBufferSize { get; set; }
    public bool DisableCompression { get; set; }

    public bool AllowedAllOrigins => AllowedOrigins is null ||  AllowedOrigins.Any() == false;

    public WebSocketConnsOptions()
    {
        SendBufferSize = 4 * 1024;
        ReceiveBufferSize = 4 * 1024;
    }
}
