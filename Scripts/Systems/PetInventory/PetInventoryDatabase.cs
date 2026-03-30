using Godot;
using System;
using System.Collections.Generic;

public class PetInventoryDatabase : BaseSystem {
    public Dictionary<string, Dictionary<string, object>> ItemTemplates { get; set; }
    public Dictionary<string, List<string>> CategoryItems { get; set; }
    public Dictionary<string, string> RarityColors { get; set; }
    public Dictionary<string, float> RarityValueMultiplier { get; set; }
    
    public override void _Ready() {
        base._Ready();
        InitializeDatabase();
    }
    
    public void InitializeDatabase() {
        ItemTemplates = new Dictionary<string, Dictionary<string, object>>();
        CategoryItems = new Dictionary<string, List<string>>();
        RarityColors = new Dictionary<string, string>();
        RarityValueMultiplier = new Dictionary<string, float>();
        
        // Initialize rarity colors
        RarityColors["Common"] = "#FFFFFF";
        RarityColors["Uncommon"] = "#1EFF00";
        RarityColors["Rare"] = "#0070FF";
        RarityColors["Epic"] = "#A335EE";
        RarityColors["Legendary"] = "#FF8000";
        
        // Initialize rarity value multipliers
        RarityValueMultiplier["Common"] = 1.0f;
        RarityValueMultiplier["Uncommon"] = 2.0f;
        RarityValueMultiplier["Rare"] = 5.0f;
        RarityValueMultiplier["Epic"] = 15.0f;
        RarityValueMultiplier["Legendary"] = 50.0f;
        
        // Initialize category items
        InitializeConsumables();
        InitializeEquipment();
        InitializeMaterials();
        InitializeSpecial();
    }
    
    private void InitializeConsumables() {
        CategoryItems["Consumable"] = new List<string>();
        
        AddItemTemplate("health_potion", "Health Potion", "Restores 50 HP", "Consumable", "Common", 50,
            new Dictionary<string, float> { { "heal", 50 } }, "InstantHeal");
        AddItemTemplate("health_potion_large", "Large Health Potion", "Restores 150 HP", "Consumable", "Uncommon", 150,
            new Dictionary<string, float> { { "heal", 150 } }, "InstantHeal");
        AddItemTemplate("mana_potion", "Mana Potion", "Restores 30 MP", "Consumable", "Common", 40,
            new Dictionary<string, float> { { "mana", 30 } }, "InstantHeal");
        AddItemTemplate("strength_buff", "Strength Elixir", "Increases attack by 20% for 5 minutes", "Consumable", "Rare", 200,
            new Dictionary<string, float> { { "attack", 20 } }, "Buff");
        AddItemTemplate("defense_buff", "Defense Elixir", "Increases defense by 20% for 5 minutes", "Consumable", "Rare", 200,
            new Dictionary<string, float> { { "defense", 20 } }, "Buff");
        AddItemTemplate("speed_buff", "Speed Elixir", "Increases speed by 15% for 5 minutes", "Consumable", "Rare", 180,
            new Dictionary<string, float> { { "speed", 15 } }, "Buff");
        AddItemTemplate("critical_buff", "Critical Elixir", "Increases critical rate by 10% for 5 minutes", "Consumable", "Epic", 350,
            new Dictionary<string, float> { { "critical", 10 } }, "Buff");
        AddItemTemplate("exp_boost", "Experience Scroll", "Doubles EXP gained for 10 minutes", "Consumable", "Epic", 500,
            new Dictionary<string, float> { { "exp_boost", 2.0f } }, "Boost");
        AddItemTemplate("luck_charm", "Lucky Charm", "Increases drop rate by 50% for 5 minutes", "Consumable", "Legendary", 800,
            new Dictionary<string, float> { { "drop_rate", 50 } }, "Boost");
    }
    
