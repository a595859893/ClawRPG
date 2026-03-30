using Godot;
using System;
using System.Collections.Generic;

public class CardCollectionDatabase : BaseSystem
{
    // All card definitions
    public Dictionary<string, CardDefinition> Cards = new Dictionary<string, CardDefinition>();
    
    // Card packs available for purchase
    public Dictionary<string, CardPack> Packs = new Dictionary<string, CardPack>();
    
    // Categories
    public string[] Categories = { "Attack", "Skill", "Power", "Defense", "Special" };
    
    // Rarity names and colors
    public Dictionary<string, Color> RarityColors = new Dictionary<string, Color>
    {
        { "Common", new Color(0.7f, 0.7f, 0.7f) },      // Gray
        { "Uncommon", new Color(0.2f, 0.8f, 0.2f) },    // Green
        { "Rare", new Color(0.2f, 0.4f, 0.9f) },        // Blue
        { "Epic", new Color(0.6f, 0.2f, 0.8f) },        // Purple
        { "Legendary", new Color(1.0f, 0.6f, 0.0f) }    // Orange
    };
    
    public override void _Ready()
    {
        InitializeCards();
        InitializePacks();
    }
    
    private void InitializeCards()
    {
        // Attack cards (20+)
        AddCard("strike", "Strike", "Attack", "Common", 1, 6, "Deal 6 damage.");
        AddCard("heavy_strike", "Heavy Strike", "Attack", "Uncommon", 2, 12, "Deal 12 damage.");
        AddCard("slash", "Slash", "Attack", "Common", 1, 5, "Deal 5 damage.");
        AddCard("double_strike", "Double Strike", "Attack", "Rare", 3, 10, "Deal 5 damage twice.");
        AddCard("combo", "Combo", "Attack", "Epic", 4, 15, "Deal 15 damage. Draw 1 card.");
        AddCard("blade_storm", "Blade Storm", "Attack", "Legendary", 5, 25, "Deal 25 damage to all enemies.");
        AddCard("quick_stab", "Quick Stab", "Attack", "Uncommon", 2, 8, "Deal 8 damage. Draw 1 card.");
        AddCard("piercing_shot", "Piercing Shot", "Attack", "Rare", 3, 14, "Deal 14 damage. Ignore defense.");
        
        // Skill cards (15+)
        AddCard("block", "Block", "Skill", "Common", 1, 0, "Gain 5 block.");
        AddCard("parry", "Parry", "Skill", "Uncommon", 2, 0, "Gain 8 block. Draw 1 card.");
        AddCard("dodge", "Dodge", "Skill", "Common", 1, 0, "Gain 5 evasion next turn.");
        AddCard("shield_wall", "Shield Wall", "Skill", "Rare", 3, 0, "Gain 15 block.");
        AddCard("counter", "Counter", "Skill", "Epic", 4, 0, "Gain 10 block. Deal 5 damage to attacker.");
        AddCard("fortify", "Fortify", "Skill", "Rare", 3, 0, "Gain 12 block. Gain 5 strength.");
        AddCard("evasive", "Evasive Maneuvers", "Skill", "Legendary", 5, 0, "Gain 20 block. Draw 2 cards.");
        AddCard("iron_skin", "Iron Skin", "Skill", "Uncommon", 2, 0, "Gain 10 block. Gain 3 dexterity.");
        
        // Power cards (15+)
        AddCard("strength", "Strength", "Power", "Common", 1, 0, "Gain 1 strength.");
        AddCard("power_surge", "Power Surge", "Power", "Uncommon", 2, 0, "Gain 3 strength.");
        AddCard("focus", "Focus", "Power", "Common", 1, 0, "Draw 2 cards.");
        AddCard("inner_power", "Inner Power", "Power", "Rare", 3, 0, "Gain 5 strength. Gain 5 dexterity.");
        AddCard("dragon_form", "Dragon Form", "Power", "Legendary", 5, 0, "Gain 8 strength. Gain 8 dexterity. Gain 8 vitality.");
        AddCard("berserker", "Berserker", "Power", "Epic", 4, 0, "Gain 6 strength. Lose 5 HP.");
        AddCard("channel", "Channel", "Power", "Rare", 3, 0, "Gain 4 mana. Draw 1 card.");
        AddCard("enrage", "Enrage", "Power", "Epic", 4, 0, "Gain 7 strength. Take 3 damage.");
        
        // Defense cards (10+)
        AddCard("heal", "Heal", "Defense", "Common", 1, 0, "Restore 5 HP.");
        AddCard("recover", "Recover", "Defense", "Uncommon", 2, 0, "Restore 10 HP.");
        AddCard("regeneration", "Regeneration", "Defense", "Rare", 3, 0, "Restore 8 HP for 3 turns.");
        AddCard("barrier", "Barrier", "Defense", "Epic", 4, 0, "Gain 20 shield. Restore 5 HP.");
        AddCard("holy_light", "Holy Light", "Defense", "Legendary", 5, 0, "Restore 25 HP. Remove all debuffs.");
        AddCard("second_wind", "Second Wind", "Defense", "Rare", 3, 0, "Restore 15 HP. Lose 5 block.");
        AddCard("life_drain", "Life Drain", "Defense", "Epic", 4, 0, "Deal 8 damage. Restore 8 HP.");
        
        // Special cards (10+)
        AddCard("wild_card", "Wild Card", "Special", "Rare", 3, 0, "Draw 3 cards. Discard 1.");
        AddCard("miracle", "Miracle", "Special", "Legendary", 5, 0, "Choose one: Deal 20 damage OR Restore 20 HP OR Gain 20 block.");
        AddCard("fate", "Fate", "Special", "Epic", 4, 0, "Shuffle your draw pile. Draw 5 cards.");
        AddCard("time_warp", "Time Warp", "Special", "Legendary", 5, 0, "Gain 2 energy. Take an extra turn.");
        AddCard("sacrifice", "Sacrifice", "Special", "Epic", 4, 0, "Lose 10 HP. Gain 5 of each stat.");
    }
    
