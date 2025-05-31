using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GodotHelper;

public partial class GameManager : Node
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


    public override async void _Ready()
    {
        // DebugHelper.WaitForDebugger();

        _deckPileNode = GetNode<Node2D>("DeckPile"); // 在主場景加一個 DeckPile 節點
        _playerHand = GetNode<Player>("PlayerHand");
        _playButton = GetNode<Button>("PlayButton");
        _playButton.Pressed += onPlayButtonPressed;

        _playButton2 = GetNode<Button>("PlayButton2");
        _playButton.Pressed += onPlayButtonPressed2;

        _dropZone = GetNode<Area2D>("DropZonePile/Area2D");
        _dropZonePileNode = GetNode<Node2D>("DropZonePile");
        InitDeck();
        DisplayDeckPile();
        // DisplayDeckPileWithRotation(_deckPileNode);
        // DealInitialCards();
        ShuffleDeck();
        await DealingCardsAsync(_playerHand, 7);

        await InitComPlayerHandAsync();
    }

    private void onPlayButtonPressed2()
    {
        _playerHand.ReorderHand();
    }

    public async Task InitComPlayerHandAsync(int? playerNumber = null)
    {
        int comPlayerNumber = playerNumber ?? ComPlayerNumber;
        for (int i = 0; i < comPlayerNumber; i++)
        {
            var playerScence = GD.Load<PackedScene>("res://Scenes/player.tscn");
            var newPlayer = playerScence.Instantiate<Player>();
            newPlayer.Name = $"COM Player {i + 1}";
            AddChild(newPlayer);
            await DealingCardsAsync(newPlayer, 7);
        }
    }

    private async void onPlayButtonPressed()
    {
        GD.Print(_deck.Count);
        if (_deck.Count > 7)
        {
            await DealingCardsAsync();
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
            string wildName = $"{nameof(CardType.Wild).ToLower()}";
            string wildDrawFourName = $"{nameof(CardType.WildDrawFour).ToLower()}";
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

    private Card CreateCard(string cardImgName, CardColor cardColor, CardType cardType, int cardNumber = -1)
    {
        var cardScence = GD.Load<PackedScene>("res://Scenes/card.tscn");
        var newCard = cardScence.Instantiate<Card>();
        newCard.InstantiateCard(_playerHand, cardImgName, cardColor, cardType, _dropZonePileNode.GetPath(), cardNumber);
        newCard.Name = $"{cardColor}{cardType}{cardNumber}";
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
    }

    private async Task DealingCardsAsync(Player? playerHand = null, int? dealNum = null)
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
}