using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GodotHelper;
using System.Threading.Tasks;

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
    [ExportGroup("Card Information")]
    [Export]
    public Vector2 CardSize = new Vector2(100, 150);

    [Export] public CardType CardType;
    [Export] public CardColor CardColor;
    [Export] public Sprite2D CardImage;

    [Export] public int Number = -1;

    // [Export] public bool IsInDeck = false;
    public bool IsInteractive = false; // 預設不可互動

    public Player PlayerHand;
    public Vector2 OriginalPosition;
    public string DropZonePath;
    public bool IsSelected = false;
    public int OriginalZIndex;

    private Tween _tween;

    public bool IsTweenRunning
    {
        get => _tween?.IsRunning() ?? false;
    }

    public bool IsDragging = false;
    private Vector2 _dragOffset;
    private Area2D _dropZone;
    private Node2D _dropZoneNode;
    private string CardImgName;
    private static int _globalZCounter = 1000;
    private GameManager _gameManager;
    private bool _isHovered = false;
    private CardAnimator _animator;

    public Vector2 DragOffset { get; private set; }


    public override void _Ready()
    {
        // DebugHelper.WaitForDebugger();

        AddToGroup("card");
        _gameManager = GetParent().GetParent<GameManager>();
        _animator = GetNode<CardAnimator>("CardAnimator");

        if (!string.IsNullOrWhiteSpace(DropZonePath))
        {
            _dropZoneNode = GetNode<Node2D>(DropZonePath);
            _dropZone = _dropZoneNode.GetNode<Area2D>("Area2D");
        }

        OriginalZIndex = ZIndex;
    }

    public override void _Process(double delta)
    {
        if (IsDragging)
        {
            GlobalPosition = GetGlobalMousePosition() - _dragOffset;
        }
    }

    public async void OnHoverEnter()
    {
        if (_isHovered || IsDragging || !IsInteractive || IsTweenRunning) return;
        _isHovered = true;
        await _animator.HoverUp();
    }

    public async void OnHoverExit()
    {
        if (!_isHovered || IsDragging || !IsInteractive) return;
        _isHovered = false;
        await _animator.HoverDown();
    }

    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (!IsInteractive)
            return;

        if (@event is InputEventMouseButton mouseEvent && IsInteractive)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    // ❗確保只有當前滑鼠下最上層的卡能觸發拖曳
                    if (_gameManager.GetCardUnderMouse() != this)
                        return;
                    _dragOffset = GetGlobalMousePosition() - GlobalPosition;
                    IsDragging = true;
                    _gameManager.SetDraggedCard(this);
                    SetAlwaysOnTop();
                }
                else if (!IsTweenRunning && _gameManager.GetCardUnderMouse() != this)
                {
                    // 只有自己是目前拖曳者時才能放開
                    IsDragging = false;
                    _gameManager.ClearDraggedCard(this);
                    HandleDrop();
                }
                else
                {
                    ReturnToOriginalZ();
                }
            }
        }
    }



    public void SetAlwaysOnTop()
    {
        if (!IsInteractive) return;
        ZAsRelative = false; // 確保 ZIndex 是全局有效的
        ZIndex = _globalZCounter++;
    }

    public void ReturnToOriginalZ()
    {
        ZIndex = OriginalZIndex;
    }

    public Card GetTopCardInDropZone(Node dropZoneNode)
    {
        Card topCard = null;
        int maxZ = int.MinValue;

        foreach (var child in dropZoneNode.GetChildren())
        {
            if (child is Card card)
            {
                if (card.ZIndex > maxZ)
                {
                    maxZ = card.ZIndex;
                    topCard = card;
                }
            }
        }

        return topCard;
    }

    private async void HandleDrop()
    {
        if (_dropZone != null)
        {
            var shapeNode = _dropZone.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            if (shapeNode != null && shapeNode.Shape is RectangleShape2D rectShape)
            {
                var dropZonePos = _dropZone.GlobalPosition;
                var dropSize = rectShape.Size;
                var halfSize = dropSize / 2;

                var dropZoneRect = new Rect2(dropZonePos - halfSize, dropSize);
                var dropZoneTopCard = GetTopCardInDropZone(_dropZoneNode);

                if (dropZoneRect.HasPoint(GlobalPosition) && dropZoneTopCard.IsPlayable(this))
                {
                    IsInteractive = false;

                    PlayerHand.RemoveCard(this);
                    _dropZoneNode.AddChild(this);
                    Position = _dropZoneNode.ToLocal(dropZonePos);
                    await PlayerHand.ReorderHand();

                    GD.Print("Card dropped in valid zone");
                    return;
                }
            }
        }

        // 沒有放到正確區域：回原位、ZIndex 還原
        GD.Print("Card dropped outside zone, returning");
        await _animator.TweenTo(this.OriginalPosition, 0.2f);
        ReturnToOriginalZ();
        await Task.Delay(100); // 微延遲防止立即觸發 Hover
        IsInteractive = true;
    }

    // private void ToggleSelection()
    // {
    //     IsSelected = !IsSelected;
    //     Position = IsSelected ? OriginalPosition + new Vector2(0, -20) : OriginalPosition;
    // }

    public bool IsPlayable(Card topCard)
    {
        if (CardColor == CardColor.Wild || topCard.CardColor == CardColor.Wild)
            return true;
        return CardColor == topCard.CardColor || (CardType == topCard.CardType && Number == topCard.Number);
    }

    public void InstantiateCard(Player playerHand, string cardImgName = "", CardColor? cardColor = null,
        CardType? cardType = null,
        string dropZonePath = null, int cardNumber = -1)
    {
        PlayerHand = playerHand;
        CardImage = GetNode<Sprite2D>("CardImage");
        var cardImg = string.IsNullOrEmpty(cardImgName) ? "deck" : cardImgName;
        if (CardImage != null)
        {
            CardImgName = cardImg;
            CardImage.Texture = GD.Load<Texture2D>($"res://Assets/Cards/{cardImg}.png");
            CardColor = cardColor ?? CardColor.Red;
            CardType = cardType ?? CardType.Number;
            Number = cardNumber;
            Vector2 textureSize = CardImage.Texture.GetSize();
            CardImage.Scale = new Vector2(
                CardSize.X / textureSize.X,
                CardSize.Y / textureSize.Y
            );
            DropZonePath = dropZonePath;
        }
    }
}