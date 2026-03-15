using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 地牢事件数据库。管理所有事件的定义、分类和属性配置。
/// </summary>
public partial class DungeonEventDatabase
{
    private Dictionary<string, Dictionary> _eventDatabase = new Dictionary<string, Dictionary>();
    
    // Event categories
    private List<string> _combatEvents = new List<string>();
    private List<string> _treasureEvents = new List<string>();
    private List<string> _mysteryEvents = new List<string>();
    private List<string> _hazardEvents = new List<string>();
    private List<string> _blessingEvents = new List<string>();
    private List<string> _curseEvents = new List<string>();
    private List<string> _npcEvents = new List<string>();
    private List<string> _explorationEvents = new List<string>();
    private List<string> _trapEvents = new List<string>();
    private List<string> _rewardEvents = new List<string>();
    
    public DungeonEventDatabase()
    {
        InitializeEventDatabase();
    }
    
    public void InitializeEventDatabase()
    {
        _eventDatabase = new Dictionary<string, Dictionary>();
        
        // Combat Events
        AddEventToDatabase("ambush", "Combat", "Uncommon", 
            "You walk into an ambush! A group of enemies has been waiting for you.",
            new Dictionary<string, object> {
                { "type", "combat" },
                { "enemy_count", 3 },
                { "enemy_type", "bandit" },
                { "difficulty", 1.2f },
                { "reward_gold", 50 },
                { "reward_exp", 30 },
                { "positive", false }
            });
            
        AddEventToDatabase("surprise_attack", "Combat", "Rare",
            "You catch enemies by surprise! You get the first strike.",
            new Dictionary<string, object> {
                { "type", "combat" },
                { "enemy_count", 2 },
                { "difficulty", 0.8f },
                { "reward_gold", 80 },
                { "reward_exp", 50 },
                { "positive", true }
            });
            
        AddEventToDatabase("reinforcements", "Combat", "Epic",
            "More enemies arrive as reinforcements!",
            new Dictionary<string, object> {
                { "type", "combat" },
                { "enemy_count", 5 },
                { "difficulty", 1.5f },
                { "reward_gold", 150 },
                { "reward_exp", 100 },
                { "positive", false }
            });
        
        // Treasure Events
        AddEventToDatabase("hidden_chest", "Treasure", "Common",
            "You find a hidden chest in the corner of the room!",
            new Dictionary<string, object> {
                { "type", "treasure" },
                { "gold_min", 20 },
                { "gold_max", 50 },
                { "item_chance", 0.3f },
                { "positive", true }
            });
            
        AddEventToDatabase("treasure_room", "Treasure", "Epic",
            "You discover a treasure room filled with riches!",
            new Dictionary<string, object> {
                { "type", "treasure" },
                { "gold_min", 200 },
                { "gold_max", 500 },
                { "item_chance", 0.8f },
                { "item_count_min", 2 },
                { "item_count_max", 4 },
                { "positive", true }
            });
            
        AddEventToDatabase("empty_chest", "Treasure", "Common",
            "You find a chest, but it's empty...",
            new Dictionary<string, object> {
                { "type", "treasure" },
                { "gold_min", 0 },
                { "gold_max", 10 },
                { "positive", false }
            });
        
        // Mystery Events
        AddEventToDatabase("strange_orb", "Mystery", "Rare",
            "You find a strange orb pulsing with mysterious energy.",
            new Dictionary<string, object> {
                { "type", "choice" },
                { "choice_a", "Touch the orb" },
                { "choice_b", "Leave it alone" },
                { "choice_a_effect", "random" },
                { "choice_b_effect", "safe" }
            });
            
        AddEventToDatabase("ancient_scroll", "Mystery", "Uncommon",
            "You discover an ancient scroll with unreadable text.",
            new Dictionary<string, object> {
                { "type", "scroll" },
                { "exp_bonus", 25 },
                { "positive", true }
            });
            
        AddEventToDatabase("cursed_door", "Mystery", "Rare",
            "A door with ominous symbols blocks your path.",
            new Dictionary<string, object> {
                { "type", "choice" },
                { "choice_a", "Open the door" },
                { "choice_b", "Find another way" },
                { "choice_a_effect", "curse" },
                { "choice_b_effect", "safe" }
            });
        
        // Hazard Events
        AddEventToDatabase("falling_rocks", "Hazard", "Uncommon",
            "Rocks start falling from the ceiling!",
            new Dictionary<string, object> {
                { "type", "dodge" },
                { "damage_min", 5 },
                { "damage_max", 15 },
                { "difficulty", 0.6f },
                { "positive", false }
            });
            
        AddEventToDatabase("flooding", "Hazard", "Rare",
            "Water starts flooding the room!",
            new Dictionary<string, object> {
                { "type", "escape" },
                { "time_limit", 10f },
                { "damage_per_second", 3 },
                { "positive", false }
            });
            
        AddEventToDatabase("collapsing_floor", "Hazard", "Epic",
            "The floor begins to collapse!",
            new Dictionary<string, object> {
                { "type", "escape" },
                { "time_limit", 8f },
                { "damage_max", 30 },
                { "reward_gold", 100 },
                { "positive", false }
            });
        
        // Blessing Events
        AddEventToDatabase("healing_fountain", "Blessing", "Common",
            "You find a fountain with glowing water that restores health.",
            new Dictionary<string, object> {
                { "type", "heal" },
                { "heal_amount", 30 },
                { "positive", true }
            });
            
        AddEventToDatabase("blessing_of_light", "Blessing", "Rare",
            "A beam of light descends upon you, granting divine blessing.",
            new Dictionary<string, object> {
                { "type", "buff" },
                { "buff_duration", 180 },
                { "attack_bonus", 1.2f },
                { "defense_bonus", 1.2f },
                { "positive", true }
            });
            
        AddEventToDatabase("treasure_blessing", "Blessing", "Epic",
            "The ancient god of wealth smiles upon you!",
            new Dictionary<string, object> {
                { "type", "fortune" },
                { "gold_multiplier", 2.0f },
                { "drop_bonus", 1.5f },
                { "duration", 300 },
                { "positive", true }
            });
        
        // Curse Events
        AddEventToDatabase("cursed_trap", "Curse", "Uncommon",
            "You trigger a curse trap! Dark energy surrounds you.",
            new Dictionary<string, object> {
                { "type", "debuff" },
                { "debuff_duration", 120 },
                { "attack_penalty", 0.8f },
                { "defense_penalty", 0.8f },
                { "positive", false }
            });
            
        AddEventToDatabase("shadow_curse", "Curse", "Rare",
            "A shadow curse binds your soul!",
            new Dictionary<string, object> {
                { "type", "curse" },
                { "curse_duration", 300 },
                { "gold_loss_rate", 0.5f },
                { "exp_loss_rate", 0.5f },
                { "positive", false }
            });
            
        AddEventToDatabase("monster_curse", "Curse", "Epic",
            "You are cursed to attract more enemies!",
            new Dictionary<string, object> {
                { "type", "attraction" },
                { "duration", 600 },
                { "enemy_spawn_rate", 2.0f },
                { "positive", false }
            });
        
        // NPC Events
        AddEventToDatabase("wandering_merchant", "NPC", "Uncommon",
            "A wandering merchant offers goods for sale.",
            new Dictionary<string, object> {
                { "type", "shop" },
                { "items", 3 },
                { "discount", 0.8f },
                { "positive", true }
            });
            
        AddEventToDatabase("mysterious_stranger", "NPC", "Rare",
            "A mysterious stranger offers to help you.",
            new Dictionary<string, object> {
                { "type", "help" },
                { "help_type", "random" },
                { "positive", true }
            });
            
        AddEventToDatabase("injured_adventurer", "NPC", "Common",
            "You find an injured adventurer who needs help.",
            new Dictionary<string, object> {
                { "type", "rescue" },
                { "gold_cost", 20 },
                { "reward_exp", 40 },
                { "reward_item", true },
                { "positive", true }
            });
        
        // Exploration Events
        AddEventToDatabase("secret_passage", "Exploration", "Uncommon",
            "You discover a secret passage leading to a hidden area!",
            new Dictionary<string, object> {
                { "type", "secret" },
                { "extra_rooms", 2 },
                { "positive", true }
            });
            
        AddEventToDatabase("dead_end", "Exploration", "Common",
            "You've reached a dead end with nothing but rubble.",
            new Dictionary<string, object> {
                { "type", "dead_end" },
                { "positive", false }
            });
            
        AddEventToDatabase("shortcut", "Exploration", "Rare",
            "You find a shortcut that saves time and effort!",
            new Dictionary<string, object> {
                { "type", "shortcut" },
                { "floor_skip", 1 },
                { "positive", true }
            });
        
        // Trap Events
        AddEventToDatabase("poison_trap", "Trap", "Common",
            "You step on a pressure plate! Poison gas fills the room.",
            new Dictionary<string, object> {
                { "type", "poison" },
                { "damage", 10 },
                { "dot_duration", 30 },
                { "positive", false }
            });
            
        AddEventToDatabase("spike_trap", "Trap", "Common",
            "Spikes shoot up from the floor!",
            new Dictionary<string, object> {
                { "type", "damage" },
                { "damage_min", 15 },
                { "damage_max", 25 },
                { "positive", false }
            });
            
        AddEventToDatabase("teleport_trap", "Trap", "Rare",
            "A teleportation trap activates!",
            new Dictionary<string, object> {
                { "type", "teleport" },
                { "destination", "random" },
                { "positive", "neutral" }
            });
        
        // Reward Events
        AddEventToDatabase("daily_bonus", "Reward", "Uncommon",
            "Your daily login bonus! The dungeon recognizes your dedication.",
            new Dictionary<string, object> {
                { "type", "bonus" },
                { "gold", 100 },
                { "exp", 50 },
                { "positive", true }
            });
            
        AddEventToDatabase("achievement_reward", "Reward", "Rare",
            "You've unlocked a dungeon achievement!",
            new Dictionary<string, object> {
                { "type", "achievement" },
                { "stat_bonus", 1.1f },
                { "positive", true }
            });
            
        AddEventToDatabase("milestone_reward", "Reward", "Legendary",
            "A major milestone! The dungeon itself acknowledges your prowess!",
            new Dictionary<string, object> {
                { "type", "milestone" },
                { "gold", 500 },
                { "exp", 300 },
                { "item_rarity", "Epic" },
                { "positive", true }
            });
        
        // Categorize events
        _combatEvents = new List<string> { "ambush", "surprise_attack", "reinforcements" };
        _treasureEvents = new List<string> { "hidden_chest", "treasure_room", "empty_chest" };
        _mysteryEvents = new List<string> { "strange_orb", "ancient_scroll", "cursed_door" };
        _hazardEvents = new List<string> { "falling_rocks", "flooding", "collapsing_floor" };
        _blessingEvents = new List<string> { "healing_fountain", "blessing_of_light", "treasure_blessing" };
        _curseEvents = new List<string> { "cursed_trap", "shadow_curse", "monster_curse" };
        _npcEvents = new List<string> { "wandering_merchant", "mysterious_stranger", "injured_adventurer" };
        _explorationEvents = new List<string> { "secret_passage", "dead_end", "shortcut" };
        _trapEvents = new List<string> { "poison_trap", "spike_trap", "teleport_trap" };
        _rewardEvents = new List<string> { "daily_bonus", "achievement_reward", "milestone_reward" };
    }
    
