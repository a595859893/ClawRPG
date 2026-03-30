using Godot;
using System;
using System.Collections.Generic;

public partial class TitleCollectionDatabase : BaseSystem
{
    // 标题稀有度颜色
    public static readonly Godot.Color CommonColor = new Godot.Color(0.7f, 0.7f, 0.7f);
    public static readonly Godot.Color UncommonColor = new Godot.Color(0.2f, 0.8f, 0.2f);
    public static readonly Godot.Color RareColor = new Godot.Color(0.2f, 0.5f, 1.0f);
    public static readonly Godot.Color EpicColor = new Godot.Color(0.6f, 0.3f, 0.9f);
    public static readonly Godot.Color LegendaryColor = new Godot.Color(1.0f, 0.6f, 0.0f);
    
    // 标题配置列表
    private static List<Dictionary> _titleConfigs = new List<Dictionary>
    {
        // Combat Titles
        new Dictionary { {"id", "warrior_legend"}, {"name", "Warrior Legend"}, {"category", "Combat"}, {"rarity", "Legendary"}, {"requirement", "Defeat 10000 enemies"}, {"description", "A legendary warrior known throughout the realm"} },
        new Dictionary { {"id", "boss_slayer"}, {"name", "Boss Slayer"}, {"category", "Combat"}, {"rarity", "Epic"}, {"requirement", "Defeat 500 bosses"}, {"description", "One who has slain countless bosses"} },
        new Dictionary { {"id", "millionaire_killer"}, {"name", "Millionaire Killer"}, {"category", "Combat"}, {"rarity", "Rare"}, {"requirement", "Defeat 1000000 enemies"}, {"description", "An elite combatant with countless victories"} },
        new Dictionary { {"id", "enemy_annihilator"}, {"name", "Enemy Annihilator"}, {"category", "Combat"}, {"rarity", "Uncommon"}, {"requirement", "Defeat 1000 enemies"}, {"description", "A proven warrior in battle"} },
        new Dictionary { {"id", "novice_hunter"}, {"name", "Novice Hunter"}, {"category", "Combat"}, {"rarity", "Common"}, {"requirement", "Defeat 100 enemies"}, {"description", "A beginning adventurer"} },
        
        // Level Titles
        new Dictionary { {"id", "immortal_being"}, {"name", "Immortal Being"}, {"category", "Level"}, {"rarity", "Legendary"}, {"requirement", "Reach level 200"}, {"description", "An immortal being of immense power"} },
        new Dictionary { {"id", "ancient_one"}, {"name", "Ancient One"}, {"category", "Level"}, {"rarity", "Epic"}, {"requirement", "Reach level 150"}, {"description", "An ancient being of great wisdom"} },
        new Dictionary { {"id", "veteran_hero"}, {"name", "Veteran Hero"}, {"category", "Level"}, {"rarity", "Rare"}, {"requirement", "Reach level 100"}, {"description", "A veteran hero of many adventures"} },
        new Dictionary { {"id", "seasoned_adventurer"}, {"name", "Seasoned Adventurer"}, {"category", "Level"}, {"rarity", "Uncommon"}, {"requirement", "Reach level 50"}, {"description", "A seasoned adventurer"} },
        new Dictionary { {"id", "newcomer"}, {"name", "Newcomer"}, {"category", "Level"}, {"rarity", "Common"}, {"requirement", "Reach level 10"}, {"description", "A fresh newcomer to the world"} },
        
        // Gold Titles
        new Dictionary { {"id", "wealth_tycoon"}, {"name", "Wealth Tycoon"}, {"category", "Gold"}, {"rarity", "Legendary"}, {"requirement", "Accumulate 10000000 gold"}, {"description", "A tycoon of immense wealth"} },
        new Dictionary { {"id", "gold_master"}, {"name", "Gold Master"}, {"category", "Gold"}, {"rarity", "Epic"}, {"requirement", "Accumulate 1000000 gold"}, {"description", "A master of wealth"} },
        new Dictionary { {"id", "rich_merchant"}, {"name", "Rich Merchant"}, {"category", "Gold"}, {"rarity", "Rare"}, {"requirement", "Accumulate 100000 gold"}, {"description", "A wealthy merchant"} },
        new Dictionary { {"id", "coin_collector"}, {"name", "Coin Collector"}, {"category", "Gold"}, {"rarity", "Uncommon"}, {"requirement", "Accumulate 10000 gold"}, {"description", "One who collects coins"} },
        new Dictionary { {"id", "penny_saver"}, {"name", "Penny Saver"}, {"category", "Gold"}, {"rarity", "Common"}, {"requirement", "Accumulate 1000 gold"}, {"description", "Learning to save"} },
        
        // Pet Titles
        new Dictionary { {"id", "pet_master"}, {"name", "Pet Master"}, {"category", "Pet"}, {"rarity", "Legendary"}, {"requirement", "Own 25 unique pets"}, {"description", "The master of all pets"} },
        new Dictionary { {"id", "pet_whisperer"}, {"name", "Pet Whisperer"}, {"category", "Pet"}, {"rarity", "Epic"}, {"requirement", "Own 15 unique pets"}, {"description", "One who speaks to pets"} },
        new Dictionary { {"id", "pet_friend"}, {"name", "Pet Friend"}, {"category", "Pet"}, {"rarity", "Rare"}, {"requirement", "Own 8 unique pets"}, {"description", "A friend to all creatures"} },
        new Dictionary { {"id", "animal_companion"}, {"name", "Animal Companion"}, {"category", "Pet"}, {"rarity", "Uncommon"}, {"requirement", "Own 3 unique pets"}, {"description", "One with animal companions"} },
        new Dictionary { {"id", "pet_owner"}, {"name", "Pet Owner"}, {"category", "Pet"}, {"rarity", "Common"}, {"requirement", "Own 1 unique pet"}, {"description", "A new pet owner"} },
        
        // Dungeon Titles
        new Dictionary { {"id", "dungeon_conqueror"}, {"name", "Dungeon Conqueror"}, {"category", "Dungeon"}, {"rarity", "Legendary"}, {"requirement", "Complete 500 dungeons"}, {"description", "One who has conquered countless dungeons"} },
        new Dictionary { {"id", "dungeon_explorer"}, {"name", "Dungeon Explorer"}, {"category", "Dungeon"}, {"rarity", "Epic"}, {"requirement", "Complete 200 dungeons"}, {"description", "An explorer of dark dungeons"} },
        new Dictionary { {"id", "dungeon_delver"}, {"name", "Dungeon Delver"}, {"category", "Dungeon"}, {"rarity", "Rare"}, {"requirement", "Complete 50 dungeons"}, {"description", "A brave dungeon delver"} },
        new Dictionary { {"id", "cave_seeker"}, {"name", "Cave Seeker"}, {"category", "Dungeon"}, {"rarity", "Uncommon"}, {"requirement", "Complete 10 dungeons"}, {"description", "One who seeks caves"} },
        new Dictionary { {"id", "dungeon_visitor"}, {"name", "Dungeon Visitor"}, {"category", "Dungeon"}, {"rarity", "Common"}, {"requirement", "Complete 1 dungeon"}, {"description", "A first-time dungeon visitor"} },
        
        // Achievement Titles
        new Dictionary { {"id", "achievement_hunter"}, {"name", "Achievement Hunter"}, {"category", "Achievement"}, {"rarity", "Epic"}, {"requirement", "Unlock 100 achievements"}, {"description", "One who seeks all achievements"} },
        new Dictionary { {"id", "trophy_collector"}, {"name", "Trophy Collector"}, {"category", "Achievement"}, {"rarity", "Rare"}, {"requirement", "Unlock 50 achievements"}, {"description", "A collector of trophies"} },
        new Dictionary { {"id", "milestone_maker"}, {"name", "Milestone Maker"}, {"category", "Achievement"}, {"rarity", "Uncommon"}, {"requirement", "Unlock 20 achievements"}, {"description", "One who reaches milestones"} },
        new Dictionary { {"id", "first_steps"}, {"name", "First Steps"}, {"category", "Achievement"}, {"rarity", "Common"}, {"requirement", "Unlock 5 achievements"}, {"description", "Taking first steps"} },
        
        // Win Titles
        new Dictionary { {"id", "champion_eternal"}, {"name", "Champion Eternal"}, {"category", "Win"}, {"rarity", "Legendary"}, {"requirement", "Win 500 games"}, {"description", "An eternal champion"} },
        new Dictionary { {"id", "victorious_one"}, {"name", "Victorious One"}, {"category", "Win"}, {"rarity", "Epic"}, {"requirement", "Win 100 games"}, {"description", "One who achieves victory"} },
        new Dictionary { {"id", "battle_winner"}, {"name", "Battle Winner"}, {"category", "Win"}, {"rarity", "Rare"}, {"requirement", "Win 25 games"}, {"description", "A winner of battles"} },
        new Dictionary { {"id", "first_victory"}, {"name", "First Victory"}, {"category", "Win"}, {"rarity", "Common"}, {"requirement", "Win 1 game"}, {"description", "A first taste of victory"} },
        
        // Special Titles
        new Dictionary { {"id", "living_legend"}, {"name", "Living Legend"}, {"category", "Special"}, {"rarity", "Legendary"}, {"requirement", "Collect all other titles"}, {"description", "A living legend"} },
        new Dictionary { {"id", "completionist"}, {"name", "Completionist"}, {"category", "Special"}, {"rarity", "Epic"}, {"requirement", "Collect 30+ titles"}, {"description", "One who completes everything"} },
    };
    
