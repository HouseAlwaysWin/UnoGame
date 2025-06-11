using System.Threading.Tasks;

public class PlayerPlayCardState : BaseGameState
{
    public PlayerPlayCardState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override Task EnterState()
    {
        return Task.CompletedTask;
    }
}
