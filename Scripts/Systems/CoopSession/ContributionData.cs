using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 贡献类型枚举
    /// </summary>
    public enum ContributionType
    {
        Damage,         // 造成伤害
        Healing,        // 治疗队友
        Tank,           // 承受伤害（吸引仇恨）
        Support,        // 辅助（Buff/DEBUFF）
        Kill,           // 击杀敌人
        Objective,      // 完成任务目标
        Survival        // 存活奖励
    }
    
    /// <summary>
    /// 玩家贡献数据
    /// </summary>
    public class PlayerContribution
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        
        // 各类贡献值
        public float TotalDamage { get; set; }        // 总伤害
        public float TotalHealing { get; set; }       // 总治疗量
        public float TotalTank { get; set; }           // 承受伤害
        public int KillCount { get; set; }            // 击杀数
        public int AssistCount { get; set; }          // 助攻数
        public int BuffsApplied { get; set; }         // 施加Buff数
        public float SurvivalTime { get; set; }        // 存活时间(秒)
        public int ObjectivesCompleted { get; set; }   // 完成目标数
        
        // 综合贡献分（用于分配收益）
        public float ContributionScore => CalculateScore();
        
        private float CalculateScore()
        {
            // 贡献分计算公式
            // 伤害权重: 1.0, 治疗权重: 1.5, 坦克权重: 1.2, 击杀权重: 10, 助攻权重: 5, Buff权重: 2, 存活权重: 0.5, 目标权重: 20
            return TotalDamage * 1.0f +
                   TotalHealing * 1.5f +
                   TotalTank * 1.2f +
                   KillCount * 10f +
                   AssistCount * 5f +
                   BuffsApplied * 2f +
                   SurvivalTime * 0.5f +
                   ObjectivesCompleted * 20f;
        }
    }

    /// <summary>
    /// 收益包定义
    /// </summary>
    public class RewardPackage
    {
        public int Experience { get; set; }       // 经验
        public int Gold { get; set; }             // 金币
        public List<string> Items { get; set; } = new List<string>();  // 物品列表
        public List<int> ItemIds { get; set; } = new List<int>();       // 物品ID列表
    }

    /// <summary>
    /// 收益分配结果
    /// </summary>
    public class DistributionResult
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        public float ContributionPercent { get; set; }  // 贡献占比
        public RewardPackage Rewards { get; set; } = new RewardPackage();
    }
}
