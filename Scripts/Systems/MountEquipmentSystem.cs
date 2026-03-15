using Godot;
using System;
using System.Collections.Generic;

namespace GameSystems {
    /// <summary>
    /// 坐骑装备系统 - 管理坐骑装备的穿戴、强化和属性加成
    /// </summary>
    // 坐骑装备类型
    public enum MountEquipmentType {
        Saddle,       // 马鞍
        Horseshoe,    // 马蹄铁
        Bridle,       // 缰绳
        Armor,        // 护甲
        Accessory     // 配饰
    }

    // 坐骑装备稀有度
    public enum MountEquipmentRarity {
        Common = 1,      // 普通
        Uncommon = 2,    // 优秀
        Rare = 3,        // 稀有
        Epic = 4,        // 史诗
        Legendary = 5    // 传说
    }

    // 坐骑装备数据
    public class MountEquipmentData {
        public string Id;
        public string Name;
        public string Description;
        public MountEquipmentType Type;
        public MountEquipmentRarity Rarity;
        
        // 属性加成
        public float AttackBonus;
        public float DefenseBonus;
        public float SpeedBonus;
        public float HealthBonus;
        public float CriticalRateBonus;
        public float CriticalDamageBonus;
        
        // 价格
        public int Price;
        
        public MountEquipmentData() {
            Id = "";
            Name = "";
            Description = "";
            Type = MountEquipmentType.Saddle;
            Rarity = MountEquipmentRarity.Common;
            AttackBonus = 0;
            DefenseBonus = 0;
            SpeedBonus = 0;
            HealthBonus = 0;
            CriticalRateBonus = 0;
            CriticalDamageBonus = 0;
            Price = 0;
        }
    }

    // 玩家坐骑装备数据
    public class PlayerMountEquipmentData {
        public string OwnedEquipmentId;  // 已拥有的装备ID
        public string EquippedMountId;    // 装备到的坐骑ID
        
        public PlayerMountEquipmentData() {
            OwnedEquipmentId = "";
            EquippedMountId = "";
        }
    }

    // 坐骑装备管理器
    public partial class MountEquipmentSystem : BaseSystem {
        public static MountEquipmentSystem Instance { get; private set; }

        // 坐骑装备数据库
        private Dictionary<string, MountEquipmentData> equipmentDatabase = new Dictionary<string, MountEquipmentData>();
        
        // 玩家已拥有的坐骑装备
        private List<PlayerMountEquipmentData> ownedEquipment = new List<PlayerMountEquipmentData>();
        
        // 坐骑装备加成缓存: mountId -> (attack, defense, speed, health, critRate, critDamage)
        private Dictionary<string, float[]> mountEquipmentBonusCache = new Dictionary<string, float[]>();

        public override void _Ready() {
            Instance = this;
            InitializeEquipmentDatabase();
        }

