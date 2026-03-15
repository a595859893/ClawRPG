using Godot;
using System;
using System.Collections.Generic;
using Framework;

public class ClassData
{
    public enum ClassType
    {
        Warrior,      // 战士 - 高生命高防御
        Mage,         // 法师 - 高魔法高爆发
        Rogue,        // 刺客 - 高闪避高暴击
        Ranger,       // 游侠 - 远程物理输出
        Paladin,      // 圣骑士 - 攻防平衡
        Necromancer,  // 死灵法师 - 召唤和控制
        Druid,        // 德鲁伊 - 变形和自然
        Bard          // 吟游诗人 - 辅助和增益
    }

    public enum ClassTier
    {
        Novice,    // 初级
        Adept,     // 熟练
        Master,    // 大师
        Legend     // 传奇
    }

    public ClassType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ClassTier Tier { get; set; }
    public int LevelRequired { get; set; }
    public int PrestigeCost { get; set; }  // 转职所需声望
    
    // 基础属性加成
    public int BaseHealthBonus { get; set; }
    public int BaseAttackBonus { get; set; }
    public int BaseDefenseBonus { get; set; }
    public int BaseMagicBonus { get; set; }
    public int BaseSpeedBonus { get; set; }
    public int BaseLuckBonus { get; set; }
    
    // 被动技能ID列表
    public List<string> PassiveSkills { get; set; }
    
    // 主动技能ID列表
    public List<string> ActiveSkills { get; set; }
    
    // 进阶职业
    public ClassType? AdvancedClass { get; set; }
    public ClassType? SecondaryClass { get; set; }  // 第二职业
    
    public ClassData()
    {
        PassiveSkills = new List<string>();
        ActiveSkills = new List<string>();
    }
}

public class ClassSystem : BaseSystem
{
    // 单例
    private static ClassSystem _instance;
    public static ClassSystem Instance => _instance;

    // 职业数据
    private Dictionary<ClassData.ClassType, ClassData> _classes;
    private Dictionary<string, ClassData> _classNameMap;
    
    // 玩家职业数据
    private ClassData.ClassType _currentClass = ClassData.ClassType.Warrior;
    private ClassData.ClassType? _secondaryClass;
    private int _classLevel = 1;
    private int _classExperience = 0;
    private int _totalClassLevels = 0;  // 累计职业等级
    private ClassData.ClassTier _currentTier = ClassData.ClassTier.Novice;
    
    // 属性加成
    private int _healthBonus = 0;
    private int _attackBonus = 0;
    private int _defenseBonus = 0;
    private int _magicBonus = 0;
    private int _speedBonus = 0;
    private int _luckBonus = 0;
    
    // 经验需求
    private int[] _experienceThresholds = { 0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500, 5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000 };
    
    public override void _Ready()
    {
        _instance = this;
        InitializeClasses();
    }

