using Godot;
using System;
using GodotHelper;

public enum CardColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Wild
}

public enum CardType
{
    Number,
    Skip,
    Reverse,
    DrawTwo,
    Wild,
    WildDrawFour
}

public partial class Card : Area2D
{
    [ExportGroup("Card Information")] [Export]
    public Vector2 CardSize = new Vector2(100, 150);

    [Export] public CardType CardType;
    [Export] public CardColor CardColor;
    [Export] public Sprite2D CardImage;
    [Export] public int Number  = -1;
    [Export] public bool IsInDeck  = false;
    
    private string CardImgName;
    public Vector2 OriginalPosition;
    private bool _isSelected = false;
    private Tween _hoverTween;
    
    public override void _Ready()
    {
        // DebugHelper.WaitForDebugger();
        if (!IsInDeck)
        {
            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
        }
    }

    private void OnMouseExited()
    {
        if (_hoverTween != null && _hoverTween.IsRunning())
        {
            _hoverTween.Kill();
        }
        RotationDegrees = 0f;
    }

    private void OnMouseEntered()
    {
        _hoverTween = CreateTween();
        _hoverTween.SetLoops();
        _hoverTween.TweenProperty(this, "rotation_degrees", 2f, 0.1).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        _hoverTween.TweenProperty(this, "rotation_degrees", -2f, 0.2).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        _hoverTween.TweenProperty(this, "rotation_degrees", 0f, 0.1).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
    }


    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left && !IsInDeck)
        {
            ToggleSelection();
        }
    }
    
    private void ToggleSelection()
    {
        _isSelected = !_isSelected;
        Position = _isSelected ? OriginalPosition + new Vector2(0, -20) : OriginalPosition;
    }

    public bool IsPlayable(Card topCard)
    {
        if (CardColor == CardColor.Wild)
            return true;
        return CardColor == topCard.CardColor || CardType == topCard.CardType || Number == topCard.Number;
        
    }

    public void InstantiateCard(string cardImgName = "", CardColor? cardColor = null, CardType? cardType = null)
    {
        CardImage = GetNode<Sprite2D>("CardImage");
        var cardImg = string.IsNullOrEmpty(cardImgName) ? "deck" : cardImgName;
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