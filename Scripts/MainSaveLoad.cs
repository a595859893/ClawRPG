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
                LoadKeybindingData(data);
                LoadPetStoryData(data);
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
        var comboSystem = GetNodeOrNull<ComboSystem>("ComboSystem");
        if (comboSystem != null && data.ComboData != null)
        {
            comboSystem.Deserialize(data.ComboData);
            GD.Print("Combo data loaded successfully!");
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
}
