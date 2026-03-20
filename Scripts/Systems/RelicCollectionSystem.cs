using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 圣物收集系统 - 管理玩家收集的圣物和装备槽位
/// 支持多种圣物槽位、稀有度和效果类型
/// </summary>
public class RelicCollectionSystem : BaseSystem
{
    /// <summary>
    /// 获取系统单例实例
    /// </summary>
    public static RelicCollectionSystem Instance { get; private set; }

    #region Enums

    /// <summary>
    /// 圣物稀有度
    /// </summary>
    public enum RelicRarity
    {
        Common,      // 普通 - 白色
        Uncommon,    // 优秀 - 绿色
        Rare,       // 稀有 - 蓝色
        Epic,       // 史诗 - 紫色
        Legendary    // 传说 - 橙色
    }

    /// <summary>
    /// 圣物槽位类型
    /// </summary>
    public enum RelicSlotType
    {
        Head,       // 头部
        Chest,      // 胸部
        Weapon,     // 武器
        Accessory,  // 饰品
        Offhand,    // 副手
        Ring,       // 戒指
        Amulet,     // 护符
        Talisman    // 护身符
    }

    #endregion

    #region Data Structures

    /// <summary>
    /// 圣物数据
    /// </summary>
    public class RelicCollectionData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RelicSlotType SlotType { get; set; }
        public RelicRarity Rarity { get; set; }
        public List<RelicEffect> Effects { get; set; } = new();
        public int Level { get; set; } = 1;
        public int MaxLevel { get; set; } = 5;
        public bool IsOwned { get; set; }

        public RelicCollectionData() { }

        public RelicCollectionData(string id, string name, string description, RelicSlotType slotType, RelicRarity rarity)
        {
            Id = id;
            Name = name;
            Description = description;
            SlotType = slotType;
            Rarity = rarity;
            IsOwned = false;
        }

