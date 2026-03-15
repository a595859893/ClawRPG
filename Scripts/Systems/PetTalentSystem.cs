using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 宠物天赋系统 - 管理宠物天赋的生成、学习和重置
/// </summary>
public class PetTalentSystem : BaseSystem
{
    public static PetTalentSystem Instance { get; private set; }

    public PlayerPetTalentData PlayerData { get; private set; }
    
    // 每只宠物初始天赋点数
    public const int INITIAL_TALENT_POINTS = 3;
    // 每升一级获得的天赋点数
    public const int TALENT_POINTS_PER_LEVEL = 1;
    // 每只宠物最多天赋数
    public const int MAX_TALENTS_PER_PET = 5;

    public override void _Ready()
    {
        Instance = this;
        PlayerData = new PlayerPetTalentData();
        LoadData();
    }

    public void LoadData()
    {
        if (SaveSystem.Instance != null)
        {
            var savedData = SaveSystem.Instance.LoadPetTalentData();
            if (savedData != null)
            {
                PlayerData = savedData;
            }
        }
    }

    public void SaveData()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SavePetTalentData(PlayerData);
        }
    }

    /// <summary>
    /// 为宠物生成随机天赋
    /// </summary>
    public List<PetTalent> GenerateRandomTalentsForPet(string petId, int count = 3)
    {
        List<PetTalent> talents = new List<PetTalent>();
        
        if (!PlayerData.PetTalents.ContainsKey(petId))
        {
            PlayerData.PetTalents[petId] = new List<PetTalent>();
            PlayerData.TalentPoints[petId] = INITIAL_TALENT_POINTS;
        }

        var talentDataList = PetTalentDatabase.Instance.GenerateTalentSet(count);
        
        foreach (var talentData in talentDataList)
        {
            PetTalent talent = new PetTalent(talentData.Id, 1);
            talent.IsUnlocked = true;
            talents.Add(talent);
            PlayerData.PetTalents[petId].Add(talent);
        }

        SaveData();
        return talents;
    }

    /// <summary>
    /// 获取宠物的所有天赋
    /// </summary>
    public List<PetTalent> GetPetTalents(string petId)
    {
        if (PlayerData.PetTalents.ContainsKey(petId))
        {
            return PlayerData.PetTalents[petId];
        }
        return new List<PetTalent>();
    }

    /// <summary>
    /// 获取宠物的天赋点数
    /// </summary>
    public int GetTalentPoints(string petId)
    {
        if (PlayerData.TalentPoints.ContainsKey(petId))
        {
            return PlayerData.TalentPoints[petId];
        }
        return 0;
    }

    /// <summary>
    /// 消耗天赋点数重新随机化宠物天赋
    /// </summary>
    public bool RerollPetTalents(string petId, int cost = 1)
    {
        if (!PlayerData.TalentPoints.ContainsKey(petId) || PlayerData.TalentPoints[petId] < cost)
        {
            GD.Print($"[PetTalentSystem] Not enough talent points to reroll for pet {petId}");
            return false;
        }

        PlayerData.TalentPoints[petId] -= cost;
        PlayerData.PetTalents[petId].Clear();

        var talentDataList = PetTalentDatabase.Instance.GenerateTalentSet(MAX_TALENTS_PER_PET);
        
        foreach (var talentData in talentDataList)
        {
            PetTalent talent = new PetTalent(talentData.Id, 1);
            talent.IsUnlocked = true;
            PlayerData.PetTalents[petId].Add(talent);
        }

        SaveData();
        GD.Print($"[PetTalentSystem] Rerolled talents for pet {petId}");
        return true;
    }

    /// <summary>
    /// 添加天赋点到宠物
    /// </summary>
    public void AddTalentPoints(string petId, int points)
    {
        if (!PlayerData.TalentPoints.ContainsKey(petId))
        {
            PlayerData.TalentPoints[petId] = INITIAL_TALENT_POINTS;
        }
        PlayerData.TalentPoints[petId] += points;
        SaveData();
    }

    /// <summary>
    /// 获取宠物某项属性的天赋加成
    /// </summary>
    public float GetTalentBonus(string petId, string statName)
    {
        float totalBonus = 0f;
        
        if (!PlayerData.PetTalents.ContainsKey(petId))
            return 0f;

        foreach (var talent in PlayerData.PetTalents[petId])
        {
            var talentData = PetTalentDatabase.Instance.GetTalent(talent.TalentId);
            if (talentData == null) continue;

            if (talentData.AffectedStat == statName || talentData.AffectedStat == "all")
            {
                totalBonus += talentData.BonusValue * talent.Level;
            }
            else if (talentData.AffectedStat == "frenzy")
            {
                if (statName == "attack") totalBonus += talentData.BonusValue * talent.Level;
                if (statName == "crit_rate") totalBonus += talentData.BonusValue * talent.Level;
            }
            else if (talentData.AffectedStat == "guard")
            {
                if (statName == "defense") totalBonus += talentData.BonusValue * talent.Level;
                if (statName == "dodge") totalBonus += talentData.BonusValue * talent.Level;
            }
            else if (talentData.AffectedStat == "blessing")
            {
                if (statName == "exp" || statName == "gold" || statName == "drop")
                    totalBonus += talentData.BonusValue * talent.Level;
            }
            else if (talentData.AffectedStat == "swift")
            {
                if (statName == "speed") totalBonus += talentData.BonusValue * talent.Level;
                if (statName == "dodge") totalBonus += talentData.BonusValue * talent.Level;
            }
        }

        return totalBonus;
    }

    /// <summary>
    /// 获取宠物所有属性加成
    /// </summary>
    public Dictionary<string, float> GetAllTalentBonuses(string petId)
    {
        Dictionary<string, float> bonuses = new Dictionary<string, float>
        {
            { "attack", 0f },
            { "defense", 0f },
            { "health", 0f },
            { "speed", 0f },
            { "crit_rate", 0f },
            { "crit_damage", 0f },
            { "lifesteal", 0f },
            { "dodge", 0f },
            { "tenacity", 0f },
            { "exp", 0f },
            { "gold", 0f },
            { "drop", 0f }
        };

        if (!PlayerData.PetTalents.ContainsKey(petId))
            return bonuses;

        foreach (var talent in PlayerData.PetTalents[petId])
        {
            var talentData = PetTalentDatabase.Instance.GetTalent(talent.TalentId);
            if (talentData == null) continue;

            float bonus = talentData.BonusValue * talent.Level;
            
            if (talentData.AffectedStat == "all")
            {
                foreach (var key in bonuses.Keys)
                {
                    bonuses[key] += bonus;
                }
            }
            else if (talentData.AffectedStat == "frenzy")
            {
                bonuses["attack"] += bonus;
                bonuses["crit_rate"] += bonus;
            }
            else if (talentData.AffectedStat == "guard")
            {
                bonuses["defense"] += bonus;
                bonuses["dodge"] += bonus;
            }
            else if (talentData.AffectedStat == "blessing")
            {
                bonuses["exp"] += bonus;
                bonuses["gold"] += bonus;
                bonuses["drop"] += bonus;
            }
            else if (talentData.AffectedStat == "swift")
            {
                bonuses["speed"] += bonus;
                bonuses["dodge"] += bonus;
            }
            else if (bonuses.ContainsKey(talentData.AffectedStat))
            {
                bonuses[talentData.AffectedStat] += bonus;
            }
        }

        return bonuses;
    }

    /// <summary>
    /// 检查宠物是否有某天赋
    /// </summary>
    public bool HasTalent(string petId, string talentId)
    {
        if (!PlayerData.PetTalents.ContainsKey(petId))
            return false;

        foreach (var talent in PlayerData.PetTalents[petId])
        {
            if (talent.TalentId == talentId)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取宠物天赋数量
    /// </summary>
    public int GetTalentCount(string petId)
    {
        if (PlayerData.PetTalents.ContainsKey(petId))
            return PlayerData.PetTalents[petId].Count;
        return 0;
    }

    /// <summary>
    /// 获取所有宠物的天赋统计
    /// </summary>
    public Dictionary<string, int> GetTalentStatistics()
    {
        Dictionary<string, int> stats = new Dictionary<string, int>
        {
            { "total_pets", 0 },
            { "total_talents", 0 },
            { "common_talents", 0 },
            { "uncommon_talents", 0 },
            { "rare_talents", 0 },
            { "epic_talents", 0 },
            { "legendary_talents", 0 }
        };

        foreach (var petTalents in PlayerData.PetTalents)
        {
            stats["total_pets"]++;
            foreach (var talent in petTalents.Value)
            {
                stats["total_talents"]++;
                var talentData = PetTalentDatabase.Instance.GetTalent(talent.TalentId);
                if (talentData != null)
                {
                    switch (talentData.Rarity)
                    {
                        case PetTalentData.TalentRarity.Common:
                            stats["common_talents"]++;
                            break;
                        case PetTalentData.TalentRarity.Uncommon:
                            stats["uncommon_talents"]++;
                            break;
                        case PetTalentData.TalentRarity.Rare:
                            stats["rare_talents"]++;
                            break;
                        case PetTalentData.TalentRarity.Epic:
                            stats["epic_talents"]++;
                            break;
                        case PetTalentData.TalentRarity.Legendary:
                            stats["legendary_talents"]++;
                            break;
                    }
                }
            }
        }

        return stats;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            SaveData();
            Instance = null;
        }
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 保存宠物天赋数据
        var petTalentsData = new Godot.Collections.Array();
        foreach (var kvp in PlayerData.PetTalents)
        {
            var petData = new Dictionary();
            petData["pet_id"] = kvp.Key;
            
            var talentsArray = new Godot.Collections.Array();
            foreach (var talent in kvp.Value)
            {
                var talentData = new Dictionary();
                talentData["talent_id"] = talent.TalentId;
                talentData["level"] = talent.Level;
                talentData["is_unlocked"] = talent.IsUnlocked;
                talentsArray.Add(talentData);
            }
            petData["talents"] = talentsArray;
            
            petTalentsData.Add(petData);
        }
        data["pet_talents"] = petTalentsData;
        
        // 保存天赋点数
        var talentPointsData = new Dictionary();
        foreach (var kvp in PlayerData.TalentPoints)
        {
            talentPointsData[kvp.Key] = kvp.Value;
        }
        data["talent_points"] = talentPointsData;
        
        return data;
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        // 导入宠物天赋数据
        if (data.Contains("pet_talents"))
        {
            PlayerData.PetTalents.Clear();
            var petTalentsArray = (Godot.Collections.Array)data["pet_talents"];
            foreach (Dictionary petData in petTalentsArray)
            {
                string petId = (string)petData["pet_id"];
                var talentsArray = (Godot.Collections.Array)petData["talents"];
                var talents = new List<PetTalent>();
                
                foreach (Dictionary talentData in talentsArray)
                {
                    var talent = new PetTalent(
                        (string)talentData["talent_id"],
                        (int)talentData["level"]
                    );
                    talent.IsUnlocked = (bool)talentData["is_unlocked"];
                    talents.Add(talent);
                }
                
                PlayerData.PetTalents[petId] = talents;
            }
        }
        
        // 导入天赋点数
        if (data.Contains("talent_points"))
        {
            var talentPointsDict = (Godot.Collections.Dictionary)data["talent_points"];
            foreach (var kvp in talentPointsDict)
            {
                PlayerData.TalentPoints[(string)kvp.Key] = (int)kvp.Value;
            }
        }

        // ===== 持久化方法 =====

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // 宠物天赋数据
            var petTalentsData = new List<Dictionary>();
            foreach (var kvp in PlayerData.PetTalents)
            {
                var talentsList = new List<Dictionary>();
                foreach (var talent in kvp.Value)
                {
                    var talentDict = new Dictionary();
                    talentDict["talent_id"] = talent.Id;
                    talentDict["level"] = talent.Level;
                    talentDict["is_unlocked"] = talent.IsUnlocked;
                    talentsList.Add(talentDict);
                }
                
                var petDict = new Dictionary();
                petDict["pet_id"] = kvp.Key;
                petDict["talents"] = talentsList;
                petTalentsData.Add(petDict);
            }
            data["pet_talents"] = petTalentsData;
            
            // 天赋点数
            var talentPointsData = new Dictionary<string, int>();
            foreach (var kvp in PlayerData.TalentPoints)
            {
                talentPointsData[kvp.Key] = kvp.Value;
            }
            data["talent_points"] = talentPointsData;
            
            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 加载宠物天赋数据
            if (data.Contains("pet_talents"))
            {
                var petTalentsArray = (Array)data["pet_talents"];
                PlayerData.PetTalents.Clear();
                
                foreach (Dictionary petData in petTalentsArray)
                {
                    string petId = petData["pet_id"].ToString();
                    var talentsArray = (Array)petData["talents"];
                    var talents = new List<PetTalent>();
                    
                    foreach (Dictionary talentData in talentsArray)
                    {
                        var talent = new PetTalent(
                            talentData["talent_id"].ToString(),
                            (int)talentData["level"]
                        );
                        talent.IsUnlocked = (bool)talentData["is_unlocked"];
                        talents.Add(talent);
                    }
                    
                    PlayerData.PetTalents[petId] = talents;
                }
            }
            
            // 加载天赋点数
            if (data.Contains("talent_points"))
            {
                var talentPointsDict = (Dictionary<string, Variant>)data["talent_points"];
                PlayerData.TalentPoints.Clear();
                foreach (var kvp in talentPointsDict)
                {
                    PlayerData.TalentPoints[kvp.Key] = (int)kvp.Value;
                }
            }
        }
    }
}
