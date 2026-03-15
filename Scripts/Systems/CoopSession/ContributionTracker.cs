using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 合作收益分配系统
/// 追踪玩家贡献并按贡献度分配收益
/// </summary>
public partial class ContributionTracker : BaseSystem
{
    #region 贡献类型枚举
    
    /// <summary>贡献类型</summary>
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
    
    #endregion

    #region 数据结构

    /// <summary>玩家贡献数据</summary>
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

    /// <summary>收益包定义</summary>
    public class RewardPackage
    {
        public int Experience { get; set; }       // 经验
        public int Gold { get; set; }             // 金币
        public List<string> Items { get; set; } = new List<string>();  // 物品列表
        public List<int> ItemIds { get; set; } = new List<int>();       // 物品ID列表
    }

    /// <summary>收益分配结果</summary>
    public class DistributionResult
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        public float ContributionPercent { get; set; }  // 贡献占比
        public RewardPackage Rewards { get; set; } = new RewardPackage();
    }

    #endregion

    #region 信号定义
    
    /// <summary>贡献更新信号</summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="contribution">贡献数据</param>
    [Signal]
    public delegate void ContributionUpdatedEventHandler(int playerId, PlayerContribution contribution);
    
    /// <summary>收益分配完成信号</summary>
    /// <param name="results">分配结果</param>
    [Signal]
    public delegate void RewardsDistributedEventHandler(Array results);

    #endregion

    #region 私有成员
    
    private readonly Dictionary<int, PlayerContribution> _playerContributions = new();
    private string _currentSessionId = "";
    private readonly object _lock = new object();
    
    // 收益分配配置
    private float _baseExpShare = 100f;      // 基础经验分享
    private float _baseGoldShare = 50f;      // 基础金币分享
    private float _killBonusExp = 20f;       // 击杀经验奖励
    private float _assistBonusExp = 10f;     // 助攻经验奖励
    private float _survivalBonusExp = 5f;    // 每10秒存活经验
    
    #endregion

    #region 属性
    
    public string CurrentSessionId => _currentSessionId;
    
    #endregion

    #region 生命周期

    public override void _Ready()
    {
        base._Ready();
        GD.Print("[ContributionTracker] Initialized");
    }

    protected override void Initialize()
    {
        IsInitialized = true;
        GD.Print("[ContributionTracker] System initialized");
    }

    #endregion

    #region 会话管理

    /// <summary>
    /// 开始追踪贡献
    /// </summary>
    public void StartTracking(string sessionId)
    {
        lock (_lock)
        {
            _currentSessionId = sessionId;
            _playerContributions.Clear();
            GD.Print($"[ContributionTracker] Started tracking for session: {sessionId}");
        }
    }

    /// <summary>
    /// 停止追踪贡献
    /// </summary>
    public void StopTracking()
    {
        lock (_lock)
        {
            GD.Print($"[ContributionTracker] Stopped tracking for session: {_currentSessionId}");
            _currentSessionId = "";
        }
    }

    /// <summary>
    /// 添加玩家到贡献追踪
    /// </summary>
    public void AddPlayer(int playerId, string playerName)
    {
        lock (_lock)
        {
            if (!_playerContributions.ContainsKey(playerId))
            {
                _playerContributions[playerId] = new PlayerContribution
                {
                    PlayerId = playerId,
                    PlayerName = playerName,
                    SurvivalTime = 0
                };
                GD.Print($"[ContributionTracker] Player added: {playerName} (ID: {playerId})");
            }
        }
    }

    /// <summary>
    /// 移除玩家
    /// </summary>
    public void RemovePlayer(int playerId)
    {
        lock (_lock)
        {
            if (_playerContributions.Remove(playerId))
            {
                GD.Print($"[ContributionTracker] Player removed: {playerId}");
            }
        }
    }

    #endregion

    #region 贡献记录

    /// <summary>
    /// 记录伤害贡献
    /// </summary>
    public void RecordDamage(int playerId, float damage, int targetId = -1)
    {
        lock (_lock)
        {
            if (_playerContributions.TryGetValue(playerId, out var contribution))
            {
                contribution.TotalDamage += damage;
                EmitSignal(SignalName.ContributionUpdated, playerId, contribution);
            }
        }
    }

    /// <summary>
    /// 记录治疗贡献
    /// </summary>
    public void RecordHealing(int playerId, float healingAmount)
    {
        lock (_lock)
        {
            if (_playerContributions.TryGetValue(playerId, out var contribution))
            {
                contribution.TotalHealing += healingAmount;
                EmitSignal(SignalName.ContributionUpdated, playerId, contribution);
            }
        }
    }

    /// <summary>
    /// 记录坦克贡献（承受伤害）
    /// </summary>
    public void RecordTank(int playerId, float damageTaken)
    {
        lock (_lock)
        {
            if (_playerContributions.TryGetValue(playerId, out var contribution))
            {
                contribution.TotalTank += damageTaken;
                EmitSignal(SignalName.ContributionUpdated, playerId, contribution);
            }
        }
    }

    /// <summary>
    /// 记录击杀
    /// </summary>
    public void RecordKill(int playerId)
    {
        lock (_lock)
        {
            if (_playerContributions.TryGetValue(playerId, out var contribution))
            {
                contribution.KillCount++;
                EmitSignal(SignalName.ContributionUpdated, playerId, contribution);
            }
        }
    }

    /// <summary>
    /// 记录助攻
    /// </summary>
    public void RecordAssist(int playerId)
    {
        lock (_lock)
        {
            if (_playerContributions.TryGetValue(playerId, out var contribution))
            {
                contribution.AssistCount++;
                EmitSignal(SignalName.ContributionUpdated, playerId, contribution);
            }
        }
    }

    /// <summary>
    /// 记录Buff施加
    /// </summary>
    public void RecordBuffApplied(int playerId)
    {
        lock (_lock)
        {
            if (_playerContributions.TryGetValue(playerId, out var contribution))
            {
                contribution.BuffsApplied++;
                EmitSignal(SignalName.ContributionUpdated, playerId, contribution);
            }
        }
    }

    /// <summary>
    /// 更新存活时间
    /// </summary>
    public void UpdateSurvivalTime(int playerId, float deltaTime)
    {
        lock (_lock)
        {
            if (_playerContributions.TryGetValue(playerId, out var contribution))
            {
                contribution.SurvivalTime += deltaTime;
            }
        }
    }

    /// <summary>
    /// 记录完成目标
    /// </summary>
    public void RecordObjective(int playerId)
    {
        lock (_lock)
        {
            if (_playerContributions.TryGetValue(playerId, out var contribution))
            {
                contribution.ObjectivesCompleted++;
                EmitSignal(SignalName.ContributionUpdated, playerId, contribution);
            }
        }
    }

    /// <summary>
    /// 记录综合贡献（从外部系统调用）
    /// </summary>
    public void RecordContribution(int playerId, ContributionType type, float value = 0, int count = 0)
    {
        switch (type)
        {
            case ContributionType.Damage:
                RecordDamage(playerId, value);
                break;
            case ContributionType.Healing:
                RecordHealing(playerId, value);
                break;
            case ContributionType.Tank:
                RecordTank(playerId, value);
                break;
            case ContributionType.Kill:
                for (int i = 0; i < count; i++) RecordKill(playerId);
                break;
            case ContributionType.Support:
                for (int i = 0; i < count; i++) RecordBuffApplied(playerId);
                break;
            case ContributionType.Objective:
                for (int i = 0; i < count; i++) RecordObjective(playerId);
                break;
        }
    }

    #endregion

    #region 贡献查询

    /// <summary>
    /// 获取玩家贡献数据
    /// </summary>
    public PlayerContribution? GetPlayerContribution(int playerId)
    {
        lock (_lock)
        {
            return _playerContributions.TryGetValue(playerId, out var contribution) ? contribution : null;
        }
    }

    /// <summary>
    /// 获取所有玩家贡献
    /// </summary>
    public List<PlayerContribution> GetAllContributions()
    {
        lock (_lock)
        {
            return _playerContributions.Values.ToList();
        }
    }

    /// <summary>
    /// 获取总贡献分
    /// </summary>
    public float GetTotalContributionScore()
    {
        lock (_lock)
        {
            return _playerContributions.Values.Sum(c => c.ContributionScore);
        }
    }

    /// <summary>
    /// 获取贡献排名
    /// </summary>
    public List<PlayerContribution> GetContributionRanking()
    {
        lock (_lock)
        {
            return _playerContributions.Values
                .OrderByDescending(c => c.ContributionScore)
                .ToList();
        }
    }

    #endregion

    #region 收益分配

    /// <summary>
    /// 分配收益（按贡献比例）
    /// </summary>
    /// <param name="baseExp">基础经验</param>
    /// <param name="baseGold">基础金币</param>
    /// <param name="bonusItems">额外物品列表</param>
    /// <returns>分配结果列表</returns>
    public List<DistributionResult> DistributeRewards(int baseExp, int baseGold, List<int>? bonusItems = null)
    {
        lock (_lock)
        {
            var results = new List<DistributionResult>();
            
            if (_playerContributions.Count == 0)
            {
                GD.PrintWarn("[ContributionTracker] No players to distribute rewards");
                return results;
            }

            float totalScore = GetTotalContributionScore();
            
            // 避免除零
            if (totalScore <= 0) totalScore = 1f;

            // 按贡献比例分配
            foreach (var contribution in _playerContributions.Values)
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

            GD.Print($"[ContributionTracker] Rewards distributed: {results.Count} players");
            
            // 发出信号
            EmitSignal(SignalName.RewardsDistributed, results.ToArray());
            
            return results;
        }
    }

    /// <summary>
    /// 计算额外经验奖励
    /// </summary>
    private float GetBonusExp(PlayerContribution contribution)
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
    private void DistributeBonusItems(List<DistributionResult> results, List<int> bonusItems)
    {
        // 按贡献排序
        var sortedResults = results.OrderByDescending(r => r.ContributionPercent).ToList();
        
        // 轮流分配
        for (int i = 0; i < bonusItems.Count; i++)
        {
            int playerIndex = i % sortedResults.Count;
            sortedResults[playerIndex].Rewards.ItemIds.Add(bonusItems[i]);
        }
    }

    /// <summary>
    /// 分配经验（简单模式：平均分配 + 贡献加成）
    /// </summary>
    public Dictionary<int, int> DistributeExperience(int totalExp)
    {
        lock (_lock)
        {
            var distribution = new Dictionary<int, int>();
            
            if (_playerContributions.Count == 0) return distribution;

            float totalScore = GetTotalContributionScore();
            if (totalScore <= 0) totalScore = 1f;

            foreach (var contribution in _playerContributions.Values)
            {
                float percent = contribution.ContributionScore / totalScore;
                int exp = (int)(totalExp * percent);
                distribution[contribution.PlayerId] = exp;
            }

            return distribution;
        }
    }

    /// <summary>
    /// 分配金币（简单模式：平均分配 + 贡献加成）
    /// </summary>
    public Dictionary<int, int> DistributeGold(int totalGold)
    {
        lock (_lock)
        {
            var distribution = new Dictionary<int, int>();
            
            if (_playerContributions.Count == 0) return distribution;

            float totalScore = GetTotalContributionScore();
            if (totalScore <= 0) totalScore = 1f;

            foreach (var contribution in _playerContributions.Values)
            {
                float percent = contribution.ContributionScore / totalScore;
                int gold = (int)(totalGold * percent);
                distribution[contribution.PlayerId] = gold;
            }

            return distribution;
        }
    }

    #endregion

    #region 配置

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
        
        GD.Print($"[ContributionTracker] Distribution params updated");
    }

    #endregion

    #region 存档支持

    public override Dictionary ExportSaveData()
    {
        lock (_lock)
        {
            var data = new Dictionary();
            data["session_id"] = _currentSessionId;
            
            var contributions = new Array();
            foreach (var kvp in _playerContributions)
            {
                var c = kvp.Value;
                contributions.Add(new Dictionary
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
            data["contributions"] = contributions;
            
            return data;
        }
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;

        lock (_lock)
        {
            if (data.ContainsKey("session_id"))
            {
                _currentSessionId = data["session_id"]?.ToString() ?? "";
            }

            if (data.ContainsKey("contributions") && data["contributions"] is Array contributionsList)
            {
                _playerContributions.Clear();
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
                    _playerContributions[contribution.PlayerId] = contribution;
                }
            }
        }
    }

    #endregion
}
