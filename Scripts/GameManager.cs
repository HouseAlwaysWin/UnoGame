using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GodotHelper;

public partial class GameManager : Node
{
    public List<Card> _deck = new();
    private Node2D _deckPileNode;
    private Node2D _playerHand;
    private Button _playButton;

    public override async void _Ready()
    {
        // DebugHelper.WaitForDebugger();

        _deckPileNode = GetNode<Node2D>("DeckPile"); // 在主場景加一個 DeckPile 節點
        _playerHand = GetNode<Node2D>("PlayerHand");
        _playButton = GetNode<Button>("PlayButton");
        _playButton.Pressed += onPlayButtonPressed;
        InitDeck();
        DisplayDeckPile();
        // DisplayDeckPileWithRotation(_deckPileNode);
        // DealInitialCards();
        ShuffleDeck();
    }

    private async void onPlayButtonPressed()
    {
        GD.Print(_deck.Count);
        if (_deck.Count > 7)
        {
            await DealInitialCardsAsync();
        }
    }


    private void InitDeck()
    {
        foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
        {
            if (color == CardColor.Wild) continue;
            for (int i = 0; i <= 9; i++)
            {
                _deck.Add(CreateCard($"{color.ToString().ToLower()}{i}", color, CardType.Number));
                if (i == 0) continue;
                _deck.Add(CreateCard($"{color.ToString().ToLower()}{i}", color, CardType.Number));
            }

            string reverseName = $"{color.ToString().ToLower()}Reverse";
            string skipName = $"{color.ToString().ToLower()}Skip";
            string drawTwoName = $"{color.ToString().ToLower()}DrawTwo";

            _deck.Add(CreateCard(reverseName, color, CardType.Reverse));
            _deck.Add(CreateCard(reverseName, color, CardType.Reverse));
            _deck.Add(CreateCard(skipName, color, CardType.Skip));
            _deck.Add(CreateCard(skipName, color, CardType.Skip));
            _deck.Add(CreateCard(drawTwoName, color, CardType.DrawTwo));
            _deck.Add(CreateCard(drawTwoName, color, CardType.DrawTwo));
        }

        for (int i = 0; i < 4; i++)
        {
            string wildName = $"{nameof(CardType.Wild).ToLower()}";
            string wildDrawFourName = $"{nameof(CardType.WildDrawFour).ToLower()}";
            _deck.Add(CreateCard(wildName, CardColor.Wild, CardType.Wild));
            _deck.Add(CreateCard(wildDrawFourName, CardColor.Wild, CardType.WildDrawFour));
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
            visualCard.IsInDeck = true;
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

    private Card CreateCard(string cardImgName, CardColor cardColor, CardType cardType)
    {
        var cardScence = GD.Load<PackedScene>("res://Scenes/card.tscn");
        var newCard = cardScence.Instantiate<Card>();
        newCard.InstantiateCard(cardImgName, cardColor, cardType);
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

    private void DealInitialCards()
    {
        const int cardsToDeal = 5;
        const float spacing = 110f;
        float startX = -((cardsToDeal - 1) * spacing / 2f);

        for (int i = 0; i < cardsToDeal; i++)
        {
            Card card = _deck[0];
            _deck.RemoveAt(0);
            card.Position = new Vector2(startX + i * spacing, 0);
            _playerHand.AddChild(card);
        }
    }

    private async Task DealInitialCardsAsync()
    {
        int cardsToDeal = 7;
        const float spacing = 100f;
        float startX = -((cardsToDeal - 1) * spacing / 2f);
        if (_deck.Count < 5) cardsToDeal = _deck.Count;

        for (int i = 0; i < cardsToDeal; i++)
        {
            if (_deck.Count > 0)
            {
                Card card = _deck[0];
                _deck.RemoveAt(0);
                // GD.Print(card.CardImage.Texture.ResourcePath);
                card.Position = _deckPileNode.GlobalPosition;
                AddChild(card);

                Vector2 targetPos = _playerHand.GlobalPosition + new Vector2(startX + i * spacing, 0);

                var tween = CreateTween();
                tween.TweenProperty(card, "global_position", targetPos, 0.5).SetTrans(Tween.TransitionType.Sine)
                    .SetEase(Tween.EaseType.Out);
                await ToSignal(tween, "finished");

                card.OriginalPosition = card.Position;

                // _playerHand.AddChild(card);
                // var pos = card.ToLocal(targetPos);
            }
        }
    }
}