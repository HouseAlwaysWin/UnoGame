using Godot;
using System.Threading.Tasks;

public partial class CardAnimator : Node
{
    private Card _card;
    private bool _isAnimating = false;
    private Tween _activeTween;
    public override void _Ready()
    {
        _card = GetParent<Card>();
    }

    public async Task HoverUp(float offsetY = -50, float duration = 0.15f)
    {
        if (_activeTween != null && _activeTween.IsRunning()) return;
        await TweenTo(_card.OriginalPosition + new Vector2(0, offsetY), duration);
    }

    public async Task HoverDown(float duration = 0.15f)
    {
        await TweenTo(_card.OriginalPosition, duration);
        _card.ReturnToOriginalZ();
    }

    public async Task TweenTo(Vector2 target, float duration)
    {
        _activeTween?.Kill();
        _activeTween = CreateTween();
        _activeTween.TweenProperty(_card, "global_position", target, duration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        await ToSignal(_activeTween, "finished");
    }


    public async Task Shake()
    {
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(_card, "rotation_degrees", 2f, 0.1)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(_card, "rotation_degrees", -2f, 0.2)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(_card, "rotation_degrees", 0f, 0.1)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        await ToSignal(tween, "finished");
    }
}