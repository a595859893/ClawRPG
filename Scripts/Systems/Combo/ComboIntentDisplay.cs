using Godot;
using System;

/// <summary>
/// Combo Intent Display — shows the next expected skill in the current combo (REQ-168).
/// Displays near the combo progress bar with color-coded feedback:
/// - Dim gray: not yet reached this step
/// - Bright/white: current step (player should press this)
/// - Green: successfully matched
/// - Red flash: combo failed (wrong key or timeout)
/// </summary>
public partial class ComboIntentDisplay : Control
{
    // === REQ-168 config ===
    [Export] private bool _enabled = true;
    [Export] private float _failureFlashDuration = 1.0f;

    // UI elements
    private PanelContainer _panel;
    private HBoxContainer _content;
    private Label _stepLabel;        // "Step 2/4"
    private TextureRect _skillIcon;   // Skill icon placeholder
    private Label _skillNameLabel;    // Skill name/ID
    private Label _statusLabel;       // "NEXT" / "MATCH" / "LOST"

    // State
    private string _currentComboId = "";
    private int _currentStep = 0;
    private int _totalSteps = 0;
    private string _expectedSkillId = "";
    private bool _isFailureState = false;

    // Animation
    private Tween _flashTween;
    private Tween _fadeTween;

    public override void _Ready()
    {
        _SetupUI();
        _ConnectSignals();
        Hide();
    }

    private void _SetupUI()
    {
        Name = "ComboIntentDisplay";
        AnchorsPreset = AnchorPreset.Custom;
        CustomMinimumSize = new Vector2(180, 60);

        // Position: bottom-right of screen, above other combo UI
        OffsetLeft = -200;
        OffsetTop = -200;
        OffsetRight = -20;
        OffsetBottom = -140;

        // Dark semi-transparent panel
        _panel = new PanelContainer();
        _panel.CustomMinimumSize = new Vector2(180, 60);
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.85f);
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        style.ContentMarginLeft = 10;
        style.ContentMarginTop = 8;
        style.ContentMarginRight = 10;
        style.ContentMarginBottom = 8;
        _panel.AddThemeStyleBoxOverride("panel", style);
        AddChild(_panel);

        _content = new HBoxContainer();
        _content.AddThemeConstantOverride("separation", 8);
        _panel.AddChild(_content);

        // Step indicator (e.g., "2/4")
        _stepLabel = new Label();
        _stepLabel.Text = "0/0";
        _stepLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _stepLabel.VerticalAlignment = VerticalAlignment.Center;
        _stepLabel.AddThemeFontSizeOverride("font_size", 14);
        _stepLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        _stepLabel.CustomMinimumSize = new Vector2(40, 30);
        _content.AddChild(_stepLabel);

        // Separator line
        var sep = new VSeparator();
        sep.CustomMinimumSize = new Vector2(2, 30);
        sep.Modulate = new Color(0.4f, 0.4f, 0.5f, 0.5f);
        _content.AddChild(sep);

        // Skill icon placeholder (TextureRect with a colored rect as fallback)
        _skillIcon = new TextureRect();
        _skillIcon.CustomMinimumSize = new Vector2(36, 36);
        _skillIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        // Use a colored placeholder since we don't have a skill icon atlas
        var iconPanel = new Panel();
        iconPanel.CustomMinimumSize = new Vector2(36, 36);
        var iconStyle = new StyleBoxFlat();
        iconStyle.BgColor = new Color(0.3f, 0.5f, 0.9f, 0.8f);
        iconStyle.CornerRadiusTopLeft = 6;
        iconStyle.CornerRadiusTopRight = 6;
        iconStyle.CornerRadiusBottomLeft = 6;
        iconStyle.CornerRadiusBottomRight = 6;
        iconPanel.AddThemeStyleBoxOverride("panel", iconStyle);
        // Replace _skillIcon with the panel as we don't have texture atlas
        _skillIcon.QueueFree();
        _content.AddChild(iconPanel);

