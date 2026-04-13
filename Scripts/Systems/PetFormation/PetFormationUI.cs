using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems.PetFormation;

/// <summary>
/// Pet Tactical Formation UI — displays three formation zone panels (REQ-176).
/// Shows Front/Mid/Rear zones with drag-and-drop or hotkey assignment.
/// Color-coded: Front=Red, Mid=Yellow, Rear=Blue.
/// Active formation indicator shows current FormationType + effect description.
/// </summary>
public partial class PetFormationUI : Control
{
    // === Config ===
    [Export] private bool _enabled = true;
    [Export] private float _animationDuration = 0.3f;

    // Zone panels
    private PanelContainer _frontPanel;
    private PanelContainer _midPanel;
    private PanelContainer _rearPanel;

    // Zone labels
    private Label _frontLabel;
    private Label _midLabel;
    private Label _rearLabel;
    private Label _frontPetLabel;
    private Label _midPetLabel;
    private Label _rearPetLabel;

    // Formation display
    private PanelContainer _formationInfoPanel;
    private Label _formationNameLabel;
    private Label _formationDescLabel;
    private Label _formationEffectLabel;

    // Hotkey hints
    private Label _hotkeyHintLabel;

    // State
    private bool _isVisible = false;

    // Reference to system
    private PetFormationSystem _system;
    private bool _subscribedToSystem = false;

    public override void _Ready()
    {
        _SetupUI();
        _ConnectToSystem();
    }

    private PetFormationSystem _GetSystem()
    {
        if (_system == null)
        {
            _system = PetFormationSystem.Instance;
        }
        return _system;
    }

    private void _ConnectToSystem()
    {
        var sys = _GetSystem();
        if (sys == null)
        {
            // Retry after a short delay
            CallDeferred(nameof(_ConnectToSystemDelayed));
            return;
        }

        sys.OnFormationChanged += _OnFormationChanged;
        sys.OnSlotAssigned += _OnSlotAssigned;
        sys.OnSlotRemoved += _OnSlotRemoved;
        _subscribedToSystem = true;

        // Sync initial state
        _SyncFromSystem();
    }

    private void _ConnectToSystemDelayed()
    {
        var sys = _GetSystem();
        if (sys != null && !_subscribedToSystem)
        {
            sys.OnFormationChanged += _OnFormationChanged;
            sys.OnSlotAssigned += _OnSlotAssigned;
            sys.OnSlotRemoved += _OnSlotRemoved;
            _subscribedToSystem = true;
            _SyncFromSystem();
        }
    }

    private void _SetupUI()
    {
        Name = "PetFormationUI";
        AnchorsPreset = AnchorsPreset.Custom;
        CustomMinimumSize = new Vector2(400, 180);

        // Position: top-center of combat prep screen
        OffsetLeft = -200;
        OffsetTop = 60;
        OffsetRight = 200;
        OffsetBottom = 240;

        // Container
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 8);
        AddChild(vbox);

        // === Formation Name Bar ===
        _formationInfoPanel = new PanelContainer();
        _formationInfoPanel.CustomMinimumSize = new Vector2(0, 32);
        var infoStyle = _CreateFlatStyle(new Color(0.08f, 0.08f, 0.12f, 0.9f), 4);
        _formationInfoPanel.AddThemeStyleboxOverride("panel", infoStyle);
        vbox.AddChild(_formationInfoPanel);

        var infoHbox = new HBoxContainer();
        _formationInfoPanel.AddChild(infoHbox);

        _formationNameLabel = new Label();
        _formationNameLabel.Text = "[ 无阵型 ]";
        _formationNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        infoHbox.AddChild(_formationNameLabel);

        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(10, 0);
        infoHbox.AddChild(spacer);

