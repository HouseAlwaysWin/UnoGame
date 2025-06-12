using Godot;
using System;

public partial class GameOverUI : Control
{
    public override void _Ready()
    {
        // Ensure this UI continues to receive input even when the
        // scene tree is paused after the game ends.
        // `ProcessMode` replaces `PauseMode` in Godot 4 to control
        // whether the node processes while the scene tree is paused.
        // Using `Always` allows the UI to respond regardless of the
        // tree's paused state.
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed && Visible)
        {
            // 點擊左鍵時關閉遊戲結束畫面
            GetTree().Paused = false; // 解除暫停
            NetworkManager.Instance.LeaveGame();
            GetTree().ChangeSceneToFile("res://Scenes/uno_main_menu.tscn");
        }
    }

}
