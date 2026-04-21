using Godot;
using System;
using System.Collections.Generic;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

namespace Game.Scripts.Systems.EquipmentReforging
{
    /// <summary>
    /// 装备洗练系统管理器
    /// </summary>
    public partial class EquipmentReforgingSystem : BaseSystem
    {
        public static EquipmentReforgingSystem Instance { get; private set; }

        // 玩家数据
        private PlayerReforgeData _playerData = new PlayerReforgeData
        {
            TotalReforges = 0,
            SuccessfulReforges = 0,
            FailedReforges = 0,
            ReforgeHistoryByType = new Dictionary<string, int>(),
            ReforgeAttributesHistory = new Dictionary<string, List<Dictionary<string, float>>>()
        };

        // 信号
        public delegate void ReforgeStarted(string equipmentId, ReforgeType type);

        public delegate void ReforgeCompleted(string equipmentId, bool success, Dictionary<string, float> newAttributes);

        public delegate void ReforgeFailed(string equipmentId, string reason);

        public override void _Ready()
        {
            Instance = this;
            LoadData();
        }

        /// <summary>
        /// 尝试洗练装备
        /// </summary>
        public bool TryReforgeEquipment(string equipmentId, ReforgeType type)
        {
            var player = GetTree().CurrentScene.GetNode<Player>("Player");
            if (player == null)
            {
                EmitSignal(nameof(ReforgeFailed), equipmentId, "Player not found");
                return false;
            }

            // 获取配方
            var recipe = EquipmentReforgingDatabase.GetRecipe(type, ReforgeRarity.Common);

            // 检查金币
            if (player.Gold < recipe.GoldCost)
            {
                EmitSignal(nameof(ReforgeFailed), equipmentId, "Not enough gold");
                return false;
            }

            // 检查材料
            foreach (var material in recipe.MaterialCosts)
            {
                int playerCount = GetMaterialCount(material.Key);
                if (playerCount < material.Value)
                {
                    EmitSignal(nameof(ReforgeFailed), equipmentId, $"Not enough {material.Key}");
                    return false;
                }
            }

            // 扣除资源
            player.Gold -= recipe.GoldCost;
            foreach (var material in recipe.MaterialCosts)
            {
                RemoveMaterial(material.Key, material.Value);
            }

            EmitSignal(nameof(ReforgeStarted), equipmentId, type);

            // 判定成功/失败
            bool success = GD.RandDouble() < recipe.SuccessRate;

            Dictionary<string, float> newAttributes;
            if (success)
            {
                newAttributes = GenerateNewAttributes(type);
                SaveReforgeHistory(equipmentId, newAttributes);
                _playerData.SuccessfulReforges++;
                ApplyReforgedAttributes(equipmentId, newAttributes);
            }
            else
            {
                newAttributes = new Dictionary<string, float>();
                _playerData.FailedReforges++;
            }

            _playerData.TotalReforges++;
            SaveData();

            EmitSignal(nameof(ReforgeCompleted), equipmentId, success, newAttributes);
            return success;
        }

        /// <summary>
        /// 高级洗练(改变稀有度)
        /// </summary>
        public bool TryAdvancedReforge(string equipmentId, ReforgeRarity targetRarity)
        {
            var player = GetTree().CurrentScene.GetNode<Player>("Player");
            if (player == null)
            {
                EmitSignal(nameof(ReforgeFailed), equipmentId, "Player not found");
                return false;
            }

            var recipe = EquipmentReforgingDatabase.GetRecipe(ReforgeType.Advanced, targetRarity);

            if (player.Gold < recipe.GoldCost)
            {
                EmitSignal(nameof(ReforgeFailed), equipmentId, "Not enough gold");
                return false;
            }

            foreach (var material in recipe.MaterialCosts)
            {
                int playerCount = GetMaterialCount(material.Key);
                if (playerCount < material.Value)
                {
                    EmitSignal(nameof(ReforgeFailed), equipmentId, $"Not enough {material.Key}");
                    return false;
                }
            }

            player.Gold -= recipe.GoldCost;
            foreach (var material in recipe.MaterialCosts)
            {
                RemoveMaterial(material.Key, material.Value);
            }

            EmitSignal(nameof(ReforgeStarted), equipmentId, ReforgeType.Advanced);

            bool success = GD.RandDouble() < recipe.SuccessRate;
            Dictionary<string, float> newAttributes;

            if (success)
            {
                newAttributes = GenerateNewAttributes(ReforgeType.Advanced);
                SaveReforgeHistory(equipmentId, newAttributes);
                _playerData.SuccessfulReforges++;
                ApplyReforgedAttributes(equipmentId, newAttributes);
            }
            else
            {
                newAttributes = new Dictionary<string, float>();
                _playerData.FailedReforges++;
            }

            _playerData.TotalReforges++;
            SaveData();

            EmitSignal(nameof(ReforgeCompleted), equipmentId, success, newAttributes);
            return success;
        }

