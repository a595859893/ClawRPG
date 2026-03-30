using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 团本Boss系统 - 管理团队副本玩法
/// 包含团队创建、Boss战斗、阶段管理、奖励分配等
/// </summary>
public partial class RaidBossSystem : BaseSystem
{
    private static RaidBossSystem instance;
    public static RaidBossSystem Instance => instance ?? (instance = new RaidBossSystem());
    
    private RaidBossData data;
    private int nextRaidId = 1;
    
    public override void _Ready()
    {
        base._Ready();
    }
    
    protected override void Initialize()
    {
        instance = this;
        data = new RaidBossData();
        GD.Print("[RaidBossSystem] Initialized - Raid Boss System ready");
    }
    
    // Create a new raid instance
    public int CreateRaid(RaidBossType raidType, string leaderId, string leaderName)
    {
        if (!RaidBossDatabase.RaidConfigs.ContainsKey(raidType))
        {
            GD.PrintErr($"[RaidBossSystem] Invalid raid type: {raidType}");
            return -1;
        }
        
        var config = RaidBossDatabase.RaidConfigs[raidType];
        
        data.CurrentRaidId = nextRaidId++;
        data.CurrentRaidType = raidType;
        data.CurrentRaidName = config.Name;
        data.LeaderId = leaderId;
        data.CurrentState = RaidState.Recruiting;
        data.CurrentPhase = 1;
        data.MaxPhases = config.MaxPhases;
        data.BossMaxHealth = config.BossHealth;
        data.BossHealth = config.BossHealth;
        data.MaxEnrageTime = config.EnrageTime;
        data.EnrageTimer = 0f;
        data.Participants.Clear();
        data.JoinedPlayerIds.Clear();
        data.RewardsClaimed = false;
        data.LootItems.Clear();
        
        // Add leader as first participant
        AddParticipant(leaderId, leaderName, RaidRole.Damage);
        
        GD.Print($"[RaidBossSystem] Raid created: {config.Name} (ID: {data.CurrentRaidId}) by {leaderName}");
        return data.CurrentRaidId;
    }
    
    // Join a raid
    public bool JoinRaid(string playerId, string playerName, RaidRole role)
    {
        if (data.CurrentState != RaidState.Recruiting)
        {
            GD.PrintErr($"[RaidBossSystem] Cannot join raid - state: {data.CurrentState}");
            return false;
        }
        
        var config = RaidBossDatabase.RaidConfigs[data.CurrentRaidType];
        if (data.Participants.Count >= config.MaxPlayers)
        {
            GD.PrintErr($"[RaidBossSystem] Raid is full");
            return false;
        }
        
        if (data.JoinedPlayerIds.Contains(playerId))
        {
            GD.PrintErr($"[RaidBossSystem] Player already in raid");
            return false;
        }
        
        AddParticipant(playerId, playerName, role);
        GD.Print($"[RaidBossSystem] {playerName} joined raid as {role}");
        return true;
    }
    
    // Leave a raid
    public bool LeaveRaid(string playerId)
    {
        var participant = data.Participants.Find(p => p.PlayerId == playerId);
        if (participant == null)
        {
            return false;
        }
        
        data.Participants.Remove(participant);
        data.JoinedPlayerIds.Remove(playerId);
        
        // If leader leaves, disband raid
        if (playerId == data.LeaderId)
        {
            data.CurrentState = RaidState.Abandoned;
            GD.Print($"[RaidBossSystem] Leader left - raid disbanded");
        }
        
        GD.Print($"[RaidBossSystem] {participant.PlayerName} left the raid");
        return true;
    }
    
    // Start the raid
    public bool StartRaid()
    {
        if (data.CurrentState != RaidState.Recruiting)
        {
            GD.PrintErr($"[RaidBossSystem] Cannot start raid - state: {data.CurrentState}");
            return false;
        }
        
        var config = RaidBossDatabase.RaidConfigs[data.CurrentRaidType];
        if (data.Participants.Count < config.MinPlayers)
        {
            GD.PrintErr($"[RaidBossSystem] Not enough players (min: {config.MinPlayers})");
            return false;
        }
        
        data.CurrentState = RaidState.InProgress;
        data.BossHealth = data.BossMaxHealth;
        data.EnrageTimer = 0f;
        data.CurrentPhase = 1;
        
        GD.Print($"[RaidBossSystem] Raid started: {data.CurrentRaidName} with {data.Participants.Count} players");
        return true;
    }
    
