using Godot;
using System;
using System.Collections.Generic;

public class SkillTreeSystem : BaseSystem
{
    private static SkillTreeSystem _instance;
    public static SkillTreeSystem Instance => _instance ??= new SkillTreeSystem();
    
    public PlayerSkillTreeData PlayerData { get; private set; }
    public SkillTreeDatabase Database { get; private set; }
    
    // Signals
    public Action<string> OnNodeUnlocked;
    public Action<string> OnNodeLocked;
    public Action OnSkillPointsChanged;
    public Action OnCategoryUnlocked;
    
    private SkillTreeSystem()
    {
        Database = SkillTreeDatabase.Instance;
        PlayerData = new PlayerSkillTreeData();
    }

    protected override void Initialize()
    {
        base.Initialize();
        // Give player initial skill points based on level if needed
        if (PlayerData.TotalSkillPoints == 0)
        {
            PlayerData.TotalSkillPoints = 10; // Starting points
        }
    }

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Godot.Dictionary();
        
        // 保存已解锁的节点
        var unlockedNodes = new Godot.Dictionary();
        foreach (var kvp in PlayerData.UnlockedNodes)
        {
            unlockedNodes[kvp.Key] = kvp.Value;
        }
        data["unlocked_nodes"] = unlockedNodes;
        
        // 保存每个分类的技能点花费
        var skillPointsSpent = new Godot.Dictionary();
        foreach (var kvp in PlayerData.SkillPointsSpent)
        {
            skillPointsSpent[kvp.Key] = kvp.Value;
        }
        data["skill_points_spent"] = skillPointsSpent;
        
        // 保存技能点数据
        data["total_skill_points"] = PlayerData.TotalSkillPoints;
        data["used_skill_points"] = PlayerData.UsedSkillPoints;
        
        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 加载已解锁的节点
        if (data.Contains("unlocked_nodes"))
        {
            PlayerData.UnlockedNodes.Clear();
            var unlockedNodes = (Godot.Dictionary)data["unlocked_nodes"];
            foreach (string key in unlockedNodes.Keys)
            {
                PlayerData.UnlockedNodes[key] = (int)unlockedNodes[key];
            }
        }
        
        // 加载每个分类的技能点花费
        if (data.Contains("skill_points_spent"))
        {
            PlayerData.SkillPointsSpent.Clear();
            var skillPointsSpent = (Godot.Dictionary)data["skill_points_spent"];
            foreach (string key in skillPointsSpent.Keys)
            {
                PlayerData.SkillPointsSpent[key] = (int)skillPointsSpent[key];
            }
        }
        
        // 加载技能点数据
        if (data.Contains("total_skill_points"))
            PlayerData.TotalSkillPoints = (int)data["total_skill_points"];
            
        if (data.Contains("used_skill_points"))
            PlayerData.UsedSkillPoints = (int)data["used_skill_points"];
        
        GD.Print($"[SkillTree] Loaded: {GetUnlockedNodeCount()} nodes unlocked, {GetAvailableSkillPoints()} points available");
    }
    
    public void AddSkillPoints(int amount)
    {
        PlayerData.TotalSkillPoints += amount;
        OnSkillPointsChanged?.Invoke();
    }
    
    public bool CanUnlockNode(string nodeId)
    {
        return Database.CanUnlockNode(nodeId, PlayerData);
    }
    
    public bool UnlockNode(string nodeId)
    {
        if (!CanUnlockNode(nodeId))
            return false;
            
        var node = Database.GetNode(nodeId);
        if (node == null)
            return false;
        
        // Deduct skill points
        PlayerData.UsedSkillPoints += node.Cost;
        
        // Mark node as unlocked
        PlayerData.UnlockedNodes[nodeId] = 1;
        
        // Track points spent per category
        if (!PlayerData.SkillPointsSpent.ContainsKey(node.SkillTreeCategory))
            PlayerData.SkillPointsSpent[node.SkillTreeCategory] = 0;
        PlayerData.SkillPointsSpent[node.SkillTreeCategory] += node.Cost;
        
        // Fire signals
        OnNodeUnlocked?.Invoke(nodeId);
        OnSkillPointsChanged?.Invoke();
        
        // Check if category is fully unlocked
        CheckCategoryUnlock(node.SkillTreeCategory);
        
        GD.Print($"[SkillTree] Unlocked node: {node.Name} for {node.Cost} points");
        return true;
    }
    
    private void CheckCategoryUnlock(string category)
    {
        if (!Database.Categories.ContainsKey(category))
            return;
            
        var categoryData = Database.Categories[category];
        int spent = PlayerData.SkillPointsSpent.ContainsKey(category) ? 
            PlayerData.SkillPointsSpent[category] : 0;
            
        if (spent >= categoryData.MaxPoints)
        {
            OnCategoryUnlocked?.Invoke();
        }
    }
    
    public bool IsNodeUnlocked(string nodeId)
    {
        return PlayerData.UnlockedNodes.ContainsKey(nodeId) && 
               PlayerData.UnlockedNodes[nodeId] > 0;
    }
    
    public Dictionary<string, float> GetAllAttributeBonuses()
    {
        var bonuses = new Dictionary<string, float>();
        
        foreach (var unlocked in PlayerData.UnlockedNodes)
        {
            if (unlocked.Value > 0)
            {
                var node = Database.GetNode(unlocked.Key);
                if (node != null && node.AttributeBonuses != null)
                {
                    foreach (var bonus in node.AttributeBonuses)
                    {
                        if (!bonuses.ContainsKey(bonus.Key))
                            bonuses[bonus.Key] = 0;
                        bonuses[bonus.Key] += bonus.Value;
                    }
                }
            }
        }
        
        return bonuses;
    }
    
    public float GetAttributeBonus(string attribute)
    {
        float total = 0;
        
        foreach (var unlocked in PlayerData.UnlockedNodes)
        {
            if (unlocked.Value > 0)
            {
                var node = Database.GetNode(unlocked.Key);
                if (node != null && node.AttributeBonuses != null && 
                    node.AttributeBonuses.ContainsKey(attribute))
                {
                    total += node.AttributeBonuses[attribute];
                }
            }
        }
        
        return total;
    }
    
    public int GetAvailableSkillPoints()
    {
        return PlayerData.TotalSkillPoints - PlayerData.UsedSkillPoints;
    }
    
    public int GetSpentPointsInCategory(string category)
    {
        return PlayerData.SkillPointsSpent.ContainsKey(category) ? 
            PlayerData.SkillPointsSpent[category] : 0;
    }
    
    public int GetUnlockedNodeCount()
    {
        int count = 0;
        foreach (var node in PlayerData.UnlockedNodes.Values)
        {
            count += node;
        }
        return count;
    }
    
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            ["total_skill_points"] = PlayerData.TotalSkillPoints,
            ["used_skill_points"] = PlayerData.UsedSkillPoints,
            ["available_skill_points"] = GetAvailableSkillPoints(),
            ["unlocked_nodes"] = GetUnlockedNodeCount(),
            ["total_nodes"] = Database.AllNodes.Count,
            ["category_points"] = PlayerData.SkillPointsSpent
        };
    }
    
    public void Reset()
    {
        PlayerData = new PlayerSkillTreeData();
        PlayerData.TotalSkillPoints = 10;
        OnSkillPointsChanged?.Invoke();
        GD.Print("[SkillTree] Reset to default");
    }
}
