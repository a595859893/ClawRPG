using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 战斗操作类型
    /// </summary>
    public enum BattleActionType
    {
        Attack,         // 普通攻击
        Skill,          // 技能释放
        BuffApply,      // Buff施加
        BuffRemove,     // Buff移除
        Heal,           // 治疗
        Damage,         // 受到伤害
        Death,          // 死亡
        Revive,         // 复活
        Dodge,          // 闪避
        Block,          // 格挡
        Counter         // 反击
    }

    /// <summary>
    /// 战斗同步数据
    /// </summary>
    public class BattleSyncData
    {
        /// <summary>
        /// 战斗操作消息
        /// </summary>
        public class BattleAction
        {
            public string ActionId { get; set; } = "";
            public int PlayerId { get; set; }
            public string PlayerName { get; set; } = "";
            public BattleActionType Type { get; set; }
            public string SkillId { get; set; } = "";
            public float Value { get; set; }  // 伤害值/治疗值
            public float TargetX { get; set; }
            public float TargetY { get; set; }
            public int TargetId { get; set; }
            public bool IsCritical { get; set; }
            public long Timestamp { get; set; }
            
            public BattleAction()
            {
                ActionId = Guid.NewGuid().ToString("N")[..12].ToUpper();
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        /// <summary>
        /// 玩家战斗状态
        /// </summary>
        public class PlayerBattleState
        {
            public int PlayerId { get; set; }
            public string PlayerName { get; set; } = "";
            public float Health { get; set; }
            public float MaxHealth { get; set; }
            public float Mana { get; set; }
            public float MaxMana { get; set; }
            public List<BuffState> ActiveBuffs { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public bool IsDead { get; set; }
            public long LastUpdate { get; set; }
            
            public PlayerBattleState()
            {
                ActiveBuffs = new List<BuffState>();
                LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        /// <summary>
        /// Buff状态
        /// </summary>
        public class BuffState
        {
            public string BuffId { get; set; } = "";
            public string BuffName { get; set; } = "";
            public int Stacks { get; set; }
            public float Duration { get; set; }  // 剩余持续时间(秒)
            public bool IsDebuff { get; set; }
        }

        /// <summary>
        /// 敌人战斗状态
        /// </summary>
        public class EnemyBattleState
        {
            public int EnemyId { get; set; }
            public string EnemyType { get; set; } = "";
            public float Health { get; set; }
            public float MaxHealth { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float AggroPlayerId { get; set; }  // 当前仇恨目标
            public bool IsDead { get; set; }
            public long LastUpdate { get; set; }
            
            public EnemyBattleState()
            {
                LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        /// <summary>
        /// 完整战斗快照（用于全量同步）
        /// </summary>
        public class BattleSnapshot
        {
            public string SessionId { get; set; } = "";
            public long Timestamp { get; set; }
            public List<PlayerBattleState> Players { get; set; }
            public List<EnemyBattleState> Enemies { get; set; }
            
            public BattleSnapshot()
            {
                Players = new List<PlayerBattleState>();
                Enemies = new List<EnemyBattleState>();
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        /// <summary>
        /// 战斗同步配置
        /// </summary>
        public class BattleSyncConfig
        {
            // 同步频率 (Hz)
            public int StateSyncRate { get; set; } = 20;  // 50ms 间隔
            public int ActionBroadcastRate { get; set; } = 30;  // ~33ms 间隔
            
            // 延迟目标 (ms)
            public int TargetLatencyMs { get; set; } = 100;
            
            // 缓冲区大小
            public int ActionBufferSize { get; set; } = 100;
            public int MaxBuffsPerPlayer { get; set; } = 10;
            
            // 压缩设置
            public bool EnableDeltaCompression { get; set; } = true;
            public bool EnableActionBatching { get; set; } = true;
        }
    }
}
