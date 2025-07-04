using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Player : Node2D
{
    public int PlayerSeqNo;
    public string PlayerId;
    public bool IsComputerPlayer = false;
    private GameManager _gameManager;
    public Tween ReorderHandeTween;

    public override void _Ready()
    {
        _gameManager = GetParent<GameManager>();
    }

    public List<Card> GetPlayerHandCards()
    {
        var handCards = this.GetChildren().Where(c => c is Card).Cast<Card>().ToList();
        return handCards;
    }

    public void ShowHandCards(bool show)
    {
        foreach (var card in GetPlayerHandCards())
        {
            card.Visible = show;
        }
    }

    public void SetHandCardsInteractive(bool interactive)
    {
        foreach (var child in this.GetChildren())
        {
            if (child is Card card)
            {
                card.IsInteractive = interactive;
            }
        }
    }



    public async void ComPlayerDealCard()
    {
        var topCard = _gameManager.DropZonePileNode.GetTopCardInDropZone();
        topCard.ResetBorder();
        var handCards = GetPlayerHandCards();
        List<Card> validCards = handCards.Where(c => _gameManager.CanPlaceCard(c)).ToList();
        if (validCards.Count == 0)
        {
            _gameManager.OnPassed();
            return;
        }
        var firstCard = validCards.First();
        firstCard.IsInteractive = false;

        _gameManager.StateMachine.CurrentPlayedCard = firstCard;
        await _gameManager.StateMachine.ChangeState(GameState.PlayerPlayCard);
    }

    public async Task ReorderHand()
    {
        var cards = new List<Card>();
        foreach (var child in this.GetChildren())
        {
            if (child is Card c)
                cards.Add(c);
        }

        int cardsToDeal = cards.Count;
        float spacing = _gameManager.CardSpacing;
        // float startX = -((cardsToDeal - 1) * spacing / 2f);
        // float startX = GetViewport().GetVisibleRect().Size.X /2;
        float startX = 0;

        var maxCardSpacing = cardsToDeal * spacing;
        if (maxCardSpacing > _gameManager.MaxCardSpacing)
        {
            spacing = _gameManager.MaxCardSpacing / cardsToDeal;
        }

        for (int i = 0; i < cardsToDeal; i++)
        {
            var card = cards[i];
            Vector2 targetPos = GlobalPosition + new Vector2(startX + i * spacing, 0);
            // ✅ 設定唯一 ZIndex
            card.ZAsRelative = false;
            card.ZIndex = i;
            ReorderHandeTween = card.CreateTween();
            ReorderHandeTween.TweenProperty(card, "global_position", targetPos, 0.09)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
            await ToSignal(ReorderHandeTween, "finished");

            card.OriginalPosition = targetPos;
            card.OriginalZIndex = card.ZIndex;
            // card.SetAlwaysOnTop();
        }
    }
}