using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class DargManager : Node2D
{
    [Export] public NodePath GameManagerPath;
    [Export] public NodePath DropZonePath;
    [Export] public NodePath GameStateMachinePath;

    private GameManager _gameManager;
    private GameStateMachine _gameStateMachine;
    private DropZone _dropZone;
    private Area2D _dropZoneArea;
    private Card _currentCard;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>(GameManagerPath);
        _dropZone = GetNode<DropZone>(DropZonePath);
        _gameStateMachine = GetNode<GameStateMachine>(GameStateMachinePath);
        _dropZoneArea = _dropZone.GetNode<Area2D>("DropZoneArea");

        // 把場上所有已生成的卡片都註冊進來
        foreach (var c in GetTree().GetNodesInGroup("card").Cast<Card>())
            RegisterCard(c);

        // 如果有動態產生卡片，也可以監聽 node_added 自動註冊
        GetTree().NodeAdded += (node) =>
        {
            if (node is Card c) RegisterCard(c);
        };
    }

    private void RegisterCard(Card card)
    {
        var startedCb = new Callable(this, nameof(OnDragStarted));
        var endedCb = new Callable(this, nameof(OnDragEnded));
        if (!card.IsConnected(nameof(Card.DragStartedSignal), startedCb))
        {
            card.Connect(
                nameof(Card.DragStartedSignal),
                new Callable(this, nameof(OnDragStarted))
            );
        }

        if (!card.IsConnected(nameof(Card.DragEndedSignal), endedCb))
        {
            card.Connect(
                nameof(Card.DragEndedSignal),
                new Callable(this, nameof(OnDragEnded))
            );
        }
    }

    private void OnDragStarted(Card card)
    {
        _currentCard = card;
        // Optional: 顯示一些 UI 提示
    }

    private async void OnDragEnded(Card card)
    {
        if (card != _currentCard) return;

        if (_dropZoneArea != null)
        {
            var shapeNode = _dropZoneArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            if (shapeNode != null && shapeNode.Shape is RectangleShape2D rectShape)
            {
                var dropZonePos = _dropZoneArea.GlobalPosition;
                var dropSize = rectShape.Size;
                var halfSize = dropSize / 2;

                var dropZoneRect = new Rect2(dropZonePos - halfSize, dropSize);
                var dropZoneTopCard = _dropZone.GetTopCardInDropZone();

                if (dropZoneRect.HasPoint(card.GlobalPosition) && _dropZone.CanPlaceCard(card, dropZoneTopCard))
                {
                    dropZoneTopCard.ResetBorder();
                    card.IsInteractive = false;
                    var playerHand = card.GetParentOrNull<Player>();
                    if (playerHand == null) return;
                    await _gameManager.MoveCardToTarget(card, playerHand, _dropZone,
                        showAnimation: false);
                    await playerHand.ReorderHand();
                    await _gameStateMachine.CardEffect(card);
                    GD.Print("Card dropped in valid zone");
                    return;
                }

            }
        }

        // 沒有放到正確區域：回原位、ZIndex 還原
        GD.Print("Card dropped outside zone, returning");
        await card.CardAnimator.TweenTo(card.OriginalPosition, 0.2f);
        card.ReturnToOriginalZ();
        await Task.Delay(100); // 微延遲防止立即觸發 Hover
        card.IsInteractive = true;

        _currentCard = null;
    }


}