using Godot;
using System;
using System.Threading.Tasks;

public enum GameState
{
    Init,
    DealCards,
    WaitForPlayerAction,
    PlayerPlayCard,
    ResolveEffect,
    CheckWinOrNextTurn,
    GameOver
}

public partial class GameStateMachine : Node
{
    private GameManager _gameManager;

    public GameState CurrentState { get; private set; }

    public override void _Ready()
    {
        _gameManager = GetParent<GameManager>(); // 或用依賴注入
        // ChangeState(GameState.Init);
    }

    public async void ChangeState(GameState newState)
    {
        GD.Print($"狀態變化: {CurrentState} ➡ {newState}");
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Init:
                InitDeck();
                ShuffleDeck();
                DisplayDeckPile();
                break;

            case GameState.DealCards:
                await _gameManager.InitPlayerAndUIAsync();
                await _gameManager.DealBeginCard();
                await DealingCardsToPlayerAsync(_gameManager.PlayerHand, 7);
                _gameManager.PlayerHand.SetAllCardsInteractive(true);
                ChangeState(GameState.WaitForPlayerAction);
                break;

            case GameState.WaitForPlayerAction:
                // 玩家能開始拖曳卡片出牌
                break;

            case GameState.ResolveEffect:
                // 可依據卡片類型做不同效果
                break;

            case GameState.CheckWinOrNextTurn:
                // 檢查是否有人沒牌，否則下一位
                break;
        }
    }

    public void InitDeck()
    {
        foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
        {
            if (color == CardColor.Wild) continue;
            for (int i = 0; i <= 9; i++)
            {
                _gameManager.Deck.Add(_gameManager.CreateCard($"{color.ToString().ToLower()}{i}", color, CardType.Number, i));
                if (i == 0) continue;
                _gameManager.Deck.Add(_gameManager.CreateCard($"{color.ToString().ToLower()}{i}", color, CardType.Number, i));
            }

            string reverseName = $"{color.ToString().ToLower()}Reverse";
            string skipName = $"{color.ToString().ToLower()}Skip";
            string drawTwoName = $"{color.ToString().ToLower()}DrawTwo";

            _gameManager.Deck.Add(_gameManager.CreateCard(reverseName, color, CardType.Reverse, 10));
            _gameManager.Deck.Add(_gameManager.CreateCard(reverseName, color, CardType.Reverse, 10));
            _gameManager.Deck.Add(_gameManager.CreateCard(skipName, color, CardType.Skip, 11));
            _gameManager.Deck.Add(_gameManager.CreateCard(skipName, color, CardType.Skip, 11));
            _gameManager.Deck.Add(_gameManager.CreateCard(drawTwoName, color, CardType.DrawTwo, 12));
            _gameManager.Deck.Add(_gameManager.CreateCard(drawTwoName, color, CardType.DrawTwo, 12));
        }

        for (int i = 0; i < 4; i++)
        {
            string wildName = $"{CardType.Wild.ToString().ToLower()}";
            string wildDrawFourName = $"{CardType.WildDrawFour.ToString().ToCamelCase()}";
            _gameManager.Deck.Add(_gameManager.CreateCard(wildName, CardColor.Wild, CardType.Wild, 13));
            _gameManager.Deck.Add(_gameManager.CreateCard(wildDrawFourName, CardColor.Wild, CardType.WildDrawFour, 14));
        }
    }

    public void DisplayDeckPile()
    {
        const int MaxStackSize = 10;
        const float OffsetStep = 3f;
        const float RotationStep = 0.5f;

        int count = Math.Min(_gameManager.Deck.Count, MaxStackSize);
        for (int i = 0; i < count; i++)
        {
            Card visualCard = _gameManager.CreateCard($"deck", CardColor.Wild, CardType.Wild); // 顯示用，不影響資料堆
            visualCard.Position = new Vector2(0, -i * OffsetStep); // 小小位移
            visualCard.ZAsRelative = false;
            visualCard.ZIndex = i;
            visualCard.RotationDegrees = GD.Randf() * RotationStep - (RotationStep / 2); // 微旋轉
            visualCard.IsInteractive = false;
            // visualCard.Modulate = new Color(1, 1, 1, 0.8f); // 透明度微低

            _gameManager.DeckPileNode.AddChild(visualCard);
        }
    }

    public void ShuffleDeck()
    {
        Random random = new Random();
        for (int i = _gameManager.Deck.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (_gameManager.Deck[i], _gameManager.Deck[j]) = (_gameManager.Deck[j], _gameManager.Deck[i]);
        }
        // 設定階層
        for (int i = 0; i < _gameManager.Deck.Count - 1; i++)
        {
            _gameManager.Deck[i].ZIndex = i;
        }
    }

    public async Task DealingCardsToPlayerAsync(Player? playerHand = null, int? dealNum = null)
    {
        Player player = playerHand ?? _gameManager.PlayerHand;
        int cardsToDeal = dealNum ?? _gameManager.CardsToDeal;
        float spacing = _gameManager.CardSpacing;
        // float startX = -((cardsToDeal - 1) * spacing / 2f);
        Vector2 windowSize = DisplayServer.WindowGetSize();
        float startX = 0;

        if (_gameManager.Deck.Count < 5) cardsToDeal = _gameManager.Deck.Count;
        var maxCardSpacing = cardsToDeal * spacing;
        if (maxCardSpacing > _gameManager.MaxCardSpacing)
        {
            spacing = _gameManager.MaxCardSpacing / cardsToDeal;
        }

        for (int i = 0; i < cardsToDeal; i++)
        {
            if (_gameManager.Deck.Count > 0)
            {
                Card card = _gameManager.Deck[0];
                _gameManager.Deck.RemoveAt(0);

                // Card visualCard = CreateCard($"deck", CardColor.Wild, CardType.Wild); // 顯示用，不影響資料堆
                // GD.Print(card.CardImage.Texture.ResourcePath);
                // AddChild(card);

                player.AddCard(card);
                if (player != _gameManager.PlayerHand)
                {
                    card.Hide();
                }
                else
                {
                    card.GlobalPosition = _gameManager.DeckPileNode.GlobalPosition;
                    // card.IsInteractive = false;
                    Vector2 targetPos = player.GlobalPosition + new Vector2(startX + i * spacing, 0);
                    card.SetAlwaysOnTop();
                    var tween = CreateTween();
                    tween.TweenProperty(card, "global_position", targetPos, 0.5).SetTrans(Tween.TransitionType.Sine)
                        .SetEase(Tween.EaseType.Out);
                    await ToSignal(tween, "finished");
                    // card.IsInteractive = true;
                    // 紀錄目前位置
                    card.OriginalPosition = targetPos;
                }
            }
        }
    }


}
