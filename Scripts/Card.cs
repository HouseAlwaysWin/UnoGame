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

    private StyleBoxFlat _originalStyleBox;

    // [Export] public bool IsInDeck = false;
    public bool IsInteractive = false; // 預設不可互動

    // public Player PlayerHand;
    public Vector2 OriginalPosition;

    // public string DropZonePath;
    public bool IsSelected = false;
    public int OriginalZIndex;

    private Tween _tween;

    [Signal]
    public delegate void DragStartedSignalEventHandler(Card card);

    [Signal]
    public delegate void DragEndedSignalEventHandler(Card card);

    public bool IsTweenRunning
    {
        get => _tween?.IsRunning() ?? false;
    }

    public bool IsDragging = false;
    private Vector2 _dragOffset;
    private Area2D _dropZoneArea;
    private Node2D _dropZoneNode;
    private string CardImgName;
    private static int _globalZCounter = 200;
    private GameManager _gameManager;
    private bool _isHovered = false;
    public CardAnimator CardAnimator;

    public Vector2 DragOffset { get; private set; }

    public override void _Ready()
    {
        // DebugHelper.WaitForDebugger();
        AddToGroup("card");
        _gameManager = GetParent().GetParent<GameManager>();
        CardAnimator = GetNode<CardAnimator>("CardAnimator");
        _dropZoneNode = _gameManager.DropZonePileNode;
        _dropZoneArea = _gameManager.DropZoneArea;
        OriginalZIndex = ZIndex;
        _originalStyleBox = GetNode<Panel>("Border").GetThemeStylebox("panel")?.Duplicate() as StyleBoxFlat;
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
        await CardAnimator.HoverUp();
    }

    public async void OnHoverExit()
    {
        if (!_isHovered || IsDragging || !IsInteractive) return;
        _isHovered = false;
        await CardAnimator.HoverDown();
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
                    // // ❗確保只有當前滑鼠下最上層的卡能觸發拖曳
                    if (_gameManager.GetCardUnderMouse() != this)
                        return;
                    _dragOffset = GetGlobalMousePosition() - GlobalPosition;
                    IsDragging = true;
                    EmitSignal(nameof(DragStartedSignal), this);
                    SetAlwaysOnTop();
                }
                else if (!IsTweenRunning && IsDragging)
                {
                    // 只有自己是目前拖曳者時才能放開
                    IsDragging = false;
                    EmitSignal(nameof(DragEndedSignal), this);

                }
            }
        }
    }

    public void SetAlwaysOnTop()
    {
        ZAsRelative = false; // 確保 ZIndex 是全局有效的
        ZIndex = _globalZCounter++;
    }

    public void ReturnToOriginalZ()
    {
        ZIndex = OriginalZIndex;
    }

    public void ResetBorder()
    {
        var border = GetNode<Panel>("Border");
        if (_originalStyleBox != null)
            border.AddThemeStyleboxOverride("panel", _originalStyleBox);
        border.Visible = true;
    }

    public void SetWildColor(CardColor newColor)
    {
        CardColor = newColor;

        // 取得 Border 節點
        var border = GetNode<Control>("Border");

        // 設定顏色
        var color = Card.GetColorFromEnum(newColor);

        // 改變 Border 顏色（根據型別）
        if (border is ColorRect cr)
            cr.Color = color;
        else if (border is Panel panel)
        {
            int borderWidth = 100;
            int radius = 12;
            var style = new StyleBoxFlat
            {
                BorderWidthBottom = borderWidth,
                BorderWidthTop = borderWidth,
                BorderWidthLeft = borderWidth,
                BorderWidthRight = borderWidth,
                BorderColor = color,
                BgColor = new Color(0, 0, 0, 0)
            };

            style.CornerRadiusTopLeft = radius;
            style.CornerRadiusTopRight = radius;
            style.CornerRadiusBottomLeft = radius;
            style.CornerRadiusBottomRight = radius;
            panel.AddThemeStyleboxOverride("panel", style);
        }
        else if (border is TextureRect tr)
            tr.Modulate = color;

        border.Visible = true; // 確保顯示
    }

    public static Color GetColorFromEnum(CardColor color)
    {
        return color switch
        {
            CardColor.Red => Colors.Red,
            CardColor.Yellow => Colors.Yellow,
            CardColor.Blue => Colors.Blue,
            CardColor.Green => Colors.Green,
            _ => Colors.White
        };
    }



    public void InstantiateCard(string cardImgName = "", CardColor? cardColor = null,
        CardType? cardType = null, int cardNumber = -1)
    {
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
        }
    }
}