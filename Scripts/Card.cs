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
    [Export]
    public Sprite2D CardImage;
    private string CardImgName;
    
    public override void _Ready()
    {
        // DebugHelper.WaitForDebugger();
    }

    public void InstantiateCard(string cardImgName = "",CardColor? cardColor = null,CardType? cardType = null)
    {
        CardImage = GetNode<Sprite2D>("CardImage");
        var cardImg  = string.IsNullOrEmpty(cardImgName) ? "deck" : cardImgName;
        if (CardImage != null)
        {
            CardImgName = cardImg;
            CardImage.Texture = GD.Load<Texture2D>($"res://Assets/Cards/{cardImg}.png");
            CardColor = cardColor ?? CardColor.Red;
            CardType = cardType ?? CardType.Number;
            Vector2 textureSize = CardImage.Texture.GetSize();
            CardImage.Scale = new Vector2(
                CardSize.X / textureSize.X,
                CardSize.Y / textureSize.Y
            );
        }
    }

    // public void SetUpCardInfo(string cardImgName,CardColor cardColor,CardType cardType )
    // {
    //     if (CardImage != null)
    //     {
    //         CardImage.Texture = GD.Load<Texture2D>($"res://Assets/Cards/{cardImgName}.png");
    //         CardColor = cardColor;
    //         CardType = cardType;
    //     }
    // }
    

  
}
