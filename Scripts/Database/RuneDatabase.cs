using System;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// Rune database - stores all rune configurations
    /// </summary>
    public class RuneDatabase : DatabaseBase
    {
        private List<RuneData> _runes = new List<RuneData>();
        private Dictionary<string, RuneData> _runeIndex = new Dictionary<string, RuneData>();
        
        private const string DEFAULT_CONFIG_PATH = "res://Resources/Config/runes_config.json";

        /// <summary>
        /// 静态实例引用（兼容原有访问模式）
        /// </summary>
        public static RuneDatabase Instance { get; private set; }

        public override object Instance => Instance;

        public RuneDatabase()
        {
            Instance = this;
            Initialize();
        }

        public override void Initialize()
        {
            LoadRunesFromConfig();
        }

        public override bool ValidateData() => _runes.Count > 0;

        /// <summary>
        /// 从配置文件加载符文数据
        /// </summary>
        private void LoadRunesFromConfig()
        {
            var loader = Loaders.RuneConfigLoader.Instance;
            string configPath = DEFAULT_CONFIG_PATH;
            
            if (!loader.Load(configPath))
            {
                GD.PrintErr($"[RuneDatabase] 符文配置加载失败: {loader.LastError}");
                GD.Print("[RuneDatabase] 尝试使用备用路径...");
                // 尝试从Scripts目录加载（开发时）
                configPath = "res://Scripts/Database/Loaders/runes_config.json";
                if (!loader.Load(configPath))
                {
                    GD.PrintErr($"[RuneDatabase] 备用路径加载也失败: {loader.LastError}");
                    return;
                }
            }

            var runeDataList = loader.GetAllRuneData();
            foreach (var rune in runeDataList)
            {
                AddRune(rune);
            }
            
            GD.Print($"[RuneDatabase] 已加载 {_runes.Count} 个符文");
        }

        private void AddRune(RuneData rune)
        {
            _runes.Add(rune);
            _runeIndex[rune.Id] = rune;
            _dataStore[rune.Id] = rune;
        }

        /// <summary>
        /// 通过 ID 获取符文（兼容原有方法名）
        /// </summary>
        public RuneData GetRuneById(string id)
        {
            if (_runeIndex.ContainsKey(id))
            {
                return _runeIndex[id];
            }
            return null;
        }

        /// <summary>
        /// 通过 ID 获取符文（IDatabase 规范别名）
        /// </summary>
        public RuneData GetRune(string runeId) => GetRuneById(runeId);

        /// <summary>
        /// 获取所有符文
        /// </summary>
        public List<RuneData> GetAllRunes()
        {
            return new List<RuneData>(_runes);
        }

        public List<RuneData> GetRunesByType(RuneType type)
        {
            return _runes.FindAll(r => r.Type == type);
        }

        public List<RuneData> GetRunesByRarity(RuneRarity rarity)
        {
            return _runes.FindAll(r => r.Rarity == rarity);
        }

        public List<RuneData> GetRunesBySlot(RuneSlotType slotType)
        {
            return _runes.FindAll(r => r.SlotType == slotType || r.SlotType == RuneSlotType.Any);
        }

        public List<RuneData> GetRunesByLevel(int playerLevel)
        {
            return _runes.FindAll(r => r.RequiredLevel <= playerLevel);
        }

        public int GetTotalRuneCount()
        {
            return _runes.Count;
        }

        public Dictionary<RuneRarity, int> GetRarityDistribution()
        {
            var distribution = new Dictionary<RuneRarity, int>();
            foreach (RuneRarity rarity in Enum.GetValues(typeof(RuneRarity)))
            {
                distribution[rarity] = _runes.FindAll(r => r.Rarity == rarity).Count;
            }
            return distribution;
        }

        // IDatabase GetData<T> override using rune-specific index
        public override T GetData<T>(string key)
        {
            if (_runeIndex.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return null;
        }

        public override int GetDataCount() => _runes.Count;

        public override IEnumerable<string> GetAllKeys() => _runeIndex.Keys;

        #region 存档持久化

        private const string KEY_RUNES = "runes";

        protected override void OnExportSaveData(Godot.Collections.Dictionary saveData)
        {
            base.OnExportSaveData(saveData);

            var runesArray = new Godot.Collections.Array();
            foreach (var kvp in _runeIndex)
            {
                var rune = kvp.Value;
                runesArray.Add(new Godot.Collections.Dictionary
                {
                    ["id"] = rune.Id,
                    ["name"] = rune.Name,
                    ["description"] = rune.Description,
                    ["type"] = (int)rune.Type,
                    ["rarity"] = (int)rune.Rarity,
                    ["slotType"] = (int)rune.SlotType,
                    ["attackBonus"] = rune.AttackBonus,
                    ["defenseBonus"] = rune.DefenseBonus,
                    ["healthBonus"] = rune.HealthBonus,
                    ["critRateBonus"] = rune.CritRateBonus,
                    ["critDamageBonus"] = rune.CritDamageBonus,
                    ["lifeStealBonus"] = rune.LifeStealBonus,
                    ["dodgeBonus"] = rune.DodgeBonus,
                    ["speedBonus"] = rune.SpeedBonus,
                    ["blockBonus"] = rune.BlockBonus,
                    ["specialEffect"] = rune.SpecialEffect ?? string.Empty,
                    ["specialEffectValue"] = rune.SpecialEffectValue,
                    ["requiredLevel"] = rune.RequiredLevel
                });
            }
            saveData[KEY_RUNES] = runesArray;
        }

        protected override void OnImportSaveData(Godot.Collections.Dictionary saveData)
        {
            base.OnImportSaveData(saveData);

            if (!saveData.ContainsKey(KEY_RUNES))
                return;

            var runesArray = (Godot.Collections.Array)saveData[KEY_RUNES];
            foreach (Godot.Collections.Dictionary runeDict in runesArray)
            {
                var rune = new RuneData
                {
                    Id = (string)runeDict["id"],
                    Name = (string)runeDict["name"],
                    Description = (string)runeDict["description"],
                    Type = (RuneType)(int)runeDict["type"],
                    Rarity = (RuneRarity)(int)runeDict["rarity"],
                    SlotType = (RuneSlotType)(int)runeDict["slotType"],
                    AttackBonus = (float)runeDict["attackBonus"],
                    DefenseBonus = (float)runeDict["defenseBonus"],
                    HealthBonus = (float)runeDict["healthBonus"],
                    CritRateBonus = (float)runeDict["critRateBonus"],
                    CritDamageBonus = (float)runeDict["critDamageBonus"],
                    LifeStealBonus = (float)runeDict["lifeStealBonus"],
                    DodgeBonus = (float)runeDict["dodgeBonus"],
                    SpeedBonus = (float)runeDict["speedBonus"],
                    BlockBonus = (float)runeDict["blockBonus"],
                    SpecialEffect = (string)runeDict["specialEffect"],
                    SpecialEffectValue = (float)runeDict["specialEffectValue"],
                    RequiredLevel = (int)runeDict["requiredLevel"]
                };
                _runeIndex[rune.Id] = rune;
                // Also add to list if not already present
                if (!_runes.Exists(r => r.Id == rune.Id))
                    _runes.Add(rune);
            }
        }

        #endregion
    }

    // ==================== 数据类型定义 ====================

    /// <summary>
    /// 符文类型
    /// </summary>
    public enum RuneType
    {
        Offensive,
        Defensive,
        Utility,
        Special
    }

    /// <summary>
    /// 符文稀有度
    /// </summary>
    public enum RuneRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// 符文槽位类型
    /// </summary>
    public enum RuneSlotType
    {
        Weapon,
        Shield,
        Chestplate,
        Helmet,
        Boots,
        Ring,
        Amulet,
        Any
    }

    /// <summary>
    /// 符文数据
    /// </summary>
    public class RuneData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RuneType Type { get; set; }
        public RuneRarity Rarity { get; set; }
        public RuneSlotType SlotType { get; set; }
        public float AttackBonus { get; set; }
        public float DefenseBonus { get; set; }
        public float HealthBonus { get; set; }
        public float CritRateBonus { get; set; }
        public float CritDamageBonus { get; set; }
        public float LifeStealBonus { get; set; }
        public float DodgeBonus { get; set; }
        public float SpeedBonus { get; set; }
        public float BlockBonus { get; set; }
        public string SpecialEffect { get; set; }
        public float SpecialEffectValue { get; set; }
        public int RequiredLevel { get; set; }

        /// <summary>
        /// 符文属性枚举（兼容原有代码）
        /// </summary>
        public enum RuneAttribute {
            Damage, Defense, MaxHealth, MaxMana, CritChance, CritDamage,
            AttackSpeed, MoveSpeed, HealthRegen, ManaRegen,
            FireResistance, IceResistance, DarkResistance
        }

        /// <summary>
        /// 符文套装类型（兼容原有代码）
        /// </summary>
        public enum RuneSet {
            None, Attack, Defense, Life, Magic, Speed, Critical,
            Balance, Dragon, Phoenix, Shadow
        }

        /// <summary>
        /// 符文类（兼容原有代码 - RuneManager 使用）
        /// </summary>
        public class Rune {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public RuneType Type { get; set; }
            public RuneRarity Rarity { get; set; }
            public RuneSet Set { get; set; }
            public Dictionary<RuneAttribute, float> Attributes { get; set; }
            public int LevelRequired { get; set; }
            public int Price { get; set; }
            public string UniquePassive { get; set; }
            public string IconPath { get; set; }

            public Rune() {
                Attributes = new Dictionary<RuneAttribute, float>();
                Set = RuneSet.None;
            }
        }

        /// <summary>
        /// 从 RuneData 隐式转换为 Rune（供 RuneManager 使用）
        /// </summary>
        public static implicit operator Rune(RuneData data)
        {
            if (data == null) return null;
            var rune = new Rune {
                Id = data.Id,
                Name = data.Name,
                Description = data.Description,
                Type = data.Type,
                Rarity = data.Rarity,
                LevelRequired = data.RequiredLevel,
                Price = 100,
                Set = RuneSet.None
            };
            if (data.AttackBonus != 0) rune.Attributes[RuneAttribute.Damage] = data.AttackBonus;
            if (data.DefenseBonus != 0) rune.Attributes[RuneAttribute.Defense] = data.DefenseBonus;
            if (data.HealthBonus != 0) rune.Attributes[RuneAttribute.MaxHealth] = data.HealthBonus;
            if (data.CritRateBonus != 0) rune.Attributes[RuneAttribute.CritChance] = data.CritRateBonus;
            if (data.CritDamageBonus != 0) rune.Attributes[RuneAttribute.CritDamage] = data.CritDamageBonus;
            if (data.LifeStealBonus != 0) rune.Attributes[RuneAttribute.Damage] = data.LifeStealBonus; // lifeSteal mapped to damage
            if (data.DodgeBonus != 0) rune.Attributes[RuneAttribute.MoveSpeed] = data.DodgeBonus; // dodge mapped to speed
            if (data.SpeedBonus != 0) rune.Attributes[RuneAttribute.MoveSpeed] = data.SpeedBonus;
            if (data.BlockBonus != 0) rune.Attributes[RuneAttribute.Defense] = data.BlockBonus; // block mapped to defense
            return rune;
        }

        /// <summary>
        /// 兼容性别名 - 原有代码使用 RuneDefinition
        /// </summary>
        public class RuneDefinition
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public RuneType Type { get; set; }
            public RuneSlot Slot { get; set; }
            public int BaseCost { get; set; }
            public int EnhanceCost { get; set; }
            public int RequiredLevel { get; set; }
            public Dictionary<string, float> Attributes { get; set; }
            public string SpecialEffect { get; set; }

            public static implicit operator RuneDefinition(RuneData data)
            {
                if (data == null) return null;
                var def = new RuneDefinition
                {
                    Id = data.Id,
                    Name = data.Name,
                    Description = data.Description,
                    Type = data.Type,
                    Slot = RuneDatabase.SlotToRuneSlot(data.SlotType),
                    RequiredLevel = data.RequiredLevel,
                    SpecialEffect = data.SpecialEffect,
                    BaseCost = 100,
                    EnhanceCost = 50,
                    Attributes = new Dictionary<string, float>()
                };
                if (data.AttackBonus != 0) def.Attributes["attack"] = data.AttackBonus;
                if (data.DefenseBonus != 0) def.Attributes["defense"] = data.DefenseBonus;
                if (data.HealthBonus != 0) def.Attributes["health"] = data.HealthBonus;
                if (data.CritRateBonus != 0) def.Attributes["critical"] = data.CritRateBonus;
                if (data.CritDamageBonus != 0) def.Attributes["crit_damage"] = data.CritDamageBonus;
                if (data.LifeStealBonus != 0) def.Attributes["life_steal"] = data.LifeStealBonus;
                if (data.DodgeBonus != 0) def.Attributes["dodge"] = data.DodgeBonus;
                if (data.SpeedBonus != 0) def.Attributes["speed"] = data.SpeedBonus;
                if (data.BlockBonus != 0) def.Attributes["block"] = data.BlockBonus;
                return def;
            }
        }

        /// <summary>
        /// 兼容性别名 - 原有代码使用 RuneSlot
        /// </summary>
        public enum RuneSlot
        {
            Helmet,
            Chest,
            Legs,
            Weapon,
            Accessory
        }

        /// <summary>
        /// 将 RuneSlotType 映射到 RuneSlot（兼容性别名）
        /// </summary>
        public static RuneSlot SlotToRuneSlot(RuneSlotType slotType)
        {
            return slotType switch
            {
                RuneSlotType.Helmet => RuneSlot.Helmet,
                RuneSlotType.Chestplate => RuneSlot.Chest,
                RuneSlotType.Boots => RuneSlot.Legs,
                RuneSlotType.Weapon => RuneSlot.Weapon,
                RuneSlotType.Ring or RuneSlotType.Amulet => RuneSlot.Accessory,
                _ => RuneSlot.Accessory
            };
        }
    }
}
