using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;

/// <summary>
/// Skill Tree Database - stores all skill tree node configurations
/// </summary>
public class SkillTreeDatabase : DatabaseBase
{
    private static SkillTreeDatabase _instance;
    public static SkillTreeDatabase Instance => _instance ??= new SkillTreeDatabase();

    public Dictionary<string, SkillTreeNode> AllNodes { get; private set; }
    public Dictionary<string, SkillTreeCategory> Categories { get; private set; }

    public SkillTreeDatabase()
    {
        AllNodes = new Dictionary<string, SkillTreeNode>();
        Categories = new Dictionary<string, SkillTreeCategory>();
        Initialize();
    }

    public override void Initialize()
    {
        InitializeCategories();
        InitializeNodes();
        Categories["combat"] = new SkillTreeCategory
        {
            CategoryId = "combat",
            Name = "Combat",
            Description = "Combat-focused skills",
            Icon = "⚔️",
            Color = "#FF4444",
            MaxPoints = 20
        };
        
        Categories["defense"] = new SkillTreeCategory
        {
            CategoryId = "defense",
            Name = "Defense",
            Description = "Defense and survival skills",
            Icon = "🛡️",
            Color = "#4488FF",
            MaxPoints = 20
        };
        
        Categories["magic"] = new SkillTreeCategory
        {
            CategoryId = "magic",
            Name = "Magic",
            Description = "Magic and elemental skills",
            Icon = "✨",
            Color = "#AA44FF",
            MaxPoints = 20
        };
        
        Categories["utility"] = new SkillTreeCategory
        {
            CategoryId = "utility",
            Name = "Utility",
            Description = "General utility skills",
            Icon = "🔧",
            Color = "#44FF88",
            MaxPoints = 15
        };
        
        Categories["special"] = new SkillTreeCategory
        {
            CategoryId = "special",
            Name = "Special",
            Description = "Special and legendary skills",
            Icon = "⭐",
            Color = "#FFD700",
            MaxPoints = 10
        };
    }
    
