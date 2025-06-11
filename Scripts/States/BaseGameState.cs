using System.Threading.Tasks;

public abstract class BaseGameState
{
    protected readonly GameStateMachine StateMachine;
    protected readonly GameManager GameManager;

    protected BaseGameState(GameStateMachine stateMachine, GameManager gameManager)
    {
        StateMachine = stateMachine;
        GameManager = gameManager;
    }

    public virtual Task EnterState()
    {
        return Task.CompletedTask;
    }

    public virtual void ExitState() { }
}
