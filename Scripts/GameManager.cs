using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GodotHelper;
using System.Linq;

public partial class GameManager : Node2D
{
    [Export] public int CardsToDeal = 7;
    [Export] public float CardSpacing = 50;
    [Export] public float MaxCardSpacing = 200;

    [Export] public int ComPlayerNumber = 3;

    public List<Card> _deck = new();
    private Node2D _deckPileNode;
    private Player _playerHand;
    private Button _playButton;
    private Button _playButton2;
    private Area2D _dropZone;
    private Node2D _dropZonePileNode;
    private Node2D _otherPlayer;
    private VBoxContainer _playerInfoPanel;
    private List<Label> _playerInfoPanelLabels = new();
    private List<Label> _playerSeqPanelLabels = new();
    private int _currentPlayerIndex = 0;
    private List<Player> _players = new();
    private bool _isClockwise = true; // true: 順時針, false: 逆時針

    private Node2D _directionArrow;
    private Tween _directionTween;
    private float _arrowRotation = 0f;
    private Card _lastHoveredCard;

    private Card _currentTopCard; // 新增
    private Card _hoveredCard = null;
    private Card _draggedCard = null;



    public override async void _Ready()
    {
        // DebugHelper.WaitForDebugger();

        _otherPlayer = GetNode<Node2D>("OtherPlayer");
        _deckPileNode = GetNode<Node2D>("DeckPile"); // 在主場景加一個 DeckPile 節點
        _playerHand = GetNode<Player>("PlayerHand");
        _playButton = GetNode<Button>("PlayButton");
        _playButton.Pressed += onPlayButtonPressed;

        _playButton2 = GetNode<Button>("PlayButton2");
        _playButton2.Pressed += onPlayButtonPressed2;


        _directionArrow = GetNode<Node2D>("DirectionArrow");
        _directionArrow.RotationDegrees = 90;
        _arrowRotation = 90;

        _dropZone = GetNode<Area2D>("DropZonePile/Area2D");
        _dropZonePileNode = GetNode<Node2D>("DropZonePile");

        _playerInfoPanel = GetNode<VBoxContainer>("UI/PlayerInfoPanel");

        InitDeck();
        DisplayDeckPile();
        ShuffleDeck();

        await InitPlayerAndUIAsync();
        await DealBeginCard();
        await DealingCardsToPlayerAsync(_playerHand, 7);
    }

    public override void _Process(double delta)
    {
        if (_draggedCard == null)
        {
            UpdateHoveredCard();
        }
        // else
        // {
        //     UpdateDraggedCard();
        // }
    }

    private async Task DealBeginCard()
    {
        Card card = _deck[0];
        _deck.RemoveAt(0);
        await MoveCardToTarget(card, _deckPileNode, _dropZonePileNode);
    }


    private void onPlayButtonPressed2()
    {
        ReverseDirection();
    }

    public async Task ReverseDirection()
    {
        _isClockwise = !_isClockwise;

        // 每次翻轉加 180 度
        _arrowRotation += 180f;

        _directionTween?.Kill();
        _directionTween = CreateTween();
        _directionTween.TweenProperty(_directionArrow, "rotation_degrees", _arrowRotation, 0.5)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        GD.Print($"方向翻轉：{(_isClockwise ? "順時針" : "逆時針")}");
    }

    public async Task InitPlayerAndUIAsync(int? playerNumber = null)
    {
        int comPlayerNumber = playerNumber ?? ComPlayerNumber;

        // 1. 加入玩家（含本地玩家與 COM）
        _players.Clear();
        _players.Add(_playerHand);

        for (int i = 1; i < comPlayerNumber + 1; i++)
        {
            var playerScence = GD.Load<PackedScene>("res://Scenes/player.tscn");
            var newPlayer = playerScence.Instantiate<Player>();
            newPlayer.PlayerId = i;
            newPlayer.Name = $"COM Player {i + 1}";
            AddChild(newPlayer);
            await DealingCardsToPlayerAsync(newPlayer, 7);
            _players.Add(newPlayer);
        }

        // 2. 打亂 _players 順序（遊戲邏輯用）
        var rng = new Random();
        _players = _players.OrderBy(_ => rng.Next()).ToList();

        // 3. 清空 UI 面板
        _playerInfoPanelLabels.Clear();
        _playerSeqPanelLabels.Clear();
        foreach (var child in _playerInfoPanel.GetChildren())
            _playerInfoPanel.RemoveChild(child);

        // 4. 建立 UI（依照打亂順序）
        for (int i = 0; i < _players.Count; i++)
        {
            CreatedPlayerUI(_players[i], i + 1); // 傳入玩家 + 頭像編號
        }

        // 5. 顯示目前玩家
        _currentPlayerIndex = 0;
        UpdateCurrentPlayerUI();
    }