    private void InitializeClasses()
    {
        _classes = new Dictionary<ClassData.ClassType, ClassData>();
        _classNameMap = new Dictionary<string, ClassData>();
        
        // 战士 Warrior
        var warrior = new ClassData
        {
            Type = ClassData.ClassType.Warrior,
            Name = "战士",
            Description = "近战物理职业，拥有高生命值和防御力，擅长使用剑盾。",
            Tier = ClassData.ClassTier.Novice,
            LevelRequired = 1,
            BaseHealthBonus = 50,
            BaseAttackBonus = 10,
            BaseDefenseBonus = 15,
            BaseMagicBonus = 0,
            BaseSpeedBonus = 5,
            BaseLuckBonus = 0,
            PassiveSkills = new List<string> { "warrior_passive_1", "warrior_passive_2" },
            ActiveSkills = new List<string> { "warrior_shield_bash", "warrior_battle_cry" },
            AdvancedClass = ClassData.ClassType.Paladin
        };
        _classes[ClassData.ClassType.Warrior] = warrior;
        _classNameMap["战士"] = warrior;
        
        // 法师 Mage
        var mage = new ClassData
        {
            Type = ClassData.ClassType.Mage,
            Name = "法师",
            Description = "远程魔法职业，拥有高魔法伤害和范围攻击能力。",
            Tier = ClassData.ClassTier.Novice,
            LevelRequired = 1,
            BaseHealthBonus = 20,
            BaseAttackBonus = 5,
            BaseDefenseBonus = 5,
            BaseMagicBonus = 20,
            BaseSpeedBonus = 10,
            BaseLuckBonus = 5,
            PassiveSkills = new List<string> { "mage_passive_1", "mage_passive_2" },
            ActiveSkills = new List<string> { "mage_fireball", "mage_teleport" },
            AdvancedClass = ClassData.ClassType.Necromancer
        };
        _classes[ClassData.ClassType.Mage] = mage;
        _classNameMap["法师"] = mage;
        
        // 刺客 Rogue
        var rogue = new ClassData
        {
            Type = ClassData.ClassType.Rogue,
            Name = "刺客",
            Description = "近战敏捷职业，拥有高闪避和暴击率，擅长暗中击杀。",
            Tier = ClassData.ClassTier.Novice,
            LevelRequired = 1,
            BaseHealthBonus = 25,
            BaseAttackBonus = 15,
            BaseDefenseBonus = 5,
            BaseMagicBonus = 0,
            BaseSpeedBonus = 20,
            BaseLuckBonus = 10,
            PassiveSkills = new List<string> { "rogue_passive_1", "rogue_passive_2" },
            ActiveSkills = new List<string> { "rogue_backstab", "rogue_smoke_bomb" },
            AdvancedClass = null
        };
        _classes[ClassData.ClassType.Rogue] = rogue;
        _classNameMap["刺客"] = rogue;
        
        // 游侠 Ranger
        var ranger = new ClassData
        {
            Type = ClassData.ClassType.Ranger,
            Name = "游侠",
            Description = "远程物理职业，擅长弓箭和陷阱，机动性强。",
            Tier = ClassData.ClassTier.Novice,
            LevelRequired = 1,
            BaseHealthBonus = 30,
            BaseAttackBonus = 15,
            BaseDefenseBonus = 8,
            BaseMagicBonus = 5,
            BaseSpeedBonus = 15,
            BaseLuckBonus = 5,
            PassiveSkills = new List<string> { "ranger_passive_1", "ranger_passive_2" },
            ActiveSkills = new List<string> { "ranger_aimed_shot", "ranger_trap" },
            AdvancedClass = null
        };
        _classes[ClassData.ClassType.Ranger] = ranger;
        _classNameMap["游侠"] = ranger;
        
        // 圣骑士 Paladin
        var paladin = new ClassData
        {
            Type = ClassData.ClassType.Paladin,
            Name = "圣骑士",
            Description = "神圣系职业，攻防兼备，能治疗队友。",
            Tier = ClassData.ClassTier.Adept,
            LevelRequired = 20,
            BaseHealthBonus = 40,
            BaseAttackBonus = 10,
            BaseDefenseBonus = 12,
            BaseMagicBonus = 10,
            BaseSpeedBonus = 5,
            BaseLuckBonus = 5,
            PassiveSkills = new List<string> { "paladin_passive_1", "paladin_passive_2" },
            ActiveSkills = new List<string> { "paladin_holy_strike", "paladin_lay_on_hands" },
            AdvancedClass = null
        };
        _classes[ClassData.ClassType.Paladin] = paladin;
        _classNameMap["圣骑士"] = paladin;
        
        // 死灵法师 Necromancer
        var necromancer = new ClassData
        {
            Type = ClassData.ClassType.Necromancer,
            Name = "死灵法师",
            Description = "黑暗系职业，擅长召唤亡灵和控制敌人。",
            Tier = ClassData.ClassTier.Adept,
            LevelRequired = 20,
            BaseHealthBonus = 25,
            BaseAttackBonus = 8,
            BaseDefenseBonus = 5,
            BaseMagicBonus = 25,
            BaseSpeedBonus = 8,
            BaseLuckBonus = 5,
            PassiveSkills = new List<string> { "necromancer_passive_1", "necromancer_passive_2" },
            ActiveSkills = new List<string> { "necromancer_summon_undead", "necromancer_life_drain" },
            AdvancedClass = null
        };
        _classes[ClassData.ClassType.Necromancer] = necromancer;
        _classNameMap["死灵法师"] = necromancer;
        
        // 德鲁伊 Druid
        var druid = new ClassData
        {
            Type = ClassData.ClassType.Druid,
            Name = "德鲁伊",
            Description = "自然系职业，能变身为动物形态，擅长治疗和召唤。",
            Tier = ClassData.ClassTier.Adept,
            LevelRequired = 20,
            BaseHealthBonus = 35,
            BaseAttackBonus = 8,
            BaseDefenseBonus = 10,
            BaseMagicBonus = 15,
            BaseSpeedBonus = 12,
            BaseLuckBonus = 8,
            PassiveSkills = new List<string> { "druid_passive_1", "druid_passive_2" },
            ActiveSkills = new List<string> { "druid_transform_bear", "druid_healing_spirit" },
            AdvancedClass = null
        };
        _classes[ClassData.ClassType.Druid] = druid;
        _classNameMap["德鲁伊"] = druid;
        
        // 吟游诗人 Bard
        var bard = new ClassData
        {
            Type = ClassData.ClassType.Bard,
            Name = "吟游诗人",
            Description = "辅助系职业，通过音乐为队友提供增益效果。",
            Tier = ClassData.ClassTier.Adept,
            LevelRequired = 20,
            BaseHealthBonus = 25,
            BaseAttackBonus = 10,
            BaseDefenseBonus = 8,
            BaseMagicBonus = 12,
            BaseSpeedBonus = 15,
            BaseLuckBonus = 15,
            PassiveSkills = new List<string> { "bard_passive_1", "bard_passive_2" },
            ActiveSkills = new List<string> { "bard_inspiring_song", "bard_charm" },
            AdvancedClass = null
        };
        _classes[ClassData.ClassType.Bard] = bard;
        _classNameMap["吟游诗人"] = bard;
        
        // 传奇职业 - 从大师职业进阶
        AddLegendClasses();
        
        UpdateBonuses();
    }
    
