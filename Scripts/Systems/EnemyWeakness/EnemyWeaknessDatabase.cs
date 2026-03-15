using Godot;
using System;
using System.Collections.Generic;

public class EnemyWeaknessDatabase : BaseSystem
{
    // 弱点类型
    public enum WeaknessType
    {
        Elemental,
        Physical,
        StatusEffect,
        CriticalSpot
    }

    // 元素类型
    public enum ElementType
    {
        Fire,
        Ice,
        Lightning,
        Water,
        Holy,
        Dark,
        Physical,
        Nature,
        Wind
    }

    // 弱点配置
    [System.Serializable]
    public class WeaknessConfig
    {
        public string ID;
        public WeaknessType Type;
        public ElementType Element;
        public float DamageMultiplier = 1.5f;      // 弱点伤害倍率
        public float ResistanceMultiplier = 0.5f;  // 抗性倍率
        public string Description = "";
    }

    // 敌人弱点配置
    [System.Serializable]
    public class EnemyWeaknessConfig
    {
        public string EnemyType;
        public List<string> WeaknessIDs = new List<string>();
        public List<string> ResistanceIDs = new List<string>();
        public string CriticalSpotHint = "";
    }

    // 所有弱点配置
    public Dictionary<string, WeaknessConfig> AllWeaknesses = new Dictionary<string, WeaknessConfig>();

    // 敌人弱点配置
    public Dictionary<string, EnemyWeaknessConfig> EnemyWeaknessConfigs = new Dictionary<string, EnemyWeaknessConfig>();

    public override void _Ready()
    {
        InitializeDatabase();
    }

    public void InitializeDatabase()
    {
        // 初始化元素弱点配置
        InitializeElementalWeaknesses();

        // 初始化物理弱点配置
        InitializePhysicalWeaknesses();

        // 初始化状态异常弱点配置
        InitializeStatusEffectWeaknesses();

        // 初始化敌人弱点配置
        InitializeEnemyWeaknessConfigs();
    }

    private void InitializeElementalWeaknesses()
    {
        // 火弱点
        AllWeaknesses["fire_weak"] = new WeaknessConfig
        {
            ID = "fire_weak",
            Type = WeaknessType.Elemental,
            Element = ElementType.Fire,
            DamageMultiplier = 1.5f,
            ResistanceMultiplier = 0.5f,
            Description = "受到火属性伤害增加 50%"
        };

        // 冰弱点
        AllWeaknesses["ice_weak"] = new WeaknessConfig
        {
            ID = "ice_weak",
            Type = WeaknessType.Elemental,
            Element = ElementType.Ice,
            DamageMultiplier = 1.5f,
            ResistanceMultiplier = 0.5f,
            Description = "受到冰属性伤害增加 50%"
        };

        // 雷弱点
        AllWeaknesses["lightning_weak"] = new WeaknessConfig
        {
            ID = "lightning_weak",
            Type = WeaknessType.Elemental,
            Element = ElementType.Lightning,
            DamageMultiplier = 1.5f,
            ResistanceMultiplier = 0.5f,
            Description = "受到雷属性伤害增加 50%"
        };

        // 圣弱点
        AllWeaknesses["holy_weak"] = new WeaknessConfig
        {
            ID = "holy_weak",
            Type = WeaknessType.Elemental,
            Element = ElementType.Holy,
            DamageMultiplier = 1.75f,
            ResistanceMultiplier = 0.3f,
            Description = "受到圣属性伤害增加 75%"
        };

        // 暗弱点
        AllWeaknesses["dark_weak"] = new WeaknessConfig
        {
            ID = "dark_weak",
            Type = WeaknessType.Elemental,
            Element = ElementType.Dark,
            DamageMultiplier = 1.75f,
            ResistanceMultiplier = 0.3f,
            Description = "受到暗属性伤害增加 75%"
        };

        // 元素抗性
        AllWeaknesses["fire_resist"] = new WeaknessConfig
        {
            ID = "fire_resist",
            Type = WeaknessType.Elemental,
            Element = ElementType.Fire,
            DamageMultiplier = 0.5f,
            ResistanceMultiplier = 1.5f,
            Description = "火属性抗性 +50%"
        };

        AllWeaknesses["ice_resist"] = new WeaknessConfig
        {
            ID = "ice_resist",
            Type = WeaknessType.Elemental,
            Element = ElementType.Ice,
            DamageMultiplier = 0.5f,
            ResistanceMultiplier = 1.5f,
            Description = "冰属性抗性 +50%"
        };
    }

