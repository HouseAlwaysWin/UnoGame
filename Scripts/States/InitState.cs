using System.Threading.Tasks;
using Godot;

public class InitState : BaseGameState
{
    public InitState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override Task EnterState()
    {
        // 1. 初始化牌庫
        StateMachine.InitDeck();

        // 2. 取得隨機種子：改用 OS.GetTicksMsec()
        int seed = (int)Time.GetTicksMsec();

        // 3. 洗牌
        StateMachine.ShuffleDeck(GameManager.Deck, seed);

        // 4. 如果這個節點 (gameManager) 所在的遊戲是 server，就發 RPC 告訴所有客戶端用同樣的 seed 也洗一次
        //    注意：GameManager 繼承自 Node，可以透過它存取 MultiplayerAPI
        if (GameManager.Multiplayer.IsServer())
        {
            GameManager.Rpc(nameof(GameManager.RpcShuffleDeck), seed);
        }

        // 5. 顯示牌堆
        StateMachine.DisplayDeckPile();

        return Task.CompletedTask;
    }
}
