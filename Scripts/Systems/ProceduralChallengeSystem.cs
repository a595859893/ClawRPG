using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 随机挑战系统 - 管理程序生成的挑战内容
/// 包含挑战生成、进度追踪、奖励发放等
/// </summary>
public partial class ProceduralChallengeSystem : BaseSystem
{
    public static ProceduralChallengeSystem Instance { get; private set; }

    private List<ProceduralChallengeData.ActiveChallenge> _activeChallenges = new List<ProceduralChallengeData.ActiveChallenge>();
    private ProceduralChallengeData.PlayerChallengeData _playerData = new ProceduralChallengeData.PlayerChallengeData();
    private int _maxActiveChallenges = 3;
    private float _challengeRefreshInterval = 300f; // 5 minutes
    private float _lastRefreshTime = 0;
    private Random _random = new Random();

    // Signals
    public static string ChallengeStarted = "ChallengeStarted";
    public static string ChallengeUpdated = "ChallengeUpdated";
    public static string ChallengeCompleted = "ChallengeCompleted";
    public static string ChallengeFailed = "ChallengeFailed";

    public override void _Ready()
    {
        Instance = this;
        ProceduralChallengeDatabase.Initialize();
    }

    public override void _Process(double delta)
    {
        // Refresh challenges periodically
        if (Time.GetTicksMsec() / 1000f - _lastRefreshTime > _challengeRefreshInterval)
        {
            RefreshChallenges();
            _lastRefreshTime = Time.GetTicksMsec() / 1000f;
        }

        // Update challenge timers
        UpdateChallengeTimers(delta);
    }

    public void RefreshChallenges()
    {
        // Remove expired challenges
        _activeChallenges.RemoveAll(c => c.Status == ProceduralChallengeData.ChallengeStatus.Failed);

        // Generate new challenges if needed
        while (_activeChallenges.Count < _maxActiveChallenges)
        {
            int playerLevel = 1;
            if (Player.Instance != null)
                playerLevel = Player.Instance.Level;

            var template = ProceduralChallengeDatabase.GenerateRandomChallenge(playerLevel);
            if (template != null)
            {
                var challenge = CreateChallengeFromTemplate(template, playerLevel);
                _activeChallenges.Add(challenge);
            }
        }
    }

    private ProceduralChallengeData.ActiveChallenge CreateChallengeFromTemplate(ProceduralChallengeData.ChallengeTemplate template, int playerLevel)
    {
        // Scale requirements and rewards based on player level
        float levelMultiplier = 1f + (playerLevel - 1) * 0.1f;
        
        var challenge = new ProceduralChallengeData.ActiveChallenge
        {
            InstanceId = Guid.NewGuid().ToString(),
            TemplateId = template.Id,
            Type = template.Type,
            Rarity = template.Rarity,
            CurrentProgress = 0,
            TargetProgress = (int)(template.BaseRequirement * levelMultiplier),
            TimeRemaining = template.BaseTimeLimit > 0 ? (int)(template.BaseTimeLimit * Math.Max(1f, levelMultiplier * 0.8f)) : 0,
            TimeLimit = template.BaseTimeLimit > 0 ? (int)(template.BaseTimeLimit * Math.Max(1f, levelMultiplier * 0.8f)) : 0,
            Status = ProceduralChallengeData.ChallengeStatus.Available,
            GoldReward = (int)(template.BaseGoldReward * levelMultiplier),
            ExpReward = (int)(template.BaseExpReward * levelMultiplier),
            BonusItems = new List<string>(),
            StartTime = DateTime.Now
        };

        return challenge;
    }

    public bool StartChallenge(string instanceId)
    {
        var challenge = GetChallenge(instanceId);
        if (challenge == null || challenge.Status != ProceduralChallengeData.ChallengeStatus.Available)
            return false;

        challenge.Status = ProceduralChallengeData.ChallengeStatus.InProgress;
        challenge.StartTime = DateTime.Now;
        
        if (challenge.TimeLimit > 0)
            challenge.TimeRemaining = challenge.TimeLimit;

        EmitSignal(ChallengeStarted, instanceId);
        return true;
    }

    public void UpdateProgress(string instanceId, int amount)
    {
        var challenge = GetChallenge(instanceId);
        if (challenge == null || challenge.Status != ProceduralChallengeData.ChallengeStatus.InProgress)
            return;

        challenge.CurrentProgress += amount;
        
        if (challenge.CurrentProgress >= challenge.TargetProgress)
        {
            CompleteChallenge(instanceId);
        }
        else
        {
            EmitSignal(ChallengeUpdated, instanceId, challenge.CurrentProgress, challenge.TargetProgress);
        }
    }

    public void UpdateProgressByType(ProceduralChallengeData.ChallengeType type, int amount)
    {
        foreach (var challenge in _activeChallenges)
        {
            if (challenge.Status == ProceduralChallengeData.ChallengeStatus.InProgress && challenge.Type == type)
            {
                UpdateProgress(challenge.InstanceId, amount);
            }
        }
    }

