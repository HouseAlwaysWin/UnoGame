using System.Threading.Tasks;

public class DealCardsState : BaseGameState
{
    public DealCardsState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override async Task EnterState()
    {
        await GameManager.DealBeginCard();
        await GameManager.InitPlayerAndUIAsync();
        await StateMachine.ChangeState(GameState.WaitForPlayerAction);
    }
}