        private void InitializeEquipmentDatabase() {
            // 马鞍装备
            AddEquipment("saddle_wooden", "木质马鞍", "基础的木质马鞍", MountEquipmentType.Saddle, MountEquipmentRarity.Common, 0, 5, 0, 20, 0, 0, 100);
            AddEquipment("saddle_iron", "铁质马鞍", "坚固的铁制马鞍", MountEquipmentType.Saddle, MountEquipmentRarity.Uncommon, 0, 15, 0, 50, 0, 0, 500);
            AddEquipment("saddle_steel", "钢制马鞍", "精炼钢制马鞍", MountEquipmentType.Saddle, MountEquipmentRarity.Rare, 0, 30, 0, 100, 0, 0, 2000);
            AddEquipment("saddle_mithril", "秘银马鞍", "稀有的秘银马鞍", MountEquipmentType.Saddle, MountEquipmentRarity.Epic, 0, 50, 0, 200, 0, 0, 8000);
            AddEquipment("saddle_dragon", "龙皮马鞍", "传说中的龙皮马鞍", MountEquipmentType.Saddle, MountEquipmentRarity.Legendary, 0, 80, 0, 400, 5, 10, 30000);

            // 马蹄铁装备
            AddEquipment("horseshoe_iron", "铁质马蹄铁", "基础马蹄铁", MountEquipmentType.Horseshoe, MountEquipmentRarity.Common, 0, 0, 5, 0, 0, 0, 80);
            AddEquipment("horseshoe_steel", "钢质马蹄铁", "更好的马蹄铁", MountEquipmentType.Horseshoe, MountEquipmentRarity.Uncommon, 0, 0, 15, 0, 0, 0, 400);
            AddEquipment("horseshoe_mithril", "秘银马蹄铁", "稀有马蹄铁", MountEquipmentType.Horseshoe, MountEquipmentRarity.Rare, 0, 0, 30, 0, 0, 0, 1500);
            AddEquipment("horseshoe_phoenix", "凤凰马蹄铁", "传说中的马蹄铁", MountEquipmentType.Horseshoe, MountEquipmentRarity.Legendary, 0, 0, 60, 0, 10, 15, 25000);

            // 缰绳装备
            AddEquipment("bridle_rope", "绳索缰绳", "基础缰绳", MountEquipmentType.Bridle, MountEquipmentRarity.Common, 5, 0, 0, 0, 0, 0, 60);
            AddEquipment("bridle_leather", "皮革缰绳", "更好的缰绳", MountEquipmentType.Bridle, MountEquipmentRarity.Uncommon, 15, 0, 0, 0, 0, 0, 300);
            AddEquipment("bridle_silk", "丝绸缰绳", "精致的丝绸缰绳", MountEquipmentType.Bridle, MountEquipmentRarity.Rare, 30, 0, 0, 0, 0, 0, 1200);
            AddEquipment("bridle_magic", "魔法缰绳", "附魔的魔法缰绳", MountEquipmentType.Bridle, MountEquipmentRarity.Epic, 50, 10, 0, 50, 3, 5, 6000);
            AddEquipment("bridle_legend", "传说缰绳", "传说中的缰绳", MountEquipmentType.Bridle, MountEquipmentRarity.Legendary, 80, 20, 0, 100, 5, 10, 20000);

            // 护甲装备
            AddEquipment("armor_leather", "皮革护甲", "基础护甲", MountEquipmentType.Armor, MountEquipmentRarity.Common, 0, 10, 0, 30, 0, 0, 150);
            AddEquipment("armor_chain", "锁甲", "更好的护甲", MountEquipmentType.Armor, MountEquipmentRarity.Uncommon, 0, 25, 0, 60, 0, 0, 600);
            AddEquipment("armor_plate", "板甲", "坚固的板甲", MountEquipmentType.Armor, MountEquipmentRarity.Rare, 0, 50, 0, 120, 0, 0, 2500);
            AddEquipment("armor_dragon", "龙鳞甲", "稀有龙鳞护甲", MountEquipmentType.Armor, MountEquipmentRarity.Epic, 10, 80, 0, 200, 3, 8, 10000);
            AddEquipment("armor_legend", "传说战甲", "传说中的护甲", MountEquipmentType.Armor, MountEquipmentRarity.Legendary, 20, 120, 0, 350, 5, 15, 40000);

            // 配饰装备
            AddEquipment("accessory_ring", "力量指环", "增加攻击力", MountEquipmentType.Accessory, MountEquipmentRarity.Common, 10, 0, 0, 0, 0, 0, 200);
            AddEquipment("accessory_amulet", "守护护符", "增加防御力", MountEquipmentType.Accessory, MountEquipmentRarity.Common, 0, 10, 0, 20, 0, 0, 200);
            AddEquipment("accessory_boots", "速度之靴", "增加速度", MountEquipmentType.Accessory, MountEquipmentRarity.Uncommon, 0, 0, 20, 0, 0, 0, 500);
            AddEquipment("accessory_crown", "王者皇冠", "增加全属性", MountEquipmentType.Accessory, MountEquipmentRarity.Rare, 30, 30, 10, 80, 3, 5, 3000);
            AddEquipment("accessory_eye", "鹰眼饰品", "增加暴击率", MountEquipmentType.Accessory, MountEquipmentRarity.Rare, 0, 0, 0, 0, 10, 0, 2500);
            AddEquipment("accessory_star", "星辰饰品", "增加暴击伤害", MountEquipmentType.Accessory, MountEquipmentRarity.Epic, 0, 0, 0, 0, 5, 20, 8000);
            AddEquipment("accessory_god", "神器配饰", "传说中的配饰", MountEquipmentType.Accessory, MountEquipmentRarity.Legendary, 50, 50, 30, 200, 10, 20, 50000);

            GD.Print($"[MountEquipmentSystem] Initialized {equipmentDatabase.Count} equipment items");
        }

