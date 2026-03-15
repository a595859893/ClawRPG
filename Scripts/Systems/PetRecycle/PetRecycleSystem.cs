using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Systems.PetRecycle;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 宠物回收系统 - 核心系统
    /// </summary>
    public class PetRecycleSystem : BaseSystem
    {
        private PetRecycleData _data;
        private PetRecycleDatabase _database;
        
        // 信号
        public signal void RecycleCompleted(PetRecycleRecord record);
        public signal void MaterialAdded(MaterialReward material);
        public signal void StatisticsUpdated();
        
        public override void _Ready()
        {
            base._Ready();
            
            // 初始化数据和数据库
            _data = new PetRecycleData();
            _database = new PetRecycleDatabase();
            
            // 添加到场景树
            AddChild(_data);
            AddChild(_database);
            
            GD.Print("[PetRecycleSystem] Initialized");
        }
        
        /// <summary>
        /// 回收宠物
        /// </summary>
        public PetRecycleRecord RecyclePet(string petType, string petName, string rarity, int level)
        {
            var record = new PetRecycleRecord
            {
                PetType = petType,
                PetName = petName,
                Rarity = rarity,
                Level = level,
                Timestamp = OS.GetUnixTime()
            };
            
            // 获取配置
            var petConfig = _database.GetPetTypeConfig(petType);
            var rarityConfig = _database.GetRarityConfig(rarity);
            var levelBonus = _database.GetLevelBonus(level);
            
            // 生成基础材料
            int baseMaterialCount = 2 + (int)(level / 10); // 2-12个基础材料
            for (int i = 0; i < baseMaterialCount; i++)
            {
                var materialId = petConfig.BaseMaterials[GD.RandI() % petConfig.BaseMaterials.Count];
                var materialConfig = _database.GetMaterialConfig(materialId);
                if (materialConfig != null)
                {
                    int quantity = (int)(GD.RandI() % 3 + 1) * rarityConfig.MaterialMultiplier * levelBonus;
                    var reward = new MaterialReward
                    {
                        MaterialId = materialId,
                        MaterialName = materialConfig.DisplayName,
                        Quantity = Math.Max(1, quantity),
                        Value = materialConfig.BaseValue * Math.Max(1, quantity)
                    };
                    record.Materials.Add(reward);
                    _data.TotalMaterials += reward.Quantity;
                    MaterialAdded?.Invoke(reward);
                }
            }
            
            // 尝试生成特殊材料
            if (GD.RandF() < rarityConfig.SpecialDropChance)
            {
                var bonusMaterialId = petConfig.BonusMaterials[GD.RandI() % petConfig.BonusMaterials.Count];
                var bonusMaterialConfig = _database.GetMaterialConfig(bonusMaterialId);
                if (bonusMaterialConfig != null)
                {
                    int quantity = 1;
                    var reward = new MaterialReward
                    {
                        MaterialId = bonusMaterialId,
                        MaterialName = bonusMaterialConfig.DisplayName,
                        Quantity = quantity,
                        Value = bonusMaterialConfig.BaseValue * quantity
                    };
                    record.Materials.Add(reward);
                    _data.TotalMaterials += quantity;
                    MaterialAdded?.Invoke(reward);
                }
            }
            
            // 计算经验奖励
            int baseExp = level * 10;
            record.ExperienceGained = (int)(baseExp * rarityConfig.ExperienceMultiplier);
            _data.TotalExperience += record.ExperienceGained;
            
            // 更新统计
            _data.TotalRecycled++;
            switch (rarity)
            {
                case "Common": _data.CommonRecycled++; break;
                case "Uncommon": _data.UncommonRecycled++; break;
                case "Rare": _data.RareRecycled++; break;
                case "Epic": _data.EpicRecycled++; break;
                case "Legendary": _data.LegendaryRecycled++; break;
            }
            
            // 添加到历史记录
            _data.RecycleHistory.Insert(0, record);
            if (_data.RecycleHistory.Count > 100)
            {
                _data.RecycleHistory.RemoveAt(_data.RecycleHistory.Count - 1);
            }
            
            // 解锁宠物类型
            _data.UnlockedPetTypes.Add(petType);
            
            // 触发信号
            RecycleCompleted?.Invoke(record);
            StatisticsUpdated?.Invoke();
            
            GD.Print($"[PetRecycleSystem] Recycled {petName} ({rarity}, Lv.{level}) -> {record.Materials.Count} materials, {record.ExperienceGained} XP");
            
            return record;
        }
        
        /// <summary>
        /// 获取预览材料
        /// </summary>
        public List<MaterialReward> PreviewRecycle(string petType, string rarity, int level)
        {
            var preview = new List<MaterialReward>();
            
            var petConfig = _database.GetPetTypeConfig(petType);
            var rarityConfig = _database.GetRarityConfig(rarity);
            var levelBonus = _database.GetLevelBonus(level);
            
            // 基础材料预览
            int baseMaterialCount = 2 + (int)(level / 10);
            for (int i = 0; i < baseMaterialCount; i++)
            {
                var materialId = petConfig.BaseMaterials[GD.RandI() % petConfig.BaseMaterials.Count];
                var materialConfig = _database.GetMaterialConfig(materialId);
                if (materialConfig != null)
                {
                    int quantity = (int)(GD.RandI() % 3 + 1) * rarityConfig.MaterialMultiplier * levelBonus;
                    preview.Add(new MaterialReward
                    {
                        MaterialId = materialId,
                        MaterialName = materialConfig.DisplayName,
                        Quantity = Math.Max(1, quantity),
                        Value = materialConfig.BaseValue * Math.Max(1, quantity)
                    });
                }
            }
            
            // 特殊材料预览 (50%概率显示)
            if (GD.RandF() < rarityConfig.SpecialDropChance * 0.5f)
            {
                var bonusMaterialId = petConfig.BonusMaterials[GD.RandI() % petConfig.BonusMaterials.Count];
                var bonusMaterialConfig = _database.GetMaterialConfig(bonusMaterialId);
                if (bonusMaterialConfig != null)
                {
                    preview.Add(new MaterialReward
                    {
                        MaterialId = bonusMaterialId,
                        MaterialName = bonusMaterialConfig.DisplayName,
                        Quantity = 1,
                        Value = bonusMaterialConfig.BaseValue
                    });
                }
            }
            
            return preview;
        }
        
        /// <summary>
        /// 获取统计数据
        /// </summary>
        public Dictionary<string, int> GetStatistics()
        {
            return new Dictionary<string, int>
            {
                { "TotalRecycled", _data.TotalRecycled },
                { "CommonRecycled", _data.CommonRecycled },
                { "UncommonRecycled", _data.UncommonRecycled },
                { "RareRecycled", _data.RareRecycled },
                { "EpicRecycled", _data.EpicRecycled },
                { "LegendaryRecycled", _data.LegendaryRecycled },
                { "TotalMaterials", _data.TotalMaterials },
                { "TotalExperience", _data.TotalExperience }
            };
        }
        
        /// <summary>
        /// 获取回收历史
        /// </summary>
        public List<PetRecycleRecord> GetRecycleHistory(int count = 10)
        {
            return _data.RecycleHistory.Take(count).ToList();
        }
        
        /// <summary>
        /// 获取已解锁的宠物类型
        /// </summary>
        public HashSet<string> GetUnlockedPetTypes()
        {
            return _data.UnlockedPetTypes;
        }
        
        /// <summary>
        /// 获取稀有度列表
        /// </summary>
        public List<string> GetRarityList()
        {
            return new List<string> { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        }
        
        /// <summary>
        /// 获取宠物类型列表
        /// </summary>
        public List<string> GetPetTypeList()
        {
            return new List<string> { "Dog", "Cat", "Bird", "Rabbit", "Dragon", "Slime", "Skeleton", "Elemental" };
        }
        
        /// <summary>
        /// 模拟回收（测试用）
        /// </summary>
        public void SimulateRecycle()
        {
            var petTypes = GetPetTypeList();
            var rarities = GetRarityList();
            
            var randomPetType = petTypes[GD.RandI() % petTypes.Count];
            var randomRarity = rarities[GD.RandI() % rarities.Count];
            var randomLevel = GD.RandI() % 50 + 1;
            var randomName = $"Test Pet {GD.RandI() % 1000}";
            
            RecyclePet(randomPetType, randomName, randomRarity, randomLevel);
        }
        
        /// <summary>
        /// 保存数据
        /// </summary>
        public Dictionary<string, object> SaveData()
        {
            return new Dictionary<string, object>
            {
                { "UnlockedPetTypes", _data.UnlockedPetTypes.ToList() },
                { "TotalRecycled", _data.TotalRecycled },
                { "CommonRecycled", _data.CommonRecycled },
                { "UncommonRecycled", _data.UncommonRecycled },
                { "RareRecycled", _data.RareRecycled },
                { "EpicRecycled", _data.EpicRecycled },
                { "LegendaryRecycled", _data.LegendaryRecycled },
                { "TotalMaterials", _data.TotalMaterials },
                { "TotalExperience", _data.TotalExperience }
            };
        }
        
        /// <summary>
        /// 加载数据
        /// </summary>
        public void LoadData(Dictionary<string, object> saveData)
        {
            if (saveData == null) return;
            
            if (saveData.ContainsKey("UnlockedPetTypes"))
            {
                var list = saveData["UnlockedPetTypes"] as Godot.Collections.Array;
                _data.UnlockedPetTypes = new HashSet<string>();
                foreach (var item in list)
                {
                    _data.UnlockedPetTypes.Add(item.ToString());
                }
            }
            
            _data.TotalRecycled = saveData.ContainsKey("TotalRecycled") ? (int)saveData["TotalRecycled"] : 0;
            _data.CommonRecycled = saveData.ContainsKey("CommonRecycled") ? (int)saveData["CommonRecycled"] : 0;
            _data.UncommonRecycled = saveData.ContainsKey("UncommonRecycled") ? (int)saveData["UncommonRecycled"] : 0;
            _data.RareRecycled = saveData.ContainsKey("RareRecycled") ? (int)saveData["RareRecycled"] : 0;
            _data.EpicRecycled = saveData.ContainsKey("EpicRecycled") ? (int)saveData["EpicRecycled"] : 0;
            _data.LegendaryRecycled = saveData.ContainsKey("LegendaryRecycled") ? (int)saveData["LegendaryRecycled"] : 0;
            _data.TotalMaterials = saveData.ContainsKey("TotalMaterials") ? (int)saveData["TotalMaterials"] : 0;
            _data.TotalExperience = saveData.ContainsKey("TotalExperience") ? (int)saveData["TotalExperience"] : 0;
            
            GD.Print("[PetRecycleSystem] Data loaded");
        }
    }
}
