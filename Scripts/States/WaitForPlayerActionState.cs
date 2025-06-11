using System.Threading.Tasks;

public class WaitForPlayerActionState : BaseGameState
{
    public WaitForPlayerActionState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override Task EnterState()
    {
        GameManager.SetCurrentPlayerHandActive();
        return Task.CompletedTask;
    }
}