    // Deal damage to boss
    public void DealDamage(string playerId, int damage)
    {
        if (data.CurrentState != RaidState.InProgress)
            return;
        
        var participant = data.Participants.Find(p => p.PlayerId == playerId);
        if (participant != null)
        {
            participant.DamageDealt += damage;
            data.TotalDamageDealt += damage;
        }
        
        data.BossHealth -= damage;
        
        // Check for phase transition
        CheckPhaseTransition();
        
        // Check for victory
        if (data.BossHealth <= 0)
        {
            Victory();
        }
    }
    
    // Heal contribution
    public void RecordHealing(string playerId, int healing)
    {
        var participant = data.Participants.Find(p => p.PlayerId == playerId);
        if (participant != null)
        {
            participant.HealingDone += healing;
            data.TotalHealingDone += healing;
        }
    }
    
    // Record player death
    public void RecordDeath(string playerId)
    {
        var participant = data.Participants.Find(p => p.PlayerId == playerId);
        if (participant != null)
        {
            participant.Deaths++;
            participant.IsAlive = false;
        }
    }
    
    // Check phase transition
    private void CheckPhaseTransition()
    {
        float healthPercent = data.BossHealth / data.BossMaxHealth;
        
        int targetPhase = data.MaxPhases;
        foreach (var phase in RaidBossDatabase.PhaseHealthThresholds)
        {
            if (healthPercent <= phase.Value && phase.Key <= data.MaxPhases)
            {
                targetPhase = phase.Key;
            }
        }
        
        if (targetPhase > data.CurrentPhase)
        {
            data.CurrentPhase = targetPhase;
            data.CurrentState = RaidState.PhaseComplete;
            GD.Print($"[RaidBossSystem] Phase {data.CurrentPhase} reached!");
            
            // Resume after short delay
            data.CurrentState = RaidState.InProgress;
        }
    }
    
    // Update enrage timer (call this in _Process)
    public void Update(float delta)
    {
        if (data.CurrentState == RaidState.InProgress)
        {
            data.EnrageTimer += delta;
            
            // Check for enrage (failure)
            if (data.EnrageTimer >= data.MaxEnrageTime)
            {
                Fail("Enrage timer expired!");
            }
        }
    }
    
    // Victory handler
    private void Victory()
    {
        data.CurrentState = RaidState.Victory;
        data.TotalRaidsCompleted++;
        data.TotalBossKills++;
        
        var config = RaidBossDatabase.RaidConfigs[data.CurrentRaidType];
        
        // Calculate rewards
        int playerCount = data.Participants.Count;
        data.TotalGoldReward = config.Rewards.Gold;
        data.TotalExpReward = config.Rewards.Exp;
        
        // Generate loot
        GenerateLoot();
        
        // Calculate contribution percentages
        CalculateContributions();
        
        // Add to history
        AddToHistory(true);
        
        GD.Print($"[RaidBossSystem] VICTORY! {data.CurrentRaidName} cleared!");
    }
    
    // Failure handler
    private void Fail(string reason)
    {
        data.CurrentState = RaidState.Failed;
        data.TotalRaidsFailed++;
        
        // Calculate contribution percentages
        CalculateContributions();
        
        // Add to history
        AddToHistory(false);
        
        GD.Print($"[RaidBossSystem] FAILED: {reason}");
    }
    
    // Generate loot based on drop rates
    private void GenerateLoot()
    {
        if (!RaidBossDatabase.LootTables.ContainsKey(data.CurrentRaidType))
            return;
        
        var lootTable = RaidBossDatabase.LootTables[data.CurrentRaidType];
        var random = new Random();
        
        foreach (var entry in lootTable)
        {
            if (random.NextDouble() < entry.DropRate)
            {
                data.LootItems.Add($"{entry.ItemId} ({entry.Rarity})");
            }
        }
        
        GD.Print($"[RaidBossSystem] Generated {data.LootItems.Count} loot items");
    }
    
    // Calculate contribution percentages
    private void CalculateContributions()
    {
        if (data.TotalDamageDealt <= 0)
            return;
        
        foreach (var participant in data.Participants)
        {
            participant.ContributionPercent = (float)participant.DamageDealt / data.TotalDamageDealt * 100f;
        }
    }
    
