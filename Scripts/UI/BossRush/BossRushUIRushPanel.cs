using Godot;
using System;
using ClawRPG.Scripts.UI.BossRush;

/// <summary>
/// Boss Rush UI - Rush Panel Component
/// Handles the main rush gameplay UI (difficulty selection, controls, rewards display)
/// </summary>
public class BossRushUIRushPanel : Control
{
    private BossRushSystem _bossRushSystem;
    
    // Sub-components
    private BossRushUIRushPanelDisplay _display;
    private BossRushUIRushPanelControls _controls;
    
    // Callbacks
    public Action<string> OnStartPressed { get; set; }
    public Action OnAdvancePressed { get; set; }
    public Action OnQuitPressed { get; set; }
    public Action OnPausePressed { get; set; }
    
    public BossRushUIRushPanel()
    {
    }
    
    public void Initialize(BossRushSystem system)
    {
        _bossRushSystem = system;
    }
    
    public void Setup(Control parent, Vector2 position, Vector2 size)
    {
        SetAnchor(AnchorPreset.FullRect);
        Position = position;
        Size = size;
        parent.AddChild(this);
        
        CreateElements();
    }
    
    private void CreateElements()
    {
        // Initialize display component
        _display = new BossRushUIRushPanelDisplay();
        _display.Initialize(_bossRushSystem);
        _display.CreateElements(this);
        
        // Initialize controls component
        _controls = new BossRushUIRushPanelControls();
        _controls.Initialize(_bossRushSystem);
        _controls.CreateElements(this);
        
        // Wire up callbacks
        _controls.OnStartPressed += (diff) => OnStartPressed?.Invoke(diff);
        _controls.OnAdvancePressed += () => OnAdvancePressed?.Invoke();
        _controls.OnQuitPressed += () => OnQuitPressed?.Invoke();
        _controls.OnPausePressed += () => OnPausePressed?.Invoke();
    }
    
    public void UpdateUI()
    {
        if (_bossRushSystem == null) return;
        
        _display.UpdateUI();
        _controls.UpdateButtonStates();
    }
}
