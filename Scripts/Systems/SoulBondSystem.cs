using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 灵魂绑定系统。管理装备与玩家之间的灵魂绑定关系和属性加成。
/// </summary>
public class SoulBondSystem : Node
{
    /// <summary>
    /// 获取系统单例实例。
    /// </summary>
    private static SoulBondSystem _instance;
    public static SoulBondSystem Instance => _instance;

    public Dictionary<string, SoulBondData> ActiveBonds { get; private set; }
    public List<SoulBondRecord> BondHistory { get; private set; }
    public int TotalBondsFormed { get; set; }
    public int HighestBondLevel { get; set; }
    public int TotalBondPointsEarned { get; set; }

    public event Action<SoulBondData> OnBondFormed;
    public event Action<SoulBondData, BondLevel> OnBondLevelUp;
    public event Action<SoulBondData, string> OnAbilityUnlocked;

    public SoulBondSystem()
    {
        _instance = this;
        ActiveBonds = new Dictionary<string, SoulBondData>();
        BondHistory = new List<SoulBondRecord>();
    }

    public override void _Ready()
    {
        LoadSoulBondData();
    }

    public string FormBond(string itemOrPetId, BondType bondType)
    {
        var config = SoulBondDatabase.Instance.GetBondConfig(itemOrPetId);
        if (config == null)
        {
            // Create default config for unknown items
            config = new BondConfig
            {
                ItemId = itemOrPetId,
                BondType = bondType,
                BasePointsRequired = 100,
                PointMultiplier = 1.0f
            };
        }

        if (ActiveBonds.ContainsKey(itemOrPetId))
        {
            return ActiveBonds[itemOrPetId].BondId;
        }

        var bondData = new SoulBondData
        {
            BondId = Guid.NewGuid().ToString(),
            ItemOrPetId = itemOrPetId,
            BondType = bondType,
            CurrentLevel = BondLevel.Awakening,
            TotalBondPoints = 0,
            BondPointsToNextLevel = CalculatePointsForNextLevel(BondLevel.Awakening, config),
            BondedAt = DateTime.Now,
            LastInteractionTime = DateTime.Now,
            InteractionCount = 0
        };

        // Unlock first ability
        var levelConfig = SoulBondDatabase.Instance.GetLevelConfig(BondLevel.Awakening);
        if (levelConfig != null && !string.IsNullOrEmpty(levelConfig.AbilityUnlock))
        {
            bondData.UnlockedAbilities.Add(levelConfig.AbilityUnlock);
            var milestone = new BondMilestone
            {
                Level = BondLevel.Awakening,
                AbilityId = levelConfig.AbilityUnlock,
                Description = SoulBondDatabase.Instance.GetAbility(levelConfig.AbilityUnlock)?.Name ?? "Unknown",
                Unlocked = true
            };
            bondData.Milestones.Add(milestone);
        }

        ActiveBonds[itemOrPetId] = bondData;
        TotalBondsFormed++;

        OnBondFormed?.Invoke(bondData);
        SaveSoulBondData();

        return bondData.BondId;
    }

    public void AddBondPoints(string itemOrPetId, int points)
    {
        if (!ActiveBonds.ContainsKey(itemOrPetId))
            return;

        var bond = ActiveBonds[itemOrPetId];
        var config = SoulBondDatabase.Instance.GetBondConfig(itemOrPetId);

        bond.TotalBondPoints += points;
        TotalBondPointsEarned += points;
        bond.LastInteractionTime = DateTime.Now;
        bond.InteractionCount++;

        // Check for level up
        CheckLevelUp(bond, config);

        SaveSoulBondData();
    }

