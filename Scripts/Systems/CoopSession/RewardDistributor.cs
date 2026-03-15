using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 收益分配器 - 负责按贡献比例分配收益
    /// </summary>
    public class RewardDistributor
    {
        // 收益分配配置
        private float _baseExpShare = 100f;
        private float _baseGoldShare = 50f;
        private float _killBonusExp = 20f;
        private float _assistBonusExp = 10f;
        private float _survivalBonusExp = 5f;
        
        /// <summary>
        /// 设置分配参数
        /// </summary>
        public void SetDistributionParams(float baseExpShare, float baseGoldShare, float killBonus, float assistBonus, float survivalBonus)
        {
            _baseExpShare = baseExpShare;
            _baseGoldShare = baseGoldShare;
            _killBonusExp = killBonus;
            _assistBonusExp = assistBonus;
            _survivalBonusExp = survivalBonus;
        }
        
        /// <summary>
        /// 计算总贡献分
        /// </summary>
        public float GetTotalContributionScore(Dictionary<int, PlayerContribution> contributions)
        {
            return contributions.Values.Sum(c => c.ContributionScore);
        }
        
        /// <summary>
        /// 分配收益（按贡献比例）
        /// </summary>
        public List<DistributionResult> DistributeRewards(
            Dictionary<int, PlayerContribution> playerContributions,
            int baseExp, 
            int baseGold, 
            List<int>? bonusItems = null)
        {
            var results = new List<DistributionResult>();
            
            if (playerContributions.Count == 0)
            {
                GD.PrintWarn("[RewardDistributor] No players to distribute rewards");
                return results;
            }

            float totalScore = GetTotalContributionScore(playerContributions);
            
            // 避免除零
            if (totalScore <= 0) totalScore = 1f;

            // 按贡献比例分配
            foreach (var contribution in playerContributions.Values)
            {
                float percent = contribution.ContributionScore / totalScore;
                
                var result = new DistributionResult
                {
                    PlayerId = contribution.PlayerId,
                    PlayerName = contribution.PlayerName,
                    ContributionPercent = percent,
                    Rewards = new RewardPackage
                    {
                        Experience = (int)(baseExp * percent),
                        Gold = (int)(baseGold * percent),
                        Items = new List<string>(),
                        ItemIds = new List<int>()
                    }
                };

                // 添加额外经验奖励（击杀、助攻、存活）
                result.Rewards.Experience += (int)GetBonusExp(contribution);
                
                results.Add(result);
            }

            // 处理额外物品（按贡献排名分配）
            if (bonusItems != null && bonusItems.Count > 0)
            {
                DistributeBonusItems(results, bonusItems);
            }

            GD.Print($"[RewardDistributor] Rewards distributed: {results.Count} players");
            
            return results;
        }
        
        /// <summary>
        /// 计算额外经验奖励
        /// </summary>
        public float GetBonusExp(PlayerContribution contribution)
        {
            float bonus = 0;
            
            // 击杀奖励
            bonus += contribution.KillCount * _killBonusExp;
            
            // 助攻奖励
            bonus += contribution.AssistCount * _assistBonusExp;
            
            // 存活奖励（每10秒）
            bonus += (contribution.SurvivalTime / 10f) * _survivalBonusExp;
            
            return bonus;
        }
        
        /// <summary>
        /// 分配额外物品（按排名）
        /// </summary>
        public void DistributeBonusItems(List<DistributionResult> results, List<int> bonusItems)
        {
            // 按贡献排名排序
            var sortedResults = results.OrderByDescending(r => r.ContributionPercent).ToList();
            
            // 按排名分配物品
            for (int i = 0; i < sortedResults.Count && i < bonusItems.Count; i++)
            {
                int itemId = bonusItems[i];
                sortedResults[i].Rewards.ItemIds.Add(itemId);
            }
            
            GD.Print($"[RewardDistributor] Bonus items distributed: {bonusItems.Count}");
        }
        
        /// <summary>
        /// 导出存档数据
        /// </summary>
        public Dictionary ExportSaveData(Dictionary<int, PlayerContribution> contributions, string sessionId)
        {
            var data = new Dictionary();
            data["session_id"] = sessionId;
            
            var contributionsList = new Godot.Collections.Array();
            foreach (var kvp in contributions)
            {
                var c = kvp.Value;
                contributionsList.Add(new Dictionary
                {
                    { "player_id", c.PlayerId },
                    { "player_name", c.PlayerName },
                    { "total_damage", c.TotalDamage },
                    { "total_healing", c.TotalHealing },
                    { "total_tank", c.TotalTank },
                    { "kill_count", c.KillCount },
                    { "assist_count", c.AssistCount },
                    { "buffs_applied", c.BuffsApplied },
                    { "survival_time", c.SurvivalTime },
                    { "objectives_completed", c.ObjectivesCompleted }
                });
            }
            data["contributions"] = contributionsList;
            
            return data;
        }
        
        /// <summary>
        /// 导入存档数据
        /// </summary>
        public void ImportSaveData(Dictionary data, out string sessionId, out Dictionary<int, PlayerContribution> contributions)
        {
            sessionId = "";
            contributions = new Dictionary<int, PlayerContribution>();
            
            if (data == null) return;

            if (data.ContainsKey("session_id"))
            {
                sessionId = data["session_id"]?.ToString() ?? "";
            }

            if (data.ContainsKey("contributions") && data["contributions"] is Godot.Collections.Array contributionsList)
            {
                foreach (Dictionary cData in contributionsList)
                {
                    var contribution = new PlayerContribution
                    {
                        PlayerId = Convert.ToInt32(cData["player_id"]),
                        PlayerName = cData["player_name"]?.ToString() ?? "",
                        TotalDamage = Convert.ToSingle(cData["total_damage"]),
                        TotalHealing = Convert.ToSingle(cData["total_healing"]),
                        TotalTank = Convert.ToSingle(cData["total_tank"]),
                        KillCount = Convert.ToInt32(cData["kill_count"]),
                        AssistCount = Convert.ToInt32(cData["assist_count"]),
                        BuffsApplied = Convert.ToInt32(cData["buffs_applied"]),
                        SurvivalTime = Convert.ToSingle(cData["survival_time"]),
                        ObjectivesCompleted = Convert.ToInt32(cData["objectives_completed"])
                    };
                    contributions[contribution.PlayerId] = contribution;
                }
            }
        }
    }
}
