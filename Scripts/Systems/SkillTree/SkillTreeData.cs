using Godot;
using System;
using System.Collections.Generic;

public class SkillTreeNode
{
    public string NodeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Tier { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }
    public int Cost { get; set; }
    public string ParentNodeId { get; set; }
    public List<string> ChildNodeIds { get; set; }
    public string SkillTreeCategory { get; set; }
    public Dictionary<string, float> AttributeBonuses { get; set; }
    public string RequiredSkill { get; set; }
    public int RequiredSkillLevel { get; set; }
    
    public SkillTreeNode()
    {
        ChildNodeIds = new List<string>();
        AttributeBonuses = new Dictionary<string, float>();
    }
}

public class SkillTreeCategory
{
    public string CategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public string Color { get; set; }
    public int MaxPoints { get; set; }
}

public class PlayerSkillTreeData
{
    public Dictionary<string, int> UnlockedNodes { get; set; }
    public Dictionary<string, int> SkillPointsSpent { get; set; }
    public int TotalSkillPoints { get; set; }
    public int UsedSkillPoints { get; set; }
    
    public PlayerSkillTreeData()
    {
        UnlockedNodes = new Dictionary<string, int>();
        SkillPointsSpent = new Dictionary<string, int>();
    }
}
