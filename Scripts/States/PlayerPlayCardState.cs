using System.Threading.Tasks;
using Godot;

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
                int index = playerHand.GetPlayerHandCards().IndexOf(card);
                bool animate = GameManager.CurrentPlayer != GameManager.MyPlayer;

                Node2D fromNode = GameManager.CurrentPlayer == GameManager.MyPlayer
                    ? playerHand
                    : GameManager.PlayerZone;

                await GameManager.MoveCardToTarget(card, fromNode, GameManager.DropZonePileNode,
                    showAnimation: animate);

                if (Multiplayer.IsServer())
                    GameManager.Rpc(nameof(GameManager.RpcPlayCard), playerHand.PlayerSeqNo, index);

                await playerHand.ReorderHand();
            }
        }

        await StateMachine.ChangeState(GameState.ResolveEffect);
    }
}
