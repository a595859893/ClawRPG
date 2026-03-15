using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Save data structures and data management
    /// </summary>
    public class SaveDataManager
    {
        /// <summary>
        /// Main save data container
        /// </summary>
        public class SaveData
        {
            public int Slot { get; set; }
            public string SaveName { get; set; } = "";
            public DateTime SaveTime { get; set; }
            public TimeSpan PlayTime { get; set; }
            public string LocationName { get; set; } = "Unknown";
            
            // Player data
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
            
            // Inventory
            public int[] Inventory { get; set; } = new int[30];
            public int[] InventoryCounts { get; set; } = new int[30];
            public int[] Equipment { get; set; } = new int[4];
            
            // Quest progress
            public int[] CompletedQuests { get; set; } = new int[0];
            public int[] ActiveQuests { get; set; } = new int[0];
            public int[] QuestProgress { get; set; } = new int[0];
            
            // Skills
            public int[] LearnedSkills { get; set; } = new int[0];
            
            // World state
            public string CurrentArea { get; set; } = "forest";
            public bool[] ExploredAreas { get; set; } = new bool[10];
            
            // Pet data
            public int ActivePetId { get; set; } = -1;
            public int PetLevel { get; set; } = 1;
            
            // Game stats
            public int TotalKills { get; set; }
            public int TotalDeaths { get; set; }
            public int TotalDamageDealt { get; set; }
            public int TotalDamageTaken { get; set; }
            
            // Extended game stats
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
            public float TotalPlayTime { get; set; }
            public int HighestLevel { get; set; }
            public int HighestCombo { get; set; }
            public int AchievementsUnlocked { get; set; }
            
            // Combo system data
            public Dictionary<string, Variant> ComboData { get; set; }
            
            // Title system data
            public string CurrentTitleId { get; set; } = "";
            public string[] UnlockedTitleIds { get; set; } = new string[0];
            
            // Quick slot data
            public string[] QuickSlotItemIds { get; set; } = new string[9];
            public int[] QuickSlotQuantities { get; set; } = new int[9];
            
            // Mount system data
            public Dictionary<string, Dictionary<string, object>> MountData { get; set; } = new();
            
            // Bookmark system data
            public Dictionary<string, object> BookmarkData { get; set; } = new();
            
            // Auto bookmark system data
            public Dictionary<string, object> AutoBookmarkData { get; set; } = new();
            
            // Enhancement system data
            public Dictionary<string, object> EnhancementData { get; set; } = new();
            
            // Auto potion system data
            public Dictionary<string, object> AutoPotionData { get; set; } = new();
            
            // Equipment visuals data (unlocked appearances)
            public Dictionary<string, string[]> UnlockedVisuals { get; set; } = new();
            
            // Enchantment system data
            public Dictionary<string, object> EnchantmentData { get; set; } = new();
            
            // Bounty system data
            public Dictionary<string, object> BountyData { get; set; } = new();
            
            // Weather system data
            public Dictionary<string, object> WeatherData { get; set; } = new();
            
            // Equipment visuals data
            public Dictionary<string, string> EquipmentVisualsData { get; set; } = new();
            
            // Keybinding data
            public Dictionary<string, int> KeybindingData { get; set; } = new();
            
            // Pet story system data
            public Dictionary<string, object> PetStoryData { get; set; } = new();
            
            // Pet egg system data
            public Dictionary<string, object> PetEggData { get; set; } = new();
            
            // Emote system data
            public Dictionary<string, object> EmoteData { get; set; } = new();
            
            // Sealed Tower system data (roguelike endless dungeon)
            public Dictionary<string, object> SealedTowerData { get; set; } = new();

            // Prestige system data
            public Dictionary<string, object> PrestigeData { get; set; } = new();
            
            // Quick Mode Reward system data
            public Dictionary<string, object> QuickModeRewardData { get; set; } = new();

            // Guild Quest system data
            public Dictionary<string, object> GuildQuestData { get; set; } = new();
            
            // Player data (legacy support)
            public object PlayerData { get; set; }
            
            // ===== 组合数据类 (新版 - 推荐使用) =====
            // 基础属性
            public PlayerBasicData BasicData { get; set; } = new();
            // 背包装备
            public PlayerInventoryData InventoryData { get; set; } = new();
            // 任务进度
            public PlayerQuestData QuestData { get; set; } = new();
            // 统计数据
            public PlayerStatisticsData StatisticsData { get; set; } = new();
            // 宠物数据
            public PlayerPetData PetData { get; set; } = new();
            // 技能数据
            public PlayerSkillData SkillData { get; set; } = new();
            // 系统数据
            public PlayerSystemData SystemData { get; set; } = new();
        }
        
        // Save slot metadata (stored separately for quick loading)
        public class SaveSlotInfo
        {
            public int Slot { get; set; }
            public string SaveName { get; set; }
            public DateTime SaveTime { get; set; }
            public TimeSpan PlayTime { get; set; }
            public string LocationName { get; set; }
            public int Level { get; set; }
        }
        
        // ===== Player data composition classes =====
        
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
            public float X { get; set; }
            public float Y { get; set; }
            public string CurrentArea { get; set; } = "forest";
        }
        
        public class PlayerInventoryData
        {
            public int[] Inventory { get; set; } = new int[30];
            public int[] InventoryCounts { get; set; } = new int[30];
            public int[] Equipment { get; set; } = new int[4];
        }
        
        public class PlayerQuestData
        {
            public int[] CompletedQuests { get; set; } = new int[0];
            public int[] ActiveQuests { get; set; } = new int[0];
            public int[] QuestProgress { get; set; } = new int[0];
        }
        
        public class PlayerStatisticsData
        {
            public int TotalKills { get; set; }
            public int TotalDeaths { get; set; }
            public int TotalDamageDealt { get; set; }
            public int TotalDamageTaken { get; set; }
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
            public float TotalPlayTime { get; set; }
        }
        
        public class PlayerPetData
        {
            public int ActivePetId { get; set; } = -1;
            public int PetLevel { get; set; } = 1;
        }
        
        public class PlayerSkillData
        {
            public int[] LearnedSkills { get; set; } = new int[0];
        }
        
        public class PlayerSystemData
        {
            public bool[] ExploredAreas { get; set; } = new bool[10];
        }
    }
}