    private void AddLegendClasses()
    {
        // 传奇战士 - 战王
        var warLord = new ClassData
        {
            Type = ClassData.ClassType.Warrior,
            Name = "战王",
            Description = "战士的终极形态，战场上的王者。",
            Tier = ClassData.ClassTier.Legend,
            LevelRequired = 50,
            BaseHealthBonus = 100,
            BaseAttackBonus = 30,
            BaseDefenseBonus = 35,
            BaseMagicBonus = 5,
            BaseSpeedBonus = 10,
            BaseLuckBonus = 10,
            PassiveSkills = new List<string> { "warlord_passive_1", "warlord_passive_2", "warlord_passive_3" },
            ActiveSkills = new List<string> { "warlord_blade_storm", "warlord_battle_frenzy" },
            AdvancedClass = null
        };
        
        // 传奇法师 - 大法师
        var archMage = new ClassData
        {
            Type = ClassData.ClassType.Mage,
            Name = "大法师",
            Description = "法师的终极形态，掌控元素奥秘。",
            Tier = ClassData.ClassTier.Legend,
            LevelRequired = 50,
            BaseHealthBonus = 40,
            BaseAttackBonus = 10,
            BaseDefenseBonus = 10,
            BaseMagicBonus = 45,
            BaseSpeedBonus = 15,
            BaseLuckBonus = 15,
            PassiveSkills = new List<string> { "archmage_passive_1", "archmage_passive_2", "archmage_passive_3" },
            ActiveSkills = new List<string> { "archmage_meteor", "archmage_time_stop" },
            AdvancedClass = null
        };
    }

    public void GainClassExperience(int amount)
    {
        _classExperience += amount;
        
        // 检查升级
        while (_classLevel < _experienceThresholds.Length && _classExperience >= _experienceThresholds[_classLevel])
        {
            _classExperience -= _experienceThresholds[_classLevel];
            _classLevel++;
            _totalClassLevels++;
            UpdateBonuses();
            GD.Print($"[ClassSystem] 职业等级提升到 {_classLevel}!");
        }
        
        // 检查进阶
        CheckTierEvolution();
    }
    
    private void CheckTierEvolution()
    {
        var currentClassData = GetCurrentClassData();
        if (currentClassData == null) return;
        
        ClassData.ClassTier newTier = ClassData.ClassTier.Novice;
        
        if (_classLevel >= 50)
            newTier = ClassData.ClassTier.Legend;
        else if (_classLevel >= 30)
            newTier = ClassData.ClassTier.Master;
        else if (_classLevel >= 15)
            newTier = ClassData.ClassTier.Adept;
        
        if (newTier != _currentTier)
        {
            _currentTier = newTier;
            GD.Print($"[ClassSystem] 职业阶级提升到 {newTier}!");
        }
    }