    public void UpdateTimeRemaining(string instanceId, int seconds)
    {
        var challenge = GetChallenge(instanceId);
        if (challenge == null || challenge.Status != ProceduralChallengeData.ChallengeStatus.InProgress)
            return;

        challenge.TimeRemaining -= seconds;
        
        if (challenge.TimeRemaining <= 0)
        {
            FailChallenge(instanceId);
        }
        else
        {
            EmitSignal(ChallengeUpdated, instanceId, challenge.CurrentProgress, challenge.TargetProgress);
        }
    }

    public void DecrementTimer(float delta)
    {
        foreach (var challenge in _activeChallenges)
        {
            if (challenge.Status == ProceduralChallengeData.ChallengeStatus.InProgress && challenge.TimeLimit > 0)
            {
                challenge.TimeRemaining -= (int)delta;
                
                if (challenge.TimeRemaining <= 0)
                {
                    FailChallenge(challenge.InstanceId);
                }
            }
        }
    }

    private void UpdateChallengeTimers(float delta)
    {
        DecrementTimer(delta);
    }

    private void CompleteChallenge(string instanceId)
    {
        var challenge = GetChallenge(instanceId);
        if (challenge == null) return;

        challenge.Status = ProceduralChallengeData.ChallengeStatus.Completed;

        // Award rewards
        if (Player.Instance != null)
        {
            Player.Instance.AddGold(challenge.GoldReward);
            Player.Instance.AddExp(challenge.ExpReward);
        }

        // Update player data
        _playerData.TotalChallengesCompleted++;
        _playerData.TotalGoldEarned += challenge.GoldReward;
        _playerData.TotalExpEarned += challenge.ExpReward;

        string typeKey = challenge.Type.ToString();
        if (!_playerData.CompletedByType.ContainsKey(typeKey))
            _playerData.CompletedByType[typeKey] = 0;
        _playerData.CompletedByType[typeKey]++;

        string rarityKey = challenge.Rarity.ToString();
        if (!_playerData.CompletedByRarity.ContainsKey(rarityKey))
            _playerData.CompletedByRarity[rarityKey] = 0;
        _playerData.CompletedByRarity[rarityKey]++;

        EmitSignal(ChallengeCompleted, instanceId, challenge.GoldReward, challenge.ExpReward);

        // Remove completed challenge and generate new one
        _activeChallenges.Remove(challenge);
        RefreshChallenges();
    }

    private void FailChallenge(string instanceId)
    {
        var challenge = GetChallenge(instanceId);
        if (challenge == null) return;

        challenge.Status = ProceduralChallengeData.ChallengeStatus.Failed;
        EmitSignal(ChallengeFailed, instanceId);

        // Remove failed challenge and generate new one
        _activeChallenges.Remove(challenge);
        RefreshChallenges();
    }

    public ProceduralChallengeData.ActiveChallenge GetChallenge(string instanceId)
    {
        foreach (var challenge in _activeChallenges)
        {
            if (challenge.InstanceId == instanceId)
                return challenge;
        }
        return null;
    }

    public ProceduralChallengeData.ActiveChallenge[] GetActiveChallenges()
    {
        return _activeChallenges.ToArray();
    }

    public ProceduralChallengeData.ActiveChallenge[] GetAvailableChallenges()
    {
        List<ProceduralChallengeData.ActiveChallenge> available = new List<ProceduralChallengeData.ActiveChallenge>();
        foreach (var challenge in _activeChallenges)
        {
            if (challenge.Status == ProceduralChallengeData.ChallengeStatus.Available ||
                challenge.Status == ProceduralChallengeData.ChallengeStatus.InProgress)
                available.Add(challenge);
        }
        return available.ToArray();
    }

    public ProceduralChallengeData.PlayerChallengeData GetPlayerData()
    {
        return _playerData;
    }

    public Dictionary<string, int> GetStatistics()
    {
        Dictionary<string, int> stats = new Dictionary<string, int>
        {
            { "total_completed", _playerData.TotalChallengesCompleted },
            { "total_gold", _playerData.TotalGoldEarned },
            { "total_exp", _playerData.TotalExpEarned }
        };
        return stats;
    }

    // Save/Load support
    public Dictionary<string, object> Save()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        
        // Save active challenges
        List<Dictionary<string, object>> challengeData = new List<Dictionary<string, object>>();
        foreach (var challenge in _activeChallenges)
        {
            challengeData.Add(new Dictionary<string, object>
            {
                { "instance_id", challenge.InstanceId },
                { "template_id", challenge.TemplateId },
                { "type", (int)challenge.Type },
                { "rarity", (int)challenge.Rarity },
                { "current_progress", challenge.CurrentProgress },
                { "target_progress", challenge.TargetProgress },
                { "time_remaining", challenge.TimeRemaining },
                { "time_limit", challenge.TimeLimit },
                { "status", (int)challenge.Status },
                { "gold_reward", challenge.GoldReward },
                { "exp_reward", challenge.ExpReward }
            });
        }
        data["active_challenges"] = challengeData;

