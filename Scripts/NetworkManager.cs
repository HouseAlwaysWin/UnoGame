using Godot;

public partial class NetworkManager : Node
{
    [Export] public int Port = 8910;
    [Export] public int MaxPlayers = 4;

    private ENetMultiplayerPeer _multiplayer;

    public static NetworkManager Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
    }

    public void HostGame()
    {
        _multiplayer = new ENetMultiplayerPeer();
        _multiplayer.CreateServer(Port, MaxPlayers);
        Multiplayer.MultiplayerPeer = _multiplayer;
        GD.Print($"Hosting game on port {Port}");
    }

    public void JoinGame(string address)
    {
        _multiplayer = new ENetMultiplayerPeer();
        _multiplayer.CreateClient(address, Port);
        Multiplayer.MultiplayerPeer = _multiplayer;
        GD.Print($"Joining {address}:{Port}");
    }

    private void OnPeerConnected(long id)
    {
        GD.Print($"Peer connected: {id}");
    }

    private void OnPeerDisconnected(long id)
    {
        GD.Print($"Peer disconnected: {id}");
    }
}
