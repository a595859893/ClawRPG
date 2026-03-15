using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 梦境数据库 - 配置梦境世界数据
/// </summary>
public class DreamscapeDatabase
{
    private static DreamscapeDatabase _instance;
    public static DreamscapeDatabase Instance => _instance ?? (_instance = new DreamscapeDatabase());
    
    public Dictionary<string, DreamscapeEntry> Dreamscapes { get; private set; }
    public Dictionary<DreamscapeType, List<DreamscapeLayer>> LayerTemplates { get; private set; }
    public Dictionary<DreamscapeType, Dictionary<int, DreamscapeReward>> LayerRewards { get; private set; }
    
    public DreamscapeDatabase()
    {
        Dreamscapes = new Dictionary<string, DreamscapeEntry>();
        LayerTemplates = new Dictionary<DreamscapeType, List<DreamscapeLayer>>();
        LayerRewards = new Dictionary<DreamscapeType, Dictionary<int, DreamscapeReward>>();
        InitializeDreamscapes();
        InitializeLayerTemplates();
        InitializeLayerRewards();
    }
    
    private void InitializeDreamscapes()
    {
        // 噩梦梦境
        Dreamscapes["nightmare"] = new DreamscapeEntry
        {
            Id = "nightmare",
            Name = "Nightmare Realm",
            Description = "Face your deepest fears in this endless nightmare",
            Type = DreamscapeType.Nightmare,
            State = DreamscapeState.Available,
            TotalLayers = 20,
            RequiredPlayerLevel = 1,
            EntryCost = 0,
            DefaultRule = DreamscapeRule.DoubleDamage,
            EnemyMultiplier = 1.5f,
            ScoreMultiplier = 1.5f,
            DropMultiplier = 1.3f
        };
        
        // 以太梦境
        Dreamscapes["ethereal"] = new DreamscapeEntry
        {
            Id = "ethereal",
            Name = "Ethereal Plane",
            Description = "Float through the ethereal realm of spirits",
            Type = DreamscapeType.Ethereal,
            State = DreamscapeState.Locked,
            TotalLayers = 15,
            RequiredPlayerLevel = 10,
            EntryCost = 100,
            DefaultRule = DreamscapeRule.FloatGravity,
            EnemyMultiplier = 1.2f,
            ScoreMultiplier = 1.3f,
            DropMultiplier = 1.2f
        };
        
        // 虚空梦境
        Dreamscapes["void"] = new DreamscapeEntry
        {
            Id = "void",
            Name = "Void Dimension",
            Description = "Survive in the endless void between dimensions",
            Type = DreamscapeType.Void,
            State = DreamscapeState.Locked,
            TotalLayers = 25,
            RequiredPlayerLevel = 20,
            EntryCost = 200,
            DefaultRule = DreamscapeRule.NoDeathPenalty,
            EnemyMultiplier = 2.0f,
            ScoreMultiplier = 2.0f,
            DropMultiplier = 1.5f
        };
        
        // 时间梦境
        Dreamscapes["temporal"] = new DreamscapeEntry
        {
            Id = "temporal",
            Name = "Temporal Rift",
            Description = "Navigate through distorted time streams",
            Type = DreamscapeType.Temporal,
            State = DreamscapeState.Locked,
            TotalLayers = 18,
            RequiredPlayerLevel = 15,
            EntryCost = 150,
            DefaultRule = DreamscapeRule.TimeSlowdown,
            EnemyMultiplier = 1.3f,
            ScoreMultiplier = 1.4f,
            DropMultiplier = 1.25f
        };
        
        // 清醒梦境
        Dreamscapes["lucid"] = new DreamscapeEntry
        {
            Id = "lucid",
            Name = "Lucid Dreams",
            Description = "Shape reality with your conscious mind",
            Type = DreamscapeType.Lucid,
            State = DreamscapeState.Locked,
            TotalLayers = 30,
            RequiredPlayerLevel = 25,
            EntryCost = 300,
            DefaultRule = DreamscapeRule.NoCooldown,
            EnemyMultiplier = 1.8f,
            ScoreMultiplier = 1.8f,
            DropMultiplier = 1.4f
        };
    }
    
