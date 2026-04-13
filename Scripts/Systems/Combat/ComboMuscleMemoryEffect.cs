using Godot;
using System;

/// <summary>
/// Combo Muscle Memory Effect (REQ-175)
/// 
/// When a player abandons a combo, the NEXT combat start shows a brief
/// "muscle memory flash" — a screen-edge vignette that reveals the abandoned
/// skill sequence at 30% opacity for ~0.5 seconds.
///
/// This runs EVERY combat after abandon (no RNG).
/// REQ-174's ghost is probabilistic; this flash always shows.
/// 
/// Narrative: "Your body remembers what your mind chose to abandon."
///
/// Usage: Spawn via ComboGhostSystem._SpawnMuscleMemoryFlash().
/// The effect is self-managing: _Trigger() starts it, _Cleanup() removes it.
/// </summary>
public partial class ComboMuscleMemoryEffect : Node
{
    // === REQ-175 config ===
    [Export] private float _flashDuration = 0.5f;      // seconds
    [Export] private float _maxOpacity = 0.30f;         // 30% opacity at peak
    [Export] private float _vignetteThickness = 80f;      // px from edge

    // Animation state
    private float _elapsed = 0f;
    private bool _active = false;
    private AbandonedComboEntry _entry;
    private Color _comboColor;

    // Overlay node (fullscreen, sits above everything)
    private Control _overlay;

    public override void _Ready()
    {
        // Start invisible — triggered externally via _Trigger()
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        if (!_active) return;

        _elapsed += (float)delta;

        // Fade in quickly (first 10%), hold, then fade out (last 40%)
        float fadeInEnd = _flashDuration * 0.1f;
        float fadeOutStart = _flashDuration * 0.6f;

        float currentAlpha = _maxOpacity;

        if (_elapsed < fadeInEnd)
        {
            // Fade in from 0 to maxOpacity
            currentAlpha = _maxOpacity * (_elapsed / fadeInEnd);
        }
        else if (_elapsed > fadeOutStart)
        {
            // Fade out
            float fadeProgress = (_elapsed - fadeOutStart) / (_flashDuration - fadeOutStart);
            currentAlpha = _maxOpacity * (1f - fadeProgress);
        }

        // Update overlay opacity
        if (_overlay != null && IsInstanceValid(_overlay))
        {
            _overlay.Modulate = new Color(1f, 1f, 1f, currentAlpha);
        }

        if (_elapsed >= _flashDuration)
        {
            _Cleanup();
        }
    }

    /// <summary>
    /// REQ-175: Trigger the muscle memory flash for the given abandoned combo.
    /// Called by ComboGhostSystem._SpawnMuscleMemoryFlash().
    /// </summary>
    public void _Trigger(AbandonedComboEntry entry)
    {
        if (entry == null) return;

        _entry = entry;
        _active = true;
        _elapsed = 0f;

        // Determine color based on abandonment type
        _comboColor = entry.AbandonmentType switch
        {
            AbandonmentType.Timeout => new Color(1.0f, 0.6f, 0.2f),
            AbandonmentType.WrongSkill => new Color(0.9f, 0.3f, 0.3f),
            AbandonmentType.ManualCancel => new Color(0.7f, 0.4f, 0.9f),
            AbandonmentType.Died => new Color(0.5f, 0.15f, 0.15f),
            _ => new Color(1f, 0.8f, 0.4f)
        };

        _BuildOverlay();
        _BuildSkillStrip();

        GD.Print($"[ComboMuscleMemory] Flash triggered for abandoned combo: {entry.ComboId}");
    }

    private void _BuildOverlay()
    {
        // Remove old overlay if any
        if (_overlay != null && IsInstanceValid(_overlay))
        {
            _overlay.QueueFree();
        }

        var tree = GetTree();
        if (tree == null) return;
        var root = tree.Root;
        if (root == null) return;

        // Create fullscreen overlay panel
        _overlay = new Panel();
        _overlay.Name = "ComboMuscleMemoryOverlay";
        _overlay.AnchorsPreset = Control.LayoutPreset.FullRect;
        _overlay.Modulate = new Color(1f, 1f, 1f, 0f); // starts transparent

        // Dark vignette background
        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = new Color(_comboColor.r, _comboColor.g, _comboColor.b, 1f);
        bgStyle.ContentMarginLeft = 0;
        bgStyle.ContentMarginTop = 0;
        bgStyle.ContentMarginRight = 0;
        bgStyle.ContentMarginBottom = 0;
        _overlay.AddThemeStyleBoxOverride("panel", bgStyle);

        // We'll use a texture with a hole (vignette shape).
        // Since Godot doesn't have native hole-punch textures, we cover the screen
        // with the color but punch a hole in the center using a separate approach.
        // For simplicity: cover screen with semi-transparent color at edges only.
        // We'll build this as multiple panels to create the vignette effect.
        _overlay.QueueFree(); // discard the solid overlay

        // Build 9-region vignette: 4 edges + 4 corners + center (transparent)
        _BuildVignettePanels(root);
    }

