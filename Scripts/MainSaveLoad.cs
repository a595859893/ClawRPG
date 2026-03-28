using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
using ClawRPG.Systems;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Mounts;
using ClawRPG.Scripts.UI;

/// <summary>
/// MainSaveLoad - 存档管理模块
/// 处理游戏数据的保存和加载
/// </summary>
public partial class MainSaveLoad : Node
{
    private float _autoSaveTimer = 0f;
    private const float AutoSaveInterval = 300f; // 5 minutes
    
    // 引用主节点
    private Main _main;
    
    public override void _Ready()
    {
        _main = GetParent<Main>();
    }
    
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        
        // 自动存档每5分钟触发一次
        _autoSaveTimer += dt;
        if (_autoSaveTimer >= AutoSaveInterval)
        {
            _autoSaveTimer = 0f;
            TriggerAutoSave();
        }
    }
    
    /// <summary>
    /// 触发自动存档
    /// </summary>
    private void TriggerAutoSave()
    {
        GD.Print("Auto save triggered...");
        // 自动存档逻辑可以在这里扩展
    }
    
    /// <summary>
    /// 加载游戏数据
    /// </summary>
    public void LoadGameData()
    {
        var saveSystem = new SaveSystem();
        if (saveSystem.HasSave(0))
        {
            GD.Print("Found save file, loading...");
            var data = saveSystem.LoadGame(0);
            if (data != null)
            {
                LoadStatistics(data);
                LoadComboData(data);
                LoadComboForgetData(data);
                LoadKeybindingData(data);
                LoadPetStoryData(data);
                LoadStyleMasteryData(data);
            }
        }
    }
    
    /// <summary>
    /// 加载统计数据
    /// </summary>
    private void LoadStatistics(SaveData data)
    {
        var statsData = new Dictionary<string, object>
        {
            ["TotalKills"] = data.TotalKills,
            ["TotalDeaths"] = data.TotalDeaths,
            ["TotalDamageDealt"] = data.TotalDamageDealt,
            ["TotalDamageTaken"] = data.TotalDamageTaken,
            ["TotalHealing"] = data.TotalHealing,
            ["CriticalHits"] = data.CriticalHits,
            ["PerfectBlocks"] = data.PerfectBlocks,
            ["Dodges"] = data.Dodges,
            ["GoldEarned"] = data.GoldEarned,
            ["GoldSpent"] = data.GoldSpent,
            ["ExperienceGained"] = data.ExperienceGained,
            ["ItemsCollected"] = data.ItemsCollected,
            ["ItemsCrafted"] = data.ItemsCrafted,
            ["QuestsCompleted"] = data.QuestsCompleted,
            ["SkillsLearned"] = data.SkillsLearned,
            ["SkillsUsed"] = data.SkillsUsed,
            ["RegionsDiscovered"] = data.RegionsDiscovered,
            ["EnemiesEncountered"] = data.EnemiesEncountered,
            ["BossesDefeated"] = data.BossesDefeated,
            ["TotalPlayTime"] = data.TotalPlayTime,
            ["HighestLevel"] = data.HighestLevel,
            ["HighestCombo"] = data.HighestCombo,
            ["AchievementsUnlocked"] = data.AchievementsUnlocked
        };
        StatisticsManager.Instance.LoadStatistics(statsData);
        GD.Print("Statistics loaded successfully!");
    }
    
    /// <summary>
    /// 加载连击数据
    /// </summary>
    private void LoadComboData(SaveData data)
    {
        var skillComboSystem = SkillComboSystem.Instance;
        if (skillComboSystem != null && data.ComboData != null)
        {
            skillComboSystem.ImportSaveData(new Dictionary(data.ComboData));
            GD.Print("Combo data loaded successfully!");
        }
    }
    
    /// <summary>
    /// 加载 Combo 遗忘数据 (REQ-154)
    /// </summary>
    private void LoadComboForgetData(SaveData data)
    {
        if (Framework.ComboForgetData.Instance != null && data.ComboForgetData != null)
        {
            Framework.ComboForgetData.Instance.ImportSaveData(new Dictionary(data.ComboForgetData));
            GD.Print("Combo forget data loaded successfully!");
        }
    }
    
    /// <summary>
    /// 加载按键绑定数据
    /// </summary>
    private void LoadKeybindingData(SaveData data)
    {
        var keybindingSystem = GetNodeOrNull<Systems.KeybindingSystem>("KeybindingSystem");
        if (keybindingSystem != null && data.KeybindingData != null)
        {
            keybindingSystem.Deserialize(data.KeybindingData);
            GD.Print("Keybinding data loaded successfully!");
        }
    }
    
    /// <summary>
    /// 加载宠物故事数据
    /// </summary>
    private void LoadPetStoryData(SaveData data)
    {
        var petStorySystem = GetNodeOrNull<PetStorySystem>("PetStorySystem");
        if (petStorySystem != null && data.PetStoryData != null)
        {
            petStorySystem.Deserialize(data.PetStoryData);
            GD.Print("Pet story data loaded successfully!");
        }
    }
    
    /// <summary>
    /// 加载风格精通数据
    /// </summary>
    private void LoadStyleMasteryData(SaveData data)
    {
        var styleMasterySystem = GetNodeOrNull<StyleMasterySystem>("StyleMasterySystem");
        if (styleMasterySystem != null && data.StyleMasteryData != null)
        {
            styleMasterySystem.ImportSaveData(new Dictionary(data.StyleMasteryData));
            GD.Print("Style mastery data loaded successfully!");
        }
    }
    
    /// <summary>
    /// 从指定槽位加载游戏
    /// </summary>
    public void LoadGame(int saveSlot)
    {
        GD.Print("Loading game from slot: " + saveSlot);
        
        var saveSystem = new SaveSystem();
        var saveData = saveSystem.LoadGame(saveSlot);
        
        if (saveData != null)
        {
            LoadPlayerData(saveData);
            LoadStatisticsFromSlot(saveData);
            LoadQuickSlotData(saveData);
            LoadMountData(saveData);
            LoadBookmarkData(saveData);
            // 可以继续添加其他数据加载...
        }
    }
    
    /// <summary>
    /// 加载玩家数据
    /// </summary>
    private void LoadPlayerData(SaveData saveData)
    {
        var player = GetNodeOrNull<Player>("../Player");
        if (player != null && saveData.PlayerData != null)
        {
            player.LoadPlayerData(saveData.PlayerData);
        }
    }
    
    /// <summary>
    /// 从存档槽加载统计数据
    /// </summary>
    private void LoadStatisticsFromSlot(SaveData saveData)
    {
        var statsData = new Dictionary<string, object>
        {
            ["TotalKills"] = saveData.TotalKills,
            ["TotalDeaths"] = saveData.TotalDeaths,
            ["TotalDamageDealt"] = saveData.TotalDamageDealt,
            ["TotalDamageTaken"] = saveData.TotalDamageTaken,
            ["TotalHealing"] = saveData.TotalHealing,
            ["CriticalHits"] = saveData.CriticalHits,
            ["PerfectBlocks"] = saveData.PerfectBlocks,
            ["Dodges"] = saveData.Dodges,
            ["GoldEarned"] = saveData.GoldEarned,
            ["GoldSpent"] = saveData.GoldSpent,
            ["ExperienceGained"] = saveData.ExperienceGained,
            ["ItemsCollected"] = saveData.ItemsCollected,
            ["ItemsCrafted"] = saveData.ItemsCrafted,
            ["QuestsCompleted"] = saveData.QuestsCompleted,
            ["SkillsLearned"] = saveData.SkillsLearned,
            ["SkillsUsed"] = saveData.SkillsUsed,
            ["RegionsDiscovered"] = saveData.RegionsDiscovered,
            ["EnemiesEncountered"] = saveData.EnemiesEncountered,
            ["BossesDefeated"] = saveData.BossesDefeated,
            ["TotalPlayTime"] = saveData.TotalPlayTime,
            ["HighestLevel"] = saveData.HighestLevel,
            ["HighestCombo"] = saveData.HighestCombo,
            ["AchievementsUnlocked"] = saveData.AchievementsUnlocked
        };
        StatisticsManager.Instance.LoadStatistics(statsData);
    }
    
    /// <summary>
    /// 加载快速槽数据
    /// </summary>
    private void LoadQuickSlotData(SaveData saveData)
    {
        if (saveData.QuickSlotItemIds != null && saveData.QuickSlotQuantities != null)
        {
            for (int i = 0; i < Mathf.Min(saveData.QuickSlotItemIds.Length, 9); i++)
            {
                if (QuickSlotSystem.Instance != null && i < 9)
                {
                    QuickSlotSystem.Instance.SetSlot(i, saveData.QuickSlotItemIds[i], saveData.QuickSlotQuantities[i]);
                }
            }
        }
    }
    
    /// <summary>
    /// 加载坐骑数据
    /// </summary>
    private void LoadMountData(SaveData saveData)
    {
        if (saveData.MountData != null && MountManager.Instance != null)
        {
            MountManager.Instance.Deserialize(saveData.MountData);
        }
    }
    
    /// <summary>
    /// 加载收藏点数据
    /// </summary>
    private void LoadBookmarkData(SaveData saveData)
    {
        if (saveData.BookmarkData != null && BookmarkSystem.Instance != null)
        {
            BookmarkSystem.Instance.Deserialize(saveData.BookmarkData);
        }
    }
    
    /// <summary>
    /// 重置自动存档计时器
    /// </summary>
    public void ResetAutoSaveTimer()
    {
        _autoSaveTimer = 0f;
    }
    
    /// <summary>
    /// 导出所有游戏数据（供存档使用）
    /// </summary>
    public Dictionary ExportSaveData()
    {
        var allData = new Dictionary();
        
        // 游戏状态数据
        var gameStateManager = GetNodeOrNull<GameStateManager>("../GameStateManager");
        if (gameStateManager != null)
        {
            allData["gameState"] = gameStateManager.ExportSaveData();
        }
        
        // 系统初始化数据
        var systemInitManager = GetNodeOrNull<SystemInitializationManager>("../SystemInitializationManager");
        if (systemInitManager != null)
        {
            allData["systemInit"] = systemInitManager.ExportSaveData();
        }
        
        // UI 数据
        var uiManager = GetNodeOrNull<UI.UIManager>("../UIManager");
        if (uiManager != null)
        {
            allData["ui"] = uiManager.ExportSaveData();
        }
        
        // 存档管理数据
        var saveLoadManager = GetNodeOrNull<SaveLoadManager>("../SaveLoadManager");
        if (saveLoadManager != null)
        {
            allData["saveLoad"] = saveLoadManager.ExportSaveData();
        }
        
        return allData;
    }
    
    /// <summary>
    /// 导入所有游戏数据（供读档使用）
    /// </summary>
    public void ImportSaveData(Dictionary data)
    {
        if (data == null) return;

        // 游戏状态数据
        if (data.Contains("gameState"))
        {
            var gameStateManager = GetNodeOrNull<GameStateManager>("../GameStateManager");
            gameStateManager?.ImportSaveData(data["gameState"] as Dictionary);
        }
        
        // 系统初始化数据
        if (data.Contains("systemInit"))
        {
            var systemInitManager = GetNodeOrNull<SystemInitializationManager>("../SystemInitializationManager");
            systemInitManager?.ImportSaveData(data["systemInit"] as Dictionary);
        }
        
        // UI 数据
        if (data.Contains("ui"))
        {
            var uiManager = GetNodeOrNull<UI.UIManager>("../UIManager");
            uiManager?.ImportSaveData(data["ui"] as Dictionary);
        }
        
        // 存档管理数据
        if (data.Contains("saveLoad"))
        {
            var saveLoadManager = GetNodeOrNull<SaveLoadManager>("../SaveLoadManager");
            saveLoadManager?.ImportSaveData(data["saveLoad"] as Dictionary);
        }
    }
}