    private void InitializeLayerTemplates()
    {
        // 噩梦梦境层配置
        LayerTemplates[DreamscapeType.Nightmare] = new List<DreamscapeLayer>();
        for (int i = 1; i <= 20; i++)
        {
            bool isBoss = (i % 5 == 0);
            var layer = new DreamscapeLayer
            {
                LayerNumber = i,
                EnemyType = isBoss ? "NightmareBoss" : "ShadowCreature",
                EnemyCount = 3 + i * 2,
                SpecialRule = (DreamscapeRule)((i % 5) + 1),
                TimeLimit = isBoss ? 180 : 120,
                BaseScore = 100 * i,
                BaseGold = 50 * i,
                BaseExperience = 30 * i,
                IsBossLayer = isBoss,
                BossType = isBoss ? $"NightmareLord{i / 5}" : ""
            };
            LayerTemplates[DreamscapeType.Nightmare].Add(layer);
        }
        
        // 以太梦境层配置
        LayerTemplates[DreamscapeType.Ethereal] = new List<DreamscapeLayer>();
        for (int i = 1; i <= 15; i++)
        {
            bool isBoss = (i % 5 == 0);
            var layer = new DreamscapeLayer
            {
                LayerNumber = i,
                EnemyType = isBoss ? "EtherealLord" : "Spirit",
                EnemyCount = 2 + i,
                SpecialRule = DreamscapeRule.FloatGravity,
                TimeLimit = isBoss ? 150 : 90,
                BaseScore = 80 * i,
                BaseGold = 40 * i,
                BaseExperience = 25 * i,
                IsBossLayer = isBoss,
                BossType = isBoss ? "EtherealQueen" : ""
            };
            LayerTemplates[DreamscapeType.Ethereal].Add(layer);
        }
        
        // 虚空梦境层配置
        LayerTemplates[DreamscapeType.Void] = new List<DreamscapeLayer>();
        for (int i = 1; i <= 25; i++)
        {
            bool isBoss = (i % 5 == 0);
            var layer = new DreamscapeLayer
            {
                LayerNumber = i,
                EnemyType = isBoss ? "VoidTitan" : "VoidSpawn",
                EnemyCount = 4 + i * 2,
                SpecialRule = DreamscapeRule.NoDeathPenalty,
                TimeLimit = isBoss ? 200 : 150,
                BaseScore = 120 * i,
                BaseGold = 60 * i,
                BaseExperience = 35 * i,
                IsBossLayer = isBoss,
                BossType = isBoss ? $"VoidEmperor{i / 5}" : ""
            };
            LayerTemplates[DreamscapeType.Void].Add(layer);
        }
        
        // 时间梦境层配置
        LayerTemplates[DreamscapeType.Temporal] = new List<DreamscapeLayer>();
        for (int i = 1; i <= 18; i++)
        {
            bool isBoss = (i % 6 == 0);
            var layer = new DreamscapeLayer
            {
                LayerNumber = i,
                EnemyType = isBoss ? "TimeWraith" : "ChronosSpawn",
                EnemyCount = 3 + (i / 2),
                SpecialRule = DreamscapeRule.TimeSlowdown,
                TimeLimit = isBoss ? 160 : 100,
                BaseScore = 90 * i,
                BaseGold = 45 * i,
                BaseExperience = 28 * i,
                IsBossLayer = isBoss,
                BossType = isBoss ? "Chronos" : ""
            };
            LayerTemplates[DreamscapeType.Temporal].Add(layer);
        }
        
        // 清醒梦境层配置
        LayerTemplates[DreamscapeType.Lucid] = new List<DreamscapeLayer>();
        for (int i = 1; i <= 30; i++)
        {
            bool isBoss = (i % 10 == 0);
            var layer = new DreamscapeLayer
            {
                LayerNumber = i,
                EnemyType = isBoss ? "DreamWeaver" : "ThoughtForm",
                EnemyCount = 5 + i,
                SpecialRule = DreamscapeRule.NoCooldown,
                TimeLimit = isBoss ? 240 : 180,
                BaseScore = 150 * i,
                BaseGold = 75 * i,
                BaseExperience = 50 * i,
                IsBossLayer = isBoss,
                BossType = isBoss ? $"LucidMaster{i / 10}" : ""
            };
            LayerTemplates[DreamscapeType.Lucid].Add(layer);
        }
    }
    
    private void InitializeLayerRewards()
    {
        // 每种梦境类型的层奖励配置
        foreach (DreamscapeType type in Enum.GetValues(typeof(DreamscapeType)))
        {
            LayerRewards[type] = new Dictionary<int, DreamscapeReward>();
            int maxLayers = type == DreamscapeType.Lucid ? 30 : 
                           type == DreamscapeType.Void ? 25 :
                           type == DreamscapeType.Nightmare ? 20 :
                           type == DreamscapeType.Temporal ? 18 : 15;
            
            for (int layer = 1; layer <= maxLayers; layer++)
            {
                float bossMultiplier = (layer % 5 == 0 || (type == DreamscapeType.Temporal && layer % 6 == 0) || (type == DreamscapeType.Lucid && layer % 10 == 0)) ? 2.0f : 1.0f;
                
                LayerRewards[type][layer] = new DreamscapeReward
                {
                    Gold = (int)(50 * layer * bossMultiplier),
                    Experience = (int)(25 * layer * bossMultiplier),
                    Items = new List<string>(),
                    DropRateBonus = 0.1f * layer,
                    BonusScore = 500 * layer
                };
            }
        }
    }
    
    public DreamscapeEntry GetDreamscape(string id)
    {
        return Dreamscapes.ContainsKey(id) ? Dreamscapes[id] : null;
    }
    
    public DreamscapeEntry GetDreamscapeByType(DreamscapeType type)
    {
        foreach (var ds in Dreamscapes.Values)
        {
            if (ds.Type == type) return ds;
        }
        return null;
    }
    
    public DreamscapeLayer GetLayer(DreamscapeType type, int layerNumber)
    {
        if (LayerTemplates.ContainsKey(type) && layerNumber > 0 && layerNumber <= LayerTemplates[type].Count)
        {
            return LayerTemplates[type][layerNumber - 1];
        }
        return null;
    }
    
    public DreamscapeReward GetLayerReward(DreamscapeType type, int layerNumber)
    {
        if (LayerRewards.ContainsKey(type) && LayerRewards[type].ContainsKey(layerNumber))
        {
            return LayerRewards[type][layerNumber];
        }
        return null;
    }
    
    public List<DreamscapeEntry> GetUnlockedDreamscapes()
    {
        var result = new List<DreamscapeEntry>();
        foreach (var ds in Dreamscapes.Values)
        {
            if (ds.State != DreamscapeState.Locked)
            {
                result.Add(ds);
            }
        }
        return result;
    }
    
    public void UnlockDreamscape(DreamscapeType type)
    {
        var ds = GetDreamscapeByType(type);
        if (ds != null && ds.State == DreamscapeState.Locked)
        {
            ds.State = DreamscapeState.Available;
        }
    }
    
    public void CheckAndUnlockDreamscapes(int playerLevel)
    {
        foreach (var ds in Dreamscapes.Values)
        {
            if (ds.State == DreamscapeState.Locked && playerLevel >= ds.RequiredPlayerLevel)
            {
                ds.State = DreamscapeState.Available;
            }
        }
    }
}