    private void InitializeNodes()
    {
        // Combat Skills - Column 0
        AddNode(new SkillTreeNode
        {
            NodeId = "combat_basic_1",
            Name = "Basic Attack",
            Description = "Increases base attack damage by 5%",
            Tier = 1,
            Column = 0,
            Row = 0,
            Cost = 1,
            ParentNodeId = "",
            SkillTreeCategory = "combat",
            AttributeBonuses = { ["attack"] = 0.05f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "combat_basic_2",
            Name = "Power Strike",
            Description = "Increases critical hit damage by 10%",
            Tier = 1,
            Column = 0,
            Row = 1,
            Cost = 1,
            ParentNodeId = "combat_basic_1",
            SkillTreeCategory = "combat",
            AttributeBonuses = { ["critical_damage"] = 0.10f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "combat_advanced_1",
            Name = "Double Strike",
            Description = "5% chance to strike twice",
            Tier = 2,
            Column = 1,
            Row = 0,
            Cost = 2,
            ParentNodeId = "combat_basic_2",
            SkillTreeCategory = "combat",
            AttributeBonuses = { ["double_strike"] = 0.05f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "combat_advanced_2",
            Name = "Bleed Wounds",
            Description = "Attacks have 10% chance to cause bleeding",
            Tier = 2,
            Column = 1,
            Row = 1,
            Cost = 2,
            ParentNodeId = "combat_advanced_1",
            SkillTreeCategory = "combat",
            AttributeBonuses = { ["bleed_chance"] = 0.10f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "combat_expert_1",
            Name = "Berserker",
            Description = "Gain 20% attack bonus when below 30% health",
            Tier = 3,
            Column = 2,
            Row = 0,
            Cost = 3,
            ParentNodeId = "combat_advanced_1",
            SkillTreeCategory = "combat",
            AttributeBonuses = { ["berserker_bonus"] = 0.20f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "combat_expert_2",
            Name = "Deadly Precision",
            Description = "Critical hit chance increased by 8%",
            Tier = 3,
            Column = 2,
            Row = 1,
            Cost = 3,
            ParentNodeId = "combat_advanced_2",
            SkillTreeCategory = "combat",
            AttributeBonuses = { ["critical_rate"] = 0.08f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "combat_master",
            Name = "God of War",
            Description = "Ultimate combat mastery - +15% all damage",
            Tier = 4,
            Column = 3,
            Row = 0,
            Cost = 5,
            ParentNodeId = "combat_expert_1",
            SkillTreeCategory = "combat",
            AttributeBonuses = { ["all_damage"] = 0.15f }
        });
        
        // Defense Skills - Column 4
        AddNode(new SkillTreeNode
        {
            NodeId = "defense_basic_1",
            Name = "Thick Skin",
            Description = "Increases base defense by 5%",
            Tier = 1,
            Column = 4,
            Row = 0,
            Cost = 1,
            ParentNodeId = "",
            SkillTreeCategory = "defense",
            AttributeBonuses = { ["defense"] = 0.05f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "defense_basic_2",
            Name = "Dodge",
            Description = "Increases dodge chance by 3%",
            Tier = 1,
            Column = 4,
            Row = 1,
            Cost = 1,
            ParentNodeId = "defense_basic_1",
            SkillTreeCategory = "defense",
            AttributeBonuses = { ["dodge"] = 0.03f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "defense_advanced_1",
            Name = "Iron Will",
            Description = "Reduces crowd control duration by 15%",
            Tier = 2,
            Column = 5,
            Row = 0,
            Cost = 2,
            ParentNodeId = "defense_basic_2",
            SkillTreeCategory = "defense",
            AttributeBonuses = { ["cc_reduction"] = 0.15f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "defense_advanced_2",
            Name = "Shield Mastery",
            Description = "Increases shield block chance by 10%",
            Tier = 2,
            Column = 5,
            Row = 1,
            Cost = 2,
            ParentNodeId = "defense_advanced_1",
            SkillTreeCategory = "defense",
            AttributeBonuses = { ["block"] = 0.10f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "defense_expert_1",
            Name = "Last Stand",
            Description = "Gain 50% defense bonus when below 25% health",
            Tier = 3,
            Column = 6,
            Row = 0,
            Cost = 3,
            ParentNodeId = "defense_advanced_1",
            SkillTreeCategory = "defense",
            AttributeBonuses = { ["last_stand_bonus"] = 0.50f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "defense_expert_2",
            Name = "Turtle Shell",
            Description = "Maximum health increased by 10%",
            Tier = 3,
            Column = 6,
            Row = 1,
            Cost = 3,
            ParentNodeId = "defense_advanced_2",
            SkillTreeCategory = "defense",
            AttributeBonuses = { ["max_health"] = 0.10f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "defense_master",
            Name = "Iron Fortress",
            Description = "Ultimate defense - +20% damage reduction",
            Tier = 4,
            Column = 7,
            Row = 0,
            Cost = 5,
            ParentNodeId = "defense_expert_1",
            SkillTreeCategory = "defense",
            AttributeBonuses = { ["damage_reduction"] = 0.20f }
        });
        
        // Magic Skills - Column 8
        AddNode(new SkillTreeNode
        {
            NodeId = "magic_basic_1",
            Name = "Mana Flow",
            Description = "Increases mana regeneration by 10%",
            Tier = 1,
            Column = 8,
            Row = 0,
            Cost = 1,
            ParentNodeId = "",
            SkillTreeCategory = "magic",
            AttributeBonuses = { ["mana_regen"] = 0.10f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "magic_basic_2",
            Name = "Arcane Wisdom",
            Description = "Increases magic damage by 5%",
            Tier = 1,
            Column = 8,
            Row = 1,
            Cost = 1,
            ParentNodeId = "magic_basic_1",
            SkillTreeCategory = "magic",
            AttributeBonuses = { ["magic_damage"] = 0.05f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "magic_advanced_1",
            Name = "Elemental Mastery",
            Description = "Increases elemental damage by 8%",
            Tier = 2,
            Column = 9,
            Row = 0,
            Cost = 2,
            ParentNodeId = "magic_basic_2",
            SkillTreeCategory = "magic",
            AttributeBonuses = { ["elemental_damage"] = 0.08f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "magic_advanced_2",
            Name = "Mana Efficiency",
            Description = "Reduces mana cost by 10%",
            Tier = 2,
            Column = 9,
            Row = 1,
            Cost = 2,
            ParentNodeId = "magic_advanced_1",
            SkillTreeCategory = "magic",
            AttributeBonuses = { ["mana_cost_reduction"] = 0.10f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "magic_expert_1",
            Name = "Arcane Burst",
            Description = "Spells have 15% chance to cause explosion",
            Tier = 3,
            Column = 10,
            Row = 0,
            Cost = 3,
            ParentNodeId = "magic_advanced_1",
            SkillTreeCategory = "magic",
            AttributeBonuses = { ["arcane_burst_chance"] = 0.15f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "magic_expert_2",
            Name = "Spell Power",
            Description = "Increases spell critical chance by 5%",
            Tier = 3,
            Column = 10,
            Row = 1,
            Cost = 3,
            ParentNodeId = "magic_advanced_2",
            SkillTreeCategory = "magic",
            AttributeBonuses = { ["spell_crit"] = 0.05f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "magic_master",
            Name = "Archmage",
            Description = "Ultimate magic mastery - +25% magic damage",
            Tier = 4,
            Column = 11,
            Row = 0,
            Cost = 5,
            ParentNodeId = "magic_expert_1",
            SkillTreeCategory = "magic",
            AttributeBonuses = { ["all_magic_damage"] = 0.25f }
        });
        
        // Utility Skills - Column 12
        AddNode(new SkillTreeNode
        {
            NodeId = "utility_basic_1",
            Name = "Swift Learner",
            Description = "Experience gain increased by 5%",
            Tier = 1,
            Column = 12,
            Row = 0,
            Cost = 1,
            ParentNodeId = "",
            SkillTreeCategory = "utility",
            AttributeBonuses = { ["exp_gain"] = 0.05f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "utility_basic_2",
            Name = "Treasure Hunter",
            Description = "Gold find increased by 10%",
            Tier = 1,
            Column = 12,
            Row = 1,
            Cost = 1,
            ParentNodeId = "utility_basic_1",
            SkillTreeCategory = "utility",
            AttributeBonuses = { ["gold_find"] = 0.10f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "utility_advanced_1",
            Name = "Lucky Star",
            Description = "Item drop rate increased by 8%",
            Tier = 2,
            Column = 13,
            Row = 0,
            Cost = 2,
            ParentNodeId = "utility_basic_2",
            SkillTreeCategory = "utility",
            AttributeBonuses = { ["drop_rate"] = 0.08f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "utility_advanced_2",
            Name = "Merchant's Eye",
            Description = "Shop prices reduced by 10%",
            Tier = 2,
            Column = 13,
            Row = 1,
            Cost = 2,
            ParentNodeId = "utility_advanced_1",
            SkillTreeCategory = "utility",
            AttributeBonuses = { ["shop_discount"] = 0.10f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "utility_expert",
            Name = "Jack of All Trades",
            Description = "All attributes increased by 3%",
            Tier = 3,
            Column = 14,
            Row = 0,
            Cost = 3,
            ParentNodeId = "utility_advanced_1",
            SkillTreeCategory = "utility",
            AttributeBonuses = { ["all_attributes"] = 0.03f }
        });
        
        // Special Skills - Column 15
        AddNode(new SkillTreeNode
        {
            NodeId = "special_legendary_1",
            Name = "Dragon's Blessing",
            Description = "Legendary power - +10% to all stats",
            Tier = 4,
            Column = 15,
            Row = 0,
            Cost = 5,
            ParentNodeId = "",
            SkillTreeCategory = "special",
            AttributeBonuses = { ["all_stats"] = 0.10f }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "special_legendary_2",
            Name = "Phoenix Rebirth",
            Description = "One free revival per dungeon",
            Tier = 4,
            Column = 15,
            Row = 1,
            Cost = 5,
            ParentNodeId = "special_legendary_1",
            SkillTreeCategory = "special",
            AttributeBonuses = { ["free_revival"] = 1 }
        });
        
        AddNode(new SkillTreeNode
        {
            NodeId = "special_legendary_3",
            Name = "Mythical Form",
            Description = "Transform into mythical form for 30 seconds",
            Tier = 5,
            Column = 16,
            Row = 0,
            Cost = 8,
            ParentNodeId = "special_legendary_2",
            SkillTreeCategory = "special",
            AttributeBonuses = { ["mythical_form_duration"] = 30 }
        });
        
        // Connect child nodes to parents
        ConnectChildNodes();
    }
    
    private void AddNode(SkillTreeNode node)
    {
        AllNodes[node.NodeId] = node;
    }
    
    private void ConnectChildNodes()
    {
        foreach (var node in AllNodes.Values)
        {
            if (!string.IsNullOrEmpty(node.ParentNodeId) && AllNodes.ContainsKey(node.ParentNodeId))
            {
                AllNodes[node.ParentNodeId].ChildNodeIds.Add(node.NodeId);
            }
        }
    }
    
    public SkillTreeNode GetNode(string nodeId)
    {
        return AllNodes.ContainsKey(nodeId) ? AllNodes[nodeId] : null;
    }
    
    public List<SkillTreeNode> GetNodesByCategory(string category)
    {
        var nodes = new List<SkillTreeNode>();
        foreach (var node in AllNodes.Values)
        {
            if (node.SkillTreeCategory == category)
            {
                nodes.Add(node);
            }
        }
        return nodes;
    }
    
    public List<SkillTreeNode> GetRootNodes()
    {
        var roots = new List<SkillTreeNode>();
        foreach (var node in AllNodes.Values)
        {
            if (string.IsNullOrEmpty(node.ParentNodeId))
            {
                roots.Add(node);
            }
        }
        return roots;
    }
    
    public bool CanUnlockNode(string nodeId, PlayerSkillTreeData playerData)
    {
        if (!AllNodes.ContainsKey(nodeId))
            return false;
            
        var node = AllNodes[nodeId];
        
        // Check if already unlocked
        if (playerData.UnlockedNodes.ContainsKey(nodeId) && playerData.UnlockedNodes[nodeId] > 0)
            return false;
        
        // Check skill points
        if (playerData.UsedSkillPoints + node.Cost > playerData.TotalSkillPoints)
            return false;
        
        // Check parent node is unlocked (unless it's a root node)
        if (!string.IsNullOrEmpty(node.ParentNodeId))
        {
            if (!playerData.UnlockedNodes.ContainsKey(node.ParentNodeId) || 
                playerData.UnlockedNodes[node.ParentNodeId] == 0)
                return false;
        }
        
        return true;
    }

    public bool ValidateData()
    {
        if (AllNodes.Count == 0 || Categories.Count == 0)
            return false;

        foreach (var node in AllNodes.Values)
        {
            if (string.IsNullOrEmpty(node.NodeId))
                return false;
            if (string.IsNullOrEmpty(node.SkillTreeCategory))
                return false;
        }

        return true;
    }

    protected override void OnExportSaveData(Godot.Collections.Dictionary saveData)
    {
        // SkillTreeDatabase 是静态配置数据库，玩家状态存储在 PlayerSkillTreeData 中
    }

    protected override void OnImportSaveData(Godot.Collections.Dictionary saveData)
    {
        // SkillTreeDatabase 是静态配置数据库，玩家状态从 PlayerSkillTreeData 恢复
    }
}
