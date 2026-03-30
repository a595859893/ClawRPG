using Godot;
using System;
using System.Collections.Generic;

public class CharacterCreationDatabase : BaseSystem
{
    // Class configurations
    public Dictionary<string, Dictionary<string, object>> Classes { get; set; } = new Dictionary<string, Dictionary<string, object>>
    {
        { "Warrior", new Dictionary<string, object>
            {
                { "name", "Warrior" },
                { "description", "A battle-hardened fighter skilled in all weapons" },
                { "icon", "⚔️" },
                { "base_hp", 120 },
                { "base_attack", 15 },
                { "base_defense", 12 },
                { "base_magic", 2 },
                { "base_speed", 8 },
                { "recommended_attribute", "Strength" },
                { "starting_weapon", "Iron Sword" },
                { "starting_armor", "Leather Armor" }
            }
        },
        { "Mage", new Dictionary<string, object>
            {
                { "name", "Mage" },
                { "description", "A master of arcane arts and elemental magic" },
                { "icon", "🔮" },
                { "base_hp", 80 },
                { "base_attack", 5 },
                { "base_defense", 6 },
                { "base_magic", 20 },
                { "base_speed", 10 },
                { "recommended_attribute", "Intelligence" },
                { "starting_weapon", "Oak Staff" },
                { "starting_armor", "Mage Robe" }
            }
        },
        { "Rogue", new Dictionary<string, object>
            {
                { "name", "Rogue" },
                { "description", "A cunning assassin who strikes from the shadows" },
                { "icon", "🗡️" },
                { "base_hp", 90 },
                { "base_attack", 14 },
                { "base_defense", 8 },
                { "base_magic", 5 },
                { "base_speed", 15 },
                { "recommended_attribute", "Agility" },
                { "starting_weapon", "Dagger" },
                { "starting_armor", "Shadow Cloak" }
            }
        },
        { "Ranger", new Dictionary<string, object>
            {
                { "name", "Ranger" },
                { "description", "A skilled marksman and survival expert" },
                { "icon", "🏹" },
                { "base_hp", 100 },
                { "base_attack", 13 },
                { "base_defense", 9 },
                { "base_magic", 4 },
                { "base_speed", 12 },
                { "recommended_attribute", "Agility" },
                { "starting_weapon", "Longbow" },
                { "starting_armor", "Ranger Vest" }
            }
        },
        { "Paladin", new Dictionary<string, object>
            {
                { "name", "Paladin" },
                { "description", "A holy warrior dedicated to justice and protection" },
                { "icon", "🛡️" },
                { "base_hp", 130 },
                { "base_attack", 12 },
                { "base_defense", 15 },
                { "base_magic", 10 },
                { "base_speed", 6 },
                { "recommended_attribute", "Vitality" },
                { "starting_weapon", "Holy Sword" },
                { "starting_armor", "Plate Armor" }
            }
        },
        { "Necromancer", new Dictionary<string, object>
            {
                { "name", "Necromancer" },
                { "description", "A dark mage who commands the forces of death" },
                { "icon", "💀" },
                { "base_hp", 85 },
                { "base_attack", 6 },
                { "base_defense", 7 },
                { "base_magic", 18 },
                { "base_speed", 9 },
                { "recommended_attribute", "Intelligence" },
                { "starting_weapon", "Bone Staff" },
                { "starting_armor", "Dark Robe" }
            }
        },
        { "Druid", new Dictionary<string, object>
            {
                { "name", "Druid" },
                { "description", "A nature mage who harnesses the power of the wild" },
                { "icon", "🌿" },
                { "base_hp", 95 },
                { "base_attack", 8 },
                { "base_defense", 10 },
                { "base_magic", 15 },
                { "base_speed", 11 },
                { "recommended_attribute", "Intelligence" },
                { "starting_weapon", "Wooden Staff" },
                { "starting_armor", "Druid Vestment" }
            }
        },
        { "Bard", new Dictionary<string, object>
            {
                { "name", "Bard" },
                { "description", "A charismatic performer with magical songs" },
                { "icon", "🎵" },
                { "base_hp", 88 },
                { "base_attack", 10 },
                { "base_defense", 8 },
                { "base_magic", 14 },
                { "base_speed", 13 },
                { "recommended_attribute", "Luck" },
                { "starting_weapon", "Lute" },
                { "starting_armor", "Silk Robe" }
            }
        }
    };
    
