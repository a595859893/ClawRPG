using Godot;
using System;

/// <summary>
/// REQ-181: Ghost Teaching Hint Panel
///
/// When a ghost appears and the player has unlocked teaching mode for that combo,
/// this panel briefly shows the next expected skill — a "preview" from their past self.
///
/// - Lv.1 (3 completions): 0.5s preview
/// - Lv.2 (7 completions): 0.8s preview + skill name
/// - Lv.3 (15 completions): 1.0s preview + skill name + combo name
///
/// Narrative: "Your past self is guiding you."
///
/// Usage: Spawned by ComboGhostSystem.OnTeachingHintRequested signal.
/// Self-managing: auto-cleans up after display duration.
/// </summary>
public partial class GhostTeachingHintPanel : Control
{
    // Teaching level colors
    private static readonly Color LV1_COLOR = new Color(0.60f, 0.50f, 0.90f, 0.85f); // Soft violet
    private static readonly Color LV2_COLOR = new Color(0.80f, 0.60f, 0.30f, 0.85f); // Warm amber
    private static readonly Color LV3_COLOR = new Color(0.30f, 0.85f, 0.70f, 0.85f); // Bright teal

    // Config
    [Export] private float _displayDuration = 0.5f;
    [Export] private float _fadeInRatio = 0.15f;   // 15% of duration for fade in
    [Export] private float _fadeOutRatio = 0.30f;  // 30% of duration for fade out

    // State
    private float _elapsed = 0f;
    private bool _active = false;
    private int _level = 0;
    private string _comboId = "";
    private string _skillName = "";

    // UI elements
    private PanelContainer _panel;
    private HBoxContainer _content;
    private Label _levelLabel;    // "👻 Lv.2"
    private Label _skillLabel;   // "← 烈焰斩"
    private Label _comboNameLabel; // "combo_001" (only Lv.3)
    private TextureRect _divider;
    private Panel _countdownBar; // shrinking bar at bottom of panel

    // Countdown bar
    private float _barMaxWidth;

    /// <summary>
    /// REQ-181: Trigger the teaching hint display.
    /// Called by ComboGhostSystem via OnTeachingHintRequested signal.
    /// </summary>
    public void ShowHint(string comboId, int level, string skillName, float duration)
    {
        if (level <= 0 || duration <= 0f) return;

        _comboId = comboId;
        _level = level;
        _skillName = string.IsNullOrEmpty(skillName) ? comboId : skillName;
        _displayDuration = duration;
        _elapsed = 0f;
        _active = true;

        // Ensure we're visible and in tree
        if (!IsInsideTree())
        {
            var tree = GetTree();
            if (tree != null)
            {
                tree.Root.AddChild(this);
            }
        }

        _BuildUI();
        GD.Print($"[GhostTeachingHint] Showing Lv.{level} hint: {skillName}, duration={duration}s");
    }

    public override void _Ready()
    {
        // Start hidden — triggered via ShowHint()
        Visible = false;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        if (!_active) return;

        _elapsed += (float)delta;

        float fadeInEnd = _displayDuration * _fadeInRatio;
        float fadeOutStart = _displayDuration * (1f - _fadeOutRatio);

        // Calculate current opacity
        float alpha = 1f;
        if (_elapsed < fadeInEnd)
        {
            alpha = _elapsed / fadeInEnd;
        }
        else if (_elapsed > fadeOutStart)
        {
            alpha = 1f - ((_elapsed - fadeOutStart) / (_displayDuration - fadeOutStart));
        }
        alpha = Mathf.Clamp(alpha, 0f, 1f);

        // Update panel opacity
        if (_panel != null && IsInstanceValid(_panel))
        {
            _panel.Modulate = new Color(1f, 1f, 1f, alpha);
        }

        // Update countdown bar
        if (_countdownBar != null && IsInstanceValid(_countdownBar))
        {
            float progress = 1f - (_elapsed / _displayDuration);
            float newWidth = _barMaxWidth * Mathf.Max(progress, 0f);
            _countdownBar.CustomMinimumSize = new Vector2(newWidth, 3f);
        }

        if (_elapsed >= _displayDuration)
        {
            _Cleanup();
        }
    }

    private void _BuildUI()
    {
        // Clear any existing children
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }

        Visible = true;
        AnchorsPreset = AnchorsPreset.Custom;
        MouseFilter = MouseFilterEnum.Ignore;

        // Position: top-left corner, below the ghost/player HUD area
        OffsetLeft = 20;
        OffsetTop = 80;
        OffsetRight = 280;
        OffsetBottom = 140;
        CustomMinimumSize = new Vector2(0, 60);