    private void UpdateBonuses()
    {
        var classData = GetCurrentClassData();
        if (classData == null) return;
        
        // 基础加成
        _healthBonus = classData.BaseHealthBonus + (_classLevel - 1) * 5;
        _attackBonus = classData.BaseAttackBonus + (_classLevel - 1) * 2;
        _defenseBonus = classData.BaseDefenseBonus + (_classLevel - 1) * 2;
        _magicBonus = classData.BaseMagicBonus + (_classLevel - 1) * 2;
        _speedBonus = classData.BaseSpeedBonus + (_classLevel - 1) * 1;
        _luckBonus = classData.BaseLuckBonus + (_classLevel - 1) * 1;
        
        // 阶级加成
        float tierMultiplier = 1.0f;
        switch (_currentTier)
        {
            case ClassData.ClassTier.Adept: tierMultiplier = 1.25f; break;
            case ClassData.ClassTier.Master: tierMultiplier = 1.5f; break;
            case ClassData.ClassTier.Legend: tierMultiplier = 2.0f; break;
        }
        
        _healthBonus = (int)(_healthBonus * tierMultiplier);
        _attackBonus = (int)(_attackBonus * tierMultiplier);
        _defenseBonus = (int)(_defenseBonus * tierMultiplier);
        _magicBonus = (int)(_magicBonus * tierMultiplier);
    }

    public ClassData GetCurrentClassData()
    {
        if (_classes.ContainsKey(_currentClass))
            return _classes[_currentClass];
        return null;
    }

    public ClassData GetClassData(ClassData.ClassType classType)
    {
        return _classes.ContainsKey(classType) ? _classes[classType] : null;
    }

    // Getters
    public ClassData.ClassType CurrentClass => _currentClass;
    public ClassData.ClassType? SecondaryClass => _secondaryClass;
    public int ClassLevel => _classLevel;
    public int ClassExperience => _classExperience;
    public int TotalClassLevels => _totalClassLevels;
    public ClassData.ClassTier CurrentTier => _currentTier;
    
    public int HealthBonus => _healthBonus;
    public int AttackBonus => _attackBonus;
    public int DefenseBonus => _defenseBonus;
    public int MagicBonus => _magicBonus;
    public int SpeedBonus => _speedBonus;
    public int LuckBonus => _luckBonus;
    
    public int ExperienceToNextLevel => _classLevel >= _experienceThresholds.Length ? -1 : _experienceThresholds[_classLevel] - _classExperience;

    public void SetClass(ClassData.ClassType newClass)
    {
        if (_classes.ContainsKey(newClass))
        {
            _currentClass = newClass;
            UpdateBonuses();
            GD.Print($"[ClassSystem] 职业已切换到 {GetCurrentClassData()?.Name}");
        }
    }

    public void SetSecondaryClass(ClassData.ClassType? newSecondaryClass)
    {
        _secondaryClass = newSecondaryClass;
        UpdateBonuses();
    }

    public Dictionary<ClassData.ClassType, ClassData> GetAllClasses()
    {
        return _classes;
    }

    public List<ClassData> GetAvailableClasses()
    {
        var result = new List<ClassData>();
        foreach (var kvp in _classes)
        {
            if (kvp.Value.Tier == ClassData.ClassTier.Novice)
                result.Add(kvp.Value);
        }
        return result;
    }

    public List<ClassData> GetAdvancedClasses()
    {
        var result = new List<ClassData>();
        foreach (var kvp in _classes)
        {
            if (kvp.Value.Tier != ClassData.ClassTier.Novice)
                result.Add(kvp.Value);
        }
        return result;
    }

    public override Dictionary ExportSaveData()
    {
        return new Dictionary
        {
            { "current_class", (int)_currentClass },
            { "secondary_class", _secondaryClass.HasValue ? (int)_secondaryClass.Value : -1 },
            { "class_level", _classLevel },
            { "class_experience", _classExperience },
            { "total_class_levels", _totalClassLevels },
            { "current_tier", (int)_currentTier }
        };
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("current_class"))
            _currentClass = (ClassData.ClassType)(int)data["current_class"];
        if (data.ContainsKey("secondary_class") && (int)data["secondary_class"] >= 0)
            _secondaryClass = (ClassData.ClassType)(int)data["secondary_class"];
        if (data.ContainsKey("class_level"))
            _classLevel = (int)data["class_level"];
        if (data.ContainsKey("class_experience"))
            _classExperience = (int)data["class_experience"];
        if (data.ContainsKey("total_class_levels"))
            _totalClassLevels = (int)data["total_class_levels"];
        if (data.ContainsKey("current_tier"))
            _currentTier = (ClassData.ClassTier)(int)data["current_tier"];
        
        UpdateBonuses();
    }
    
    // 旧的存档支持方法（保留兼容性）
    public Dictionary<string, object> SaveData()
    {
        return ExportSaveData();
    }

    public void LoadData(Dictionary<string, object> data)
    {
        ImportSaveData(new Dictionary(data));
    }
}
