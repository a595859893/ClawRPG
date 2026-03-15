using System;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 游戏模式类型
    /// </summary>
    public enum GameModeType
    {
        Normal,     // 普通模式
        Quick,      // 快速模式
        Challenge,  // 挑战模式
        BossRush    // Boss rush模式
    }

    /// <summary>
    /// 游戏模式配置
    /// </summary>
    [Serializable]
    public class GameModeConfig
    {
        // 单例实例
        private static GameModeConfig _instance;
        public static GameModeConfig Instance => _instance ??= new GameModeConfig();

        // 当前游戏模式
        public GameModeType CurrentMode { get; set; } = GameModeType.Normal;

        // 快速模式配置
        public QuickModeConfig QuickMode { get; set; } = new QuickModeConfig();

        // 普通模式配置
        public NormalModeConfig NormalMode { get; set; } = new NormalModeConfig();

        // 挑战模式配置
        public ChallengeModeConfig ChallengeMode { get; set; } = new ChallengeModeConfig();

        /// <summary>
        /// 快速模式是否启用
        /// </summary>
        public bool IsQuickMode => CurrentMode == GameModeType.Quick;

        /// <summary>
        /// 获取当前模式的地下城房间数量乘数
        /// </summary>
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
        /// 获取当前模式的楼层数乘数
        /// </summary>
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
        /// 获取当前模式的敌人数量乘数
        /// </summary>
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
        /// 获取当前模式的敌人强度乘数
        /// </summary>
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
        /// 获取当前模式的敌人刷新间隔乘数
        /// </summary>
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
        /// 获取当前模式的最大敌人数量乘数
        /// </summary>
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
        /// 获取当前模式的宝藏价值乘数
        /// </summary>
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
        /// 获取当前模式的经验值乘数
        /// </summary>
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
        /// 获取当前模式的金币掉落乘数
        /// </summary>
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
        /// 设置游戏模式
        /// </summary>
        public void SetGameMode(GameModeType mode)
        {
            CurrentMode = mode;
            Godot.GD.Print($"[GameModeConfig] Game mode set to: {mode}");
        }

        /// <summary>
        /// 启用快速模式
        /// </summary>
        public void EnableQuickMode()
        {
            SetGameMode(GameModeType.Quick);
        }

        /// <summary>
        /// 禁用快速模式（切换回普通模式）
        /// </summary>
        public void DisableQuickMode()
        {
            SetGameMode(GameModeType.Normal);
        }

        /// <summary>
        /// 切换快速模式
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
    /// 快速模式配置
    /// </summary>
    [Serializable]
    public class QuickModeConfig
    {
        // 是否启用快速模式
        public bool Enabled { get; set; } = false;

        // 目标单局时长（分钟）
        public int TargetDurationMinutes { get; set; } = 15;

        // 房间数量乘数（减少房间）
        public float RoomCountMultiplier { get; set; } = 0.6f;

        // 楼层数量乘数（减少楼层）
        public float FloorCountMultiplier { get; set; } = 0.6f;

        // 敌人数量乘数（减少敌人）
        public float EnemyCountMultiplier { get; set; } = 0.5f;

        // 敌人强度乘数（削弱敌人）
        public float EnemyStrengthMultiplier { get; set; } = 0.7f;

        // 敌人生成间隔乘数（加快生成）
        public float SpawnIntervalMultiplier { get; set; } = 0.8f;

        // 最大敌人数量乘数
        public float MaxEnemiesMultiplier { get; set; } = 0.6f;

        // 宝藏价值乘数
        public float TreasureValueMultiplier { get; set; } = 1.2f;

        // 经验值加成乘数
        public float XPBonusMultiplier { get; set; } = 1.3f;

        // 金币掉落加成乘数
        public float GoldDropMultiplier { get; set; } = 1.3f;

        // 是否跳过普通敌人战斗（直接遭遇精英/Boss）
        public bool SkipNormalEncounters { get; set; } = false;

        // 是否减少陷阱和谜题房间
        public bool ReduceSpecialRooms { get; set; } = true;
    }

    /// <summary>
    /// 普通模式配置
    /// </summary>
    [Serializable]
    public class NormalModeConfig
    {
        // 目标单局时长（分钟）
        public int TargetDurationMinutes { get; set; } = 45;

        // 房间数量乘数
        public float RoomCountMultiplier { get; set; } = 1.0f;

        // 楼层数量乘数
        public float FloorCountMultiplier { get; set; } = 1.0f;

        // 敌人数量乘数
        public float EnemyCountMultiplier { get; set; } = 1.0f;

        // 敌人强度乘数
        public float EnemyStrengthMultiplier { get; set; } = 1.0f;

        // 敌人生成间隔乘数
        public float SpawnIntervalMultiplier { get; set; } = 1.0f;

        // 最大敌人数量乘数
        public float MaxEnemiesMultiplier { get; set; } = 1.0f;

        // 宝藏价值乘数
        public float TreasureValueMultiplier { get; set; } = 1.0f;

        // 经验值乘数
        public float XPBonusMultiplier { get; set; } = 1.0f;

        // 金币掉落乘数
        public float GoldDropMultiplier { get; set; } = 1.0f;
    }

    /// <summary>
    /// 挑战模式配置
    /// </summary>
    [Serializable]
    public class ChallengeModeConfig
    {
        // 是否启用挑战模式
        public bool Enabled { get; set; } = false;

        // 目标单局时长（分钟）
        public int TargetDurationMinutes { get; set; } = 30;

        // 房间数量乘数
        public float RoomCountMultiplier { get; set; } = 1.2f;

        // 楼层数量乘数
        public float FloorCountMultiplier { get; set; } = 1.0f;

        // 敌人数量乘数
        public float EnemyCountMultiplier { get; set; } = 1.5f;

        // 敌人强度乘数
        public float EnemyStrengthMultiplier { get; set; } = 1.5f;

        // 敌人生成间隔乘数
        public float SpawnIntervalMultiplier { get; set; } = 0.7f;

        // 最大敌人数量乘数
        public float MaxEnemiesMultiplier { get; set; } = 1.5f;

        // 宝藏价值乘数
        public float TreasureValueMultiplier { get; set; } = 1.5f;

        // 经验值加成乘数
        public float XPBonusMultiplier { get; set; } = 2.0f;

        // 金币掉落加成乘数
        public float GoldDropMultiplier { get; set; } = 2.0f;

        // 是否禁止使用技能
        public bool DisableSkills { get; set; } = false;

        // 是否禁止使用药水
        public bool DisablePotions { get; set; } = false;
    }
}