        public float GetRarityMultiplier()
        {
            return Rarity switch
            {
                RelicRarity.Common => 1.0f,
                RelicRarity.Uncommon => 1.25f,
                RelicRarity.Rare => 1.5f,
                RelicRarity.Epic => 2.0f,
                RelicRarity.Legendary => 3.0f,
                _ => 1.0f
            };
        }
    }

    /// <summary>
    /// 玩家圣物数据
    /// </summary>
    public class PlayerRelicCollectionData
    {
        public HashSet<string> OwnedRelicIds { get; set; } = new();
        public Dictionary<RelicSlotType, string> EquippedRelics { get; set; } = new();
        public Dictionary<string, int> RelicLevels { get; set; } = new();
        public int CollectionScore { get; set; }
        public int TotalRelicsOwned { get; set; }
    }

    #endregion

    #region Singleton

    public override void _Ready()
    {
        Instance = this;
        Initialize();
    }

    protected override void Initialize()
    {
        _playerData = new PlayerRelicCollectionData();
        InitializeRelicDatabase();
        GD.Print($"[RelicCollectionSystem] Initialized with {_relicDatabase.Count} relics");
        IsInitialized = true;
    }

    #endregion

    #region Properties

    private Dictionary<string, RelicCollectionData> _relicDatabase = new();
    private PlayerRelicCollectionData _playerData;

    public PlayerRelicCollectionData PlayerData => _playerData;

    #endregion

    #region Database

    private void InitializeRelicDatabase()
    {
        // Head Slot Relics
        AddRelic(new RelicCollectionData("relic_crown_vision", "先见之冠", "增加10%暴击率", RelicSlotType.Head, RelicRarity.Rare));
        GetRelic("relic_crown_vision").Effects.Add(new StatModifierEffect("crit_rate", 0.10f));

        AddRelic(new RelicCollectionData("relic_helm_stone", "石之头盔", "增加5%防御力", RelicSlotType.Head, RelicRarity.Common));
        GetRelic("relic_helm_stone").Effects.Add(new StatModifierEffect("defense", 0.05f));

        AddRelic(new RelicCollectionData("relic_dragon_mask", "龙之面具", "攻击时5%几率造成双倍伤害", RelicSlotType.Head, RelicRarity.Epic));
        GetRelic("relic_dragon_mask").Effects.Add(new TriggerEffect("on_attack", "double_damage", 0.05f, 5f));

        AddRelic(new RelicCollectionData("relic_phoenix_crown", "凤凰之冠", "死亡时25%几率复活", RelicSlotType.Head, RelicRarity.Legendary));
        GetRelic("relic_phoenix_crown").Effects.Add(new TriggerEffect("on_death", "revive", 0.25f, 60f));

        // Chest Slot Relics
        AddRelic(new RelicCollectionData("relic_armor_plate", "护甲碎片", "增加50点防御力", RelicSlotType.Chest, RelicRarity.Common));
        GetRelic("relic_armor_plate").Effects.Add(new StatModifierEffect("defense", 50f));

        AddRelic(new RelicCollectionData("relic_iron_chest", "铁胸甲", "增加10%生命值", RelicSlotType.Chest, RelicRarity.Uncommon));
        GetRelic("relic_iron_chest").Effects.Add(new StatModifierEffect("health", 0.10f));

        AddRelic(new RelicCollectionData("relic_thorn_vest", "荆棘之衣", "受到攻击时反弹20%伤害", RelicSlotType.Chest, RelicRarity.Rare));
        GetRelic("relic_thorn_vest").Effects.Add(new PassiveEffect("thorns", 0.20f));

        AddRelic(new RelicCollectionData("relic_dragon_scale_chest", "龙鳞胸甲", "每5秒恢复1%最大生命值", RelicSlotType.Chest, RelicRarity.Epic));
        GetRelic("relic_dragon_scale_chest").Effects.Add(new PassiveEffect("regeneration", 1f));

        // Weapon Slot Relics
        AddRelic(new RelicCollectionData("relic_sword_fragment", "断剑碎片", "增加5%攻击力", RelicSlotType.Weapon, RelicRarity.Common));
        GetRelic("relic_sword_fragment").Effects.Add(new StatModifierEffect("attack", 0.05f));

        AddRelic(new RelicCollectionData("relic_blade_wing", "刀锋之翼", "增加8%攻击速度", RelicSlotType.Weapon, RelicRarity.Uncommon));
        GetRelic("relic_blade_wing").Effects.Add(new StatModifierEffect("speed", 0.08f));

        AddRelic(new RelicCollectionData("relic_blood_sword", "血之剑", "增加5%生命偷取", RelicSlotType.Weapon, RelicRarity.Rare));
        GetRelic("relic_blood_sword").Effects.Add(new StatModifierEffect("lifesteal", 0.05f));

        AddRelic(new RelicCollectionData("relic_thunder_fist", "雷霆之拳", "攻击时有10%几率使敌人麻痹2秒", RelicSlotType.Weapon, RelicRarity.Epic));
        GetRelic("relic_thunder_fist").Effects.Add(new TriggerEffect("on_attack", "stun", 0.10f, 8f));

        AddRelic(new RelicCollectionData("relic_void_blade", "虚空之刃", "攻击时有3%几率造成300%伤害", RelicSlotType.Weapon, RelicRarity.Legendary));
        GetRelic("relic_void_blade").Effects.Add(new TriggerEffect("on_attack", "execute", 0.03f, 10f));

        // Accessory Slot Relics
        AddRelic(new RelicCollectionData("relic_lucky_charm", "幸运符", "增加3%闪避率", RelicSlotType.Accessory, RelicRarity.Common));
        GetRelic("relic_lucky_charm").Effects.Add(new StatModifierEffect("dodge", 0.03f));

        AddRelic(new RelicCollectionData("relic_shadow_cloak", "暗影斗篷", "增加5%闪避率", RelicSlotType.Accessory, RelicRarity.Uncommon));
        GetRelic("relic_shadow_cloak").Effects.Add(new StatModifierEffect("dodge", 0.05f));

        AddRelic(new RelicCollectionData("relic_wisdom_eye", "智慧之眼", "增加10%经验获取", RelicSlotType.Accessory, RelicRarity.Rare));
        GetRelic("relic_wisdom_eye").Effects.Add(new PassiveEffect("exp_boost", 0.10f));

        AddRelic(new RelicCollectionData("relic_golden_belt", "黄金腰带", "金币获取增加15%", RelicSlotType.Accessory, RelicRarity.Epic));
        GetRelic("relic_golden_belt").Effects.Add(new PassiveEffect("gold_boost", 0.15f));

        // Offhand Slot Relics
        AddRelic(new RelicCollectionData("relic_shield_shard", "盾牌碎片", "增加30点防御力", RelicSlotType.Offhand, RelicRarity.Common));
        GetRelic("relic_shield_shard").Effects.Add(new StatModifierEffect("defense", 30f));

        AddRelic(new RelicCollectionData("relic_tower_shield", "塔盾", "受到伤害时5%几率减少50%", RelicSlotType.Offhand, RelicRarity.Rare));
        GetRelic("relic_tower_shield").Effects.Add(new TriggerEffect("on_take_damage", "damage_reduction", 0.05f, 10f));

        AddRelic(new RelicCollectionData("relic_aegis", "神盾", "生命值低于20%时获得无敌3秒", RelicSlotType.Offhand, RelicRarity.Legendary));
        GetRelic("relic_aegis").Effects.Add(new TriggerEffect("on_low_health", "invincibility", 1.0f, 60f));

        // Ring Slot Relics
        AddRelic(new RelicCollectionData("relic_copper_ring", "铜戒指", "增加2%暴击伤害", RelicSlotType.Ring, RelicRarity.Common));
        GetRelic("relic_copper_ring").Effects.Add(new StatModifierEffect("crit_damage", 0.02f));

        AddRelic(new RelicCollectionData("relic_silver_ring", "银戒指", "增加5%暴击伤害", RelicSlotType.Ring, RelicRarity.Uncommon));
        GetRelic("relic_silver_ring").Effects.Add(new StatModifierEffect("crit_damage", 0.05f));

        AddRelic(new RelicCollectionData("relic_emerald_ring", "翡翠戒指", "击杀敌人时3%几率回复生命", RelicSlotType.Ring, RelicRarity.Rare));
        GetRelic("relic_emerald_ring").Effects.Add(new TriggerEffect("on_kill", "life_steal", 0.03f, 2f));

        AddRelic(new RelicCollectionData("relic_diamond_ring", "钻石戒指", "增加10%暴击率和15%暴击伤害", RelicSlotType.Ring, RelicRarity.Epic));
        GetRelic("relic_diamond_ring").Effects.Add(new StatModifierEffect("crit_rate", 0.10f));
        GetRelic("relic_diamond_ring").Effects.Add(new StatModifierEffect("crit_damage", 0.15f));

        AddRelic(new RelicCollectionData("relic_ring_of_power", "力量之戒", "所有属性增加5%", RelicSlotType.Ring, RelicRarity.Legendary));
        GetRelic("relic_ring_of_power").Effects.Add(new StatModifierEffect("attack", 0.05f));
        GetRelic("relic_ring_of_power").Effects.Add(new StatModifierEffect("defense", 0.05f));
        GetRelic("relic_ring_of_power").Effects.Add(new StatModifierEffect("health", 0.05f));
        GetRelic("relic_ring_of_power").Effects.Add(new StatModifierEffect("speed", 0.05f));

        // Amulet Slot Relics
        AddRelic(new RelicCollectionData("relic_necklace_shard", "项链碎片", "增加3%生命值", RelicSlotType.Amulet, RelicRarity.Common));
        GetRelic("relic_necklace_shard").Effects.Add(new StatModifierEffect("health", 0.03f));

        AddRelic(new RelicCollectionData("relic_protection_amulet", "保护护符", "受到致命伤害时有5%几率免疫", RelicSlotType.Amulet, RelicRarity.Rare));
        GetRelic("relic_protection_amulet").Effects.Add(new TriggerEffect("on_fatal_damage", "immunity", 0.05f, 30f));

        AddRelic(new RelicCollectionData("relic_soul_harvest", "灵魂收割", "击杀敌人时获得额外灵魂", RelicSlotType.Amulet, RelicRarity.Epic));
        GetRelic("relic_soul_harvest").Effects.Add(new PassiveEffect("soul_gather", 1f));

        AddRelic(new RelicCollectionData("relic_sunfire_amulet", "阳炎护符", "攻击时10%几率灼烧敌人", RelicSlotType.Amulet, RelicRarity.Legendary));
        GetRelic("relic_sunfire_amulet").Effects.Add(new TriggerEffect("on_attack", "burn", 0.10f, 3f));

        // Talisman Slot Relics
        AddRelic(new RelicCollectionData("relic_wooden_charm", "木制护符", "增加1%所有属性", RelicSlotType.Talisman, RelicRarity.Common));
        GetRelic("relic_wooden_charm").Effects.Add(new StatModifierEffect("attack", 0.01f));
        GetRelic("relic_wooden_charm").Effects.Add(new StatModifierEffect("defense", 0.01f));

        AddRelic(new RelicCollectionData("relic_fortune_talisman", "命运护符", "增加10%掉落率", RelicSlotType.Talisman, RelicRarity.Uncommon));
        GetRelic("relic_fortune_talisman").Effects.Add(new PassiveEffect("drop_boost", 0.10f));

        AddRelic(new RelicCollectionData("relic_mystic_talisman", "神秘护符", "技能冷却速度加快10%", RelicSlotType.Talisman, RelicRarity.Rare));
        GetRelic("relic_mystic_talisman").Effects.Add(new PassiveEffect("cooldown_reduction", 0.10f));

        AddRelic(new RelicCollectionData("relic_cosmic_talisman", "宇宙护符", "所有属性增加8%", RelicSlotType.Talisman, RelicRarity.Epic));
        GetRelic("relic_cosmic_talisman").Effects.Add(new StatModifierEffect("attack", 0.08f));
        GetRelic("relic_cosmic_talisman").Effects.Add(new StatModifierEffect("defense", 0.08f));
        GetRelic("relic_cosmic_talisman").Effects.Add(new StatModifierEffect("health", 0.08f));
        GetRelic("relic_cosmic_talisman").Effects.Add(new StatModifierEffect("speed", 0.08f));

        AddRelic(new RelicCollectionData("relic_ancient_talisman", "远古护符", "所有属性增加15%,所有效果提升5%", RelicSlotType.Talisman, RelicRarity.Legendary));
        GetRelic("relic_ancient_talisman").Effects.Add(new StatModifierEffect("attack", 0.15f));
        GetRelic("relic_ancient_talisman").Effects.Add(new StatModifierEffect("defense", 0.15f));
        GetRelic("relic_ancient_talisman").Effects.Add(new StatModifierEffect("health", 0.15f));
        GetRelic("relic_ancient_talisman").Effects.Add(new StatModifierEffect("speed", 0.15f));

        GD.Print($"[RelicCollectionSystem] Database initialized with {_relicDatabase.Count} relics");
    }

    private void AddRelic(RelicCollectionData relic)
    {
        _relicDatabase[relic.Id] = relic;
    }

    #endregion

    #region Public API

    /// <summary>
    /// 获取圣物
    /// </summary>
    public RelicCollectionData GetRelic(string relicId)
    {
        return _relicDatabase.TryGetValue(relicId, out var relic) ? relic : null;
    }

    /// <summary>
    /// 获取所有圣物
    /// </summary>
    public List<RelicCollectionData> GetAllRelics()
    {
        return new List<RelicCollectionData>(_relicDatabase.Values);
    }

    /// <summary>
    /// 按槽位获取圣物
    /// </summary>
    public List<RelicCollectionData> GetRelicsBySlot(RelicSlotType slotType)
    {
        var result = new List<RelicCollectionData>();
        foreach (var relic in _relicDatabase.Values)
        {
            if (relic.SlotType == slotType)
                result.Add(relic);
        }
        return result;
    }

    /// <summary>
    /// 按稀有度获取圣物
    /// </summary>
    public List<RelicCollectionData> GetRelicsByRarity(RelicRarity rarity)
    {
        var result = new List<RelicCollectionData>();
        foreach (var relic in _relicDatabase.Values)
        {
            if (relic.Rarity == rarity)
                result.Add(relic);
        }
        return result;
    }

    /// <summary>
    /// 获取玩家拥有的圣物
    /// </summary>
    public List<RelicCollectionData> GetOwnedRelics()
    {
        var result = new List<RelicCollectionData>();
        foreach (var relicId in _playerData.OwnedRelicIds)
        {
            var relic = GetRelic(relicId);
            if (relic != null)
                result.Add(relic);
        }
        return result;
    }

    /// <summary>
    /// 获取已装备的圣物
    /// </summary>
    public Dictionary<RelicSlotType, RelicCollectionData> GetEquippedRelics()
    {
        var result = new Dictionary<RelicSlotType, RelicCollectionData>();
        foreach (var kvp in _playerData.EquippedRelics)
        {
            var relic = GetRelic(kvp.Value);
            if (relic != null)
                result[kvp.Key] = relic;
        }
        return result;
    }

    /// <summary>
    /// 获得圣物
    /// </summary>
    public bool AcquireRelic(string relicId)
    {
        var relic = GetRelic(relicId);
        if (relic == null)
        {
            GD.PrintErr($"[RelicCollectionSystem] Relic not found: {relicId}");
            return false;
        }

        if (_playerData.OwnedRelicIds.Contains(relicId))
        {
            GD.Print($"[RelicCollectionSystem] Already owned: {relicId}");
            return false;
        }

        _playerData.OwnedRelicIds.Add(relicId);
        _playerData.TotalRelicsOwned++;
        UpdateCollectionScore();

        GD.Print($"[RelicCollectionSystem] Acquired relic: {relic.Name} ({relic.Rarity})");
        return true;
    }

    /// <summary>
    /// 装备圣物到槽位
    /// </summary>
    public bool EquipRelic(string relicId, RelicSlotType slotType)
    {
        if (!_playerData.OwnedRelicIds.Contains(relicId))
        {
            GD.Print($"[RelicCollectionSystem] Cannot equip - not owned: {relicId}");
            return false;
        }

        var relic = GetRelic(relicId);
        if (relic == null)
        {
            GD.PrintErr($"[RelicCollectionSystem] Relic not found: {relicId}");
            return false;
        }

        if (relic.SlotType != slotType)
        {
            GD.Print($"[RelicCollectionSystem] Relic slot mismatch: {relic.SlotType} != {slotType}");
            return false;
        }

        // 如果槽位已有圣物，先卸下
        if (_playerData.EquippedRelics.ContainsKey(slotType))
        {
            UnequipRelic(slotType);
        }

        _playerData.EquippedRelics[slotType] = relicId;
        ApplyRelicEffects();

        GD.Print($"[RelicCollectionSystem] Equipped {relic.Name} to {slotType}");
        return true;
    }

    /// <summary>
    /// 卸下槽位的圣物
    /// </summary>
    public bool UnequipRelic(RelicSlotType slotType)
    {
        if (!_playerData.EquippedRelics.ContainsKey(slotType))
        {
            GD.Print($"[RelicCollectionSystem] No relic equipped in slot: {slotType}");
            return false;
        }

        var relicId = _playerData.EquippedRelics[slotType];
        var relic = GetRelic(relicId);

        if (relic != null)
        {
            GD.Print($"[RelicCollectionSystem] Unequipped {relic.Name} from {slotType}");
        }

        _playerData.EquippedRelics.Remove(slotType);
        ApplyRelicEffects();
        return true;
    }

    /// <summary>
    /// 检查圣物是否已拥有
    /// </summary>
    public bool HasRelic(string relicId)
    {
        return _playerData.OwnedRelicIds.Contains(relicId);
    }

    /// <summary>
    /// 检查槽位是否已装备
    /// </summary>
    public bool IsSlotEquipped(RelicSlotType slotType)
    {
        return _playerData.EquippedRelics.ContainsKey(slotType);
    }

    /// <summary>
    /// 获取收集完成度
    /// </summary>
    public float GetCollectionProgress()
    {
        return _relicDatabase.Count > 0 ? (float)_playerData.TotalRelicsOwned / _relicDatabase.Count : 0f;
    }

    /// <summary>
    /// 获取收集分数
    /// </summary>
    public int GetCollectionScore()
    {
        return _playerData.CollectionScore;
    }

    private void UpdateCollectionScore()
    {
        int score = 0;
        foreach (var relicId in _playerData.OwnedRelicIds)
        {
            var relic = GetRelic(relicId);
            if (relic != null)
            {
                score += (int)(100 * relic.GetRarityMultiplier());
            }
        }
        _playerData.CollectionScore = score;
    }

    /// <summary>
    /// 应用所有已装备圣物的效果
    /// </summary>
    private void ApplyRelicEffects()
    {
        var player = GetTree().CurrentScene?.GetNodeOrNull<Player>("Player");
        if (player == null) return;

        // 重置所有加成
        player.RelicAttackBonus = 0;
        player.RelicDefenseBonus = 0;
        player.RelicHealthBonus = 0;
        player.RelicSpeedBonus = 0;
        player.RelicCritRateBonus = 0;
        player.RelicCritDamageBonus = 0;
        player.RelicLifestealBonus = 0;
        player.RelicDodgeBonus = 0;

        // 应用已装备圣物的效果
        foreach (var kvp in _playerData.EquippedRelics)
        {
            var relic = GetRelic(kvp.Value);
            if (relic == null) continue;

            foreach (var effect in relic.Effects)
            {
                if (effect is StatModifierEffect statEffect)
                {
                    effect.Apply(player);
                }
            }
        }
    }

    #endregion

    #region Persistence

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();

        // 拥有的圣物
        data["owned_relics"] = new List<string>(_playerData.OwnedRelicIds);

        // 已装备的圣物
        var equippedList = new List<Dictionary<string, object>>();
        foreach (var kvp in _playerData.EquippedRelics)
        {
            equippedList.Add(new Dictionary<string, object>
            {
                { "slot", kvp.Key.ToString() },
                { "relic_id", kvp.Value }
            });
        }
        data["equipped_relics"] = equippedList;

        // 圣物等级
        data["relic_levels"] = _playerData.RelicLevels;

        // 收集分数
        data["collection_score"] = _playerData.CollectionScore;

        // 总拥有数量
        data["total_owned"] = _playerData.TotalRelicsOwned;

        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;

        // 加载拥有的圣物
        if (data.Contains("owned_relics"))
        {
            var ownedArray = (Array)data["owned_relics"];
            _playerData.OwnedRelicIds.Clear();
            foreach (var relicId in ownedArray)
            {
                _playerData.OwnedRelicIds.Add(relicId.ToString());
            }
        }

        // 加载已装备的圣物
        if (data.Contains("equipped_relics"))
        {
            var equippedList = (Array)data["equipped_relics"];
            _playerData.EquippedRelics.Clear();
            foreach (Dictionary<string, object> equippedData in equippedList)
            {
                if (equippedData.Contains("slot") && equippedData.Contains("relic_id"))
                {
                    var slotStr = equippedData["slot"].ToString();
                    var relicId = equippedData["relic_id"].ToString();
                    if (Enum.TryParse<RelicSlotType>(slotStr, out var slotType))
                    {
                        _playerData.EquippedRelics[slotType] = relicId;
                    }
                }
            }
        }

        // 加载圣物等级
        if (data.Contains("relic_levels"))
        {
            var levelsData = data["relic_levels"] as Dictionary;
            if (levelsData != null)
            {
                _playerData.RelicLevels.Clear();
                foreach (var kvp in levelsData)
                {
                    _playerData.RelicLevels[kvp.Key.ToString()] = Convert.ToInt32(kvp.Value);
                }
            }
        }

        // 加载收集分数
        if (data.Contains("collection_score"))
            _playerData.CollectionScore = Convert.ToInt32(data["collection_score"]);

        // 加载总拥有数量
        if (data.Contains("total_owned"))
            _playerData.TotalRelicsOwned = Convert.ToInt32(data["total_owned"]);

        ApplyRelicEffects();
        GD.Print($"[RelicCollectionSystem] Loaded {_playerData.TotalRelicsOwned} relics, score: {_playerData.CollectionScore}");
    }

    #endregion
}
