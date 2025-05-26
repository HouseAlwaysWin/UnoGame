using Godot;
using System;
using GodotHelper;

public enum CardColor { Red, Blue, Green, Yellow, Wild }
public enum CardType { Number, Skip, Reverse, DrawTwo, Wild, WildDrawFour }
public partial class Card : Node2D
{
    [ExportGroup("Card Information")]
    [Export]
    public Vector2 CardSize = new Vector2(100,150);
    [Export]
    public CardType CardType;
    [Export]
    public CardColor CardColor;

    public string CardImgName;
    public Sprite2D CardImage;

    public override void _Ready()
    {
        // DebugHelper.WaitForDebugger();
        InitCard();
    }

    private void InitCard()
    {
        CardImage = GetNode<Sprite2D>("CardImage");
        var cardImgName = string.IsNullOrEmpty(CardImgName)? "deck" : CardImgName;
        if (CardImage != null)
        {
            CardImage.Texture = GD.Load<Texture2D>($"res://Assets/Cards/{cardImgName}.png");
            Vector2 textureSize = CardImage.Texture.GetSize();
            CardImage.Scale = new Vector2(
                CardSize.X / textureSize.X,
                CardSize.Y / textureSize.Y
            );
        }
    }

    public void SetUpCardInfo(string cardImgName,CardColor cardColor,CardType cardType )
    {
        if (CardImage != null)
        {
            CardImage.Texture = GD.Load<Texture2D>($"res://Assets/Cards/{cardImgName}.png");
            CardColor = cardColor;
            CardType = cardType;
        }
    }
    

  
}
