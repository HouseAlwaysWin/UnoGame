using System.Threading.Tasks;

public class CheckWinOrNextTurnState : BaseGameState
{
    public CheckWinOrNextTurnState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override Task EnterState()
    {
        return Task.CompletedTask;
    }
}