    private void InitializePhysicalWeaknesses()
    {
        // 斩击弱点
        AllWeaknesses["slash_weak"] = new WeaknessConfig
        {
            ID = "slash_weak",
            Type = WeaknessType.Physical,
            Element = ElementType.Physical,
            DamageMultiplier = 1.5f,
            ResistanceMultiplier = 0.5f,
            Description = "斩击伤害增加 50%"
        };

        // 打击弱点
        AllWeaknesses["blunt_weak"] = new WeaknessConfig
        {
            ID = "blunt_weak",
            Type = WeaknessType.Physical,
            Element = ElementType.Physical,
            DamageMultiplier = 1.5f,
            ResistanceMultiplier = 0.5f,
            Description = "打击伤害增加 50%"
        };

        // 穿刺弱点
        AllWeaknesses["pierce_weak"] = new WeaknessConfig
        {
            ID = "pierce_weak",
            Type = WeaknessType.Physical,
            Element = ElementType.Physical,
            DamageMultiplier = 1.5f,
            ResistanceMultiplier = 0.5f,
            Description = "穿刺伤害增加 50%"
        };
    }

    private void InitializeStatusEffectWeaknesses()
    {
        // 燃烧易伤
        AllWeaknesses["burn_vulnerable"] = new WeaknessConfig
        {
            ID = "burn_vulnerable",
            Type = WeaknessType.StatusEffect,
            Element = ElementType.Fire,
            DamageMultiplier = 1.5f,
            ResistanceMultiplier = 0.5f,
            Description = "燃烧持续伤害增加 50%"
        };

        // 冰冻易伤
        AllWeaknesses["freeze_vulnerable"] = new WeaknessConfig
        {
            ID = "freeze_vulnerable",
            Type = WeaknessType.StatusEffect,
            Element = ElementType.Ice,
            DamageMultiplier = 1.5f,
            ResistanceMultiplier = 0.5f,
            Description = "冰冻持续时间增加 50%"
        };

        // 感电易伤
        AllWeaknesses["shock_vulnerable"] = new WeaknessConfig
        {
            ID = "shock_vulnerable",
            Type = WeaknessType.StatusEffect,
            Element = ElementType.Lightning,
            DamageMultiplier = 1.5f,
            ResistanceMultiplier = 0.5f,
            Description = "感电持续伤害增加 50%"
        };

        // 中毒易伤
        AllWeaknesses["poison_vulnerable"] = new WeaknessConfig
        {
            ID = "poison_vulnerable",
            Type = WeaknessType.StatusEffect,
            Element = ElementType.Nature,
            DamageMultiplier = 1.5f,
            ResistanceMultiplier = 0.5f,
            Description = "中毒持续伤害增加 50%"
        };
    }