    // Background stories
    public Dictionary<string, Dictionary<string, object>> Backgrounds { get; set; } = new Dictionary<string, Dictionary<string, object>>
    {
        { "Commoner", new Dictionary<string, object>
            {
                { "name", "Commoner" },
                { "description", "You grew up as an ordinary citizen" },
                { "bonuses", new Dictionary<string, int> { { "gold", 50 }, { "charisma", 2 } } },
                { "starting_items", new string[] { "Bread", "Water Flask" } }
            }
        },
        { "Noble", new Dictionary<string, object>
            {
                { "name", "Noble" },
                { "description", "You were born into wealth and privilege" },
                { "bonuses", new Dictionary<string, int> { { "gold", 200 }, { "charisma", 5 } } },
                { "starting_items", new string[] { "Noble's Letter", "Gold Ring" } }
            }
        },
        { "Soldier", new Dictionary<string, object>
            {
                { "name", "Soldier" },
                { "description", "You served in the army" },
                { "bonuses", new Dictionary<string, int> { { "defense", 3 }, { "attack", 2 } } },
                { "starting_items", new string[] { "Military Medal", "Rations" } }
            }
        },
        { "Scholar", new Dictionary<string, object>
            {
                { "name", "Scholar" },
                { "description", "You dedicated your life to knowledge" },
                { "bonuses", new Dictionary<string, int> { { "intelligence", 4 }, { "magic", 2 } } },
                { "starting_items", new string[] { "Ancient Book", "Reading Glasses" } }
            }
        },
        { "Outlaw", new Dictionary<string, object>
            {
                { "name", "Outlaw" },
                { "description", "You lived outside the law" },
                { "bonuses", new Dictionary<string, int> { { "agility", 4 }, { "luck", 3 } } },
                { "starting_items", new string[] { "Lockpick", "Thief's Mask" } }
            }
        },
        { "Priest", new Dictionary<string, object>
            {
                { "name", "Priest" },
                { "description", "You served a divine entity" },
                { "bonuses", new Dictionary<string, int> { { "vitality", 3 }, { "magic", 4 } } },
                { "starting_items", new string[] { "Holy Symbol", "Healing Potion" } }
            }
        },
        { "Mercenary", new Dictionary<string, object>
            {
                { "name", "Mercenary" },
                { "description", "You fought for coin" },
                { "bonuses", new Dictionary<string, int> { { "attack", 4 }, { "defense", 2 } } },
                { "starting_items", new string[] { "Mercenary Contract", "Bandage" } }
            }
        },
        { "Wanderer", new Dictionary<string, object>
            {
                { "name", "Wanderer" },
                { "description", "You traveled the lands alone" },
                { "bonuses", new Dictionary<string, int> { { "speed", 5 }, { "luck", 2 } } },
                { "starting_items", new string[] { "Traveler's Map", "Compass" } }
            }
        }
    };
    
    // Hair styles
    public string[] HairStyles { get; set; } = {
        "Short",
        "Long",
        "Curly",
        "Bald",
        "Mohawk",
        "Braided",
        "Ponytail",
        "Spiky"
    };
    
    // Skin colors
    public string[] SkinColors { get; set; } = {
        "Pale",
        "Fair",
        "Tan",
        "Brown",
        "Dark",
        "Exotic"
    };
    
    // Eye colors
    public string[] EyeColors { get; set; } = {
        "Blue",
        "Green",
        "Brown",
        "Black",
        "Gray",
        "Violet",
        "Gold",
        "Red"
    };
    
    public override void _Ready()
    {
        GD.Print("[CharacterCreationDatabase] Database initialized with " + Classes.Count + " classes and " + Backgrounds.Count + " backgrounds");
    }
    
    public string[] GetClassNames()
    {
        var names = new string[Classes.Count];
        int i = 0;
        foreach (var k in Classes.Keys)
        {
            names[i++] = k;
        }
        return names;
    }
    
    public string[] GetBackgroundNames()
    {
        var names = new string[Backgrounds.Count];
        int i = 0;
        foreach (var k in Backgrounds.Keys)
        {
            names[i++] = k;
        }
        return names;
    }
    
    public Dictionary<string, object> GetClassData(string className)
    {
        if (Classes.ContainsKey(className))
            return Classes[className];
        return null;
    }
    
    public Dictionary<string, object> GetBackgroundData(string backgroundName)
    {
        if (Backgrounds.ContainsKey(backgroundName))
            return Backgrounds[backgroundName];
        return null;
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        // CharacterCreationDatabase 是静态配置数据，不需要持久化
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        // CharacterCreationDatabase 是静态配置数据，不需要持久化
    }
}
