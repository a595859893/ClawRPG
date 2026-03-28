using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Save data structures and data management for game persistence.
    /// Contains all data classes needed for saving and loading game state.
    /// </summary>
    public class SaveDataManager
    {
        /// <summary>
        /// Main container for all save data including player info, inventory, quests, and system data.
        /// </summary>
        public class SaveData
        {
            /// <summary>Save slot index.</summary>
            public int Slot { get; set; }
            
            /// <summary>Name of the save file.</summary>
            public string SaveName { get; set; } = "";
            
            /// <summary>Timestamp when the save was created.</summary>
            public DateTime SaveTime { get; set; }
            
            /// <summary>Total play time for this save.</summary>
            public TimeSpan PlayTime { get; set; }
            
            /// <summary>Name of the current location/area.</summary>
            public string LocationName { get; set; } = "Unknown";
            
            // Player data
            /// <summary>Player level.</summary>
            public int Level { get; set; } = 1;
            
            /// <summary>Current experience points.</summary>
            public int Experience { get; set; }
            
            /// <summary>Current health points.</summary>
            public int CurrentHealth { get; set; } = 100;
            
            /// <summary>Maximum health points.</summary>
            public int MaxHealth { get; set; } = 100;
            
            /// <summary>Current mana points.</summary>
            public int CurrentMana { get; set; } = 50;
            
            /// <summary>Maximum mana points.</summary>
            public int MaxMana { get; set; } = 50;
            
            /// <summary>Current gold amount.</summary>
            public int Gold { get; set; }
            
            /// <summary>Player strength attribute.</summary>
            public int Strength { get; set; } = 10;
            
            /// <summary>Player agility attribute.</summary>
            public int Agility { get; set; } = 10;
            
            /// <summary>Player intelligence attribute.</summary>
            public int Intelligence { get; set; } = 10;
            
            // Position
            /// <summary>Player X position.</summary>
            public float X { get; set; }
            
            /// <summary>Player Y position.</summary>
            public float Y { get; set; }
            
            // Inventory
            /// <summary>Inventory item IDs.</summary>
            public int[] Inventory { get; set; } = new int[30];
            
            /// <summary>Inventory item quantities.</summary>
            public int[] InventoryCounts { get; set; } = new int[30];
            
            /// <summary>Equipped item IDs.</summary>
            public int[] Equipment { get; set; } = new int[4];
            
            // Quest progress
            /// <summary>Completed quest IDs.</summary>
            public int[] CompletedQuests { get; set; } = new int[0];
            
            /// <summary>Active quest IDs.</summary>
            public int[] ActiveQuests { get; set; } = new int[0];
            
            /// <summary>Quest progress values.</summary>
            public int[] QuestProgress { get; set; } = new int[0];
            
            // Skills
            /// <summary>Learned skill IDs.</summary>
            public int[] LearnedSkills { get; set; } = new int[0];
            
            // World state
            /// <summary>Current area/region identifier.</summary>
            public string CurrentArea { get; set; } = "forest";
            
            /// <summary>Explored area flags.</summary>
            public bool[] ExploredAreas { get; set; } = new bool[10];
            
            // Pet data
            /// <summary>Active pet ID.</summary>
            public int ActivePetId { get; set; } = -1;
            
            /// <summary>Pet level.</summary>
            public int PetLevel { get; set; } = 1;
            
            // Game stats
            /// <summary>Total enemies killed.</summary>
            public int TotalKills { get; set; }
            
            /// <summary>Total times player died.</summary>
            public int TotalDeaths { get; set; }
            
            /// <summary>Total damage dealt to enemies.</summary>
            public int TotalDamageDealt { get; set; }
            
            /// <summary>Total damage taken from enemies.</summary>
            public int TotalDamageTaken { get; set; }
            
            // Extended game stats
            /// <summary>Total healing received.</summary>
            public int TotalHealing { get; set; }
            
            /// <summary>Total critical hits landed.</summary>
            public int CriticalHits { get; set; }
            
            /// <summary>Total perfect blocks executed.</summary>
            public int PerfectBlocks { get; set; }
            
            /// <summary>Total dodges performed.</summary>
            public int Dodges { get; set; }
            
            /// <summary>Total gold earned.</summary>
            public int GoldEarned { get; set; }
            
            /// <summary>Total gold spent.</summary>
            public int GoldSpent { get; set; }
            
            /// <summary>Total experience gained.</summary>
            public int ExperienceGained { get; set; }
            
            /// <summary>Total items collected.</summary>
            public int ItemsCollected { get; set; }
            
            /// <summary>Total items crafted.</summary>
            public int ItemsCrafted { get; set; }
            
            /// <summary>Total quests completed.</summary>
            public int QuestsCompleted { get; set; }
            
            /// <summary>Total skills learned.</summary>
            public int SkillsLearned { get; set; }
            
            /// <summary>Total skills used.</summary>
            public int SkillsUsed { get; set; }
            
            /// <summary>Total regions discovered.</summary>
            public int RegionsDiscovered { get; set; }
            
            /// <summary>Total enemies encountered.</summary>
            public int EnemiesEncountered { get; set; }
            
            /// <summary>Total bosses defeated.</summary>
            public int BossesDefeated { get; set; }
            
            /// <summary>Total play time in seconds.</summary>
            public float TotalPlayTime { get; set; }
            
            /// <summary>Highest level achieved.</summary>
            public int HighestLevel { get; set; }
            
            /// <summary>Highest combo achieved.</summary>
            public int HighestCombo { get; set; }
            
            /// <summary>Total achievements unlocked.</summary>
            public int AchievementsUnlocked { get; set; }
            
            // Combo system data
            /// <summary>Combo system serialized data.</summary>
            public Dictionary<string, Variant> ComboData { get; set; }
            
            /// <summary>Combo forget system serialized data (REQ-154).</summary>
            public Dictionary<string, object> ComboForgetData { get; set; }
            
            // Title system data
            /// <summary>Current title ID.</summary>
            public string CurrentTitleId { get; set; } = "";
            
            /// <summary>Array of unlocked title IDs.</summary>
            public string[] UnlockedTitleIds { get; set; } = new string[0];
            
            // Quick slot data
            /// <summary>Quick slot item IDs.</summary>
            public string[] QuickSlotItemIds { get; set; } = new string[9];
            
            /// <summary>Quick slot quantities.</summary>
            public int[] QuickSlotQuantities { get; set; } = new int[9];
            
            /// <summary>Mount system data dictionary.</summary>
            public Dictionary<string, Dictionary<string, object>> MountData { get; set; } = new();
            
            /// <summary>Bookmark system data dictionary.</summary>
            public Dictionary<string, object> BookmarkData { get; set; } = new();
            
            /// <summary>Auto bookmark system data dictionary.</summary>
            public Dictionary<string, object> AutoBookmarkData { get; set; } = new();
            
            /// <summary>Enhancement system data dictionary.</summary>
            public Dictionary<string, object> EnhancementData { get; set; } = new();
            
            /// <summary>Auto potion system data dictionary.</summary>
            public Dictionary<string, object> AutoPotionData { get; set; } = new();
            
            /// <summary>Unlocked equipment visuals dictionary.</summary>
            public Dictionary<string, string[]> UnlockedVisuals { get; set; } = new();
            
            /// <summary>Enchantment system data dictionary.</summary>
            public Dictionary<string, object> EnchantmentData { get; set; } = new();
            
            /// <summary>Bounty system data dictionary.</summary>
            public Dictionary<string, object> BountyData { get; set; } = new();
            
            /// <summary>Weather system data dictionary.</summary>
            public Dictionary<string, object> WeatherData { get; set; } = new();
            
            /// <summary>Equipment visuals data dictionary.</summary>
            public Dictionary<string, string> EquipmentVisualsData { get; set; } = new();
            
            /// <summary>Keybinding data dictionary.</summary>
            public Dictionary<string, int> KeybindingData { get; set; } = new();
            
            /// <summary>Pet story system data dictionary.</summary>
            public Dictionary<string, object> PetStoryData { get; set; } = new();
            
            /// <summary>Pet egg system data dictionary.</summary>
            public Dictionary<string, object> PetEggData { get; set; } = new();
            
            /// <summary>Emote system data dictionary.</summary>
            public Dictionary<string, object> EmoteData { get; set; } = new();
            
            /// <summary>Sealed Tower system data dictionary.</summary>
            public Dictionary<string, object> SealedTowerData { get; set; } = new();

            /// <summary>Prestige system data dictionary.</summary>
            public Dictionary<string, object> PrestigeData { get; set; } = new();
            
            /// <summary>Quick Mode Reward system data dictionary.</summary>
            public Dictionary<string, object> QuickModeRewardData { get; set; } = new();

            /// <summary>Style Mastery system data dictionary.</summary>
            public Dictionary<string, object> StyleMasteryData { get; set; } = new();

            /// <summary>Guild Quest system data dictionary.</summary>
            public Dictionary<string, object> GuildQuestData { get; set; } = new();
            
            /// <summary>Legacy player data (for backwards compatibility).</summary>
            public object PlayerData { get; set; }
            
            // ===== Composition Data Classes (New - Recommended) =====
            
            /// <summary>Basic player attributes data.</summary>
            public PlayerBasicData BasicData { get; set; } = new();
            
            /// <summary>Inventory and equipment data.</summary>
            public PlayerInventoryData InventoryData { get; set; } = new();
            
            /// <summary>Quest progress data.</summary>
            public PlayerQuestData QuestData { get; set; } = new();
            
            /// <summary>Statistics data.</summary>
            public PlayerStatisticsData StatisticsData { get; set; } = new();
            
            /// <summary>Pet data.</summary>
            public PlayerPetData PetData { get; set; } = new();
            
            /// <summary>Skill data.</summary>
            public PlayerSkillData SkillData { get; set; } = new();
            
            /// <summary>System data.</summary>
            public PlayerSystemData SystemData { get; set; } = new();
        }
        
        /// <summary>
        /// Save slot metadata stored separately for quick loading.
        /// </summary>
        public class SaveSlotInfo
        {
            /// <summary>Save slot index.</summary>
            public int Slot { get; set; }
            
            /// <summary>Name of the save.</summary>
            public string SaveName { get; set; }
            
            /// <summary>Timestamp when saved.</summary>
            public DateTime SaveTime { get; set; }
            
            /// <summary>Total play time.</summary>
            public TimeSpan PlayTime { get; set; }
            
            /// <summary>Current location name.</summary>
            public string LocationName { get; set; }
            
            /// <summary>Player level at save time.</summary>
            public int Level { get; set; }
        }
        
        // ===== Player Data Composition Classes =====
        
        /// <summary>
        /// Basic player attribute data.
        /// </summary>
        public class PlayerBasicData
        {
            /// <summary>Player level.</summary>
            public int Level { get; set; } = 1;
            
            /// <summary>Current experience points.</summary>
            public int Experience { get; set; }
            
            /// <summary>Current health.</summary>
            public int CurrentHealth { get; set; } = 100;
            
            /// <summary>Maximum health.</summary>
            public int MaxHealth { get; set; } = 100;
            
            /// <summary>Current mana.</summary>
            public int CurrentMana { get; set; } = 50;
            
            /// <summary>Maximum mana.</summary>
            public int MaxMana { get; set; } = 50;
            
            /// <summary>Current gold.</summary>
            public int Gold { get; set; }
            
            /// <summary>Strength attribute.</summary>
            public int Strength { get; set; } = 10;
            
            /// <summary>Agility attribute.</summary>
            public int Agility { get; set; } = 10;
            
            /// <summary>Intelligence attribute.</summary>
            public int Intelligence { get; set; } = 10;
            
            /// <summary>X position.</summary>
            public float X { get; set; }
            
            /// <summary>Y position.</summary>
            public float Y { get; set; }
            
            /// <summary>Current area identifier.</summary>
            public string CurrentArea { get; set; } = "forest";
        }
        
        /// <summary>
        /// Player inventory and equipment data.
        /// </summary>
        public class PlayerInventoryData
        {
            /// <summary>Inventory item IDs.</summary>
            public int[] Inventory { get; set; } = new int[30];
            
            /// <summary>Inventory item quantities.</summary>
            public int[] InventoryCounts { get; set; } = new int[30];
            
            /// <summary>Equipped item IDs.</summary>
            public int[] Equipment { get; set; } = new int[4];
        }
        
        /// <summary>
        /// Player quest progress data.
        /// </summary>
        public class PlayerQuestData
        {
            /// <summary>Completed quest IDs.</summary>
            public int[] CompletedQuests { get; set; } = new int[0];
            
            /// <summary>Active quest IDs.</summary>
            public int[] ActiveQuests { get; set; } = new int[0];
            
            /// <summary>Quest progress values.</summary>
            public int[] QuestProgress { get; set; } = new int[0];
        }
        
        /// <summary>
        /// Player statistics data.
        /// </summary>
        public class PlayerStatisticsData
        {
            /// <summary>Total kills.</summary>
            public int TotalKills { get; set; }
            
            /// <summary>Total deaths.</summary>
            public int TotalDeaths { get; set; }
            
            /// <summary>Total damage dealt.</summary>
            public int TotalDamageDealt { get; set; }
            
            /// <summary>Total damage taken.</summary>
            public int TotalDamageTaken { get; set; }
            
            /// <summary>Total healing.</summary>
            public int TotalHealing { get; set; }
            
            /// <summary>Critical hits count.</summary>
            public int CriticalHits { get; set; }
            
            /// <summary>Perfect blocks count.</summary>
            public int PerfectBlocks { get; set; }
            
            /// <summary>Dodges count.</summary>
            public int Dodges { get; set; }
            
            /// <summary>Gold earned.</summary>
            public int GoldEarned { get; set; }
            
            /// <summary>Gold spent.</summary>
            public int GoldSpent { get; set; }
            
            /// <summary>Experience gained.</summary>
            public int ExperienceGained { get; set; }
            
            /// <summary>Items collected.</summary>
            public int ItemsCollected { get; set; }
            
            /// <summary>Items crafted.</summary>
            public int ItemsCrafted { get; set; }
            
            /// <summary>Quests completed.</summary>
            public int QuestsCompleted { get; set; }
            
            /// <summary>Skills learned.</summary>
            public int SkillsLearned { get; set; }
            
            /// <summary>Skills used.</summary>
            public int SkillsUsed { get; set; }
            
            /// <summary>Total play time in seconds.</summary>
            public float TotalPlayTime { get; set; }
        }
        
        /// <summary>
        /// Player pet data.
        /// </summary>
        public class PlayerPetData
        {
            /// <summary>Active pet ID (-1 if no pet).</summary>
            public int ActivePetId { get; set; } = -1;
            
            /// <summary>Pet level.</summary>
            public int PetLevel { get; set; } = 1;
        }
        
        /// <summary>
        /// Player skill data.
        /// </summary>
        public class PlayerSkillData
        {
            /// <summary>Learned skill IDs.</summary>
            public int[] LearnedSkills { get; set; } = new int[0];
        }
        
        /// <summary>
        /// Player system data including world state.
        /// </summary>
        public class PlayerSystemData
        {
            /// <summary>Explored area flags.</summary>
            public bool[] ExploredAreas { get; set; } = new bool[10];
        }
    }
}
