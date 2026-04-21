using Godot;
using System;
using System.Collections.Generic;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

public partial class GuildDiplomacyData : BaseSystem
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
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var relationsData = new Godot.Array();
        foreach (var kvp in Relations)
        {
            var relationDict = new Dictionary
            {
                { "guild_id", kvp.Key },
                { "guild_name", kvp.Value.GuildName },
                { "type", (int)kvp.Value.Type },
                { "trust", kvp.Value.Trust },
                { "treaty_turns", kvp.Value.TreatyTurns }
            };
            relationsData.Add(relationDict);
        }
        
        return new Dictionary
        {
            { "relations", relationsData }
        };
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        Relations.Clear();
        
        if (data.Contains("relations"))
        {
            var relationsArray = data["relations"] as Godot.Array;
            foreach (Dictionary relData in relationsArray)
            {
                string guildId = (string)relData["guild_id"];
                var relation = new GuildRelation
                {
                    GuildId = guildId,
                    GuildName = (string)relData["guild_name"],
                    Type = (RelationType)(int)relData["type"],
                    Trust = (int)relData["trust"],
                    TreatyTurns = (int)relData["treaty_turns"],
                    LastUpdate = DateTime.Now
                };
                Relations[guildId] = relation;
            }
        }
    }
}
