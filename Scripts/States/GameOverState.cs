using System.Threading.Tasks;
using Godot;

public class GameOverState : BaseGameState
{
    public GameOverState(GameStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override Task EnterState()
    {
        GameManager.GameOverUI.GetNode<Label>("PanelContainer/VBoxContainer/PlayerWinLabel").Text = $"{GameManager.CurrentPlayer.Name} wins!";
        GameManager.GameOverUI.Visible = true;
        GameManager.GetTree().Paused = true;
        return Task.CompletedTask;
    }
}
