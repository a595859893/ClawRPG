using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 多人游戏大厅配置数据库
    /// 游戏模式、难度配置、房间设置
    /// </summary>
    public class MultiplayerLobbyDatabase : BaseSystem
    {
        public static MultiplayerLobbyDatabase Instance { get; private set; }
        
        // 游戏模式配置
        public class GameModeConfig
        {
            public string ModeId;
            public string DisplayName;
            public string Description;
            public int MinPlayers;
            public int MaxPlayers;
            public bool RequiresPassword;
            public int[] AvailableDifficulties;
            public string IconColor;
            public Dictionary<string, int> Rewards; // "gold" -> amount
        }
        
        // 难度配置
        public class DifficultyConfig
        {
            public int Level;
            public string Name;
            public float EnemyMultiplier;
            public float RewardMultiplier;
            public int RecommendedLevel;
        }
        
        // 游戏模式字典
        public Dictionary<string, GameModeConfig> GameModes = new Dictionary<string, GameModeConfig>();
        
        // 难度配置列表
        public List<DifficultyConfig> Difficulties = new List<DifficultyConfig>();
        
        // 房间设置模板
        public class RoomSettings
        {
            public string Name;
            public int MaxPlayers;
            public int TimeLimit; // 秒
            public bool AllowLateJoin;
            public bool FriendlyFire;
        }
        
        public Dictionary<string, RoomSettings> RoomSettingsTemplates = new Dictionary<string, RoomSettings>();
        
        public override void _Ready()
        {
            Instance = this;
            Name = "MultiplayerLobbyDatabase";
            InitializeGameModes();
            InitializeDifficulties();
            InitializeRoomSettings();
        }
        
        private void InitializeGameModes()
        {
            // Co-op Dungeon - 合作地下城
            GameModes["CoopDungeon"] = new GameModeConfig
            {
                ModeId = "CoopDungeon",
                DisplayName = "Co-op Dungeon",
                Description = "Team up with friends to conquer dungeons",
                MinPlayers = 1,
                MaxPlayers = 4,
                RequiresPassword = false,
                AvailableDifficulties = new int[] { 1, 2, 3, 4, 5 },
                IconColor = "#4CAF50",
                Rewards = new Dictionary<string, int>
                {
                    { "gold", 500 },
                    { "exp", 1000 }
                }
            };
            
            // PvP Battle - 玩家对战
            GameModes["PvPBattle"] = new GameModeConfig
            {
                ModeId = "PvPBattle",
                DisplayName = "PvP Battle",
                Description = "Fight against other players",
                MinPlayers = 2,
                MaxPlayers = 8,
                RequiresPassword = false,
                AvailableDifficulties = new int[] { 1, 2, 3 },
                IconColor = "#F44336",
                Rewards = new Dictionary<string, int>
                {
                    { "gold", 1000 },
                    { "exp", 1500 }
                }
            };
            
            // Racing - 竞速
            GameModes["Racing"] = new GameModeConfig
            {
                ModeId = "Racing",
                DisplayName = "Racing",
                Description = "Race through obstacle courses",
                MinPlayers = 2,
                MaxPlayers = 8,
                RequiresPassword = false,
                AvailableDifficulties = new int[] { 1, 2, 3 },
                IconColor = "#FF9800",
                Rewards = new Dictionary<string, int>
                {
                    { "gold", 300 },
                    { "exp", 500 }
                }
            };
            
            // Boss Rush - Boss rush
            GameModes["BossRush"] = new GameModeConfig
            {
                ModeId = "BossRush",
                DisplayName = "Boss Rush",
                Description = "Team up to defeat powerful bosses",
                MinPlayers = 1,
                MaxPlayers = 4,
                RequiresPassword = false,
                AvailableDifficulties = new int[] { 1, 2, 3, 4, 5 },
                IconColor = "#9C27B0",
                Rewards = new Dictionary<string, int>
                {
                    { "gold", 2000 },
                    { "exp", 3000 }
                }
            };
            
            // Treasure Hunt - 寻宝
            GameModes["TreasureHunt"] = new GameModeConfig
            {
                ModeId = "TreasureHunt",
                DisplayName = "Treasure Hunt",
                Description = "Find treasures faster than other teams",
                MinPlayers = 2,
                MaxPlayers = 4,
                RequiresPassword = false,
                AvailableDifficulties = new int[] { 1, 2, 3 },
                IconColor = "#FFD700",
                Rewards = new Dictionary<string, int>
                {
                    { "gold", 800 },
                    { "exp", 1200 }
                }
            };
            
            // Survival - 生存模式
            GameModes["Survival"] = new GameModeConfig
            {
                ModeId = "Survival",
                DisplayName = "Survival",
                Description = "Survive as long as possible against waves",
                MinPlayers = 1,
                MaxPlayers = 4,
                RequiresPassword = false,
                AvailableDifficulties = new int[] { 1, 2, 3, 4, 5 },
                IconColor = "#00BCD4",
                Rewards = new Dictionary<string, int>
                {
                    { "gold", 1500 },
                    { "exp", 2500 }
                }
            };
        }
        
        private void InitializeDifficulties()
        {
            Difficulties.Add(new DifficultyConfig { Level = 1, Name = "Easy", EnemyMultiplier = 0.8f, RewardMultiplier = 0.8f, RecommendedLevel = 1 });
            Difficulties.Add(new DifficultyConfig { Level = 2, Name = "Normal", EnemyMultiplier = 1.0f, RewardMultiplier = 1.0f, RecommendedLevel = 10 });
            Difficulties.Add(new DifficultyConfig { Level = 3, Name = "Hard", EnemyMultiplier = 1.5f, RewardMultiplier = 1.5f, RecommendedLevel = 25 });
            Difficulties.Add(new DifficultyConfig { Level = 4, Name = "Nightmare", EnemyMultiplier = 2.0f, RewardMultiplier = 2.5f, RecommendedLevel = 40 });
            Difficulties.Add(new DifficultyConfig { Level = 5, Name = "Legendary", EnemyMultiplier = 3.0f, RewardMultiplier = 4.0f, RecommendedLevel = 60 });
        }
        
        private void InitializeRoomSettings()
        {
            RoomSettingsTemplates["Quick"] = new RoomSettings
            {
                Name = "Quick Match",
                MaxPlayers = 4,
                TimeLimit = 600,
                AllowLateJoin = true,
                FriendlyFire = false
            };
            
            RoomSettingsTemplates["Standard"] = new RoomSettings
            {
                Name = "Standard",
                MaxPlayers = 4,
                TimeLimit = 1800,
                AllowLateJoin = true,
                FriendlyFire = false
            };
            
            RoomSettingsTemplates["Tournament"] = new RoomSettings
            {
                Name = "Tournament",
                MaxPlayers = 8,
                TimeLimit = 3600,
                AllowLateJoin = false,
                FriendlyFire = true
            };
        }
        
        public GameModeConfig GetGameMode(string modeId)
        {
            return GameModes.ContainsKey(modeId) ? GameModes[modeId] : null;
        }
        
        public DifficultyConfig GetDifficulty(int level)
        {
            return Difficulties.Find(d => d.Level == level);
        }
        
        public string[] GetAvailableGameModes()
        {
            string[] modes = new string[GameModes.Count];
            int i = 0;
            foreach (var kvp in GameModes)
            {
                modes[i++] = kvp.Key;
            }
            return modes;
        }
    }
}
