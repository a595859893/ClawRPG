using Godot;
using System;

public class ArenaTournamentMain : Node
{
    private ArenaTournamentSystem _system;
    private ArenaTournamentUI _ui;
    
    public override void _Ready()
    {
        _system = new ArenaTournamentSystem();
        _ui = new ArenaTournamentUI();
        
        // 添加 UI 到场景
        var canvasLayer = new CanvasLayer();
        canvasLayer.Layer = 100;
        canvasLayer.AddChild(_ui);
        AddChild(canvasLayer);
        
        // 初始隐藏
        _ui.Hide();
        
        GD.Print("Arena Tournament System initialized");
    }
    
    public override void _Input(InputEvent event_)
    {
        if (event_ is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // Ctrl+Shift+A: 切换竞技场锦标赛 UI
            if (keyEvent.Ctrl && keyEvent.Shift && keyEvent.Keycode == Key.A)
            {
                _ui.Toggle();
                GetTree().SetInputAsHandled();
            }
        }
    }
    
    public ArenaTournamentSystem GetSystem() => _system;
    public ArenaTournamentUI GetUI() => _ui;
}
