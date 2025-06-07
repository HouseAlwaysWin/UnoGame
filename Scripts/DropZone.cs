using Godot;
using System;
using System.Threading.Tasks;

public partial class DropZone : Node2D
{
    private Area2D _dropZoneArea;
    private GameManager _gameManager;

    public override void _Ready()
    {
        _dropZoneArea = GetNode<Area2D>("DropZoneArea");
        _gameManager = GetParentOrNull<GameManager>();
        // _dropZoneArea.AreaEntered += OnAreaEntered;
    }

    private  void OnAreaEntered(Node2D body)
    {
        if (body is Card card && card.IsDragging)
        {
            // // ❗確保只有當前滑鼠下最上層的卡能觸發拖曳
            // if (_gameManager.GetCardUnderMouse() != card)
            //     return;
            // card.IsDragging = false;
            // // 3. 取得目前棄牌堆最上層的卡
            // var topCard = GetTopCardInDropZone(this.GetParent<Node2D>());
            //
            // // 4. 用規則服務判斷是否合法（把原先 Card.HandleDrop 裡的 IsPlayable 邏輯抽出）
            // if (CanPlaceCard(card, topCard))
            // {
            //     card.IsInteractive = false;
            //     var playerHand = card.GetParentOrNull<Player>();
            //     if (playerHand == null) return;
            //     await _gameManager.MoveCardToTarget(card, playerHand, _gameManager.DropZonePileNode,
            //         showAnimation: false);
            //     await playerHand.ReorderHand();
            // }
            // else
            // {
            //     _gameManager.ClearDraggedCard(card);
            //     card.ReturnToOriginalZ();
            // }

            var shapeNode = _dropZoneArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            if (shapeNode != null && shapeNode.Shape is RectangleShape2D rectShape)
            {
                var dropZonePos = _dropZoneArea.GlobalPosition;
                var dropSize = rectShape.Size;
                var halfSize = dropSize / 2;

                var dropZoneRect = new Rect2(dropZonePos - halfSize, dropSize);
                var topCard = GetTopCardInDropZone();

                if (dropZoneRect.HasPoint(GlobalPosition) && CanPlaceCard(card, topCard))
                {
                    card.IsInteractive = false;
                    var playerHand = card.GetParentOrNull<Player>();
                    if (playerHand == null) return;
                     _gameManager.MoveCardToTarget(card, playerHand, this,
                        showAnimation: false);
                     playerHand.ReorderHand();

                    GD.Print("Card dropped in valid zone");
                    return;
                }
            }
            else
            {
                card.IsDragging = false;
                card.IsInteractive = false;
                _gameManager.ClearDraggedCard(card);
                card.ReturnToOriginalZ();
            }
        }
    }


    public  bool CanPlaceCard(Card card, Card topCard)
    {
        if (card.CardColor == CardColor.Wild || topCard.CardColor == CardColor.Wild)
            return true;
        return card.CardColor == topCard.CardColor
               || (card.CardType == topCard.CardType && card.Number == topCard.Number);
    }

    public Card GetTopCardInDropZone()
    {
        Card topCard = null;
        int maxZ = int.MinValue;

        foreach (var child in this.GetChildren())
        {
            if (child is Card card)
            {
                if (card.ZIndex > maxZ)
                {
                    maxZ = card.ZIndex;
                    topCard = card;
                }
            }
        }

        return topCard;
    }

    private async void HandleDrop(Card card)
    {
        if (_dropZoneArea != null)
        {
            var shapeNode = _dropZoneArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            if (shapeNode != null && shapeNode.Shape is RectangleShape2D rectShape)
            {
                var dropZonePos = _dropZoneArea.GlobalPosition;
                var dropSize = rectShape.Size;
                var halfSize = dropSize / 2;

                var dropZoneRect = new Rect2(dropZonePos - halfSize, dropSize);
                var topCard = GetTopCardInDropZone();
                if (dropZoneRect.HasPoint(GlobalPosition) && CanPlaceCard(card, topCard))
                {
                    card.IsInteractive = false;
                    // PlayerHand.RemoveCard(this);
                    // playerHand.RemoveChild(this);
                    // _dropZoneNode.AddChild(this);
                    // Position = _dropZoneNode.ToLocal(dropZonePos);

                    var playerHand = card.GetParentOrNull<Player>();
                    if (playerHand == null) return;
                    await _gameManager.MoveCardToTarget(card, playerHand, _gameManager.DropZonePileNode,
                        showAnimation: false);
                    await playerHand.ReorderHand();

                    GD.Print("Card dropped in valid zone");
                    return;
                }
            }
        }

        // 沒有放到正確區域：回原位、ZIndex 還原
        GD.Print("Card dropped outside zone, returning");
        // await _animator.TweenTo(this.OriginalPosition, 0.2f);
        // await _gameManager.MoveCardToTarget(card, playerHand, _gameManager.DropZonePileNode,
        //     showAnimation: true,showCard:true);
        card.ReturnToOriginalZ();
        await Task.Delay(100); // 微延遲防止立即觸發 Hover
        card.IsInteractive = true;
    }
}