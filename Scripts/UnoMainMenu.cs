using Godot;
using System;

public partial class UnoMainMenu : Control
{
    public override void _Ready()
    {
        GetNode<Button>("VBoxContainer/StartButton").Pressed += OnStartPressed;
        GetNode<Button>("VBoxContainer/HostButton").Pressed += OnHostPressed;
        GetNode<Button>("VBoxContainer/JoinButton").Pressed += OnJoinPressed;
        GetNode<Button>("VBoxContainer/ExitButton").Pressed += OnExitPressed;
        _addressEdit = GetNode<LineEdit>("VBoxContainer/AddressEdit");
    }

    private LineEdit _addressEdit;

    private void OnStartPressed()
    {
        // 載入主遊戲場景（你可以換成你的 GameScene.tscn）
        GetTree().ChangeSceneToFile("res://Scenes/main.tscn");
    }

    private void OnHostPressed()
    {
        NetworkManager.Instance.HostGame();
        GetTree().ChangeSceneToFile("res://Scenes/main.tscn");
    }

    private void OnJoinPressed()
    {
        var addr = _addressEdit.Text;
        if (string.IsNullOrWhiteSpace(addr))
            addr = "127.0.0.1";
        NetworkManager.Instance.JoinGame(addr);
        GetTree().ChangeSceneToFile("res://Scenes/main.tscn");
    }

    private void OnExitPressed()
    {
        NetworkManager.Instance.LeaveGame();
        GetTree().Quit();
    }
}
