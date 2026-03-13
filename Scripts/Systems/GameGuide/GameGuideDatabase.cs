using System;
using System.Collections.Generic;

public static class GameGuideDatabase
{
    // 指南类别配置
    public static Dictionary<string, GuideCategory> Categories = new Dictionary<string, GuideCategory>
    {
        // 基础类别 - 初始解锁
        { "getting_started", new GuideCategory { Id = "getting_started", Name = "Getting Started", Description = "Learn the basics of the game", Icon = "res://icons/book.png", IsDefaultUnlocked = true, SortOrder = 1 } },
        { "combat", new GuideCategory { Id = "combat", Name = "Combat Basics", Description = "Learn how to fight", Icon = "res://icons/sword.png", IsDefaultUnlocked = true, SortOrder = 2 } },
        
        // 进阶类别 - 需要完成前置
        { "advanced_combat", new GuideCategory { Id = "advanced_combat", Name = "Advanced Combat", Description = "Master combat techniques", Icon = "res://icons/shield.png", UnlockRequirement = "combat_basics", SortOrder = 3 } },
        { "pets", new GuideCategory { Id = "pets", Name = "Pet System", Description = "Learn about pets", Icon = "res://icons/pet.png", UnlockRequirement = "getting_started", SortOrder = 4 } },
        { "items", new GuideCategory { Id = "items", Name = "Items & Equipment", Description = "Understand items", Icon = "res://icons/chest.png", UnlockRequirement = "getting_started", SortOrder = 5 } },
        
        // 高级类别
        { "dungeons", new GuideCategory { Id = "dungeons", Name = "Dungeons", Description = "Explore dungeons", Icon = "res://icons/dungeon.png", UnlockRequirement = "combat_basics", SortOrder = 6 } },
        { "multiplayer", new GuideCategory { Id = "multiplayer", Name = "Multiplayer", Description = "Play with others", Icon = "res://icons/party.png", UnlockRequirement = "advanced_combat", SortOrder = 7 } },
        { "guilds", new GuideCategory { Id = "guilds", Name = "Guilds", Description = "Join or create guilds", Icon = "res://icons/castle.png", UnlockRequirement = "multiplayer", SortOrder = 8 } },
        { "economy", new GuideCategory { Id = "economy", Name = "Economy", Description = "Learn trading", Icon = "res://icons/coin.png", UnlockRequirement = "items", SortOrder = 9 } },
        
        // 专家类别
        { "crafting", new GuideCategory { Id = "crafting", Name = "Crafting", Description = "Create items", Icon = "res://icons/hammer.png", UnlockRequirement = "items", SortOrder = 10 } },
        { "mounts", new GuideCategory { Id = "mounts", Name = "Mounts", Description = "Ride mounts", Icon = "res://icons/horse.png", UnlockRequirement = "pets", SortOrder = 11 } },
        { "achievements", new GuideCategory { Id = "achievements", Name = "Achievements", Description = "Track accomplishments", Icon = "res://icons/trophy.png", UnlockRequirement = "getting_started", SortOrder = 12 } },
        
        // 隐藏类别
        { "secrets", new GuideCategory { Id = "secrets", Name = "Secrets & Tips", Description = "Hidden tips", Icon = "res://icons/star.png", UnlockRequirement = "advanced_combat", SortOrder = 99 } }
    };

