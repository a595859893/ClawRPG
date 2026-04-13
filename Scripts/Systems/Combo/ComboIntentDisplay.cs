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
        AnchorsPreset = AnchorsPreset.Custom;
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
        if (SkillComboSystem.Instance != null)
        {
            SkillComboSystem.ComboProgressUpdated += OnComboProgressUpdated;
            SkillComboSystem.ComboFailed += OnComboFailed;
            SkillComboSystem.ComboCompleted += OnComboCompleted;
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

        // REQ-184: Close the ghost因果链 — record this abandonment even if
        // ComboGhostSystem's own OnComboFailed handler has the wrong system reference.
        // This ensures "失败被记住" even when the ghost probability doesn't trigger.
        string comboId = _currentComboId;
        _RecordGhostAbandonment(comboId);

        // Fade out after duration, THEN show REQ-182 acknowledgment feedback
        _fadeTween?.Kill();
        _fadeTween = CreateTween();
        _fadeTween.TweenInterval(_failureFlashDuration);
        _fadeTween.TweenProperty(this, "modulate:a", 0f, 0.3f);
        _fadeTween.TweenCallback(Callable.From(() =>
        {
            Hide();
            _ResetColors();
            // REQ-182: After failure flash fades, show acknowledgment visual
            _ShowAcknowledgmentFeedback(comboId);
        }));
    }

    /// <summary>
    /// REQ-182: Show acknowledgment feedback after combo failure flash fades.
    /// - If ghost is active for this combo: show ghost narrative text
    /// - If no ghost: show brief rune flash ("记忆已铭刻")
    /// Both are brief (&lt;2s) and non-blocking.
    /// </summary>
    private void _ShowAcknowledgmentFeedback(string comboId)
    {
        if (string.IsNullOrEmpty(comboId)) return;

        bool ghostActive = ComboGhostSystem.Instance?.ShouldShowGhostForCombo(comboId) ?? false;

        if (ghostActive)
        {
            _ShowGhostNarrative(comboId);
        }
        else
        {
            _SpawnRuneFlash();
        }
    }

    /// <summary>
    /// REQ-182: Ghost narrative — "它记得这个招式..." text near the bottom of screen.
    /// Shows for 1.5s then fades. Triggered when ghost is currently active for this combo.
    /// </summary>
    private void _ShowGhostNarrative(string comboId)
    {
        var tree = GetTree();
        if (tree == null) return;
        var root = tree.Root;
        if (root == null) return;

        // Get ghost info for narrative
        var ghost = ComboGhostSystem.Instance?.GetCurrentGhost();
        string narrative = "它记得这个招式...";
        if (ghost != null)
        {
            // Calculate "reincarnation count" from timestamp
            var elapsed = DateTime.Now - ghost.AbandonedTimestamp;
            int cycles = Math.Max(1, (int)(elapsed.TotalMinutes / 5) + 1); // rough estimate
            narrative = $"第{cycles}次轮回，它记得这个招式...";
        }

        // Create a simple centered-bottom narrative panel
        var panel = new PanelContainer();
        panel.Name = "GhostNarrativePanel";
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.05f, 0.05f, 0.1f, 0.85f);
        style.CornerRadiusTopLeft = 6;
        style.CornerRadiusTopRight = 6;
        style.CornerRadiusBottomLeft = 6;
        style.CornerRadiusBottomRight = 6;
        style.ContentMarginLeft = 16;
        style.ContentMarginTop = 8;
        style.ContentMarginRight = 16;
        style.ContentMarginBottom = 8;
        panel.AddThemeStyleBoxOverride("panel", style);

        var label = new Label();
        label.Text = narrative;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AddThemeFontSizeOverride("font_size", 14);
        label.AddThemeColorOverride("font_color", new Color(0.75f, 0.75f, 1f, 0.9f));
        panel.AddChild(label);

        // Center horizontally, position at bottom
        var vpSize = tree.Root?.GetViewport()?.GetVisibleRect().Size ?? new Vector2(1920, 1080);
        panel.SetAnchorsPreset(Control.LayoutPreset.CenterH);
        panel.OffsetTop = (float)(vpSize.Y * 0.75);
        panel.OffsetBottom = panel.OffsetTop + 40;

        root.AddChild(panel);

        // Fade in + hold + fade out
        panel.Modulate = Colors.Transparent;
        var tw = CreateTween();
        tw.TweenProperty(panel, "modulate:a", 1f, 0.25f);
        tw.TweenInterval(1.0f);
        tw.TweenProperty(panel, "modulate:a", 0f, 0.25f);
        tw.TweenCallback(Callable.From(() =>
        {
            if (IsInstanceValid(panel)) panel.QueueFree();
        }));
    }

    /// <summary>
    /// REQ-182: Brief rune flash when ghost is NOT active — "记忆已铭刻" acknowledgment.
    /// Shows a rune symbol that flashes and fades quickly (0.7s total).
    /// </summary>
    private void _SpawnRuneFlash()
    {
        var tree = GetTree();
        if (tree == null) return;
        var root = tree.Root;
        if (root == null) return;

        var vfx = new ComboAcknowledgmentVFX();
        root.AddChild(vfx);
        vfx.TriggerRuneFlash();
    }

    /// <summary>
    /// REQ-184: Record the abandoned combo in ComboGhostSystem.
    /// Called at the end of the failure animation to close the UX因果链:
    /// 失败 → 视觉承认(符文闪过) → ghost记录.
    /// </summary>
    private void _RecordGhostAbandonment(string comboId)
    {
        if (string.IsNullOrEmpty(comboId)) return;
        if (ComboGhostSystem.Instance == null) return;

        try
        {
            var progress = SkillComboSystem.Instance?.GetPlayerProgress();
            if (progress == null) return;

            if (progress.TryGetValue(comboId, out var p) && p != null)
            {
                var allCombos = SkillComboSystem.Instance?.GetAllCombos();
                int totalSteps = 0;
                if (allCombos != null && allCombos.TryGetValue(comboId, out var comboData) && comboData != null)
                {
                    totalSteps = comboData.SkillIds?.Count ?? 0;
                }
                ComboGhostSystem.Instance.RecordAbandonedCombo(
                    comboId,
                    p.CurrentStep,
                    totalSteps,
                    AbandonmentType.WrongSkill);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[ComboIntentDisplay] REQ-184 _RecordGhostAbandonment failed: {ex.Message}");
        }
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
        if (SkillComboSystem.Instance == null) return;

        // Get active combo progress from SkillComboSystem
        var progress = SkillComboSystem.Instance.GetPlayerProgress();
        if (!progress.TryGetValue(comboId, out var activeCombo) || activeCombo == null) return;

        // Get combo definition to find expected skill sequence
        var combos = SkillComboSystem.Instance.GetAllCombos();
        if (!combos.TryGetValue(comboId, out var comboDef)) return;

        // Determine expected skill at current step
        string expectedSkill = "";
        int total = comboDef.SkillIds.Count;
        if (activeCombo.CurrentStep < total)
        {
            expectedSkill = comboDef.SkillIds[activeCombo.CurrentStep];
        }
        if (string.IsNullOrEmpty(expectedSkill))
        {
            // No active combo intent to show
            if (IsVisibleSlow())
            {
                FadeOut();
            }
            return;
        }

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

    private void OnComboCompleted(string comboId, int chainCount)
    {
        if (!_enabled) return;
        // Combo successfully completed - brief green flash then hide
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
            // REQ-183 fix: was referencing non-existent ComboSystem, corrected to SkillComboSystem
            if (SkillComboSystem.Instance != null)
            {
                SkillComboSystem.ComboProgressUpdated -= OnComboProgressUpdated;
                SkillComboSystem.ComboFailed -= OnComboFailed;
                SkillComboSystem.ComboCompleted -= OnComboCompleted;
            }
        }
    }
}
