using Godot;
using System;

public partial class CrossServerBattleMain : BaseSystem
{
    private static CrossServerBattleSystem _system;
    private static CrossServerBattleUI _ui;

    public override void _Ready()
    {
        // Initialize system
        _system = new CrossServerBattleSystem();
        _system.Name = "CrossServerBattleSystem";
        GetTree().Root.AddChild(_system);

        // Initialize UI
        _ui = new CrossServerBattleUI();
        _ui.Name = "CrossServerBattleUI";
        _ui.Visible = false;
        GetTree().Root.AddChild(_ui);

        // Register default server and player for demo
        _system.RegisterServer("server_1", "Alpha Server", 100, 50);
        _system.RegisterServer("server_2", "Beta Server", 80, 45);
        _system.RegisterServer("server_3", "Gamma Server", 120, 55);
        
        _system.RegisterPlayer("player_1", "Hero", "server_1");
        _system.RegisterPlayer("player_2", "Warrior", "server_2");
        _system.RegisterPlayer("player_3", "Mage", "server_2");
        _system.RegisterPlayer("player_4", "Rogue", "server_3");
        _system.RegisterPlayer("player_5", "Paladin", "server_3");

        GD.Print("[CrossServerBattleMain] Initialized");
    }

    public static CrossServerBattleSystem GetSystem()
    {
        return _system;
    }

    public static CrossServerBattleUI GetUI()
    {
        return _ui;
    }

    public static void ToggleCrossServerBattleUI()
    {
        if (_ui != null)
        {
            _ui.ToggleVisibility();
        }
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        // CrossServerBattleMain 是容器系统，无持久化状态
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        // CrossServerBattleMain 是容器系统，无持久化状态
    }
}
