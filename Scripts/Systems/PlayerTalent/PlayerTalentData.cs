using System;
using System.Collections.Generic;
using Godot;

public class PlayerTalentData
{
    public enum TalentTree
    {
        Combat,      // 战斗型
        Defense,     // 防御型
        Support,     // 辅助型
        Agility      // 敏捷型
    }
    
    public enum TalentRarity
    {
        Basic,      // 基础
        Advanced,   // 进阶
        Expert,     // 专家
        Master      // 大师
    }
    
    public class TalentNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TalentTree Tree { get; set; }
        public TalentRarity Rarity { get; set; }
        public int Tier { get; set; }          // 1-5 层
        public int Cost { get; set; }          // 点数需求
        public Dictionary<string, float> Bonuses { get; set; }  // 属性加成
        public List<string> Requires { get; set; }  // 前置天赋
    }
    
    public class PlayerTalentSaveData
    {
        public Dictionary<TalentTree, int> UnlockedTrees { get; set; } = new Dictionary<TalentTree, int>();
        public Dictionary<TalentTree, int> TreePoints { get; set; } = new Dictionary<TalentTree, int>();  // 已分配点数
        public HashSet<string> UnlockedTalents { get; set; } = new HashSet<string>();
        public int TotalPointsSpent { get; set; }
    }
}