        // Skill name + status
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 2);
        _content.AddChild(vbox);

        _skillNameLabel = new Label();
        _skillNameLabel.Text = "---";
        _skillNameLabel.HorizontalAlignment = HorizontalAlignment.Left;
        _skillNameLabel.AddThemeFontSizeOverride("font_size", 13);
        _skillNameLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        vbox.AddChild(_skillNameLabel);

        _statusLabel = new Label();
        _statusLabel.Text = "NEXT";
        _statusLabel.HorizontalAlignment = HorizontalAlignment.Left;
        _statusLabel.AddThemeFontSizeOverride("font_size", 11);
        _statusLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        vbox.AddChild(_statusLabel);
    }

    private void _ConnectSignals()
    {
        if (ComboSystem.Instance != null)
        {
            ComboSystem.ComboProgressUpdated += OnComboProgressUpdated;
            ComboSystem.ComboFailed += OnComboFailed;
            ComboSystem.ComboExecuted += OnComboExecuted;
        }
    }

    private void _Show(string comboId, int step, int total, string skillId, string status, Color skillColor)
    {
        if (!_enabled) return;

        _currentComboId = comboId;
        _currentStep = step;
        _totalSteps = total;
        _expectedSkillId = skillId;
        _isFailureState = false;

        _stepLabel.Text = $"{step}/{total}";
        _skillNameLabel.Text = skillId;
        _statusLabel.Text = status;
        _skillNameLabel.AddThemeColorOverride("font_color", skillColor);
        _statusLabel.AddThemeColorOverride("font_color", skillColor);

        Show();
        Modulate = Colors.White;
    }

    private void _ShowFailure()
    {
        if (!_enabled) return;
        _isFailureState = true;
        _statusLabel.Text = "LOST";
        _statusLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.3f));
        _skillNameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.3f));

        // Red flash animation
        _flashTween?.Kill();
        _flashTween = CreateTween();
        _flashTween.TweenProperty(this, "modulate", new Color(1f, 0.3f, 0.3f, 1f), 0.1f);
        _flashTween.TweenProperty(this, "modulate", new Color(1f, 0.6f, 0.6f, 1f), 0.1f);
        _flashTween.TweenProperty(this, "modulate", new Color(1f, 0.3f, 0.3f, 1f), 0.1f);
        _flashTween.TweenProperty(this, "modulate", new Color(1f, 0.6f, 0.6f, 1f), 0.1f);

        // Fade out after duration
        _fadeTween?.Kill();
        _fadeTween = CreateTween();
        _fadeTween.TweenInterval(_failureFlashDuration);
        _fadeTween.TweenProperty(this, "modulate:a", 0f, 0.3f);
        _fadeTween.TweenCallback(Callable.From(() =>
        {
            Hide();
            _ResetColors();
        }));
    }

    private void _ResetColors()
    {
        _statusLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        _skillNameLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
    }

    private void OnComboProgressUpdated(string comboId, int currentStep, float timeRemaining)
    {
        if (!_enabled) return;

        // Get combo data to know total steps and expected skill
        if (ComboSystem.Instance == null) return;

        string expectedSkill = ComboSystem.Instance.GetExpectedSkill(comboId);
        if (expectedSkill == null)
        {
            // No active combo intent to show
            if (IsVisibleSlow())
            {
                FadeOut();
            }
            return;
        }

        // Get total steps from the combo
        var allCombos = ComboSystem.Instance.GetType()
            .GetField("_combos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // Fallback: just show step info
        int total = currentStep + 1; // approximate

        // Determine display state based on step progress
        if (currentStep == 0)
        {
            // Just started, show first skill expectation
            _Show(comboId, 1, total, expectedSkill, "NEXT", new Color(1f, 0.9f, 0.4f));
        }
        else
        {
            // In progress - show next expected
            _Show(comboId, currentStep + 1, total, expectedSkill, "NEXT", new Color(1f, 0.85f, 0.3f));
        }
    }

    private void OnComboFailed(string comboId)
    {
        if (!_enabled) return;
        _ShowFailure();
    }

    private void OnComboExecuted(string comboId, float damage, string description)
    {
        if (!_enabled) return;
        // Combo successfully executed - brief green flash then hide
        _statusLabel.Text = "DONE!";
        _statusLabel.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.4f));
        _skillNameLabel.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.4f));

        _fadeTween?.Kill();
        _fadeTween = CreateTween();
        _fadeTween.TweenInterval(1.5f);
        _fadeTween.TweenProperty(this, "modulate:a", 0f, 0.3f);
        _fadeTween.TweenCallback(Callable.From(() =>
        {
            Hide();
            _ResetColors();
        }));
    }

    private void FadeOut()
    {
        _fadeTween?.Kill();
        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(this, "modulate:a", 0f, 0.3f);
        _fadeTween.TweenCallback(Callable.From(() =>
        {
            Hide();
            _ResetColors();
        }));
    }

    private bool IsVisibleSlow()
    {
        return IsVisibleInTree();
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        if (!enabled) Hide();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            if (ComboSystem.Instance != null)
            {
                ComboSystem.ComboProgressUpdated -= OnComboProgressUpdated;
                ComboSystem.ComboFailed -= OnComboFailed;
                ComboSystem.ComboExecuted -= OnComboExecuted;
            }
        }
    }
}