        private void AddEquipment(string id, string name, string desc, MountEquipmentType type, MountEquipmentRarity rarity,
            float attack, float defense, float speed, float health, float critRate, float critDamage, int price) {
            var data = new MountEquipmentData {
                Id = id,
                Name = name,
                Description = desc,
                Type = type,
                Rarity = rarity,
                AttackBonus = attack,
                DefenseBonus = defense,
                SpeedBonus = speed,
                HealthBonus = health,
                CriticalRateBonus = critRate,
                CriticalDamageBonus = critDamage,
                Price = price
            };
            equipmentDatabase[id] = data;
        }

        // 获取装备数据
        public MountEquipmentData GetEquipmentData(string equipmentId) {
            if (equipmentDatabase.ContainsKey(equipmentId)) {
                return equipmentDatabase[equipmentId];
            }
            return null;
        }

        // 购买装备
        public bool PurchaseEquipment(string equipmentId) {
            var equipment = GetEquipmentData(equipmentId);
            if (equipment == null) {
                GD.Warn($"[MountEquipmentSystem] Equipment not found: {equipmentId}");
                return false;
            }

            // 检查是否已拥有
            if (IsOwned(equipmentId)) {
                GD.Warn($"[MountEquipmentSystem] Already owned: {equipmentId}");
                return false;
            }

            // 检查金币
            var player = GetTree().GetFirstNodeInGroup("Player") as Node;
            if (player == null) return false;

            var goldProperty = player.GetProperty("Gold");
            int currentGold = goldProperty != null ? (int)goldProperty : 0;
            
            if (currentGold < equipment.Price) {
                GD.Warn($"[MountEquipmentSystem] Not enough gold: {currentGold} < {equipment.Price}");
                return false;
            }

            // 扣除金币
            player.SetProperty("Gold", currentGold - equipment.Price);

            // 添加到已拥有列表
            var owned = new PlayerMountEquipmentData {
                OwnedEquipmentId = equipmentId,
                EquippedMountId = ""
            };
            ownedEquipment.Add(owned);

            GD.Print($"[MountEquipmentSystem] Purchased {equipment.Name} for {equipment.Price} gold");
            return true;
        }

        // 检查是否已拥有
        public bool IsOwned(string equipmentId) {
            foreach (var owned in ownedEquipment) {
                if (owned.OwnedEquipmentId == equipmentId) return true;
            }
            return false;
        }

        // 装备到坐骑
        public bool EquipToMount(string equipmentId, string mountId) {
            if (!IsOwned(equipmentId)) {
                GD.Warn($"[MountEquipmentSystem] Not owned: {equipmentId}");
                return false;
            }

            // 先卸下该装备从其他坐骑
            UnequipFromMount(equipmentId);

            // 装备到坐骑
            foreach (var owned in ownedEquipment) {
                if (owned.OwnedEquipmentId == equipmentId) {
                    owned.EquippedMountId = mountId;
                    UpdateMountBonusCache(mountId);
                    var equipment = GetEquipmentData(equipmentId);
                    GD.Print($"[MountEquipmentSystem] Equipped {equipment.Name} to mount {mountId}");
                    return true;
                }
            }
            return false;
        }

        // 从坐骑卸下
        public bool UnequipFromMount(string equipmentId) {
            foreach (var owned in ownedEquipment) {
                if (owned.OwnedEquipmentId == equipmentId && owned.EquippedMountId != "") {
                    string oldMountId = owned.EquippedMountId;
                    owned.EquippedMountId = "";
                    UpdateMountBonusCache(oldMountId);
                    var equipment = GetEquipmentData(equipmentId);
                    GD.Print($"[MountEquipmentSystem] Unequipped {equipment.Name} from mount {oldMountId}");
                    return true;
                }
            }
            return false;
        }

