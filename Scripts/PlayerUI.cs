using Godot;
using System;

public partial class PlayerUI : HBoxContainer
{
    public string PlayerId;
    public Player Player;
    public Label SeqNo;
    public TextureRect PlayerImg;
    public Label PlayerName;
    public Label HandCardNumber;

    public override void _Ready()
    {
        SeqNo = GetNode<Label>("SeqNo");
        PlayerImg = GetNode<TextureRect>("PlayerImg");
        PlayerName = GetNode<Label>("PlayerName");
        HandCardNumber = GetNode<Label>("HandCardNumber");
    }

    public void SetPlayerHandCardNumber()
    {
        HandCardNumber.Text = $"({Player.GetPlayerHandCards().Count})";
    }

    public void InitPlayerUI(Player player, string seqNo, string avatar, string name)
    {
        // PlayerId = playerId;
        Player = player;
        SeqNo.Text = seqNo;
        PlayerImg.Texture = GD.Load<Texture2D>($"res://Assets/Avatars/{avatar}");
        PlayerName.Text = name;
        Name = name;
    }
}