    // Add participant
    private void AddParticipant(string playerId, string playerName, RaidRole role)
    {
        data.Participants.Add(new RaidParticipant
        {
            PlayerId = playerId,
            PlayerName = playerName,
            Role = role,
            DamageDealt = 0,
            HealingDone = 0,
            Deaths = 0,
            IsAlive = true,
            ContributionPercent = 0
        });
        data.JoinedPlayerIds.Add(playerId);
    }
    
    // Add to history
    private void AddToHistory(bool victory)
    {
        int clearTime = (int)data.EnrageTimer;
        
        data.History.Add(new RaidHistoryRecord
        {
            RaidName = data.CurrentRaidName,
            Timestamp = DateTime.Now,
            Victory = victory,
            ClearTime = clearTime,
            DamageDealt = data.TotalDamageDealt,
            GoldReward = data.TotalGoldReward,
            ExpReward = data.TotalExpReward
        });
        
        // Update best clear time
        if (victory && (data.BestClearTime == 0 || clearTime < data.BestClearTime))
        {
            data.BestClearTime = clearTime;
        }
        
        // Update totals
        data.TotalRaidsJoined++;
    }
    
    // Get current raid status
    public Dictionary<string, object> GetRaidStatus()
    {
        return new Dictionary<string, object>
        {
            { "raidId", data.CurrentRaidId },
            { "raidName", data.CurrentRaidName },
            { "raidType", data.CurrentRaidType.ToString() },
            { "state", data.CurrentState.ToString() },
            { "phase", data.CurrentPhase },
            { "maxPhases", data.MaxPhases },
            { "bossHealth", data.BossHealth },
            { "bossMaxHealth", data.BossMaxHealth },
            { "healthPercent", data.BossMaxHealth > 0 ? data.BossHealth / data.BossMaxHealth * 100 : 0 },
            { "enrageTimer", data.EnrageTimer },
            { "maxEnrageTime", data.MaxEnrageTime },
            { "playerCount", data.Participants.Count }
        };
    }
    
    // Get statistics
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "totalRaidsJoined", data.TotalRaidsJoined },
            { "totalRaidsCompleted", data.TotalRaidsCompleted },
            { "totalRaidsFailed", data.TotalRaidsFailed },
            { "totalBossKills", data.TotalBossKills },
            { "totalDamageDealt", data.TotalDamageDealt },
            { "totalHealingDone", data.TotalHealingDone },
            { "bestClearTime", data.BestClearTime },
            { "winRate", data.TotalRaidsJoined > 0 ? (float)data.TotalRaidsCompleted / data.TotalRaidsJoined * 100 : 0 }
        };
    }
    
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "totalRaidsJoined", data.TotalRaidsJoined },
            { "totalRaidsCompleted", data.TotalRaidsCompleted },
            { "totalRaidsFailed", data.TotalRaidsFailed },
            { "totalBossKills", data.TotalBossKills },
            { "totalDamageDealt", data.TotalDamageDealt },
            { "totalHealingDone", data.TotalHealingDone },
            { "bestClearTime", data.BestClearTime },
            { "history", data.History.Count }
        };
    }
    
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("totalRaidsJoined")) this.data.TotalRaidsJoined = Convert.ToInt32(data["totalRaidsJoined"]);
        if (data.Contains("totalRaidsCompleted")) this.data.TotalRaidsCompleted = Convert.ToInt32(data["totalRaidsCompleted"]);
        if (data.Contains("totalRaidsFailed")) this.data.TotalRaidsFailed = Convert.ToInt32(data["totalRaidsFailed"]);
        if (data.Contains("totalBossKills")) this.data.TotalBossKills = Convert.ToInt32(data["totalBossKills"]);
        if (data.Contains("totalDamageDealt")) this.data.TotalDamageDealt = Convert.ToInt32(data["totalDamageDealt"]);
        if (data.Contains("totalHealingDone")) this.data.TotalHealingDone = Convert.ToInt32(data["totalHealingDone"]);
        if (data.Contains("bestClearTime")) this.data.BestClearTime = Convert.ToInt32(data["bestClearTime"]);
        
        GD.Print("[RaidBossSystem] Data loaded");
    }
}
