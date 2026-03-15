using Godot;
using System;
using System.Collections.Generic;

public class GuildDiplomacySystem : BaseSystem
{
    // 外交系统
    private GuildDiplomacyData diplomacyData;
    private GuildSystem guildSystem;
    
    // 信号
    [Signal] public delegate void RelationChanged(string guildId, GuildDiplomacyData.RelationType newType);
    [Signal] public delegate void TreatySigned(string guildId, GuildDiplomacyData.RelationType type, int duration);
    [Signal] public delegate void TreatyBroken(string guildId);
    
    // 关系加成配置
    private Dictionary<GuildDiplomacyData.RelationType, Dictionary<string, float>> RelationBonuses = new Dictionary<GuildDiplomacyData.RelationType, Dictionary<string, float>>
    {
        { GuildDiplomacyData.RelationType.Ally, new Dictionary<string, float>
            {
                { "gold_bonus", 0.25f },
                { "exp_bonus", 0.15f },
                { "trade_discount", 0.20f },
                { "attack_bonus", 0.10f }
            }
        },
        { GuildDiplomacyData.RelationType.NonAggression, new Dictionary<string, float>
            {
                { "trade_discount", 0.10f },
                { "defense_bonus", 0.05f }
            }
        },
        { GuildDiplomacyData.RelationType.Enemy, new Dictionary<string, float>
            {
                { "gold_bonus", -0.15f },
                { "exp_bonus", -0.10f },
                { "trade_discount", -0.15f },
                { "attack_bonus", 0.15f }
            }
        },
        { GuildDiplomacyData.RelationType.Neutral, new Dictionary<string, float>
            {
                { "gold_bonus", 0f },
                { "exp_bonus", 0f },
                { "trade_discount", 0f },
                { "attack_bonus", 0f }
            }
        }
    };
    
    public override void _Ready()
    {
        diplomacyData = GetNode<GuildDiplomacyData>("/root/SaveSystem/GuildDiplomacyData");
        guildSystem = GetNode<GuildSystem>("/root/Game/GuildSystem");
        
        // 连接信号
        SaveSystem.DataLoaded += OnDataLoaded;
    }
    
    public void OnDataLoaded()
    {
        // 初始化外交数据
    }
    
    // 设置外交关系
    public bool SetRelation(string targetGuildId, GuildDiplomacyData.RelationType type, int treatyTurns = 0)
    {
        if (guildSystem == null || !guildSystem.HasGuild) return false;
        
        var relation = new GuildDiplomacyData.GuildRelation
        {
            GuildId = targetGuildId,
            Type = type,
            Trust = 0,
            TreatyTurns = treatyTurns,
            LastUpdate = DateTime.Now
        };
        
        diplomacyData.Relations[targetGuildId] = relation;
        
        EmitSignal(nameof(RelationChanged), targetGuildId, (int)type);
        
        if (treatyTurns > 0)
        {
            EmitSignal(nameof(TreatySigned), targetGuildId, (int)type, treatyTurns);
        }
        
        SaveSystem.SaveGame();
        return true;
    }
    
    // 发起外交提议
    public bool SendProposal(string targetGuildId, GuildDiplomacyData.RelationType proposedType)
    {
        // 这里可以添加提议逻辑
        return SetRelation(targetGuildId, proposedType);
    }
    
    // 终止条约
    public bool BreakTreaty(string targetGuildId)
    {
        if (!diplomacyData.Relations.ContainsKey(targetGuildId)) return false;
        
        diplomacyData.Relations.Remove(targetGuildId);
        
        EmitSignal(nameof(TreatyBroken), targetGuildId);
        SaveSystem.SaveGame();
        
        return true;
    }
    
    // 获取关系加成
    public Dictionary<string, float> GetRelationBonuses(string targetGuildId)
    {
        if (!diplomacyData.Relations.ContainsKey(targetGuildId))
            return RelationBonuses[GuildDiplomacyData.RelationType.Neutral];
        
        var type = diplomacyData.Relations[targetGuildId].Type;
        return RelationBonuses[type];
    }
    
    // 计算关系信任变化
    public void UpdateTrust(string targetGuildId, int trustChange)
    {
        if (!diplomacyData.Relations.ContainsKey(targetGuildId)) return;
        
        var relation = diplomacyData.Relations[targetGuildId];
        relation.Trust = Mathf.Clamp(relation.Trust + trustChange, -100, 100);
        relation.LastUpdate = DateTime.Now;
        
        // 信任变化可能影响关系类型
        if (relation.Trust >= 75 && relation.Type == GuildDiplomacyData.RelationType.Neutral)
        {
            SetRelation(targetGuildId, GuildDiplomacyData.RelationType.NonAggression);
        }
        else if (relation.Trust <= -75 && relation.Type != GuildDiplomacyData.RelationType.Enemy)
        {
            SetRelation(targetGuildId, GuildDiplomacyData.RelationType.Enemy);
        }
    }
    
    // 更新条约回合
    public void UpdateTreaties()
    {
        foreach (var kvp in diplomacyData.Relations)
        {
            if (kvp.Value.TreatyTurns > 0)
            {
                kvp.Value.TreatyTurns--;
                
                if (kvp.Value.TreatyTurns <= 0)
                {
                    // 条约到期，转为中立
                    SetRelation(kvp.Key, GuildDiplomacyData.RelationType.Neutral);
                }
            }
        }
    }
    
    // 获取所有关系
    public Dictionary<string, GuildDiplomacyData.GuildRelation> GetAllRelations()
    {
        return diplomacyData.Relations;
    }
    
    // 获取特定关系
    public GuildDiplomacyData.GuildRelation GetRelation(string targetGuildId)
    {
        if (diplomacyData.Relations.ContainsKey(targetGuildId))
            return diplomacyData.Relations[targetGuildId];
        
        return null;
    }
    
    // 获取盟友列表
    public List<string> GetAllies()
    {
        var allies = new List<string>();
        foreach (var kvp in diplomacyData.Relations)
        {
            if (kvp.Value.Type == GuildDiplomacyData.RelationType.Ally)
                allies.Add(kvp.Key);
        }
        return allies;
    }
    
    // 获取敌对公会列表
    public List<string> GetEnemies()
    {
        var enemies = new List<string>();
        foreach (var kvp in diplomacyData.Relations)
        {
            if (kvp.Value.Type == GuildDiplomacyData.RelationType.Enemy)
                enemies.Add(kvp.Key);
        }
        return enemies;
    }
}
