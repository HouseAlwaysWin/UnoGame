using Godot;
using System;
using System.Collections.Generic;
using GodotHelper;

public partial class GameManager : Node
{
    public List<Card> _deck = new();
    private Node2D _deckPileNode;
    public override void _Ready()
    {
        // DebugHelper.WaitForDebugger();

        _deckPileNode = GetNode<Node2D>("DeckPile"); // 在主場景加一個 DeckPile 節點
        InitDeck();
        DisplayDeckPile();
    }

    private void InitDeck()
    {
        foreach (CardColor color  in Enum.GetValues(typeof(CardColor)))
        {
            if (color == CardColor.Wild) continue;
            for (int i = 0; i <= 9; i++)
            {
               _deck.Add(CreateCard($"{color.ToString().ToLower()}{i}",color,CardType.Number)); 
            }

            string reverseName = $"{color.ToString().ToLower()}Reverse";
            string skipName = $"{color.ToString().ToLower()}Skip";
            string drawTwoName = $"{color.ToString().ToLower()}DrawTwo";
            
            _deck.Add(CreateCard(reverseName,color,CardType.Reverse));
            _deck.Add(CreateCard(reverseName,color,CardType.Reverse));
            _deck.Add(CreateCard(skipName,color,CardType.Skip));
            _deck.Add(CreateCard(skipName,color,CardType.Skip));
            _deck.Add(CreateCard(drawTwoName,color,CardType.DrawTwo));
            _deck.Add(CreateCard(drawTwoName,color,CardType.DrawTwo));
        }

        for (int i = 0; i < 4; i++)
        {
            string wildName = $"{nameof(CardType.Wild).ToLower()}";
            string wildDrawFourName = $"{nameof(CardType.WildDrawFour).ToLower()}";
            _deck.Add(CreateCard(wildName,CardColor.Wild,CardType.Wild));
            _deck.Add(CreateCard(wildDrawFourName,CardColor.Wild,CardType.WildDrawFour));
        }
        
    }

    private void DisplayDeckPile()
    {
        const int MaxStackSize = 10;
        const float OffsetStep = 2.5f;
        const float RotationStep = 0.5f;
    
        int count = Math.Min(_deck.Count, MaxStackSize);
        for (int i = 0; i < count; i++)
        {
            Card visualCard = CreateCard($"deck",CardColor.Wild, CardType.Wild); // 顯示用，不影響資料堆
            visualCard.Position = new Vector2(0, -i * OffsetStep); // 小小位移
            visualCard.ZIndex = i;
            visualCard.RotationDegrees = GD.Randf() * RotationStep - (RotationStep / 2); // 微旋轉
            // visualCard.Modulate = new Color(1, 1, 1, 0.8f); // 透明度微低
        
            _deckPileNode.AddChild(visualCard);
        }
    }
    
    private  Card CreateCard(string cardImgName,CardColor cardColor,CardType cardType)
    {
        var cardScence = GD.Load<PackedScene>("res://Scenes/card.tscn");
        var newCard = cardScence.Instantiate<Card>();
        newCard.SetUpCardInfo(cardImgName, cardColor, cardType);
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
    
}