using System.Collections.Generic;
using System.Linq;

namespace UnoCardGame.Scripts;

public partial class ComPlayer : Player
{
    private GameManager _gameManager;
    private GameStateMachine _gameStateMachine;
    public override void _Ready()
    {
        _gameManager = GetParent<GameManager>();
        _gameStateMachine = GetParent<GameStateMachine>();
    }

    public async void DealCard()
    {
        var handCards = GetPlayerHandCards();
        List<Card> validCards = handCards.Where(c => _gameManager.CanPlaceCard(c)).ToList();
        if (validCards.Count == 0)
        {
            _gameManager.OnPassed();
            return;
        }
        var firstCard = validCards.First();
        await _gameManager.MoveCardToTarget(firstCard, _gameManager.PlayerZone, _gameManager.DropZonePileNode);

        if (firstCard.CardType == CardType.Wild || firstCard.CardType == CardType.WildDrawFour)
        {
            return;
        }
    }


}