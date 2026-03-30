using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.GemSystem {
    /// <summary>
    /// 宝石数据库 - 定义所有可用宝石
    /// </summary>
    
    public class GemDatabase {
        private static GemDatabase _instance;
        public static GemDatabase Instance => _instance ??= new GemDatabase();
        
        // 宝石ID -> 宝石数据
        private System.Collections.Generic.Dictionary<string, GemData> _gems = new System.Collections.Generic.Dictionary<string, GemData>();
        
        // 按类型索引
        private System.Collections.Generic.Dictionary<GemType, List<GemData>> _gemsByType = new System.Collections.Generic.Dictionary<GemType, List<GemData>>();
        
        // 按稀有度索引
        private System.Collections.Generic.Dictionary<GemRarity, List<GemData>> _gemsByRarity = new System.Collections.Generic.Dictionary<GemRarity, List<GemData>>();
        
        private GemDatabase() {
            InitializeGems();
        }
        
        private void InitializeGems() {
            // ===== 红宝石 (攻击) =====
            AddGem(new GemData("ruby_common_1", "碎裂红宝石", GemType.Ruby, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "attack", 5 } }, 50));
            AddGem(new GemData("ruby_common_2", "红宝石碎片", GemType.Ruby, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "attack", 8 } }, 80));
            AddGem(new GemData("ruby_uncommon_1", "红宝石", GemType.Ruby, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "attack", 15 } }, 200));
            AddGem(new GemData("ruby_uncommon_2", "优质红宝石", GemType.Ruby, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "attack", 20 }, { "critical_rate", 1 } }, 350));
            AddGem(new GemData("ruby_rare_1", "璀璨红宝石", GemType.Ruby, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "attack", 35 }, { "critical_damage", 5 } }, 800));
            AddGem(new GemData("ruby_rare_2", "炽热红宝石", GemType.Ruby, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "attack", 40 }, { "critical_rate", 2 } }, 1000));
            AddGem(new GemData("ruby_epic_1", "火焰红宝石", GemType.Ruby, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "attack", 60 }, { "critical_damage", 10 }, { "attack", 5 } }, 2500));
            AddGem(new GemData("ruby_epic_2", "熔岩红宝石", GemType.Ruby, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "attack", 70 }, { "critical_rate", 5 } }, 3000));
            AddGem(new GemData("ruby_legendary_1", "龙血红宝石", GemType.Ruby, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "attack", 120 }, { "critical_damage", 20 }, { "critical_rate", 3 } }, 10000));
            AddGem(new GemData("ruby_legendary_2", "凤凰之血", GemType.Ruby, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "attack", 150 }, { "attack", 30 }, { "critical_damage", 15 } }, 15000));
            
            // ===== 蓝宝石 (防御) =====
            AddGem(new GemData("sapphire_common_1", "碎裂蓝宝石", GemType.Sapphire, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "defense", 5 } }, 50));
            AddGem(new GemData("sapphire_common_2", "蓝宝石碎片", GemType.Sapphire, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "defense", 8 } }, 80));
            AddGem(new GemData("sapphire_uncommon_1", "蓝宝石", GemType.Sapphire, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "defense", 15 } }, 200));
            AddGem(new GemData("sapphire_uncommon_2", "优质蓝宝石", GemType.Sapphire, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "defense", 20 }, { "resilience", 1 } }, 350));
            AddGem(new GemData("sapphire_rare_1", "璀璨蓝宝石", GemType.Sapphire, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "defense", 35 }, { "health", 50 } }, 800));
            AddGem(new GemData("sapphire_rare_2", "冰霜蓝宝石", GemType.Sapphire, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "defense", 40 }, { "resilience", 2 } }, 1000));
            AddGem(new GemData("sapphire_epic_1", "寒冰蓝宝石", GemType.Sapphire, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "defense", 60 }, { "health", 100 }, { "resilience", 3 } }, 2500));
            AddGem(new GemData("sapphire_epic_2", "深海蓝宝石", GemType.Sapphire, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "defense", 70 }, { "health", 150 } }, 3000));
            AddGem(new GemData("sapphire_legendary_1", "泰坦之蓝", GemType.Sapphire, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "defense", 120 }, { "health", 300 }, { "resilience", 5 } }, 10000));
            AddGem(new GemData("sapphire_legendary_2", "世界之核", GemType.Sapphire, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "defense", 150 }, { "defense", 30 }, { "health", 200 } }, 15000));
            
            // ===== 绿宝石 (生命) =====
            AddGem(new GemData("emerald_common_1", "碎裂绿宝石", GemType.Emerald, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "health", 20 } }, 50));
            AddGem(new GemData("emerald_common_2", "绿宝石碎片", GemType.Emerald, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "health", 35 } }, 80));
            AddGem(new GemData("emerald_uncommon_1", "绿宝石", GemType.Emerald, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "health", 60 } }, 200));
            AddGem(new GemData("emerald_uncommon_2", "优质绿宝石", GemType.Emerald, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "health", 80 }, { "defense", 5 } }, 350));
            AddGem(new GemData("emerald_rare_1", "璀璨绿宝石", GemType.Emerald, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "health", 120 }, { "defense", 10 } }, 800));
            AddGem(new GemData("emerald_rare_2", "自然绿宝石", GemType.Emerald, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "health", 150 }, { "resilience", 1 } }, 1000));
            AddGem(new GemData("emerald_epic_1", "森林绿宝石", GemType.Emerald, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "health", 250 }, { "defense", 20 }, { "health", 20 } }, 2500));
            AddGem(new GemData("emerald_epic_2", "生命绿宝石", GemType.Emerald, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "health", 300 }, { "resilience", 3 } }, 3000));
            AddGem(new GemData("emerald_legendary_1", "世界之绿", GemType.Emerald, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "health", 500 }, { "defense", 30 }, { "resilience", 5 } }, 10000));
            AddGem(new GemData("emerald_legendary_2", "自然之心", GemType.Emerald, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "health", 600 }, { "health", 100 }, { "defense", 20 } }, 15000));
            
            // ===== 钻石 (暴击) =====
            AddGem(new GemData("diamond_common_1", "碎裂钻石", GemType.Diamond, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "critical_rate", 1 } }, 80));
            AddGem(new GemData("diamond_common_2", "钻石碎片", GemType.Diamond, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "critical_rate", 1.5f } }, 120));
            AddGem(new GemData("diamond_uncommon_1", "钻石", GemType.Diamond, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "critical_rate", 3 }, { "critical_damage", 5 } }, 400));
            AddGem(new GemData("diamond_uncommon_2", "优质钻石", GemType.Diamond, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "critical_rate", 4 }, { "attack", 10 } }, 600));
            AddGem(new GemData("diamond_rare_1", "璀璨钻石", GemType.Diamond, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "critical_rate", 5 }, { "critical_damage", 10 } }, 1200));
            AddGem(new GemData("diamond_rare_2", "闪光钻石", GemType.Diamond, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "critical_rate", 6 }, { "attack", 20 } }, 1500));
            AddGem(new GemData("diamond_epic_1", "永恒钻石", GemType.Diamond, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "critical_rate", 8 }, { "critical_damage", 20 }, { "critical_rate", 1 } }, 3500));
            AddGem(new GemData("diamond_epic_2", "纯净钻石", GemType.Diamond, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "critical_rate", 10 }, { "attack", 30 } }, 4000));
            AddGem(new GemData("diamond_legendary_1", "真理之钻", GemType.Diamond, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "critical_rate", 15 }, { "critical_damage", 30 }, { "critical_rate", 3 } }, 12000));
            AddGem(new GemData("diamond_legendary_2", "创世之光", GemType.Diamond, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "critical_rate", 20 }, { "critical_damage", 50 }, { "attack", 50 } }, 18000));
            
            // ===== 黄宝石 (速度) =====
            AddGem(new GemData("topaz_common_1", "碎裂黄宝石", GemType.Topaz, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "speed", 2 } }, 50));
            AddGem(new GemData("topaz_common_2", "黄宝石碎片", GemType.Topaz, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "speed", 3 } }, 80));
            AddGem(new GemData("topaz_uncommon_1", "黄宝石", GemType.Topaz, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "speed", 5 } }, 200));
            AddGem(new GemData("topaz_uncommon_2", "优质黄宝石", GemType.Topaz, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "speed", 7 }, { "critical_rate", 1 } }, 350));
            AddGem(new GemData("topaz_rare_1", "璀璨黄宝石", GemType.Topaz, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "speed", 10 }, { "attack", 10 } }, 800));
            AddGem(new GemData("topaz_rare_2", "疾风黄宝石", GemType.Topaz, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "speed", 12 }, { "critical_rate", 2 } }, 1000));
            AddGem(new GemData("topaz_epic_1", "闪电黄宝石", GemType.Topaz, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "speed", 18 }, { "attack", 20 }, { "speed", 2 } }, 2500));
            AddGem(new GemData("topaz_epic_2", "风暴黄宝石", GemType.Topaz, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "speed", 22 }, { "critical_rate", 3 } }, 3000));
            AddGem(new GemData("topaz_legendary_1", "极速之星", GemType.Topaz, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "speed", 35 }, { "attack", 40 }, { "critical_rate", 5 } }, 10000));
            AddGem(new GemData("topaz_legendary_2", "时间之晶", GemType.Topaz, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "speed", 50 }, { "speed", 10 }, { "critical_rate", 3 } }, 15000));
            
            // ===== 紫宝石 (魔法) =====
            AddGem(new GemData("amethyst_common_1", "碎裂紫宝石", GemType.Amethyst, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "magic", 5 } }, 50));
            AddGem(new GemData("amethyst_common_2", "紫宝石碎片", GemType.Amethyst, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "magic", 8 } }, 80));
            AddGem(new GemData("amethyst_uncommon_1", "紫宝石", GemType.Amethyst, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "magic", 15 } }, 200));
            AddGem(new GemData("amethyst_uncommon_2", "优质紫宝石", GemType.Amethyst, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "magic", 20 }, { "critical_rate", 1 } }, 350));
            AddGem(new GemData("amethyst_rare_1", "璀璨紫宝石", GemType.Amethyst, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "magic", 35 }, { "critical_damage", 5 } }, 800));
            AddGem(new GemData("amethyst_rare_2", "奥术紫宝石", GemType.Amethyst, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "magic", 40 }, { "magic", 5 } }, 1000));
            AddGem(new GemData("amethyst_epic_1", "魔力紫宝石", GemType.Amethyst, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "magic", 60 }, { "critical_damage", 10 }, { "magic", 10 } }, 2500));
            AddGem(new GemData("amethyst_epic_2", "秘法紫宝石", GemType.Amethyst, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "magic", 70 }, { "critical_rate", 3 } }, 3000));
            AddGem(new GemData("amethyst_legendary_1", "星界之紫", GemType.Amethyst, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "magic", 120 }, { "critical_damage", 20 }, { "critical_rate", 3 } }, 10000));
            AddGem(new GemData("amethyst_legendary_2", "魔法之源", GemType.Amethyst, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "magic", 150 }, { "magic", 30 }, { "critical_damage", 15 } }, 15000));
            
            // ===== 黑曜石 (韧性) =====
            AddGem(new GemData("onyx_common_1", "碎裂黑曜石", GemType.Onyx, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "resilience", 1 } }, 60));
            AddGem(new GemData("onyx_common_2", "黑曜石碎片", GemType.Onyx, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "resilience", 1.5f } }, 100));
            AddGem(new GemData("onyx_uncommon_1", "黑曜石", GemType.Onyx, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "resilience", 3 }, { "defense", 10 } }, 300));
            AddGem(new GemData("onyx_uncommon_2", "优质黑曜石", GemType.Onyx, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "resilience", 4 }, { "health", 50 } }, 450));
            AddGem(new GemData("onyx_rare_1", "璀璨黑曜石", GemType.Onyx, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "resilience", 5 }, { "defense", 20 } }, 900));
            AddGem(new GemData("onyx_rare_2", "坚固黑曜石", GemType.Onyx, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "resilience", 6 }, { "health", 100 } }, 1100));
            AddGem(new GemData("onyx_epic_1", "永韧黑曜石", GemType.Onyx, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "resilience", 8 }, { "defense", 35 }, { "health", 80 } }, 2800));
            AddGem(new GemData("onyx_epic_2", "不屈黑曜石", GemType.Onyx, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "resilience", 10 }, { "health", 150 }, { "defense", 20 } }, 3300));
            AddGem(new GemData("onyx_legendary_1", "意志之岩", GemType.Onyx, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "resilience", 15 }, { "defense", 50 }, { "health", 200 } }, 11000));
            AddGem(new GemData("onyx_legendary_2", "不灭意志", GemType.Onyx, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "resilience", 20 }, { "resilience", 5 }, { "health", 300 } }, 16000));
            
            // ===== 珍珠 (幸运) =====
            AddGem(new GemData("pearl_common_1", "小珍珠", GemType.Pearl, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "luck", 1 } }, 70));
            AddGem(new GemData("pearl_common_2", "珍珠碎片", GemType.Pearl, GemRarity.Common, 
                new System.Collections.Generic.Dictionary<string, float> { { "luck", 2 } }, 110));
            AddGem(new GemData("pearl_uncommon_1", "珍珠", GemType.Pearl, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "luck", 4 }, { "critical_rate", 1 } }, 350));
            AddGem(new GemData("pearl_uncommon_2", "优质珍珠", GemType.Pearl, GemRarity.Uncommon, 
                new System.Collections.Generic.Dictionary<string, float> { { "luck", 5 }, { "health", 30 } }, 500));
            AddGem(new GemData("pearl_rare_1", "璀璨珍珠", GemType.Pearl, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "luck", 7 }, { "critical_rate", 2 }, { "critical_damage", 5 } }, 1000));
            AddGem(new GemData("pearl_rare_2", "幸运珍珠", GemType.Pearl, GemRarity.Rare, 
                new System.Collections.Generic.Dictionary<string, float> { { "luck", 10 }, { "defense", 10 } }, 1300));
            AddGem(new GemData("pearl_epic_1", "命运珍珠", GemType.Pearl, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "luck", 15 }, { "critical_rate", 3 }, { "critical_damage", 10 } }, 3000));
            AddGem(new GemData("pearl_epic_2", "奇迹珍珠", GemType.Pearl, GemRarity.Epic, 
                new System.Collections.Generic.Dictionary<string, float> { { "luck", 20 }, { "attack", 30 }, { "defense", 30 } }, 3500));
            AddGem(new GemData("pearl_legendary_1", "命运之星", GemType.Pearl, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "luck", 30 }, { "critical_rate", 5 }, { "critical_damage", 20 } }, 12000));
            AddGem(new GemData("pearl_legendary_2", "奇迹之源", GemType.Pearl, GemRarity.Legendary, 
                new System.Collections.Generic.Dictionary<string, float> { { "luck", 50 }, { "luck", 10 }, { "critical_rate", 5 } }, 18000));
        }
        
        private void AddGem(GemData gem) {
            _gems[gem.GemId] = gem;
            
            // 按类型索引
            if (!_gemsByType.ContainsKey(gem.Type)) {
                _gemsByType[gem.Type] = new List<GemData>();
            }
            _gemsByType[gem.Type].Add(gem);
            
            // 按稀有度索引
            if (!_gemsByRarity.ContainsKey(gem.Rarity)) {
                _gemsByRarity[gem.Rarity] = new List<GemData>();
            }
            _gemsByRarity[gem.Rarity].Add(gem);
        }
        
        public GemData GetGem(string gemId) {
            return _gems.TryGetValue(gemId, out var gem) ? gem : null;
        }
        
        public List<GemData> GetAllGems() {
            return new List<GemData>(_gems.Values);
        }
        
        public List<GemData> GetGemsByType(GemType type) {
            return _gemsByType.TryGetValue(type, out var gems) ? new List<GemData>(gems) : new List<GemData>();
        }
        
        public List<GemData> GetGemsByRarity(GemRarity rarity) {
            return _gemsByRarity.TryGetValue(rarity, out var gems) ? new List<GemData>(gems) : new List<GemData>();
        }
        
        public List<GemData> GetGemsByTypeAndRarity(GemType type, GemRarity rarity) {
            List<GemData> result = new List<GemData>();
            var typeGems = GetGemsByType(type);
            foreach (var gem in typeGems) {
                if (gem.Rarity == rarity) {
                    result.Add(gem);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 随机获取一颗宝石（用于奖励）
        /// </summary>
        public GemData GetRandomGem(GemRarity minRarity = GemRarity.Common) {
            // 按稀有度权重随机
            float[] weights = { 50, 30, 15, 4, 1 }; // Common, Uncommon, Rare, Epic, Legendary
            int rarityIndex = 0;
            
            float random = GD.Randf() * 100;
            float cumulative = 0;
            
            for (int i = 0; i < weights.Length; i++) {
                if ((int)minRarity <= i) {
                    cumulative += weights[i];
                    if (random <= cumulative) {
                        rarityIndex = i;
                        break;
                    }
                }
            }
            
            GemRarity selectedRarity = (GemRarity)rarityIndex;
            var gems = GetGemsByRarity(selectedRarity);
            
            if (gems.Count > 0) {
                return gems[GD.Rand() % gems.Count];
            }
            
            // 回退到最低稀有度
            gems = GetGemsByRarity(minRarity);
            return gems.Count > 0 ? gems[0] : null;
        }
        
        /// <summary>
        /// 获取指定类型和稀有度的宝石数量
        /// </summary>
        public int GetGemCount(GemType? type = null, GemRarity? rarity = null) {
            if (type.HasValue && rarity.HasValue) {
                return GetGemsByTypeAndRarity(type.Value, rarity.Value).Count;
            } else if (type.HasValue) {
                return GetGemsByType(type.Value).Count;
            } else if (rarity.HasValue) {
                return GetGemsByRarity(rarity.Value).Count;
            }
            return _gems.Count;
        }
    }
}
