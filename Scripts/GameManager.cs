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

    public List<Card> Deck = new();
    public List<Player> Players = new();
    public Player PlayerHand;
    public Node2D DropZonePileNode;
    public Area2D DropZoneArea;
    public Node2D DeckPileNode;
    public VBoxContainer PlayerInfoPanel;
    private readonly List<Label> _playerInfoPanelLabels = new();
    private readonly List<Label> _playerSeqPanelLabels = new();
    private Card _hoveredCard = null;
    private Card _draggedCard = null;

    private Button _playButton;
    private Button _playButton2;
    private Node2D _otherPlayer;
    private int _currentPlayerIndex = 0;
    private bool _isClockwise = true; // true: 順時針, false: 逆時針

    private Node2D _directionArrow;
    private Tween _directionTween;
    private float _arrowRotation = 0f;
    private Card _lastHoveredCard;

    private Card _currentTopCard; // 新增
    private GameStateMachine _gameStateMachine;




    public override async void _Ready()
    {
        // DebugHelper.WaitForDebugger();

        _otherPlayer = GetNode<Node2D>("OtherPlayer");
        DeckPileNode = GetNode<Node2D>("DeckPile"); // 在主場景加一個 DeckPile 節點
        PlayerHand = GetNode<Player>("PlayerHand");
        _playButton = GetNode<Button>("PlayButton");
        _playButton.Pressed += onPlayButtonPressed;

        _playButton2 = GetNode<Button>("PlayButton2");
        _playButton2.Pressed += onPlayButtonPressed2;


        _directionArrow = GetNode<Node2D>("DirectionArrow");
        _directionArrow.RotationDegrees = 90;
        _arrowRotation = 90;

        DropZoneArea = GetNode<Area2D>("DropZonePile/Area2D");
        DropZonePileNode = GetNode<Node2D>("DropZonePile");

        PlayerInfoPanel = GetNode<VBoxContainer>("UI/PlayerInfoPanel");
        // 初始狀態交給 StateMachine 處理
        _gameStateMachine = GetNode<GameStateMachine>("GameStateMachine");
        _gameStateMachine.ChangeState(GameState.Init);
        _gameStateMachine.ChangeState(GameState.DealCards);

        // InitDeck();
        // DisplayDeckPile();
        // ShuffleDeck();
        // await InitPlayerAndUIAsync();

        // await DealBeginCard();
        // await _gameStateMachine.DealingCardsToPlayerAsync(PlayerHand, 7);
    }

    public override void _Process(double delta)
    {
        if (_draggedCard == null)
        {
            UpdateHoveredCard();
        }

    }

    public async Task DealBeginCard()
    {
        Card card = Deck[0];
        Deck.RemoveAt(0);
        await MoveOneCardToTarget(card, DeckPileNode, DropZonePileNode);
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
        Players.Clear();
        Players.Add(PlayerHand);

        for (int i = 1; i < comPlayerNumber + 1; i++)
        {
            var playerScence = GD.Load<PackedScene>("res://Scenes/player.tscn");
            var newPlayer = playerScence.Instantiate<Player>();
            newPlayer.PlayerId = i;
            newPlayer.Name = $"COM Player {i + 1}";
            AddChild(newPlayer);
            await _gameStateMachine.DealingCardsToPlayerAsync(newPlayer, 7);
            Players.Add(newPlayer);
        }

        // 2. 打亂 _players 順序（遊戲邏輯用）
        var rng = new Random();
        Players = Players.OrderBy(_ => rng.Next()).ToList();

        // 3. 清空 UI 面板
        _playerInfoPanelLabels.Clear();
        _playerSeqPanelLabels.Clear();
        foreach (var child in PlayerInfoPanel.GetChildren())
            PlayerInfoPanel.RemoveChild(child);

        // 4. 建立 UI（依照打亂順序）
        for (int i = 0; i < Players.Count; i++)
        {
            CreatedPlayerUI(Players[i], i + 1); // 傳入玩家 + 頭像編號
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
        PlayerInfoPanel.AddChild(hbox);
        _playerInfoPanelLabels.Add(label);
    }

    public void NextTurn()
    {
        if (_isClockwise)
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % Players.Count;
        }
        else
        {
            _currentPlayerIndex = (_currentPlayerIndex - 1 + Players.Count) % Players.Count;
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
        var topCard = GetTopCard();
        if (_hoveredCard != topCard)
        {
            _hoveredCard?.OnHoverExit();
            topCard?.OnHoverEnter();
            _hoveredCard = topCard;
        }
    }

    public Card GetTopCard()
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
        return topCard;
    }


    private float DistanceToMouse(Card card)
    {
        return card.GlobalPosition.DistanceTo(GetGlobalMousePosition());
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


    private async void onPlayButtonPressed()
    {
        GD.Print(Deck.Count);
        if (Deck.Count > 7)
        {
            await _gameStateMachine.DealingCardsToPlayerAsync();
            await PlayerHand.ReorderHand();
            PlayerHand.SetAllCardsInteractive(true);
        }
    }

    public Card CreateCard(string cardImgName, CardColor cardColor, CardType cardType, int cardNumber = -1, int zindex = 0)
    {
        var cardScence = GD.Load<PackedScene>("res://Scenes/card.tscn");
        var newCard = cardScence.Instantiate<Card>();
        newCard.InstantiateCard(PlayerHand, cardImgName, cardColor, cardType, DropZonePileNode.GetPath(), cardNumber);
        newCard.Name = $"{cardColor}{cardType}{cardNumber}";
        newCard.ZIndex = zindex;
        return newCard;
    }

    /// <summary>
    /// 通用卡片移動
    /// </summary>
    /// <param name="card"></param>
    /// <param name="fromNode"></param>
    /// <param name="toNode"></param>
    /// <param name="offset"></param>
    /// <param name="duration"></param>
    public async Task MoveOneCardToTarget(Card card, Node2D fromNode, Node2D toNode, Vector2 offset = default,
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
    }
}