    private void AddEventToDatabase(string id, string category, string rarity, string description, Dictionary<string, object> properties)
    {
        var eventData = new Dictionary<string, object>
        {
            { "id", id },
            { "category", category },
            { "rarity", rarity },
            { "description", description }
        };
        
        foreach (var prop in properties)
        {
            eventData[prop.Key] = prop.Value;
        }
        
        _eventDatabase[id] = eventData;
    }
    
    public Dictionary<string, Dictionary> GetEventDatabase()
    {
        return _eventDatabase;
    }
    
    public Dictionary<string, object> GetEvent(string eventId)
    {
        if (_eventDatabase.TryGetValue(eventId, out var eventData))
            return eventData;
        return null;
    }
    
    public List<string> GetCombatEvents() => _combatEvents;
    public List<string> GetTreasureEvents() => _treasureEvents;
    public List<string> GetMysteryEvents() => _mysteryEvents;
    public List<string> GetHazardEvents() => _hazardEvents;
    public List<string> GetBlessingEvents() => _blessingEvents;
    public List<string> GetCurseEvents() => _curseEvents;
    public List<string> GetNpcEvents() => _npcEvents;
    public List<string> GetExplorationEvents() => _explorationEvents;
    public List<string> GetTrapEvents() => _trapEvents;
    public List<string> GetRewardEvents() => _rewardEvents;
    
    public List<string> GetEventListForCategory(string category)
    {
        return category switch
        {
            "Combat" => _combatEvents,
            "Treasure" => _treasureEvents,
            "Mystery" => _mysteryEvents,
            "Hazard" => _hazardEvents,
            "Blessing" => _blessingEvents,
            "Curse" => _curseEvents,
            "NPC" => _npcEvents,
            "Exploration" => _explorationEvents,
            "Trap" => _trapEvents,
            "Reward" => _rewardEvents,
            _ => new List<string>()
        };
    }
}
