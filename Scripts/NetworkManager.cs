using Godot;

public partial class NetworkManager : Node
{
    [Export] public int Port = 8910;
    [Export] public int MaxPlayers = 4;

    private SceneMultiplayer _multiplayer = new SceneMultiplayer();

    public static NetworkManager Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        Multiplayer.MultiplayerPeer = _multiplayer;
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
    }

    public void HostGame()
    {
        var peer = new ENetMultiplayerPeer();
        peer.CreateServer(Port, MaxPlayers);
        _multiplayer.MultiplayerPeer = peer;
        GD.Print($"Hosting game on port {Port}");
    }

    public void JoinGame(string address)
    {
        var peer = new ENetMultiplayerPeer();
        peer.CreateClient(address, Port);
        _multiplayer.MultiplayerPeer = peer;
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
