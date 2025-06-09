using Godot;
using System;
using System.Threading.Tasks;

public partial class WildColorSelector : Control
{
    public override void _Ready()
    {
        // Connect the button signals to the color selection method
        GetNode<Button>("PanelContainer/VBoxContainer/ButtonRed").Pressed += () => SelectColor(CardColor.Red);
        GetNode<Button>("PanelContainer/VBoxContainer/ButtonGreen").Pressed += () => SelectColor(CardColor.Green);
        GetNode<Button>("PanelContainer/VBoxContainer/ButtonBlue").Pressed += () => SelectColor(CardColor.Blue);
        GetNode<Button>("PanelContainer/VBoxContainer/ButtonYellow").Pressed += () => SelectColor(CardColor.Yellow);
    }
    private TaskCompletionSource<CardColor> _colorSelectedTcs;
    public Task<CardColor> ShowAndWait()
    {
        Visible = true;
        _colorSelectedTcs = new TaskCompletionSource<CardColor>();
        return _colorSelectedTcs.Task;
    }
    private void SelectColor(CardColor color)
    {
        Visible = false;
        _colorSelectedTcs?.TrySetResult(color);
    }
}
