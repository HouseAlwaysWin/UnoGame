using System.Threading.Tasks;

public class CheckWinOrNextTurnState : BaseGameState
{
    public CheckWinOrNextTurnState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override async Task EnterState()
    {
        if (GameManager.IsPlayerWin())
        {
            await StateMachine.ChangeState(GameState.GameOver);
            return;
        }

        int skip = 1;
        var card = StateMachine.CurrentPlayedCard;
        if (card != null)
        {
            switch (card.CardType)
            {
                case CardType.WildDrawFour:
                case CardType.DrawTwo:
                case CardType.Skip:
                    skip = 2;
                    break;
                case CardType.Reverse:
                    if (GameManager.Players.Count == 2)
                        skip = 2;
                    break;
            }
        }

        GameManager.NextTurn(skip);
        StateMachine.CurrentPlayedCard = null;

        await StateMachine.ChangeState(GameState.WaitForPlayerAction);
    }
}