    public void CreatedPlayerUI(Player player, int number)
    {
        var hbox = new HBoxContainer();

        var avatar = new TextureRect
        {
            Texture = GD.Load<Texture2D>($"res://Assets/Avatars/avatar{number}.jpeg"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            CustomMinimumSize = new Vector2(50, 50),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };

        var seqLabel = new Label
        {
            Text = "1",
            Name = player.Name

        };

        var label = new Label
        {
            Text = player.Name,
            Name = player.Name
        };
        hbox.AddChild(seqLabel);
        hbox.AddChild(avatar);
        hbox.AddChild(label);
        _playerSeqPanelLabels.Add(seqLabel);
        _playerInfoPanel.AddChild(hbox);
        _playerInfoPanelLabels.Add(label);
    }


    public void NextTurn()
    {
        if (_isClockwise)
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }
        else
        {
            _currentPlayerIndex = (_currentPlayerIndex - 1 + _players.Count) % _players.Count;
        }

        UpdateCurrentPlayerUI();
    }

    public void UpdateCurrentPlayerUI()
    {
        for (int i = 0; i < _playerInfoPanelLabels.Count; i++)
        {
            _playerSeqPanelLabels[i].Text = $"{i + 1}.";
            _playerInfoPanelLabels[i]
                .AddThemeColorOverride("font_color", i == _currentPlayerIndex ? Colors.Yellow : Colors.White);
        }
    }

    private void UpdateHoveredCard()
    {
        var space = GetWorld2D().DirectSpaceState;
        var result = space.IntersectPoint(new PhysicsPointQueryParameters2D
        {
            Position = GetGlobalMousePosition(),
            CollideWithAreas = true
        });

        Card topCard = null;
        int maxZ = int.MinValue;

        foreach (var r in result)
        {
            var area = r["collider"].As<Area2D>();
            if (area != null && area.IsInGroup("card") && area is Card c && c.IsInteractive)
            {
                if (c.ZIndex > maxZ || (c.ZIndex == maxZ && DistanceToMouse(c) < DistanceToMouse(topCard)))
                {
                    maxZ = c.ZIndex;
                    topCard = c;
                }
            }
        }

        if (_hoveredCard != topCard)
        {
            _hoveredCard?.OnHoverExit();
            topCard?.OnHoverEnter();
            _hoveredCard = topCard;
        }
    }

    private float DistanceToMouse(Card card)
    {
        return card.GlobalPosition.DistanceTo(GetGlobalMousePosition());
    }

    private void UpdateDraggedCard()
    {
        if (_draggedCard != null)
        {
            _draggedCard.GlobalPosition = GetGlobalMousePosition() - _draggedCard.DragOffset;
        }
    }

    public Card GetCardUnderMouse()
    {
        return _hoveredCard;
    }

    public void SetDraggedCard(Card card)
    {
        _draggedCard = card;
        _hoveredCard?.OnHoverExit(); // 停止 hover 狀態
        _hoveredCard = null;
    }

    public void ClearDraggedCard(Card card)
    {
        if (_draggedCard == card)
            _draggedCard = null;
    }

    public Card GetTopCardUnderMouse()
    {
        return _lastHoveredCard;
    }

    private async void onPlayButtonPressed()
    {
        GD.Print(_deck.Count);
        if (_deck.Count > 7)
        {
            await DealingCardsToPlayerAsync();
            _playerHand.ReorderHand();
        }
    }


    private void InitDeck()
    {
        foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
        {
            if (color == CardColor.Wild) continue;
            for (int i = 0; i <= 9; i++)
            {
                _deck.Add(CreateCard($"{color.ToString().ToLower()}{i}", color, CardType.Number, i));
                if (i == 0) continue;
                _deck.Add(CreateCard($"{color.ToString().ToLower()}{i}", color, CardType.Number, i));
            }

            string reverseName = $"{color.ToString().ToLower()}Reverse";
            string skipName = $"{color.ToString().ToLower()}Skip";
            string drawTwoName = $"{color.ToString().ToLower()}DrawTwo";

            _deck.Add(CreateCard(reverseName, color, CardType.Reverse, 10));
            _deck.Add(CreateCard(reverseName, color, CardType.Reverse, 10));
            _deck.Add(CreateCard(skipName, color, CardType.Skip, 11));
            _deck.Add(CreateCard(skipName, color, CardType.Skip, 11));
            _deck.Add(CreateCard(drawTwoName, color, CardType.DrawTwo, 12));
            _deck.Add(CreateCard(drawTwoName, color, CardType.DrawTwo, 12));
        }

        for (int i = 0; i < 4; i++)
        {
            string wildName = $"{CardType.Wild.ToString().ToLower()}";
            string wildDrawFourName = $"{CardType.WildDrawFour.ToString().ToCamelCase()}";
            _deck.Add(CreateCard(wildName, CardColor.Wild, CardType.Wild, 13));
            _deck.Add(CreateCard(wildDrawFourName, CardColor.Wild, CardType.WildDrawFour, 14));
        }
    }

