using System.Threading.Tasks;
using Godot;

public class ResolveEffectState : BaseGameState
{
    public ResolveEffectState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override async Task EnterState()
    {
        var card = StateMachine.CurrentPlayedCard;
        if (card != null)
        {
            if (card.CardType == CardType.Wild)
            {
                if (GameManager.CurrentPlayer == GameManager.MyPlayer)
                {
                    CardColor selectColor = await GameManager.ColorSelector.ShowAndWait();
                    card.CardColor = selectColor;
                    card.SetWildColor(selectColor);
                }
                else
                {
                    GD.Randomize();
                    var index = (int)(GD.Randi() % 4);
                    var randomColor = (CardColor)index;
                    card.SetWildColor(randomColor);
                }
            }
            else if (card.CardType == CardType.WildDrawFour)
            {
                if (GameManager.CurrentPlayer == GameManager.MyPlayer)
                {
                    CardColor selectColor = await GameManager.ColorSelector.ShowAndWait();
                    card.CardColor = selectColor;
                    card.SetWildColor(selectColor);
                }
                else
                {
                    GD.Randomize();
                    var index = (int)(GD.Randi() % 4);
                    var randomColor = (CardColor)index;
                    card.SetWildColor(randomColor);
                }

                if (GameManager.NextPlayer == GameManager.MyPlayer)
                {
                    await StateMachine.DealingCardsToPlayerAsync(GameManager.NextPlayer, 4);
                    await GameManager.NextPlayer.ReorderHand();
                }
                else
                {
                    await StateMachine.DealingCardsToPlayerAsync(GameManager.NextPlayer, 4, false, false);
                }
            }
            else if (card.CardType == CardType.DrawTwo)
            {
                if (GameManager.NextPlayer == GameManager.MyPlayer)
                {
                    await StateMachine.DealingCardsToPlayerAsync(GameManager.NextPlayer, 2);
                    await GameManager.NextPlayer.ReorderHand();
                }
                else
                {
                    await StateMachine.DealingCardsToPlayerAsync(GameManager.NextPlayer, 2, false, false);
                }
            }
            else if (card.CardType == CardType.Reverse)
            {
                await GameManager.ReverseDirection();
            }
        }

        await StateMachine.ChangeState(GameState.CheckWinOrNextTurn);
    }
}
