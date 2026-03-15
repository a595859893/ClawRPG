using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 玩家基础属性数据
    /// </summary>
    [Serializable]
    public class PlayerBasicData
    {
        public int Level { get; set; } = 1;
        public int Experience { get; set; }
        public int CurrentHealth { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
        public int CurrentMana { get; set; } = 50;
        public int MaxMana { get; set; } = 50;
        public int Gold { get; set; }
        public int Strength { get; set; } = 10;
        public int Agility { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        
        // Position
        public float X { get; set; }
        public float Y { get; set; }
        
        // Location
        public string LocationName { get; set; } = "Unknown";
        public string CurrentArea { get; set; } = "forest";
        public bool[] ExploredAreas { get; set; } = new bool[10];
        
        // Player identity
        public string CurrentTitleId { get; set; } = "";
        public string[] UnlockedTitleIds { get; set; } = new string[0];
        
        // Combo system
        public Dictionary<string, Variant> ComboData { get; set; }
    }
    
    /// <summary>
    /// 玩家背包装备数据
    /// </summary>
    [Serializable]
    public class PlayerInventoryData
    {
        public int[] Inventory { get; set; } = new int[30];
        public int[] InventoryCounts { get; set; } = new int[30];
        public int[] Equipment { get; set; } = new int[4];
        
        // Quick slot data
        public string[] QuickSlotItemIds { get; set; } = new string[9];
        public int[] QuickSlotQuantities { get; set; } = new int[9];
        
        // Equipment visuals
        public Dictionary<string, string[]> UnlockedVisuals { get; set; } = new();
        public Dictionary<string, string> EquipmentVisualsData { get; set; } = new();
    }
    
    /// <summary>
    /// 玩家任务进度数据
    /// </summary>
    [Serializable]
    public class PlayerQuestData
    {
        public int[] CompletedQuests { get; set; } = new int[0];
        public int[] ActiveQuests { get; set; } = new int[0];
        public int[] QuestProgress { get; set; } = new int[0];
        
        // Guild Quest
        public Dictionary<string, object> GuildQuestData { get; set; } = new();
    }
    
    /// <summary>
    /// 玩家统计数据
    /// </summary>
    [Serializable]
    public class PlayerStatisticsData
    {
        // Basic stats
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        
        // Extended stats
        public int TotalHealing { get; set; }
        public int CriticalHits { get; set; }
        public int PerfectBlocks { get; set; }
        public int Dodges { get; set; }
        public int GoldEarned { get; set; }
        public int GoldSpent { get; set; }
        public int ExperienceGained { get; set; }
        public int ItemsCollected { get; set; }
        public int ItemsCrafted { get; set; }
        public int QuestsCompleted { get; set; }
        public int SkillsLearned { get; set; }
        public int SkillsUsed { get; set; }
        public int RegionsDiscovered { get; set; }
        public int EnemiesEncountered { get; set; }
        public int BossesDefeated { get; set; }
        
        // Playtime & records
        public float TotalPlayTime { get; set; }
        public int HighestLevel { get; set; }
        public int HighestCombo { get; set; }
        public int AchievementsUnlocked { get; set; }
    }
    
    /// <summary>
    /// 玩家宠物数据
    /// </summary>
    [Serializable]
    public class PlayerPetData
    {
        public int ActivePetId { get; set; } = -1;
        public int PetLevel { get; set; } = 1;
        
        // Pet systems
        public Dictionary<string, object> PetStoryData { get; set; } = new();
        public Dictionary<string, object> PetEggData { get; set; } = new();
    }
    
    /// <summary>
    /// 玩家技能数据
    /// </summary>
    [Serializable]
    public class PlayerSkillData
    {
        public int[] LearnedSkills { get; set; } = new int[0];
    }
    
    /// <summary>
    /// 玩家系统数据（各种游戏系统的高级数据）
    /// </summary>
    [Serializable]
    public class PlayerSystemData
    {
        // Mount system
        public Dictionary<string, Dictionary<string, object>> MountData { get; set; } = new();
        
        // Bookmark system
        public Dictionary<string, object> BookmarkData { get; set; } = new();
        public Dictionary<string, object> AutoBookmarkData { get; set; } = new();
        
        // Enhancement system
        public Dictionary<string, object> EnhancementData { get; set; } = new();
        
        // Auto potion
        public Dictionary<string, object> AutoPotionData { get; set; } = new();
        
        // Enchantment
        public Dictionary<string, object> EnchantmentData { get; set; } = new();
        
        // Bounty
        public Dictionary<string, object> BountyData { get; set; } = new();
        
        // Weather
        public Dictionary<string, object> WeatherData { get; set; } = new();
        
        // Keybinding
        public Dictionary<string, int> KeybindingData { get; set; } = new();
        
        // Emote
        public Dictionary<string, object> EmoteData { get; set; } = new();
        
        // Sealed Tower (roguelike endless dungeon)
        public Dictionary<string, object> SealedTowerData { get; set; } = new();
        
        // Prestige
        public Dictionary<string, object> PrestigeData { get; set; } = new();
        
        // Quick Mode Reward
        public Dictionary<string, object> QuickModeRewardData { get; set; } = new();
    }
}
