using Godot;
using System;
using System.Collections.Generic;

public class BattlePassManager : Node
{
    public static BattlePassManager Instance { get; private set; }
    
    // Battle Pass data
    private Dictionary<int, BattlePassSeason> seasons = new Dictionary<int, BattlePassSeason>();
    private int currentSeasonId = 1;
    private int playerLevel = 1;
    private int playerXP = 0;
    private int premiumCurrency = 0; // Diamonds
    private bool hasPremiumPass = false;
    
    // Season data structure
    private class BattlePassSeason
    {
        public int SeasonId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxLevel { get; set; }
        public int XpPerLevel { get; set; }
        public List<BattlePassReward> FreeRewards { get; set; }
        public List<BattlePassReward> PremiumRewards { get; set; }
        public List<BattlePassChallenge> Challenges { get; set; }
    }
    
    private class BattlePassReward
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // gold/item/diamond/exp
        public int Amount { get; set; }
        public string ItemId { get; set; }
    }
    
    private class BattlePassChallenge
    {
        public int ChallengeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // kill/collect/complete/survive
        public int Target { get; set; }
        public int Progress { get; set; }
        public bool IsCompleted { get; set; }
        public int XpReward { get; set; }
    }
    
    public override void _Ready()
    {
        Instance = this;
        InitializeSeasons();
    }
    
    private void InitializeSeasons()
    {
        // Season 1: Genesis
        var season1 = new BattlePassSeason
        {
            SeasonId = 1,
            Name = "Genesis",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(90),
            MaxLevel = 100,
            XpPerLevel = 1000,
            FreeRewards = new List<BattlePassReward>(),
            PremiumRewards = new List<BattlePassReward>(),
            Challenges = new List<BattlePassChallenge>()
        };
        
        // Free rewards (every 5 levels)
        string[] freeItems = { "gold", "health_potion", "enhancement_stone", "rare_chest", "epic_chest" };
        int[] freeAmounts = { 1000, 5, 10, 1, 1 };
        for (int i = 1; i <= 100; i += 5)
        {
            int idx = (i / 5) % freeItems.Length;
            season1.FreeRewards.Add(new BattlePassReward
            {
                Level = i,
                Name = $"Level {i} Reward",
                Type = freeItems[idx],
                Amount = freeAmounts[idx]
            });
        }
        
        // Premium rewards (every 5 levels, offset by 2)
        string[] premItems = { "diamond", "legendary_chest", "mount_token", "pet_token", "artifact" };
        int[] premAmounts = { 50, 1, 1, 1, 1 };
        for (int i = 1; i <= 100; i += 5)
        {
            int idx = (i / 5) % premItems.Length;
            season1.PremiumRewards.Add(new BattlePassReward
            {
                Level = i,
                Name = $"Premium Level {i}",
                Type = premItems[idx],
                Amount = premAmounts[idx]
            });
        }
        
        // Challenges
        season1.Challenges.Add(new BattlePassChallenge { ChallengeId = 1, Name = "Monster Slayer", Description = "Defeat 500 enemies", Type = "kill", Target = 500, XpReward = 5000 });
        season1.Challenges.Add(new BattlePassChallenge { ChallengeId = 2, Name = "Boss Hunter", Description = "Defeat 50 bosses", Type = "kill", Target = 50, XpReward = 10000 });
        season1.Challenges.Add(new BattlePassChallenge { ChallengeId = 3, Name = "Dungeon Master", Description = "Complete 20 dungeons", Type = "complete", Target = 20, XpReward = 8000 });
        season1.Challenges.Add(new BattlePassChallenge { ChallengeId = 4, Name = "Gold Hoarder", Description = "Earn 100,000 gold", Type = "collect", Target = 100000, XpReward = 3000 });
        season1.Challenges.Add(new BattlePassChallenge { ChallengeId = 5, Name = "Survivor", Description = "Survive 30 minutes in combat", Type = "survive", Target = 1800, XpReward = 5000 });
        season1.Challenges.Add(new BattlePassChallenge { ChallengeId = 6, Name = "Crit Master", Description = "Land 200 critical hits", Type = "kill", Target = 200, XpReward = 4000 });
        season1.Challenges.Add(new BattlePassChallenge { ChallengeId = 7, Name = "Trader", Description = "Complete 10 trades", Type = "complete", Target = 10, XpReward = 2000 });
        season1.Challenges.Add(new BattlePassChallenge { ChallengeId = 8, Name = "Craftsman", Description = "Craft 50 items", Type = "complete", Target = 50, XpReward = 3000 });
        
        seasons[1] = season1;
    }
    
    public void AddXP(int amount)
    {
        playerXP += amount;
        CheckLevelUp();
    }
    
    private void CheckLevelUp()
    {
        var season = seasons[currentSeasonId];
        while (playerXP >= season.XpPerLevel && playerLevel < season.MaxLevel)
        {
            playerXP -= season.XpPerLevel;
            playerLevel++;
        }
    }
    
    public void UpdateChallengeProgress(string type, int amount)
    {
        var season = seasons[currentSeasonId];
        foreach (var challenge in season.Challenges)
        {
            if (challenge.Type == type && !challenge.IsCompleted)
            {
                challenge.Progress += amount;
                if (challenge.Progress >= challenge.Target)
                {
                    challenge.IsCompleted = true;
                    AddXP(challenge.XpReward);
                }
            }
        }
    }
    
    public bool ClaimReward(int level, bool isPremium)
    {
        var season = seasons[currentSeasonId];
        var rewards = isPremium ? season.PremiumRewards : season.FreeRewards;
        
        foreach (var reward in rewards)
        {
            if (reward.Level == level)
            {
                // Grant reward based on type
                switch (reward.Type)
                {
                    case "gold":
                        GameManager.Instance.AddGold(reward.Amount);
                        break;
                    case "diamond":
                        premiumCurrency += reward.Amount;
                        break;
                    case "exp":
                        GameManager.Instance.AddExp(reward.Amount);
                        break;
                    // Item rewards would be added to inventory
                }
                return true;
            }
        }
        return false;
    }
    
    public void PurchasePremiumPass()
    {
        // In real implementation, would use in-app purchase
        hasPremiumPass = true;
    }
    
    // Getters
    public int GetCurrentLevel() => playerLevel;
    public int GetCurrentXP() => playerXP;
    public int GetXPToNextLevel() => seasons[currentSeasonId].XpPerLevel;
    public bool HasPremiumPass() => hasPremiumPass;
    public int GetPremiumCurrency() => premiumCurrency;
    public string GetSeasonName() => seasons[currentSeasonId].Name;
    public int GetDaysRemaining() => (seasons[currentSeasonId].EndDate - DateTime.Now).Days;
    
    public List<BattlePassReward> GetFreeRewards() => seasons[currentSeasonId].FreeRewards;
    public List<BattlePassReward> GetPremiumRewards() => seasons[currentSeasonId].PremiumRewards;
    public List<BattlePassChallenge> GetChallenges() => seasons[currentSeasonId].Challenges;
    
    public Dictionary<string, object> GetSaveData()
    {
        return new Dictionary<string, object>
        {
            { "currentSeasonId", currentSeasonId },
            { "playerLevel", playerLevel },
            { "playerXP", playerXP },
            { "premiumCurrency", premiumCurrency },
            { "hasPremiumPass", hasPremiumPass }
        };
    }
    
    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("currentSeasonId")) currentSeasonId = (int)data["currentSeasonId"];
        if (data.ContainsKey("playerLevel")) playerLevel = (int)data["playerLevel"];
        if (data.ContainsKey("playerXP")) playerXP = (int)data["playerXP"];
        if (data.ContainsKey("premiumCurrency")) premiumCurrency = (int)data["premiumCurrency"];
        if (data.ContainsKey("hasPremiumPass")) hasPremiumPass = (bool)data["hasPremiumPass"];
    }
}
