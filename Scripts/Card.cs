using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [Export] public int Number = -1;
    [Export] public bool IsInDeck = false;

    public Vector2 OriginalPosition;
    public string DropZonePath;
    public bool IsSelected = false;
    public int OriginalZIndex;


    private Tween _hoverTween;
    private bool _isDragging = false;
    private Vector2 _dragOffset;
    private Area2D _dropZone;
    private string CardImgName;
    private static int _globalZCounter = 1000;

    public override void _Ready()
    {
        // DebugHelper.WaitForDebugger();
        AddToGroup("card");
        if (!IsInDeck)
        {
            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
        }

        if (!string.IsNullOrWhiteSpace(DropZonePath))
        {
            _dropZone = (Area2D)GetNode(DropZonePath);
        }
    }

    public override void _Process(double delta)
    {
        if (_isDragging)
        {
            GlobalPosition = GetGlobalMousePosition() - _dragOffset;
        }
    }

    private void KillShakeTween()
    {
        if (_hoverTween != null && _hoverTween.IsRunning() || IsInDeck)
        {
            _hoverTween.Kill();
            RotationDegrees = 0f;
        }
    }

    private void OnMouseExited()
    {
        KillShakeTween();
    }

    private void OnMouseEntered()
    {
        if (IsInDeck) return;
        _hoverTween = CreateTween();
        _hoverTween.SetLoops();
        _hoverTween.TweenProperty(this, "rotation_degrees", 2f, 0.1).SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        _hoverTween.TweenProperty(this, "rotation_degrees", -2f, 0.2).SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        _hoverTween.TweenProperty(this, "rotation_degrees", 0f, 0.1).SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }


    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && !IsInDeck)
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
                else
                {
                    // 只有自己是目前拖曳者時才能放開
                    _isDragging = false;
                    HandleDrop();
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
        ZAsRelative = false; // 確保 ZIndex 是全局有效的
        OriginalZIndex = ZIndex;
        ZIndex = _globalZCounter++;
        RaiseToTop();
    }

    public void RaiseToTop()
    {
        var parent = GetParent();
        if (parent != null)
        {
            parent.RemoveChild(this);
            parent.AddChild(this); // 加回去會成為場景樹中最後一個 → 最上層
        }
    }

    public void ReturnToOriginalZ()
    {
        ZIndex = OriginalZIndex;
    }

    private void HandleDrop()
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

                if (dropZoneRect.HasPoint(GlobalPosition))
                {
                    IsInDeck = true;
                    Position = dropZonePos;
                    KillShakeTween();
                    GD.Print("Card dropped in valid zone");
                    return;
                }
            }
        }

        // 沒有放到正確區域：回原位、ZIndex 還原
        GD.Print("Card dropped outside zone, returning");
        ReturnToOriginalZ();
        var tween = CreateTween();
        tween.TweenProperty(this, "position", this.OriginalPosition, 0.2)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    private void ToggleSelection()
    {
        IsSelected = !IsSelected;
        Position = IsSelected ? OriginalPosition + new Vector2(0, -20) : OriginalPosition;
    }

    public bool IsPlayable(Card topCard)
    {
        if (CardColor == CardColor.Wild)
            return true;
        return CardColor == topCard.CardColor || CardType == topCard.CardType || Number == topCard.Number;
    }

    public void InstantiateCard(string cardImgName = "", CardColor? cardColor = null, CardType? cardType = null,
        string dropZonePath = null)
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
            DropZonePath = dropZonePath;
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