        // Determine color by level
        Color accentColor = _level switch
        {
            3 => LV3_COLOR,
            2 => LV2_COLOR,
            _ => LV1_COLOR
        };

        // Main panel
        _panel = new PanelContainer();
        _panel.Modulate = new Color(1f, 1f, 1f, 0f); // starts transparent
        AddChild(_panel);

        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.05f, 0.03f, 0.12f, 0.90f);
        panelStyle.BorderColorLeft = new Color(accentColor.r, accentColor.g, accentColor.b, 0.7f);
        panelStyle.BorderColorRight = new Color(accentColor.r, accentColor.g, accentColor.b, 0.7f);
        panelStyle.BorderColorTop = new Color(accentColor.r, accentColor.g, accentColor.b, 0.7f);
        panelStyle.BorderColorBottom = new Color(accentColor.r, accentColor.g, accentColor.b, 0.7f);
        panelStyle.BorderWidthLeft = 1;
        panelStyle.BorderWidthRight = 1;
        panelStyle.BorderWidthTop = 1;
        panelStyle.BorderWidthBottom = 1;
        panelStyle.CornerRadiusTopLeft = 6;
        panelStyle.CornerRadiusTopRight = 6;
        panelStyle.CornerRadiusBottomLeft = 6;
        panelStyle.CornerRadiusBottomRight = 6;
        panelStyle.ContentMarginLeft = 12;
        panelStyle.ContentMarginTop = 8;
        panelStyle.ContentMarginRight = 12;
        panelStyle.ContentMarginBottom = 16; // extra bottom for countdown bar
        _panel.AddThemeStyleBoxOverride("panel", panelStyle);

        // Content row
        _content = new HBoxContainer();
        _content.AddThemeConstantOverride("separation", 8);
        _panel.AddChild(_content);

        // Ghost icon + level
        _levelLabel = new Label
        {
            Text = $"👻 Lv.{_level}",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _levelLabel.AddThemeFontSizeOverride("font_size", 13);
        _levelLabel.AddThemeColorOverride("font_color", new Color(accentColor.r, accentColor.g, accentColor.b, 1f));
        _content.AddChild(_levelLabel);

        // Divider
        _divider = new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.SizeAndTexture,
            CustomMinimumSize = new Vector2(1, 20),
            Modulate = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f)
        };
        _content.AddChild(_divider);

        // Skill hint arrow + name
        _skillLabel = new Label
        {
            Text = $"← {_skillName}",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _skillLabel.AddThemeFontSizeOverride("font_size", 14);
        _skillLabel.AddThemeColorOverride("font_color", new Color(0.90f, 0.88f, 1.0f, 1f));
        _content.AddChild(_skillLabel);

        // Lv.3: show combo name as well
        if (_level >= 3 && !string.IsNullOrEmpty(_comboId))
        {
            _comboNameLabel = new Label
            {
                Text = $" [{_comboId}]",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            _comboNameLabel.AddThemeFontSizeOverride("font_size", 11);
            _comboNameLabel.AddThemeColorOverride("font_color", new Color(accentColor.r, accentColor.g, accentColor.b, 0.7f));
            _content.AddChild(_comboNameLabel);
        }

        // Countdown bar (bottom of panel, outside content HBox)
        _countdownBar = new Panel
        {
            Name = "CountdownBar"
        };
        _countdownBar.ZIndex = 10;

        // We'll add it to the panel directly (below content)
        var barContainer = new Control { CustomMinimumSize = new Vector2(0, 3) };
        _panel.AddChild(barContainer);
        barContainer.AddChild(_countdownBar);

        // Position bar at bottom
        _countdownBar.AnchorsPreset = Control.LayoutPreset.HorizontalStretch;
        _countdownBar.AnchorLeft = 0;
        _countdownBar.AnchorRight = 1;
        _countdownBar.OffsetTop = 0;
        _countdownBar.OffsetBottom = 3;
        _barMaxWidth = 256f; // will be updated when we know the actual panel width

        var barStyle = new StyleBoxFlat
        {
            BgColor = accentColor,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0
        };
        _countdownBar.AddThemeStyleBoxOverride("panel", barStyle);

        // Subscribe to resize to track bar width
        Resized += () =>
        {
            if (_countdownBar != null && IsInstanceValid(_countdownBar))
                _barMaxWidth = _panel.CustomMinimumSize.X > 0 ? _panel.CustomMinimumSize.X : 256f;
        };

        _barMaxWidth = _panel.CustomMinimumSize.X > 0 ? _panel.CustomMinimumSize.X : 256f;
    }

    private void _Cleanup()
    {
        _active = false;
        Visible = false;

        if (IsInsideTree())
        {
            QueueFree();
        }
    }
}
