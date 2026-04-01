using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Systems.PetRecycle;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 宠物回收系统 - 核心系统
    /// </summary>
    public partial class PetRecycleSystem : BaseSystem
    {
        private PetRecycleData _data;
        private PetRecycleDatabase _database;
        
        // 信号
        public Action<PetRecycleRecord> RecycleCompleted;
        public Action<MaterialReward> MaterialAdded;
        public Action StatisticsUpdated;
        
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

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            if (_data == null) return data;

            // 保存已解锁的宠物类型
            data["unlocked_pet_types"] = new List<string>(_data.UnlockedPetTypes);

            // 保存回收历史
            var historyList = new List<Dictionary<string, Variant>>();
            foreach (var record in _data.RecycleHistory)
            {
                var recordDict = new Dictionary<string, Variant>
                {
                    ["pet_type"] = record.PetType ?? "",
                    ["pet_name"] = record.PetName ?? "",
                    ["rarity"] = record.Rarity ?? "",
                    ["level"] = record.Level,
                    ["experience_gained"] = record.ExperienceGained,
                    ["timestamp"] = record.Timestamp
                };

                // 保存材料奖励
                var materialsList = new List<Dictionary<string, Variant>>();
                foreach (var mat in record.Materials)
                {
                    materialsList.Add(new Dictionary<string, Variant>
                    {
                        ["material_id"] = mat.MaterialId ?? "",
                        ["material_name"] = mat.MaterialName ?? "",
                        ["quantity"] = mat.Quantity,
                        ["value"] = mat.Value
                    });
                }
                recordDict["materials"] = materialsList;
                historyList.Add(recordDict);
            }
            data["recycle_history"] = historyList;

            // 保存统计数据
            data["total_recycled"] = _data.TotalRecycled;
            data["common_recycled"] = _data.CommonRecycled;
            data["uncommon_recycled"] = _data.UncommonRecycled;
            data["rare_recycled"] = _data.RareRecycled;
            data["epic_recycled"] = _data.EpicRecycled;
            data["legendary_recycled"] = _data.LegendaryRecycled;
            data["total_materials"] = _data.TotalMaterials;
            data["total_experience"] = _data.TotalExperience;

            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null || _data == null) return;

            // 加载已解锁的宠物类型
            if (data.TryGetValue("unlocked_pet_types", out var typesData))
                _data.UnlockedPetTypes = new HashSet<string>((List<string>)typesData);

            // 加载回收历史
            if (data.TryGetValue("recycle_history", out var historyData))
            {
                _data.RecycleHistory = new List<PetRecycleRecord>();
                var historyList = (List<Variant>)historyData;
                foreach (var recordVar in historyList)
                {
                    var recordDict = (Dictionary<string, Variant>)recordVar;
                    var record = new PetRecycleRecord();

                    if (recordDict.TryGetValue("pet_type", out var petType))
                        record.PetType = (string)petType;
                    if (recordDict.TryGetValue("pet_name", out var petName))
                        record.PetName = (string)petName;
                    if (recordDict.TryGetValue("rarity", out var rarity))
                        record.Rarity = (string)rarity;
                    if (recordDict.TryGetValue("level", out var level))
                        record.Level = (int)level;
                    if (recordDict.TryGetValue("experience_gained", out var expGained))
                        record.ExperienceGained = (int)expGained;
                    if (recordDict.TryGetValue("timestamp", out var timestamp))
                        record.Timestamp = (int)timestamp;

                    // 加载材料奖励
                    if (recordDict.TryGetValue("materials", out var materialsData))
                    {
                        record.Materials = new List<MaterialReward>();
                        var materialsList = (List<Variant>)materialsData;
                        foreach (var matVar in materialsList)
                        {
                            var matDict = (Dictionary<string, Variant>)matVar;
                            var mat = new MaterialReward();

                            if (matDict.TryGetValue("material_id", out var matId))
                                mat.MaterialId = (string)matId;
                            if (matDict.TryGetValue("material_name", out var matName))
                                mat.MaterialName = (string)matName;
                            if (matDict.TryGetValue("quantity", out var qty))
                                mat.Quantity = (int)qty;
                            if (matDict.TryGetValue("value", out var val))
                                mat.Value = (int)val;

                            record.Materials.Add(mat);
                        }
                    }

                    _data.RecycleHistory.Add(record);
                }
            }

            // 加载统计数据
            if (data.TryGetValue("total_recycled", out var total))
                _data.TotalRecycled = (int)total;
            if (data.TryGetValue("common_recycled", out var common))
                _data.CommonRecycled = (int)common;
            if (data.TryGetValue("uncommon_recycled", out var uncommon))
                _data.UncommonRecycled = (int)uncommon;
            if (data.TryGetValue("rare_recycled", out var rare))
                _data.RareRecycled = (int)rare;
            if (data.TryGetValue("epic_recycled", out var epic))
                _data.EpicRecycled = (int)epic;
            if (data.TryGetValue("legendary_recycled", out var legendary))
                _data.LegendaryRecycled = (int)legendary;
            if (data.TryGetValue("total_materials", out var totalMat))
                _data.TotalMaterials = (int)totalMat;
            if (data.TryGetValue("total_experience", out var totalExp))
                _data.TotalExperience = (int)totalExp;
        }
    }
}
