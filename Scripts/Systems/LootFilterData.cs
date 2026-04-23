using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Defines the available filter dimensions for loot filtering.
    /// </summary>
    public enum FilterDimension
    {
        /// <summary>Filter by rarity tier.</summary>
        Rarity,
        
        /// <summary>Filter by loot type.</summary>
        Type,
        
        /// <summary>Filter by attribute affinity.</summary>
        Attribute,
        
        /// <summary>Filter by source origin.</summary>
        Source
    }

    /// <summary>
    /// A single filter rule that can be enabled or disabled.
    /// </summary>
    [Serializable]
    public class FilterRule
    {
        /// <summary>Which dimension this rule applies to.</summary>
        public FilterDimension Dimension;
        
        /// <summary>The value to match (e.g., LootRarity.Epic, LootType.Equipment).</summary>
        public string Value;
        
        /// <summary>Whether this rule is currently active.</summary>
        public bool Enabled = true;
        
        /// <summary>Display label for this rule.</summary>
        public string Label;
    }

    /// <summary>
    /// A saved filter preset with a name and set of active rules.
    /// </summary>
    [Serializable]
    public class FilterPreset
    {
        /// <summary>Unique preset identifier.</summary>
        public string Id;
        
        /// <summary>Human-readable name shown in UI.</summary>
        public string Name;
        
        /// <summary>All rules in this preset.</summary>
        public List<FilterRule> Rules = new List<FilterRule>();
        
        /// <summary>Icon resource path for the preset button.</summary>
        public string IconPath;
    }

    /// <summary>
    /// Stores the complete loot filter state for a player session.
    /// Persisted as a Godot Resource.
    /// </summary>
    [Serializable]
    public partial class LootFilterData : Resource
    {
        /// <summary>All available presets.</summary>
        public List<FilterPreset> Presets = new List<FilterPreset>();

        /// <summary>Currently active preset id (null = custom).</summary>
        public string ActivePresetId;

        /// <summary>Custom rules active when no preset is selected.</summary>
        public List<FilterRule> CustomRules = new List<FilterRule>();

        /// <summary>
        /// Returns the currently active preset, or null if using custom rules.
        /// </summary>
        public FilterPreset GetActivePreset()
        {
            if (string.IsNullOrEmpty(ActivePresetId))
                return null;
            
            foreach (var p in Presets)
                if (p.Id == ActivePresetId)
                    return p;
            return null;
        }

        /// <summary>
        /// Gets all rules that should be applied: preset rules or custom rules.
        /// </summary>
        public List<FilterRule> GetActiveRules()
        {
            var preset = GetActivePreset();
            return preset != null ? preset.Rules : CustomRules;
        }

        /// <summary>
        /// Creates the default built-in presets.
        /// </summary>
        public void CreateDefaultPresets()
        {
            Presets.Clear();

            // "只看传说" preset
            var legendaryOnly = new FilterPreset
            {
                Id = "preset_legendary_only",
                Name = "只看传说",
                Rules = new List<FilterRule>
                {
                    new FilterRule { Dimension = FilterDimension.Rarity, Value = "Legendary", Label = "传说", Enabled = true }
                }
            };

            // "只看材料" preset
            var materialsOnly = new FilterPreset
            {
                Id = "preset_materials_only",
                Name = "只看材料",
                Rules = new List<FilterRule>
                {
                    new FilterRule { Dimension = FilterDimension.Type, Value = "Material", Label = "材料", Enabled = true }
                }
            };

            // "只看装备" preset
            var equipmentOnly = new FilterPreset
            {
                Id = "preset_equipment_only",
                Name = "只看装备",
                Rules = new List<FilterRule>
                {
                    new FilterRule { Dimension = FilterDimension.Type, Value = "Equipment", Label = "装备", Enabled = true }
                }
            };

            // "稀有以上" preset
            var rareAbove = new FilterPreset
            {
                Id = "preset_rare_above",
                Name = "稀有以上",
                Rules = new List<FilterRule>
                {
                    new FilterRule { Dimension = FilterDimension.Rarity, Value = "Rare", Label = "稀有+", Enabled = true },
                    new FilterRule { Dimension = FilterDimension.Rarity, Value = "Epic", Label = "史诗", Enabled = true },
                    new FilterRule { Dimension = FilterDimension.Rarity, Value = "Legendary", Label = "传说", Enabled = true }
                }
            };

            Presets.Add(legendaryOnly);
            Presets.Add(materialsOnly);
            Presets.Add(equipmentOnly);
            Presets.Add(rareAbove);
        }

        /// <summary>
        /// Adds a new preset or updates an existing one.
        /// </summary>
        public void SavePreset(FilterPreset preset)
        {
            for (int i = 0; i < Presets.Count; i++)
            {
                if (Presets[i].Id == preset.Id)
                {
                    Presets[i] = preset;
                    return;
                }
            }
            Presets.Add(preset);
        }
    }
}