        /// <summary>
        /// 生成新属性
        /// </summary>
        private Dictionary<string, float> GenerateNewAttributes(ReforgeType type)
        {
            var attributes = new Dictionary<string, float>();
            int attributeCount = type switch
            {
                ReforgeType.Basic => 2,
                ReforgeType.Advanced => 3,
                ReforgeType.Legendary => 5,
                _ => 2
            };

            // 基础属性数量
            int basicCount = (int)(attributeCount * 0.6);
            int rareCount = attributeCount - basicCount;

            // 生成基础属性
            HashSet<string> usedAttributes = new HashSet<string>();
            for (int i = 0; i < basicCount; i++)
            {
                var attr = EquipmentReforgingDatabase.GetRandomAttribute(ReforgeType.Basic);
                if (!usedAttributes.Contains(attr.Name))
                {
                    float value = (float)(GD.RandDouble() * (attr.MaxValue - attr.MinValue) + attr.MinValue);
                    attributes[attr.Name] = (float)Math.Round(value, 1);
                    usedAttributes.Add(attr.Name);
                }
            }

            // 生成稀有属性
            if (type >= ReforgeType.Advanced)
            {
                for (int i = 0; i < rareCount; i++)
                {
                    var attr = EquipmentReforgingDatabase.GetRandomAttribute(ReforgeType.Basic);
                    // 尝试获取稀有属性
                    var rareAttrs = EquipmentReforgingDatabase.GetAttributesForType(ReforgeType.Advanced);
                    rareAttrs.RemoveAll(a => usedAttributes.Contains(a.Name));
                    if (rareAttrs.Count > 0)
                    {
                        var randomIndex = GD.Randi() % rareAttrs.Count;
                        var rareAttr = rareAttrs[randomIndex];
                        float value = (float)(GD.RandDouble() * (rareAttr.MaxValue - rareAttr.MinValue) + rareAttr.MinValue);
                        attributes[rareAttr.Name] = (float)Math.Round(value, 1);
                        usedAttributes.Add(rareAttr.Name);
                    }
                }
            }

            return attributes;
        }

        /// <summary>
        /// 应用洗练后的属性
        /// </summary>
        private void ApplyReforgedAttributes(string equipmentId, Dictionary<string, float> attributes)
        {
            // 集成到装备系统 - 这里需要根据实际装备系统实现
            // 可以通过信号通知装备系统更新属性
            GD.Print($"Applied reforged attributes to {equipmentId}: {string.Join(", ", attributes)}");
        }

