using Godot;
using System;
using System.Collections.Generic;

public class StreakDatabase
{
    private static StreakDatabase _instance;
    public static StreakDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new StreakDatabase();
            return _instance;
        }
    }
    
    // Streak rewards by streak length
    public Dictionary<int, StreakReward> LoginRewards { get; private set; } = new Dictionary<int, StreakReward>();
    public Dictionary<int, StreakReward> BattleRewards { get; private set; } = new Dictionary<int, StreakReward>();
    public Dictionary<int, StreakReward> QuestRewards { get; private set; } = new Dictionary<int, StreakReward>();
    public Dictionary<int, StreakReward> DungeonRewards { get; private set; } = new Dictionary<int, StreakReward>();
    public Dictionary<int, StreakReward> PetInteractionRewards { get; private set; } = new Dictionary<int, StreakReward>();
    
    // Streak milestone rewards
    public Dictionary<int, StreakReward> MilestoneRewards { get; private set; } = new Dictionary<int, StreakReward>();
    
    // Streak freeze cost
    public int StreakFreezeCost { get; private set; } = 100;
    public int MaxFreezeTokens { get; private set; } = 3;
    
    // Streak decay time (hours)
    public int StreakDecayHours { get; private set; } = 36;
    
    public StreakDatabase()
    {
        InitializeLoginRewards();
        InitializeBattleRewards();
        InitializeQuestRewards();
        InitializeDungeonRewards();
        InitializePetInteractionRewards();
        InitializeMilestoneRewards();
    }
    
    private void InitializeLoginRewards()
    {
        // Daily login rewards
        LoginRewards[1] = new StreakReward { Gold = 50, Exp = 25, ItemId = "", ItemCount = 0 };
        LoginRewards[2] = new StreakReward { Gold = 75, Exp = 40, ItemId = "", ItemCount = 0 };
        LoginRewards[3] = new StreakReward { Gold = 100, Exp = 60, ItemId = "health_potion", ItemCount = 3 };
        LoginRewards[4] = new StreakReward { Gold = 125, Exp = 80, ItemId = "", ItemCount = 0 };
        LoginRewards[5] = new StreakReward { Gold = 150, Exp = 100, ItemId = "mana_potion", ItemCount = 3 };
        LoginRewards[6] = new StreakReward { Gold = 200, Exp = 125, ItemId = "", ItemCount = 0 };
        LoginRewards[7] = new StreakReward { Gold = 300, Exp = 200, ItemId = "rare_chest", ItemCount = 1 };
        
        // Loop rewards for longer streaks
        for (int i = 8; i <= 30; i++)
        {
            int loopDay = ((i - 1) % 7) + 1;
            int multiplier = 1 + (i / 7);
            LoginRewards[i] = new StreakReward
            {
                Gold = LoginRewards[loopDay].Gold * multiplier,
                Exp = LoginRewards[loopDay].Exp * multiplier,
                ItemId = i % 7 == 0 ? "epic_chest" : LoginRewards[loopDay].ItemId,
                ItemCount = i % 7 == 0 ? 1 : LoginRewards[loopDay].ItemCount
            };
        }
    }
    
    private void InitializeBattleRewards()
    {
        // Battle streak rewards (consecutive days of battling)
        BattleRewards[1] = new StreakReward { Gold = 30, Exp = 20, ItemId = "", ItemCount = 0 };
        BattleRewards[3] = new StreakReward { Gold = 75, Exp = 50, ItemId = "strength_scroll", ItemCount = 1 };
        BattleRewards[5] = new StreakReward { Gold = 150, Exp = 100, ItemId = "rare_gem", ItemCount = 1 };
        BattleRewards[7] = new StreakReward { Gold = 300, Exp = 200, ItemId = "epic_chest", ItemCount = 1 };
        BattleRewards[10] = new StreakReward { Gold = 500, Exp = 350, ItemId = "legendary_chest", ItemCount = 1 };
        BattleRewards[15] = new StreakReward { Gold = 750, Exp = 500, ItemId = "legendary_gem", ItemCount = 2 };
        BattleRewards[30] = new StreakReward { Gold = 1500, Exp = 1000, ItemId = "ancient_relic", ItemCount = 1 };
    }
    
    private void InitializeQuestRewards()
    {
        // Quest completion streak rewards
        QuestRewards[1] = new StreakReward { Gold = 25, Exp = 15, ItemId = "", ItemCount = 0 };
        QuestRewards[3] = new StreakReward { Gold = 60, Exp = 40, ItemId = "quest_scroll", ItemCount = 1 };
        QuestRewards[5] = new StreakReward { Gold = 120, Exp = 80, ItemId = "epic_chest", ItemCount = 1 };
        QuestRewards[7] = new StreakReward { Gold = 250, Exp = 175, ItemId = "rare_material", ItemCount = 2 };
        QuestRewards[10] = new StreakReward { Gold = 400, Exp = 300, ItemId = "legendary_chest", ItemCount = 1 };
    }
    
    private void InitializeDungeonRewards()
    {
        // Dungeon completion streak rewards
        DungeonRewards[1] = new StreakReward { Gold = 40, Exp = 30, ItemId = "", ItemCount = 0 };
        DungeonRewards[3] = new StreakReward { Gold = 100, Exp = 75, ItemId = "dungeon_key", ItemCount = 1 };
        DungeonRewards[5] = new StreakReward { Gold = 200, Exp = 150, ItemId = "epic_chest", ItemCount = 1 };
        DungeonRewards[7] = new StreakReward { Gold = 350, Exp = 250, ItemId = "legendary_chest", ItemCount = 1 };
        DungeonRewards[10] = new StreakReward { Gold = 600, Exp = 450, ItemId = "ancient_artifact", ItemCount = 1 };
    }
    
    private void InitializePetInteractionRewards()
    {
        // Pet interaction streak rewards
        PetInteractionRewards[1] = new StreakReward { Gold = 20, Exp = 10, ItemId = "", ItemCount = 0 };
        PetInteractionRewards[3] = new StreakReward { Gold = 50, Exp = 30, ItemId = "pet_food", ItemCount = 3 };
        PetInteractionRewards[5] = new StreakReward { Gold = 100, Exp = 60, ItemId = "pet_toy", ItemCount = 1 };
        PetInteractionRewards[7] = new StreakReward { Gold = 200, Exp = 125, ItemId = "epic_pet_chest", ItemCount = 1 };
        PetInteractionRewards[10] = new StreakReward { Gold = 350, Exp = 250, ItemId = "legendary_pet_chest", ItemCount = 1 };
    }
    
    private void InitializeMilestoneRewards()
    {
        // Special milestone rewards for any streak type
        MilestoneRewards[7] = new StreakReward { Gold = 500, Exp = 300, ItemId = "streak_booster", ItemCount = 1 };
        MilestoneRewards[14] = new StreakReward { Gold = 1000, Exp = 600, ItemId = "streak_shield", ItemCount = 1 };
        MilestoneRewards[30] = new StreakReward { Gold = 2500, Exp = 1500, ItemId = "streak_guardian", ItemCount = 1 };
        MilestoneRewards[60] = new StreakReward { Gold = 5000, Exp = 3000, ItemId = "streak_legend", ItemCount = 1 };
        MilestoneRewards[100] = new StreakReward { Gold = 10000, Exp = 5000, ItemId = "streak_eternal", ItemCount = 1 };
    }
    
    public StreakReward GetReward(StreakType type, int streakLength)
    {
        Dictionary<int, StreakReward> rewards = type switch
        {
            StreakType.Login => LoginRewards,
            StreakType.Battle => BattleRewards,
            StreakType.Quest => QuestRewards,
            StreakType.Dungeon => DungeonRewards,
            StreakType.PetInteraction => PetInteractionRewards,
            _ => LoginRewards
        };
        
        if (rewards.ContainsKey(streakLength))
            return rewards[streakLength];
        
        // Find closest lower key
        int closestKey = 0;
        foreach (var key in rewards.Keys)
        {
            if (key < streakLength && key > closestKey)
                closestKey = key;
        }
        
        return closestKey > 0 ? rewards[closestKey] : new StreakReward();
    }
    
    public StreakReward GetMilestoneReward(int streakLength)
    {
        if (MilestoneRewards.ContainsKey(streakLength))
            return MilestoneRewards[streakLength];
        
        // Find closest milestone
        int[] milestones = { 7, 14, 30, 60, 100 };
        int closest = 0;
        foreach (var m in milestones)
        {
            if (m < streakLength && m > closest)
                closest = m;
        }
        
        return closest > 0 ? MilestoneRewards[closest] : new StreakReward();
    }
}

public class StreakReward
{
    public int Gold { get; set; } = 0;
    public int Exp { get; set; } = 0;
    public string ItemId { get; set; } = "";
    public int ItemCount { get; set; } = 0;
}