    // 指南配置
    public static Dictionary<string, GuideConfig> Guides = new Dictionary<string, GuideConfig>
    {
        // Getting Started
        { "welcome", new GuideConfig { Id = "welcome", Category = "getting_started", Title = "Welcome to ClawRPG", Content = "Welcome! This guide will help you get started.\n\nFirst, create your character using the Character Creation menu (Ctrl+Shift+C).\n\nThen, begin your adventure by exploring dungeons and fighting enemies.\n\nControls:\n- WASD: Move\n- Click: Attack\n- Space: Interact\n- ESC: Menu", Priority = 1, ReadTime = 60 } },
        { "basic_controls", new GuideConfig { Id = "basic_controls", Category = "getting_started", Title = "Basic Controls", Content = "Movement: WASD or Arrow Keys\nAttack: Left Click\nInteract: Space or E\nMenu: ESC\nInventory: I\nSkills: K\n\nQuick Cast: 1-9 number keys", Priority = 2, ReadTime = 45 } },
        
        // Combat Basics
        { "combat_basics", new GuideConfig { Id = "combat_basics", Category = "combat", Title = "Combat Basics", Content = "Combat in ClawRPG is turn-based with real-time elements.\n\nAttack: Click on enemies to attack\nDefense: Use Block to reduce damage\nSkills: Press K to open skill menu\nStatus Effects: Watch for poison, burn, etc.", Priority = 1, ReadTime = 60 } },
        { "elements", new GuideConfig { Id = "elements", Category = "combat", Title = "Elemental System", Content = "Elements: Fire, Ice, Lightning, Water, Holy, Dark, Physical, Nature, Wind\n\nSome enemies are weak to specific elements.\nUse the right element for bonus damage!\n\n- Fire beats Ice\n- Ice beats Fire\n- Holy beats Dark\n- Dark beats Holy", Priority = 2, ReadTime = 45 } },
        
        // Advanced Combat
        { "combos", new GuideConfig { Id = "combos", Category = "advanced_combat", Title = "Skill Combos", Content = "Chain skills together for combo bonuses!\n\nSome skills unlock when used after others.\nCheck your Skill Combo menu (C key) for available combos.\n\nCombos can deal extra damage or apply special effects.", Priority = 1, ReadTime = 60 } },
        { "stances", new GuideConfig { Id = "stances", Category = "advanced_combat", Title = "Combat Stances", Content = "Switch between combat stances for different playstyles:\n\n- Balanced: No bonuses or penalties\n- Offensive: +20% damage, -10% defense\n- Defensive: +20% defense, -10% damage\n- Swift: +15% speed, -5% defense\n- Fanatic: +30% damage, -25% defense\n- Guard: +40% defense, -20% damage", Priority = 2, ReadTime = 45 } },
        
        // Pets
        { "pet_basics", new GuideConfig { Id = "pet_basics", Category = "pets", Title = "Pet Basics", Content = "Pets fight alongside you and provide bonuses.\n\nGet your first pet from the Pet System (P key).\nPets gain experience and level up.\nDifferent pets have different abilities and element types.", Priority = 1, ReadTime = 60 } },
        { "pet_evolution", new GuideConfig { Id = "pet_evolution", Category = "pets", Title = "Pet Evolution", Content = "Pets can evolve into stronger forms!\n\nEvolution requires:\n- Reaching required level\n- Gathering evolution points\n- Having the right evolution type\n\nEvolving increases stats and unlocks new skills.", Priority = 2, ReadTime = 45 } },
        { "pet_bonding", new GuideConfig { Id = "pet_bonding", Category = "pets", Title = "Pet Friendship", Content = "Build friendship with your pet through interactions:\n\n- Pet: Increases happiness\n- Play: Fun activities\n- Feed: Give treats\n- Train: Improve skills\n\nHigher friendship = better bonuses in battle!", Priority = 3, ReadTime = 45 } },
        
        // Items
        { "item_rarity", new GuideConfig { Id = "item_rarity", Category = "items", Title = "Item Rarity", Content = "Items come in different rarities:\n\n- Common (Gray): Basic items\n- Uncommon (Green): Better stats\n- Rare (Blue): Good bonuses\n- Epic (Purple): Powerful effects\n- Legendary (Orange): Best items!\n\nHigher rarity = better stats and special abilities.", Priority = 1, ReadTime = 45 } },
        { "equipment_slots", new GuideConfig { Id = "equipment_slots", Category = "items", Title = "Equipment Slots", Content = "Equipment slots:\n\n- Weapon: Increases attack\n- Armor: Increases defense\n- Helmet: Various bonuses\n- Boots: Speed and defense\n- Gloves: Attack and crit\n- Ring: Various bonuses\n- Amulet: Special effects", Priority = 2, ReadTime = 45 } },
        
        // Dungeons
        { "dungeon_exploration", new GuideConfig { Id = "dungeon_exploration", Category = "dungeons", Title = "Dungeon Exploration", Content = "Explore procedural dungeons!\n\n- Each floor has different rooms\n- Fight enemies and collect loot\n- Find treasure rooms and shops\n- Reach the boss at the end\n\nDungeons get harder as you progress.", Priority = 1, ReadTime = 60 } },
        { "floor_progression", new GuideConfig { Id = "floor_progression", Category = "dungeons", Title = "Floor Progression", Content = "Each dungeon has multiple floors.\n\n- Clear all enemies to unlock stairs\n- Higher floors = more enemies and better loot\n- Boss appears every 5 floors\n- Difficulty scales with floor number", Priority = 2, ReadTime = 45 } },
        
        // Multiplayer
        { "multiplayer_modes", new GuideConfig { Id = "multiplayer_modes", Category = "multiplayer", Title = "Multiplayer Modes", Content = "Play with others!\n\nModes:\n- Co-op Dungeon: Team up\n- PvP Battle: Fight others\n- Racing: Race courses\n- Boss Rush: Kill bosses\n- Treasure Hunt: Find treasure\n- Survival: Last stand", Priority = 1, ReadTime = 45 } },
        
        // Guilds
        { "guild_basics", new GuideConfig { Id = "guild_basics", Category = "guilds", Title = "Guild Basics", Content = "Join or create a guild!\n\nBenefits:\n- Guild bank for sharing items\n- Guild quests for extra rewards\n- Guild wars for competition\n- Guild technology upgrades\n- Meet and play with friends!", Priority = 1, ReadTime = 60 } },
        
        // Economy
        { "trading", new GuideConfig { Id = "trading", Category = "economy", Title = "Trading Basics", Content = "Earn gold and trade items!\n\nWays to earn gold:\n- Sell items to shops\n- Complete quests\n- Defeat enemies\n- Trade with players\n\nSpend gold on:\n- New equipment\n- Consumables\n- Pets and mounts", Priority = 1, ReadTime = 45 } },
        
        // Crafting
        { "crafting_basics", new GuideConfig { Id = "crafting_basics", Category = "crafting", Title = "Crafting Basics", Content = "Create powerful items!\n\nOpen Crafting menu (C key).\nSelect a recipe and add materials.\nHigher rarity items need rarer materials.\n\nSome recipes unlock as you level up.", Priority = 1, ReadTime = 60 } },
        
        // Mounts
        { "mount_basics", new GuideConfig { Id = "mount_basics", Category = "mounts", Title = "Mount Basics", Content = "Ride mounts for speed and style!\n\nGet mounts from the Mount System (M key).\nMounts provide:\n- Movement speed bonus\n- Combat bonuses\n- Unique abilities\n\nTrain mounts to unlock more skills!", Priority = 1, ReadTime = 45 } },
        
        // Achievements
        { "achievements_system", new GuideConfig { Id = "achievements_system", Category = "achievements", Title = "Achievements", Content = "Track your accomplishments!\n\nOpen Achievements menu (Shift+H).\nComplete challenges to earn achievements.\nAchievements reward gold and experience.\n\nSome achievements unlock special content!", Priority = 1, ReadTime = 45 } },
        
        // Secrets
        { "hidden_secrets", new GuideConfig { Id = "hidden_secrets", Category = "secrets", Title = "Hidden Secrets", Content = "Tips and tricks:\n\n- Check every corner for secret rooms\n- Some NPCs have secret quests\n- Elemental combos create reactions\n- Pets can learn hidden abilities\n- Daily login rewards!\n\nThere's always more to discover!", Priority = 1, ReadTime = 45 } }
    };

    // 颜色配置
    public static string GetCategoryColor(string rarity)
    {
        return rarity switch
        {
            "getting_started" => "#4CAF50",
            "combat" => "#2196F3",
            "advanced_combat" => "#9C27B0",
            "pets" => "#FF9800",
            "items" => "#795548",
            "dungeons" => "#607D8B",
            "multiplayer" => "#E91E63",
            "guilds" => "#673AB7",
            "economy" => "#FFC107",
            "crafting" => "#00BCD4",
            "mounts" => "#8BC34A",
            "achievements" => "#FF5722",
            "secrets" => "#9E9E9E",
            _ => "#FFFFFF"
        };
    }
}

public class GuideCategory
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public bool IsDefaultUnlocked { get; set; }
    public string UnlockRequirement { get; set; }
    public int SortOrder { get; set; }
}

public class GuideConfig
{
    public string Id { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int Priority { get; set; }
    public int ReadTime { get; set; } // seconds estimated
}