        /// <summary>
        /// 获取玩家材料数量
        /// </summary>
        private int GetMaterialCount(string materialId)
        {
            // 从背包系统获取材料数量
            var inventoryManager = GetTree().CurrentScene.GetNode<InventoryManager>("CanvasLayer/UI/InventoryManager");
            if (inventoryManager != null)
            {
                var items = inventoryManager.GetInventoryItems();
                foreach (var item in items)
                {
                    if (item.Id == materialId)
                    {
                        return item.Quantity;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 移除材料
        /// </summary>
        private void RemoveMaterial(string materialId, int count)
        {
            var inventoryManager = GetTree().CurrentScene.GetNode<InventoryManager>("CanvasLayer/UI/InventoryManager");
            if (inventoryManager != null)
            {
                inventoryManager.RemoveItemById(materialId, count);
            }
        }

        /// <summary>
        /// 保存洗练历史
        /// </summary>
        private void SaveReforgeHistory(string equipmentId, Dictionary<string, float> attributes)
        {
            if (!_playerData.ReforgeHistoryByType.ContainsKey(equipmentId))
            {
                _playerData.ReforgeHistoryByType[equipmentId] = 0;
            }
            _playerData.ReforgeHistoryByType[equipmentId]++;

            if (!_playerData.ReforgeAttributesHistory.ContainsKey(equipmentId))
            {
                _playerData.ReforgeAttributesHistory[equipmentId] = new List<Dictionary<string, float>>();
            }
            _playerData.ReforgeAttributesHistory[equipmentId].Add(new Dictionary<string, float>(attributes));
        }

        /// <summary>
        /// 获取洗练统计
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            float successRate = _playerData.TotalReforges > 0
                ? (float)_playerData.SuccessfulReforges / _playerData.TotalReforges * 100
                : 0;

            return new Dictionary<string, object>
            {
                { "total_reforges", _playerData.TotalReforges },
                { "successful_reforges", _playerData.SuccessfulReforges },
                { "failed_reforges", _playerData.FailedReforges },
                { "success_rate", successRate }
            };
        }

        /// <summary>
        /// 获取装备洗练历史
        /// </summary>
        public List<Dictionary<string, float>> GetEquipmentReforgeHistory(string equipmentId)
        {
            if (_playerData.ReforgeAttributesHistory.ContainsKey(equipmentId))
            {
                return _playerData.ReforgeAttributesHistory[equipmentId];
            }
            return new List<Dictionary<string, float>>();
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public void SaveData()
        {
            var saveSystem = GetTree().CurrentScene.GetNode<SaveSystem>("SaveSystem");
            if (saveSystem != null)
            {
                SaveSystem.SaveGame();
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public void LoadData()
        {
            // 从存档加载数据
            GD.Print("EquipmentReforgingSystem: Data loaded");
        }

        /// <summary>
        /// 获取配方预览
        /// </summary>
        public ReforgeRecipe GetRecipePreview(ReforgeType type, ReforgeRarity rarity)
        {
            return EquipmentReforgingDatabase.GetRecipe(type, rarity);
        }

        /// <summary>
        /// 检查是否可以洗练
        /// </summary>
        public bool CanReforge(ReforgeType type, ReforgeRarity rarity)
        {
            var recipe = GetRecipePreview(type, rarity);
            var player = GetTree().CurrentScene.GetNode<Player>("Player");
            if (player == null || player.Gold < recipe.GoldCost)
                return false;

            foreach (var material in recipe.MaterialCosts)
            {
                if (GetMaterialCount(material.Key) < material.Value)
                    return false;
            }

            return true;
        }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 玩家洗练数据
        data["total_reforges"] = _playerData.TotalReforges;
        data["successful_reforges"] = _playerData.SuccessfulReforges;
        data["failed_reforges"] = _playerData.FailedReforges;
        
        // 按类型的洗练历史
        var reforgeTypeHistoryData = new Dictionary<string, object>();
        foreach (var kvp in _playerData.ReforgeHistoryByType)
        {
            reforgeTypeHistoryData[kvp.Key] = kvp.Value;
        }
        data["reforge_type_history"] = reforgeTypeHistoryData;
        
        return data;
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("total_reforges")) _playerData.TotalReforges = (int)data["total_reforges"];
        if (data.Contains("successful_reforges")) _playerData.SuccessfulReforges = (int)data["successful_reforges"];
        if (data.Contains("failed_reforges")) _playerData.FailedReforges = (int)data["failed_reforges"];
        
        _playerData.ReforgeHistoryByType.Clear();
        if (data.Contains("reforge_type_history"))
        {
            var reforgeTypeHistoryData = (Dictionary)data["reforge_type_history"];
            foreach (var kvp in reforgeTypeHistoryData)
            {
                _playerData.ReforgeHistoryByType[kvp.Key] = (int)kvp.Value;
            }
        }
    }
}
}
