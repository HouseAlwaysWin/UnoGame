using Godot;
using System;

public partial class GameOverUI : Control
{
    public override void _Ready()
    {

    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed && Visible)
        {
            // 點擊左鍵時關閉遊戲結束畫面
            GetTree().Paused = false; // 解除暫停
            GetTree().ChangeSceneToFile("res://Scenes/uno_main_menu.tscn");
        }
    }

}
