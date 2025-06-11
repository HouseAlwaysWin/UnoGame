using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GodotHelper;

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
    private readonly Dictionary<GameState, BaseGameState> _states = new();
    private BaseGameState _currentState;

    public GameState CurrentState { get; private set; }

    /// <summary>
    /// The card that has been played and awaiting resolution.
    /// </summary>
    public Card? CurrentPlayedCard { get; set; }

    public override void _Ready()
    {
        _gameManager = GetParent<GameManager>();
        _states[GameState.Init] = new InitState(this, _gameManager);
        _states[GameState.DealCards] = new DealCardsState(this, _gameManager);
        _states[GameState.WaitForPlayerAction] = new WaitForPlayerActionState(this, _gameManager);
        _states[GameState.PlayerPlayCard] = new PlayerPlayCardState(this, _gameManager);
        _states[GameState.ResolveEffect] = new ResolveEffectState(this, _gameManager);
        _states[GameState.CheckWinOrNextTurn] = new CheckWinOrNextTurnState(this, _gameManager);
        _states[GameState.GameOver] = new GameOverState(this, _gameManager);
    }

    public async Task ChangeState(GameState newState)
    {
        GD.Print($"狀態變化: {CurrentState} ➡ {newState}");
        _currentState?.ExitState();
        CurrentState = newState;
        _currentState = _states[newState];
        await _currentState.EnterState();
    }

    public void InitDeck()
    {
        foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
        {
            if (color == CardColor.Wild) continue;
            for (int i = 0; i <= 9; i++)
            {
                _gameManager.Deck.Add(_gameManager.CreateCard($"{color.ToString().ToLower()}{i}", color,
                    CardType.Number, i));
                if (i == 0) continue;
                _gameManager.Deck.Add(_gameManager.CreateCard($"{color.ToString().ToLower()}{i}", color,
                    CardType.Number, i));
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
            string wildDrawFourName = $"{CardType.WildDrawFour.ToString().ToLowerCamelCase()}";
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
            Card visualCard = _gameManager.CreateCard($"deck", CardColor.Wild, CardType.Wild);
            visualCard.Position = new Vector2(0, -i * OffsetStep);
            visualCard.ZAsRelative = false;
            visualCard.ZIndex = i;
            visualCard.RotationDegrees = GD.Randf() * RotationStep - (RotationStep / 2);
            visualCard.IsInteractive = false;
            visualCard.Modulate = new Color(1, 1, 1, 0.8f);

            _gameManager.DeckPileNode.AddChild(visualCard);
        }
    }

    public void ShuffleDeck(List<Card> cards)
    {
        Random random = new Random();
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
            cards[i].ZIndex = i;
        }
    }

    public async Task DealingCardsToPlayerAsync(Player? playerHand = null, int dealNum = 1, bool showAnimation = true,
        bool showCard = true)
    {
        await _gameManager.MoveCardsToTarget(_gameManager.Deck, dealNum, _gameManager.DeckPileNode, playerHand,
            offset: (i) => { return new Vector2(i * _gameManager.CardSpacing, 0); }, showAnimation: showAnimation,
            showCard: showCard);
    }
}
