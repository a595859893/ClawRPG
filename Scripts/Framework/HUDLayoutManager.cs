namespace ClawRPG.Scripts.Framework
{

using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using FileAccess = Godot.FileAccess;

/// <summary>
/// HUD layout configuration for a single draggable element.
/// </summary>
[System.Serializable]
public class HUDLayoutConfig
{
    public string ElementId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float Scale { get; set; } = 1.0f;
    public bool IsDefault { get; set; } = true;
}

/// <summary>
/// Manages drag, resize, and position persistence for HUD elements.
/// - Alt + Drag = Move
/// - Alt + Scroll = Resize (0.5x ~ 2.0x)
/// - Double-click = Reset to default
/// - Grid snap overlay while dragging
/// 
/// Usage: Call HUDLayoutManager.Initialize() once at game start,
/// then call RegisterHUD(elementId, control) for each draggable HUD element.
/// </summary>
public partial class HUDLayoutManager : Node
{
    public static HUDLayoutManager Instance { get; private set; }

    private const string CONFIG_FILE = "user://hud_layout.cfg";
    private const float MIN_SCALE = 0.5f;
    private const float MAX_SCALE = 2.0f;
    private const float GRID_SIZE = 8.0f;
    private const float DEFAULT_SCALE = 1.0f;

    private Dictionary<string, Control> _registeredElements = new Dictionary<string, Control>();
    private Dictionary<string, HUDLayoutConfig> _configs = new Dictionary<string, HUDLayoutConfig>();

    // Dragging state
    private Control _draggingControl;
    private string _draggingElementId;
    private Vector2 _dragOffset;
    private bool _isDragging;
    private bool _altPressed;

    // Visual feedback
    private Panel _gridOverlay;
    private Label _scaleLabel;

    // Config enabled
    private bool _initialized = false;

    public override void _Ready()
    {
        base._Ready();

        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;

        LoadConfigs();
        SetupGridOverlay();
        SetupScaleLabel();

        _initialized = true;
        GD.Print("[HUDLayoutManager] Initialized");
    }

    public override void _Process(double delta)
    {
        // Track Alt key state
        _altPressed = Input.IsKeyPressed(Key.Alt);
    }

    public override void _Input(InputEvent @event)
    {
        if (_draggingControl == null) return;

        // Release drag on mouse up
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
        {
            StopDragging();
        }

        // Alt + scroll to resize
        if (_altPressed && @event is InputEventMouseButton mb2 && mb2.ButtonIndex == MouseButton.WheelUp)
        {
            ResizeElement(_draggingElementId, 0.1f);
            AcceptEvent();
        }
        else if (_altPressed && @event is InputEventMouseButton mb3 && mb3.ButtonIndex == MouseButton.WheelDown)
        {
            ResizeElement(_draggingElementId, -0.1f);
            AcceptEvent();
        }
    }

    /// <summary>
    /// Register a HUD element for drag/resize management.
    /// </summary>
    public void RegisterHUD(string elementId, Control control)
    {
        if (!_initialized)
        {
            GD.PrintWarn("[HUDLayoutManager] Not initialized yet, deferring registration of: " + elementId);
            return;
        }

        if (_registeredElements.ContainsKey(elementId))
        {
            GD.PrintWarn("[HUDLayoutManager] Element already registered: " + elementId);
            return;
        }

        _registeredElements[elementId] = control;
        control.Resizable = false; // We handle our own drag

        // Load saved config or use defaults
        if (_configs.TryGetValue(elementId, out var config))
        {
            ApplyConfig(control, config);
        }
        else
        {
            // Store default position
            var defaultConfig = new HUDLayoutConfig
            {
                ElementId = elementId,
                PositionX = control.Position.X,
                PositionY = control.Position.Y,
                Scale = DEFAULT_SCALE,
                IsDefault = true
            };
            _configs[elementId] = defaultConfig;
        }

        GD.Print("[HUDLayoutManager] Registered HUD element: " + elementId);
    }

    /// <summary>
    /// Start dragging an element (call from mouse filter pass-through).
    /// Returns true if drag was started.
    /// </summary>
    public bool TryStartDrag(Control control, string elementId, Vector2 mousePos)
    {
        if (!_altPressed) return false;

        _draggingControl = control;
        _draggingElementId = elementId;
        _dragOffset = control.Position - mousePos;
        _isDragging = true;

        ShowGridOverlay(control);
        return true;
    }

    /// <summary>
    /// Update drag position (call from _Input when dragging).
    /// </summary>
    public void UpdateDragPosition(Vector2 mousePos)
    {
        if (!_isDragging || _draggingControl == null) return;

        var newPos = mousePos + _dragOffset;
        newPos = SnapToGrid(newPos);
        _draggingControl.Position = newPos;
    }

    /// <summary>
    /// Handle double-click to reset (call from GUIInput on each HUD element).
    /// </summary>
    public void OnElementDoubleClicked(string elementId)
    {
        if (!_altPressed) return;

        if (_configs.TryGetValue(elementId, out var config) && _registeredElements.TryGetValue(elementId, out var control))
        {
            var defaultPos = new Vector2(config.PositionX, config.PositionY);
            control.Position = defaultPos;
            control.Scale = Vector2.One * DEFAULT_SCALE;
            config.Scale = DEFAULT_SCALE;
            config.IsDefault = true;
            SaveConfigs();
            GD.Print($"[HUDLayoutManager] Reset {elementId} to default position");
        }
    }

    private void StopDragging()
    {
        if (_draggingControl != null && _draggingElementId != null)
        {
            // Save final position
            if (_configs.TryGetValue(_draggingElementId, out var config))
            {
                config.PositionX = _draggingControl.Position.X;
                config.PositionY = _draggingControl.Position.Y;
                config.IsDefault = false;
                SaveConfigs();
            }
        }

        _draggingControl = null;
        _draggingElementId = null;
        _isDragging = false;
        HideGridOverlay();
    }

    private void ResizeElement(string elementId, float delta)
    {
        if (!_registeredElements.TryGetValue(elementId, out var control)) return;
        if (!_configs.TryGetValue(elementId, out var config)) return;

        var newScale = Mathf.Clamp(config.Scale + delta, MIN_SCALE, MAX_SCALE);
        config.Scale = newScale;
        control.Scale = Vector2.One * newScale;

        ShowScaleLabel(control, newScale);
        SaveConfigs();
    }

    private Vector2 SnapToGrid(Vector2 pos)
    {
        return new Vector2(
            Mathf.Round(pos.X / GRID_SIZE) * GRID_SIZE,
            Mathf.Round(pos.Y / GRID_SIZE) * GRID_SIZE
        );
    }

    private void ApplyConfig(Control control, HUDLayoutConfig config)
    {
        control.Position = new Vector2(config.PositionX, config.PositionY);
        control.Scale = Vector2.One * config.Scale;
    }

    private void LoadConfigs()
    {
        try
        {
            if (!Godot.FileAccess.FileExists(CONFIG_FILE)) return;

            using var file = Godot.FileAccess.Open(CONFIG_FILE, FileAccess.ModeFlags.Read);
            if (file == null) return;

            string json = file.GetAsText();
            var parsed = JSON.ParseString(json);
            if (parsed.Error != Error.Ok) return;

            var dict = parsed.Result.AsGodotDictionary();
            foreach (var kvp in dict)
            {
                var elementId = kvp.Key.ToString();
                var cd = kvp.Value.AsGodotDictionary();
                _configs[elementId] = new HUDLayoutConfig
                {
                    ElementId = elementId,
                    PositionX = (float)cd.GetValueOrDefault("x", 0f),
                    PositionY = (float)cd.GetValueOrDefault("y", 0f),
                    Scale = (float)cd.GetValueOrDefault("scale", 1f),
                    IsDefault = (bool)cd.GetValueOrDefault("isDefault", true)
                };
            }

            GD.Print($"[HUDLayoutManager] Loaded {_configs.Count} HUD layout configs");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[HUDLayoutManager] Failed to load configs: {ex.Message}");
        }
    }

    private void SaveConfigs()
    {
        try
        {
            using var file = Godot.FileAccess.Open(CONFIG_FILE, FileAccess.ModeFlags.Write);
            if (file == null) return;

            var dict = new GodotDictionary();
            foreach (var kvp in _configs)
            {
                var cd = new GodotDictionary
                {
                    { "x", kvp.Value.PositionX },
                    { "y", kvp.Value.PositionY },
                    { "scale", kvp.Value.Scale },
                    { "isDefault", kvp.Value.IsDefault }
                };
                dict[kvp.Key] = cd;
            }

            var json = JSON.stringify(dict);
            file.StoreString(json);
            file.Flush();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[HUDLayoutManager] Failed to save configs: {ex.Message}");
        }
    }

    private void SetupGridOverlay()
    {
        _gridOverlay = new Panel();
        _gridOverlay.Name = "GridOverlay";
        _gridOverlay.ZIndex = 999;
        _gridOverlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_gridOverlay);
        _gridOverlay.Visible = false;

        // Style
        var style = new StyleBoxFlat();
        style.BgColor = new Color(1, 1, 0, 0.1f);
        style.BorderColor = new Color(1, 1, 0, 0.3f);
        style.BorderWidthLeft = 1;
        style.BorderWidthRight = 1;
        style.BorderWidthTop = 1;
        style.BorderWidthBottom = 1;
        _gridOverlay.AddThemeStyleboxOverride("panel", style);
    }

    private void ShowGridOverlay(Control target)
    {
        _gridOverlay.GlobalPosition = target.GlobalPosition;
        _gridOverlay.Size = target.Size * target.Scale;
        _gridOverlay.Visible = true;
    }

    private void HideGridOverlay()
    {
        _gridOverlay.Visible = false;
    }

    private void SetupScaleLabel()
    {
        _scaleLabel = new Label();
        _scaleLabel.Name = "ScaleLabel";
        _scaleLabel.ZIndex = 1000;
        _scaleLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
        _scaleLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0));
        _scaleLabel.Text = "100%";
        AddChild(_scaleLabel);
        _scaleLabel.Visible = false;
    }

    private async void ShowScaleLabel(Control target, float scale)
    {
        _scaleLabel.Text = $"{(int)(scale * 100)}%";
        _scaleLabel.Position = target.GlobalPosition + new Vector2(0, -30);
        _scaleLabel.Visible = true;

        // Hide after 1 second
        await ToSignal(target.GetTree().CreateTimer(1.0), Timer.SignalName.Timeout);
        if (_scaleLabel != null)
            _scaleLabel.Visible = false;
    }
}

} // namespace ClawRPG.Scripts.Framework
