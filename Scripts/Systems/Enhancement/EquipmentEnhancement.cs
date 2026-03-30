using Godot;
using Godot.Collections;
using System;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems.Enhancement {
    /// <summary>
    /// 装备强化系统 - 强化数据和管理器
    /// </summary>
    
    public enum EnhancementType {
        Weapon,
        Armor,
        Accessory
    }
    
    public enum EnhancementResult {
        Success,
        Failed,
        MaxLevel
    }
    
    [System.Serializable]
    public class EnhancementMaterial {
        public string ItemId;
        public int Count;
        
        public EnhancementMaterial() {}
        
        public EnhancementMaterial(string itemId, int count) {
            ItemId = itemId;
            Count = count;
        }
    }
    
    [System.Serializable]
    public class EnhancementData {
        public string ItemId;
        public int EnhancementLevel;
        public int TryCount;
        
        public EnhancementData() {
            EnhancementLevel = 0;
            TryCount = 0;
        }
        
        public EnhancementData(string itemId) {
            ItemId = itemId;
            EnhancementLevel = 0;
            TryCount = 0;
        }
    }
    
    public class EnhancementSystem : BaseSystem {
        public static EnhancementSystem Instance { get; private set; }
        
        // 强化配置
        private const int MaxEnhancementLevel = 10;
        private const float BaseSuccessRate = 0.95f;
        private const float LevelFailurePenalty = 0.08f; // 每级失败率增加8%
        
        // 信号
        public delegate void OnEnhancementStarted(string itemId, int level);
        
        public delegate void OnEnhancementComplete(string itemId, int level, EnhancementResult result);
        
        public delegate void OnEnhancementMaterialsChanged();
        
        // 强化石物品ID (与ItemSystem中的ID对应)
        public const string EnhanceStoneCommon = "401";
        public const string EnhanceStoneUncommon = "402";
        public const string EnhanceStoneRare = "403";
        public const string EnhanceStoneEpic = "404";
        public const string EnhanceStoneLegendary = "405";
        
        // 玩家数据
        private System.Collections.Generic.Dictionary<string, EnhancementData> _enhancedItems = new();
        private Player _player;
        
        // Tutorial tracking
        private bool _hasTriggeredFirstEnhance = false; 
        
        public override void _Ready() {
            Instance = this;
        }
        
        public void Initialize(Player player) {
            _player = player;
        }
        
        /// <summary>
        /// 获取强化成功率
        /// </summary>
        public float GetSuccessRate(int currentLevel, string stoneId) {
            float baseRate = BaseSuccessRate - (currentLevel * LevelFailurePenalty);
            
            // 强化石品质加成
            float stoneBonus = 0f;
            switch (stoneId) {
                case "401": stoneBonus = 0f; break;
                case "402": stoneBonus = 0.05f; break;
                case "403": stoneBonus = 0.10f; break;
                case "404": stoneBonus = 0.15f; break;
                case "405": stoneBonus = 0.25f; break;
                default: stoneBonus = 0f; break;
            }
            
            return Mathf.Clamp(baseRate + stoneBonus, 0.1f, 0.99f);
        }
        
        /// <summary>
        /// 获取强化所需材料
        /// </summary>
        public System.Collections.Generic.Dictionary<string, int> GetRequiredMaterials(int level, EnhancementType type) {
            var materials = new System.Collections.Generic.Dictionary<string, int>();
            
            // 基础材料数量基于等级和类型
            int baseCount = level + 1;
            int multiplier = type switch {
                EnhancementType.Weapon => 2,
                EnhancementType.Armor => 2,
                EnhancementType.Accessory => 1,
                _ => 1
            };
            
            materials["401"] = baseCount * multiplier;
            
            // 高级强化石需求
            if (level >= 3) {
                materials["402"] = 1;
            }
            if (level >= 5) {
                materials["403"] = 1;
            }
            if (level >= 7) {
                materials["404"] = 1;
            }
            if (level >= 9) {
                materials["405"] = 1;
            }
            
            return materials;
        }
        
        /// <summary>
        /// 检查是否有足够的强化材料
        /// </summary>
        public bool HasMaterials(int level, EnhancementType type) {
            var materials = GetRequiredMaterials(level, type);
            var inventory = _player?.GetInventory();
            
            if (inventory == null) return false;
            
            foreach (var mat in materials) {
                if (!inventory.ContainsKey(mat.Key) || inventory[mat.Key] < mat.Value) {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 执行装备强化
        /// </summary>
        public EnhancementResult EnhanceItem(string itemId, int level, EnhancementType type, string stoneId) {
            if (level >= MaxEnhancementLevel) {
                EmitSignal(SignalName.OnEnhancementComplete, itemId, level, EnhancementResult.MaxLevel);
                return EnhancementResult.MaxLevel;
            }
            
            // 检查材料
            if (!HasMaterials(level, type)) {
                EmitSignal(SignalName.OnEnhancementComplete, itemId, level, EnhancementResult.Failed);
                return EnhancementResult.Failed;
            }
            
            // 消耗材料
            var materials = GetRequiredMaterials(level, type);
            var inventory = _player.GetInventory();
            
            foreach (var mat in materials) {
                inventory[mat.Key] -= mat.Value;
                if (inventory[mat.Key] <= 0) {
                    inventory.Remove(mat.Key);
                }
            }
            
            EmitSignal(SignalName.OnEnhancementStarted, itemId, level);
            
            // 计算成功率
            float successRate = GetSuccessRate(level, stoneId);
            float roll = (float)GD.Randf();
            
            EnhancementResult result;
            int newLevel;
            
            if (roll < successRate) {
                // 成功
                newLevel = level + 1;
                result = EnhancementResult.Success;
            } else {
                // 失败
                newLevel = Math.Max(0, level - 1);
                result = EnhancementResult.Failed;
            }
            
            // 保存强化数据
            if (!_enhancedItems.ContainsKey(itemId)) {
                _enhancedItems[itemId] = new EnhancementData(itemId);
            }
            _enhancedItems[itemId].EnhancementLevel = newLevel;
            _enhancedItems[itemId].TryCount++;
            
            // 更新玩家背包
            _player.SetInventory(inventory);
            
            EmitSignal(SignalName.OnEnhancementComplete, itemId, newLevel, result);
            
            // Trigger tutorial for first enhancement
            if (!_hasTriggeredFirstEnhance && result == EnhancementResult.Success)
            {
                _hasTriggeredFirstEnhance = true;
                TutorialSystem.Trigger(TutorialTrigger.FirstEnhance);
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取物品的强化等级
        /// </summary>
        public int GetEnhancementLevel(string itemId) {
            if (_enhancedItems.ContainsKey(itemId)) {
                return _enhancedItems[itemId].EnhancementLevel;
            }
            return 0;
        }
        
        /// <summary>
        /// 获取强化属性加成
        /// </summary>
        public float GetEnhancementBonus(string itemId, string baseStat) {
            int level = GetEnhancementLevel(itemId);
            if (level == 0) return 0f;
            
            // 每级增加5%属性
            return level * 0.05f;
        }
        
        /// <summary>
        /// 序列化强化数据
        /// </summary>
        public Dictionary Serialize() {
            var data = new System.Collections.Generic.Dictionary<string, object>();
            var items = new Array<Dictionary>();
            
            foreach (var kvp in _enhancedItems) {
                var itemData = new Dictionary {
                    { "itemId", kvp.Key },
                    { "level", kvp.Value.EnhancementLevel },
                    { "tryCount", kvp.Value.TryCount }
                };
                items.Add(itemData);
            }
            
            data["enhancedItems"] = items;
            return data;
        }
        
        /// <summary>
        /// 反序列化强化数据
        /// </summary>
        public void Deserialize(Dictionary data) {
            _enhancedItems.Clear();
            
            if (!data.ContainsKey("enhancedItems")) return;
            
            var items = (Array<Dictionary>)data["enhancedItems"];
            foreach (var itemData in items) {
                string itemId = (string)itemData["itemId"];
                int level = (int)itemData["level"];
                int tryCount = (int)itemData["tryCount"];
                
                var enhancementData = new EnhancementData(itemId) {
                    EnhancementLevel = level,
                    TryCount = tryCount
                };
                _enhancedItems[itemId] = enhancementData;
            }
        }
        
        /// <summary>
        /// 获取玩家强化石数量
        /// </summary>
        public System.Collections.Generic.Dictionary<string, int> GetPlayerEnhancementStones() {
            var inventory = _player?.GetInventory();
            var stones = new System.Collections.Generic.Dictionary<string, int>();
            
            string[] stoneIds = {
                EnhanceStoneCommon,
                EnhanceStoneUncommon,
                EnhanceStoneRare,
                EnhanceStoneEpic,
                EnhanceStoneLegendary
            };
            
            foreach (var stoneId in stoneIds) {
                if (inventory != null && inventory.ContainsKey(stoneId)) {
                    stones[stoneId] = inventory[stoneId];
                } else {
                    stones[stoneId] = 0;
                }
            }
            
            return stones;
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
        {
            return Serialize();
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
        {
            if (data == null) return;
            Deserialize(data);
        }
    }
}
