using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Combo Fatigue Panel (REQ-179)
/// 
/// Displays the current fatigue level of the active pet's combo as star indicators.
/// Shows ★☆☆ / ★★☆ / ★★★ based on how fatigued the current combo is.
/// 
/// Fatigue levels:
/// - ★☆☆ (0-33%): Combo is fresh, no fatigue
/// - ★★☆ (34-66%): Combo is getting fatigued, reduced effectiveness
/// - ★★★ (67-100%): Combo is highly fatigued, significantly reduced damage
/// 
/// Integration:
/// - Created in CombatPrepScreen or PetCombatCompanionUI
/// - Queries ComboFatigueSystem.Instance.GetFatigueStars() for display
/// - Subscribes to OnFatigueChanged for real-time updates
/// </summary>
public partial class ComboFatiguePanel : Control
{
    // UI Elements
    private HBoxContainer _container;
    private Label _fatigueLabel;
    private Label _starsLabel;
    private Label _multiplierLabel;
    private PanelContainer _warningPanel;
    private Label _warningLabel;

    // State
    private string _currentPetId = "";
    private float _currentFatigue = 0f;

    // Colors
    private readonly Color ColorFresh = new Color(0.4f, 0.9f, 0.4f);      // Green - ★★☆
    private readonly Color ColorModerate = new Color(0.9f, 0.7f, 0.2f);  // Amber - ★★☆
    private readonly Color ColorHigh = new Color(0.9f, 0.3f, 0.2f);     // Red - ★★★
    private readonly Color ColorDefault = new Color(0.7f, 0.7f, 0.7f);   // Gray - ★☆☆

    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        RefreshDisplay();
    }

    private void SetupUI()
    {
        Name = "ComboFatiguePanel";
        AnchorsPreset = AnchorsPreset.Custom;
        CustomMinimumSize = new Vector2(200, 50);

        // Main container
        _container = new HBoxContainer();
        _container.AddThemeConstantOverride("separation", 8);
        AddChild(_container);

        // Label: "疲劳:"
        _fatigueLabel = new Label
        {
            Text = "疲劳:",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _fatigueLabel.AddThemeFontSizeOverride("font_size", 13);
        _fatigueLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        _fatigueLabel.CustomMinimumSize = new Vector2(40, 20);
        _container.AddChild(_fatigueLabel);

        // Stars display
        _starsLabel = new Label
        {
            Text = "☆☆☆",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _starsLabel.AddThemeFontSizeOverride("font_size", 16);
        _starsLabel.AddThemeColorOverride("font_color", ColorDefault);
        _starsLabel.CustomMinimumSize = new Vector2(50, 20);
        _container.AddChild(_starsLabel);

        // Damage multiplier display
        _multiplierLabel = new Label
        {
            Text = "(100%)",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        _multiplierLabel.AddThemeFontSizeOverride("font_size", 11);
        _multiplierLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        _multiplierLabel.CustomMinimumSize = new Vector2(50, 20);
        _container.AddChild(_multiplierLabel);

        // Spacer
        _container.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });

        // Warning panel (shown for high fatigue)
        _warningPanel = new PanelContainer
        {
            Visible = false
        };
        _warningPanel.CustomMinimumSize = new Vector2(0, 24);

        var warningStyle = new StyleBoxFlat();
        warningStyle.BgColor = new Color(0.6f, 0.2f, 0.1f, 0.8f);
        warningStyle.CornerRadiusTopLeft = 4;
        warningStyle.CornerRadiusTopRight = 4;
        warningStyle.CornerRadiusBottomLeft = 4;
        warningStyle.CornerRadiusBottomRight = 4;
        warningStyle.ContentMarginLeft = 8;
        warningStyle.ContentMarginTop = 4;
        warningStyle.ContentMarginRight = 8;
        warningStyle.ContentMarginBottom = 4;
        _warningPanel.AddThemeStyleBoxOverride("panel", warningStyle);

        _warningLabel = new Label
        {
            Text = "⚠ 高疲劳",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _warningLabel.AddThemeFontSizeOverride("font_size", 11);
        _warningPanel.AddChild(_warningLabel);
        _container.AddChild(_warningPanel);
    }

    private void ConnectSignals()
    {
        // Subscribe to fatigue changes from ComboFatigueSystem
        try
        {
            if (ComboFatigueSystem.Instance != null)
            {
                ComboFatigueSystem.OnFatigueChanged += OnFatigueChanged;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[ComboFatiguePanel] Failed to connect to ComboFatigueSystem: {ex.Message}");
        }

        // Subscribe to pet changes (from PetCombatCompanionSystem if available)
        try
        {
            var companionType = Type.GetType("PetCombatCompanionSystem, ClawRPG");
            if (companionType != null)
            {
                var instanceProp = companionType.GetProperty("Instance");
                if (instanceProp != null)
                {
                    var instance = instanceProp.GetValue(null) as Godot.Node;
                    if (instance != null)
                    {
                        // Try to subscribe to pet changed signals if they exist
                        var signalDelegate = instance.GetType().GetMethod("add_PetChanged");
                        if (signalDelegate != null)
                        {
                            // Signal-based connection would go here
                        }
                    }
                }
            }
        }
        catch
        {
            // Pet system not available
        }
    }

    private void OnFatigueChanged(string petId, float newFatigue)
    {
        if (string.IsNullOrEmpty(_currentPetId) || petId == _currentPetId)
        {
            _currentFatigue = newFatigue;
            RefreshDisplay();
        }
    }

    /// <summary>
    /// Refresh the fatigue display based on current state.
    /// Called on init and when pet changes.
    /// </summary>
    public void RefreshDisplay()
    {
        if (!IsInstanceValid(this))
            return;

        // Get current fatigue from system
        if (ComboFatigueSystem.Instance != null)
        {
            _currentFatigue = ComboFatigueSystem.Instance.GetFatigueLevel(_currentPetId);
        }

        // Update stars display
        string stars = ComboFatigueSystem.Instance?.FatigueToStars(_currentFatigue) ?? "☆☆☆";
        _starsLabel.Text = stars;

        // Update stars color based on fatigue level
        Color starColor;
        if (_currentFatigue < 0.33f)
        {
            starColor = ColorFresh;
        }
        else if (_currentFatigue < 0.66f)
        {
            starColor = ColorModerate;
        }
        else
        {
            starColor = ColorHigh;
        }
        _starsLabel.AddThemeColorOverride("font_color", starColor);

        // Update damage multiplier display
        float multiplier = 1.0f - (_currentFatigue * 0.5f); // 1.0 at 0% fatigue, 0.5 at 100%
        int multiplierPercent = (int)(multiplier * 100);
        _multiplierLabel.Text = $"({multiplierPercent}%)";

        // Update multiplier color
        Color multColor;
        if (multiplierPercent >= 85)
            multColor = new Color(0.4f, 0.9f, 0.4f);
        else if (multiplierPercent >= 70)
            multColor = new Color(0.9f, 0.7f, 0.2f);
        else
            multColor = new Color(0.9f, 0.3f, 0.2f);
        _multiplierLabel.AddThemeColorOverride("font_color", multColor);

        // Show/hide warning for high fatigue
        _warningPanel.Visible = _currentFatigue >= 0.33f;

        // Update tooltip
        _starsLabel.TooltipText = $"Combo疲劳等级: {_currentFatigue:P0}\n" +
            $"当前combo伤害倍率: {multiplierPercent}%";
    }

    /// <summary>
    /// Set the current pet ID to track fatigue for.
    /// </summary>
    public void SetPetId(string petId)
    {
        _currentPetId = petId;
        RefreshDisplay();
    }

    /// <summary>
    /// Get the current fatigue level (0.0 - 1.0).
    /// </summary>
    public float GetCurrentFatigue() => _currentFatigue;

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            // Cleanup signal connections
            try
            {
                if (ComboFatigueSystem.Instance != null)
                {
                    ComboFatigueSystem.OnFatigueChanged -= OnFatigueChanged;
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