        _formationDescLabel = new Label();
        _formationDescLabel.Text = "";
        _formationDescLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _formationDescLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 1f));
        infoHbox.AddChild(_formationDescLabel);

        // === Zone Panels Row ===
        var zonesHbox = new HBoxContainer();
        zonesHbox.Alignment = BoxContainer.AlignmentMode.Center;
        zonesHbox.CustomMinimumSize = new Vector2(0, 80);
        zonesHbox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        vbox.AddChild(zonesHbox);

        // Front Zone (Red)
        _frontPanel = _CreateZonePanel(PetFormationSlot.Front, new Color(0.8f, 0.2f, 0.2f, 0.8f));
        zonesHbox.AddChild(_frontPanel);

        // Separator
        var sep1 = new Label();
        sep1.Text = "  ⟷  ";
        sep1.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f, 1f));
        zonesHbox.AddChild(sep1);

        // Mid Zone (Yellow)
        _midPanel = _CreateZonePanel(PetFormationSlot.Mid, new Color(0.85f, 0.75f, 0.1f, 0.8f));
        zonesHbox.AddChild(_midPanel);

        // Separator
        var sep2 = new Label();
        sep2.Text = "  ⟷  ";
        sep2.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f, 1f));
        zonesHbox.AddChild(sep2);

        // Rear Zone (Blue)
        _rearPanel = _CreateZonePanel(PetFormationSlot.Rear, new Color(0.2f, 0.4f, 0.85f, 0.8f));
        zonesHbox.AddChild(_rearPanel);

        // === Hotkey Hints ===
        _hotkeyHintLabel = new Label();
        _hotkeyHintLabel.Text = "快捷键: 1=前锋  2=中线  3=后卫  |  点击宠物头像分配";
        _hotkeyHintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _hotkeyHintLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f, 0.8f));
        _hotkeyHintLabel.AddThemeConstantOverride("font_size", 10);
        vbox.AddChild(_hotkeyHintLabel);

        Hide();
    }

    private PanelContainer _CreateZonePanel(PetFormationSlot slot, Color borderColor)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(90, 80);
        panel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.12f, 0.12f, 0.18f, 0.85f);
        style.BorderColor = borderColor;
        style.BorderWidthLeft = 3;
        style.BorderWidthTop = 3;
        style.BorderWidthRight = 3;
        style.BorderWidthBottom = 3;
        style.CornerRadiusTopLeft = 6;
        style.CornerRadiusTopRight = 6;
        style.CornerRadiusBottomLeft = 6;
        style.CornerRadiusBottomRight = 6;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.Alignment = BoxContainer.AlignmentMode.Center;
        panel.AddChild(vbox);

        // Slot position label
        var slotLabel = new Label();
        slotLabel.Text = slot switch
        {
            PetFormationSlot.Front => "前排",
            PetFormationSlot.Mid => "中线",
            PetFormationSlot.Rear => "后排",
            _ => "?"
        };
        slotLabel.HorizontalAlignment = HorizontalAlignment.Center;
        slotLabel.AddThemeColorOverride("font_color", borderColor);
        slotLabel.AddThemeConstantOverride("font_size", 12);
        vbox.AddChild(slotLabel);

        // Pet ID label (empty state)
        var petLabel = new Label();
        petLabel.Name = "PetLabel";
        petLabel.Text = "(空)";
        petLabel.HorizontalAlignment = HorizontalAlignment.Center;
        petLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f, 0.8f));
        petLabel.AddThemeConstantOverride("font_size", 10);
        vbox.AddChild(petLabel);

        // Hotkey indicator
        var hotkeyLabel = new Label();
        hotkeyLabel.Text = slot switch
        {
            PetFormationSlot.Front => "[1]",
            PetFormationSlot.Mid => "[2]",
            PetFormationSlot.Rear => "[3]",
            _ => ""
        };
        hotkeyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        hotkeyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f, 0.6f));
        hotkeyLabel.AddThemeConstantOverride("font_size", 10);
        vbox.AddChild(hotkeyLabel);

        // Store reference for later update
        switch (slot)
        {
            case PetFormationSlot.Front:
                _frontLabel = slotLabel;
                _frontPetLabel = petLabel;
                break;
            case PetFormationSlot.Mid:
                _midLabel = slotLabel;
                _midPetLabel = petLabel;
                break;
            case PetFormationSlot.Rear:
                _rearLabel = slotLabel;
                _rearPetLabel = petLabel;
                break;
        }

        // Wire up input
        panel.Connect(SignalName.GuiInput, this, nameof(_OnZoneGuiInput), new VariantArray(slot));

        return panel;
    }

    private StyleBoxFlat _CreateFlatStyle(Color bgColor, float cornerRadius = 0)
    {
        var style = new StyleBoxFlat();
        style.BgColor = bgColor;
        if (cornerRadius > 0)
        {
            style.CornerRadiusTopLeft = cornerRadius;
            style.CornerRadiusTopRight = cornerRadius;
            style.CornerRadiusBottomLeft = cornerRadius;
            style.CornerRadiusBottomRight = cornerRadius;
        }
        return style;
    }

    private void _OnZoneGuiInput(PetFormationSlot slot, InputEvent @event)
    {
        if (!_enabled) return;
        if (@event is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == MouseButton.Left)
        {
            _HandleZoneClicked(slot);
        }
    }

    private void _HandleZoneClicked(PetFormationSlot slot)
    {
        var sys = _GetSystem();
        if (sys == null) return;

        // Toggle: if slot already filled, clear it; otherwise just signal intent
        if (sys.IsSlotEmpty(slot))
        {
            // Signal that user wants to assign — UI shows ready state
            // Actual assignment happens via hotkey or drag from pet list
            _HighlightSlot(slot, true);
            GD.Print($"[PetFormationUI] Zone {slot} clicked — assign pet here");
        }
        else
        {
            sys.RemovePetFromSlot(slot);
        }
    }

    private void _HighlightSlot(PetFormationSlot slot, bool highlight)
    {
        var panel = slot switch
        {
            PetFormationSlot.Front => _frontPanel,
            PetFormationSlot.Mid => _midPanel,
            PetFormationSlot.Rear => _rearPanel,
            _ => null
        };

        if (panel == null) return;

        var tween = CreateTween();
        var targetColor = highlight ? new Color(1f, 1f, 0.3f, 1f) : _GetBorderColorForSlot(slot);
        var style = panel.GetThemeStylebox("panel") as StyleBoxFlat;
        if (style != null)
        {
            tween.TweenProperty(style, "border_color", targetColor, 0.2f);
        }
    }

    private Color _GetBorderColorForSlot(PetFormationSlot slot)
    {
        return slot switch
        {
            PetFormationSlot.Front => new Color(0.8f, 0.2f, 0.2f, 0.8f),
            PetFormationSlot.Mid => new Color(0.85f, 0.75f, 0.1f, 0.8f),
            PetFormationSlot.Rear => new Color(0.2f, 0.4f, 0.85f, 0.8f),
            _ => new Color(0.5f, 0.5f, 0.5f, 0.8f)
        };
    }

    public override void _Input(InputEvent @event)
    {
        if (!_enabled) return;

        // Hotkeys: 1=Front, 2=Mid, 3=Rear
        if (@event is InputEventKey key && key.Pressed)
        {
            var sys = _GetSystem();
            if (sys == null) return;

            switch ((Key)key.Scancode)
            {
                case Key.Key1:
                    _HandleHotkeyAssign(PetFormationSlot.Front);
                    break;
                case Key.Key2:
                    _HandleHotkeyAssign(PetFormationSlot.Mid);
                    break;
                case Key.Key3:
                    _HandleHotkeyAssign(PetFormationSlot.Rear);
                    break;
                case Key.Escape:
                    if (_isVisible) HideFormationUI();
                    break;
            }
        }
    }

    private void _HandleHotkeyAssign(PetFormationSlot slot)
    {
        var sys = _GetSystem();
        if (sys == null) return;

        if (!sys.IsSlotEmpty(slot))
        {
            // Already occupied — just clear it
            sys.RemovePetFromSlot(slot);
        }
        else
        {
            // Signal UI highlight — actual assignment should come from pet selection
            _HighlightSlot(slot, true);
            GD.Print($"[PetFormationUI] Hotkey pressed for {slot} — waiting for pet selection");
        }
    }

    #region Signal Handlers

    private void _OnFormationChanged(FormationType formation, FormationEffect effect)
    {
        _UpdateFormationDisplay(formation, effect);
    }

    private void _OnSlotAssigned(int petId, PetFormationSlot slot)
    {
        _UpdateSlotDisplay(slot, petId);
    }

    private void _OnSlotRemoved(PetFormationSlot slot)
    {
        _UpdateSlotDisplay(slot, null);
    }

    #endregion

    private void _SyncFromSystem()
    {
        var sys = _GetSystem();
        if (sys == null) return;

        // Sync slot displays
        _UpdateSlotDisplay(PetFormationSlot.Front, sys.GetPetIdInSlot(PetFormationSlot.Front));
        _UpdateSlotDisplay(PetFormationSlot.Mid, sys.GetPetIdInSlot(PetFormationSlot.Mid));
        _UpdateSlotDisplay(PetFormationSlot.Rear, sys.GetPetIdInSlot(PetFormationSlot.Rear));

        // Sync formation display
        var (type, effect) = sys.GetActiveFormation();
        _UpdateFormationDisplay(type, effect);
    }

    private void _UpdateSlotDisplay(PetFormationSlot slot, int? petId)
    {
        Label petLabel = slot switch
        {
            PetFormationSlot.Front => _frontPetLabel,
            PetFormationSlot.Mid => _midPetLabel,
            PetFormationSlot.Rear => _rearPetLabel,
            _ => null
        };

        if (petLabel == null) return;

        if (petId.HasValue && petId.Value > 0)
        {
            petLabel.Text = $"宠物 #{petId.Value}";
            petLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
        }
        else
        {
            petLabel.Text = "(空)";
            petLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f, 0.8f));
        }
    }

    private void _UpdateFormationDisplay(FormationType formation, FormationEffect effect)
    {
        var sys = _GetSystem();
        if (sys == null) return;

        _formationNameLabel.Text = $"[ {sys.GetFormationDisplayName()} ]";

        var config = sys._GetDatabase().GetConfig(formation);
        if (config != null)
        {
            _formationDescLabel.Text = config.Description;

            string effectText = "";
            if (effect.DamageMod != 1.0f)
                effectText += $"伤害{effect.DamageMod:F0%} ";
            if (effect.TakenMod != 1.0f)
                effectText += $"受到{effect.TakenMod:F0%} ";
            if (!string.IsNullOrEmpty(effect.SpecialEffect))
                effectText += effect.SpecialEffect;
            _formationEffectLabel.Text = effectText.TrimEnd();
        }
        else
        {
            _formationDescLabel.Text = "";
            _formationEffectLabel.Text = "";
        }

        // Color code formation name by type
        Color nameColor = formation switch
        {
            FormationType.AggressiveRush => new Color(1.0f, 0.3f, 0.3f, 1f),
            FormationType.Balanced => new Color(0.3f, 1.0f, 0.3f, 1f),
            FormationType.GuardFormation => new Color(0.3f, 0.6f, 1.0f, 1f),
            FormationType.PincerSetup => new Color(1.0f, 0.6f, 0.1f, 1f),
            FormationType.FlexibleAssault => new Color(0.8f, 0.4f, 1.0f, 1f),
            FormationType.Solo => new Color(0.6f, 0.6f, 0.6f, 1f),
            _ => new Color(0.5f, 0.5f, 0.5f, 1f)
        };
        _formationNameLabel.AddThemeColorOverride("font_color", nameColor);
    }

    #region Show/Hide

    public void ShowFormationUI()
    {
        if (!_enabled) return;
        Show();
        _isVisible = true;
        _SyncFromSystem();

        // Fade in animation
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1f, 0.25f);
    }

    public void HideFormationUI()
    {
        _isVisible = false;

        // Fade out
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.2f);
        tween.Finished += () => Hide();
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        if (!enabled)
            HideFormationUI();
    }

    #endregion
}

// Helper extension for private _GetDatabase access (avoid duplicating logic)
internal static class PetFormationSystemExtensions
{
    public static PetFormationDatabase _GetDatabase(this PetFormationSystem sys)
    {
        // Use reflection to access private _database field
        var field = typeof(PetFormationSystem).GetField("_database",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(sys) as PetFormationDatabase;
    }
}
