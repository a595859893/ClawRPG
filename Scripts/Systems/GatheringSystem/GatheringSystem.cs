using Godot;
using Godot.Collections;
using System;
using System.Text.Json;
using Array = System.Array;

public partial class GatheringSystem : BaseSystem
{
    public static GatheringSystem Instance;
    
    private System.Collections.Generic.Dictionary<string, GatheringTool> tools = new System.Collections.Generic.Dictionary<string, GatheringTool>();
    private System.Collections.Generic.Dictionary<string, int> gatheringStats = new System.Collections.Generic.Dictionary<string, int>
    {
        { "total_gathers", 0 },
        { "herbs_gathered", 0 },
        { "ores_gathered", 0 },
        { "wood_gathered", 0 },
        { "fish_caught", 0 },
        { "insects_caught", 0 },
        { "crystals_gathered", 0 },
        { "mushrooms_gathered", 0 },
        { "fruits_gathered", 0 },
        { "total_resources", 0 },
        { "rare_finds", 0 },
        { "epic_finds", 0 },
        { "legendary_finds", 0 }
    };
    
    private Player player;
    
    public class GatheringTool
    {
        public string toolId;
        public string toolName;
        public string toolType;
        public int levelRequired;
        public int durability;
        public float efficiency;
        
        public GatheringTool(string id, string name, string type, int level, int dur, float eff)
        {
            toolId = id;
            toolName = name;
            toolType = type;
            levelRequired = level;
            durability = dur;
            efficiency = eff;
        }
    }
    
    public override void _Ready()
    {
        Instance = this;
        InitializeTools();
    }
    
    private void InitializeTools()
    {
        // Basic tools
        tools["wooden_sickle"] = new GatheringTool("wooden_sickle", "Wooden Sickle", "sickle", 1, 50, 1.0f);
        tools["wooden_axe"] = new GatheringTool("wooden_axe", "Wooden Axe", "axe", 1, 50, 1.0f);
        tools["wooden_pickaxe"] = new GatheringTool("wooden_pickaxe", "Wooden Pickaxe", "pickaxe", 1, 50, 1.0f);
        tools["wooden_fishing_rod"] = new GatheringTool("wooden_fishing_rod", "Wooden Fishing Rod", "fishing_rod", 1, 50, 1.0f);
        tools["wooden_net"] = new GatheringTool("wooden_net", "Wooden Net", "net", 1, 50, 1.0f);
        
        // Intermediate tools
        tools["iron_sickle"] = new GatheringTool("iron_sickle", "Iron Sickle", "sickle", 15, 100, 1.3f);
        tools["iron_axe"] = new GatheringTool("iron_axe", "Iron Axe", "axe", 15, 100, 1.3f);
        tools["iron_pickaxe"] = new GatheringTool("iron_pickaxe", "Iron Pickaxe", "pickaxe", 15, 100, 1.3f);
        tools["iron_fishing_rod"] = new GatheringTool("iron_fishing_rod", "Iron Fishing Rod", "fishing_rod", 15, 100, 1.3f);
        tools["iron_net"] = new GatheringTool("iron_net", "Iron Net", "net", 15, 100, 1.3f);
        
        // Advanced tools
        tools["golden_sickle"] = new GatheringTool("golden_sickle", "Golden Sickle", "sickle", 30, 200, 1.6f);
        tools["golden_axe"] = new GatheringTool("golden_axe", "Golden Axe", "axe", 30, 200, 1.6f);
        tools["golden_pickaxe"] = new GatheringTool("golden_pickaxe", "Golden Pickaxe", "pickaxe", 30, 200, 1.6f);
        tools["golden_fishing_rod"] = new GatheringTool("golden_fishing_rod", "Golden Fishing Rod", "fishing_rod", 30, 200, 1.6f);
        tools["golden_net"] = new GatheringTool("golden_net", "Golden Net", "net", 30, 200, 1.6f);
        
        // Legendary tools
        tools["diamond_sickle"] = new GatheringTool("diamond_sickle", "Diamond Sickle", "sickle", 50, 500, 2.0f);
        tools["diamond_axe"] = new GatheringTool("diamond_axe", "Diamond Axe", "axe", 50, 500, 2.0f);
        tools["diamond_pickaxe"] = new GatheringTool("diamond_pickaxe", "Diamond Pickaxe", "pickaxe", 50, 500, 2.0f);
        tools["diamond_fishing_rod"] = new GatheringTool("diamond_fishing_rod", "Diamond Fishing Rod", "fishing_rod", 50, 500, 2.0f);
        tools["diamond_net"] = new GatheringTool("diamond_net", "Diamond Net", "net", 50, 500, 2.0f);
    }
    
    public void SetPlayer(Player p)
    {
        player = p;
    }
    
    public Dictionary GetTool(string toolId)
    {
        if (tools.ContainsKey(toolId))
        {
            var tool = tools[toolId];
            return new Dictionary
            {
                { "tool_id", tool.toolId },
                { "tool_name", tool.toolName },
                { "tool_type", tool.toolType },
                { "level_required", tool.levelRequired },
                { "durability", tool.durability },
                { "efficiency", tool.efficiency }
            };
        }
        return new System.Collections.Generic.Dictionary<string, object>();
    }
    
    public Array<Dictionary> GetAllTools()
    {
        Array<Dictionary> result = new Array<Dictionary>();
        foreach (var tool in tools.Values)
        {
            result.Add(new Dictionary
            {
                { "tool_id", tool.toolId },
                { "tool_name", tool.toolName },
                { "tool_type", tool.toolType },
                { "level_required", tool.levelRequired },
                { "durability", tool.durability },
                { "efficiency", tool.efficiency }
            });
        }
        return result;
    }
    