    private void _BuildVignettePanels(Node parent)
    {
        if (_overlay != null && IsInstanceValid(_overlay))
            _overlay.QueueFree();

        var viewportSize = new Vector2(1280, 720); // fallback
        var tree = GetTree();
        if (tree != null && tree.Root != null)
            viewportSize = tree.Root.GetViewport().GetVisibleRect().Size;

        float w = viewportSize.x;
        float h = viewportSize.y;
        float t = _vignetteThickness;

        // Parent control (invisible, just for organization)
        _overlay = new Control();
        _overlay.Name = "ComboMuscleMemoryOverlay";
        _overlay.AnchorsPreset = Control.LayoutPreset.FullRect;
        _overlay.Modulate = new Color(1f, 1f, 1f, _maxOpacity);
        _overlay.MouseFilter = Control.MouseFilterEnum.Ignore; // don't block input
        parent.AddChild(_overlay);

        var col = new Color(_comboColor.r, _comboColor.g, _comboColor.b, 1f);

        // Helper to make a stylebox
        StyleBoxFlat makeStyle() { var s = new StyleBoxFlat(); s.BgColor = col; return s; }

        // Top strip
        _AddVignettePanel(_overlay, "Top", new Rect2(0, 0, w, t), makeStyle());
        // Bottom strip
        _AddVignettePanel(_overlay, "Bottom", new Rect2(0, h - t, w, t), makeStyle());
        // Left strip
        _AddVignettePanel(_overlay, "Left", new Rect2(0, t, t, h - 2 * t), makeStyle());
        // Right strip
        _AddVignettePanel(_overlay, "Right", new Rect2(w - t, t, t, h - 2 * t), makeStyle());
    }

    private void _AddVignettePanel(Node parent, string name, Rect2 rect, StyleBoxFlat style)
    {
        var p = new Panel();
        p.Name = name;
        p.AnchorsPreset = Control.LayoutPreset.Custom;
        p.CustomMinimumSize = new Vector2(rect.Size.x, rect.Size.y);
        p.OffsetLeft = rect.Position.x;
        p.OffsetTop = rect.Position.y;
        p.OffsetRight = rect.Position.x + rect.Size.x;
        p.OffsetBottom = rect.Position.y + rect.Size.y;
        p.MouseFilter = Control.MouseFilterEnum.Ignore;
        p.AddThemeStyleBoxOverride("panel", style);
        parent.AddChild(p);
    }

