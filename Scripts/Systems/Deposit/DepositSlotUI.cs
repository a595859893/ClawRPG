using Godot;
using System;

/// <summary>
/// UI panel showing current deposit slot levels and intensities.
/// Attached to a CanvasLayer in the game UI.
/// </summary>
public class DepositSlotUI : Control
{
    private HBoxContainer _slotsContainer;
    private Label _titleLabel;
    private bool _isInitialized = false;

    // Deposit slot UI elements (one per DepositType)
    private Control[] _slotNodes = new Control[5]; // 5 deposit types
    private Label[] _slotLabels = new Label[5];
    private TextureProgress[] _slotBars = new TextureProgress[5];

    public override void _Ready()
    {
        // Defer construction to give DepositData time to initialize
        CallDeferred(nameof(InitializeUI));
    }

    private void InitializeUI()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        // Find or create root container
        _slotsContainer = new HBoxContainer();
        _slotsContainer.SetAnchorsPreset(Control.LayoutPreset.Wide);
        _slotsContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _slotsContainer.CustomMinimumSize = new Vector2(0, 60);
        AddChild(_slotsContainer);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "沉积槽";
        _titleLabel.Hide();
        AddChild(_titleLabel);

        // Create one slot display per deposit type
        var types = new[] {
            DepositData.DepositType.Ember,
            DepositData.DepositType.Sediment,
            DepositData.DepositType.Echo,
            DepositData.DepositType.Debt,
            DepositData.DepositType.Synergy
        };

        for (int i = 0; i < types.Length; i++)
        {
            var slotNode = CreateSlotDisplay(types[i], i);
            _slotsContainer.AddChild(slotNode);
            _slotNodes[i] = slotNode;
        }

        // Subscribe to deposit updates
        if (DepositData.Instance != null)
        {
            // REQ-151-03: Godot 3→4 Signal migration
            DepositData.Instance.DepositUpdated += OnDepositUpdated;
            DepositData.Instance.DepositLevelChanged += OnDepositLevelChanged;

            // Initial display
            RefreshAllSlots();
        }

        // Hide by default, show when combat starts
        HideAllSlots();
    }

    private Control CreateSlotDisplay(DepositData.DepositType type, int index)
    {
        var container = new VBoxContainer();
        container.CustomMinimumSize = new Vector2(80, 60);
        container.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;

        // Icon + name label
        var nameLabel = new Label();
        nameLabel.Text = GetTypeName(type);
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AddColorOverride("font_color", GetTypeColor(type));
        container.AddChild(nameLabel);
        _slotLabels[index] = nameLabel;

        // Progress bar
        var bar = new TextureProgress();
        bar.CustomMinimumSize = new Vector2(70, 10);
        bar.MaxValue = 100;
        bar.Value = 0;
        bar.TintProgress = GetTypeColor(type);
        container.AddChild(bar);
        _slotBars[index] = bar;

        // Level label
        var levelLabel = new Label();
        levelLabel.Text = "Lv0";
        levelLabel.HorizontalAlignment = HorizontalAlignment.Center;
        levelLabel.AddColorOverride("font_color", GetTypeColor(type));
        levelLabel.Name = "LevelLabel";
        container.AddChild(levelLabel);

        return container;
    }

    private void OnDepositUpdated(DepositData.DepositType type)
    {
        UpdateSlot(type);
    }

    private void OnDepositLevelChanged(DepositData.DepositType type, int newLevel)
    {
        // Visual pulse effect on level up
        UpdateSlot(type);
        PulseSlot(type);
    }

    private void UpdateSlot(DepositData.DepositType type)
    {
        var depositData = DepositData.Instance;
        if (depositData == null) return;

        int idx = (int)type;
        if (idx < 0 || idx >= _slotNodes.Length) return;

        var slot = depositData.GetSlot(type);
        if (slot == null) return;

        // Update bar (XP progress toward next level)
        float xpProgress = slot.Xp / slot.XpForNextLevel() * 100f;
        _slotBars[idx].Value = Mathf.Clamp(xpProgress, 0, 100);

        // Update level label
        var levelLabel = _slotNodes[idx].GetNode<Label>("LevelLabel");
        if (levelLabel != null)
        {
            levelLabel.Text = $"Lv{slot.Level}";
        }

        // Update color based on intensity
        Color color = GetTypeColor(type);
        if (slot.Level > 0)
        {
            color.A = 1.0f;
        }
        else
        {
            color.A = 0.3f;
        }
        _slotLabels[idx].AddColorOverride("font_color", color);
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < 5; i++)
        {
            UpdateSlot((DepositData.DepositType)i);
        }
    }

    private void PulseSlot(DepositData.DepositType type)
    {
        int idx = (int)type;
        if (idx < 0 || idx >= _slotNodes.Length) return;

        // Simple flash effect: briefly double modulate
        var tween = CreateTween();
        var label = _slotLabels[idx];
        var originalColor = label.GetColor("font_color");
        var brightColor = originalColor;
        brightColor.L = Mathf.Min(1.0f, originalColor.L + 0.3f);

        tween.TweenProperty(label, "modulate", brightColor, 0.1f);
        tween.TweenProperty(label, "modulate", originalColor, 0.3f);
    }

    private void ShowAllSlots()
    {
        _titleLabel.Show();
        foreach (var node in _slotNodes)
        {
            if (node != null) node.Show();
        }
    }

    private void HideAllSlots()
    {
        _titleLabel.Hide();
        foreach (var node in _slotNodes)
        {
            if (node != null) node.Hide();
        }
    }

    // ── Utilities ─────────────────────────────────────────────────────────

    private string GetTypeName(DepositData.DepositType type)
    {
        return type switch
        {
            DepositData.DepositType.Ember => "余烬",
            DepositData.DepositType.Sediment => "护盾",
            DepositData.DepositType.Echo => "残影",
            DepositData.DepositType.Debt => "血债",
            DepositData.DepositType.Synergy => "协同",
            _ => type.ToString()
        };
    }

    private Color GetTypeColor(DepositData.DepositType type)
    {
        return type switch
        {
            DepositData.DepositType.Ember => new Color(1.0f, 0.4f, 0.1f),     // Orange-red (fire)
            DepositData.DepositType.Sediment => new Color(0.6f, 0.4f, 0.2f), // Brown
            DepositData.DepositType.Echo => new Color(0.5f, 0.5f, 1.0f),     // Blue
            DepositData.DepositType.Debt => new Color(0.8f, 0.1f, 0.1f),     // Dark red
            DepositData.DepositType.Synergy => new Color(0.3f, 1.0f, 0.3f), // Green
            _ => Colors.Gray
        };
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Show deposit slot UI (call when entering a run).</summary>
    public void ShowDepositUI() => ShowAllSlots();

    /// <summary>Hide deposit slot UI (e.g., in menus).</summary>
    public void HideDepositUI() => HideAllSlots();
}
