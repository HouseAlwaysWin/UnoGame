using Godot;
using System;

public partial class UnoMainMenu : Control
{
    public override void _Ready()
    {
        GetNode<Button>("VBoxContainer/StartButton").Pressed += OnStartPressed;
        GetNode<Button>("VBoxContainer/ExitButton").Pressed += OnExitPressed;
    }

    private void OnStartPressed()
    {
        // 載入主遊戲場景（你可以換成你的 GameScene.tscn）
        GetTree().ChangeSceneToFile("res://Scenes/main.tscn");
    }

    private void OnExitPressed()
    {
        GetTree().Quit();
    }
}
