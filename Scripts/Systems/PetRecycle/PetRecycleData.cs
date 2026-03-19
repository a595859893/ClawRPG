using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetRecycle {
    /// <summary>
    /// 宠物回收数据结构
    /// </summary>
    public class PetRecycleData : BaseSystem
    {
        // 已解锁的宠物类型
        public HashSet<string> UnlockedPetTypes { get; set; } = new HashSet<string>();
        
        // 回收历史记录
        public List<PetRecycleRecord> RecycleHistory { get; set; } = new List<PetRecycleRecord>();
        
        // 统计追踪
        public int TotalRecycled { get; set; } = 0;
        public int CommonRecycled { get; set; } = 0;
        public int UncommonRecycled { get; set; } = 0;
        public int RareRecycled { get; set; } = 0;
        public int EpicRecycled { get; set; } = 0;
        public int LegendaryRecycled { get; set; } = 0;
        public int TotalMaterials { get; set; } = 0;
        public int TotalExperience { get; set; } = 0;
        
        public override void _Ready()
        {
            base._Ready();
            GD.Print("[PetRecycleData] Initialized");
        }

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            // 保存已解锁的宠物类型
            data["unlocked_pet_types"] = new List<string>(UnlockedPetTypes);

            // 保存回收历史
            var historyList = new List<Dictionary<string, Variant>>();
            foreach (var record in RecycleHistory)
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
            data["total_recycled"] = TotalRecycled;
            data["common_recycled"] = CommonRecycled;
            data["uncommon_recycled"] = UncommonRecycled;
            data["rare_recycled"] = RareRecycled;
            data["epic_recycled"] = EpicRecycled;
            data["legendary_recycled"] = LegendaryRecycled;
            data["total_materials"] = TotalMaterials;
            data["total_experience"] = TotalExperience;

            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            // 加载已解锁的宠物类型
            if (data.TryGetValue("unlocked_pet_types", out var typesData))
                UnlockedPetTypes = new HashSet<string>((List<string>)typesData);

            // 加载回收历史
            if (data.TryGetValue("recycle_history", out var historyData))
            {
                RecycleHistory = new List<PetRecycleRecord>();
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

                    RecycleHistory.Add(record);
                }
            }

            // 加载统计数据
            if (data.TryGetValue("total_recycled", out var total))
                TotalRecycled = (int)total;
            if (data.TryGetValue("common_recycled", out var common))
                CommonRecycled = (int)common;
            if (data.TryGetValue("uncommon_recycled", out var uncommon))
                UncommonRecycled = (int)uncommon;
            if (data.TryGetValue("rare_recycled", out var rare))
                RareRecycled = (int)rare;
            if (data.TryGetValue("epic_recycled", out var epic))
                EpicRecycled = (int)epic;
            if (data.TryGetValue("legendary_recycled", out var legendary))
                LegendaryRecycled = (int)legendary;
            if (data.TryGetValue("total_materials", out var totalMat))
                TotalMaterials = (int)totalMat;
            if (data.TryGetValue("total_experience", out var totalExp))
                TotalExperience = (int)totalExp;
        }
    }
    
    /// <summary>
    /// 宠物回收记录
    /// </summary>
    public class PetRecycleRecord
    {
        public string PetType { get; set; } = "";
        public string PetName { get; set; } = "";
        public string Rarity { get; set; } = "";
        public int Level { get; set; } = 1;
        public List<MaterialReward> Materials { get; set; } = new List<MaterialReward>();
        public int ExperienceGained { get; set; } = 0;
        public int Timestamp { get; set; } = 0;
    }
    
    /// <summary>
    /// 材料奖励
    /// </summary>
    public class MaterialReward
    {
        public string MaterialId { get; set; } = "";
        public string MaterialName { get; set; } = "";
        public int Quantity { get; set; } = 0;
        public int Value { get; set; } = 0;
    }
}
