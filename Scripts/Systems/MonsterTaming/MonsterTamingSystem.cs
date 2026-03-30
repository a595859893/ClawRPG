using Godot;
using System;
using System.Collections.Generic;

public class MonsterTamingSystem : BaseSystem
{
    private MonsterTamingData _data;
    private Random _random = new Random();
    
    public override void _Ready()
    {
        _data = new MonsterTamingData();
    }
    
    public MonsterTamingData GetData() => _data;
    
    public void LoadData(MonsterTamingData data)
    {
        _data = data;
    }
    
    // Generate a wild monster for capturing
    public Dictionary<string, Variant> GenerateWildMonster(int playerLevel)
    {
        string type = MonsterTamingDatabase.MonsterTypes[_random.Next(MonsterTamingDatabase.MonsterTypes.Length)];
        string rarity = GetRandomRarity();
        
        var template = MonsterTamingDatabase.Templates[type];
        float rarityHP = MonsterTamingDatabase.RarityHPBonus[rarity];
        float rarityATK = MonsterTamingDatabase.RarityATKBonus[rarity];
        float rarityDEF = MonsterTamingDatabase.RarityDEFBonus[rarity];
        
        int level = Math.Max(1, playerLevel + _random.Next(-3, 4));
        
        Dictionary<string, int> stats = new Dictionary<string, int>
        {
            ["HP"] = (int)(template.BaseHP * rarityHP * (1 + level * 0.1f)),
            ["ATK"] = (int)(template.BaseATK * rarityATK * (1 + level * 0.1f)),
            ["DEF"] = (int)(template.BaseDEF * rarityDEF * (1 + level * 0.1f)),
            ["Speed"] = (int)(template.Speed * 10)
        };
        
        return new Dictionary<string, Variant>
        {
            ["type"] = type,
            ["rarity"] = rarity,
            ["level"] = level,
            ["stats"] = stats,
            ["captureRate"] = template.CaptureRate
        };
    }
    
    private string GetRandomRarity()
    {
        float roll = (float)_random.NextDouble();
        if (roll < 0.01f) return "Legendary";      // 1%
        if (roll < 0.05f) return "Epic";            // 4%
        if (roll < 0.15f) return "Rare";            // 10%
        if (roll < 0.40f) return "Uncommon";        // 25%
        return "Common";                             // 60%
    }
    
    // Attempt to capture a monster
    public bool AttemptCapture(Dictionary<string, Variant> monster, float currentHealth, float maxHealth)
    {
        _data.TotalCaptureAttempts++;
        
        string type = (string)monster["type"];
        string rarity = (string)monster["rarity"];
        float baseRate = (float)monster["captureRate"];
        
        // Calculate capture rate
        float healthMod = MonsterTamingDatabase.HealthBonus(currentHealth, maxHealth);
        float rarityMod = MonsterTamingDatabase.RarityPenalty(rarity);
        
        float finalRate = baseRate * healthMod * rarityMod;
        finalRate = Mathf.Clamp(finalRate, 0.01f, 0.95f);
        
        bool success = (float)_random.NextDouble() < finalRate;
        
        if (success)
        {
            _data.SuccessfulCaptures++;
            _data.TotalMonstersTamed++;
            
            // Update rarity stats
            switch (rarity)
            {
                case "Legendary": _data.LegendaryCaptures++; break;
                case "Epic": _data.EpicCaptures++; break;
                case "Rare": _data.RareCaptures++; break;
                case "Uncommon": _data.UncommonCaptures++; break;
                case "Common": _data.CommonCaptures++; break;
            }
            
            // Add to tamed monsters
            TameMonster(monster);
        }
        
        return success;
    }
    
    private void TameMonster(Dictionary<string, Variant> monster)
    {
        var tamed = new MonsterTamingData.TamedMonster
        {
            Id = _data.TamedMonsters.Count + 1,
            Name = GenerateMonsterName((string)monster["type"], (string)monster["rarity"]),
            Type = (string)monster["type"],
            Rarity = (string)monster["rarity"],
            Level = (int)monster["level"],
            Experience = 0,
            BondLevel = 1,
            BattlesWon = 0,
            TamedAt = DateTime.Now,
            Stats = new Dictionary<string, int>((Dictionary<string, int>)monster["stats"])
        };
        
        _data.TamedMonsters[tamed.Id] = tamed;
    }
    
    private string GenerateMonsterName(string type, string rarity)
    {
        string[] prefixes = { "", "Alpha", "Beta", "Omega", "Prime", "Ultra", "Mega", "Giga" };
        string prefix = prefixes[_random.Next(prefixes.Length)];
        
        if (rarity == "Legendary" || rarity == "Epic")
            prefix = "Alpha " + type;
        else if (rarity == "Rare")
            prefix = "Beta " + type;
        else
            prefix = type;
        
        return prefix;
    }
    
    // Get tamed monster by ID
    public MonsterTamingData.TamedMonster GetTamedMonster(int id)
    {
        if (_data.TamedMonsters.ContainsKey(id))
            return _data.TamedMonsters[id];
        return null;
    }
    
    // Release a tamed monster
    public bool ReleaseMonster(int id)
    {
        if (_data.TamedMonsters.ContainsKey(id))
        {
            _data.TamedMonsters.Remove(id);
            return true;
        }
        return false;
    }
    
    // Train monster (gain experience)
    public void TrainMonster(int id, int experienceGained)
    {
        if (_data.TamedMonsters.ContainsKey(id))
        {
            var monster = _data.TamedMonsters[id];
            monster.Experience += experienceGained;
            
            // Level up every 100 experience
            int newLevel = 1 + (monster.Experience / 100);
            if (newLevel > monster.Level)
            {
                monster.Level = newLevel;
                // Increase stats
                monster.Stats["HP"] += 5;
                monster.Stats["ATK"] += 2;
                monster.Stats["DEF"] += 1;
                monster.Stats["Speed"] += 1;
            }
        }
    }
    
    // Increase bond level
    public void IncreaseBond(int id, int amount = 1)
    {
        if (_data.TamedMonsters.ContainsKey(id))
        {
            var monster = _data.TamedMonsters[id];
            monster.BondLevel = Mathf.Min(monster.BondLevel + amount, 10);
        }
    }
    
    // Get statistics
    public Dictionary<string, Variant> GetStatistics()
    {
        return new Dictionary<string, Variant>
        {
            ["total_attempts"] = _data.TotalCaptureAttempts,
            ["successful_captures"] = _data.SuccessfulCaptures,
            ["capture_rate"] = _data.TotalCaptureAttempts > 0 ? 
                (float)_data.SuccessfulCaptures / _data.TotalCaptureAttempts : 0f,
            ["total_tamed"] = _data.TotalMonstersTamed,
            ["legendary"] = _data.LegendaryCaptures,
            ["epic"] = _data.EpicCaptures,
            ["rare"] = _data.RareCaptures,
            ["uncommon"] = _data.UncommonCaptures,
            ["common"] = _data.CommonCaptures,
            ["tamed_count"] = _data.TamedMonsters.Count
        };
    }
    
    // Save data
    public MonsterTamingData SaveData()
    {
        return _data;
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();

        // 委托给数据层
        if (_data != null)
        {
            var dataData = _data.ExportSaveData();
            foreach (var kvp in dataData)
            {
                data[kvp.Key] = kvp.Value;
            }
        }

        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        // 委托给数据层
        if (_data != null)
        {
            _data.ImportSaveData(data);
        }
    }
}
