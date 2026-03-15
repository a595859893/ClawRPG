using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.GuildHall {
    public class GuildHallDatabase : BaseSystem {
        public Dictionary<string, GuildHallRoom> Rooms { get; private set; }
        public Dictionary<string, GuildHallDecoration> Decorations { get; private set; }
        public Dictionary<string, GuildHallUpgrade> Upgrades { get; private set; }
        
        public override void _Ready() {
            base._Ready();
            InitializeDatabase();
        }
        
        private void InitializeDatabase() {
            Rooms = new Dictionary<string, GuildHallRoom>();
            Decorations = new Dictionary<string, GuildHallDecoration>();
            Upgrades = new Dictionary<string, GuildHallUpgrade>();
            
            // Room configurations
            AddRoom("Main Hall", "The central gathering area for guild members", 1, 0, 1000);
            AddRoom("War Room", "Strategize battles and plan attacks", 5, 5000, 5000);
            AddRoom("Treasury", "Store guild gold and valuable items", 3, 2000, 3000);
            AddRoom("Training Grounds", "Practice combat and train members", 2, 1500, 2500);
            AddRoom("Library", "Research skills and lore", 4, 3000, 4000);
            AddRoom("Garden", "Relax and restore stamina", 1, 1000, 1500);
            AddRoom("Workshop", "Craft items and equipment", 3, 2500, 3500);
            AddRoom("Arena", "PvP battles between members", 6, 6000, 6000);
            AddRoom("Altar", "Perform rituals and blessings", 5, 4500, 5000);
            AddRoom("Secret Vault", "Hidden storage for rare items", 8, 8000, 10000);
            
            // Decoration configurations
            AddDecoration("Banner", "Guild banner with logo", "Common", 100, 5, "Decoration");
            AddDecoration("Chair", "Comfortable seating", "Common", 50, 2, "Furniture");
            AddDecoration("Table", "Meeting table", "Common", 75, 3, "Furniture");
            AddDecoration("Torch", "Illumination device", "Common", 25, 1, "Lighting");
            AddDecoration("Crystal", "Magic crystal centerpiece", "Uncommon", 200, 10, "Artifact");
            AddDecoration("Trophy", "Achievement display", "Uncommon", 150, 8, "Display");
            AddDecoration("Statue", "Guild founder statue", "Rare", 500, 20, "Display");
            AddDecoration("Painting", "Artistic decoration", "Rare", 400, 15, "Display");
            AddDecoration("Fountain", "Decorative water feature", "Epic", 800, 30, "Feature");
            AddDecoration("Throne", "Guild leader seat", "Legendary", 1500, 50, "Furniture");
            AddDecoration("Ancient Artifact", "Powerful relic", "Legendary", 2000, 75, "Artifact");
            
            // Upgrade configurations
            AddUpgrade("Gold Storage", "Increase gold capacity", 1, 1000, 10000, 10000);
            AddUpgrade("Member Capacity", "Allow more members", 1, 2000, 5000, 5000);
            AddUpgrade("Experience Boost", "Faster experience gain", 1, 1500, 7500, 7500);
            AddUpgrade("Crafting Speed", "Faster item crafting", 1, 1200, 6000, 6000);
            AddUpgrade("Meeting Room", "Enable guild meetings", 1, 3000, 10000, 10000);
        }
        
        private void AddRoom(string name, string description, int requiredLevel, int goldCost, int expRequired) {
            Rooms[name] = new GuildHallRoom {
                Name = name,
                Description = description,
                RequiredLevel = requiredLevel,
                GoldCost = goldCost,
                ExperienceRequired = expRequired
            };
        }
        
        private void AddDecoration(string name, string description, string rarity, int goldCost, int prestige, string category) {
            Decorations[name] = new GuildHallDecoration {
                Name = name,
                Description = description,
                Rarity = rarity,
                GoldCost = goldCost,
                PrestigeValue = prestige,
                Category = category
            };
        }
        
        private void AddUpgrade(string name, string description, int maxLevel, int baseCost, int baseExp, int prestige) {
            Upgrades[name] = new GuildHallUpgrade {
                Name = name,
                Description = description,
                MaxLevel = maxLevel,
                BaseGoldCost = baseCost,
                BaseExperience = baseExp,
                PrestigeRequired = prestige
            };
        }
        
        public GuildHallRoom GetRoom(string name) {
            return Rooms.ContainsKey(name) ? Rooms[name] : null;
        }
        
        public GuildHallDecoration GetDecoration(string name) {
            return Decorations.ContainsKey(name) ? Decorations[name] : null;
        }
        
        public GuildHallUpgrade GetUpgrade(string name) {
            return Upgrades.ContainsKey(name) ? Upgrades[name] : null;
        }
    }
    
    public class GuildHallRoom {
        public string Name { get; set; }
        public string Description { get; set; }
        public int RequiredLevel { get; set; }
        public int GoldCost { get; set; }
        public int ExperienceRequired { get; set; }
    }
    
    public class GuildHallDecoration {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rarity { get; set; }
        public int GoldCost { get; set; }
        public int PrestigeValue { get; set; }
        public string Category { get; set; }
    }
    
    public class GuildHallUpgrade {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxLevel { get; set; }
        public int BaseGoldCost { get; set; }
        public int BaseExperience { get; set; }
        public int PrestigeRequired { get; set; }
    }
}
