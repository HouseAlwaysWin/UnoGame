using System.Threading.Tasks;

public class ResolveEffectState : BaseGameState
{
    public ResolveEffectState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override Task EnterState()
    {
        return Task.CompletedTask;
    }
}
