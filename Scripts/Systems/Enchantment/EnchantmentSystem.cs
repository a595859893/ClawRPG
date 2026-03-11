using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Enchantment
{
    /// <summary>
    /// 附魔管理器 - 处理所有附魔相关逻辑
    /// </summary>
    public class EnchantmentSystem
    {
        private static EnchantmentSystem _instance;
        public static EnchantmentSystem Instance => _instance ??= new EnchantmentSystem();

        // 附魔背包 (存储附魔卷轴)
        private Dictionary<int, int> _enchantmentInventory = new Dictionary<int, int>();

        // 装备附魔映射 (itemInstanceId -> List of enchantments)
        private Dictionary<int, List<EquipmentEnchantment>> _equipmentEnchantments = new Dictionary<int, List<EquipmentEnchantment>>();

        // 信号系统
        public event Action<EquipmentEnchantment> OnEnchantmentAdded;
        public event Action<int, EquipmentEnchantment> OnEnchantmentRemoved;
        public event Action<bool, string> OnEnchantmentResult;

        // 附魔耐久度消耗
        private const int EnchantmentDurabilityCost = 10;
        private const int MaxEnchantmentsPerEquipment = 3;

        public EnchantmentSystem()
        {
            // 初始给予一些附魔卷轴
            AddEnchantmentScroll(1, 5);  // 锋利
            AddEnchantmentScroll(2, 3);  // 锐利
            AddEnchantmentScroll(6, 5);  // 坚固
            AddEnchantmentScroll(7, 3);  // 铁壁
        }

        /// <summary>
        /// 获取附魔背包数量
        /// </summary>
        public int GetEnchantmentCount(int enchantmentId)
        {
            return _enchantmentInventory.ContainsKey(enchantmentId) ? _enchantmentInventory[enchantmentId] : 0;
        }

        /// <summary>
        /// 获取所有附魔卷轴
        /// </summary>
        public Dictionary<int, int> GetInventory()
        {
            return new Dictionary<int, int>(_enchantmentInventory);
        }

        /// <summary>
        /// 添加附魔卷轴到背包
        /// </summary>
        public void AddEnchantmentScroll(int enchantmentId, int count = 1)
        {
            if (_enchantmentInventory.ContainsKey(enchantmentId))
                _enchantmentInventory[enchantmentId] += count;
            else
                _enchantmentInventory[enchantmentId] = count;
        }

        /// <summary>
        /// 移除附魔卷轴
        /// </summary>
        public bool RemoveEnchantmentScroll(int enchantmentId, int count = 1)
        {
            if (!_enchantmentInventory.ContainsKey(enchantmentId))
                return false;

            if (_enchantmentInventory[enchantmentId] < count)
                return false;

            _enchantmentInventory[enchantmentId] -= count;
            if (_enchantmentInventory[enchantmentId] <= 0)
                _enchantmentInventory.Remove(enchantmentId);

            return true;
        }

        /// <summary>
        /// 检查是否可以附魔
        /// </summary>
        public bool CanEnchant(int enchantmentId, int playerLevel, int equipmentSlot)
        {
            var enchantment = EnchantmentDatabase.Instance.GetEnchantment(enchantmentId);
            if (enchantment == null)
                return false;

            // 检查玩家等级
            if (playerLevel < enchantment.RequiredPlayerLevel)
                return false;

            // 检查附魔卷轴数量
            if (GetEnchantmentCount(enchantmentId) < 1)
                return false;

            // 检查装备附魔数量限制
            if (_equipmentEnchantments.ContainsKey(equipmentSlot))
            {
                if (_equipmentEnchantments[equipmentSlot].Count >= MaxEnchantmentsPerEquipment)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 执行附魔
        /// </summary>
        public bool Enchant(int enchantmentId, int playerLevel, int equipmentSlot)
        {
            var enchantment = EnchantmentDatabase.Instance.GetEnchantment(enchantmentId);
            if (enchantment == null)
            {
                OnEnchantmentResult?.Invoke(false, "附魔不存在");
                return false;
            }

            // 检查玩家等级
            if (playerLevel < enchantment.RequiredPlayerLevel)
            {
                OnEnchantmentResult?.Invoke(false, $"需要 {enchantment.RequiredPlayerLevel} 级才能使用此附魔");
                return false;
            }

            // 检查附魔卷轴
            if (!RemoveEnchantmentScroll(enchantmentId, 1))
            {
                OnEnchantmentResult?.Invoke(false, "附魔卷轴不足");
                return false;
            }

            // 检查装备附魔数量限制
            if (_equipmentEnchantments.ContainsKey(equipmentSlot))
            {
                if (_equipmentEnchantments[equipmentSlot].Count >= MaxEnchantmentsPerEquipment)
                {
                    // 退还卷轴
                    AddEnchantmentScroll(enchantmentId, 1);
                    OnEnchantmentResult?.Invoke(false, "该装备附魔数量已达上限");
                    return false;
                }
            }

            // 随机判定成功
            var random = new Random();
            if (random.NextDouble() < enchantment.SuccessRate)
            {
                // 附魔成功
                var equipmentEnchantment = new EquipmentEnchantment
                {
                    EnchantmentId = enchantmentId,
                    EnchantmentName = enchantment.Name,
                    Rarity = enchantment.Rarity,
                    Attribute = enchantment.Attribute,
                    AttributeValue = enchantment.AttributeValue,
                    Durability = 100,
                    MaxDurability = 100
                };

                if (!_equipmentEnchantments.ContainsKey(equipmentSlot))
                    _equipmentEnchantments[equipmentSlot] = new List<EquipmentEnchantment>();

                _equipmentEnchantments[equipmentSlot].Add(equipmentEnchantment);

                OnEnchantmentAdded?.Invoke(equipmentEnchantment);
                OnEnchantmentResult?.Invoke(true, $"附魔成功！{enchantment.Name} (+{enchantment.AttributeValue} {GetAttributeName(enchantment.Attribute)})");
                return true;
            }
            else
            {
                // 附魔失败
                OnEnchantmentResult?.Invoke(false, $"附魔失败！卷轴已消耗");
                return false;
            }
        }

        /// <summary>
        /// 移除装备附魔
        /// </summary>
        public bool RemoveEnchantment(int equipmentSlot, int index)
        {
            if (!_equipmentEnchantments.ContainsKey(equipmentSlot))
                return false;

            var enchantments = _equipmentEnchantments[equipmentSlot];
            if (index < 0 || index >= enchantments.Count)
                return false;

            var removed = enchantments[index];
            enchantments.RemoveAt(index);

            OnEnchantmentRemoved?.Invoke(equipmentSlot, removed);
            return true;
        }

        /// <summary>
        /// 获取装备的所有附魔
        /// </summary>
        public List<EquipmentEnchantment> GetEquipmentEnchantments(int equipmentSlot)
        {
            if (!_equipmentEnchantments.ContainsKey(equipmentSlot))
                return new List<EquipmentEnchantment>();

            return new List<EquipmentEnchantment>(_equipmentEnchantments[equipmentSlot]);
        }

        /// <summary>
        /// 获取装备附魔的总属性加成
        /// </summary>
        public float GetTotalAttributeBonus(int equipmentSlot, EnchantmentAttribute attribute)
        {
            float total = 0f;

            if (_equipmentEnchantments.ContainsKey(equipmentSlot))
            {
                foreach (var enchant in _equipmentEnchantments[equipmentSlot])
                {
                    if (enchant.Attribute == attribute || enchant.Attribute == EnchantmentAttribute.AllAttributes)
                    {
                        total += enchant.AttributeValue;
                    }
                }
            }

            return total;
        }

        /// <summary>
        /// 获取所有装备附魔加成
        /// </summary>
        public Dictionary<EnchantmentAttribute, float> GetAllAttributeBonuses(int equipmentSlot)
        {
            var bonuses = new Dictionary<EnchantmentAttribute, float>();

            if (_equipmentEnchantments.ContainsKey(equipmentSlot))
            {
                foreach (var enchant in _equipmentEnchantments[equipmentSlot])
                {
                    if (bonuses.ContainsKey(enchant.Attribute))
                        bonuses[enchant.Attribute] += enchant.AttributeValue;
                    else
                        bonuses[enchant.Attribute] = enchant.AttributeValue;
                }
            }

            return bonuses;
        }

        /// <summary>
        /// 修理附魔
        /// </summary>
        public bool RepairEnchantment(int equipmentSlot, int index, int cost)
        {
            if (!_equipmentEnchantments.ContainsKey(equipmentSlot))
                return false;

            var enchantments = _equipmentEnchantments[equipmentSlot];
            if (index < 0 || index >= enchantments.Count)
                return false;

            var enchant = enchantments[index];
            if (enchant.Durability >= enchant.MaxDurability)
                return false;

            // 消耗金币修理
            var player = Main.Instance?.GetPlayer();
            if (player == null || player.Gold < cost)
                return false;

            player.Gold -= cost;
            enchant.Durability = enchant.MaxDurability;

            OnEnchantmentResult?.Invoke(true, $"附魔已修复！花费 {cost} 金币");
            return true;
        }

        /// <summary>
        /// 序列化附魔数据
        /// </summary>
        public Dictionary<string, object> Serialize()
        {
            var data = new Dictionary<string, object>();

            // 序列化附魔背包
            var inventoryList = new List<Dictionary<string, int>>();
            foreach (var kvp in _enchantmentInventory)
            {
                inventoryList.Add(new Dictionary<string, int>
                {
                    { "id", kvp.Key },
                    { "count", kvp.Value }
                });
            }
            data["inventory"] = inventoryList;

            // 序列化装备附魔
            var equipmentList = new List<Dictionary<string, object>>();
            foreach (var kvp in _equipmentEnchantments)
            {
                var slotEnchantments = new List<Dictionary<string, object>>();
                foreach (var enchant in kvp.Value)
                {
                    slotEnchantments.Add(new Dictionary<string, object>
                    {
                        { "enchantmentId", enchant.EnchantmentId },
                        { "name", enchant.EnchantmentName },
                        { "rarity", (int)enchant.Rarity },
                        { "attribute", (int)enchant.Attribute },
                        { "value", enchant.AttributeValue },
                        { "durability", enchant.Durability },
                        { "maxDurability", enchant.MaxDurability }
                    });
                }
                equipmentList.Add(new Dictionary<string, object>
                {
                    { "slot", kvp.Key },
                    { "enchantments", slotEnchantments }
                });
            }
            data["equipment"] = equipmentList;

            return data;
        }

        /// <summary>
        /// 反序列化附魔数据
        /// </summary>
        public void Deserialize(Dictionary<string, object> data)
        {
            _enchantmentInventory.Clear();
            _equipmentEnchantments.Clear();

            if (data == null)
                return;

            // 反序列化附魔背包
            if (data.ContainsKey("inventory") && data["inventory"] is List<object> inventoryList)
            {
                foreach (var item in inventoryList)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        int id = Convert.ToInt32(dict["id"]);
                        int count = Convert.ToInt32(dict["count"]);
                        _enchantmentInventory[id] = count;
                    }
                }
            }

            // 反序列化装备附魔
            if (data.ContainsKey("equipment") && data["equipment"] is List<object> equipmentList)
            {
                foreach (var item in equipmentList)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        int slot = Convert.ToInt32(dict["slot"]);
                        var enchantments = new List<EquipmentEnchantment>();

                        if (dict.ContainsKey("enchantments") && dict["enchantments"] is List<object> enchantList)
                        {
                            foreach (var e in enchantList)
                            {
                                if (e is Dictionary<string, object> eDict)
                                {
                                    var enchant = new EquipmentEnchantment
                                    {
                                        EnchantmentId = Convert.ToInt32(eDict["enchantmentId"]),
                                        EnchantmentName = eDict["name"].ToString(),
                                        Rarity = (EnchantmentRarity)Convert.ToInt32(eDict["rarity"]),
                                        Attribute = (EnchantmentAttribute)Convert.ToInt32(eDict["attribute"]),
                                        AttributeValue = Convert.ToSingle(eDict["value"]),
                                        Durability = Convert.ToInt32(eDict["durability"]),
                                        MaxDurability = Convert.ToInt32(eDict["maxDurability"])
                                    };
                                    enchantments.Add(enchant);
                                }
                            }
                        }

                        _equipmentEnchantments[slot] = enchantments;
                    }
                }
            }
        }

        private string GetAttributeName(EnchantmentAttribute attribute)
        {
            return attribute switch
            {
                EnchantmentAttribute.Damage => "伤害",
                EnchantmentAttribute.Defense => "防御",
                EnchantmentAttribute.Health => "生命",
                EnchantmentAttribute.Mana => "法力",
                EnchantmentAttribute.CriticalRate => "暴击率",
                EnchantmentAttribute.CriticalDamage => "暴击伤害",
                EnchantmentAttribute.AttackSpeed => "攻击速度",
                EnchantmentAttribute.MoveSpeed => "移动速度",
                EnchantmentAttribute.FireResistance => "火抗",
                EnchantmentAttribute.IceResistance => "冰抗",
                EnchantmentAttribute.LightningResistance => "雷抗",
                EnchantmentAttribute.PoisonResistance => "毒抗",
                EnchantmentAttribute.AllAttributes => "全属性",
                _ => attribute.ToString()
            };
        }
    }
}