        // Save player data
        data["player_data"] = new Dictionary<string, object>
        {
            { "total_completed", _playerData.TotalChallengesCompleted },
            { "total_gold", _playerData.TotalGoldEarned },
            { "total_exp", _playerData.TotalExpEarned }
        };

        data["last_refresh"] = _lastRefreshTime;

        return data;
    }

    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;

        // Load active challenges
        if (data.ContainsKey("active_challenges"))
        {
            var challengeList = (List<object>)data["active_challenges"];
            _activeChallenges.Clear();
            
            foreach (var c in challengeList)
            {
                var cdict = (Dictionary<string, object>)c;
                var challenge = new ProceduralChallengeData.ActiveChallenge
                {
                    InstanceId = (string)cdict["instance_id"],
                    TemplateId = (string)cdict["template_id"],
                    Type = (ProceduralChallengeData.ChallengeType)(int)cdict["type"],
                    Rarity = (ProceduralChallengeData.ChallengeRarity)(int)cdict["rarity"],
                    CurrentProgress = (int)cdict["current_progress"],
                    TargetProgress = (int)cdict["target_progress"],
                    TimeRemaining = (int)cdict["time_remaining"],
                    TimeLimit = (int)cdict["time_limit"],
                    Status = (ProceduralChallengeData.ChallengeStatus)(int)cdict["status"],
                    GoldReward = (int)cdict["gold_reward"],
                    ExpReward = (int)cdict["exp_reward"],
                    BonusItems = new List<string>()
                };
                _activeChallenges.Add(challenge);
            }
        }

        // Load player data
        if (data.ContainsKey("player_data"))
        {
            var pdata = (Dictionary<string, object>)data["player_data"];
            _playerData.TotalChallengesCompleted = (int)pdata["total_completed"];
            _playerData.TotalGoldEarned = (int)pdata["total_gold"];
            _playerData.TotalExpEarned = (int)pdata["total_exp"];
        }

        if (data.ContainsKey("last_refresh"))
        {
            _lastRefreshTime = (float)data["last_refresh"];
        }
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        // Export active challenges
        List<object> challengeData = new List<object>();
        foreach (var challenge in _activeChallenges)
        {
            challengeData.Add(new Dictionary<string, object>
            {
                { "instance_id", challenge.InstanceId },
                { "template_id", challenge.TemplateId },
                { "type", (int)challenge.Type },
                { "rarity", (int)challenge.Rarity },
                { "current_progress", challenge.CurrentProgress },
                { "target_progress", challenge.TargetProgress },
                { "time_remaining", challenge.TimeRemaining },
                { "time_limit", challenge.TimeLimit },
                { "status", (int)challenge.Status },
                { "gold_reward", challenge.GoldReward },
                { "exp_reward", challenge.ExpReward }
            });
        }
        data["active_challenges"] = challengeData;

        // Export player stats
        data["player_data"] = new Dictionary<string, object>
        {
            { "total_completed", _playerData.TotalChallengesCompleted },
            { "total_gold", _playerData.TotalGoldEarned },
            { "total_exp", _playerData.TotalExpEarned }
        };

        // Export settings
        data["max_active_challenges"] = _maxActiveChallenges;
        data["last_refresh"] = _lastRefreshTime;

        return new Dictionary(data);
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        // Import active challenges
        if (data.ContainsKey("active_challenges"))
        {
            var challengeList = (Godot.Collections.Array)data["active_challenges"];
            _activeChallenges.Clear();

            foreach (var c in challengeList)
            {
                var cdict = (Dictionary)c;
                var challenge = new ProceduralChallengeData.ActiveChallenge
                {
                    InstanceId = (string)cdict["instance_id"],
                    TemplateId = (string)cdict["template_id"],
                    Type = (ProceduralChallengeData.ChallengeType)(int)cdict["type"],
                    Rarity = (ProceduralChallengeData.ChallengeRarity)(int)cdict["rarity"],
                    CurrentProgress = (int)cdict["current_progress"],
                    TargetProgress = (int)cdict["target_progress"],
                    TimeRemaining = (int)cdict["time_remaining"],
                    TimeLimit = (int)cdict["time_limit"],
                    Status = (ProceduralChallengeData.ChallengeStatus)(int)cdict["status"],
                    GoldReward = (int)cdict["gold_reward"],
                    ExpReward = (int)cdict["exp_reward"],
                    BonusItems = new List<string>()
                };
                _activeChallenges.Add(challenge);
            }
        }

        // Import player stats
        if (data.ContainsKey("player_data"))
        {
            var pdata = (Dictionary)data["player_data"];
            _playerData.TotalChallengesCompleted = (int)pdata["total_completed"];
            _playerData.TotalGoldEarned = (int)pdata["total_gold"];
            _playerData.TotalExpEarned = (int)pdata["total_exp"];
        }

        if (data.ContainsKey("max_active_challenges"))
        {
            _maxActiveChallenges = (int)data["max_active_challenges"];
        }

        if (data.ContainsKey("last_refresh"))
        {
            _lastRefreshTime = (float)data["last_refresh"];
        }
    }
}
