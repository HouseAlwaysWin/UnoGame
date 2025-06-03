using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Player : Node2D
{
    public int PlayerId;
    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetParent<GameManager>();
    }

    public void AddCard(Card card)
    {
        // card.PlayerId = PlayerId;
        AddChild(card);
        // ReorderHand();
    }

    public void RemoveCard(Card card)
    {
        RemoveChild(card);
        // ReorderHand();
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
            var tween = card.CreateTween();
            tween.TweenProperty(card, "global_position", targetPos, 0.09)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
            await ToSignal(tween, "finished");

            card.OriginalPosition = targetPos;
            card.OriginalZIndex = card.ZIndex;
            // card.SetAlwaysOnTop();
        }
    }
}