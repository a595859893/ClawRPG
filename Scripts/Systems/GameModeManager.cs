using Godot;
using System;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 游戏模式管理器
    /// 提供快速模式切换和相关配置访问
    /// </summary>
    public class GameModeManager : Node
    {
        private static GameModeManager _instance;
        public static GameModeManager Instance => _instance;

        // 游戏配置
        public GameModeConfig Config { get; private set; }

        // 游戏模式变更信号
        public static Signal ModeChanged => new("mode_changed");
        public static Signal QuickModeEnabled => new("quick_mode_enabled");
        public static Signal QuickModeDisabled => new("quick_mode_disabled");

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
        /// 当前是否为快速模式
        /// </summary>
        public bool IsQuickMode()
        {
            return Config.IsQuickMode;
        }

        /// <summary>
        /// 启用快速模式
        /// </summary>
        public void EnableQuickMode()
        {
            Config.EnableQuickMode();
            QuickModeEnabled?.Emit();
            ModeChanged?.Emit();
            GD.Print("[GameModeManager] Quick Mode enabled");
        }

        /// <summary>
        /// 禁用快速模式
        /// </summary>
        public void DisableQuickMode()
        {
            Config.DisableQuickMode();
            QuickModeDisabled?.Emit();
            ModeChanged?.Emit();
            GD.Print("[GameModeManager] Quick Mode disabled");
        }

        /// <summary>
        /// 切换快速模式
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
        /// 设置游戏模式
        /// </summary>
        public void SetGameMode(GameModeType mode)
        {
            Config.SetGameMode(mode);
            ModeChanged?.Emit();
            GD.Print($"[GameModeManager] Game mode set to: {mode}");
        }

        /// <summary>
        /// 获取房间范围（用于快速模式减少房间数）
        /// </summary>
        public (int min, int max) GetRoomRange(int originalMin, int originalMax)
        {
            float multiplier = Config.GetRoomCountMultiplier();
            int min = Mathf.Max(2, (int)(originalMin * multiplier));
            int max = Mathf.Max(min + 1, (int)(originalMax * multiplier));
            return (min, max);
        }

        /// <summary>
        /// 获取敌人数量（用于快速模式减少敌人）
        /// </summary>
        public int GetEnemyCount(int originalCount)
        {
            return (int)(originalCount * Config.GetEnemyCountMultiplier());
        }

        /// <summary>
        /// 获取敌人强度乘数
        /// </summary>
        public float GetEnemyStrengthMultiplier()
        {
            return Config.GetEnemyStrengthMultiplier();
        }

        /// <summary>
        /// 获取敌人生成间隔
        /// </summary>
        public float GetSpawnInterval(float originalInterval)
        {
            return originalInterval * Config.GetSpawnIntervalMultiplier();
        }

        /// <summary>
        /// 获取最大敌人数量
        /// </summary>
        public int GetMaxEnemies(int originalMax)
        {
            return (int)(originalMax * Config.GetMaxEnemiesMultiplier());
        }

        /// <summary>
        /// 获取宝藏价值乘数
        /// </summary>
        public float GetTreasureMultiplier()
        {
            return Config.GetTreasureValueMultiplier();
        }

        /// <summary>
        /// 获取经验值乘数
        /// </summary>
        public float GetXPMultiplier()
        {
            return Config.GetXPBonusMultiplier();
        }

        /// <summary>
        /// 获取金币掉落乘数
        /// </summary>
        public float GetGoldMultiplier()
        {
            return Config.GetGoldDropMultiplier();
        }

        /// <summary>
        /// 获取当前模式名称
        /// </summary>
        public string GetCurrentModeName()
        {
            return Config.CurrentMode.ToString();
        }

        /// <summary>
        /// 获取目标游戏时长（分钟）
        /// </summary>
        public int GetTargetDurationMinutes()
        {
            return Config.CurrentMode switch
            {
                GameModeType.Quick => Config.QuickMode.TargetDurationMinutes,
                GameModeType.Challenge => Config.ChallengeMode.TargetDurationMinutes,
                _ => Config.NormalMode.TargetDurationMinutes
            };
        }
    }
}
