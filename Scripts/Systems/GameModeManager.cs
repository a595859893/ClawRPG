using Godot;
using System;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Game mode manager that provides quick mode switching and configuration access.
    /// Coordinates with GameModeConfig to manage game mode settings.
    /// </summary>
    public class GameModeManager : BaseSystem
    {
        private static GameModeManager _instance;
        
        /// <summary>
        /// Gets the singleton instance of GameModeManager.
        /// </summary>
        public static GameModeManager Instance => _instance;

        /// <summary>
        /// Gets the current game mode configuration.
        /// </summary>
        /// <value>GameModeConfig instance for accessing mode settings.</value>
        public GameModeConfig Config { get; private set; }

        // Game mode change signals
        public static Signal ModeChanged => new("mode_changed");
        public static Signal QuickModeEnabled => new("quick_mode_enabled");
        public static Signal QuickModeDisabled => new("quick_mode_disabled");

        /// <summary>
        /// Creates a new GameModeManager instance.
        /// </summary>
        public GameModeManager()
        {
            _instance = this;
            Config = GameModeConfig.Instance;
        }

        public override void _Ready()
        {
            GD.Print("[GameModeManager] Initialized");
        }

        /// <summary>
        /// Checks if quick mode is currently active.
        /// </summary>
        /// <returns>True if quick mode is enabled.</returns>
        public bool IsQuickMode()
        {
            return Config.IsQuickMode;
        }

        /// <summary>
        /// Enables quick mode.
        /// </summary>
        public void EnableQuickMode()
        {
            Config.EnableQuickMode();
            QuickModeEnabled?.Emit();
            ModeChanged?.Emit();
            GD.Print("[GameModeManager] Quick Mode enabled");
        }

        /// <summary>
        /// Disables quick mode.
        /// </summary>
        public void DisableQuickMode()
        {
            Config.DisableQuickMode();
            QuickModeDisabled?.Emit();
            ModeChanged?.Emit();
            GD.Print("[GameModeManager] Quick Mode disabled");
        }

        /// <summary>
        /// Toggles quick mode on or off.
        /// </summary>
        public void ToggleQuickMode()
        {
            Config.ToggleQuickMode();
            
            if (Config.IsQuickMode)
            {
                QuickModeEnabled?.Emit();
                GD.Print("[GameModeManager] Quick Mode toggled ON");
            }
            else
            {
                QuickModeDisabled?.Emit();
                GD.Print("[GameModeManager] Quick Mode toggled OFF");
            }
            
            ModeChanged?.Emit();
        }

        /// <summary>
        /// Sets the game mode to a specific type.
        /// </summary>
        /// <param name="mode">The game mode to set.</param>
        public void SetGameMode(GameModeType mode)
        {
            Config.SetGameMode(mode);
            ModeChanged?.Emit();
            GD.Print($"[GameModeManager] Game mode set to: {mode}");
        }

        /// <summary>
        /// Gets the room range adjusted for the current game mode.
        /// </summary>
        /// <param name="originalMin">Original minimum room count.</param>
        /// <param name="originalMax">Original maximum room count.</param>
        /// <returns>Tuple of (min, max) adjusted room counts.</returns>
        public (int min, int max) GetRoomRange(int originalMin, int originalMax)
        {
            float multiplier = Config.GetRoomCountMultiplier();
            int min = Mathf.Max(2, (int)(originalMin * multiplier));
            int max = Mathf.Max(min + 1, (int)(originalMax * multiplier));
            return (min, max);
        }

        /// <summary>
        /// Gets the enemy count adjusted for the current game mode.
        /// </summary>
        /// <param name="originalCount">Original enemy count.</param>
        /// <returns>Adjusted enemy count.</returns>
        public int GetEnemyCount(int originalCount)
        {
            return (int)(originalCount * Config.GetEnemyCountMultiplier());
        }

        /// <summary>
        /// Gets the enemy strength multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for enemy stats.</returns>
        public float GetEnemyStrengthMultiplier()
        {
            return Config.GetEnemyStrengthMultiplier();
        }

        /// <summary>
        /// Gets the spawn interval adjusted for the current game mode.
        /// </summary>
        /// <param name="originalInterval">Original spawn interval.</param>
        /// <returns>Adjusted spawn interval.</returns>
        public float GetSpawnInterval(float originalInterval)
        {
            return originalInterval * Config.GetSpawnIntervalMultiplier();
        }

        /// <summary>
        /// Gets the max enemies count adjusted for the current game mode.
        /// </summary>
        /// <param name="originalMax">Original max enemy count.</param>
        /// <returns>Adjusted max enemy count.</returns>
        public int GetMaxEnemies(int originalMax)
        {
            return (int)(originalMax * Config.GetMaxEnemiesMultiplier());
        }

        /// <summary>
        /// Gets the treasure value multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for treasure values.</returns>
        public float GetTreasureMultiplier()
        {
            return Config.GetTreasureValueMultiplier();
        }

        /// <summary>
        /// Gets the XP multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for experience points.</returns>
        public float GetXPMultiplier()
        {
            return Config.GetXPBonusMultiplier();
        }

        /// <summary>
        /// Gets the gold drop multiplier for the current mode.
        /// </summary>
        /// <returns>Multiplier for gold drops.</returns>
        public float GetGoldMultiplier()
        {
            return Config.GetGoldDropMultiplier();
        }

        /// <summary>
        /// Gets the name of the current game mode.
        /// </summary>
        /// <returns>String representation of the current mode.</returns>
        public string GetCurrentModeName()
        {
            return Config.CurrentMode.ToString();
        }

        /// <summary>
        /// Gets the target game duration in minutes for the current mode.
        /// </summary>
        /// <returns>Target duration in minutes.</returns>
        public int GetTargetDurationMinutes()
        {
            return Config.CurrentMode switch
            {
                GameModeType.Quick => Config.QuickMode.TargetDurationMinutes,
                GameModeType.Challenge => Config.ChallengeMode.TargetDurationMinutes,
                _ => Config.NormalMode.TargetDurationMinutes
            };
        }

        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["current_mode"] = (int)Config.CurrentMode;
            data["is_quick_mode"] = Config.IsQuickMode;
            return data;
        }

        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.Contains("current_mode"))
            {
                var mode = (GameModeType)(int)data["current_mode"];
                Config.SetGameMode(mode);
            }
            
            if (data.Contains("is_quick_mode"))
            {
                bool isQuick = (bool)data["is_quick_mode"];
                if (isQuick && !Config.IsQuickMode)
                    Config.EnableQuickMode();
                else if (!isQuick && Config.IsQuickMode)
                    Config.DisableQuickMode();
            }
        }
    }
}
