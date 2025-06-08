using Godot;
using System;
using System.Threading.Tasks;

public partial class DropZone : Node2D
{
    private Area2D _dropZoneArea;
    private GameManager _gameManager;

    public override void _Ready()
    {
        _dropZoneArea = GetNode<Area2D>("DropZoneArea");
        _gameManager = GetParentOrNull<GameManager>();
    }

    public  bool CanPlaceCard(Card card, Card topCard)
    {
        if (card.CardColor == CardColor.Wild || topCard.CardColor == CardColor.Wild)
            return true;
        return card.CardColor == topCard.CardColor
               || (card.CardType == topCard.CardType && card.Number == topCard.Number);
    }
    
    

    public Card GetTopCardInDropZone()
    {
        Card topCard = null;
        int maxZ = int.MinValue;

        foreach (var child in this.GetChildren())
        {
            if (child is Card card)
            {
                if (card.ZIndex > maxZ)
                {
                    maxZ = card.ZIndex;
                    topCard = card;
                }
            }
        }

        return topCard;
    }
   
}