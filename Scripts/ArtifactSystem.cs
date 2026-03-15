using Godot;
using System;
using System.Collections.Generic;

public class ArtifactSystem : BaseSystem
{
    // 神器数据存储
    private Dictionary<string, ArtifactData> _artifacts = new Dictionary<string, ArtifactData>();
    private List<string> _unlockedArtifacts = new List<string>();
    private string _equippedArtifact = "";
    
    // 信号
    public static Signal<string> ArtifactUnlocked { get; } = new Signal<string>();
    public static Signal<string> ArtifactEquipped { get; } = new Signal<string>();
    public static Signal<string> ArtifactUnequipped { get; } = new Signal<string>();
    
    // 持久化
    private const string SAVE_KEY = "artifact_system";
    
    public override void _Ready()
    {
        InitializeArtifacts();
        LoadData();
    }
    
    /// <summary>
    /// 系统名称
    /// </summary>
    protected override string SystemName => "Artifact";
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        var unlocked = new Array();
        foreach (var artifact in _unlockedArtifacts)
        {
            unlocked.Add(artifact);
        }
        
        data["unlocked_artifacts"] = unlocked;
        data["equipped_artifact"] = _equippedArtifact;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("unlocked_artifacts"))
        {
            var unlocked = (Array)data["unlocked_artifacts"];
            _unlockedArtifacts.Clear();
            foreach (var artifact in unlocked)
            {
                _unlockedArtifacts.Add((string)artifact);
            }
        }
        
        if (data.Contains("equipped_artifact"))
        {
            _equippedArtifact = (string)data["equipped_artifact"];
        }
    }
    
    private void InitializeArtifacts()
    {
        // 创建一个完整的神器系统
        // 传说神器 (Legendary)
        AddArtifact(new ArtifactData(
            "solar_crown", "Solar Crown", "Sun King's Crown",
            ArtifactRarity.Legendary, ArtifactType.Helmet,
            "佩戴者获得太阳之力", 500, 50, 30, 20, 25, 0.15f, 0.10f
        ));
        
        AddArtifact(new ArtifactData(
            "void_heart", "Void Heart", "Heart of Darkness",
            ArtifactRarity.Legendary, ArtifactType.Chestplate,
            "暗影与虚空的力量在血管中流淌", 800, 40, 40, 15, 30, 0.20f, 0.15f
        ));
        
        AddArtifact(new ArtifactData(
            "dragon_ring", "Dragon Ring", "Ancient Dragon's Ring",
            ArtifactRarity.Legendary, ArtifactType.Ring,
            "蕴含古老巨龙的祝福", 300, 60, 20, 25, 20, 0.12f, 0.08f
        ));
        
        AddArtifact(new ArtifactData(
            "phoenix_cloak", "Phoenix Cloak", "Eternal Flame Cloak",
            ArtifactRarity.Legendary, ArtifactType.Cloak,
            "凤凰的永不熄灭之火", 400, 30, 35, 30, 35, 0.18f, 0.12f
        ));
        
        AddArtifact(new ArtifactData(
            "thunder_hammer", "Thunder Hammer", "Mjolnir's Blessing",
            ArtifactRarity.Legendary, ArtifactType.Weapon,
            "雷神之锤的祝福", 600, 80, 15, 10, 15, 0.25f, 0.20f
        ));
        
        // 史诗神器 (Epic)
        AddArtifact(new ArtifactData(
            "shadow_dagger", "Shadow Dagger", "Blade of Shadows",
            ArtifactRarity.Epic, ArtifactType.Weapon,
            "在阴影中隐藏的利刃", 350, 55, 20, 15, 20, 0.15f, 0.10f
        ));
        
        AddArtifact(new ArtifactData(
            "crystal_amulet", "Crystal Amulet", "Mana Crystal",
            ArtifactRarity.Epic, ArtifactType.Amulet,
            "蕴含纯净的魔法能量", 250, 25, 45, 20, 25, 0.10f, 0.08f
        ));
        
        AddArtifact(new ArtifactData(
            "iron_shield", "Iron Shield", "Ancient Fortress",
            ArtifactRarity.Epic, ArtifactType.Shield,
            "固若金汤的防御", 400, 20, 55, 25, 30, 0.08f, 0.12f
        ));
        
        AddArtifact(new ArtifactData(
            "spirit_bow", "Spirit Bow", "Elven Archer's Bow",
            ArtifactRarity.Epic, ArtifactType.Weapon,
            "精灵族的神弓", 320, 50, 25, 30, 15, 0.14f, 0.09f
        ));
        
        AddArtifact(new ArtifactData(
            "arcane_staff", "Arcane Staff", "Wizard's Mastery",
            ArtifactRarity.Epic, ArtifactType.Weapon,
            "大法师的奥术权杖", 380, 45, 35, 15, 25, 0.16f, 0.11f
        ));
        
        // 稀有神器 (Rare)
        AddArtifact(new ArtifactData(
            "steel_boots", "Steel Boots", "Iron Greaves",
            ArtifactRarity.Rare, ArtifactType.Boots,
            "坚固的钢铁护靴", 180, 30, 25, 20, 15, 0.06f, 0.05f
        ));
        
        AddArtifact(new ArtifactData(
            "leather_gloves", "Leather Gloves", "Assassin手套",
            ArtifactRarity.Rare, ArtifactType.Gloves,
            "刺客的敏捷手套", 150, 35, 20, 25, 10, 0.07f, 0.04f
        ));
        
        AddArtifact(new ArtifactData(
            "golden_bracelet", "Golden Bracelet", "Wealth Charm",
            ArtifactRarity.Rare, ArtifactType.Bracelet,
            "财富的象征", 200, 20, 30, 15, 20, 0.05f, 0.06f
        ));
        
        AddArtifact(new ArtifactData(
            "emerald_belt", "Emerald Belt", "Nature's Grace",
            ArtifactRarity.Rare, ArtifactType.Belt,
            "大自然的恩赐", 170, 25, 28, 18, 22, 0.06f, 0.05f
        ));
        
        // 优秀神器 (Uncommon)
        AddArtifact(new ArtifactData(
            "silver_pendant", "Silver Pendant", "Moonlight Charm",
            ArtifactRarity.Uncommon, ArtifactType.Amulet,
            "月光护符", 100, 15, 20, 12, 15, 0.03f, 0.03f
        ));
        
        AddArtifact(new ArtifactData(
            "copper_ring", "Copper Ring", "Simple Band",
            ArtifactRarity.Uncommon, ArtifactType.Ring,
            "简单的铜戒指", 80, 18, 15, 10, 12, 0.02f, 0.02f
        ));
        
        AddArtifact(new ArtifactData(
            "bronze_helmet", "Bronze Helmet", "Soldier's Helm",
            ArtifactRarity.Uncommon, ArtifactType.Helmet,
            "战士的头盔", 120, 20, 18, 15, 10, 0.03f, 0.03f
        ));
        
        // 普通神器 (Common)
        AddArtifact(new ArtifactData(
            "wooden_charm", "Wooden Charm", "Basic Charm",
            ArtifactRarity.Common, ArtifactType.Amulet,
            "基础护符", 50, 10, 10, 8, 8, 0.01f, 0.01f
        ));
        
        AddArtifact(new ArtifactData(
            "stone_pendant", "Stone Pendant", "Rock Pendant",
            ArtifactRarity.Common, ArtifactType.Necklace,
            "简单的石头吊坠", 45, 8, 12, 10, 10, 0.01f, 0.01f
        ));
    }
    
    private void AddArtifact(ArtifactData artifact)
    {
        _artifacts[artifact.id] = artifact;
    }
    
    // 获取所有神器
    public Dictionary<string, ArtifactData> GetAllArtifacts()
    {
        return _artifacts;
    }
    
    // 获取已解锁的神器
    public List<string> GetUnlockedArtifacts()
    {
        return _unlockedArtifacts;
    }
    
    // 解锁神器
    public void UnlockArtifact(string artifactId)
    {
        if (_artifacts.ContainsKey(artifactId) && !_unlockedArtifacts.Contains(artifactId))
        {
            _unlockedArtifacts.Add(artifactId);
            ArtifactUnlocked?.Invoke(artifactId);
            SaveData();
        }
    }
    
    // 装备神器
    public void EquipArtifact(string artifactId)
    {
        if (_unlockedArtifacts.Contains(artifactId))
        {
            _equippedArtifact = artifactId;
            ArtifactEquipped?.Invoke(artifactId);
            SaveData();
        }
    }
    
    // 卸下神器
    public void UnequipArtifact()
    {
        if (_equippedArtifact != "")
        {
            string unequipped = _equippedArtifact;
            _equippedArtifact = "";
            ArtifactUnequipped?.Invoke(unequipped);
            SaveData();
        }
    }
    
    // 获取当前装备的神器
    public string GetEquippedArtifact()
    {
        return _equippedArtifact;
    }
    
    // 获取已装备神器的加成
    public ArtifactBonus GetEquippedBonus()
    {
        if (_equippedArtifact == "" || !_artifacts.ContainsKey(_equippedArtifact))
        {
            return new ArtifactBonus();
        }
        
        return _artifacts[_equippedArtifact].bonus;
    }
    
    // 检查是否已解锁
    public bool IsUnlocked(string artifactId)
    {
        return _unlockedArtifacts.Contains(artifactId);
    }
    
    // 检查是否已装备
    public bool IsEquipped(string artifactId)
    {
        return _equippedArtifact == artifactId;
    }
    
    // 获取神器数据
    public ArtifactData GetArtifact(string artifactId)
    {
        if (_artifacts.ContainsKey(artifactId))
        {
            return _artifacts[artifactId];
        }
        return null;
    }
    
    // 随机解锁一个神器（通过战斗/任务获得）
    public string RandomUnlockArtifact()
    {
        List<string> locked = new List<string>();
        foreach (var kvp in _artifacts)
        {
            if (!_unlockedArtifacts.Contains(kvp.Key))
            {
                locked.Add(kvp.Key);
            }
        }
        
        if (locked.Count == 0) return "";
        
        // 基于稀有度权重
        var weights = new Dictionary<ArtifactRarity, int>
        {
            { ArtifactRarity.Common, 40 },
            { ArtifactRarity.Uncommon, 30 },
            { ArtifactRarity.Rare, 18 },
            { ArtifactRarity.Epic, 9 },
            { ArtifactRarity.Legendary, 3 }
        };
        
        // 按稀有度分类
        var rarityGroups = new Dictionary<ArtifactRarity, List<string>>();
        foreach (var id in locked)
        {
            var rarity = _artifacts[id].rarity;
            if (!rarityGroups.ContainsKey(rarity))
                rarityGroups[rarity] = new List<string>();
            rarityGroups[rarity].Add(id);
        }
        
        // 随机选择稀有度
        int totalWeight = 0;
        foreach (var w in weights)
            totalWeight += w.Value;
        
        int roll = GD.RandI() % totalWeight;
        ArtifactRarity selectedRarity = ArtifactRarity.Common;
        
        int cumulative = 0;
        foreach (var w in weights)
        {
            cumulative += w.Value;
            if (roll < cumulative)
            {
                selectedRarity = w.Key;
                break;
            }
        }
        
        // 从选中的稀有度中随机选择
        if (rarityGroups.ContainsKey(selectedRarity) && rarityGroups[selectedRarity].Count > 0)
        {
            string selected = rarityGroups[selectedRarity][GD.RandI() % rarityGroups[selectedRarity].Count];
            UnlockArtifact(selected);
            return selected;
        }
        
        // 如果选中的稀有度没有未解锁的，随机选一个
        string fallback = locked[GD.RandI() % locked.Count];
        UnlockArtifact(fallback);
        return fallback;
    }
    
    // 获取统计信息
    public Dictionary<string, int> GetStats()
    {
        var stats = new Dictionary<string, int>();
        stats["total_artifacts"] = _artifacts.Count;
        stats["unlocked_count"] = _unlockedArtifacts.Count;
        
        int legendary = 0, epic = 0, rare = 0, uncommon = 0, common = 0;
        foreach (var id in _unlockedArtifacts)
        {
            switch (_artifacts[id].rarity)
            {
                case ArtifactRarity.Legendary: legendary++; break;
                case ArtifactRarity.Epic: epic++; break;
                case ArtifactRarity.Rare: rare++; break;
                case ArtifactRarity.Uncommon: uncommon++; break;
                case ArtifactRarity.Common: common++; break;
            }
        }
        
        stats["legendary"] = legendary;
        stats["epic"] = epic;
        stats["rare"] = rare;
        stats["uncommon"] = uncommon;
        stats["common"] = common;
        
        return stats;
    }
    
    // 存档
    public Dictionary<string, object> Save()
    {
        var data = new Dictionary<string, object>();
        data["unlocked_artifacts"] = _unlockedArtifacts;
        data["equipped_artifact"] = _equippedArtifact;
        return data;
    }
    
    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("unlocked_artifacts"))
        {
            _unlockedArtifacts = new List<string>((System.Collections.IEnumerable)data["unlocked_artifacts"]);
        }
        
        if (data.ContainsKey("equipped_artifact"))
        {
            _equippedArtifact = (string)data["equipped_artifact"];
        }
    }
    
    private void SaveData()
    {
        var saveGame = new FileAccess();
        string savePath = "user://" + SAVE_KEY + ".dat";
        saveGame.Open(savePath, FileAccess.ModeFlags.Write);
        
        var data = Save();
        string json = JSON.Stringify(data);
        saveGame.StoreString(json);
        saveGame.Close();
    }
    
    private void LoadData()
    {
        string savePath = "user://" + SAVE_KEY + ".dat";
        if (FileAccess.FileExists(savePath))
        {
            var saveGame = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
            string json = saveGame.GetAsText();
            saveGame.Close();
            
            var data = (Dictionary<string, object>)JSON.ParseString(json);
            Load(data);
        }
    }
}

