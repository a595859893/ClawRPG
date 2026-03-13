using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Database for procedural name generation system
    /// </summary>
    public class ProceduralNameDatabase {
        // Prefixes - organized by power level
        public static readonly string[] CommonPrefixes = {
            "Rusty", "Old", "Simple", "Basic", "Plain", "Crude", "Worn", "Damaged",
            "Minor", "Weak", "Cheap", "Standard", "Ordinary", "Common"
        };
        
        public static readonly string[] UncommonPrefixes = {
            "Polished", "Reinforced", "Enhanced", "Improved", "Fine", "Quality",
            "Sturdy", "Reliable", "Solid", "Dependable", "Crafted", "Worked"
        };
        
        public static readonly string[] RarePrefixes = {
            "Enchanted", "Magical", "Mystic", "Mighty", "Powerful", "Superior",
            "Exceptional", "Remarkable", "Distinguished", "Exquisite", "Intricate"
        };
        
        public static readonly string[] EpicPrefixes = {
            "Ancient", "Legendary", "Mythical", "Divine", "Celestial", "Ethereal",
            "Sovereign", "Majestic", "Grand", "Magnificent", "Supreme", "Transcendent"
        };
        
        public static readonly string[] LegendaryPrefixes = {
            "Omnipotent", "Eternal", "Primordial", "Cosmic", "Astral", "Infinite",
            "Transcendent", "Mythic", "Godly", "Celestial", "Dimensional", "Immortal"
        };
        
        // Middle parts
        public static readonly string[] MiddleParts = {
            "of the", "of", "'s", "of the", "of", "'s",
            "the", "from", "with", "bearing", "carrying", "holding"
        };
        
        // Suffixes - organized by element/theme
        public static readonly string[] CommonSuffixes = {
            "Edge", "Blade", "Guard", "Strap", "Cloth", "Ring", "Band",
            "Gem", "Stone", "Orb", "Core", "Heart", "Essence"
        };
        
        public static readonly string[] UncommonSuffixes = {
            "Flame", "Frost", "Storm", "Shadow", "Light", "Nature",
            "Iron", "Steel", "Bronze", "Silver", "Gold", "Crystal"
        };
        
        public static readonly string[] RareSuffixes = {
            "Inferno", "Glacier", "Tempest", "Void", "Radiance", "Terra",
            "Mithril", "Adamantite", "Ethereal", "Mystic", "Arcane", "Enchanted"
        };
        
        public static readonly string[] EpicSuffixes = {
            "Dragon", "Phoenix", "Titan", "Demon", "Angel", "God",
            "Cosmic", "Astral", "Celestial", "Demonic", "Divine", "Eternal"
        };
        
        public static readonly string[] LegendarySuffixes = {
            "Creation", "Destruction", "Annihilation", "Salvation", "Judgment",
            "Armageddon", "Apocalypse", "Genesis", "Infinity", "Oblivion",
            "Paradox", "Eternity", "Dimension", "Universe", "Existence"
        };
        
        // Item type keywords
        public static readonly Dictionary<string, string[]> TypeSuffixes = new Dictionary<string, string[]> {
            { "Weapon", new[] { "Sword", "Axe", "Dagger", "Staff", "Bow", "Hammer", "Spear", "Blade" } },
            { "Armor", new[] { "Shield", "Helmet", "Chestplate", "Gauntlets", "Boots", "Cuirass", "Greaves", "Pauldrons" } },
            { "Accessory", new[] { "Amulet", "Ring", "Bracelet", "Crown", " Cloak", "Belt", "Charm", "Talisman" } },
            { "Potion", new[] { "Elixir", "Tonic", "Draught", "Potion", "Phial", "Vial", "Bottle", "Flask" } },
            { "Scroll", new[] { "Tome", "Book", "Scroll", "Tablet", "Manuscript", "Codex", "Grimoire", "Legend" } },
            { "Pet", new[] { "Companion", "Familiar", "Spirit", "Guardian", "Beast", "Creature", "Warden", " Ally" } },
            { "Mount", new[] { "Steed", "Mount", "Rider", "Charger", "Courser", "Destrier", "Palfrey", "Hawk" } },
            { "Material", new[] { "Essence", "Fragment", "Shard", "Crystal", "Ingot", "Ore", "Gem", "Relic" } }
        };
        
        // Style-specific words
        public static readonly Dictionary<string, string[]> StylePrefixes = new Dictionary<string, string[]> {
            { "Fantasy", new[] { "Arcane", "Mystic", "Elder", "Forgotten", "Sacred", "Cursed", "Ancient", "Mythic" } },
            { "Modern", new[] { "Tech", "Neo", "Cyber", "Quantum", "Nano", "Plasma", "Quantum", "Alpha" } },
            { "Mythical", new[] { "Divine", "Celestial", "Olympian", "Nether", "Abyssal", "Astral", "Ethereal", "Primordial" } },
            { "Ancient", new[] { "Primordial", "Eternal", "Forgotten", "Lost", "Buried", "Sealed", "Dormant", "Ancient" } }
        };
        
        // Rarity colors for UI
        public static readonly Dictionary<string, Color> RarityColors = new Dictionary<string, Color> {
            { "Common", new Color(0.7f, 0.7f, 0.7f) },
            { "Uncommon", new Color(0.3f, 0.8f, 0.3f) },
            { "Rare", new Color(0.3f, 0.5f, 1.0f) },
            { "Epic", new Color(0.6f, 0.3f, 0.9f) },
            { "Legendary", new Color(1.0f, 0.6f, 0.1f) }
        };
        
        // Get prefix by rarity
        public static string[] GetPrefixesByRarity(string rarity) {
            switch (rarity) {
                case "Common": return CommonPrefixes;
                case "Uncommon": return UncommonPrefixes;
                case "Rare": return RarePrefixes;
                case "Epic": return EpicPrefixes;
                case "Legendary": return LegendaryPrefixes;
                default: return CommonPrefixes;
            }
        }
        
        // Get suffix by rarity
        public static string[] GetSuffixesByRarity(string rarity) {
            switch (rarity) {
                case "Common": return CommonSuffixes;
                case "Uncommon": return UncommonSuffixes;
                case "Rare": return RareSuffixes;
                case "Epic": return EpicSuffixes;
                case "Legendary": return LegendarySuffixes;
                default: return CommonSuffixes;
            }
        }
        
        // Get all rarities
        public static string[] GetAllRarities() {
            return new[] { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        }
        
        // Get all item types
        public static string[] GetAllTypes() {
            return new[] { "Weapon", "Armor", "Accessory", "Potion", "Scroll", "Pet", "Mount", "Material" };
        }
        
        // Get all styles
        public static string[] GetAllStyles() {
            return new[] { "Fantasy", "Modern", "Mythical", "Ancient" };
        }
    }
}
