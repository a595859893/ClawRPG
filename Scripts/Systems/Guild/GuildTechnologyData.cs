using System;
using System.Collections.Generic;
using Godot;

public class GuildTechnologyData
{
    // 科技类型
    public enum TechCategory
    {
        Combat,      // 战斗
        Economy,     // 经济
        Production,  // 生产
        Social,      // 社交
        Defense      // 防御
    }

    // 科技等级
    public enum TechLevel
    {
        Basic,     // 基础
        Advanced,  // 高级
        Master,    // 大师
        Legendary  // 传奇
    }

    // 单个科技数据
    public class Technology
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TechCategory Category { get; set; }
        public TechLevel Level { get; set; }
        public int ResearchCost { get; set; }  // 研究费用
        public int ResearchTime { get; set; }   // 研究时间(秒)
        public Dictionary<string, float> Bonuses { get; set; }  // 加成属性
        public int MaxLevel { get; set; }  // 最大等级
    }

    // 公会科技进度
    public class GuildTechnologyProgress
    {
        public string TechId { get; set; }
        public int CurrentLevel { get; set; }
        public bool IsResearching { get; set; }
        public long ResearchStartTime { get; set; }
        public int TotalResearchTime { get; set; }
    }

    // 公会科技数据
    public Dictionary<string, GuildTechnologyProgress> GuildTechs { get; set; } = new Dictionary<string, GuildTechnologyProgress>();
    public int AvailablePoints { get; set; }  // 可用科技点数
    public int TotalResearched { get; set; }  // 已研究科技数
}