    private void DisplayDeckPile()
    {
        const int MaxStackSize = 10;
        const float OffsetStep = 3f;
        const float RotationStep = 0.5f;

        int count = Math.Min(_deck.Count, MaxStackSize);
        for (int i = 0; i < count; i++)
        {
            Card visualCard = CreateCard($"deck", CardColor.Wild, CardType.Wild); // 顯示用，不影響資料堆
            visualCard.Position = new Vector2(0, -i * OffsetStep); // 小小位移
            visualCard.ZAsRelative = false;
            visualCard.ZIndex = i;
            visualCard.RotationDegrees = GD.Randf() * RotationStep - (RotationStep / 2); // 微旋轉
            visualCard.IsInteractive = false;
            // visualCard.Modulate = new Color(1, 1, 1, 0.8f); // 透明度微低

            _deckPileNode.AddChild(visualCard);
        }
    }

    private void DisplayDeckPileWithRotation(Node2D parent, int count = 10)
    {
        const float radius = 10f;
        const float angleStep = 10f; // 每張牌角度間隔（degrees）

        for (int i = 0; i < count; i++)
        {
            var visualCard = CreateCard("deck", CardColor.Wild, CardType.Wild);
            visualCard.Position = Vector2.Zero;
            visualCard.RotationDegrees = -angleStep * (count / 2f) + i * angleStep; // -25 ~ +25
            visualCard.ZIndex = i;
            parent.AddChild(visualCard);
        }
    }

    private Card CreateCard(string cardImgName, CardColor cardColor, CardType cardType, int cardNumber = -1, int zindex = 0)
    {
        var cardScence = GD.Load<PackedScene>("res://Scenes/card.tscn");
        var newCard = cardScence.Instantiate<Card>();
        newCard.InstantiateCard(_playerHand, cardImgName, cardColor, cardType, _dropZonePileNode.GetPath(), cardNumber);
        newCard.Name = $"{cardColor}{cardType}{cardNumber}";
        newCard.ZIndex = zindex;
        return newCard;
    }

    private void ShuffleDeck()
    {
        Random random = new Random();
        for (int i = _deck.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
        }
        // 設定階層
        for (int i = 0; i < _deck.Count - 1; i++)
        {
            _deck[i].ZIndex = i;
        }
    }

    private async Task DealingCardsToPlayerAsync(Player? playerHand = null, int? dealNum = null)
    {
        Player player = playerHand ?? _playerHand;
        int cardsToDeal = dealNum ?? CardsToDeal;
        float spacing = CardSpacing;
        // float startX = -((cardsToDeal - 1) * spacing / 2f);
        Vector2 windowSize = DisplayServer.WindowGetSize();
        float startX = 0;

        if (_deck.Count < 5) cardsToDeal = _deck.Count;
        var maxCardSpacing = cardsToDeal * spacing;
        if (maxCardSpacing > MaxCardSpacing)
        {
            spacing = MaxCardSpacing / cardsToDeal;
        }

        for (int i = 0; i < cardsToDeal; i++)
        {
            if (_deck.Count > 0)
            {
                Card card = _deck[0];
                _deck.RemoveAt(0);

                // Card visualCard = CreateCard($"deck", CardColor.Wild, CardType.Wild); // 顯示用，不影響資料堆
                // GD.Print(card.CardImage.Texture.ResourcePath);
                // AddChild(card);

                player.AddCard(card);
                if (player != _playerHand)
                {
                    card.Hide();
                }
                else
                {
                    card.GlobalPosition = _deckPileNode.GlobalPosition;
                    card.IsInteractive = false;
                    Vector2 targetPos = player.GlobalPosition + new Vector2(startX + i * spacing, 0);
                    card.SetAlwaysOnTop();
                    var tween = CreateTween();
                    tween.TweenProperty(card, "global_position", targetPos, 0.5).SetTrans(Tween.TransitionType.Sine)
                        .SetEase(Tween.EaseType.Out);
                    await ToSignal(tween, "finished");
                    card.IsInteractive = true;
                    // 紀錄目前位置
                    card.OriginalPosition = targetPos;
                }
            }
        }
    }


    /// <summary>
    /// 通用卡片移動
    /// </summary>
    /// <param name="card"></param>
    /// <param name="fromNode"></param>
    /// <param name="toNode"></param>
    /// <param name="offset"></param>
    /// <param name="duration"></param>
    public async Task MoveCardToTarget(Card card, Node2D fromNode, Node2D toNode, Vector2 offset = default,
        float duration = 0.4f)
    {
        // 1. 紀錄卡牌目前的 global 位置
        // Vector2 fromGlobal = fromNode.GlobalPosition;

        // 2. 從來源節點移除，加到目的地節點
        if (card.GetParent() == fromNode)
            fromNode?.RemoveChild(card);
        toNode.AddChild(card);

        // 3. 將卡片位置設定為原本 global 位置（在新 parent 下）
        card.GlobalPosition = fromNode.GlobalPosition;
        card.SetAlwaysOnTop();
        card.IsInteractive = false;

        // 4. 計算動畫目標位置
        Vector2 targetGlobal = toNode is Node2D to2D ? to2D.GlobalPosition + offset : toNode.GlobalPosition;

        // 5. 執行 Tween 動畫
        var tween = CreateTween();
        tween.TweenProperty(card, "global_position", targetGlobal, duration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        await ToSignal(tween, "finished");

        card.OriginalPosition = targetGlobal;
        card.IsInteractive = false;
    }
}