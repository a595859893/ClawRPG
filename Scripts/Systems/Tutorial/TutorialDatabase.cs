using Godot;
using System;
using System.Collections.Generic;

public class TutorialStep
{
    public string StepId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string TargetElement { get; set; }
    public string HighlightColor { get; set; } = "#FFFF00";
    public bool RequireAction { get; set; } = false;
    public string ActionType { get; set; } = "";
    public int Duration { get; set; } = 0;
}

public class TutorialDefinition
{
    public string TutorialId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public int Priority { get; set; } = 0;
    public List<TutorialStep> Steps { get; set; } = new List<TutorialStep>();
    public string Icon { get; set; } = "";
    public bool AutoStart { get; set; } = false;
    public int RequiredLevel { get; set; } = 1;
}

public class TutorialDatabase
{
    private static TutorialDatabase _instance;
    public static TutorialDatabase Instance => _instance ??= new TutorialDatabase();

    public Dictionary<string, TutorialDefinition> Tutorials { get; private set; } = new Dictionary<string, TutorialDefinition>();

    public TutorialDatabase()
    {
        InitializeTutorials();
    }

    private void InitializeTutorials()
    {
        // Combat Basics Tutorial
        var combatTutorial = new TutorialDefinition
        {
            TutorialId = "combat_basics",
            Title = "战斗基础",
            Description = "学习游戏的基本战斗机制",
            Category = "Combat",
            Priority = 1,
            AutoStart = true,
            Icon = "⚔️",
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "combat_1", Title = "攻击敌人", Description = "点击敌人进行攻击", TargetElement = "enemy", Duration = 5 },
                new TutorialStep { StepId = "combat_2", Title = "使用技能", Description = "按技能快捷键使用技能", TargetElement = "skill_bar", Duration = 5 },
                new TutorialStep { StepId = "combat_3", Title = "躲避攻击", Description = "使用闪避技能躲避敌人攻击", TargetElement = "dodge_button", Duration = 5 },
                new TutorialStep { StepId = "combat_4", Title = "使用物品", Description = "按快捷键使用药水", TargetElement = "item_bar", Duration = 5 }
            }
        };
        Tutorials["combat_basics"] = combatTutorial;

        // Movement Tutorial
        var movementTutorial = new TutorialDefinition
        {
            TutorialId = "movement",
            Title = "移动控制",
            Description = "学习如何在游戏世界中移动",
            Category = "Basic",
            Priority = 1,
            AutoStart = true,
            Icon = "🏃",
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "move_1", Title = " WASD 移动", Description = "使用 WASD 键移动角色", TargetElement = "movement_area", Duration = 5 },
                new TutorialStep { StepId = "move_2", Title = " Shift 加速", Description = "按住 Shift 键加速移动", TargetElement = "movement_area", Duration = 3 },
                new TutorialStep { StepId = "move_3", Title = " 空格跳跃", Description = "按空格键跳跃", TargetElement = "jump_button", Duration = 3 }
            }
        };
        Tutorials["movement"] = movementTutorial;

        // Inventory Tutorial
        var inventoryTutorial = new TutorialDefinition
        {
            TutorialId = "inventory",
            Title = "背包系统",
            Description = "了解背包的使用和管理",
            Category = "System",
            Priority = 5,
            Icon = "🎒",
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "inv_1", Title = "打开背包", Description = "按 I 键打开背包", TargetElement = "inventory_button", RequireAction = true, ActionType = "press_i" },
                new TutorialStep { StepId = "inv_2", Title = " 使用物品", Description = "双击物品使用", TargetElement = "inventory_grid", Duration = 5 },
                new TutorialStep { StepId = "inv_3", Title = " 整理背包", Description = "点击整理按钮自动整理", TargetElement = "sort_button", Duration = 3 }
            }
        };
        Tutorials["inventory"] = inventoryTutorial;

        // Skill Tree Tutorial
        var skillTreeTutorial = new TutorialDefinition
        {
            TutorialId = "skill_tree",
            Title = "技能树",
            Description = "学习如何分配技能点",
            Category = "Progression",
            Priority = 10,
            Icon = "🌳",
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "skill_1", Title = "打开技能树", Description = "按 K 键打开技能树", TargetElement = "skill_tree_button", RequireAction = true, ActionType = "press_k" },
                new TutorialStep { StepId = "skill_2", Title = " 选择技能", Description = "点击技能节点查看详情", TargetElement = "skill_node", Duration = 3 },
                new TutorialStep { StepId = "skill_3", Title = " 分配点数", Description = "点击加点按钮分配技能点", TargetElement = "allocate_button", RequireAction = true, ActionType = "click" }
            }
        };
        Tutorials["skill_tree"] = skillTreeTutorial;

        // Pet System Tutorial
        var petTutorial = new TutorialDefinition
        {
            TutorialId = "pet_system",
            Title = "宠物系统",
            Description = "了解如何培养和使用宠物",
            Category = "Pets",
            Priority = 15,
            Icon = "🐾",
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "pet_1", Title = "获得宠物", Description = "完成宠物任务获得宠物蛋", TargetElement = "pet_egg", Duration = 5 },
                new TutorialStep { StepId = "pet_2", Title = " 孵化宠物", Description = "等待宠物蛋孵化", TargetElement = "hatch_button", RequireAction = true, ActionType = "click" },
                new TutorialStep { StepId = "pet_3", Title = " 宠物战斗", Description = "宠物会自动协助战斗", TargetElement = "pet_battle", Duration = 5 },
                new TutorialStep { StepId = "pet_4", Title = " 宠物升级", Description = "通过战斗让宠物获得经验", TargetElement = "pet_exp", Duration = 5 }
            }
        };
        Tutorials["pet_system"] = petTutorial;

        // Guild System Tutorial
        var guildTutorial = new TutorialDefinition
        {
            TutorialId = "guild_system",
            Title = "公会系统",
            Description = "了解公会功能和社交",
            Category = "Social",
            Priority = 20,
            Icon = "🏰",
            RequiredLevel = 10,
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "guild_1", Title = " 创建公会", Description = "花费金币创建自己的公会", TargetElement = "create_guild", Duration = 5 },
                new TutorialStep { StepId = "guild_2", Title = " 邀请好友", Description = "邀请好友加入公会", TargetElement = "invite_button", Duration = 5 },
                new TutorialStep { StepId = "guild_3", Title = " 公会任务", Description = "完成公会任务获得贡献度", TargetElement = "guild_quest", Duration = 5 },
                new TutorialStep { StepId = "guild_4", Title = " 升级公会", Description = "用贡献度升级公会设施", TargetElement = "upgrade_button", Duration = 5 }
            }
        };
        Tutorials["guild_system"] = guildTutorial;

        // Crafting Tutorial
        var craftingTutorial = new TutorialDefinition
        {
            TutorialId = "crafting",
            Title = "制作系统",
            Description = "学习如何制作装备和物品",
            Category = "Crafting",
            Priority = 8,
            Icon = "🔨",
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "craft_1", Title = " 打开工坊", Description = "按 C 键打开制作界面", TargetElement = "crafting_button", RequireAction = true, ActionType = "press_c" },
                new TutorialStep { StepId = "craft_2", Title = " 选择配方", Description = "从列表中选择要制作的物品", TargetElement = "recipe_list", Duration = 5 },
                new TutorialStep { StepId = "craft_3", Title = " 开始制作", Description = "确认材料后点击制作", TargetElement = "craft_button", RequireAction = true, ActionType = "click" }
            }
        };
        Tutorials["crafting"] = craftingTutorial;

        // Auction House Tutorial
        var auctionTutorial = new TutorialDefinition
        {
            TutorialId = "auction_house",
            Title = "拍卖行",
            Description = "了解如何买卖物品",
            Category = "Economy",
            Priority = 12,
            Icon = "💰",
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "auction_1", Title = " 浏览市场", Description = "查看当前拍卖的物品", TargetElement = "market_list", Duration = 5 },
                new TutorialStep { StepId = "auction_2", Title = " 搜索物品", Description = "使用搜索功能找到需要的物品", TargetElement = "search_box", Duration = 3 },
                new TutorialStep { StepId = "auction_3", Title = " 购买物品", Description = "点击购买获取物品", TargetElement = "buy_button", RequireAction = true, ActionType = "click" },
                new TutorialStep { StepId = "auction_4", Title = " 挂售物品", Description = "将自己的物品放到拍卖行", TargetElement = "sell_button", Duration = 5 }
            }
        };
        Tutorials["auction_house"] = auctionTutorial;

        // Mount System Tutorial
        var mountTutorial = new TutorialDefinition
        {
            TutorialId = "mount_system",
            Title = "坐骑系统",
            Description = "学习如何获得和使用坐骑",
            Category = "Pets",
            Priority = 7,
            Icon = "🐎",
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "mount_1", Title = " 获得坐骑", Description = "通过任务或购买获得坐骑", TargetElement = "mount_source", Duration = 5 },
                new TutorialStep { StepId = "mount_2", Title = " 召唤坐骑", Description = "按 M 键召唤坐骑", TargetElement = "mount_button", RequireAction = true, ActionType = "press_m" },
                new TutorialStep { StepId = "mount_3", Title = " 骑乘移动", Description = "骑乘坐骑移动更快", TargetElement = "riding_area", Duration = 5 }
            }
        };
        Tutorials["mount_system"] = mountTutorial;

        // Dungeon Tutorial
        var dungeonTutorial = new TutorialDefinition
        {
            TutorialId = "dungeon",
            Title = "地下城",
            Description = "了解地下城探索",
            Category = "Combat",
            Priority = 6,
            Icon = "🏰",
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "dungeon_1", Title = " 进入地下城", Description = "找到入口进入地下城", TargetElement = "dungeon_entrance", Duration = 5 },
                new TutorialStep { StepId = "dungeon_2", Title = " 探索房间", Description = "点击房间探索并触发事件", TargetElement = "room_node", Duration = 5 },
                new TutorialStep { StepId = "dungeon_3", Title = " 战斗获胜", Description = "击败房间中的敌人", TargetElement = "combat_area", Duration = 10 },
                new TutorialStep { StepId = "dungeon_4", Title = " 领取奖励", Description = "击败 Boss 后领取奖励", TargetElement = "reward_chest", Duration = 5 }
            }
        };
        Tutorials["dungeon"] = dungeonTutorial;

        // Multiplayer Tutorial
        var multiplayerTutorial = new TutorialDefinition
        {
            TutorialId = "multiplayer",
            Title = "多人游戏",
            Description = "学习如何与其他玩家一起游戏",
            Category = "Social",
            Priority = 18,
            Icon = "👥",
            RequiredLevel = 5,
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "multi_1", Title = " 创建房间", Description = "创建多人游戏房间", TargetElement = "create_room", Duration = 5 },
                new TutorialStep { StepId = "multi_2", Title = " 邀请好友", Description = "邀请好友加入房间", TargetElement = "invite_friend", Duration = 5 },
                new TutorialStep { StepId = "multi_3", Title = " 开始游戏", Description = "所有玩家准备后开始游戏", TargetElement = "start_button", RequireAction = true, ActionType = "click" }
            }
        };
        Tutorials["multiplayer"] = multiplayerTutorial;

        // Economy Tutorial
        var economyTutorial = new TutorialDefinition
        {
            TutorialId = "economy",
            Title = "经济系统",
            Description = "了解游戏的经济系统",
            Category = "Economy",
            Priority = 9,
            Icon = "📊",
            Steps = new List<TutorialStep>
            {
                new TutorialStep { StepId = "econ_1", Title = " 赚取金币", Description = "通过战斗、任务、出售物品赚取金币", TargetElement = "gold_source", Duration = 5 },
                new TutorialStep { StepId = "econ_2", Title = " 购买物品", Description = "在商店购买需要的物品", TargetElement = "shop_button", Duration = 5 },
                new TutorialStep { StepId = "econ_3", Title = " 管理财富", Description = "合理分配金币用于升级和购物", TargetElement = "gold_display", Duration = 5 }
            }
        };
        Tutorials["economy"] = economyTutorial;
    }

    public TutorialDefinition GetTutorial(string tutorialId)
    {
        return Tutorials.ContainsKey(tutorialId) ? Tutorials[tutorialId] : null;
    }

    public List<TutorialDefinition> GetTutorialsByCategory(string category)
    {
        List<TutorialDefinition> result = new List<TutorialDefinition>();
        foreach (var tutorial in Tutorials.Values)
        {
            if (tutorial.Category == category)
                result.Add(tutorial);
        }
        return result;
    }

    public List<TutorialDefinition> GetAvailableTutorials(int playerLevel)
    {
        List<TutorialDefinition> result = new List<TutorialDefinition>();
        foreach (var tutorial in Tutorials.Values)
        {
            if (tutorial.RequiredLevel <= playerLevel)
                result.Add(tutorial);
        }
        result.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        return result;
    }

    public string[] GetCategories()
    {
        HashSet<string> categories = new HashSet<string>();
        foreach (var tutorial in Tutorials.Values)
        {
            categories.Add(tutorial.Category);
        }
        string[] result = new string[categories.Count];
        categories.CopyTo(result);
        return result;
    }
}
