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

    // 使用共享的数据结构 (from ContributionData.cs)
    // PlayerContribution, RewardPackage, DistributionResult now in ContributionData.cs

    #region 信号定义
    
    /// <summary>贡献更新信号</summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="contribution">贡献数据</param>
public delegate void ContributionUpdatedEventHandler(int playerId, PlayerContribution contribution);
    
    /// <summary>收益分配完成信号</summary>
    /// <param name="results">分配结果</param>
public delegate void RewardsDistributedEventHandler(Array results);

    #endregion

    #region 私有成员
    
    private readonly Dictionary<int, PlayerContribution> _playerContributions = new();
    private string _currentSessionId = "";
    private readonly object _lock = new object();
    
    // 收益分配器
    private readonly RewardDistributor _rewardDistributor = new();
    
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
    public void RecordObjectiveCompleted(int playerId)
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
    /// 批量记录贡献（用于离线结算）
    /// </summary>
    public void BatchRecordContributions(int playerId, float damage, float healing, float tank, int kills, int assists, int buffs, float survivalTime, int objectives)
    {
        lock (_lock)
        {
            if (_playerContributions.TryGetValue(playerId, out var contribution))
            {
                contribution.TotalDamage += damage;
                contribution.TotalHealing += healing;
                contribution.TotalTank += tank;
                contribution.KillCount += kills;
                contribution.AssistCount += assists;
                contribution.BuffsApplied += buffs;
                contribution.SurvivalTime += survivalTime;
                contribution.ObjectivesCompleted += objectives;
                
                EmitSignal(SignalName.ContributionUpdated, playerId, contribution);
            }
        }
    }

    #endregion

    #region 查询

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
    /// 获取所有贡献数据
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
            var results = _rewardDistributor.DistributeRewards(_playerContributions, baseExp, baseGold, bonusItems);
            
            GD.Print($"[ContributionTracker] Rewards distributed: {results.Count} players");
            
            // 发出信号
            EmitSignal(SignalName.RewardsDistributed, results.ToArray());
            
            return results;
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
        _rewardDistributor.SetDistributionParams(baseExpShare, baseGoldShare, killBonus, assistBonus, survivalBonus);
        GD.Print($"[ContributionTracker] Distribution params updated");
    }

    #endregion

    #region 存档支持

    public override Dictionary<string, object> ExportSaveData()
    {
        lock (_lock)
        {
            return _rewardDistributor.ExportSaveData(_playerContributions, _currentSessionId);
        }
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        lock (_lock)
        {
            _rewardDistributor.ImportSaveData(data, out _currentSessionId, out _playerContributions);
        }
    }

    #endregion
}
