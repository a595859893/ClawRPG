using Godot;
using Godot.Collections;
using System;

public partial class GatheringNode : BaseSystem2D
{
    [Export] public string nodeId = "";
    [Export] public ResourceType resourceType = ResourceType.Herb;
    [Export] public int requiredLevel = 1;
    [Export] public float respawnTime = 60f;
    
    private bool isDepleted = false;
    private float timer = 0f;
    private Sprite2D resourceSprite;
    private Area2D interactionArea;
    private Label levelLabel;
    
    public enum ResourceType
    {
        Herb,
        Ore,
        Wood,
        Fish,
        Insect,
        Crystal,
        Mushroom,
        Fruit
    }
    
    public override void _Ready()
    {
        base._Ready();
        
        // Create visual representation
        resourceSprite = new Sprite2D();
        AddChild(resourceSprite);
        
        // Create interaction area
        interactionArea = new Area2D();
        AddChild(interactionArea);
        
        var collision = new CollisionShape2D();
        var circle = new CircleShape2D();
        circle.Radius = 32;
        collision.Shape = circle;
        interactionArea.AddChild(collision);
        
        // Level label
        levelLabel = new Label();
        levelLabel.Position = new Vector2(-10, -40);
        AddChild(levelLabel);
        
        UpdateVisuals();
    }
    
    public override void _Process(double delta)
    {
        if (isDepleted)
        {
            timer += (float)delta;
            if (timer >= respawnTime)
            {
                isDepleted = false;
                timer = 0;
                UpdateVisuals();
            }
        }
    }
    
    private void UpdateVisuals()
    {
        if (isDepleted)
        {
            Modulate = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            levelLabel.Text = "Depleted";
        }
        else
        {
            Modulate = new Color(1, 1, 1, 1);
            levelLabel.Text = $"Lv.{requiredLevel}";
        }
    }
    
    public bool CanGather(int playerLevel)
    {
        return !isDepleted && playerLevel >= requiredLevel;
    }
    
    public Array<Dictionary> Gather(int playerLevel, string toolType)
    {
        if (!CanGather(playerLevel))
        {
            return new Array<Dictionary>();
        }
        
        isDepleted = true;
        timer = 0;
        UpdateVisuals();
        
        // Calculate rewards based on tool and level
        float bonus = 1.0f;
        if (IsCorrectTool(toolType))
        {
            bonus = 1.5f;
        }
        
        int baseAmount = GetBaseAmount();
        int amount = (int)(baseAmount * bonus * (1 + playerLevel * 0.1f));
        
        Array<Dictionary> rewards = new Array<Dictionary>();
        rewards.Add(new Dictionary {
            { "type", "item" },
            { "id", GetResourceItemId() },
            { "amount", amount },
            { "rarity", GetRarity() }
        });
        
        return rewards;
    }
    
    private bool IsCorrectTool(string toolType)
    {
        return toolType switch
        {
            "sickle" => resourceType == ResourceType.Herb || resourceType == ResourceType.Mushroom || resourceType == ResourceType.Fruit,
            "pickaxe" => resourceType == ResourceType.Ore || resourceType == ResourceType.Crystal,
            "axe" => resourceType == ResourceType.Wood,
            "fishing_rod" => resourceType == ResourceType.Fish,
            "net" => resourceType == ResourceType.Insect,
            _ => false
        };
    }
    
    private int GetBaseAmount()
    {
        return resourceType switch
        {
            ResourceType.Herb => 3,
            ResourceType.Ore => 2,
            ResourceType.Wood => 4,
            ResourceType.Fish => 2,
            ResourceType.Insect => 3,
            ResourceType.Crystal => 1,
            ResourceType.Mushroom => 3,
            ResourceType.Fruit => 4,
            _ => 1
        };
    }
    
    private string GetResourceItemId()
    {
        return resourceType switch
        {
            ResourceType.Herb => "herb_green",
            ResourceType.Ore => "ore_iron",
            ResourceType.Wood => "wood_oak",
            ResourceType.Fish => "fish_common",
            ResourceType.Insect => "insect_beetle",
            ResourceType.Crystal => "crystal_amethyst",
            ResourceType.Mushroom => "mushroom_red",
            ResourceType.Fruit => "fruit_apple",
            _ => "resource_basic"
        };
    }
    
    private string GetRarity()
    {
        var random = new Random();
        int roll = random.Next(100);
        
        if (roll < 60) return "common";
        if (roll < 85) return "uncommon";
        if (roll < 95) return "rare";
        if (roll < 99) return "epic";
        return "legendary";
    }
    
    public Dictionary GetGatheringNodeData()
    {
        return new Dictionary
        {
            { "node_id", nodeId },
            { "resource_type", (int)resourceType },
            { "required_level", requiredLevel },
            { "respawn_time", respawnTime },
            { "is_depleted", isDepleted },
            { "position", new Vector2(X, Y) }
        };
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "node_id", nodeId },
            { "resource_type", (int)resourceType },
            { "required_level", requiredLevel },
            { "respawn_time", respawnTime },
            { "is_depleted", isDepleted },
            { "timer", timer }
        };
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("node_id"))
            nodeId = (string)data["node_id"];
        
        if (data.Contains("resource_type"))
            resourceType = (ResourceType)(int)data["resource_type"];
        
        if (data.Contains("required_level"))
            requiredLevel = (int)data["required_level"];
        
        if (data.Contains("respawn_time"))
            respawnTime = (float)data["respawn_time"];
        
        if (data.Contains("is_depleted"))
            isDepleted = (bool)data["is_depleted"];
        
        if (data.Contains("timer"))
            timer = (float)data["timer"];
        
        UpdateVisuals();
    }
}
