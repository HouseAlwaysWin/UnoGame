using System.Threading.Tasks;

public class GameOverState : BaseGameState
{
    public GameOverState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override Task EnterState()
    {
        return Task.CompletedTask;
    }
}