    private void InitializeEquipment() {
        CategoryItems["Equipment"] = new List<string>();
        
        AddItemTemplate("pet_collar_common", "Simple Collar", "A basic collar for pets", "Equipment", "Common", 25,
            new Dictionary<string, float> { { "attack", 2 }, { "defense", 2 } }, "");
        AddItemTemplate("pet_collar_rare", "Steel Collar", "A sturdy steel collar", "Equipment", "Rare", 150,
            new Dictionary<string, float> { { "attack", 8 }, { "defense", 8 }, { "health", 50 } }, "");
        AddItemTemplate("pet_collar_epic", "Dragon Scale Collar", "A collar made from dragon scales", "Equipment", "Epic", 400,
            new Dictionary<string, float> { { "attack", 15 }, { "defense", 15 }, { "health", 100 }, { "critical", 5 } }, "");
        AddItemTemplate("pet_collar_legendary", "Celestial Collar", "A collar blessed by the heavens", "Equipment", "Legendary", 1000,
            new Dictionary<string, float> { { "attack", 25 }, { "defense", 25 }, { "health", 200 }, { "critical", 10 }, { "speed", 10 } }, "LifeSteal");
    }
    
    private void InitializeMaterials() {
        CategoryItems["Material"] = new List<string>();
        
        AddItemTemplate("pet_food_basic", "Basic Pet Food", "Simple food for pets", "Material", "Common", 10,
            new Dictionary<string, float> { { "happiness", 10 } }, "");
        AddItemTemplate("pet_food_premium", "Premium Pet Food", "Delicious food pets love", "Material", "Uncommon", 50,
            new Dictionary<string, float> { { "happiness", 30 }, { "affection", 10 } }, "");
        AddItemTemplate("pet_food_gourmet", "Gourmet Pet Food", "Exquisite food for beloved pets", "Material", "Rare", 150,
            new Dictionary<string, float> { { "happiness", 50 }, { "affection", 25 }, { "exp", 20 } }, "");
        AddItemTemplate("evolution_stone", "Evolution Stone", "A mysterious stone that triggers evolution", "Material", "Epic", 300,
            new Dictionary<string, float> { { "evolution_progress", 50 } }, "");
        AddItemTemplate("evolution_orb", "Evolution Orb", "A powerful orb that greatly accelerates evolution", "Material", "Legendary", 800,
            new Dictionary<string, float> { { "evolution_progress", 100 } }, "");
    }
    
    private void InitializeSpecial() {
        CategoryItems["Special"] = new List<string>();
        
        AddItemTemplate("pet_ticket", "Pet Summon Ticket", "A ticket to summon a random pet", "Special", "Rare", 250,
            new Dictionary<string, float> { }, "SummonPet");
        AddItemTemplate("pet_egg_rare", "Rare Pet Egg", "An egg that hatches into a rare pet", "Special", "Epic", 500,
            new Dictionary<string, float> { }, "HatchPet");
        AddItemTemplate("pet_egg_legendary", "Legendary Pet Egg", "An egg that hatches into a legendary pet", "Special", "Legendary", 1500,
            new Dictionary<string, float> { }, "HatchPet");
        AddItemTemplate("skill_reset", "Skill Reset Book", "Resets all pet skill points", "Special", "Epic", 400,
            new Dictionary<string, float> { }, "ResetSkills");
        AddItemTemplate("stat_boost", "Stat Boost Potion", "Permanently increases a random stat by 5%", "Special", "Legendary", 1000,
            new Dictionary<string, float> { { "stat_boost", 5 } }, "PermanentBoost");
    }
    
    private void AddItemTemplate(string id, string name, string desc, string category, string rarity, int value, Dictionary<string, float> stats, string effect) {
        ItemTemplates[id] = new Dictionary<string, object> {
            { "name", name },
            { "description", desc },
            { "category", category },
            { "rarity", rarity },
            { "value", value },
            { "stats", stats },
            { "special_effect", effect }
        };
        CategoryItems[category].Add(id);
    }
    
    public Dictionary<string, object> GetItemTemplate(string itemId) {
        if (ItemTemplates.ContainsKey(itemId)) {
            return ItemTemplates[itemId];
        }
        return null;
    }
    
    public string GetRarityColor(string rarity) {
        return RarityColors.ContainsKey(rarity) ? RarityColors[rarity] : "#FFFFFF";
    }
    
    public float GetRarityMultiplier(string rarity) {
        return RarityValueMultiplier.ContainsKey(rarity) ? RarityValueMultiplier[rarity] : 1.0f;
    }

        public override Dictionary<string, object> ExportSaveData() => new();
        public override void ImportSaveData(Dictionary<string, object> data) { }
}
