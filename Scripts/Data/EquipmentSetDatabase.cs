using Godot;
using System;
using System.Collections.Generic;
using Game.EquipmentSetDataSpace;

namespace Game
{
    /// <summary>
    /// 装备套装数据库
    /// </summary>
    public class EquipmentSetDatabase
    {
        private static EquipmentSetDatabase _instance;
        public static EquipmentSetDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EquipmentSetDatabase();
                return _instance;
            }
        }

        // 套装定义
        private Dictionary<string, EquipmentSet> _sets = new Dictionary<string, EquipmentSet>();

        // 按类型索引
        private Dictionary<SetType, List<EquipmentSet>> _setsByType = new Dictionary<SetType, List<EquipmentSet>>();

        // 按稀有度索引
        private Dictionary<SetRarity, List<EquipmentSet>> _setsByRarity = new Dictionary<SetRarity, List<EquipmentSet>>();

        public EquipmentSetDatabase()
        {
            InitializeSets();
        }

        private void InitializeSets()
        {
            // 战士套装 - 力量
            CreateSet("set_warrior_fire", "火焰战士", "燃烧的战甲", SetType.Armor, SetRarity.Epic,
                new List<SetItemData>
                {
                    new SetItemData { ItemId = "fire_sword", Name = "火焰剑", Type = SetType.Weapon },
                    new SetItemData { ItemId = "fire_helmet", Name = "火焰头盔", Type = SetType.Armor },
                    new SetItemData { ItemId = "fire_armor", Name = "火焰胸甲", Type = SetType.Armor },
                    new SetItemData { ItemId = "fire_boots", Name = "火焰靴子", Type = SetType.Armor },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "攻击+15%，火焰伤害+10", AttackBonus = 0.15f },
                    new SetBonusData { PieceCount = 4, Description = "攻击+30%，火焰伤害+25%，攻击时概率触发燃烧", AttackBonus = 0.30f },
                });

            // 法师套装 - 智慧
            CreateSet("set_mage_frost", "冰霜法师", "冰封的智慧", SetType.Mixed, SetRarity.Epic,
                new List<SetItemData>
                {
                    new SetItemData { ItemId = "frost_staff", Name = "冰霜法杖", Type = SetType.Weapon },
                    new SetItemData { ItemId = "frost_robe", Name = "冰霜长袍", Type = SetType.Armor },
                    new SetItemData { ItemId = "frost_amulet", Name = "冰霜护符", Type = SetType.Accessory },
                    new SetItemData { ItemId = "frost_ring", Name = "冰霜戒指", Type = SetType.Accessory },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "魔法+20%，冰霜伤害+15", MagicBonus = 0.20f },
                    new SetBonusData { PieceCount = 4, Description = "魔法+40%，冰霜伤害+30%，暴击率+10%", MagicBonus = 0.40f, CritRateBonus = 0.10f },
                });

            // 刺客套装 - 速度
            CreateSet("set_assassin_shadow", "暗影刺客", "无形的杀手", SetType.Mixed, SetRarity.Legendary,
                new List<SetItemData>
                {
                    new SetItemData { ItemId = "shadow_dagger", Name = "暗影匕首", Type = SetType.Weapon },
                    new SetItemData { ItemId = "shadow_cloak", Name = "暗影斗篷", Type = SetType.Armor },
                    new SetItemData { ItemId = "shadow_boots", Name = "暗影靴子", Type = SetType.Armor },
                    new SetItemData { ItemId = "shadow_claw", Name = "暗影手套", Type = SetType.Accessory },
                    new SetItemData { ItemId = "shadow_mask", Name = "暗影面具", Type = SetType.Accessory },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "速度+20%，闪避+10%", SpeedBonus = 0.20f, DodgeBonus = 0.10f },
                    new SetBonusData { PieceCount = 3, Description = "速度+30%，暴击率+15%", SpeedBonus = 0.30f, CritRateBonus = 0.15f },
                    new SetBonusData { PieceCount = 5, Description = "速度+50%，暴击伤害+30%，攻击必定命中", SpeedBonus = 0.50f, CritDamageBonus = 0.30f },
                });

            // 圣骑士套装 - 防御
            CreateSet("set_paladin_holy", "神圣骑士", "光明的守护", SetType.Armor, SetRarity.Epic,
                new List<SetItemData>
                {
                    new SetItemData { ItemId = "holy_shield", Name = "神圣盾牌", Type = SetType.Weapon },
                    new SetItemData { ItemId = "holy_plate", Name = "神圣板甲", Type = SetType.Armor },
                    new SetItemData { ItemId = "holy_helmet", Name = "神圣头盔", Type = SetType.Armor },
                    new SetItemData { ItemId = "holy_gauntlets", Name = "神圣护手", Type = SetType.Accessory },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "防御+25%，生命+20%", DefenseBonus = 0.25f, HealthBonus = 0.20f },
                    new SetBonusData { PieceCount = 4, Description = "防御+50%，生命+40%，受到攻击概率无敌", DefenseBonus = 0.50f, HealthBonus = 0.40f },
                });

            // 龙战士套装 - 力量
            CreateSet("set_dragon_warrior", "龙战士", "龙的传人", SetType.Mixed, SetRarity.Legendary,
                new List<SetItemData>
                {
                    new SetItemData { ItemId = "dragon_sword", Name = "龙牙剑", Type = SetType.Weapon },
                    new SetItemData { ItemId = "dragon_helm", Name = "龙鳞头盔", Type = SetType.Armor },
                    new SetItemData { ItemId = "dragon_armor", Name = "龙鳞甲", Type = SetType.Armor },
                    new SetItemData { ItemId = "dragon_boots", Name = "龙鳞靴子", Type = SetType.Armor },
                    new SetItemData { ItemId = "dragon_amulet", Name = "龙心护符", Type = SetType.Accessory },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "攻击+20%，生命偷取+10%", AttackBonus = 0.20f, LifeStealBonus = 0.10f },
                    new SetBonusData { PieceCount = 3, Description = "攻击+35%，防御+15%", AttackBonus = 0.35f, DefenseBonus = 0.15f },
                    new SetBonusData { PieceCount = 5, Description = "攻击+60%，暴击伤害+40%，龙息特效", AttackBonus = 0.60f, CritDamageBonus = 0.40f },
                });

            // 精灵射手套装 - 敏捷
            CreateSet("set_ranger_nature", "精灵射手", "自然之怒", SetType.Mixed, SetRarity.Rare,
                new List<SetItemData>
                {
                    new SetItemData { ItemId = "nature_bow", Name = "精灵弓", Type = SetType.Weapon },
                    new SetItemData { ItemId = "nature_vest", Name = "精灵皮甲", Type = SetType.Armor },
                    new SetItemData { ItemId = "nature_quiver", Name = "精灵箭袋", Type = SetType.Accessory },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "攻击+15%，速度+10%", AttackBonus = 0.15f, SpeedBonus = 0.10f },
                    new SetBonusData { PieceCount = 3, Description = "攻击+30%，暴击率+15%，远程伤害+20%", AttackBonus = 0.30f, CritRateBonus = 0.15f },
                });

            // 召唤师套装 - 魔法
            CreateSet("set_summoner_arcane", "奥术召唤师", "奥秘之力", SetType.Mixed, SetRarity.Rare,
                new List<SetItemData>
                {
                    new SetItemData { ItemId = "arcane_tome", Name = "奥术典籍", Type = SetType.Weapon },
                    new SetItemData { ItemId = "arcane_robe", Name = "奥术长袍", Type = SetType.Armor },
                    new SetItemData { ItemId = "arcane_crystal", Name = "奥术水晶", Type = SetType.Accessory },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "魔法+20%，经验获取+15%", MagicBonus = 0.20f, EXPBonus = 0.15f },
                    new SetBonusData { PieceCount = 3, Description = "魔法+40%，经验获取+30%，召唤物增强", MagicBonus = 0.40f, EXPBonus = 0.30f },
                });

            // 矿工套装 - 采集
            CreateSet("set_miner_dwarven", "矮人矿工", "大地之子", SetType.Accessory, SetRarity.Uncommon,
                new List<SetItemData>
                {
                    new SetItemData { ItemId = "dwarven_pick", Name = "矮人镐", Type = SetType.Weapon },
                    new SetItemData { ItemId = "dwarven_lamp", Name = "矮人灯", Type = SetType.Accessory },
                    new SetItemData { ItemId = "dwarven_gloves", Name = "矮人手套", Type = SetType.Accessory },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "采集效率+25%，金币获取+10%", GoldBonus = 0.10f },
                    new SetBonusData { PieceCount = 3, Description = "采集效率+50%，金币获取+25%，rare材料掉落率提升", GoldBonus = 0.25f },
                });

            // 商人套装 - 交易
            CreateSet("set_merchant_gilded", "黄金商人", "财富之道", SetType.Accessory, SetRarity.Uncommon,
                new List<SetItemData>
                {
                    new SetItemData { ItemId = "gold_scales", Name = "黄金天平", Type = SetType.Accessory },
                    new SetItemData { ItemId = "gold_pouch", Name = "黄金钱包", Type = SetType.Accessory },
                    new SetItemData { ItemId = "gold_ring", Name = "黄金戒指", Type = SetType.Accessory },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "交易价格+15%，金币获取+20%", GoldBonus = 0.20f },
                    new SetBonusData { PieceCount = 3, Description = "交易价格+30%，金币获取+40%，稀有物品发现率提升", GoldBonus = 0.40f },
                });

            // 学者套装 - 知识
            CreateSet("set_scholar_ancient", "古老学者", "智慧之源", SetType.Mixed, SetRarity.Rare,
                new SetItemData[]
                {
                    new SetItemData { ItemId = "ancient_tome", Name = "古老典籍", Type = SetType.Weapon },
                    new SetItemData { ItemId = "ancient_robes", Name = "古老长袍", Type = SetType.Armor },
                    new SetItemData { ItemId = "ancient_glasses", Name = "古老眼镜", Type = SetType.Accessory },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "经验获取+20%，魔法+10%", EXPBonus = 0.20f, MagicBonus = 0.10f },
                    new SetBonusData { PieceCount = 3, Description = "经验获取+40%，所有技能冷却-10%", EXPBonus = 0.40f },
                });

            // 渔民套装 - 钓鱼
            CreateSet("set_fisher_trident", "海王", "海洋之力", SetType.Weapon, SetRarity.Uncommon,
                new SetItemData[]
                {
                    new SetItemData { ItemId = "trident", Name = "三叉戟", Type = SetType.Weapon },
                    new SetItemData { ItemId = "fishing_hat", Name = "渔夫帽", Type = SetType.Accessory },
                    new SetItemData { ItemId = "fishing_vest", Name = "渔夫背心", Type = SetType.Armor },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "钓鱼经验+30%，珍稀鱼捕获率+15%" },
                    new SetBonusData { PieceCount = 3, Description = "钓鱼经验+60%，传说鱼捕获率+10%，金币+15%", GoldBonus = 0.15f },
                });

            // 炼金师套装 - 炼金
            CreateSet("set_alchemist_elixir", "炼金大师", "转化之力", SetType.Mixed, SetRarity.Rare,
                new SetItemData[]
                {
                    new SetItemData { ItemId = "elixir_staff", Name = "炼金法杖", Type = SetType.Weapon },
                    new SetItemData { ItemId = "elixir_pouch", Name = "炼金袋", Type = SetType.Accessory },
                    new SetItemData { ItemId = "elixir_apron", Name = "炼金围裙", Type = SetType.Armor },
                },
                new List<SetBonusData>
                {
                    new SetBonusData { PieceCount = 2, Description = "炼金成功率+15%，材料掉落+20%" },
                    new SetBonusData { PieceCount = 3, Description = "炼金成功率+30%，稀有材料掉落+15%，经验+25%", EXPBonus = 0.25f },
                });
        }

        private void CreateSet(string setId, string name, string description, SetType type, SetRarity rarity,
            List<SetItemData> items, List<SetBonusData> bonuses)
        {
            var set = new EquipmentSet
            {
                SetId = setId,
                Name = name,
                Description = description,
                Type = type,
                Rarity = rarity
            };

            foreach (var item in items)
            {
                set.Items.Add(new SetItem
                {
                    ItemId = item.ItemId,
                    Name = item.Name,
                    Type = item.Type,
                    Rarity = rarity
                });
            }

            foreach (var bonus in bonuses)
            {
                set.Bonuses.Add(new SetBonus
                {
                    PieceCount = bonus.PieceCount,
                    Description = bonus.Description,
                    AttackBonus = bonus.AttackBonus,
                    DefenseBonus = bonus.DefenseBonus,
                    HealthBonus = bonus.HealthBonus,
                    MagicBonus = bonus.MagicBonus,
                    SpeedBonus = bonus.SpeedBonus,
                    CritRateBonus = bonus.CritRateBonus,
                    CritDamageBonus = bonus.CritDamageBonus,
                    LifeStealBonus = bonus.LifeStealBonus,
                    DodgeBonus = bonus.DodgeBonus,
                    EXPBonus = bonus.EXPBonus,
                    GoldBonus = bonus.GoldBonus
                });
            }

            _sets[setId] = set;

            // 索引
            if (!_setsByType.ContainsKey(type))
                _setsByType[type] = new List<EquipmentSet>();
            _setsByType[type].Add(set);

            if (!_setsByRarity.ContainsKey(rarity))
                _setsByRarity[rarity] = new List<EquipmentSet>();
            _setsByRarity[rarity].Add(set);
        }

        public EquipmentSet GetSet(string setId)
        {
            return _sets.ContainsKey(setId) ? _sets[setId] : null;
        }

        public List<EquipmentSet> GetAllSets()
        {
            return new List<EquipmentSet>(_sets.Values);
        }

        public List<EquipmentSet> GetSetsByType(SetType type)
        {
            return _setsByType.ContainsKey(type) ? _setsByType[type] : new List<EquipmentSet>();
        }

        public List<EquipmentSet> GetSetsByRarity(SetRarity rarity)
        {
            return _setsByRarity.ContainsKey(rarity) ? _setsByRarity[rarity] : new List<EquipmentSet>();
        }

        public string GetSetIdByItemId(string itemId)
        {
            foreach (var set in _sets.Values)
            {
                foreach (var item in set.Items)
                {
                    if (item.ItemId == itemId)
                        return set.SetId;
                }
            }
            return null;
        }

        private class SetItemData
        {
            public string ItemId { get; set; }
            public string Name { get; set; }
            public SetType Type { get; set; }
        }

        private class SetBonusData
        {
            public int PieceCount { get; set; }
            public string Description { get; set; }
            public float AttackBonus { get; set; }
            public float DefenseBonus { get; set; }
            public float HealthBonus { get; set; }
            public float MagicBonus { get; set; }
            public float SpeedBonus { get; set; }
            public float CritRateBonus { get; set; }
            public float CritDamageBonus { get; set; }
            public float LifeStealBonus { get; set; }
            public float DodgeBonus { get; set; }
            public float EXPBonus { get; set; }
            public float GoldBonus { get; set; }
        }
    }
}
