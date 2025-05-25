using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public List<Card> _deck = new();

    public override void _Ready()
    {
        InitDeck();
    }

    private void InitDeck()
    {
        foreach (CardColor color  in Enum.GetValues(typeof(CardColor)))
        {
            if (color == CardColor.Wild) continue;
            for (int i = 0; i <= 9; i++)
            {
               _deck.Add(Card.CreateCard($"{color.ToString().ToLower()}{i}",color,CardType.Number)); 
            }

            string reverseName = $"{color.ToString().ToLower()}Reverse";
            string skipName = $"{color.ToString().ToLower()}Skip";
            string drawTwoName = $"{color.ToString().ToLower()}DrawTwo";
            
            _deck.Add(Card.CreateCard(reverseName,color,CardType.Reverse));
            _deck.Add(Card.CreateCard(reverseName,color,CardType.Reverse));
            _deck.Add(Card.CreateCard(skipName,color,CardType.Skip));
            _deck.Add(Card.CreateCard(skipName,color,CardType.Skip));
            _deck.Add(Card.CreateCard(drawTwoName,color,CardType.DrawTwo));
            _deck.Add(Card.CreateCard(drawTwoName,color,CardType.DrawTwo));
        }

        for (int i = 0; i < 4; i++)
        {
            string wildName = $"{nameof(CardType.Wild).ToLower()}";
            string wildDrawFourName = $"{nameof(CardType.WildDrawFour).ToLower()}";
            _deck.Add(Card.CreateCard(wildName,CardColor.Wild,CardType.Wild));
            _deck.Add(Card.CreateCard(wildDrawFourName,CardColor.Wild,CardType.WildDrawFour));
        }
        
    }

    
}