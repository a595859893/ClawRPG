using Godot;
using Godot.Collections;
using System;
using System.Linq;
using Array = System.Array;

namespace ClawRPG.Scripts.Systems.GemSystem {
    /// <summary>
    /// 宝石镶嵌系统管理器
    /// </summary>
    
    public partial class GemSystem : BaseSystem {
        private static GemSystem _instance;
        public static GemSystem Instance => _instance ??= GetNode<GemSystem>("/root/GemSystem");

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            _gemDatabase = GemDatabase.Instance;
        }
        
        // 信号系统
        public static event Action<string, string> GemInserted; // 装备ID, 宝石ID
        public static event Action<string, string> GemRemoved; // 装备ID, 宝石ID
        public static event Action<string, int, bool> GemSlotUnlocked; // 装备ID, 槽位索引, 是否成功
        
        // 玩家宝石数据
        private PlayerGemData _playerGemData = new PlayerGemData();
        
        // 宝石数据库
        private GemDatabase _gemDatabase;
        
        // 每个装备类型的默认槽位数
        private System.Collections.Generic.Dictionary<string, int> _defaultSlotCount = new System.Collections.Generic.Dictionary<string, int> {
            { "weapon", 3 },
            { "armor", 4 },
            { "helmet", 2 },
            { "boots", 2 },
            { "gloves", 2 },
            { "accessory", 2 }
        };
        
        private GemSystem() {
            _gemDatabase = GemDatabase.Instance;
        }
        
        protected override void Initialize() {
            base.Initialize();
            LoadData();
        }
        
        /// <summary>
        /// 镶嵌宝石到装备
        /// </summary>
        public bool InsertGem(string equipmentId, string equipmentType, int slotIndex, string gemId) {
            // 验证宝石是否存在
            var gem = _gemDatabase.GetGem(gemId);
            if (gem == null) {
                GD.PrintErr($"[GemSystem] Gem not found: {gemId}");
                return false;
            }
            
            // 验证玩家是否拥有宝石
            if (_playerGemData.GetGemCount(gemId) <= 0) {
                GD.PrintErr($"[GemSystem] Player doesn't have gem: {gemId}");
                return false;
            }
            
            // 获取或创建装备槽位
            int slotCount = _defaultSlotCount.TryGetValue(equipmentType, out int count) ? count : 2;
            var slots = _playerGemData.GetOrCreateEquipmentSlots(equipmentId, slotCount);
            
            // 验证槽位索引
            if (slotIndex < 0 || slotIndex >= slots.Count) {
                GD.PrintErr($"[GemSystem] Invalid slot index: {slotIndex}");
                return false;
            }
            
            // 验证槽位是否已解锁
            var slot = slots[slotIndex];
            if (!slot.IsUnlocked) {
                GD.PrintErr($"[GemSystem] Slot not unlocked: {slotIndex}");
                return false;
            }
            
            // 如果槽位已有宝石，先移除
            if (slot.HasGem) {
                RemoveGemDirect(equipmentId, slotIndex);
            }
            
            // 扣除宝石
            _playerGemData.RemoveGem(gemId, 1);
            
            // 镶嵌宝石
            slot.GemId = gemId;
            
            GemInserted?.Invoke(equipmentId, gemId);
            
            SaveData();
            return true;
        }
        
        /// <summary>
        /// 从装备移除宝石
        /// </summary>
        public bool RemoveGem(string equipmentId, int slotIndex) {
            // 获取装备槽位
            if (!_playerGemData.EquipmentSlots.TryGetValue(equipmentId, out var slots)) {
                GD.PrintErr($"[GemSystem] Equipment not found: {equipmentId}");
                return false;
            }
            
            // 验证槽位索引
            if (slotIndex < 0 || slotIndex >= slots.Count) {
                GD.PrintErr($"[GemSystem] Invalid slot index: {slotIndex}");
                return false;
            }
            
            var slot = slots[slotIndex];
            if (!slot.HasGem) {
                GD.PrintErr($"[GemSystem] Slot is empty: {slotIndex}");
                return false;
            }
            
            string gemId = slot.GemId;
            bool success = RemoveGemDirect(equipmentId, slotIndex);
            
            if (success) {
                GemRemoved?.Invoke(equipmentId, gemId);
                SaveData();
            }
            
            return success;
        }
        
