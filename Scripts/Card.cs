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
    public bool IsInteractive = true; // 預設不可互動

    public Player PlayerHand;
    public Vector2 OriginalPosition;
    public string DropZonePath;
    public bool IsSelected = false;
    public int OriginalZIndex;


    private Tween _tween;
    public bool IsTweenRunning { get => _tween?.IsRunning() ?? false; }
    private bool _isDragging = false;
    private Vector2 _dragOffset;
    private Area2D _dropZone;
    private Node2D _dropZoneNode;
    private string CardImgName;
    private static int _globalZCounter = 1000;
    private GameManager _gameManager;
    private bool _isHovered = false;

    public override void _Ready()
    {
        // DebugHelper.WaitForDebugger();
        _gameManager = GetParent().GetParent<GameManager>();
        AddToGroup("card");
        // if (IsInteractive)
        // {
        //     MouseEntered += OnMouseEntered;
        //     MouseExited += OnMouseExited;
        // }


        if (!string.IsNullOrWhiteSpace(DropZonePath))
        {
            _dropZoneNode = GetNode<Node2D>(DropZonePath);
            _dropZone = _dropZoneNode.GetNode<Area2D>("Area2D");
        }

        OriginalZIndex = ZIndex;
    }

    public override void _Process(double delta)
    {
        if (_isDragging)
        {
            GlobalPosition = GetGlobalMousePosition() - _dragOffset;
        }


        if (IsTweenRunning || _isDragging) return;
        if (IsInteractive && IsTopZIndexUnderMouse())
        {
            if (!_isHovered)
            {
                ShowUpCardTween();
                _isHovered = true;
            }
        }
        else
        {
            if (_isHovered)
            {
                ShowDownCardTween();
                _isHovered = false;
            }
        }
    }


    // private void OnMouseExited()
    // {
    //     if (_isDragging) return;
    //     ShowDownCardTween();
    // }

    // private void OnMouseEntered()
    // {
    //     if (!IsTopZIndexUnderMouse() || IsTweenRunning || _isDragging) return;
    //     ShowUpCardTween();
    // }

    private async Task ShakeTween()
    {
        _tween = CreateTween();
        _tween.SetLoops();
        _tween.TweenProperty(this, "rotation_degrees", 2f, 0.1).SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        _tween.TweenProperty(this, "rotation_degrees", -2f, 0.2).SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        _tween.TweenProperty(this, "rotation_degrees", 0f, 0.1).SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        await ToSignal(_tween, "finished");
    }

    private void KillShakeTween()
    {
        if (_tween != null || !IsInteractive)
        {
            if (_tween.IsRunning())
            {
                _tween.Kill();
            }

            RotationDegrees = 0f;
        }
    }

    private async void ShowUpCardTween()
    {
        if (!IsInteractive) return;
        _tween?.Kill(); // 若還有舊 Tween，先取消
        _tween = CreateTween();
        _tween.TweenProperty(this, "global_position", OriginalPosition + new Vector2(0, -50), 0.15)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        await ToSignal(_tween, "finished");
    }

    private async void ShowDownCardTween()
    {
        if (!IsInteractive) return;
        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenProperty(this, "global_position", OriginalPosition, 0.15)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        await ToSignal(_tween, "finished");
        ReturnToOriginalZ();
    }


    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && IsInteractive)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    if (!IsTopZIndexUnderMouse())
                        return; // 若不是最上層卡，不接受拖曳
                    _dragOffset = GetGlobalMousePosition() - GlobalPosition;
                    _isDragging = true;
                    SetAlwaysOnTop();
                }
                else if (!IsTweenRunning && IsTopZIndexUnderMouse())
                {
                    // 只有自己是目前拖曳者時才能放開
                    _isDragging = false;
                    HandleDrop();
                }
                else
                {
                    ReturnToOriginalZ();
                }
            }
        }
    }


    private bool IsTopZIndexUnderMouse()
    {
        // 獲取滑鼠目前位置下所有碰到的 Area2D（使用 DirectSpaceState）
        var space = GetWorld2D().DirectSpaceState;
        // 滑鼠目前位置下有哪些 Area2D 被碰到
        var result = space.IntersectPoint(new PhysicsPointQueryParameters2D
        {
            Position = GetGlobalMousePosition(),
            CollideWithAreas = true
        });
        // 準備找出 ZIndex 最大的卡片
        int maxZ = int.MinValue;
        Card topCard = null;

        foreach (var r in result)
        {
            var area = r["collider"].As<Area2D>();
            // 過濾掉不是卡片的物件，並確認是否是我們的 Card 類別
            if (area != null && area.IsInGroup("card") && area is Card c)
            {
                // 如果這張卡片的 ZIndex 更高，就記住它
                if (c.ZIndex > maxZ)
                {
                    maxZ = c.ZIndex;
                    topCard = c;
                }
            }
        }

        // 最後回傳：如果滑鼠下最高 ZIndex 的卡片是我自己，才允許拖曳
        return topCard == this;
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
                    KillShakeTween();

                    // Vector2 globalPos = dropZonePos;
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
        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenProperty(this, "global_position", this.OriginalPosition, 0.2)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        await ToSignal(_tween, "finished");
        ReturnToOriginalZ();
        await Task.Delay(100); // 微延遲防止立即觸發 Hover
        IsInteractive = true;
    }

    private void ToggleSelection()
    {
        IsSelected = !IsSelected;
        Position = IsSelected ? OriginalPosition + new Vector2(0, -20) : OriginalPosition;
    }

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