using Godot;
using System;

public partial class Mask : ColorRect
{
    public override void _Ready()
    {
        // Connect GUI 輸入事件
        MouseFilter = MouseFilterEnum.Stop;
    }

    private void OnGuiInput(InputEvent @event)
    {
        // 把這個事件吃掉，不再往下傳
        AcceptEvent();
    }
}
