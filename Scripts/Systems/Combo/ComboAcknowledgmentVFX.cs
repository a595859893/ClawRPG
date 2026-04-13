using Godot;
using System;

/// <summary>
/// REQ-182: Combo Acknowledgment VFX — brief rune flash for "记忆已铭刻" moment.
/// 
/// When a combo fails and no ghost appears, a brief rune symbol flashes
/// in the center of the screen to acknowledge "这次失败 was remembered".
/// 
/// Visual: A single rune symbol (✧) in ghostly purple, fades in/out quickly.
/// Duration: ~0.7s total (non-blocking, non-intrusive).
/// </summary>
public partial class ComboAcknowledgmentVFX : Control
{
    private Label _runeLabel;
    private Tween _tween;

    public override void _Ready()
    {
        Name = "ComboAcknowledgmentVFX";
        AnchorsPreset = Control.AnchorsPreset.Center;
        OffsetLeft = -40;
        OffsetTop = -40;
        OffsetRight = 40;
        OffsetBottom = 40;
        MouseFilter = Control.MouseFilterEnum.Ignore;

        // Semi-transparent dark backing
        var backing = new Panel();
        backing.CustomMinimumSize = new Vector2(80, 80);
        var backingStyle = new StyleBoxFlat();
        backingStyle.BgColor = new Color(0.05f, 0.03f, 0.12f, 0.6f);
        backingStyle.CornerRadiusTopLeft = 40;
        backingStyle.CornerRadiusTopRight = 40;
        backingStyle.CornerRadiusBottomLeft = 40;
        backingStyle.CornerRadiusBottomRight = 40;
        backing.AddThemeStyleBoxOverride("panel", backingStyle);
        AddChild(backing);

        // Rune label
        _runeLabel = new Label();
        _runeLabel.Text = "✧";
        _runeLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _runeLabel.VerticalAlignment = VerticalAlignment.Center;
        _runeLabel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _runeLabel.AddThemeFontSizeOverride("font_size", 36);
        // Ghostly purple color
        _runeLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.4f, 1f, 0f));
        AddChild(_runeLabel);

        Hide();
    }

    /// <summary>
    /// Trigger the rune flash: scale in + color fade in + hold + fade out.
    /// Total duration: ~0.7s
    /// </summary>
    public void TriggerRuneFlash()
    {
        Show();
        Scale = new Vector2(0.3f, 0.3f);
        Modulate = Colors.White;
        _runeLabel.Modulate = new Color(0.6f, 0.4f, 1f, 0f);

        _tween?.Kill();
        _tween = CreateTween();
        _tween.SetParallel(true);

        // Scale bounce in
        _tween.TweenProperty(this, "scale", new Vector2(1.1f, 1.1f), 0.15f)
            .SetTrans(Tween.TransitionType.Back);
        // Color fade in
        _tween.TweenProperty(_runeLabel, "modulate:a", 1f, 0.15f);
        // Scale settle
        _tween.TweenProperty(this, "scale", new Vector2(1f, 1f), 0.1f)
            .SetTrans(Tween.TransitionType.Back);

        // Hold briefly
        _tween.TweenInterval(0.2f);

        // Fade out
        _tween.TweenProperty(this, "modulate:a", 0f, 0.25f);
        _tween.TweenProperty(_runeLabel, "modulate:a", 0f, 0.25f);

        _tween.TweenCallback(Callable.From(() =>
        {
            if (IsInstanceValid(this)) QueueFree();
        }));
    }
}
