using Godot;
using Godot.Collections;
using System;

namespace ClawRPG.Scripts.Systems.GemSystem {
    /// <summary>
    /// 宝石数据结构
    /// </summary>
    
    public enum GemType {
        Ruby,      // 红宝石 - 攻击
        Sapphire,  // 蓝宝石 - 防御
        Emerald,   // 绿宝石 - 生命
        Diamond,   // 钻石 - 暴击
        Topaz,     // 黄宝石 - 速度
        Amethyst,  // 紫宝石 - 魔法
        Onyx,      // 黑曜石 - 韧性
        Pearl      // 珍珠 - 幸运
    }
    
    public enum GemRarity {
        Common,     // 普通
        Uncommon,   // 优秀
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }
    
    [System.Serializable]
    public class GemData {
        public string GemId;
        public string Name;
        public string Description;
        public GemType Type;
        public GemRarity Rarity;
        public System.Collections.Generic.Dictionary<string, float> Attributes; // 属性加成
        public int Value; // 价值金币
        public string Icon; // 图标资源路径
        
        public GemData() {
            Attributes = new System.Collections.Generic.Dictionary<string, float>();
        }
        
        public GemData(string gemId, string name, GemType type, GemRarity rarity, System.Collections.Generic.Dictionary<string, float> attrs, int value) {
            GemId = gemId;
            Name = name;
            Type = type;
            Rarity = rarity;
            Attributes = attrs != null ? new System.Collections.Generic.Dictionary<string, float>(attrs) : new System.Collections.Generic.Dictionary<string, float>();
            Value = value;
            Description = GetDescription();
        }
        
        private string GetDescription() {
            string desc = "";
            foreach (var attr in Attributes) {
                string attrName = attr.Key switch {
                    "attack" => "攻击",
                    "defense" => "防御",
                    "health" => "生命",
                    "critical_rate" => "暴击率",
                    "critical_damage" => "暴击伤害",
                    "speed" => "速度",
                    "magic" => "魔法",
                    "resilience" => "韧性",
                    "luck" => "幸运",
                    _ => attr.Key
                };
                float val = attr.Value;
                if (attr.Key.ContainsKey("rate") || attr.Key.ContainsKey("damage")) {
                    desc += $"{attrName} +{val:F1}% ";
                } else {
                    desc += $"{attrName} +{val} ";
                }
            }
            return desc.Trim();
        }
        
        public static string GetGemTypeName(GemType type) {
            return type switch {
                GemType.Ruby => "红宝石",
                GemType.Sapphire => "蓝宝石",
                GemType.Emerald => "绿宝石",
                GemType.Diamond => "钻石",
                GemType.Topaz => "黄宝石",
                GemType.Amethyst => "紫宝石",
                GemType.Onyx => "黑曜石",
                GemType.Pearl => "珍珠",
                _ => "未知"
            };
        }
        
        public static string GetRarityName(GemRarity rarity) {
            return rarity switch {
                GemRarity.Common => "普通",
                GemRarity.Uncommon => "优秀",
                GemRarity.Rare => "稀有",
                GemRarity.Epic => "史诗",
                GemRarity.Legendary => "传说",
                _ => "未知"
            };
        }
        
        public static Color GetRarityColor(GemRarity rarity) {
            return rarity switch {
                GemRarity.Common => new Color(1f, 1f, 1f),
                GemRarity.Uncommon => new Color(0f, 1f, 0f),
                GemRarity.Rare => new Color(0f, 0.5f, 1f),
                GemRarity.Epic => new Color(0.6f, 0.2f, 1f),
                GemRarity.Legendary => new Color(1f, 0.6f, 0f),
                _ => new Color(1f, 1f, 1f)
            };
        }
    }
    
    /// <summary>
    /// 装备宝石槽位数据
    /// </summary>
    [System.Serializable]
    public class EquipmentGemSlot {
        public int SlotIndex;
        public bool IsUnlocked;
        public string GemId; // 已镶嵌的宝石ID，空字符串表示未镶嵌
        
        public EquipmentGemSlot() {
            GemId = "";
            IsUnlocked = false; 
        }
        
        public EquipmentGemSlot(int index, bool unlocked = false) {
            SlotIndex = index;
            IsUnlocked = unlocked;
            GemId = "";
        }
        
        public bool HasGem => !string.IsNullOrEmpty(GemId);
    }
    
    /// <summary>
    /// 玩家宝石数据
    /// </summary>
    [System.Serializable]
    public class PlayerGemData {
        public System.Collections.Generic.Dictionary<string, int> OwnedGems; // 宝石ID -> 数量
        public System.Collections.Generic.Dictionary<string, List<EquipmentGemSlot>> EquipmentSlots; // 装备ID -> 宝石槽位
        
        public PlayerGemData() {
            OwnedGems = new System.Collections.Generic.Dictionary<string, int>();
            EquipmentSlots = new System.Collections.Generic.Dictionary<string, List<EquipmentGemSlot>>();
        }
        
        public int GetGemCount(string gemId) {
            return OwnedGems.TryGetValue(gemId, out int count) ? count : 0;
        }
        
        public void AddGem(string gemId, int count = 1) {
            if (OwnedGems.ContainsKey(gemId)) {
                OwnedGems[gemId] += count;
            } else {
                OwnedGems[gemId] = count;
            }
        }
        
        public bool RemoveGem(string gemId, int count = 1) {
            if (OwnedGems.TryGetValue(gemId, out int currentCount)) {
                if (currentCount >= count) {
                    OwnedGems[gemId] -= count;
                    if (OwnedGems[gemId] <= 0) {
                        OwnedGems.Remove(gemId);
                    }
                    return true;
                }
            }
            return false;
        }
        
        public List<EquipmentGemSlot> GetOrCreateEquipmentSlots(string equipmentId, int slotCount) {
            if (EquipmentSlots.TryGetValue(equipmentId, out var slots)) {
                // 确保槽位数量足够
                while (slots.Count < slotCount) {
                    slots.Add(new EquipmentGemSlot(slots.Count, slots.Count < 2)); // 默认解锁2个槽位
                }
                return slots;
            }
            
            // 创建新的槽位
            slots = new List<EquipmentGemSlot>();
            for (int i = 0; i < slotCount; i++) {
                slots.Add(new EquipmentGemSlot(i, i < 2)); // 默认解锁2个槽位
            }
            EquipmentSlots[equipmentId] = slots;
            return slots;
        }
    }
}