        /// <summary>
        /// 直接移除宝石（不触发信号，用于内部操作）
        /// </summary>
        private bool RemoveGemDirect(string equipmentId, int slotIndex) {
            if (!_playerGemData.EquipmentSlots.TryGetValue(equipmentId, out var slots)) {
                return false;
            }
            
            if (slotIndex < 0 || slotIndex >= slots.Count) {
                return false;
            }
            
            var slot = slots[slotIndex];
            if (!slot.HasGem) {
                return false;
            }
            
            // 返还宝石给玩家
            string gemId = slot.GemId;
            _playerGemData.AddGem(gemId, 1);
            
            // 清空槽位
            slot.GemId = "";
            
            return true;
        }
        
        /// <summary>
        /// 解锁装备槽位
        /// </summary>
        public bool UnlockSlot(string equipmentId, string equipmentType, int slotIndex, int unlockCost = 500) {
            // 获取或创建装备槽位
            int slotCount = _defaultSlotCount.TryGetValue(equipmentType, out int count) ? count : 2;
            var slots = _playerGemData.GetOrCreateEquipmentSlots(equipmentId, slotCount);
            
            // 验证槽位索引
            if (slotIndex < 0 || slotIndex >= slots.Count) {
                GD.PrintErr($"[GemSystem] Invalid slot index: {slotIndex}");
                GemSlotUnlocked?.Invoke(equipmentId, slotIndex, false);
                return false;
            }
            
            var slot = slots[slotIndex];
            
            // 检查是否已解锁
            if (slot.IsUnlocked) {
                GD.PrintErr($"[GemSystem] Slot already unlocked: {slotIndex}");
                GemSlotUnlocked?.Invoke(equipmentId, slotIndex, false);
                return false;
            }
            
            // 检查玩家金币是否足够
            var player = Main.Instance?.GetPlayer();
            if (player == null || player.Gold < unlockCost) {
                GD.PrintErr($"[GemSystem] Not enough gold: need {unlockCost}, have {player?.Gold ?? 0}");
                GemSlotUnlocked?.Invoke(equipmentId, slotIndex, false);
                return false;
            }
            
            // 扣除金币
            player.Gold -= unlockCost;
            
            // 解锁槽位
            slot.IsUnlocked = true;
            
            GemSlotUnlocked?.Invoke(equipmentId, slotIndex, true);
            
            SaveData();
            return true;
        }
        
        /// <summary>
        /// 获取装备的宝石槽位
        /// </summary>
        public List<EquipmentGemSlot> GetEquipmentSlots(string equipmentId, string equipmentType) {
            int slotCount = _defaultSlotCount.TryGetValue(equipmentType, out int count) ? count : 2;
            return _playerGemData.GetOrCreateEquipmentSlots(equipmentId, slotCount);
        }
        
        /// <summary>
        /// 获取装备已镶嵌的宝石属性加成
        /// </summary>
        public System.Collections.Generic.Dictionary<string, float> GetEquipmentGemBonuses(string equipmentId, string equipmentType) {
            var bonuses = new System.Collections.Generic.Dictionary<string, float>();
            var slots = GetEquipmentSlots(equipmentId, equipmentType);
            
            foreach (var slot in slots) {
                if (slot.HasGem && slot.IsUnlocked) {
                    var gem = _gemDatabase.GetGem(slot.GemId);
                    if (gem != null) {
                        foreach (var attr in gem.Attributes) {
                            if (bonuses.ContainsKey(attr.Key)) {
                                bonuses[attr.Key] += attr.Value;
                            } else {
                                bonuses[attr.Key] = attr.Value;
                            }
                        }
                    }
                }
            }
            
            return bonuses;
        }
        
        /// <summary>
        /// 获取玩家拥有的宝石数量
        /// </summary>
        public int GetOwnedGemCount(string gemId) {
            return _playerGemData.GetGemCount(gemId);
        }
        
        /// <summary>
        /// 获取玩家拥有的所有宝石
        /// </summary>
        public System.Collections.Generic.Dictionary<string, int> GetOwnedGems() {
            return new System.Collections.Generic.Dictionary<string, int>(_playerGemData.OwnedGems);
        }
        
        /// <summary>
        /// 添加宝石到玩家背包
        /// </summary>
        public void AddGem(string gemId, int count = 1) {
            _playerGemData.AddGem(gemId, count);
            SaveData();
        }
        
        /// <summary>
        /// 批量添加宝石（用于奖励）
        /// </summary>
        public void AddGems(System.Collections.Generic.Dictionary<string, int> gems) {
            foreach (var gem in gems) {
                _playerGemData.AddGem(gem.Key, gem.Value);
            }
            SaveData();
        }
        
