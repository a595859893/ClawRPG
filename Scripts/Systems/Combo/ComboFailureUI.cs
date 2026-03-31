using Godot;
using System;

/// <summary>
/// Combo Failure UI — displays the "COMBO LOST" text with animations (REQ-171).
/// 
/// Visual design:
/// - Full-screen bottom-edge "COMBO LOST" label with impact font styling
/// - Color: red-orange gradient or solid warning red
/// - Animation: slam in from top → hold → fade out
/// - Accompanies Engine.TimeScale slowdown for visceral impact
/// </summary>
public partial class ComboFailureUI : Control
{
    // UI elements
    private Label _lostLabel;
    private PanelContainer _panel;
    private Tween _tween;

    // Animation timing constants
    private const float SLAM_DURATION = 0.15f;
    private const float HOLD_RATIO = 0.6f;   // 60% of total duration is hold
    private const float FADE_RATIO = 0.25f; // 25% is fade out

    public override void _Ready()
    {
        Name = "ComboFailureUI";
        AnchorsPreset = Control.AnchorPreset.FullRect;
        ZIndex = 100;

        _BuildUI();
    }

    private void _BuildUI()
    {
        // Semi-transparent dark vignette at bottom of screen
        var vignette = new PanelContainer();
        vignette.Name = "Vignette";
        vignette.AnchorsPreset = Control.AnchorPreset.BottomWide;
        vignette.OffsetLeft = 0;
        vignette.OffsetRight = 0;
        vignette.OffsetTop = -120;
        vignette.OffsetBottom = 0;

        var vignetteStyle = new StyleBoxFlat();
        vignetteStyle.BgColor = new Color(0.6f, 0.1f, 0.05f, 0.0f); // transparent start
        vignetteStyle.CornerRadiusTopLeft = 0;
        vignetteStyle.CornerRadiusTopRight = 0;
        vignette.AddThemeStyleBoxOverride("panel", vignetteStyle);
        AddChild(vignette);

        // "COMBO LOST" label
        _lostLabel = new Label();
        _lostLabel.Name = "LostLabel";
        _lostLabel.Text = "COMBO LOST";
        _lostLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _lostLabel.VerticalAlignment = VerticalAlignment.Bottom;
        _lostLabel.Position = new Vector2(0, -100);
        _lostLabel.Size = new Vector2(1920, 80); // large for fullHD reference
        _lostLabel.AddThemeFontSizeOverride("font_size", 72);
        _lostLabel.Modulate = new Color(1f, 1f, 1f, 0f); // invisible initially

        // Use a bold red color
        _lostLabel.AddThemeColorOverride("font_color", new Color(1f, 0.2f, 0.1f, 1f));

        vignette.AddChild(_lostLabel);

        _panel = vignette;
    }

    /// <summary>
    /// Trigger the COMBO LOST display animation.
    /// </summary>
    /// <param name="totalDuration">Total visible time in seconds</param>
    public void ShowFailure(float totalDuration)
    {
        Visible = true;
        Modulate = new Color(1f, 1f, 1f, 1f);

        _tween?.kill();

        float holdDuration = totalDuration * HOLD_RATIO;
        float fadeDuration = totalDuration * FADE_RATIO;

        // === Phase 1: Slam in (scale 1.5 → 1.0 + fade in) ===
        _lostLabel.Modulate = new Color(1f, 1f, 1f, 0f);
        _lostLabel.Scale = new Vector2(1.5f, 1.5f);

        _tween = CreateTween();
        _tween.SetParallel(true);

        _tween.TweenProperty(_lostLabel, "modulate:a", 1f, SLAM_DURATION)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        _tween.TweenProperty(_lostLabel, "scale", new Vector2(1f, 1f), SLAM_DURATION)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);

        // === Phase 2: Hold (then fade out) ===
        _tween.TweenInterval(holdDuration);

        _tween.TweenProperty(_lostLabel, "modulate:a", 0f, fadeDuration)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Linear);

        _tween.Chain().TweenCallback(Callable.From(Hide));
    }

    public override void _ExitTree()
    {
        _tween?.kill();
    }
}
