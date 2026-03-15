namespace ClawRPG.Systems
{
    /// <summary>
    /// Rune database with all rune configurations
    /// </summary>
    public static class RuneDatabase
    {
        public static readonly Dictionary<string, Rune> Runes = new Dictionary<string, Rune>();
        
        static RuneDatabase()
        {
            InitializeRunes();
        }
        
        private static void InitializeRunes()
        {
            // Fire Runes
            AddRune("fire_rune_common", "Fire Rune", "Adds fire damage", RuneType.FireRune, RuneRarity.Common, damageBonus: 5f);
            AddRune("fire_rune_uncommon", "Greater Fire Rune", "Adds significant fire damage", RuneType.FireRune, RuneRarity.Uncommon, damageBonus: 10f);
            AddRune("fire_rune_rare", "Inferno Rune", "Powerful fire with burn effect", RuneType.FireRune, RuneRarity.Rare, damageBonus: 18f, onHit: true);
            AddRune("fire_rune_epic", "Blazing Rune", "Devastating fire with area burn", RuneType.FireRune, RuneRarity.Epic, damageBonus: 28f, onHit: true);
            AddRune("fire_rune_legendary", "Phoenix Rune", "Legendary fire with explosive damage", RuneType.FireRune, RuneRarity.Legendary, damageBonus: 40f, onHit: true, onKill: true);
            AddRune("fire_rune_mythic", "Solar Rune", "Mythical fire embodying sun's power", RuneType.FireRune, RuneRarity.Mythic, damageBonus: 60f, critDamage: 25f, onHit: true, onKill: true);
            
            // Ice Runes
            AddRune("ice_rune_common", "Ice Rune", "Adds ice damage", RuneType.IceRune, RuneRarity.Common, damageBonus: 5f);
            AddRune("ice_rune_uncommon", "Frost Rune", "Ice with slow effect", RuneType.IceRune, RuneRarity.Uncommon, damageBonus: 10f, speedBonus: -5f);
            AddRune("ice_rune_rare", "Blizzard Rune", "Powerful ice with freezing", RuneType.IceRune, RuneRarity.Rare, damageBonus: 18f, speedBonus: -10f, onHit: true);
            AddRune("ice_rune_epic", "Glacial Rune", "Devastating ice that shatters", RuneType.IceRune, RuneRarity.Epic, damageBonus: 28f, speedBonus: -15f, onHit: true, onCrit: true);
            AddRune("ice_rune_legendary", "Absolute Zero Rune", "Legendary ice that freezes time", RuneType.IceRune, RuneRarity.Legendary, damageBonus: 40f, speedBonus: -25f, onHit: true, onCrit: true);
            AddRune("ice_rune_mythic", "Cosmic Freeze Rune", "Mythical ice from space", RuneType.IceRune, RuneRarity.Mythic, damageBonus: 60f, speedBonus: -35f, onHit: true, onCrit: true, onKill: true);
            
            // Lightning Runes
            AddRune("lightning_rune_common", "Lightning Rune", "Adds lightning damage", RuneType.LightningRune, RuneRarity.Common, damageBonus: 5f, speedBonus: 3f);
            AddRune("lightning_rune_uncommon", "Thunder Rune", "Lightning with chain effect", RuneType.LightningRune, RuneRarity.Uncommon, damageBonus: 10f, speedBonus: 5f);
            AddRune("lightning_rune_rare", "Storm Rune", "Lightning chains between targets", RuneType.LightningRune, RuneRarity.Rare, damageBonus: 18f, speedBonus: 8f, onHit: true);
            AddRune("lightning_rune_epic", "Tempest Rune", "Devastating lightning chains", RuneType.LightningRune, RuneRarity.Epic, damageBonus: 28f, speedBonus: 12f, onHit: true);
            AddRune("lightning_rune_legendary", "Zeus Rune", "Legendary lightning of thunder god", RuneType.LightningRune, RuneRarity.Legendary, damageBonus: 40f, speedBonus: 18f, critChance: 5f, onHit: true);
            AddRune("lightning_rune_mythic", "Thunderlord Rune", "Mythical lightning commanding storms", RuneType.LightningRune, RuneRarity.Mythic, damageBonus: 60f, speedBonus: 25f, critChance: 10f, onHit: true);
            
            // Defense Runes
            AddRune("shield_rune_common", "Shield Rune", "Minor shield protection", RuneType.ShieldRune, RuneRarity.Common, defenseBonus: 10f);
            AddRune("shield_rune_uncommon", "Barrier Rune", "Shield that absorbs damage", RuneType.ShieldRune, RuneRarity.Uncommon, defenseBonus: 20f);
            AddRune("shield_rune_rare", "Aegis Rune", "Powerful shield with regen", RuneType.ShieldRune, RuneRarity.Rare, defenseBonus: 35f, regen: 2f);
            AddRune("shield_rune_epic", "Divine Shield Rune", "Divine shield that blocks", RuneType.ShieldRune, RuneRarity.Epic, defenseBonus: 50f, regen: 5f, onDamaged: true);
            AddRune("shield_rune_legendary", "Titan Shield Rune", "Legendary shield of titans", RuneType.ShieldRune, RuneRarity.Legendary, defenseBonus: 70f, regen: 10f, onDamaged: true);
            AddRune("shield_rune_mythic", "Eternal Ward Rune", "Mythical shield that never breaks", RuneType.ShieldRune, RuneRarity.Mythic, defenseBonus: 100f, regen: 15f, onDamaged: true);
            
            // Health Runes
            AddRune("health_rune_common", "Health Rune", "Increases max health", RuneType.HealthRune, RuneRarity.Common, healthBonus: 50f);
            AddRune("health_rune_uncommon", "Vitality Rune", "Significant health increase", RuneType.HealthRune, RuneRarity.Uncommon, healthBonus: 100f);
            AddRune("health_rune_rare", "Life Rune", "Great health with regen", RuneType.HealthRune, RuneRarity.Rare, healthBonus: 200f, regen: 3f);
            AddRune("health_rune_epic", "Soul Rune", "Powerful health with regen", RuneType.HealthRune, RuneRarity.Epic, healthBonus: 350f, regen: 8f);
            AddRune("health_rune_legendary", "Dragon Heart Rune", "Legendary health with vitality", RuneType.HealthRune, RuneRarity.Legendary, healthBonus: 500f, regen: 15f);
            AddRune("health_rune_mythic", "World Tree Rune", "Mythical life force", RuneType.HealthRune, RuneRarity.Mythic, healthBonus: 800f, regen: 25f);
            
            // Critical Runes
            AddRune("crit_rune_common", "Critical Rune", "Increases crit chance", RuneType.CritRune, RuneRarity.Common, critChance: 2f);
            AddRune("crit_rune_uncommon", "Precision Rune", "Significant crit increase", RuneType.CritRune, RuneRarity.Uncommon, critChance: 5f);
            AddRune("crit_rune_rare", "Deadly Rune", "Great crit and damage", RuneType.CritRune, RuneRarity.Rare, critChance: 10f, critDamage: 15f);
            AddRune("crit_rune_epic", "Assassin Rune", "Devastating critical strikes", RuneType.CritRune, RuneRarity.Epic, critChance: 15f, critDamage: 30f, onCrit: true);
            AddRune("crit_rune_legendary", "Death Mark Rune", "Legendary marks enemies", RuneType.CritRune, RuneRarity.Legendary, critChance: 20f, critDamage: 50f, onCrit: true, onKill: true);
            AddRune("crit_rune_mythic", "Fate Rune", "Mythical controls fate", RuneType.CritRune, RuneRarity.Mythic, critChance: 30f, critDamage: 75f, onCrit: true, onKill: true);
            
            // Life Steal Runes
            AddRune("lifesteal_rune_common", "Vampiric Rune", "Life steal on hit", RuneType.LifeStealRune, RuneRarity.Common, lifeSteal: 3f);
            AddRune("lifesteal_rune_uncommon", "Blood Rune", "Stronger life steal", RuneType.LifeStealRune, RuneRarity.Uncommon, lifeSteal: 6f);
            AddRune("lifesteal_rune_rare", "Drain Rune", "Powerful life steal", RuneType.LifeStealRune, RuneRarity.Rare, lifeSteal: 10f, onHit: true);
            AddRune("lifesteal_rune_epic", "Sanguine Rune", "Life steal with area", RuneType.LifeStealRune, RuneRarity.Epic, lifeSteal: 15f, onHit: true, onKill: true);
            AddRune("lifesteal_rune_legendary", "Blood Lord Rune", "Legendary of blood lord", RuneType.LifeStealRune, RuneRarity.Legendary, lifeSteal: 22f, healthBonus: 200f, onHit: true, onKill: true);
            AddRune("lifesteal_rune_mythic", "Primordial Blood Rune", "Mythical ancient blood", RuneType.LifeStealRune, RuneRarity.Mythic, lifeSteal: 35f, healthBonus: 400f, onHit: true, onKill: true);
            
            // Speed Runes
            AddRune("speed_rune_common", "Swift Rune", "Increases movement", RuneType.SpeedRune, RuneRarity.Common, speedBonus: 5f);
            AddRune("speed_rune_uncommon", "Wind Rune", "Significant speed", RuneType.SpeedRune, RuneRarity.Uncommon, speedBonus: 10f);
            AddRune("speed_rune_rare", "Stormfoot Rune", "Great speed boost", RuneType.SpeedRune, RuneRarity.Rare, speedBonus: 18f);
            AddRune("speed_rune_epic", "Lightning Feet Rune", "Powerful speed", RuneType.SpeedRune, RuneRarity.Epic, speedBonus: 28f);
            AddRune("speed_rune_legendary", "Mercury Rune", "Legendary messenger god", RuneType.SpeedRune, RuneRarity.Legendary, speedBonus: 40f);
            AddRune("speed_rune_mythic", "Warp Rune", "Mythical bends space", RuneType.SpeedRune, RuneRarity.Mythic, speedBonus: 60f);
            
            // Mana Runes
            AddRune("mana_rune_common", "Mana Rune", "Increases max mana", RuneType.ManaRune, RuneRarity.Common, manaBonus: 30f);
            AddRune("mana_rune_uncommon", "Arcane Rune", "Significant mana", RuneType.ManaRune, RuneRarity.Uncommon, manaBonus: 60f);
            AddRune("mana_rune_rare", "Mystic Rune", "Mana with regen", RuneType.ManaRune, RuneRarity.Rare, manaBonus: 120f, regen: 2f);
            AddRune("mana_rune_epic", "Wizard Rune", "Powerful mana", RuneType.ManaRune, RuneRarity.Epic, manaBonus: 200f, regen: 5f);
            AddRune("mana_rune_legendary", "Archmage Rune", "Legendary of archmages", RuneType.ManaRune, RuneRarity.Legendary, manaBonus: 350f, regen: 10f);
            AddRune("mana_rune_mythic", "Cosmic Mind Rune", "Mythical cosmic energy", RuneType.ManaRune, RuneRarity.Mythic, manaBonus: 500f, regen: 18f);
            
            // Regeneration Runes
            AddRune("regen_rune_common", "Regen Rune", "Slow health regen", RuneType.RegenerationRune, RuneRarity.Common, regen: 2f);
            AddRune("regen_rune_uncommon", "Healing Rune", "Moderate regen", RuneType.RegenerationRune, RuneRarity.Uncommon, regen: 5f);
            AddRune("regen_rune_rare", "Rejuvenation Rune", "Strong regen", RuneType.RegenerationRune, RuneRarity.Rare, regen: 10f);
            AddRune("regen_rune_epic", "Restoration Rune", "Powerful scaling regen", RuneType.RegenerationRune, RuneRarity.Epic, regen: 18f);
            AddRune("regen_rune_legendary", "Phoenix Rebirth Rune", "Legendary resurrection", RuneType.RegenerationRune, RuneRarity.Legendary, regen: 30f, onDamaged: true);
            AddRune("regen_rune_mythic", "Eternal Life Rune", "Mythical immortality", RuneType.RegenerationRune, RuneRarity.Mythic, regen: 50f, healthBonus: 300f, onDamaged: true);
            
            // Poison Runes
            AddRune("poison_rune_common", "Poison Rune", "Adds poison damage", RuneType.PoisonRune, RuneRarity.Common, damageBonus: 5f);
            AddRune("poison_rune_uncommon", "Venom Rune", "Poison with DoT", RuneType.PoisonRune, RuneRarity.Uncommon, damageBonus: 10f, onHit: true);
            AddRune("poison_rune_rare", "Toxic Rune", "Powerful weakens", RuneType.PoisonRune, RuneRarity.Rare, damageBonus: 18f, onHit: true);
            AddRune("poison_rune_epic", "Plague Rune", "Poison spreads", RuneType.PoisonRune, RuneRarity.Epic, damageBonus: 28f, onHit: true, onKill: true);
            AddRune("poison_rune_legendary", "Pestilence Rune", "Legendary ancient plague", RuneType.PoisonRune, RuneRarity.Legendary, damageBonus: 40f, onHit: true, onKill: true);
            AddRune("poison_rune_mythic", "Biohazard Rune", "Mythical cosmic biohazard", RuneType.PoisonRune, RuneRarity.Mythic, damageBonus: 60f, onHit: true, onKill: true);
            
            // Holy Runes
            AddRune("holy_rune_common", "Holy Rune", "Adds holy damage", RuneType.HolyRune, RuneRarity.Common, damageBonus: 5f);
            AddRune("holy_rune_uncommon", "Sacred Rune", "Holy with healing", RuneType.HolyRune, RuneRarity.Uncommon, damageBonus: 10f, regen: 2f);
            AddRune("holy_rune_rare", "Divine Rune", "Holy with strong healing", RuneType.HolyRune, RuneRarity.Rare, damageBonus: 18f, regen: 5f);
            AddRune("holy_rune_epic", "Celestial Rune", "Holy with healing", RuneType.HolyRune, RuneRarity.Epic, damageBonus: 28f, regen: 10f, onHit: true, onDamaged: true);
            AddRune("holy_rune_legendary", "Seraph Rune", "Legendary of seraphim", RuneType.HolyRune, RuneRarity.Legendary, damageBonus: 40f, regen: 18f, healthBonus: 200f, onHit: true, onDamaged: true);
            AddRune("holy_rune_mythic", "Heavenly Host Rune", "Mythical celestial forces", RuneType.HolyRune, RuneRarity.Mythic, damageBonus: 60f, regen: 30f, healthBonus: 400f, onHit: true, onDamaged: true, onKill: true);
            
            // Dark Runes
            AddRune("dark_rune_common", "Dark Rune", "Adds dark damage", RuneType.DarkRune, RuneRarity.Common, damageBonus: 5f);
            AddRune("dark_rune_uncommon", "Shadow Rune", "Dark with life drain", RuneType.DarkRune, RuneRarity.Uncommon, damageBonus: 10f, lifeSteal: 3f);
            AddRune("dark_rune_rare", "Void Rune", "Powerful dark energy", RuneType.DarkRune, RuneRarity.Rare, damageBonus: 18f, lifeSteal: 5f, onHit: true);
            AddRune("dark_rune_epic", "Abyss Rune", "Devastating void power", RuneType.DarkRune, RuneRarity.Epic, damageBonus: 28f, lifeSteal: 8f, onHit: true, onKill: true);
            AddRune("dark_rune_legendary", "Demon Lord Rune", "Legendary of demon lord", RuneType.DarkRune, RuneRarity.Legendary, damageBonus: 40f, lifeSteal: 12f, healthBonus: 200f, onHit: true, onKill: true);
            AddRune("dark_rune_mythic", "Chaos Rune", "Mythical primordial chaos", RuneType.DarkRune, RuneRarity.Mythic, damageBonus: 60f, lifeSteal: 18f, healthBonus: 400f, onHit: true, onKill: true);
            
            // Special Runes
            AddRune("invuln_rune_legendary", "Divine Blessing Rune", "Temporary invulnerability", RuneType.InvulnerabilityRune, RuneRarity.Legendary, defenseBonus: 50f, onDamaged: true);
            AddRune("invuln_rune_mythic", "God Mode Rune", "Near-godlike protection", RuneType.InvulnerabilityRune, RuneRarity.Mythic, defenseBonus: 100f, healthBonus: 500f, onDamaged: true, onKill: true);
            AddRune("teleport_rune_legendary", "Portal Rune", "Enables teleportation", RuneType.TeleportRune, RuneRarity.Legendary, speedBonus: 20f, onHit: true);
            AddRune("teleport_rune_mythic", "Dimension Warp Rune", "Warps dimensional barriers", RuneType.TeleportRune, RuneRarity.Mythic, speedBonus: 35f, onHit: true, onKill: true);
        }
        
        private static void AddRune(string id, string name, string desc, RuneType type, RuneRarity rarity, 
            float damageBonus = 0, float defenseBonus = 0, float healthBonus = 0, float manaBonus = 0,
            float speedBonus = 0, float critChance = 0, float critDamage = 0, float lifeSteal = 0,
            float regen = 0, bool onHit = false, bool onKill = false, bool onDamaged = false, bool onCrit = false)
        {
            int[] rarityPowers = { 10, 25, 50, 100, 200, 500 };
            int power = rarityPowers[(int)rarity];
            
            Runes[id] = new Rune
            {
                Id = id,
                Name = name,
                Description = desc,
                Type = type,
                Rarity = rarity,
                DamageBonus = damageBonus,
                DefenseBonus = defenseBonus,
                HealthBonus = healthBonus,
                ManaBonus = manaBonus,
                SpeedBonus = speedBonus,
                CritChance = critChance,
                CritDamage = critDamage,
                LifeSteal = lifeSteal,
                Regen = regen,
                OnHitEffect = onHit,
                OnKillEffect = onKill,
                OnDamagedEffect = onDamaged,
                OnCriticalEffect = onCrit,
                LevelRequired = (int)rarity * 10 + 1,
                Power = power
            };
        }
        
        public static Rune GetRune(string id)
        {
            return Runes.TryGetValue(id, out var rune) ? rune : null;
        }
        
        public static List<Rune> GetRunesByType(RuneType type)
        {
            return Runes.Values.Where(r => r.Type == type).OrderBy(r => r.Rarity).ToList();
        }
        
        public static List<Rune> GetRunesByRarity(RuneRarity rarity)
        {
            return Runes.Values.Where(r => r.Rarity == rarity).OrderBy(r => r.Type).ToList();
        }
        
        public static List<Rune> GetAllRunes()
        {
            return Runes.Values.OrderBy(r => r.Type).ThenBy(r => r.Rarity).ToList();
        }
        
        public static int GetTotalRuneCount() => Runes.Count;
        
        public static int GetRarityCount(RuneRarity rarity)
        {
            return Runes.Values.Count(r => r.Rarity == rarity);
        }
    }
}
