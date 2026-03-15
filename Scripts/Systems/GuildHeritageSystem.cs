using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// 公会传承系统 - 管理公会遗产和永久加成
/// </summary>
public partial class GuildHeritageSystem : BaseSystem
{
    public static GuildHeritageSystem Instance { get; private set; }

    public Dictionary<string, GuildHeritage> GuildHeritages { get; private set; }
    public Dictionary<string, PlayerHeritageData> PlayerData { get; private set; }
    public GuildHeritageStatistics Statistics { get; private set; }
    public SignalContainer<GuildHeritageSignal> HeritageSignal { get; private set; }

    public GuildHeritageSystem()
    {
        GuildHeritages = new Dictionary<string, GuildHeritage>();
        PlayerData = new Dictionary<string, PlayerHeritageData>();
        Statistics = new GuildHeritageStatistics
        {
            HeritagesByType = new Dictionary<HeritageType, int>()
        };
        HeritageSignal = new SignalContainer<GuildHeritageSignal>();
    }

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        Initialize();
    }

    protected override void Initialize()
    {
        GD.Print("GuildHeritageSystem initialized");
        IsInitialized = true;
    }

    public GuildHeritage CreateGuildHeritage(string guildId, string guildName)
    {
        if (GuildHeritages.ContainsKey(guildId))
        {
            return GuildHeritages[guildId];
        }

        var heritage = new GuildHeritage
        {
            GuildId = guildId,
            GuildName = guildName,
            UnlockedHeritages = new Dictionary<string, int>(),
            TotalHeritagePoints = 0,
            ContributionHistory = new Dictionary<string, int>(),
            LastUpdated = DateTime.Now,
            ActiveEffects = new List<string>()
        };

        GuildHeritages[guildId] = heritage;
        Statistics.TotalGuildsWithHeritages++;
        
        HeritageSignal.EmitSignal(GuildHeritageSignal.Type.GuildHeritageCreated, guildId, guildName);
        
        return heritage;
    }

    public bool AddContribution(string guildId, string playerId, int points)
    {
        if (!GuildHeritages.ContainsKey(guildId))
        {
            return false;
        }

        var guild = GuildHeritages[guildId];
        
        guild.TotalHeritagePoints += points;
        
        if (!guild.ContributionHistory.ContainsKey(playerId))
        {
            guild.ContributionHistory[playerId] = 0;
        }
        guild.ContributionHistory[playerId] += points;

        if (!PlayerData.ContainsKey(playerId))
        {
            PlayerData[playerId] = new PlayerHeritageData
            {
                PlayerId = playerId,
                PersonalContribution = 0,
                PersonalUnlocks = new Dictionary<string, int>()
            };
        }
        
        PlayerData[playerId].PersonalContribution += points;
        PlayerData[playerId].LastContributionTime = DateTime.Now;

        CheckAndUnlockHeritages(guild);

        guild.LastUpdated = DateTime.Now;
        
        HeritageSignal.EmitSignal(GuildHeritageSignal.Type.ContributionAdded, guildId, playerId, points);
        
        return true;
    }

    private void CheckAndUnlockHeritages(GuildHeritage guild)
    {
        var db = GuildHeritageDatabase.Instance;
        var types = Enum.GetValues(typeof(HeritageType));
        
        foreach (HeritageType type in types)
        {
            var herId = GetCurrentHeritageId(guild, type);
            if (herId == null)
            {
                herId = db.TierMapping[HeritageTier.Bronze][type];
            }
            else
            {
                var current = db.GetHeritage(herId);
                var tierOrder = new[] { HeritageTier.None, HeritageTier.Bronze, HeritageTier.Silver, HeritageTier.Gold, HeritageTier.Platinum, HeritageTier.Diamond };
                var nextTierIndex = Array.IndexOf(tierOrder, current.Tier) + 1;
                
                if (nextTierIndex >= tierOrder.Length)
                    continue;
                
                var nextTier = tierOrder[nextTierIndex];
                if (nextTier == HeritageTier.Diamond)
                    herId = db.TierMapping[HeritageTier.Diamond][type];
                else
                    herId = db.TierMapping[nextTier][type];
            }

            var heritage = db.GetHeritage(herId);
            if (heritage != null && guild.TotalHeritagePoints >= heritage.RequiredPoints)
            {
                if (!guild.UnlockedHeritages.ContainsKey(herId))
                {
                    guild.UnlockedHeritages[herId] = 1;
                    guild.ActiveEffects.Add(herId);
                    Statistics.TotalHeritagesUnlocked++;
                    
                    if (!Statistics.HeritagesByType.ContainsKey(type))
                        Statistics.HeritagesByType[type] = 0;
                    Statistics.HeritagesByType[type]++;
                    
                    HeritageSignal.EmitSignal(GuildHeritageSignal.Type.HeritageUnlocked, guild.GuildId, herId);
                }
            }
        }
    }

    private string GetCurrentHeritageId(GuildHeritage guild, HeritageType type)
    {
        var db = GuildHeritageDatabase.Instance;
        if (db.HeritagesByType.ContainsKey(type))
        {
            foreach (var id in db.HeritagesByType[type])
            {
                if (guild.UnlockedHeritages.ContainsKey(id))
                    return id;
            }
        }
        return null;
    }

    public List<HeritageBonus> GetGuildActiveHeritages(string guildId)
    {
        if (!GuildHeritages.ContainsKey(guildId))
            return new List<HeritageBonus>();

        var result = new List<HeritageBonus>();
        var db = GuildHeritageDatabase.Instance;
        
        foreach (var heritageId in GuildHeritages[guildId].ActiveEffects)
        {
            var heritage = db.GetHeritage(heritageId);
            if (heritage != null)
                result.Add(heritage);
        }
        
        return result;
    }

    public Dictionary<string, float> CalculateGuildBonuses(string guildId)
    {
        var bonuses = new Dictionary<string, float>
        {
            { "damage", 0f },
            { "defense", 0f },
            { "magic", 0f },
            { "gold", 0f },
            { "exp", 0f },
            { "dropRate", 0f }
        };

        var heritages = GetGuildActiveHeritages(guildId);
        
        foreach (var heritage in heritages)
        {
            bonuses["damage"] += heritage.DamageBonus;
            bonuses["defense"] += heritage.DefenseBonus;
            bonuses["magic"] += heritage.MagicBonus;
            bonuses["gold"] += heritage.GoldBonus;
            bonuses["exp"] += heritage.ExpBonus;
            bonuses["dropRate"] += heritage.DropRateBonus;
        }

        return bonuses;
    }

    public float GetDamageBonus(string guildId)
    {
        var bonuses = CalculateGuildBonuses(guildId);
        return bonuses["damage"];
    }

    public float GetDefenseBonus(string guildId)
    {
        var bonuses = CalculateGuildBonuses(guildId);
        return bonuses["defense"];
    }

    public float GetMagicBonus(string guildId)
    {
        var bonuses = CalculateGuildBonuses(guildId);
        return bonuses["magic"];
    }

    public float GetGoldBonus(string guildId)
    {
        var bonuses = CalculateGuildBonuses(guildId);
        return bonuses["gold"];
    }

    public float GetExpBonus(string guildId)
    {
        var bonuses = CalculateGuildBonuses(guildId);
        return bonuses["exp"];
    }

    public float GetDropRateBonus(string guildId)
    {
        var bonuses = CalculateGuildBonuses(guildId);
        return bonuses["dropRate"];
    }

    public List<HeritageBonus> GetAvailableUpgrades(string guildId)
    {
        if (!GuildHeritages.ContainsKey(guildId))
            return new List<HeritageBonus>();

        var result = new List<HeritageBonus>();
        var db = GuildHeritageDatabase.Instance;
        var guild = GuildHeritages[guildId];

        var types = Enum.GetValues(typeof(HeritageType));
        foreach (HeritageType type in types)
        {
            if (db.CanUpgrade(guild, type))
            {
                var herId = GetCurrentHeritageId(guild, type);
                if (herId == null)
                    herId = db.TierMapping[HeritageTier.Bronze][type];
                else
                {
                    var current = db.GetHeritage(herId);
                    var tierOrder = new[] { HeritageTier.None, HeritageTier.Bronze, HeritageTier.Silver, HeritageTier.Gold, HeritageTier.Platinum, HeritageTier.Diamond };
                    var nextTierIndex = Array.IndexOf(tierOrder, current.Tier) + 1;
                    
                    if (nextTierIndex < tierOrder.Length)
                    {
                        var nextTier = tierOrder[nextTierIndex];
                        herId = db.TierMapping[nextTier][type];
                    }
                }

                var heritage = db.GetHeritage(herId);
                if (heritage != null && guild.TotalHeritagePoints >= heritage.RequiredPoints)
                    result.Add(heritage);
            }
        }

        return result;
    }

    public Dictionary<string, int> GetTopContributors(string guildId, int count = 10)
    {
        if (!GuildHeritages.ContainsKey(guildId))
            return new Dictionary<string, int>();

        var contributors = new List<KeyValuePair<string, int>>();
        foreach (var kvp in GuildHeritages[guildId].ContributionHistory)
        {
            contributors.Add(kvp);
        }

        contributors.Sort((a, b) => b.Value.CompareTo(a.Value));

        var result = new Dictionary<string, int>();
        for (int i = 0; i < Math.Min(count, contributors.Count); i++)
        {
            result[contributors[i].Key] = contributors[i].Value;
        }

        return result;
    }

    public Dictionary<HeritageType, HeritageTier> GetGuildHeritageTiers(string guildId)
    {
        var result = new Dictionary<HeritageType, HeritageTier>();
        
        if (!GuildHeritages.ContainsKey(guildId))
            return result;

        var db = GuildHeritageDatabase.Instance;
        var guild = GuildHeritages[guildId];
        var types = Enum.GetValues(typeof(HeritageType));

        foreach (HeritageType type in types)
        {
            var currentId = GetCurrentHeritageId(guild, type);
            if (currentId != null)
            {
                var heritage = db.GetHeritage(currentId);
                result[type] = heritage.Tier;
            }
            else
            {
                result[type] = HeritageTier.None;
            }
        }

        return result;
    }

    protected override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        foreach (var kvp in GuildHeritages)
        {
            data[$"guild_{kvp.Key}_points"] = kvp.Value.TotalHeritagePoints;
            
            int i = 0;
            foreach (var heritageId in kvp.Value.UnlockedHeritages.Keys)
            {
                data[$"guild_{kvp.Key}_heritage_{i}"] = heritageId.GetHashCode();
                i++;
            }
        }

        return data;
    }

    protected override void ImportSaveData(Dictionary data)
    {
        GuildHeritages.Clear();
        
        foreach (var kvp in data)
        {
            if (kvp.Key.ToString().StartsWith("guild_") && kvp.Key.ToString().Contains("_points"))
            {
                var guildId = kvp.Key.ToString().Replace("guild_", "").Replace("_points", "");
                if (!GuildHeritages.ContainsKey(guildId))
                {
                    CreateGuildHeritage(guildId, "Imported Guild");
                }
                GuildHeritages[guildId].TotalHeritagePoints = Convert.ToInt32(kvp.Value);
            }
        }
        
        foreach (var kvp in data)
        {
            if (kvp.Key.ToString().StartsWith("guild_") && kvp.Key.ToString().Contains("_heritage_"))
            {
                var parts = kvp.Key.ToString().Split('_');
                if (parts.Length >= 3)
                {
                    var guildId = parts[1];
                    if (GuildHeritages.ContainsKey(guildId))
                    {
                        foreach (var heritage in GuildHeritageDatabase.Instance.Heritages.Values)
                        {
                            if (heritage.Id.GetHashCode() == Convert.ToInt32(kvp.Value))
                            {
                                GuildHeritages[guildId].UnlockedHeritages[heritage.Id] = 1;
                                GuildHeritages[guildId].ActiveEffects.Add(heritage.Id);
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        CheckAllGuildsForUnlocks();
    }

    private void CheckAllGuildsForUnlocks()
    {
        foreach (var guild in GuildHeritages.Values)
        {
            CheckAndUnlockHeritages(guild);
        }
    }
}

public class GuildHeritageSignal
{
    public enum Type
    {
        GuildHeritageCreated,
        HeritageUnlocked,
        ContributionAdded,
        HeritageUpgraded
    }

    public Type SignalType { get; set; }
    public string GuildId { get; set; }
    public string HeritageId { get; set; }
    public string PlayerId { get; set; }
    public int Value { get; set; }

    public GuildHeritageSignal(Type type, string guildId = "", string heritageId = "", string playerId = "", int value = 0)
    {
        SignalType = type;
        GuildId = guildId;
        HeritageId = heritageId;
        PlayerId = playerId;
        Value = value;
    }
}
