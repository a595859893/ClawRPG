using Godot;
using System;

public partial class MonsterTamingUI
{
    private void OnMonsterSelected(TameableMonster monster)
    {
        _selectedMonster = monster;
        UpdateInfoPanel(monster);
    }
    
    private void OnTameMethodPressed(MonsterTamingSystem.TamingMethod method)
    {
        if (_selectedMonster == null || _selectedMonster.IsTamed) return;
        
        // Get player data (simplified - in real game, get from player)
        int playerLevel = 50; // Placeholder
        int playerGold = 10000; // Placeholder
        
        bool success = MonsterTamingSystem.Instance.AttemptTame(_selectedMonster, method, "player", playerLevel, playerGold);
        
        if (success)
        {
            // Show success feedback
            var tween = GetTree().CreateTween();
            _monsterNameLabel.Text = $"✅ {_selectedMonster.Name} tamed!";
            _monsterNameLabel.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.5f));
        }
        
        RefreshDisplay();
    }
    
    private void OnRefreshPressed()
    {
        MonsterTamingSystem.Instance.RefreshWildMonsters();
        RefreshDisplay();
    }
}
