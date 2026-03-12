using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    public class PetSkillTreeDatabase
    {
        private static PetSkillTreeDatabase _instance;
        public static PetSkillTreeDatabase Instance => _instance ??= new PetSkillTreeDatabase();

        public Dictionary<string, Dictionary<PetSkillTreeData.SkillTreeType, List<PetSkillTreeData.SkillNode>>> SkillTrees = 
            new Dictionary<string, Dictionary<PetSkillTreeData.SkillTreeType, List<PetSkillTreeData.SkillNode>>>();

        public PetSkillTreeDatabase() => InitializeSkillTrees();

        private void InitializeSkillTrees()
        {
            InitializeFireSkillTrees();
            InitializeWaterSkillTrees();
            InitializeIceSkillTrees();
            InitializeLightningSkillTrees();
            InitializeBeastSkillTrees();
            InitializeMythicalSkillTrees();
            InitializeCommonSkillTrees();
            InitializeUndeadSkillTrees();
        }

        private void InitializeFireSkillTrees()
        {
            var fireType = "Fire";
            var offensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "fire_ignite_1", Name = "Ignite", Description = "Attacks apply burn", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "fire_ignite", StatBonuses = new Dictionary<string, float> { { "burn_chance", 0.1f } } },
                new PetSkillTreeData.SkillNode { NodeId = "fire_blaze_1", Name = "Blaze", Description = "Fire damage +15%", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "fire_blaze", StatBonuses = new Dictionary<string, float> { { "fire_damage", 0.15f } }, Prerequisites = new List<string> { "fire_ignite_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "fire_inferno_1", Name = "Inferno", Description = "Burn damage +25%", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 2, Cost = 2, IconName = "fire_inferno", StatBonuses = new Dictionary<string, float> { { "burn_damage", 0.25f } }, Prerequisites = new List<string> { "fire_blaze_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "fire_phoenix_1", Name = "Phoenix Flame", Description = "Ultimate: Revive 50% HP", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 3, Cost = 3, IconName = "fire_phoenix", IsUltimate = true, Prerequisites = new List<string> { "fire_inferno_1" } }
            };
            var defensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "fire_heat_1", Name = "Heat Shield", Description = "Fire resistance +20%", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 1, Cost = 1, IconName = "fire_heat", StatBonuses = new Dictionary<string, float> { { "fire_resistance", 0.2f } } },
                new PetSkillTreeData.SkillNode { NodeId = "fire_adapt_1", Name = "Heat Adaptation", Description = "Max HP +10%", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 1, Cost = 1, IconName = "fire_adapt", StatBonuses = new Dictionary<string, float> { { "max_health", 0.1f } }, Prerequisites = new List<string> { "fire_heat_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "fire_rebirth_1", Name = "Rebirth", Description = "Ultimate: Full HP recover", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 3, Cost = 3, IconName = "fire_rebirth", IsUltimate = true, Prerequisites = new List<string> { "fire_adapt_1" } }
            };
            var supportNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "fire_warmth_1", Name = "Warmth", Description = "Heal allies 5 HP/sec", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 1, Cost = 1, IconName = "fire_warmth", StatBonuses = new Dictionary<string, float> { { "aoe_heal", 5f } } },
                new PetSkillTreeData.SkillNode { NodeId = "fire_buff_1", Name = "Fire Blessing", Description = "Ally fire damage +10%", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 1, Cost = 1, IconName = "fire_buff", StatBonuses = new Dictionary<string, float> { { "ally_fire_buff", 0.1f } }, Prerequisites = new List<string> { "fire_warmth_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "fire_spirit_1", Name = "Fire Spirit", Description = "Ultimate: Summon spirit", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 3, Cost = 3, IconName = "fire_spirit", IsUltimate = true, Prerequisites = new List<string> { "fire_buff_1" } }
            };
            var specialNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "fire_natural_1", Name = "Inner Fire", Description = "Critical +8%", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 1, Cost = 1, IconName = "fire_natural", StatBonuses = new Dictionary<string, float> { { "critical_chance", 0.08f } } },
                new PetSkillTreeData.SkillNode { NodeId = "fire_energy_1", Name = "Burning Energy", Description = "Cooldown -10%", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 1, Cost = 1, IconName = "fire_energy", StatBonuses = new Dictionary<string, float> { { "cooldown_reduction", 0.1f } }, Prerequisites = new List<string> { "fire_natural_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "fire_ascend_1", Name = "Ascension", Description = "Ultimate: Fire elemental", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 3, Cost = 3, IconName = "fire_ascend", IsUltimate = true, Prerequisites = new List<string> { "fire_energy_1" } }
            };
            AddSkillTree(fireType, PetSkillTreeData.SkillTreeType.Offensive, offensiveNodes);
            AddSkillTree(fireType, PetSkillTreeData.SkillTreeType.Defensive, defensiveNodes);
            AddSkillTree(fireType, PetSkillTreeData.SkillTreeType.Support, supportNodes);
            AddSkillTree(fireType, PetSkillTreeData.SkillTreeType.Special, specialNodes);
        }

        private void InitializeWaterSkillTrees()
        {
            var waterType = "Water";
            var offensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "water_splash_1", Name = "Splash", Description = "Apply wet", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "water_splash", StatBonuses = new Dictionary<string, float> { { "wet_chance", 0.15f } } },
                new PetSkillTreeData.SkillNode { NodeId = "water_flood_1", Name = "Flood", Description = "Water damage +15%", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "water_flood", StatBonuses = new Dictionary<string, float> { { "water_damage", 0.15f } }, Prerequisites = new List<string> { "water_splash_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "water_tsunami_1", Name = "Tsunami", Description = "Ultimate: Water wave", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 3, Cost = 3, IconName = "water_tsunami", IsUltimate = true, Prerequisites = new List<string> { "water_flood_1" } }
            };
            var defensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "water_aquatic_1", Name = "Aquatic", Description = "Water resistance +25%", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 1, Cost = 1, IconName = "water_aquatic", StatBonuses = new Dictionary<string, float> { { "water_resistance", 0.25f } } },
                new PetSkillTreeData.SkillNode { NodeId = "water_tide_1", Name = "High Tide", Description = "Regen 10 HP/sec", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 2, Cost = 2, IconName = "water_tide", StatBonuses = new Dictionary<string, float> { { "hp_regen", 10f } }, Prerequisites = new List<string> { "water_aquatic_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "water_barrier_1", Name = "Water Barrier", Description = "Ultimate: Absorb damage", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 3, Cost = 3, IconName = "water_barrier", IsUltimate = true, Prerequisites = new List<string> { "water_tide_1" } }
            };
            var supportNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "water_heal_1", Name = "Healing Waters", Description = "Heal allies 15 HP", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 1, Cost = 1, IconName = "water_heal", StatBonuses = new Dictionary<string, float> { { "aoe_heal", 15f } } },
                new PetSkillTreeData.SkillNode { NodeId = "water_purify_1", Name = "Purify", Description = "Remove debuffs", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 1, Cost = 1, IconName = "water_purify", StatBonuses = new Dictionary<string, float> { { "cleanse", 1 } }, Prerequisites = new List<string> { "water_heal_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "water_serenity_1", Name = "Serenity", Description = "Ultimate: Full heal", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 3, Cost = 3, IconName = "water_serenity", IsUltimate = true, Prerequisites = new List<string> { "water_purify_1" } }
            };
            var specialNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "water_flow_1", Name = "Flow State", Description = "Mana +20%", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 1, Cost = 1, IconName = "water_flow", StatBonuses = new Dictionary<string, float> { { "mana_pool", 0.2f } } },
                new PetSkillTreeData.SkillNode { NodeId = "water_depths_1", Name = "Deep Waters", Description = "Max HP +20%", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 2, Cost = 2, IconName = "water_depths", StatBonuses = new Dictionary<string, float> { { "max_health", 0.2f } }, Prerequisites = new List<string> { "water_flow_1" } }
            };
            AddSkillTree(waterType, PetSkillTreeData.SkillTreeType.Offensive, offensiveNodes);
            AddSkillTree(waterType, PetSkillTreeData.SkillTreeType.Defensive, defensiveNodes);
            AddSkillTree(waterType, PetSkillTreeData.SkillTreeType.Support, supportNodes);
            AddSkillTree(waterType, PetSkillTreeData.SkillTreeType.Special, specialNodes);
        }

        private void InitializeIceSkillTrees()
        {
            var iceType = "Ice";
            var offensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "ice_freeze_1", Name = "Freeze", Description = "May freeze", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "ice_freeze", StatBonuses = new Dictionary<string, float> { { "freeze_chance", 0.1f } } },
                new PetSkillTreeData.SkillNode { NodeId = "ice_shatter_1", Name = "Shatter", Description = "Frozen +30% dmg", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "ice_shatter", StatBonuses = new Dictionary<string, float> { { "frozen_damage_bonus", 0.3f } }, Prerequisites = new List<string> { "ice_freeze_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "ice_absolute_1", Name = "Absolute Zero", Description = "Ultimate: Freeze all", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 3, Cost = 3, IconName = "ice_absolute", IsUltimate = true, Prerequisites = new List<string> { "ice_shatter_1" } }
            };
            var defensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "ice_armor_1", Name = "Ice Armor", Description = "Physical -15%", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 1, Cost = 1, IconName = "ice_armor", StatBonuses = new Dictionary<string, float> { { "physical_reduction", 0.15f } } },
                new PetSkillTreeData.SkillNode { NodeId = "ice_eternal_1", Name = "Eternal Winter", Description = "Ultimate: Invincible", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 3, Cost = 3, IconName = "ice_eternal", IsUltimate = true, Prerequisites = new List<string> { "ice_armor_1" } }
            };
            var supportNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "ice_chill_1", Name = "Chilling Presence", Description = "Slow 20%", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 1, Cost = 1, IconName = "ice_chill", StatBonuses = new Dictionary<string, float> { { "enemy_slow", 0.2f } } },
                new PetSkillTreeData.SkillNode { NodeId = "ice_aurora_1", Name = "Aurora Borealis", Description = "Ultimate: Freeze all", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 3, Cost = 3, IconName = "ice_aurora", IsUltimate = true, Prerequisites = new List<string> { "ice_chill_1" } }
            };
            var specialNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "ice_crystal_1", Name = "Crystal Form", Description = "Defense +15%", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 1, Cost = 1, IconName = "ice_crystal", StatBonuses = new Dictionary<string, float> { { "defense", 0.15f } } },
                new PetSkillTreeData.SkillNode { NodeId = "ice_primordial_1", Name = "Primordial Ice", Description = "Ultimate: Ice god", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 3, Cost = 3, IconName = "ice_primordial", IsUltimate = true, Prerequisites = new List<string> { "ice_crystal_1" } }
            };
            AddSkillTree(iceType, PetSkillTreeData.SkillTreeType.Offensive, offensiveNodes);
            AddSkillTree(iceType, PetSkillTreeData.SkillTreeType.Defensive, defensiveNodes);
            AddSkillTree(iceType, PetSkillTreeData.SkillTreeType.Support, supportNodes);
            AddSkillTree(iceType, PetSkillTreeData.SkillTreeType.Special, specialNodes);
        }

        private void InitializeLightningSkillTrees()
        {
            var lightningType = "Lightning";
            var offensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "lightning_shock_1", Name = "Shock", Description = "May shock", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "lightning_shock", StatBonuses = new Dictionary<string, float> { { "shock_chance", 0.15f } } },
                new PetSkillTreeData.SkillNode { NodeId = "lightning_bolt_1", Name = "Lightning Bolt", Description = "Lightning +20%", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "lightning_bolt", StatBonuses = new Dictionary<string, float> { { "lightning_damage", 0.2f } }, Prerequisites = new List<string> { "lightning_shock_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "lightning_thunder_1", Name = "Thunder God", Description = "Ultimate: Thunder", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 3, Cost = 3, IconName = "lightning_thunder", IsUltimate = true, Prerequisites = new List<string> { "lightning_bolt_1" } }
            };
            var defensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "lightning_insulate_1", Name = "Insulation", Description = "Lightning -30%", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 1, Cost = 1, IconName = "lightning_insulate", StatBonuses = new Dictionary<string, float> { { "lightning_resistance", 0.3f } } },
                new PetSkillTreeData.SkillNode { NodeId = "lightning_surge_1", Name = "Power Surge", Description = "Ultimate: Absorb", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 3, Cost = 3, IconName = "lightning_surge", IsUltimate = true, Prerequisites = new List<string> { "lightning_insulate_1" } }
            };
            var supportNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "lightning_speed_1", Name = "Speed Boost", Description = "Ally speed +20%", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 1, Cost = 1, IconName = "lightning_speed", StatBonuses = new Dictionary<string, float> { { "ally_speed", 0.2f } } },
                new PetSkillTreeData.SkillNode { NodeId = "lightning_storm_1", Name = "Lightning Storm", Description = "Ultimate: Electrify", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 3, Cost = 3, IconName = "lightning_storm", IsUltimate = true, Prerequisites = new List<string> { "lightning_speed_1" } }
            };
            var specialNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "lightning_quick_1", Name = "Quick Strike", Description = "Atk speed +15%", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 1, Cost = 1, IconName = "lightning_quick", StatBonuses = new Dictionary<string, float> { { "attack_speed", 0.15f } } }
            };
            AddSkillTree(lightningType, PetSkillTreeData.SkillTreeType.Offensive, offensiveNodes);
            AddSkillTree(lightningType, PetSkillTreeData.SkillTreeType.Defensive, defensiveNodes);
            AddSkillTree(lightningType, PetSkillTreeData.SkillTreeType.Support, supportNodes);
            AddSkillTree(lightningType, PetSkillTreeData.SkillTreeType.Special, specialNodes);
        }

        private void InitializeBeastSkillTrees()
        {
            var wolfType = "Wolf";
            var offensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "wolf_fang_1", Name = "Sharp Fang", Description = "Attack +10%", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "wolf_fang", StatBonuses = new Dictionary<string, float> { { "attack", 0.1f } } },
                new PetSkillTreeData.SkillNode { NodeId = "wolf_pack_1", Name = "Pack Hunter", Description = "Pack +15%", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "wolf_pack", StatBonuses = new Dictionary<string, float> { { "pack_bonus", 0.15f } }, Prerequisites = new List<string> { "wolf_fang_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "wolf_alpha_1", Name = "Alpha Strike", Description = "Ultimate: Pack attack", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 3, Cost = 3, IconName = "wolf_alpha", IsUltimate = true, Prerequisites = new List<string> { "wolf_pack_1" } }
            };
            var defensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "wolf_fur_1", Name = "Thick Fur", Description = "Defense +10%", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 1, Cost = 1, IconName = "wolf_fur", StatBonuses = new Dictionary<string, float> { { "defense", 0.1f } } },
                new PetSkillTreeData.SkillNode { NodeId = "wolf_feral_1", Name = "Feral Defense", Description = "Ultimate: Full def", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 3, Cost = 3, IconName = "wolf_feral", IsUltimate = true, Prerequisites = new List<string> { "wolf_fur_1" } }
            };
            var supportNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "wolf_scent_1", Name = "Keen Scent", Description = "Detect +25%", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 1, Cost = 1, IconName = "wolf_scent", StatBonuses = new Dictionary<string, float> { { "detect_range", 0.25f } } }
            };
            var specialNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "wolf_instinct_1", Name = "Predator Instinct", Description = "Critical +8%", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 1, Cost = 1, IconName = "wolf_instinct", StatBonuses = new Dictionary<string, float> { { "critical_chance", 0.08f } } }
            };
            AddSkillTree(wolfType, PetSkillTreeData.SkillTreeType.Offensive, offensiveNodes);
            AddSkillTree(wolfType, PetSkillTreeData.SkillTreeType.Defensive, defensiveNodes);
            AddSkillTree(wolfType, PetSkillTreeData.SkillTreeType.Support, supportNodes);
            AddSkillTree(wolfType, PetSkillTreeData.SkillTreeType.Special, specialNodes);
        }

        private void InitializeMythicalSkillTrees()
        {
            var dragonType = "Dragon";
            var offensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "dragon_fire_1", Name = "Dragon Fire", Description = "Fire +25%", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "dragon_fire", StatBonuses = new Dictionary<string, float> { { "fire_damage", 0.25f } } },
                new PetSkillTreeData.SkillNode { NodeId = "dragon_breath_1", Name = "Dragon Breath", Description = "AOE breath", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "dragon_breath", StatBonuses = new Dictionary<string, float> { { "breath_aoe", 1 } }, Prerequisites = new List<string> { "dragon_fire_1" } },
                new PetSkillTreeData.SkillNode { NodeId = "dragon_apocalypse_1", Name = "Apocalypse", Description = "Ultimate: World burn", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 3, Cost = 3, IconName = "dragon_apocalypse", IsUltimate = true, Prerequisites = new List<string> { "dragon_breath_1" } }
            };
            var defensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "dragon_scales_1", Name = "Dragon Scales", Description = "All resist +20%", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 1, Cost = 1, IconName = "dragon_scales", StatBonuses = new Dictionary<string, float> { { "all_resistance", 0.2f } } },
                new PetSkillTreeData.SkillNode { NodeId = "dragon_immortal_1", Name = "Dragon Immortal", Description = "Ultimate: Immortal", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 3, Cost = 3, IconName = "dragon_immortal", IsUltimate = true, Prerequisites = new List<string> { "dragon_scales_1" } }
            };
            var supportNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "dragon_aura_1", Name = "Dragon Aura", Description = "Ally dmg +15%", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 1, Cost = 1, IconName = "dragon_aura", StatBonuses = new Dictionary<string, float> { { "ally_damage", 0.15f } } }
            };
            var specialNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "dragon_might_1", Name = "Dragon Might", Description = "All stats +15%", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 1, Cost = 1, IconName = "dragon_might", StatBonuses = new Dictionary<string, float> { { "all_attributes", 0.15f } } }
            };
            AddSkillTree(dragonType, PetSkillTreeData.SkillTreeType.Offensive, offensiveNodes);
            AddSkillTree(dragonType, PetSkillTreeData.SkillTreeType.Defensive, defensiveNodes);
            AddSkillTree(dragonType, PetSkillTreeData.SkillTreeType.Support, supportNodes);
            AddSkillTree(dragonType, PetSkillTreeData.SkillTreeType.Special, specialNodes);
        }

        private void InitializeCommonSkillTrees()
        {
            var slimeType = "Slime";
            var offensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "slime_splash_1", Name = "Splash", Description = "Attack +5%", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "slime_splash", StatBonuses = new Dictionary<string, float> { { "attack", 0.05f } } }
            };
            var defensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "slime_gel_1", Name = "Gelatinous", Description = "Defense +5%", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 1, Cost = 1, IconName = "slime_gel", StatBonuses = new Dictionary<string, float> { { "defense", 0.05f } } }
            };
            var supportNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "slime_heal_1", Name = "Slime Heal", Description = "Small heal", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 1, Cost = 1, IconName = "slime_heal", StatBonuses = new Dictionary<string, float> { { "aoe_heal", 2f } } }
            };
            var specialNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "slime_growth_1", Name = "Growth", Description = "HP +5%", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 1, Cost = 1, IconName = "slime_growth", StatBonuses = new Dictionary<string, float> { { "max_health", 0.05f } } }
            };
            AddSkillTree(slimeType, PetSkillTreeData.SkillTreeType.Offensive, offensiveNodes);
            AddSkillTree(slimeType, PetSkillTreeData.SkillTreeType.Defensive, defensiveNodes);
            AddSkillTree(slimeType, PetSkillTreeData.SkillTreeType.Support, supportNodes);
            AddSkillTree(slimeType, PetSkillTreeData.SkillTreeType.Special, specialNodes);
        }

        private void InitializeUndeadSkillTrees()
        {
            var skeletonType = "Skeleton";
            var offensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "skel_bone_1", Name = "Bone Throw", Description = "Attack +5%", Type = PetSkillTreeData.SkillTreeType.Offensive, Tier = 1, Cost = 1, IconName = "skel_bone", StatBonuses = new Dictionary<string, float> { { "attack", 0.05f } } }
            };
            var defensiveNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "skel_bone_armor_1", Name = "Bone Armor", Description = "Defense +5%", Type = PetSkillTreeData.SkillTreeType.Defensive, Tier = 1, Cost = 1, IconName = "skel_bone_armor", StatBonuses = new Dictionary<string, float> { { "defense", 0.05f } } }
            };
            var supportNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "skel_fear_1", Name = "Terrify", Description = "Fear chance", Type = PetSkillTreeData.SkillTreeType.Support, Tier = 1, Cost = 1, IconName = "skel_fear", StatBonuses = new Dictionary<string, float> { { "fear_chance", 0.05f } } }
            };
            var specialNodes = new List<PetSkillTreeData.SkillNode>
            {
                new PetSkillTreeData.SkillNode { NodeId = "skel_life_drain_1", Name = "Life Drain", Description = "Lifesteal +3%", Type = PetSkillTreeData.SkillTreeType.Special, Tier = 1, Cost = 1, IconName = "skel_life_drain", StatBonuses = new Dictionary<string, float> { { "lifesteal", 0.03f } } }
            };
            AddSkillTree(skeletonType, PetSkillTreeData.SkillTreeType.Offensive, offensiveNodes);
            AddSkillTree(skeletonType, PetSkillTreeData.SkillTreeType.Defensive, defensiveNodes);
            AddSkillTree(skeletonType, PetSkillTreeData.SkillTreeType.Support, supportNodes);
            AddSkillTree(skeletonType, PetSkillTreeData.SkillTreeType.Special, specialNodes);
        }

        private void AddSkillTree(string petType, PetSkillTreeData.SkillTreeType treeType, List<PetSkillTreeData.SkillNode> nodes)
        {
            if (!SkillTrees.ContainsKey(petType))
                SkillTrees[petType] = new Dictionary<PetSkillTreeData.SkillTreeType, List<PetSkillTreeData.SkillNode>>();
            SkillTrees[petType][treeType] = nodes;
        }

        public List<PetSkillTreeData.SkillNode> GetSkillTree(string petType, PetSkillTreeData.SkillTreeType treeType)
        {
            if (SkillTrees.TryGetValue(petType, out var trees) && trees.TryGetValue(treeType, out var nodes))
                return nodes;
            if (SkillTrees.TryGetValue("Slime", out trees) && trees.TryGetValue(treeType, out nodes))
                return nodes;
            return new List<PetSkillTreeData.SkillNode>();
        }

        public List<string> GetAvailablePetTypes() => new List<string>(SkillTrees.Keys);
    }
}
