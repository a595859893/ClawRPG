using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ComboReplay
{
    /// <summary>
    /// Combo 回放数据 - 可序列化存档格式
    /// </summary>
    public class ComboReplayData
    {
        /// <summary>存档版本</summary>
        public int Version { get; set; } = 1;
        
        /// <summary>战斗随机种子</summary>
        public int Seed { get; set; }
        
        /// <summary>战斗开始绝对时间戳</summary>
        public double StartTimestamp { get; set; }
        
        /// <summary>战斗持续秒数</summary>
        public float DurationSeconds { get; set; }
        
        /// <summary>玩家使用的操作序列</summary>
        public List<PlayerActionRecord> Actions { get; set; } = new List<PlayerActionRecord>();
        
        /// <summary>完成的Combo序列</summary>
        public List<ComboRecord> Combos { get; set; } = new List<ComboRecord>();
        
        /// <summary>元数据</summary>
        public ReplayMetadata Metadata { get; set; } = new ReplayMetadata();
    }

    /// <summary>
    /// 玩家操作记录
    /// </summary>
    public class PlayerActionRecord
    {
        /// <summary>相对战斗开始的时间（秒）</summary>
        public float Time { get; set; }
        
        /// <summary>操作类型</summary>
        public PlayerActionType Type { get; set; }
        
        /// <summary>技能ID（如果是技能操作）</summary>
        public string SkillId { get; set; } = "";
        
        /// <summary>目标敌人ID（如果有）</summary>
        public string TargetId { get; set; } = "";
        
        /// <summary>玩家屏幕位置 X</summary>
        public float PlayerPosX { get; set; }
        
        /// <summary>玩家屏幕位置 Y</summary>
        public float PlayerPosY { get; set; }
        
        /// <summary>额外数据（JSON字符串，扩展用）</summary>
        public string ExtraData { get; set; } = "";
    }

    /// <summary>
    /// 玩家操作类型
    /// </summary>
    public enum PlayerActionType
    {
        SkillUse = 0,
        ComboCompleted = 1,
        Movement = 2,
        ItemUsed = 3,
        Dodge = 4
    }

    /// <summary>
    /// Combo 完成记录
    /// </summary>
    public class ComboRecord
    {
        /// <summary>相对战斗开始的时间（秒）</summary>
        public float Time { get; set; }
        
        /// <summary>Combo ID</summary>
        public string ComboId { get; set; } = "";
        
        /// <summary>Combo 名称</summary>
        public string ComboName { get; set; } = "";
        
        /// <summary>使用的技能序列</summary>
        public List<string> SkillSequence { get; set; } = new List<string>();
        
        /// <summary>造成的伤害</summary>
        public int Damage { get; set; }
        
        /// <summary>是否造成击杀</summary>
        public bool Killed { get; set; }
    }

    /// <summary>
    /// 回放元数据（不上报，仅本地使用）
    /// </summary>
    public class ReplayMetadata
    {
        /// <summary>回放文件创建时间</summary>
        public double CreatedAt { get; set; }
        
        /// <summary>游戏版本</summary>
        public string GameVersion { get; set; } = "1.0.0";
        
        /// <summary>玩家等级</summary>
        public int PlayerLevel { get; set; }
        
        /// <summary>战斗结果：victory/defeat</summary>
        public string Result { get; set; } = "victory";
        
        /// <summary>关卡/场景名称</summary>
        public string SceneName { get; set; } = "";
        
        /// <summary>参与的敌人数量</summary>
        public int EnemyCount { get; set; }
    }
}
