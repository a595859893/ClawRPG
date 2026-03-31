using Godot;
using System;

/// <summary>
/// Combo Failure Buffer System — manages the "emotional cushion" when a combo breaks (REQ-171).
/// 
/// When ComboFailed fires (wrong key or timeout), this system:
/// 1. Triggers a brief slow-motion effect via AnimationEffectManager
/// 2. Shows the COMBO LOST UI overlay
/// 3. Auto-restores normal game speed after the configured duration
///
/// This is purely a visual/feedback layer — combat logic continues uninterrupted.
/// </summary>
public partial class ComboFailureBufferSystem : BaseSystem
{
    /// <summary>
    /// Singleton instance
    /// </summary>
    public static ComboFailureBufferSystem Instance { get; private set; }

    // === REQ-171 config ===
    [Export] private bool _enabled = true;
    [Export] private float _slowdownDuration = 1.0f;    // seconds
    [Export] private float _slowdownScale = 0.25f;      // 25% speed during slowdown

    // State
    private bool _isBufferActive = false;
    private float _bufferTimer = 0f;
    private float _originalTimeScale = 1f;

    // UI reference
    private ComboFailureUI _failureUI;

    public override void _Ready()
    {
        Instance = this;
        _ConnectSignals();
        _SetupUI();
    }

    /// <summary>
    /// Lazy initialization for use from static signal handlers —
    /// ensures the system node is in the scene tree before handling signals.
    /// </summary>
    private static void _EnsureInitialized()
    {
        if (Instance != null) return;

        var systemNode = new Node();
        systemNode.Name = "ComboFailureBufferSystem";
        var newInstance = new ComboFailureBufferSystem();
        Instance = newInstance;
        systemNode.AddChild(newInstance);

        var tree = Engine.GetMainLoop()?.GetTree();
        if (tree?.CurrentScene != null)
            tree.CurrentScene.AddChild(systemNode);
        else
            tree?.Root.AddChild(systemNode);
    }

    private void _ConnectSignals()
    {
        ComboSystem.ComboFailed += _OnComboFailed;
    }

    private void _SetupUI()
    {
        // Create the failure UI as a CanvasLayer child
        var canvasLayer = new CanvasLayer();
        canvasLayer.Name = "ComboFailureUI";
        canvasLayer.Layer = 150; // Above most UI
        GetTree().Root.AddChild(canvasLayer);

        _failureUI = new ComboFailureUI();
        _failureUI.Visible = false;
        canvasLayer.AddChild(_failureUI);
    }

    private void _OnComboFailed(string comboId)
    {
        _EnsureInitialized(); // ensure system is in scene tree
        if (!_enabled || _isBufferActive) return;

        _isBufferActive = true;
        _bufferTimer = _slowdownDuration;

        // Store original time scale and apply slowdown
        _originalTimeScale = Engine.TimeScale;
        AnimationEffectManager.Instance?.TriggerSlowMotion(_slowdownScale, _slowdownDuration);

        // Show COMBO LOST UI
        _failureUI?.ShowFailure(_slowdownDuration);
    }

    public override void _Process(double delta)
    {
        if (!_isBufferActive) return;

        _bufferTimer -= (float)delta;
        if (_bufferTimer <= 0)
        {
            _RestoreNormalSpeed();
        }
    }

    private void _RestoreNormalSpeed()
    {
        _isBufferActive = false;
        _bufferTimer = 0f;
        Engine.TimeScale = _originalTimeScale;
        _failureUI?.Hide();
    }

    /// <summary>
    /// Enable or disable the failure buffer effect at runtime.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        if (!enabled && _isBufferActive)
        {
            _RestoreNormalSpeed();
        }
    }

    public override void _ExitTree()
    {
        ComboSystem.ComboFailed -= _OnComboFailed;
        if (_isBufferActive)
        {
            Engine.TimeScale = _originalTimeScale;
        }
    }
}
