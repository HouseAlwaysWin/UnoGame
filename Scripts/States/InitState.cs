using System.Threading.Tasks;

public class InitState : BaseGameState
{
    public InitState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override Task EnterState()
    {
        StateMachine.InitDeck();
        int seed = (int)Time.GetTicksMsec();
        StateMachine.ShuffleDeck(GameManager.Deck, seed);
        if (Multiplayer.IsServer())
            GameManager.Rpc(nameof(GameManager.RpcShuffleDeck), seed);
        StateMachine.DisplayDeckPile();
        return Task.CompletedTask;
    }
}