    private void InitializeEnemyWeaknessConfigs()
    {
        // 火焰元素敌人 - 弱冰抗火
        EnemyWeaknessConfigs["FireElemental"] = new EnemyWeaknessConfig
        {
            EnemyType = "FireElemental",
            WeaknessIDs = new List<string> { "ice_weak", "water_weak", "freeze_vulnerable" },
            ResistanceIDs = new List<string> { "fire_resist" },
            CriticalSpotHint = "核心是弱点"
        };

        // 冰霜元素敌人 - 弱火抗冰
        EnemyWeaknessConfigs["IceElemental"] = new EnemyWeaknessConfig
        {
            EnemyType = "IceElemental",
            WeaknessIDs = new List<string> { "fire_weak", "burn_vulnerable" },
            ResistanceIDs = new List<string> { "ice_resist" },
            CriticalSpotHint = "冰晶核心"
        };

        // 雷电元素敌人 - 弱水
        EnemyWeaknessConfigs["LightningElemental"] = new EnemyWeaknessConfig
        {
            EnemyType = "LightningElemental",
            WeaknessIDs = new List<string> { "water_weak", "shock_vulnerable" },
            ResistanceIDs = new List<string>(),
            CriticalSpotHint = "电能节点"
        };

        // 暗影敌人 - 弱圣
        EnemyWeaknessConfigs["ShadowCreature"] = new EnemyWeaknessConfig
        {
            EnemyType = "ShadowCreature",
            WeaknessIDs = new List<string> { "holy_weak" },
            ResistanceIDs = new List<string> { "dark_weak" },
            CriticalSpotHint = "光明照亮的部位"
        };

        // 神圣敌人 - 弱暗
        EnemyWeaknessConfigs["HolyCreature"] = new EnemyWeaknessConfig
        {
            EnemyType = "HolyCreature",
            WeaknessIDs = new List<string> { "dark_weak" },
            ResistanceIDs = new List<string> { "holy_weak" },
            CriticalSpotHint = "神圣光环"
        };

        // 机械敌人 - 弱雷电
        EnemyWeaknessConfigs["Mechanical"] = new EnemyWeaknessConfig
        {
            EnemyType = "Mechanical",
            WeaknessIDs = new List<string> { "lightning_weak", "shock_vulnerable" },
            ResistanceIDs = new List<string>(),
            CriticalSpotHint = "电路节点"
        };

        // 亡灵敌人 - 弱圣
        EnemyWeaknessConfigs["Undead"] = new EnemyWeaknessConfig
        {
            EnemyType = "Undead",
            WeaknessIDs = new List<string> { "holy_weak", "fire_weak" },
            ResistanceIDs = new List<string> { "poison_vulnerable" },
            CriticalSpotHint = "灵魂之火"
        };

        // 动物敌人 - 弱物理
        EnemyWeaknessConfigs["Beast"] = new EnemyWeaknessConfig
        {
            EnemyType = "Beast",
            WeaknessIDs = new List<string> { "slash_weak", "pierce_weak" },
            ResistanceIDs = new List<string>(),
            CriticalSpotHint = "心脏或咽喉"
        };

        // 装甲敌人 - 弱打击
        EnemyWeaknessConfigs["Armored"] = new EnemyWeaknessConfig
        {
            EnemyType = "Armored",
            WeaknessIDs = new List<string> { "blunt_weak" },
            ResistanceIDs = new List<string> { "slash_weak", "pierce_weak" },
            CriticalSpotHint = "装甲缝隙"
        };

        // 飞行敌人 - 弱穿刺
        EnemyWeaknessConfigs["Flying"] = new EnemyWeaknessConfig
        {
            EnemyType = "Flying",
            WeaknessIDs = new List<string> { "pierce_weak" },
            ResistanceIDs = new List<string>(),
            CriticalSpotHint = "翅膀关节"
        };
    }

    public WeaknessConfig GetWeaknessConfig(string id)
    {
        if (AllWeaknesses.ContainsKey(id))
            return AllWeaknesses[id];
        return null;
    }

    public EnemyWeaknessConfig GetEnemyWeaknessConfig(string enemyType)
    {
        if (EnemyWeaknessConfigs.ContainsKey(enemyType))
            return EnemyWeaknessConfigs[enemyType];
        return null;
    }

    public List<WeaknessConfig> GetEnemyWeaknesses(string enemyType)
    {
        var config = GetEnemyWeaknessConfig(enemyType);
        if (config == null) return new List<WeaknessConfig>();

        var weaknesses = new List<WeaknessConfig>();
        foreach (var id in config.WeaknessIDs)
        {
            var weakness = GetWeaknessConfig(id);
            if (weakness != null)
                weaknesses.Add(weakness);
        }
        return weaknesses;
    }

        public override Dictionary ExportSaveData() => new();
        public override void ImportSaveData(Dictionary data) { }
}
