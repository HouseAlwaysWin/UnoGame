using System.Threading.Tasks;

public class PlayerPlayCardState : BaseGameState
{
    public PlayerPlayCardState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override async Task EnterState()
    {
        var card = StateMachine.CurrentPlayedCard;
        if (card != null)
        {
            var playerHand = GameManager.CurrentPlayer;
            if (card.GetParent() != GameManager.DropZonePileNode)
            {
                bool animate = GameManager.CurrentPlayer != GameManager.MyPlayer;
                await GameManager.MoveCardToTarget(card, playerHand, GameManager.DropZonePileNode, showAnimation: animate);
                await playerHand.ReorderHand();
            }
        }

        await StateMachine.ChangeState(GameState.ResolveEffect);
    }
}