// 神器稀有度
public enum ArtifactRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

// 神器类型
public enum ArtifactType
{
    Helmet,
    Chestplate,
    Weapon,
    Shield,
    Ring,
    Amulet,
    Necklace,
    Cloak,
    Boots,
    Gloves,
    Belt,
    Bracelet
}

// 神器数据结构
public class ArtifactData
{
    public string id;
    public string name;
    public string displayName;
    public ArtifactRarity rarity;
    public ArtifactType type;
    public string description;
    public int power;
    public int attack;
    public int defense;
    public int health;
    public int speed;
    public float critChance;
    public float critDamage;
    public ArtifactBonus bonus;
    
    public ArtifactData(string id, string name, string displayName, ArtifactRarity rarity, 
        ArtifactType type, string description, int power, int attack, int defense, 
        int health, int speed, float critChance, float critDamage)
    {
        this.id = id;
        this.name = name;
        this.displayName = displayName;
        this.rarity = rarity;
        this.type = type;
        this.description = description;
        this.power = power;
        this.attack = attack;
        this.defense = defense;
        this.health = health;
        this.speed = speed;
        this.critChance = critChance;
        this.critDamage = critDamage;
        
        this.bonus = new ArtifactBonus(attack, defense, health, speed, critChance, critDamage);
    }
}

// 神器加成
public class ArtifactBonus
{
    public int attack;
    public int defense;
    public int health;
    public int speed;
    public float critChance;
    public float critDamage;
    
    public ArtifactBonus(int attack = 0, int defense = 0, int health = 0, int speed = 0, 
        float critChance = 0f, float critDamage = 0f)
    {
        this.attack = attack;
        this.defense = defense;
        this.health = health;
        this.speed = speed;
        this.critChance = critChance;
        this.critDamage = critDamage;
    }
    
    public static ArtifactBonus operator +(ArtifactBonus a, ArtifactBonus b)
    {
        return new ArtifactBonus(
            a.attack + b.attack,
            a.defense + b.defense,
            a.health + b.health,
            a.speed + b.speed,
            a.critChance + b.critChance,
            a.critDamage + b.critDamage
        );
    }
}
