using System.Threading.Tasks;

public class InitState : BaseGameState
{
    public InitState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override Task EnterState()
    {
        StateMachine.InitDeck();
        StateMachine.ShuffleDeck(GameManager.Deck);
        StateMachine.DisplayDeckPile();
        return Task.CompletedTask;
    }
}