    public static List<Dictionary> GetAllTitleConfigs()
    {
        return _titleConfigs;
    }
    
    public static Dictionary GetTitleById(string id)
    {
        foreach (var title in _titleConfigs)
        {
            if ((string)title["id"] == id)
            {
                return title;
            }
        }
        return null;
    }
    
    public static List<Dictionary> GetTitlesByCategory(string category)
    {
        List<Dictionary> result = new List<Dictionary>();
        foreach (var title in _titleConfigs)
        {
            if ((string)title["category"] == category)
            {
                result.Add(title);
            }
        }
        return result;
    }
    
    public static List<Dictionary> GetTitlesByRarity(string rarity)
    {
        List<Dictionary> result = new List<Dictionary>();
        foreach (var title in _titleConfigs)
        {
            if ((string)title["rarity"] == rarity)
            {
                result.Add(title);
            }
        }
        return result;
    }
    
    public static Godot.Color GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "Common": return CommonColor;
            case "Uncommon": return UncommonColor;
            case "Rare": return RareColor;
            case "Epic": return EpicColor;
            case "Legendary": return LegendaryColor;
            default: return CommonColor;
        }
    }
    
    public static string[] GetCategories()
    {
        return new string[] { "Combat", "Level", "Gold", "Pet", "Dungeon", "Achievement", "Win", "Special" };
    }
    
    public static string[] GetRarities()
    {
        return new string[] { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
    }
}
