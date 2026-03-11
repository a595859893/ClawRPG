namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// 符文稀有度等级
    /// </summary>
    public enum RuneRarity
    {
        Common,     // 普通
        Uncommon,  // 优秀
        Rare,      // 稀有
        Epic,      // 史诗
        Legendary  // 传说
    }

    /// <summary>
    /// 符文类型
    /// </summary>
    public enum RuneType
    {
        Attack,     // 攻击
        Defense,   // 防御
        Health,    // 生命
        Speed,     // 速度
        Critical,  // 暴击
        Magic,     // 魔法
        LifeSteal, // 生命偷取
        Dodge      // 闪避
    }

    /// <summary>
    /// 符文数据
    /// </summary>
    public class Rune
    {
        public string Id;
        public string Name;
        public RuneType Type;
        public RuneRarity Rarity;
        public int Level;
        public float AttributeValue;
        public bool IsEquipped;
    }

    /// <summary>
    /// 符文实例数据
    /// </summary>
    public class RuneInstance
    {
        public string UniqueId;
        public string RuneId;
        public int SlotIndex;
        public bool IsLocked;
    }

    /// <summary>
    /// 符文套装数据
    /// </summary>
    public class RuneSet
    {
        public string Id;
        public string Name;
        public int[] RuneTypeCounts; // 每个类型需要的数量
        public float[] BonusAttributes; // 2件/4件/6件加成
    }

    /// <summary>
    /// 玩家符文数据
    /// </summary>
    public class PlayerRuneData
    {
        public List<RuneInstance> OwnedRunes = new List<RuneInstance>();
        public Dictionary<string, int> EquippedRunes = new Dictionary<string, int>(); // Type -> SlotIndex
        public HashSet<string> DiscoveredRunes = new HashSet<string>();
        public int TotalRunesFound;
        public int TotalRuneUpgrades;
    }
}
