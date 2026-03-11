using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 附魔系统 - 管理装备附魔功能
    /// </summary>
    public class EnchantmentSystem {
        private static EnchantmentSystem _instance;
        public static EnchantmentSystem Instance {
            get {
                if (_instance == null) {
                    _instance = new EnchantmentSystem();
                }
                return _instance;
            }
        }

        // 玩家已解锁的附魔
        private HashSet<int> _unlockedEnchantments;
        // 附魔历史记录（用于成就等）
        private List<EnchantmentData> _enchantmentHistory;
        // 附魔统计
        private Dictionary<EnchantmentRarity, int> _rarityCount;

        public event Action<EnchantmentData> OnEnchantmentUnlocked;
        public event Action<EnchantmentData> OnEnchantmentUsed;

        public EnchantmentSystem() {
            _unlockedEnchantments = new HashSet<int>();
            _enchantmentHistory = new List<EnchantmentData>();
            _rarityCount = new Dictionary<EnchantmentRarity, int>();
            
            InitializeDefaults();
        }

        /// <summary>
        /// 初始化默认附魔
        /// </summary>
        private void InitializeDefaults() {
            // 解锁初始附魔
            _unlockedEnchantments.Add(1);   // 锋利
            _unlockedEnchantments.Add(11);  // 坚固
            _unlockedEnchantments.Add(21);  // 敏捷
        }

        /// <summary>
        /// 解锁附魔
        /// </summary>
        public bool UnlockEnchantment(int enchantmentId) {
            if (_unlockedEnchantments.Contains(enchantmentId)) {
                return false;
            }

            var enchantment = EnchantmentDatabase.Instance.GetEnchantment(enchantmentId);
            if (enchantment == null) return false;

            _unlockedEnchantments.Add(enchantmentId);
            
            // 更新统计
            if (!_rarityCount.ContainsKey(enchantment.Rarity)) {
                _rarityCount[enchantment.Rarity] = 0;
            }
            _rarityCount[enchantment.Rarity]++;

            OnEnchantmentUnlocked?.Invoke(enchantment);
            return true;
        }

        /// <summary>
        /// 检查附魔是否已解锁
        /// </summary>
        public bool IsEnchantmentUnlocked(int enchantmentId) {
            return _unlockedEnchantments.Contains(enchantmentId);
        }

        /// <summary>
        /// 执行附魔
        /// </summary>
        public EnchantResult ApplyEnchantment(Item item, EnchantmentData enchantment, int playerLevel) {
            // 检查等级要求
            if (playerLevel < enchantment.RequiredLevel) {
                return EnchantResult.Fail($"等级不足，需要 {enchantment.RequiredLevel} 级");
            }

            // 检查附魔类型是否匹配
            if (!IsCompatible(item, enchantment)) {
                return EnchantResult.Fail("此附魔不适用于该装备类型");
            }

            // 检查金币
            var player = GameManager.Instance?.Player;
            if (player == null || player.Gold < enchantment.GoldCost) {
                return EnchantResult.Fail($"金币不足，需要 {enchantment.GoldCost} 金币");
            }

            // 扣除金币
            player.ModifyGold(-enchantment.GoldCost);

            // 记录使用
            if (!_unlockedEnchantments.Contains(enchantment.Id)) {
                UnlockEnchantment(enchantment.Id);
            }
            _enchantmentHistory.Add(enchantment);
            OnEnchantmentUsed?.Invoke(enchantment);

            // 应用附魔到物品（这里假设物品有 Enchantments 列表）
            if (item.Enchantments == null) {
                item.Enchantments = new List<EnchantmentData>();
            }
            item.Enchantments.Add(enchantment);

            // 提升物品等级
            int levelBonus = (int)enchantment.Rarity * 2;
            item.Level += levelBonus;

            return EnchantResult.Success(enchantment, $"附魔成功！{enchantment.Name} (+{levelBonus}物品等级)");
        }

        /// <summary>
        /// 移除附魔
        /// </summary>
        public EnchantResult RemoveEnchantment(Item item, EnchantmentData enchantment) {
            if (item.Enchantments == null || !item.Enchantments.Contains(enchantment)) {
                return EnchantResult.Fail("该物品没有此附魔");
            }

            item.Enchantments.Remove(enchantment);
            item.Level = Math.Max(1, item.Level - (int)enchantment.Rarity * 2);

            return EnchantResult.Success(enchantment, $"已移除附魔 {enchantment.Name}");
        }

        /// <summary>
        /// 检查附魔是否与物品兼容
        /// </summary>
        public bool IsCompatible(Item item, EnchantmentData enchantment) {
            if (item == null || enchantment == null) return false;

            // 通用附魔适用于所有类型
            if (enchantment.Type == EnchantmentType.Universal) return true;

            // 根据物品类型检查
            switch (item.Type) {
                case ItemType.Weapon:
                    return enchantment.Type == EnchantmentType.Weapon || 
                           enchantment.Type == EnchantmentType.Universal;
                case ItemType.Armor:
                case ItemType.Helmet:
                case ItemType.Boots:
                case ItemType.Gloves:
                    return enchantment.Type == EnchantmentType.Armor || 
                           enchantment.Type == EnchantmentType.Universal;
                case ItemType.Ring:
                case ItemType.Amulet:
                    return enchantment.Type == EnchantmentType.Accessory || 
                           enchantment.Type == EnchantmentType.Universal;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 计算物品的总附魔属性加成
        /// </summary>
        public Dictionary<EnchantmentAttribute, float> CalculateEnchantmentBonus(Item item) {
            var totalBonus = new Dictionary<EnchantmentAttribute, float>();
            
            if (item?.Enchantments == null) return totalBonus;

            foreach (var enchant in item.Enchantments) {
                foreach (var attr in enchant.Attributes) {
                    if (!totalBonus.ContainsKey(attr.Key)) {
                        totalBonus[attr.Key] = 0;
                    }
                    totalBonus[attr.Key] += attr.Value;
                }
            }

            return totalBonus;
        }

        /// <summary>
        /// 获取已解锁附魔列表
        /// </summary>
        public List<EnchantmentData> GetUnlockedEnchantments() {
            var result = new List<EnchantmentData>();
            foreach (var id in _unlockedEnchantments) {
                var e = EnchantmentDatabase.Instance.GetEnchantment(id);
                if (e != null) result.Add(e);
            }
            return result;
        }

        /// <summary>
        /// 获取附魔统计
        /// </summary>
        public Dictionary<EnchantmentRarity, int> GetRarityStatistics() {
            return new Dictionary<EnchantmentRarity, int>(_rarityCount);
        }

        /// <summary>
        /// 获取附魔历史数量
        /// </summary>
        public int GetEnchantmentHistoryCount() {
            return _enchantmentHistory.Count;
        }

        /// <summary>
        /// 检查是否已收集所有附魔
        /// </summary>
        public bool HasCollectedAllEnchantments() {
            return _unlockedEnchantments.Count >= EnchantmentDatabase.Instance.GetAllEnchantments().Count;
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public Dictionary<string, object> SaveData() {
            var data = new Dictionary<string, object>();
            data["unlockedEnchantments"] = new List<int>(_unlockedEnchantments);
            return data;
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public void LoadData(Dictionary<string, object> data) {
            if (data == null) return;
            
            if (data.ContainsKey("unlockedEnchantments")) {
                var list = data["unlockedEnchantments"] as List<object>;
                if (list != null) {
                    _unlockedEnchantments.Clear();
                    foreach (var item in list) {
                        _unlockedEnchantments.Add(Convert.ToInt32(item));
                    }
                }
            }
        }
    }
}
