using Godot;
using System;

public partial class PlayerUI : HBoxContainer
{
    public string PlayerId;
    public Label SeqNo;
    public TextureRect PlayerImg;
    public Label PlayerName;
    public override void _Ready()
    {
        SeqNo = GetNode<Label>("SeqNo");
        PlayerImg = GetNode<TextureRect>("PlayerImg");
        PlayerName = GetNode<Label>("PlayerName");
    }

    public void InitPlayerUI(string playerId, string seqNo, string avatar, string name)
    {
        PlayerId = playerId;
        SeqNo.Text = seqNo;
        PlayerImg.Texture = GD.Load<Texture2D>($"res://Assets/Avatars/{avatar}");
        PlayerName.Text = name;
        Name = name;

    }

}