    public Array<Dictionary> GetToolsByType(string toolType)
    {
        Array<Dictionary> result = new Array<Dictionary>();
        foreach (var tool in tools.Values)
        {
            if (tool.toolType == toolType)
            {
                result.Add(new Dictionary
                {
                    { "tool_id", tool.toolId },
                    { "tool_name", tool.toolName },
                    { "level_required", tool.levelRequired },
                    { "durability", tool.durability },
                    { "efficiency", tool.efficiency }
                });
            }
        }
        return result;
    }
    
    public void RecordGathering(string resourceType, string rarity)
    {
        gatheringStats["total_gathers"] = gatheringStats.GetValueOrDefault("total_gathers", 0) + 1;
        
        switch (resourceType.ToLower())
        {
            case "herb":
                gatheringStats["herbs_gathered"] = gatheringStats.GetValueOrDefault("herbs_gathered", 0) + 1;
                gatheringStats["total_resources"] = gatheringStats.GetValueOrDefault("total_resources", 0) + 1;
                break;
            case "ore":
                gatheringStats["ores_gathered"] = gatheringStats.GetValueOrDefault("ores_gathered", 0) + 1;
                gatheringStats["total_resources"] = gatheringStats.GetValueOrDefault("total_resources", 0) + 1;
                break;
            case "wood":
                gatheringStats["wood_gathered"] = gatheringStats.GetValueOrDefault("wood_gathered", 0) + 1;
                gatheringStats["total_resources"] = gatheringStats.GetValueOrDefault("total_resources", 0) + 1;
                break;
            case "fish":
                gatheringStats["fish_caught"] = gatheringStats.GetValueOrDefault("fish_caught", 0) + 1;
                gatheringStats["total_resources"] = gatheringStats.GetValueOrDefault("total_resources", 0) + 1;
                break;
            case "insect":
                gatheringStats["insects_caught"] = gatheringStats.GetValueOrDefault("insects_caught", 0) + 1;
                gatheringStats["total_resources"] = gatheringStats.GetValueOrDefault("total_resources", 0) + 1;
                break;
            case "crystal":
                gatheringStats["crystals_gathered"] = gatheringStats.GetValueOrDefault("crystals_gathered", 0) + 1;
                gatheringStats["total_resources"] = gatheringStats.GetValueOrDefault("total_resources", 0) + 1;
                break;
            case "mushroom":
                gatheringStats["mushrooms_gathered"] = gatheringStats.GetValueOrDefault("mushrooms_gathered", 0) + 1;
                gatheringStats["total_resources"] = gatheringStats.GetValueOrDefault("total_resources", 0) + 1;
                break;
            case "fruit":
                gatheringStats["fruits_gathered"] = gatheringStats.GetValueOrDefault("fruits_gathered", 0) + 1;
                gatheringStats["total_resources"] = gatheringStats.GetValueOrDefault("total_resources", 0) + 1;
                break;
        }
        
        if (rarity == "rare") gatheringStats["rare_finds"] = gatheringStats.GetValueOrDefault("rare_finds", 0) + 1;
        if (rarity == "epic") gatheringStats["epic_finds"] = gatheringStats.GetValueOrDefault("epic_finds", 0) + 1;
        if (rarity == "legendary") gatheringStats["legendary_finds"] = gatheringStats.GetValueOrDefault("legendary_finds", 0) + 1;
    }
    
    public Dictionary GetGatheringStats()
    {
        return new Dictionary(gatheringStats);
    }
    
    public void SaveGatheringData(Dictionary data)
    {
        if (data.ContainsKey("gathering_stats"))
        {
            gatheringStats = new System.Collections.Generic.Dictionary<string, int>((Dictionary)data["gathering_stats"]);
        }
    }
    
    public Dictionary GetSaveData()
    {
        return new Dictionary
        {
            { "gathering_stats", gatheringStats }
        };
    }
    
    public void LoadSaveData(Dictionary data)
    {
        if (data.ContainsKey("gathering_stats"))
        {
            gatheringStats = new System.Collections.Generic.Dictionary<string, int>((Dictionary)data["gathering_stats"]);
        }
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
    {
        var data = new System.Collections.Generic.Dictionary<string, object>();
        
        // 采集统计
        foreach (var kvp in gatheringStats)
        {
            data[kvp.Key] = kvp.Value;
        }
        
        // 工具数据
        var toolsData = new Godot.Collections.Array();
        foreach (var kvp in tools)
        {
            toolsData.Add(new System.Collections.Generic.System.Collections.Generic.Dictionary<string, object>
            {
                { "tool_id", kvp.Value.toolId },
                { "tool_name", kvp.Value.toolName },
                { "tool_type", kvp.Value.toolType },
                { "level_required", kvp.Value.levelRequired },
                { "durability", kvp.Value.durability },
                { "efficiency", kvp.Value.efficiency }
            });
        }
        data["tools"] = toolsData;
        
        return data;
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 采集统计
        foreach (var key in gatheringStats.Keys)
        {
            if (data.Contains(key))
            {
                gatheringStats[key] = (int)data[key];
            }
        }
        
        // 工具数据
        if (data.ContainsKey("tools"))
        {
            var toolsData = (Array)data["tools"];
            foreach (Dictionary toolData in toolsData)
            {
                string toolId = (string)toolData["tool_id"];
                if (tools.ContainsKey(toolId))
                {
                    tools[toolId].durability = (int)toolData["durability"];
                    tools[toolId].efficiency = (float)toolData["efficiency"];
                }
            }
        }
    }
}