    private void _BuildSkillStrip()
    {
        if (_entry == null) return;

        var tree = GetTree();
        if (tree == null || tree.Root == null) return;
        var viewportSize = tree.Root.GetViewport().GetVisibleRect().Size;

        var comboId = _entry.ComboId;
        var abandonedAtStep = _entry.AbandonedAtStep;

        var comboData = ComboSystem.Instance?.GetAllCombos().GetValueOrDefault(comboId);

        // Skill strip: centered at bottom above vignette
        var stripWidth = 400f;
        var stripHeight = 60f;
        float centerX = (viewportSize.x - stripWidth) / 2f;
        float stripY = viewportSize.y - _vignetteThickness - stripHeight - 24f;

        var strip = new Panel();
        strip.Name = "SkillStrip";
        strip.AnchorsPreset = Control.LayoutPreset.Custom;
        strip.CustomMinimumSize = new Vector2(stripWidth, stripHeight);
        strip.OffsetLeft = centerX;
        strip.OffsetTop = stripY;
        strip.OffsetRight = centerX + stripWidth;
        strip.OffsetBottom = stripY + stripHeight;
        strip.MouseFilter = Control.MouseFilterEnum.Ignore;

        var stripStyle = new StyleBoxFlat();
        stripStyle.BgColor = new Color(0.05f, 0.05f, 0.08f, 0.75f);
        stripStyle.CornerRadiusTopLeft = 10;
        stripStyle.CornerRadiusTopRight = 10;
        stripStyle.CornerRadiusBottomLeft = 10;
        stripStyle.CornerRadiusBottomRight = 10;
        stripStyle.ContentMarginLeft = 16;
        stripStyle.ContentMarginTop = 10;
        stripStyle.ContentMarginRight = 16;
        stripStyle.ContentMarginBottom = 10;
        strip.AddThemeStyleBoxOverride("panel", stripStyle);
        _overlay.AddChild(strip);

        var hbox = new HBoxContainer();
        hbox.Name = "HBox";
        hbox.Alignment = HBoxContainer.AlignmentMode.Center;
        hbox.AddThemeConstantOverride("separation", 8);
        strip.AddChild(hbox);

        // Show skill icons up to (not including) the abandoned step
        if (comboData?.SkillSequence != null)
        {
            int stepsToShow = System.Math.Min(abandonedAtStep, comboData.SkillSequence.Count);
            for (int i = 0; i < stepsToShow; i++)
            {
                var skillId = comboData.SkillSequence[i];
                bool isLast = (i == stepsToShow - 1);
                var icon = _MakeSkillIcon(skillId, isLast);
                hbox.AddChild(icon);
            }
        }

        // Trail indicator
        var trail = new Label();
        trail.Text = "→ ...";
        trail.AddThemeFontSizeOverride("font_size", 16);
        trail.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f, 0.7f));
        trail.VerticalAlignment = VerticalAlignment.Center;
        hbox.AddChild(trail);

        // Combo name above strip
        var nameLabel = new Label();
        nameLabel.Name = "ComboName";
        nameLabel.Text = $"[ {comboData?.ComboName ?? comboId} ]";
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AddThemeFontSizeOverride("font_size", 13);
        nameLabel.AddThemeColorOverride("font_color", new Color(_comboColor.r * 0.9f, _comboColor.g * 0.9f, _comboColor.b * 0.9f, 0.75f));
        nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;

        var namePanel = new Panel();
        namePanel.Name = "ComboNamePanel";
        namePanel.AnchorsPreset = Control.LayoutPreset.Custom;
        namePanel.CustomMinimumSize = new Vector2(stripWidth, 24f);
        namePanel.OffsetLeft = centerX;
        namePanel.OffsetTop = stripY - 26f;
        namePanel.OffsetRight = centerX + stripWidth;
        namePanel.OffsetBottom = stripY - 2f;
        namePanel.MouseFilter = Control.MouseFilterEnum.Ignore;
        var namePanelStyle = new StyleBoxFlat();
        namePanelStyle.BgColor = new Color(0f, 0f, 0f, 0f);
        namePanel.AddThemeStyleBoxOverride("panel", namePanelStyle);
        namePanel.AddChild(nameLabel);
        _overlay.AddChild(namePanel);
    }

    private Control _MakeSkillIcon(string skillId, bool isLast)
    {
        var container = new Panel();
        container.CustomMinimumSize = new Vector2(40, 40);

        var style = new StyleBoxFlat();
        style.BgColor = isLast
            ? new Color(_comboColor.r, _comboColor.g, _comboColor.b, 0.9f)
            : new Color(_comboColor.r * 0.4f, _comboColor.g * 0.4f, _comboColor.b * 0.4f, 0.35f);
        style.CornerRadiusTopLeft = 6;
        style.CornerRadiusTopRight = 6;
        style.CornerRadiusBottomLeft = 6;
        style.CornerRadiusBottomRight = 6;
        container.AddThemeStyleBoxOverride("panel", style);

        var label = new Label();
        // Truncate skill ID to 4 chars for display
        label.Text = skillId.Length > 4 ? skillId.Substring(0, 4) : skillId;
        label.AddThemeFontSizeOverride("font_size", 10);
        label.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, isLast ? 1f : 0.55f));
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        container.AddChild(label);

        return container;
    }

    private void _Cleanup()
    {
        _active = false;

        if (_overlay != null && IsInstanceValid(_overlay))
        {
            _overlay.QueueFree();
            _overlay = null;
        }

        QueueFree();
    }
}