        // 获取坐骑的装备加成
        public float[] GetMountEquipmentBonus(string mountId) {
            if (mountEquipmentBonusCache.ContainsKey(mountId)) {
                return mountEquipmentBonusCache[mountId];
            }
            return new float[] { 0, 0, 0, 0, 0, 0 }; // attack, defense, speed, health, critRate, critDamage
        }

        // 更新坐骑加成缓存
        private void UpdateMountBonusCache(string mountId) {
            float[] bonus = new float[] { 0, 0, 0, 0, 0, 0 };
            
            foreach (var owned in ownedEquipment) {
                if (owned.EquippedMountId == mountId) {
                    var equipment = GetEquipmentData(owned.OwnedEquipmentId);
                    if (equipment != null) {
                        bonus[0] += equipment.AttackBonus;
                        bonus[1] += equipment.DefenseBonus;
                        bonus[2] += equipment.SpeedBonus;
                        bonus[3] += equipment.HealthBonus;
                        bonus[4] += equipment.CriticalRateBonus;
                        bonus[5] += equipment.CriticalDamageBonus;
                    }
                }
            }
            
            mountEquipmentBonusCache[mountId] = bonus;
        }

        // 获取坐骑已装备的物品
        public List<MountEquipmentData> GetEquippedItems(string mountId) {
            List<MountEquipmentData> equipped = new List<MountEquipmentData>();
            
            foreach (var owned in ownedEquipment) {
                if (owned.EquippedMountId == mountId) {
                    var equipment = GetEquipmentData(owned.OwnedEquipmentId);
                    if (equipment != null) {
                        equipped.Add(equipment);
                    }
                }
            }
            
            return equipped;
        }

        // 获取玩家已拥有的所有装备
        public List<MountEquipmentData> GetOwnedEquipment() {
            List<MountEquipmentData> owned = new List<MountEquipmentData>();
            
            foreach (var item in ownedEquipment) {
                var equipment = GetEquipmentData(item.OwnedEquipmentId);
                if (equipment != null) {
                    owned.Add(equipment);
                }
            }
            
            return owned;
        }

        // 获取指定类型的装备
        public List<MountEquipmentData> GetEquipmentByType(MountEquipmentType type) {
            List<MountEquipmentData> result = new List<MountEquipmentData>();
            
            foreach (var item in equipmentDatabase.Values) {
                if (item.Type == type) {
                    result.Add(item);
                }
            }
            
            return result;
        }

        // 获取指定稀有度的装备
        public List<MountEquipmentData> GetEquipmentByRarity(MountEquipmentRarity rarity) {
            List<MountEquipmentData> result = new List<MountEquipmentData>();
            
            foreach (var item in equipmentDatabase.Values) {
                if (item.Rarity == rarity) {
                    result.Add(item);
                }
            }
            
            return result;
        }

        // 保存数据
        public Dictionary<string, object> SaveData() {
            Dictionary<string, object> data = new Dictionary<string, object>();
            
            List<Dictionary<string, string>> ownedList = new List<Dictionary<string, string>>();
            foreach (var item in ownedEquipment) {
                ownedList.Add(new Dictionary<string, string> {
                    { "id", item.OwnedEquipmentId },
                    { "mount", item.EquippedMountId }
                });
            }
            data["owned"] = ownedList;
            
            return data;
        }

        // 加载数据
        public void LoadData(Dictionary<string, object> data) {
            if (data == null) return;
            
            ownedEquipment.Clear();
            mountEquipmentBonusCache.Clear();
            
            if (data.ContainsKey("owned")) {
                var ownedList = data["owned"] as List<object>;
                if (ownedList != null) {
                    foreach (var item in ownedList) {
                        var dict = item as Dictionary<string, object>;
                        if (dict != null) {
                            var owned = new PlayerMountEquipmentData {
                                OwnedEquipmentId = dict["id"].ToString(),
                                EquippedMountId = dict["mount"].ToString()
                            };
                            ownedEquipment.Add(owned);
                            
                            // 更新缓存
                            if (owned.EquippedMountId != "") {
                                UpdateMountBonusCache(owned.EquippedMountId);
                            }
                        }
                    }
                }
            }
            
            GD.Print($"[MountEquipmentSystem] Loaded {ownedEquipment.Count} owned equipment");
        }
    }
}