    private void AddCard(string id, string name, string category, string rarity, int cost, int baseDamage, string description)
    {
        var card = new CardDefinition
        {
            Id = id,
            Name = name,
            Category = category,
            Rarity = rarity,
            Cost = cost,
            BaseDamage = baseDamage,
            Description = description
        };
        Cards[id] = card;
    }
    
    private void InitializePacks()
    {
        // Starter Pack - Basic cards
        Packs["starter_pack"] = new CardPack
        {
            Id = "starter_pack",
            Name = "Starter Pack",
            Price = 100,
            Cards = new List<PackCardEntry>
            {
                new PackCardEntry("strike", 3),
                new PackCardEntry("slash", 3),
                new PackCardEntry("block", 3),
                new PackCardEntry("heal", 2),
                new PackCardEntry("strength", 2),
                new PackCardEntry("focus", 2)
            }
        };
        
        // Attack Pack - Attack-focused cards
        Packs["attack_pack"] = new CardPack
        {
            Id = "attack_pack",
            Name = "Attack Pack",
            Price = 250,
            Cards = new List<PackCardEntry>
            {
                new PackCardEntry("strike", 2),
                new PackCardEntry("heavy_strike", 2),
                new PackCardEntry("slash", 2),
                new PackCardEntry("double_strike", 1),
                new PackCardEntry("quick_stab", 2),
                new PackCardEntry("piercing_shot", 1)
            }
        };
        
        // Defense Pack - Defense-focused cards
        Packs["defense_pack"] = new CardPack
        {
            Id = "defense_pack",
            Name = "Defense Pack",
            Price = 250,
            Cards = new List<PackCardEntry>
            {
                new PackCardEntry("block", 2),
                new PackCardEntry("parry", 2),
                new PackCardEntry("dodge", 2),
                new PackCardEntry("shield_wall", 1),
                new PackCardEntry("heal", 2),
                new PackCardEntry("recover", 1)
            }
        };
        
        // Power Pack - Power-focused cards
        Packs["power_pack"] = new CardPack
        {
            Id = "power_pack",
            Name = "Power Pack",
            Price = 350,
            Cards = new List<PackCardEntry>
            {
                new PackCardEntry("strength", 2),
                new PackCardEntry("power_surge", 2),
                new PackCardEntry("focus", 2),
                new PackCardEntry("inner_power", 1),
                new PackCardEntry("berserker", 1),
                new PackCardEntry("channel", 1)
            }
        };
        
        // Epic Pack - Epic and Legendary cards
        Packs["epic_pack"] = new CardPack
        {
            Id = "epic_pack",
            Name = "Epic Pack",
            Price = 500,
            Cards = new List<PackCardEntry>
            {
                new PackCardEntry("combo", 1),
                new PackCardEntry("counter", 1),
                new PackCardEntry("evasive", 1),
                new PackCardEntry("barrier", 1),
                new PackCardEntry("dragon_form", 1),
                new PackCardEntry("wild_card", 1),
                new PackCardEntry("fate", 1)
            }
        };
        
        // Legendary Pack - Only Legendary cards
        Packs["legendary_pack"] = new CardPack
        {
            Id = "legendary_pack",
            Name = "Legendary Pack",
            Price = 1000,
            Cards = new List<PackCardEntry>
            {
                new PackCardEntry("blade_storm", 1),
                new PackCardEntry("dragon_form", 1),
                new PackCardEntry("holy_light", 1),
                new PackCardEntry("miracle", 1),
                new PackCardEntry("time_warp", 1),
                new PackCardEntry("sacrifice", 1)
            }
        };
    }
    
    public CardDefinition GetCard(string cardId)
    {
        if (Cards.ContainsKey(cardId))
            return Cards[cardId];
        return null;
    }
    
    public CardPack GetPack(string packId)
    {
        if (Packs.ContainsKey(packId))
            return Packs[packId];
        return null;
    }
    
    public Color GetRarityColor(string rarity)
    {
        if (RarityColors.ContainsKey(rarity))
            return RarityColors[rarity];
        return Colors.White;
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        // CardCollectionDatabase 是静态配置数据，不需要持久化
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        // CardCollectionDatabase 是静态配置数据，不需要持久化
    }
}

public class CardDefinition
{
    public string Id;
    public string Name;
    public string Category;
    public string Rarity;
    public int Cost;
    public int BaseDamage;
    public string Description;
}

public class CardPack
{
    public string Id;
    public string Name;
    public int Price;
    public List<PackCardEntry> Cards = new List<PackCardEntry>();
}

public class PackCardEntry
{
    public string CardId;
    public int Weight;
    
    public PackCardEntry(string cardId, int weight)
    {
        CardId = cardId;
        Weight = weight;
    }
}
