using System;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Defines the available game mode types.
    /// </summary>
    public enum GameModeType
    {
        /// <summary>Standard gameplay mode.</summary>
        Normal,
        
        /// <summary>Quick mode with reduced duration and content.</summary>
        Quick,
        
        /// <summary>Challenge mode with increased difficulty and rewards.</summary>
        Challenge,
        
        /// <summary>Boss rush mode focusing on boss battles.</summary>
        BossRush
    }

    /// <summary>
    /// Game mode configuration that manages settings for different game modes.
    /// </summary>
    [Serializable]
    public class GameModeConfig
    {
        // Singleton instance
        private static GameModeConfig _instance;
        
        /// <summary>
        /// Gets the singleton instance of GameModeConfig.
        /// </summary>
        public static GameModeConfig Instance => _instance ??= new GameModeConfig();

        /// <summary>
        /// Gets or sets the current game mode.
        /// </summary>
        public GameModeType CurrentMode { get; set; } = GameModeType.Normal;

        /// <summary>
        /// Quick mode configuration settings.
        /// </summary>
        public QuickModeConfig QuickMode { get; set; } = new QuickModeConfig();

        /// <summary>
        /// Normal mode configuration settings.
        /// </summary>
        public NormalModeConfig NormalMode { get; set; } = new NormalModeConfig();

        /// <summary>
        /// Challenge mode configuration settings.
        /// </summary>
        public ChallengeModeConfig ChallengeMode { get; set; } = new ChallengeModeConfig();

        /// <summary>
        /// Gets whether quick mode is currently active.
        /// </summary>
        /// <value>True if current mode is Quick.</value>
        public bool IsQuickMode => CurrentMode == GameModeType.Quick;

        /// <summary>
        /// Gets the room count multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for dungeon room count.</returns>
        public float GetRoomCountMultiplier()
        {
            return CurrentMode switch
            {
                GameModeType.Quick => QuickMode.RoomCountMultiplier,
                GameModeType.Challenge => ChallengeMode.RoomCountMultiplier,
                GameModeType.BossRush => 0.5f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets the floor count multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for dungeon floor count.</returns>
        public float GetFloorCountMultiplier()
        {
            return CurrentMode switch
            {
                GameModeType.Quick => QuickMode.FloorCountMultiplier,
                GameModeType.Challenge => ChallengeMode.FloorCountMultiplier,
                GameModeType.BossRush => 0.3f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets the enemy count multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for enemy spawn count.</returns>
        public float GetEnemyCountMultiplier()
        {
            return CurrentMode switch
            {
                GameModeType.Quick => QuickMode.EnemyCountMultiplier,
                GameModeType.Challenge => ChallengeMode.EnemyCountMultiplier,
                GameModeType.BossRush => 1.5f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets the enemy strength multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for enemy stats.</returns>
        public float GetEnemyStrengthMultiplier()
        {
            return CurrentMode switch
            {
                GameModeType.Quick => QuickMode.EnemyStrengthMultiplier,
                GameModeType.Challenge => ChallengeMode.EnemyStrengthMultiplier,
                GameModeType.BossRush => 2.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets the spawn interval multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for enemy spawn timing.</returns>
        public float GetSpawnIntervalMultiplier()
        {
            return CurrentMode switch
            {
                GameModeType.Quick => QuickMode.SpawnIntervalMultiplier,
                GameModeType.Challenge => ChallengeMode.SpawnIntervalMultiplier,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets the max enemies multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for maximum enemy count.</returns>
        public float GetMaxEnemiesMultiplier()
        {
            return CurrentMode switch
            {
                GameModeType.Quick => QuickMode.MaxEnemiesMultiplier,
                GameModeType.Challenge => ChallengeMode.MaxEnemiesMultiplier,
                GameModeType.BossRush => 2.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets the treasure value multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for treasure values.</returns>
        public float GetTreasureValueMultiplier()
        {
            return CurrentMode switch
            {
                GameModeType.Quick => QuickMode.TreasureValueMultiplier,
                GameModeType.Challenge => ChallengeMode.TreasureValueMultiplier,
                GameModeType.BossRush => 2.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets the XP bonus multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for experience points.</returns>
        public float GetXPBonusMultiplier()
        {
            return CurrentMode switch
            {
                GameModeType.Quick => QuickMode.XPBonusMultiplier,
                GameModeType.Challenge => ChallengeMode.XPBonusMultiplier,
                GameModeType.BossRush => 1.5f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets the gold drop multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for gold drops.</returns>
        public float GetGoldDropMultiplier()
        {
            return CurrentMode switch
            {
                GameModeType.Quick => QuickMode.GoldDropMultiplier,
                GameModeType.Challenge => ChallengeMode.GoldDropMultiplier,
                GameModeType.BossRush => 2.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Sets the current game mode.
        /// </summary>
        /// <param name="mode">The game mode to set.</param>
        public void SetGameMode(GameModeType mode)
        {
            CurrentMode = mode;
            Godot.GD.Print($"[GameModeConfig] Game mode set to: {mode}");
        }

        /// <summary>
        /// Enables quick mode.
        /// </summary>
        public void EnableQuickMode()
        {
            SetGameMode(GameModeType.Quick);
        }

        /// <summary>
        /// Disables quick mode and returns to normal mode.
        /// </summary>
        public void DisableQuickMode()
        {
            SetGameMode(GameModeType.Normal);
        }

        /// <summary>
        /// Toggles quick mode on or off.
        /// </summary>
        public void ToggleQuickMode()
        {
            if (IsQuickMode)
                DisableQuickMode();
            else
                EnableQuickMode();
        }
    }

    /// <summary>
    /// Configuration settings for Quick mode.
    /// </summary>
    [Serializable]
    public class QuickModeConfig
    {
        /// <summary>Whether quick mode is enabled.</summary>
        public bool Enabled { get; set; } = false;

        /// <summary>Target duration for a quick mode session in minutes.</summary>
        public int TargetDurationMinutes { get; set; } = 15;

        /// <summary>Multiplier for room count (reduces rooms).</summary>
        public float RoomCountMultiplier { get; set; } = 0.6f;

        /// <summary>Multiplier for floor count (reduces floors).</summary>
        public float FloorCountMultiplier { get; set; } = 0.6f;

        /// <summary>Multiplier for enemy count (reduces enemies).</summary>
        public float EnemyCountMultiplier { get; set; } = 0.5f;

        /// <summary>Multiplier for enemy strength (weakens enemies).</summary>
        public float EnemyStrengthMultiplier { get; set; } = 0.7f;

        /// <summary>Multiplier for spawn interval (faster spawns).</summary>
        public float SpawnIntervalMultiplier { get; set; } = 0.8f;

        /// <summary>Multiplier for max enemies.</summary>
        public float MaxEnemiesMultiplier { get; set; } = 0.6f;

        /// <summary>Multiplier for treasure value.</summary>
        public float TreasureValueMultiplier { get; set; } = 1.2f;

        /// <summary>Bonus multiplier for XP.</summary>
        public float XPBonusMultiplier { get; set; } = 1.3f;

        /// <summary>Bonus multiplier for gold drops.</summary>
        public float GoldDropMultiplier { get; set; } = 1.3f;

        /// <summary>Whether to skip normal enemy encounters.</summary>
        public bool SkipNormalEncounters { get; set; } = false;

        /// <summary>Whether to reduce special rooms.</summary>
        public bool ReduceSpecialRooms { get; set; } = true;
    }

    /// <summary>
    /// Configuration settings for Normal mode.
    /// </summary>
    [Serializable]
    public class NormalModeConfig
    {
        /// <summary>Target duration for a normal mode session in minutes.</summary>
        public int TargetDurationMinutes { get; set; } = 45;

        /// <summary>Multiplier for room count.</summary>
        public float RoomCountMultiplier { get; set; } = 1.0f;

        /// <summary>Multiplier for floor count.</summary>
        public float FloorCountMultiplier { get; set; } = 1.0f;

        /// <summary>Multiplier for enemy count.</summary>
        public float EnemyCountMultiplier { get; set; } = 1.0f;

        /// <summary>Multiplier for enemy strength.</summary>
        public float EnemyStrengthMultiplier { get; set; } = 1.0f;

        /// <summary>Multiplier for spawn interval.</summary>
        public float SpawnIntervalMultiplier { get; set; } = 1.0f;

        /// <summary>Multiplier for max enemies.</summary>
        public float MaxEnemiesMultiplier { get; set; } = 1.0f;

        /// <summary>Multiplier for treasure value.</summary>
        public float TreasureValueMultiplier { get; set; } = 1.0f;

        /// <summary>Multiplier for XP.</summary>
        public float XPBonusMultiplier { get; set; } = 1.0f;

        /// <summary>Multiplier for gold drops.</summary>
        public float GoldDropMultiplier { get; set; } = 1.0f;
    }

    /// <summary>
    /// Configuration settings for Challenge mode.
    /// </summary>
    [Serializable]
    public class ChallengeModeConfig
    {
        /// <summary>Whether challenge mode is enabled.</summary>
        public bool Enabled { get; set; } = false;

        /// <summary>Target duration for a challenge mode session in minutes.</summary>
        public int TargetDurationMinutes { get; set; } = 30;

        /// <summary>Multiplier for room count.</summary>
        public float RoomCountMultiplier { get; set; } = 1.2f;

        /// <summary>Multiplier for floor count.</summary>
        public float FloorCountMultiplier { get; set; } = 1.0f;

        /// <summary>Multiplier for enemy count.</summary>
        public float EnemyCountMultiplier { get; set; } = 1.5f;

        /// <summary>Multiplier for enemy strength.</summary>
        public float EnemyStrengthMultiplier { get; set; } = 1.5f;

        /// <summary>Multiplier for spawn interval.</summary>
        public float SpawnIntervalMultiplier { get; set; } = 0.7f;

        /// <summary>Multiplier for max enemies.</summary>
        public float MaxEnemiesMultiplier { get; set; } = 1.5f;

        /// <summary>Multiplier for treasure value.</summary>
        public float TreasureValueMultiplier { get; set; } = 1.5f;

        /// <summary>Bonus multiplier for XP.</summary>
        public float XPBonusMultiplier { get; set; } = 2.0f;

        /// <summary>Bonus multiplier for gold drops.</summary>
        public float GoldDropMultiplier { get; set; } = 2.0f;

        /// <summary>Whether skills are disabled in challenge mode.</summary>
        public bool DisableSkills { get; set; } = false;

        /// <summary>Whether potions are disabled in challenge mode.</summary>
        public bool DisablePotions { get; set; } = false;
    }
}
