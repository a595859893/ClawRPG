using Godot;
using System;

namespace ClawRPG.Scripts.Systems.PetScout
{
    /// <summary>
    /// Pet Scout System - 宠物侦察系统数据
    /// 宠物感知玩家视角盲区敌人，通过发光/警告音提示
    /// </summary>
    public partial class PetScoutData : Node
    {
        /// <summary>
        /// 侦察模式是否启用
        /// </summary>
        public bool ScoutEnabled { get; set; } = true;

        /// <summary>
        /// 盲区扇形角度（度）
        /// </summary>
        public float BlindSpotAngle { get; set; } = 120f;

        /// <summary>
        /// 感知半径
        /// </summary>
        public float PerceptionRadius { get; set; } = 300f;

        /// <summary>
        /// 提示冷却时间（秒）
        /// </summary>
        public float AlertCooldown { get; set; } = 3f;

        /// <summary>
        /// 是否播放警告音效
        /// </summary>
        public bool SoundEnabled { get; set; } = true;

        /// <summary>
        /// 最后一次警报时间（按敌人ID索引）
        /// </summary>
        public Dictionary<string, float> LastAlertTime { get; set; } = new Dictionary<string, float>();

        /// <summary>
        /// 当前检测到的盲区敌人列表
        /// </summary>
        public System.Collections.Generic.List<string> DetectedEnemyIds { get; set; } = new System.Collections.Generic.List<string>();
    }

    /// <summary>
    /// 侦察警报类型
    /// </summary>
    public enum ScoutAlertType
    {
        None,
        EnemyDetected,    // 发现敌人
        EnemyClose,       // 敌人接近
        EnemyBehind       // 背后有敌人
    }

    /// <summary>
    /// 侦察警报信息
    /// </summary>
    public class ScoutAlert
    {
        public string EnemyId { get; set; }
        public Vector2 EnemyPosition { get; set; }
        public ScoutAlertType AlertType { get; set; }
        public float Time { get; set; }
        public float Distance { get; set; }
        public float AngleFromBehind { get; set; }
    }
}
