using Godot;
using System;

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
        CardImage = GetNode<Sprite2D>("CardImage");
        InitCard();
    }

    private void InitCard()
    {
        if (CardImage != null)
        {
            CardImage.Texture = GD.Load<Texture2D>($"res://Assets/Cards/{CardImgName}.png");
            Vector2 textureSize = CardImage.Texture.GetSize();
            CardImage.Scale = new Vector2(
                CardSize.X / textureSize.X,
                CardSize.Y / textureSize.Y
            );
        }
    }
    
    public static Card CreateCard(string cardImgName,CardColor cardColor,CardType cardType)
    {
        var cardScence = GD.Load<PackedScene>("res://Scenes/card.tscn");
        var newCard = cardScence.Instantiate<Card>();
        newCard.CardImgName = cardImgName;
        newCard.CardColor = cardColor;
        newCard.CardType = cardType;
        return newCard;
    }
  
}
