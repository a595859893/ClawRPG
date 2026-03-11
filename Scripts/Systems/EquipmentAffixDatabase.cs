using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Equipment affix database - defines all possible affixes
    /// </summary>
    public static class EquipmentAffixDatabase
    {
        // All available affixes
        private static List<EquipmentAffix> _affixes = new List<EquipmentAffix>();
        
        // Affixes by type
        private static Dictionary<AffixType, List<EquipmentAffix>> _affixesByType = new Dictionary<AffixType, List<EquipmentAffix>>();
        
        // Affixes by minimum quality
        private static Dictionary<ItemQuality, List<EquipmentAffix>> _affixesByQuality = new Dictionary<ItemQuality, List<EquipmentAffix>>();
        
        public static bool IsInitialized { get; private set; } = false; 

        /// <summary>
        /// Initialize all affixes
        /// </summary>
        public static void Initialize()
        {
            if (IsInitialized) return;
            
            // Clear existing
            _affixes.Clear();
            _affixesByType.Clear();
            _affixesByQuality.Clear();
            
            // ===== PREFIXES (Attack/Defense/Life/Speed) =====
            // Common prefixes (Common quality)
            AddAffix("sharp", "锋利的", AffixType.Prefix, ItemQuality.Common, 5f, "attack", 10f);
            AddAffix("sturdy", "坚固的", AffixType.Prefix, ItemQuality.Common, 10f, "defense", 10f);
            AddAffix("hardy", "强壮的", AffixType.Prefix, ItemQuality.Common, 20f, "health", 10f);
            AddAffix("swift", "迅捷的", AffixType.Prefix, ItemQuality.Common, 3f, "speed", 10f);
            
            // Uncommon prefixes
            AddAffix("fierce", "凶猛的", AffixType.Prefix, ItemQuality.Uncommon, 12f, "attack", 8f);
            AddAffix("reinforced", "强化的", AffixType.Prefix, ItemQuality.Uncommon, 25f, "defense", 8f);
            AddAffix("robust", "强健的", AffixType.Prefix, ItemQuality.Uncommon, 50f, "health", 8f);
            AddAffix("agile", "灵活的", AffixType.Prefix, ItemQuality.Uncommon, 7f, "speed", 8f);
            
            // Rare prefixes
            AddAffix("vicious", "恶毒的", AffixType.Prefix, ItemQuality.Rare, 20f, "attack", 6f);
            AddAffix("impenetrable", "坚不可摧的", AffixType.Prefix, ItemQuality.Rare, 45f, "defense", 6f);
            AddAffix("vital", "致命的", AffixType.Prefix, ItemQuality.Rare, 100f, "health", 6f);
            AddAffix("blazing", "炽热的", AffixType.Prefix, ItemQuality.Rare, 12f, "speed", 6f);
            
            // Epic prefixes
            AddAffix("devastating", "毁灭性的", AffixType.Prefix, ItemQuality.Epic, 35f, "attack", 4f);
            AddAffix("invincible", "无敌的", AffixType.Prefix, ItemQuality.Epic, 80f, "defense", 4f);
            AddAffix("eternal", "永恒的", AffixType.Prefix, ItemQuality.Epic, 200f, "health", 4f);
            AddAffix("windwalker", "风行者", AffixType.Prefix, ItemQuality.Epic, 20f, "speed", 4f);
            
            // Legendary prefixes
            AddAffix("godslayer", "弑神者", AffixType.Prefix, ItemQuality.Legendary, 60f, "attack", 2f);
            AddAffix("immortal", "不朽者", AffixType.Prefix, ItemQuality.Legendary, 150f, "defense", 2f);
            AddAffix("divine", "神圣的", AffixType.Prefix, ItemQuality.Legendary, 400f, "health", 2f);
            AddAffix("lightning", "闪电的", AffixType.Prefix, ItemQuality.Legendary, 35f, "speed", 2f);
            
            // ===== SUFFIXES (Crit/Lifesteal/Dodge/Resistance) =====
            // Common suffixes
            AddAffix("of Striking", "打击", AffixType.Suffix, ItemQuality.Common, 2f, "crit_rate", 10f);
            AddAffix("of Vampirism", "吸血", AffixType.Suffix, ItemQuality.Common, 1f, "lifesteal", 10f);
            
            // Uncommon suffixes
            AddAffix("of Precision", "精准", AffixType.Suffix, ItemQuality.Uncommon, 5f, "crit_rate", 8f);
            AddAffix("of Leeching", "汲取", AffixType.Suffix, ItemQuality.Uncommon, 3f, "lifesteal", 8f);
            AddAffix("of Evasion", "闪避", AffixType.Suffix, ItemQuality.Uncommon, 3f, "dodge", 8f);
            
            // Rare suffixes
            AddAffix("of Deadliness", "致命", AffixType.Suffix, ItemQuality.Rare, 10f, "crit_rate", 6f);
            AddAffix("of Carnage", "屠杀", AffixType.Suffix, ItemQuality.Rare, 6f, "crit_damage", 6f);
            AddAffix("of Soul Drinking", "饮魂", AffixType.Suffix, ItemQuality.Rare, 5f, "lifesteal", 6f);
            AddAffix("of Shadow", "暗影", AffixType.Suffix, ItemQuality.Rare, 6f, "dodge", 6f);
            AddAffix("of Fortitude", "坚韧", AffixType.Suffix, ItemQuality.Rare, 8f, "resistance", 6f);
            
            // Epic suffixes
            AddAffix("of Annihilation", "湮灭", AffixType.Suffix, ItemQuality.Epic, 18f, "crit_rate", 4f);
            AddAffix("of Massacre", "大屠杀", AffixType.Suffix, ItemQuality.Epic, 12f, "crit_damage", 4f);
            AddAffix("of Bloodletting", "放血", AffixType.Suffix, ItemQuality.Epic, 9f, "lifesteal", 4f);
            AddAffix("of Phasing", "相位", AffixType.Suffix, ItemQuality.Epic, 10f, "dodge", 4f);
            AddAffix("of Warding", "守护", AffixType.Suffix, ItemQuality.Epic, 15f, "resistance", 4f);
            
            // Legendary suffixes
            AddAffix("of Execution", "处决", AffixType.Suffix, ItemQuality.Legendary, 30f, "crit_rate", 2f);
            AddAffix("of Destruction", "破坏", AffixType.Suffix, ItemQuality.Legendary, 25f, "crit_damage", 2f);
            AddAffix("of Immolation", "献祭", AffixType.Suffix, ItemQuality.Legendary, 15f, "lifesteal", 2f);
            AddAffix("of Ethereal", "虚无", AffixType.Suffix, ItemQuality.Legendary, 18f, "dodge", 2f);
            AddAffix("of Aegis", "护盾", AffixType.Suffix, ItemQuality.Legendary, 25f, "resistance", 2f);
            
            // Build lookup dictionaries
            foreach (var affix in _affixes)
            {
                if (!_affixesByType.ContainsKey(affix.Type))
                    _affixesByType[affix.Type] = new List<EquipmentAffix>();
                _affixesByType[affix.Type].Add(affix);
                
                if (!_affixesByQuality.ContainsKey(affix.MinQuality))
                    _affixesByQuality[affix.MinQuality] = new List<EquipmentAffix>();
                _affixesByQuality[affix.MinQuality].Add(affix);
            }
            
            IsInitialized = true;
            GD.Print($"[EquipmentAffixDatabase] Initialized with {_affixes.Count} affixes");
        }
        
        private static void AddAffix(string id, string name, AffixType type, ItemQuality minQuality, 
            float value, string attrName, float weight)
        {
            string[] attrNames = { "attack", "defense", "health", "speed", "crit_rate", "crit_damage", "lifesteal", "dodge", "resistance" };
            string[] attrDescriptions = { "攻击", "防御", "生命", "速度", "暴击率", "暴击伤害", "生命偷取", "闪避", "韧性" };
            
            int idx = Array.IndexOf(attrNames, attrName);
            string desc = idx >= 0 ? $"+{value} {attrDescriptions[idx]}" : $"+{value} {attrName}";
            
            _affixes.Add(new EquipmentAffix
            {
                Id = id,
                Name = name,
                Description = desc,
                Type = type,
                MinQuality = minQuality,
                AttributeValue = value,
                AttributeName = attrName,
                Weight = weight
            });
        }
        
        /// <summary>
        /// Get all affixes
        /// </summary>
        public static List<EquipmentAffix> GetAllAffixes() => new List<EquipmentAffix>(_affixes);
        
        /// <summary>
        /// Get affixes by type
        /// </summary>
        public static List<EquipmentAffix> GetAffixesByType(AffixType type)
        {
            return _affixesByType.ContainsKey(type) ? new List<EquipmentAffix>(_affixesByType[type]) : new List<EquipmentAffix>();
        }
        
        /// <summary>
        /// Get affixes by minimum quality
        /// </summary>
        public static List<EquipmentAffix> GetAffixesByQuality(ItemQuality minQuality)
        {
            return _affixesByQuality.ContainsKey(minQuality) ? new List<EquipmentAffix>(_affixesByQuality[minQuality]) : new List<EquipmentAffix>();
        }
        
        /// <summary>
        /// Get affixes available for a given quality level
        /// </summary>
        public static List<EquipmentAffix> GetAffixesForQuality(ItemQuality quality)
        {
            List<EquipmentAffix> result = new List<EquipmentAffix>();
            
            // Include affixes from Common up to the given quality
            ItemQuality[] qualities = { ItemQuality.Common, ItemQuality.Uncommon, ItemQuality.Rare, ItemQuality.Epic, ItemQuality.Legendary };
            bool includeHigher = false; 
            
            foreach (var q in qualities)
            {
                if (q == quality) includeHigher = true;
                if (includeHigher && _affixesByQuality.ContainsKey(q))
                {
                    result.AddRange(_affixesByQuality[q]);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get affix count by quality
        /// </summary>
        public static int GetAffixCount(ItemQuality quality)
        {
            switch (quality)
            {
                case ItemQuality.Common: return 0;    // No affixes
                case ItemQuality.Uncommon: return 1; // 1 affix
                case ItemQuality.Rare: return 2;      // 2 affixes
                case ItemQuality.Epic: return 3;     // 3 affixes
                case ItemQuality.Legendary: return 4; // 4 affixes
                default: return 0;
            }
        }
    }
}