    private void CheckLevelUp(SoulBondData bond, BondConfig config)
    {
        var nextLevel = GetNextLevel(bond.CurrentLevel);
        if (nextLevel == BondLevel.None)
            return;

        var pointsNeeded = CalculatePointsForNextLevel(bond.CurrentLevel, config);

        while (bond.TotalBondPoints >= pointsNeeded && nextLevel != BondLevel.None)
        {
            var previousLevel = bond.CurrentLevel;
            bond.CurrentLevel = nextLevel;
            bond.BondPointsToNextLevel = CalculatePointsForNextLevel(nextLevel, config);

            // Unlock ability for new level
            var levelConfig = SoulBondDatabase.Instance.GetLevelConfig(nextLevel);
            if (levelConfig != null && !string.IsNullOrEmpty(levelConfig.AbilityUnlock))
            {
                if (!bond.UnlockedAbilities.Contains(levelConfig.AbilityUnlock))
                {
                    bond.UnlockedAbilities.Add(levelConfig.AbilityUnlock);
                    var milestone = new BondMilestone
                    {
                        Level = nextLevel,
                        AbilityId = levelConfig.AbilityUnlock,
                        Description = SoulBondDatabase.Instance.GetAbility(levelConfig.AbilityUnlock)?.Name ?? "Unknown",
                        Unlocked = true
                    };
                    bond.Milestones.Add(milestone);
                    OnAbilityUnlocked?.Invoke(bond, levelConfig.AbilityUnlock);
                }
            }

            // Record history
            var record = new SoulBondRecord
            {
                BondId = bond.BondId,
                Timestamp = DateTime.Now,
                PreviousLevel = previousLevel,
                NewLevel = nextLevel,
                AbilityUnlocked = levelConfig?.AbilityUnlock,
                PointsSpent = bond.TotalBondPoints
            };
            BondHistory.Add(record);

            OnBondLevelUp?.Invoke(bond, nextLevel);

            if ((int)nextLevel > HighestBondLevel)
                HighestBondLevel = (int)nextLevel;

            nextLevel = GetNextLevel(nextLevel);
            if (nextLevel == BondLevel.None)
                break;
        }
    }

    private BondLevel GetNextLevel(BondLevel current)
    {
        return current switch
        {
            BondLevel.None => BondLevel.Awakening,
            BondLevel.Awakening => BondLevel.Manifestation,
            BondLevel.Manifestation => BondLevel.Convergence,
            BondLevel.Convergence => BondLevel.Transcendence,
            BondLevel.Transcendence => BondLevel.Nirvana,
            BondLevel.Nirvana => BondLevel.None,
            _ => BondLevel.None
        };
    }

    private int CalculatePointsForNextLevel(BondLevel current, BondConfig config)
    {
        var levelConfig = SoulBondDatabase.Instance.GetLevelConfig(current);
        if (levelConfig == null)
            return 100;

        return (int)(levelConfig.PointsRequired * config.PointMultiplier);
    }

    public Dictionary<string, float> GetBondStatBonuses(string itemOrPetId)
    {
        if (!ActiveBonds.ContainsKey(itemOrPetId))
            return new Dictionary<string, float>();

        var bond = ActiveBonds[itemOrPetId];
        var result = new Dictionary<string, float>();

        var levelsToCheck = new List<BondLevel>
        {
            BondLevel.Awakening,
            BondLevel.Manifestation,
            BondLevel.Convergence,
            BondLevel.Transcendence,
            BondLevel.Nirvana
        };

        foreach (var level in levelsToCheck)
        {
            if (bond.CurrentLevel >= level)
            {
                var levelConfig = SoulBondDatabase.Instance.GetLevelConfig(level);
                if (levelConfig?.StatBonus != null)
                {
                    foreach (var stat in levelConfig.StatBonus)
                    {
                        if (result.ContainsKey(stat.Key))
                            result[stat.Key] += stat.Value;
                        else
                            result[stat.Key] = stat.Value;
                    }
                }
            }
        }

        return result;
    }

    public bool HasAbility(string itemOrPetId, string abilityId)
    {
        if (!ActiveBonds.ContainsKey(itemOrPetId))
            return false;

        return ActiveBonds[itemOrPetId].UnlockedAbilities.Contains(abilityId);
    }

    public List<string> GetUnlockedAbilities(string itemOrPetId)
    {
        if (!ActiveBonds.ContainsKey(itemOrPetId))
            return new List<string>();

        return ActiveBonds[itemOrPetId].UnlockedAbilities;
    }

    public void InteractWithBond(string itemOrPetId, int points = 10)
    {
        AddBondPoints(itemOrPetId, points);
    }

    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            ["total_bonds"] = TotalBondsFormed,
            ["active_bonds"] = ActiveBonds.Count,
            ["highest_level"] = HighestBondLevel,
            ["total_points"] = TotalBondPointsEarned,
            ["average_level"] = ActiveBonds.Count > 0 ? (float)HighestBondLevel / ActiveBonds.Count : 0
        };
    }

    private void LoadSoulBondData()
    {
        // Load from save system
    }

    public void SaveSoulBondData()
    {
        // Save to save system
    }

    public void ResetAllBonds()
    {
        ActiveBonds.Clear();
        BondHistory.Clear();
        TotalBondsFormed = 0;
        HighestBondLevel = 0;
        TotalBondPointsEarned = 0;
        SaveSoulBondData();
    }
}
