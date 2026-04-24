using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 阵营系统 - 管理玩家阵营选择和阵营声望
/// </summary>
public partial class FactionSystem : BaseSystem
{
    // Singleton instance
    public static FactionSystem Instance { get; private set; }
    
    // Faction definitions
    private Dictionary<string, Faction> factions = new Dictionary<string, Faction>();
    
    // Player faction reputation
    private Dictionary<string, int> playerReputation = new Dictionary<string, int>();
    
    // Reputation constants
    public const int MIN_REPUTATION = -1000;
    public const int MAX_REPUTATION = 1000;
    public const int REP_PER_LEVEL = 200;
    
    // Faction relationship modifiers
    private Dictionary<string, Dictionary<string, int>> factionRelations = new Dictionary<string, Dictionary<string, int>>();
    
    public override void _Ready()
    {
        Instance = this;
        InitializeFactions();
        LoadData();
    }
    
    /// <summary>
    /// 系统名称
    /// </summary>
    protected override string SystemName => "Faction";
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        var reputations = new Dictionary<string, object>();
        foreach (var kvp in playerReputation)
        {
            reputations[kvp.Key] = kvp.Value;
        }
        
        data["reputations"] = reputations;
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null || !data.ContainsKey("reputations")) return;
        
        var reputations = (Dictionary)data["reputations"];
        foreach (var kvp in reputations)
        {
            if (playerReputation.ContainsKey(kvp.Key))
            {
                playerReputation[kvp.Key] = (int)kvp.Value;
            }
        }
    }
    
    private void InitializeFactions()
    {
        // Create factions
        AddFaction("Warriors", "The Order of the Crimson Blade", FactionType.Military, 
            "A prestigious warrior order dedicated to martial prowess and honor in battle.",
            "#FF4444", new string[] { "Knights", "Merchants" }, new string[] { "Bandits", "Cultists" });
            
        AddFaction("Mages", "The Arcane Council", FactionType.Magic,
            "A secretive council of powerful mages who guard ancient knowledge.",
            "#8844FF", new string[] { "Scholars" }, new string[] { "Cultists", "Bandits" });
            
        AddFaction("Merchants", "The Golden Trade Guild", FactionType.Economic,
            "A wealthy guild controlling trade routes across the realm.",
            "#FFD700", new string[] { "Warriors", "Scholars" }, new string[] { "Bandits" });
            
        AddFaction("Scholars", "The University of Light", FactionType.Academic,
            "Academics and researchers devoted to discovering truth and knowledge.",
            "#44AAFF", new string[] { "Mages", "Merchants" }, new string[] { "Cultists" });
            
        AddFaction("Cultists", "The Shadow Brotherhood", FactionType.Criminal,
            "A mysterious organization operating in the shadows.",
            "#444444", new string[] { }, new string[] { "Warriors", "Mages", "Scholars" });
            
        AddFaction("Bandits", "The Outlaw Band", FactionType.Criminal,
            "Renegades and criminals who reject society's laws.",
            "#AA4444", new string[] { "Cultists" }, new string[] { "Warriors", "Merchants" });
            
        AddFaction("Knights", "The Holy Order", FactionType.Military,
            "Holy knights dedicated to protecting the innocent and fighting evil.",
            "#FFFFFF", new string[] { "Warriors", "Scholars" }, new string[] { "Cultists", "Bandits" });
            
        AddFaction("Healers", "The Temple of Life", FactionType.Religious,
            "Healers who serve those in need and maintain sacred sites.",
            "#44FF88", new string[] { "Knights", "Scholars" }, new string[] { "Cultists" });
            
        // Set up faction relationships
        SetupFactionRelations();
    }
    
    private void AddFaction(string id, string name, FactionType type, string description, 
        string color, string[] allies, string[] enemies)
    {
        Faction faction = new Faction
        {
            Id = id,
            Name = name,
            Type = type,
            Description = description,
            Color = color,
            Allies = new List<string>(allies),
            Enemies = new List<string>(enemies)
        };
        factions[id] = faction;
        playerReputation[id] = 0; // Start at neutral
    }
    
    private void SetupFactionRelations()
    {
        // Initialize faction relations
        foreach (var faction in factions.Keys)
        {
            factionRelations[faction] = new Dictionary<string, int>();
        }
        
        // Set relationship modifiers
        foreach (var kvp in factions)
        {
            string factionId = kvp.Key;
            Faction faction = kvp.Value;
            
            foreach (string ally in faction.Allies)
            {
                if (factions.ContainsKey(ally))
                {
                    factionRelations[factionId][ally] = 50; // Positive relation
                }
            }
            
            foreach (string enemy in faction.Enemies)
            {
                if (factions.ContainsKey(enemy))
                {
                    factionRelations[factionId][enemy] = -50; // Negative relation
                }
            }
        }
    }
    
    // Get reputation level for a faction
    public ReputationLevel GetReputationLevel(string factionId)
    {
        if (!playerReputation.ContainsKey(factionId))
            return ReputationLevel.Neutral;
            
        int rep = playerReputation[factionId];
        
        if (rep >= 800) return ReputationLevel.Exalted;
        if (rep >= 600) return ReputationLevel.Honored;
        if (rep >= 400) return ReputationLevel.Friendly;
        if (rep >= 200) return ReputationLevel.Neutral;
        if (rep >= 0) return ReputationLevel.Neutral;
        if (rep >= -200) return ReputationLevel.Unfriendly;
        if (rep >= -400) return ReputationLevel.Hostile;
        return ReputationLevel.Hated;
    }
    
    // Get reputation level name
    public string GetReputationLevelName(string factionId)
    {
        return GetReputationLevel(factionId).ToString();
    }
    
    // Modify reputation
    public void ModifyReputation(string factionId, int amount)
    {
        if (!playerReputation.ContainsKey(factionId))
            playerReputation[factionId] = 0;
        
        playerReputation[factionId] = Mathf.Clamp(
            playerReputation[factionId] + amount, 
            MIN_REPUTATION, 
            MAX_REPUTATION
        );
        
        // Also affect related factions
        if (factionRelations.ContainsKey(factionId))
        {
            foreach (var kvp in factionRelations[factionId])
            {
                string relatedFaction = kvp.Key;
                int relationModifier = kvp.Value;
                
                if (playerReputation.ContainsKey(relatedFaction))
                {
                    int indirectChange = (int)(amount * relationModifier * 0.01);
                    playerReputation[relatedFaction] = Mathf.Clamp(
                        playerReputation[relatedFaction] + indirectChange,
                        MIN_REPUTATION,
                        MAX_REPUTATION
                    );
                }
            }
        }
        
        SaveData();
        EmitSignal(nameof(ReputationChanged), factionId, playerReputation[factionId]);
    }
    
    // Get faction info
    public Faction GetFaction(string factionId)
    {
        return factions.ContainsKey(factionId) ? factions[factionId] : null;
    }
    
    // Get all factions
    public List<Faction> GetAllFactions()
    {
        return new List<Faction>(factions.Values);
    }
    
    // Get player reputation for a faction
    public int GetReputation(string factionId)
    {
        return playerReputation.ContainsKey(factionId) ? playerReputation[factionId] : 0;
    }
    
    // Get all player reputations
    public Dictionary<string, int> GetAllReputations()
    {
        return new Dictionary<string, int>(playerReputation);
    }
    
    // Calculate faction bonus/penalty
    public float GetFactionBonus(string factionId)
    {
        ReputationLevel level = GetReputationLevel(factionId);
        
        switch (level)
        {
            case ReputationLevel.Exalted: return 1.25f;
            case ReputationLevel.Honored: return 1.15f;
            case ReputationLevel.Friendly: return 1.10f;
            case ReputationLevel.Unfriendly: return 0.90f;
            case ReputationLevel.Hostile: return 0.75f;
            case ReputationLevel.Hated: return 0.50f;
            default: return 1.0f;
        }
    }
    
    // Get discount for faction merchants
    public float GetMerchantDiscount(string factionId)
    {
        ReputationLevel level = GetReputationLevel(factionId);
        
        switch (level)
        {
            case ReputationLevel.Exalted: return 0.25f;
            case ReputationLevel.Honored: return 0.20f;
            case ReputationLevel.Friendly: return 0.15f;
            case ReputationLevel.Hostile: return 0.10f; // Surcharge actually
            case ReputationLevel.Hated: return 0.25f;
            default: return 0.0f;
        }
    }
    
    // Check if player can access faction vendor
    public bool CanAccessVendor(string factionId)
    {
        return GetReputationLevel(factionId) >= ReputationLevel.Friendly;
    }
    
    // Get quests available from faction
    public List<string> GetAvailableQuests(string factionId)
    {
        List<string> quests = new List<string>();
        ReputationLevel level = GetReputationLevel(factionId);
        
        // Quest availability based on reputation
        switch (level)
        {
            case ReputationLevel.Hated:
                quests.Add("faction_punish_" + factionId);
                break;
            case ReputationLevel.Hostile:
                quests.Add("faction_stealth_" + factionId);
                break;
            case ReputationLevel.Unfriendly:
                quests.Add("faction_earn_rep_" + factionId);
                break;
            case ReputationLevel.Neutral:
                quests.Add("faction_intro_" + factionId);
                quests.Add("faction_gather_" + factionId);
                break;
            case ReputationLevel.Friendly:
                quests.Add("faction_deliver_" + factionId);
                quests.Add("faction_hunt_" + factionId);
                break;
            case ReputationLevel.Honored:
                quests.Add("faction_elite_" + factionId);
                quests.Add("faction_escort_" + factionId);
                break;
            case ReputationLevel.Exalted:
                quests.Add("faction_legendary_" + factionId);
                quests.Add("faction_leader_" + factionId);
                break;
        }
        
        return quests;
    }
    
    // Complete a faction quest
    public void CompleteFactionQuest(string questId, string factionId)
    {
        // Base reputation reward
        int baseReward = 100;
        
        // Bonus based on quest difficulty
        if (questId.ContainsKey("legendary") || questId.ContainsKey("leader"))
            baseReward = 300;
        else if (questId.ContainsKey("elite") || questId.ContainsKey("escort"))
            baseReward = 200;
        else if (questId.ContainsKey("hunt") || questId.ContainsKey("deliver"))
            baseReward = 150;
        
        ModifyReputation(factionId, baseReward);
    }
    
    // Save/Load functionality
    public void SaveData()
    {
        // Save reputation data
        foreach (var kvp in playerReputation)
        {
            PlayerPrefs.SetInt("faction_rep_" + kvp.Key, kvp.Value);
        }
    }
    
    public void LoadData()
    {
        // Load reputation data
        foreach (string factionId in factions.Keys)
        {
            if (PlayerPrefs.HasKey("faction_rep_" + factionId))
            {
                playerReputation[factionId] = PlayerPrefs.GetInt("faction_rep_" + factionId);
            }
        }
    }
    
    // Signal
}

// Faction class
public class Faction
{
    public string Id { get; set; }
    public string Name { get; set; }
    public FactionType Type { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public List<string> Allies { get; set; }
    public List<string> Enemies { get; set; }
}

// Faction types
public enum FactionType
{
    Military,
    Magic,
    Economic,
    Academic,
    Criminal,
    Religious
}

// Reputation levels
public enum ReputationLevel
{
    Hated = -3,
    Hostile = -2,
    Unfriendly = -1,
    Neutral = 0,
    Friendly = 1,
    Honored = 2,
    Exalted = 3
}
