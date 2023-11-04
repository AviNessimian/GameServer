namespace GameServer.Core.Models;

public class Player
{
    public string Id { get; set; }
    public string DeviceId { get; set; }
    public string ConnectionId { get; set; }
    public int Coins { get; set; }
}
