namespace ClawRPG.Systems
{
    /// <summary>
    /// Rune types with different effects
    /// </summary>
    public enum RuneType
    {
        // Damage runes
        FireRune,
        IceRune,
        LightningRune,
        PoisonRune,
        HolyRune,
        DarkRune,
        
        // Defense runes
        ShieldRune,
        ArmorRune,
        HealthRune,
        ResistRune,
        
        // Utility runes
        SpeedRune,
        CritRune,
        ManaRune,
        StealthRune,
        
        // Special runes
        LifeStealRune,
        TeleportRune,
        InvulnerabilityRune,
        RegenerationRune
    }

    /// <summary>
    /// Rune rarity levels
    /// </summary>
    public enum RuneRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    /// <summary>
    /// Rune slot types for different equipment
    /// </summary>
    public enum RuneSlotType
    {
        Weapon,
        Armor,
        Accessory,
        Helmet,
        Boots,
        Gloves
    }

    /// <summary>
    /// Individual rune data
    /// </summary>
    public class Rune
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RuneType Type { get; set; }
        public RuneRarity Rarity { get; set; }
        
        // Effect values
        public float DamageBonus { get; set; }
        public float DefenseBonus { get; set; }
        public float HealthBonus { get; set; }
        public float ManaBonus { get; set; }
        public float SpeedBonus { get; set; }
        public float CritChance { get; set; }
        public float CritDamage { get; set; }
        public float LifeSteal { get; set; }
        public float Regen { get; set; }
        public float DamageReflect { get; set; }
        public float ElementalResist { get; set; }
        
        // Special effects
        public bool OnHitEffect { get; set; }
        public bool OnKillEffect { get; set; }
        public bool OnDamagedEffect { get; set; }
        public bool OnCriticalEffect { get; set; }
        
        public int LevelRequired { get; set; }
        public int Power { get; set; }
    }

    /// <summary>
    /// Rune slot on equipment
    /// </summary>
    public class RuneSlot
    {
        public RuneSlotType SlotType { get; set; }
        public int SlotIndex { get; set; }
        public Rune Rune { get; set; }
        public bool IsUnlocked { get; set; }
    }

    /// <summary>
    /// Player's rune collection
    /// </summary>
    public class PlayerRuneData
    {
        public List<Rune> OwnedRunes { get; set; }
        public List<Rune> EquippedRunes { get; set; }
        public Dictionary<string, int> RuneCount { get; set; }
        public int TotalRunesCrafted { get; set; }
        public int TotalRunesDiscovered { get; set; }
        
        public PlayerRuneData()
        {
            OwnedRunes = new List<Rune>();
            EquippedRunes = new List<Rune>();
            RuneCount = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Rune synthesis recipe
    /// </summary>
    public class RuneRecipe
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Rune ResultRune { get; set; }
        public List<RuneRecipeIngredient> Ingredients { get; set; }
        public int SuccessRate { get; set; }
        
        public RuneRecipe()
        {
            Ingredients = new List<RuneRecipeIngredient>();
        }
    }

    public class RuneRecipeIngredient
    {
        public string RuneId { get; set; }
        public int Count { get; set; }
        public RuneRarity RequiredRarity { get; set; }
    }

    /// <summary>
    /// Rune collection statistics
    /// </summary>
    public class RuneStatistics
    {
        public int TotalRunesOwned { get; set; }
        public int UniqueRunes { get; set; }
        public Dictionary<RuneRarity, int> RarityBreakdown { get; set; }
        public Dictionary<RuneType, int> TypeBreakdown { get; set; }
        public int TotalCrafted { get; set; }
        public int SuccessfulCrafts { get; set; }
        
        public RuneStatistics()
        {
            RarityBreakdown = new Dictionary<RuneRarity, int>();
            TypeBreakdown = new Dictionary<RuneType, int>();
        }
    }
}
