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

    [Export] public int ComPlayerNumber = 4;

    public List<Card> Deck = new();
    public List<Player> Players = new();
    public Node2D PlayerHandZone;
    public CardColor CurrentCardColor;
    public string CurrentPlayerId = "Player 1"; // 當前玩家手牌的 PlayerId

    public Player CurrentPlayer =>
        Players.FirstOrDefault(p => p.PlayerSeqNo == _currentPlayerIndex); // 當前玩家手牌的 PlayerId
    public Player NextPlayer =>
        Players.FirstOrDefault(p => p.PlayerSeqNo == (_isClockwise ? 
            (_currentPlayerIndex + 1) % Players.Count:(_currentPlayerIndex - 1)+ Players.Count% Players.Count)); // 當前玩家手牌的 PlayerId
 

    public Node2D DropZonePileNode;
    public Area2D DropZoneArea;
    public Node2D DeckPileNode;
    public VBoxContainer PlayerInfoPanel;

    public List<PlayerUI> PlayerUIInfos = new();

    // private readonly List<Label> _playerInfoPanelLabels = new();
    // private readonly List<Label> _playerSeqPanelLabels = new();
    private Card _hoveredCard = null;
    private Card _draggedCard = null;

    private Button _playButton;
    private Button _playButton2;
    private Button _playButton3;
    private Button _playButton4;
    private Button _playButton5;
    private Button _playButton6;
    private Button _playButton7;

    private Node2D _playerZone;
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

        _playerZone = GetNode<Node2D>("PlayerZone");
        DeckPileNode = GetNode<Node2D>("DeckPile"); // 在主場景加一個 DeckPile 節點
        PlayerHandZone = GetNode<Node2D>("PlayerHandZone");
        _playButton = GetNode<Button>("UI/TestButton/PlayButton");
        _playButton.Pressed += onPlayButtonPressed;

        _playButton2 = GetNode<Button>("UI/TestButton/PlayButton2");
        _playButton2.Pressed += onPlayButtonPressed2;

        _playButton3 = GetNode<Button>("UI/TestButton/PlayButton3");
        _playButton3.Pressed += onPlayButtonPressed3;

        _playButton4 = GetNode<Button>("UI/TestButton/PlayButton4");
        _playButton4.Pressed += onPlayButtonPressed4;

        _playButton5 = GetNode<Button>("UI/TestButton/PlayButton5");
        _playButton5.Pressed += onPlayButtonPressed5;
        
        _playButton6 = GetNode<Button>("UI/TestButton/PlayButton6");
        _playButton6.Pressed += onPlayButtonPressed6;
        
        _playButton7 = GetNode<Button>("UI/TestButton/PlayButton7");
        _playButton7.Pressed += onPlayButtonPressed7;

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
    }


    private async void onPlayButtonPressed()
    {
        OnPassed();
    }

    private void onPlayButtonPressed2()
    {
        NextTurn();
    }

    private async void onPlayButtonPressed3()
    {
        await ReverseDirection();
    }

    private async void onPlayButtonPressed4()
    {
        // CurrentPlayerHand.ShowHandCards(true);

        Card firstCard = CurrentPlayer.GetPlayerHandCards().FirstOrDefault();
        await MoveCardToTarget(firstCard, _playerZone, DropZonePileNode, showCard: true);
    }

    private bool _showAllCars = false;

    private async void onPlayButtonPressed5()
    {
        _showAllCars = !_showAllCars;
        foreach (var player in Players)
        {
            if (player == CurrentPlayer)
            {
                player.ShowHandCards(_showAllCars);
            }
        }
    }

    private async void onPlayButtonPressed6()
    {
        NextTurn(2);
    }
    
    private async void onPlayButtonPressed7()
    {
        await _gameStateMachine.DealingCardsToPlayerAsync(NextPlayer, 4);
        await NextPlayer.ReorderHand();
    }
    
    public async void OnPassed()
    {
        await _gameStateMachine.DealingCardsToPlayerAsync(CurrentPlayer);
        CurrentPlayer.SetHandCardsInteractive(true);
        await CurrentPlayer.ReorderHand();
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
        await MoveCardToTarget(card, DeckPileNode, DropZonePileNode);
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
        // 清空 UI 面板
        foreach (var child in PlayerInfoPanel.GetChildren())
            PlayerInfoPanel.RemoveChild(child);

        for (int i = 1; i <= comPlayerNumber; i++)
        {
            var playerName = $"Player {i}";
            //   設定Player UI
            var playerUIScence = GD.Load<PackedScene>("res://Scenes/player_ui.tscn");
            var newPlayerUI = playerUIScence.Instantiate<PlayerUI>();
            PlayerInfoPanel.AddChild(newPlayerUI);
            newPlayerUI.InitPlayerUI(playerName, i.ToString(), $"avatar{i}.jpeg", playerName);
            PlayerUIInfos.Add(newPlayerUI);

            //  建立 Player 實體
            var playerScence = GD.Load<PackedScene>("res://Scenes/player.tscn");
            var newPlayer = playerScence.Instantiate<Player>();
            AddChild(newPlayer);
            newPlayer.PlayerSeqNo = i - 1; // 玩家序號從 0 開始
            newPlayer.PlayerId = playerName;
            newPlayer.Name = playerName;
            // if (newPlayer.PlayerId == CurrentPlayerHandId)
            // {
            //     newPlayer.GlobalPosition = PlayerHandZone.GlobalPosition;
            // }
            // else
            // {
            // newPlayer.GlobalPosition = _playerZone.GlobalPosition + new Vector2(0, i * -150);
            // newPlayer.GlobalPosition = PlayerHandZone.GlobalPosition + new Vector2(0, i * -150);

            newPlayer.GlobalPosition = PlayerHandZone.GlobalPosition;
            if (newPlayer.PlayerId != CurrentPlayerId)
            {
                await _gameStateMachine.DealingCardsToPlayerAsync(newPlayer, 7, false, false);
            }
            else
            {
                await _gameStateMachine.DealingCardsToPlayerAsync(newPlayer, 7, true, true);
            }

            // }
            Players.Add(newPlayer);
        }

        _currentPlayerIndex = 0;

        UpdateCurrentPlayerUI();
        // 2. 打亂 _players 順序（遊戲邏輯用）
        // var rng = new Random();
        // Players = Players.OrderBy(_ => rng.Next()).ToList();
        // var player = Players.FirstOrDefault(p => p.PlayerId == CurrentPlayerHandId);
        // await _gameStateMachine.DealingCardsToPlayerAsync(player, 7);
        SetCurrentPlayerHandActive();
    }

    public void NextTurn(int skip = 1)
    {
        if (_isClockwise)
        {
            _currentPlayerIndex = (_currentPlayerIndex + skip) % Players.Count;
        }
        else
        {
            _currentPlayerIndex = (_currentPlayerIndex - skip + Players.Count) % Players.Count;
        }

        UpdateCurrentPlayerUI();
        SetCurrentPlayerHandActive();
    }

    public void UpdateCurrentPlayerUI()
    {
        for (int i = 0; i < PlayerUIInfos.Count; i++)
        {
            var playerUI = PlayerUIInfos[i];
            playerUI.SeqNo.Text = $"{i + 1}.";
            playerUI.PlayerName.AddThemeColorOverride("font_color",
                i == _currentPlayerIndex ? Colors.Yellow : Colors.White);
        }
    }

    public void SetCurrentPlayerHandActive()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            var player = Players[i];
            if (player.PlayerSeqNo == _currentPlayerIndex)
            {
                player.SetHandCardsInteractive(true);
            }
            else
            {
                player.SetHandCardsInteractive(false);
            }
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


    public Card CreateCard(string cardImgName, CardColor cardColor, CardType cardType, int cardNumber = -1,
        int zindex = 0)
    {
        var cardScence = GD.Load<PackedScene>("res://Scenes/card.tscn");
        var newCard = cardScence.Instantiate<Card>();
        newCard.InstantiateCard(cardImgName, cardColor, cardType, cardNumber);
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
    /// <param name="duration"></param
    public async Task MoveCardToTarget(Card card, Node2D fromNode, Node2D toNode,
        bool showAnimation = true, bool showCard = true, float duration = 0.4f,
        Vector2 offset = default)
    {
        // 從來源節點移除，加到目的地節點
        card.GetParent<Node2D>()?.RemoveChild(card);
        card.Visible = showCard;

        toNode.AddChild(card);

        // 3. 將卡片位置設定為原本 global 位置（在新 parent 下）
        card.GlobalPosition = fromNode.GlobalPosition;
        card.SetAlwaysOnTop();

        // 4. 計算動畫目標位置
        Vector2 targetGlobal = toNode is Node2D to2D ? to2D.GlobalPosition + offset : toNode.GlobalPosition;

        // 5. 執行 Tween 動畫
        if (showAnimation)
        {
            var tween = CreateTween();
            tween.TweenProperty(card, "global_position", targetGlobal, duration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
            await ToSignal(tween, "finished");
        }

        card.OriginalPosition = targetGlobal;
        card.GlobalPosition = targetGlobal;
    }

    /// <summary>
    /// 移動堆疊卡牌
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="moveNums"></param>
    /// <param name="fromNode"></param>
    /// <param name="toNode"></param>
    /// <param name="offset"></param>
    /// <param name="duration"></param>
    public async Task MoveCardsToTarget(List<Card> deck, int moveNums, Node2D fromNode, Node2D toNode,
        bool showAnimation = true, bool showCard = true, float duration = 0.4f,
        Func<int, Vector2> offset = null
    )
    {
        for (int i = 0; i < moveNums; i++)
        {
            Card card = deck.FirstOrDefault();
            if (card == null) break;
            deck.RemoveAt(0);
            var offsetValue = offset != null ? offset(i) : new Vector2(i * CardSpacing, 0);
            await MoveCardToTarget(card, fromNode, toNode, showAnimation, showCard, duration, offsetValue);
        }
    }
}