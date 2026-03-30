using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ClawRPG.Scripts.Systems.EquipmentRecycle
{
    /// <summary>
    /// 装备回收系统 - 金币回收机制
    /// 玩家可以分解不需要的装备获取金币和材料
    /// </summary>
    public partial class EquipmentRecycleSystem : BaseSystem
    {
        public static EquipmentRecycleSystem Instance { get; private set; }

        // 回收配置 - 按稀有度
        private Dictionary<string, RecycleConfig> recycleConfig = new Dictionary<string, RecycleConfig>
        {
            { "Common", new RecycleConfig { Gold = 10, Materials = new List<string> { "scrap_metal" }, MaterialCount = 1 } },
            { "Uncommon", new RecycleConfig { Gold = 25, Materials = new List<string> { "scrap_metal", "iron_ingot" }, MaterialCount = 2 } },
            { "Rare", new RecycleConfig { Gold = 50, Materials = new List<string> { "iron_ingot", "silver_ingot" }, MaterialCount = 3 } },
            { "Epic", new RecycleConfig { Gold = 100, Materials = new List<string> { "silver_ingot", "gold_ingot", "gem_fragment" }, MaterialCount = 4 } },
            { "Legendary", new RecycleConfig { Gold = 200, Materials = new List<string> { "gold_ingot", "gem_fragment", "crystal" }, MaterialCount = 5 } },
            { "Mythical", new RecycleConfig { Gold = 500, Materials = new List<string> { "gold_ingot", "crystal", "rare_ore" }, MaterialCount = 6 } }
        };

        // 装备类型回收加成
        private Dictionary<string, float> typeBonus = new Dictionary<string, float>
        {
            { "Weapon", 1.2f },
            { "Armor", 1.1f },
            { "Accessory", 1.0f },
            { "Mount", 1.5f },
            { "Pet", 1.5f }
        };

        // 强化等级回收加成
        private float enhancementBonusPerLevel = 0.1f;

        // 回收统计
        private RecycleStats stats = new RecycleStats();

        // 信号
public delegate void RecycleCompletedEventHandler(string itemName, int goldReward, List<string> materials);
public delegate void RecycleFailedEventHandler(string reason);

        public override void _Ready()
        {
            Instance = this;
            LoadStats();
        }

        /// <summary>
        /// 回收装备
        /// </summary>
        public RecycleResult RecycleEquipment(Dictionary<string, object> item)
        {
            var result = new RecycleResult { Success = false, Gold = 0, Materials = new List<string>(), Message = "" };

            if (item == null || item.Count == 0)
            {
                result.Message = "无效的装备";
                EmitSignal(SignalName.RecycleFailed, result.Message);
                return result;
            }

            string rarity = item.ContainsKey("rarity") ? item["rarity"].ToString() : "Common";
            string itemType = item.ContainsKey("type") ? item["type"].ToString() : "Default";
            int enhancementLevel = item.ContainsKey("enhancement_level") ? Convert.ToInt32(item["enhancement_level"]) : 0;

            if (!recycleConfig.ContainsKey(rarity))
            {
                rarity = "Common";
            }

            var config = recycleConfig[rarity];
            float bonus = typeBonus.ContainsKey(itemType) ? typeBonus[itemType] : 1.0f;
            int goldReward = (int)(config.Gold * bonus);

            // 强化加成
            if (enhancementLevel > 0)
            {
                goldReward = (int)(goldReward * (1.0f + enhancementLevel * enhancementBonusPerLevel));
            }

            // 生成回收材料
            var materials = new List<string>();
            for (int i = 0; i < config.MaterialCount; i++)
            {
                string material = config.Materials[GD.RandI() % config.Materials.Count];
                materials.Add(material);

                // 统计材料
                if (!stats.MaterialsObtained.ContainsKey(material))
                {
                    stats.MaterialsObtained[material] = 0;
                }
                stats.MaterialsObtained[material]++;
            }

            // 添加金币到玩家
            PlayerData.AddGold(goldReward);

            // 添加材料到背包
            foreach (string material in materials)
            {
                InventoryManager.AddItem(material, 1);
            }

            // 更新统计
            stats.TotalRecycled++;
            stats.TotalGoldEarned += goldReward;
            stats.FavoriteRarity = rarity;

            result.Success = true;
            result.Gold = goldReward;
            result.Materials = materials;
            result.Message = $"回收成功！获得 {goldReward} 金币和 {materials.Count} 种材料";

            EmitSignal(SignalName.RecycleCompleted, item.ContainsKey("name") ? item["name"].ToString() : "Unknown", goldReward, materials);
            SaveStats();

            return result;
        }

        /// <summary>
        /// 批量回收多个装备
        /// </summary>
        public BatchRecycleResult BatchRecycle(List<Dictionary<string, object>> items)
        {
            var result = new BatchRecycleResult { Success = false, TotalGold = 0, TotalMaterials = new List<string>(), Count = 0 };

            foreach (var item in items)
            {
                var recycleResult = RecycleEquipment(item);
                if (recycleResult.Success)
                {
                    result.TotalGold += recycleResult.Gold;
                    result.TotalMaterials.AddRange(recycleResult.Materials);
                    result.Count++;
                }
            }

            result.Success = result.Count > 0;
            return result;
        }

        /// <summary>
        /// 获取回收预览
        /// </summary>
        public RecyclePreview GetRecyclePreview(Dictionary<string, object> item)
        {
            var preview = new RecyclePreview { Gold = 0, Materials = new List<string>(), Message = "" };

            if (item == null || item.Count == 0)
            {
                preview.Message = "无效的装备";
                return preview;
            }

            string rarity = item.ContainsKey("rarity") ? item["rarity"].ToString() : "Common";
            string itemType = item.ContainsKey("type") ? item["type"].ToString() : "Default";
            int enhancementLevel = item.ContainsKey("enhancement_level") ? Convert.ToInt32(item["enhancement_level"]) : 0;

            if (!recycleConfig.ContainsKey(rarity))
            {
                rarity = "Common";
            }

            var config = recycleConfig[rarity];
            float bonus = typeBonus.ContainsKey(itemType) ? typeBonus[itemType] : 1.0f;
            int goldReward = (int)(config.Gold * bonus);

            if (enhancementLevel > 0)
            {
                goldReward = (int)(goldReward * (1.0f + enhancementLevel * enhancementBonusPerLevel));
            }

            preview.Gold = goldReward;
            preview.Materials = new List<string>(config.Materials);
            preview.Message = $"可回收: {goldReward} 金币 + {config.MaterialCount} 种材料";

            return preview;
        }

        /// <summary>
        /// 获取回收统计
        /// </summary>
        public RecycleStats GetStats()
        {
            return stats;
        }

        /// <summary>
        /// 保存统计
        /// </summary>
        public void SaveStats()
        {
            string json = JsonSerializer.Serialize(stats);
            PlayerData.SetMeta("equipment_recycle_stats", json);
        }

        /// <summary>
        /// 加载统计
        /// </summary>
        public void LoadStats()
        {
            if (PlayerData.HasMeta("equipment_recycle_stats"))
            {
                try
                {
                    string json = PlayerData.GetMeta("equipment_recycle_stats");
                    stats = JsonSerializer.Deserialize<RecycleStats>(json);
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Failed to load recycle stats: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 重置统计
        /// </summary>
        public void ResetStats()
        {
            stats = new RecycleStats();
            SaveStats();
        }
    }

    // 配置类
    public class RecycleConfig
    {
        public int Gold { get; set; }
        public List<string> Materials { get; set; }
        public int MaterialCount { get; set; }
    }

    // 结果类
    public class RecycleResult
    {
        public bool Success { get; set; }
        public int Gold { get; set; }
        public List<string> Materials { get; set; }
        public string Message { get; set; }
    }

    // 批量结果类
    public class BatchRecycleResult
    {
        public bool Success { get; set; }
        public int TotalGold { get; set; }
        public List<string> TotalMaterials { get; set; }
        public int Count { get; set; }
    }

    // 预览类
    public class RecyclePreview
    {
        public int Gold { get; set; }
        public List<string> Materials { get; set; }
        public string Message { get; set; }
    }

    // 统计类
    public class RecycleStats
    {
        public int TotalRecycled { get; set; }
        public int TotalGoldEarned { get; set; }
        public Dictionary<string, int> MaterialsObtained { get; set; } = new Dictionary<string, int>();
        public string FavoriteRarity { get; set; } = "";
    }
    
}
