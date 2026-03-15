using Godot;
using System;
using System.Collections.Generic;

public class GuildDiplomacyData : BaseSystem
{
    // 外交关系数据
    public Dictionary<string, GuildRelation> Relations = new Dictionary<string, GuildRelation>();
    
    // 关系类型
    public enum RelationType
    {
        Neutral = 0,
        Ally = 1,
        Enemy = 2,
        NonAggression = 3
    }
    
    // 关系结构
    public class GuildRelation
    {
        public string GuildId;
        public string GuildName;
        public RelationType Type;
        public int Trust; // -100 to 100
        public int TreatyTurns; // 条约剩余回合
        public DateTime LastUpdate;
    }
    
    public override void _Ready()
    {
        SaveSystem.DataLoaded += LoadData;
    }
    
    public void LoadData()
    {
        if (SaveSystem.CurrentSave.GuildDiplomacyData != null)
        {
            var data = SaveSystem.CurrentSave.GuildDiplomacyData;
            // 加载外交关系数据
        }
    }
    
    public Dictionary<string, object> Save()
    {
        return new Dictionary<string, object>
        {
            { "Relations", Relations }
        };
    }
}