        /// <summary>
        /// 检查装备是否可以镶嵌指定类型的宝石
        /// </summary>
        public bool CanInsertGemType(string equipmentType, GemType gemType) {
            // 武器可以镶嵌任何类型
            if (equipmentType == "weapon") return true;
            
            // 防具主要镶嵌防御/生命/韧性
            if (equipmentType == "armor" || equipmentType == "helmet" || equipmentType == "boots" || equipmentType == "gloves") {
                return gemType == GemType.Sapphire || gemType == GemType.Emerald || gemType == GemType.Onyx;
            }
            
            // 饰品可以镶嵌任何类型
            if (equipmentType == "accessory") return true;
            
            return true;
        }
        
        /// <summary>
        /// 获取指定装备类型的可用槽位数
        /// </summary>
        public int GetSlotCount(string equipmentType) {
            return _defaultSlotCount.TryGetValue(equipmentType, out int count) ? count : 2;
        }
        
        /// <summary>
        /// 获取宝石详情
        /// </summary>
        public GemData GetGem(string gemId) {
            return _gemDatabase.GetGem(gemId);
        }
        
        /// <summary>
        /// 获取所有可用宝石
        /// </summary>
        public List<GemData> GetAllGems() {
            return _gemDatabase.GetAllGems();
        }
        
        /// <summary>
        /// 获取指定类型的宝石
        /// </summary>
        public List<GemData> GetGemsByType(GemType type) {
            return _gemDatabase.GetGemsByType(type);
        }
        
        /// <summary>
        /// 获取指定稀有度的宝石
        /// </summary>
        public List<GemData> GetGemsByRarity(GemRarity rarity) {
            return _gemDatabase.GetGemsByRarity(rarity);
        }
        
        /// <summary>
        /// 随机获取一颗宝石（用于奖励）
        /// </summary>
        public GemData GetRandomGem(GemRarity minRarity = GemRarity.Common) {
            return _gemDatabase.GetRandomGem(minRarity);
        }
        
        /// <summary>
        /// 获取所有已镶嵌宝石的装备ID列表
        /// </summary>
        public List<string> GetEquippedEquipmentIds() {
            return _playerGemData.EquipmentSlots.Keys.ToList();
        }
        
        /// <summary>
        /// 导出保存数据（继承自 BaseSystem）
        /// </summary>
        public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary {
                { "owned_gems", _playerGemData.OwnedGems },
                { "equipment_slots", SaveEquipmentSlots() }
            };
            return data;
        }
        
        /// <summary>
        /// 保存装备槽位数据
        /// </summary>
        private Dictionary SaveEquipmentSlots() {
            var data = new System.Collections.Generic.Dictionary<string, object>();
            foreach (var kvp in _playerGemData.EquipmentSlots) {
                var slots = new Godot.Collections.Array();
                foreach (var slot in kvp.Value) {
                    var slotData = new Dictionary {
                        { "index", slot.SlotIndex },
                        { "unlocked", slot.IsUnlocked },
                        { "gem_id", slot.GemId }
                    };
                    slots.Add(slotData);
                }
                data[kvp.Key] = slots;
            }
            return data;
        }
        
        /// <summary>
        /// 导入保存数据（继承自 BaseSystem）
        /// </summary>
        public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.Contains("owned_gems")) {
                var gems = data["owned_gems"] as Dictionary;
                _playerGemData.OwnedGems = new System.Collections.Generic.Dictionary<string, int>();
                foreach (var key in gems.Keys) {
                    _playerGemData.OwnedGems[key.ToString()] = (int)gems[key];
                }
            }
            
            if (data.Contains("equipment_slots")) {
                LoadEquipmentSlots(data["equipment_slots"] as Dictionary);
            }
        }
        
        /// <summary>
        /// 加载装备槽位数据
        /// </summary>
        private void LoadEquipmentSlots(Dictionary data) {
            if (data == null) return;
            
            _playerGemData.EquipmentSlots = new System.Collections.Generic.Dictionary<string, List<EquipmentGemSlot>>();
            foreach (var key in data.Keys) {
                var slotsArray = data[key] as Array;
                var slots = new List<EquipmentGemSlot>();
                
                foreach (var slotData in slotsArray) {
                    var sd = slotData as Dictionary;
                    var slot = new EquipmentGemSlot {
                        SlotIndex = (int)sd["index"],
                        IsUnlocked = (bool)sd["unlocked"],
                        GemId = sd["gem_id"].ToString()
                    };
                    slots.Add(slot);
                }
                
                _playerGemData.EquipmentSlots[key.ToString()] = slots;
            }
        }
}
}
