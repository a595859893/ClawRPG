using Godot;
using System;
using System.Collections.Generic;

public class DailyDungeonData
{
    public enum DungeonType
    {
        AbyssTower,      // 深渊之塔
        DragonLair,      // 龙穴
        AncientTomb,     // 远古墓穴
        DemonCastle,     // 恶魔城堡
        SacredGround     // 神圣之地
    }

    public enum Difficulty
    {
        Easy,      // 简单
        Normal,    // 普通
        Hard,      // 困难
        Epic,      // 史诗
        Legendary  // 传奇
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public DungeonType Type { get; set; }
    public Difficulty Difficulty { get; set; }
    public int RecommendedLevel { get; set; }
    public int TotalFloors { get; set; }
    public int CurrentFloor { get; set; }
    public int TimeLimit { get; set; }  // seconds
    public List<string> RewardItems { get; set; }
    public int GoldReward { get; set; }
    public int ExpReward { get; set; }
    public bool IsCompleted { get; set; }
    public int BestFloor { get; set; }

    public DailyDungeonData()
    {
        RewardItems = new List<string>();
    }
}

public class DailyDungeonDatabase
{
    private static readonly List<DailyDungeonData> _dungeons = new List<DailyDungeonData>
    {
        // Abyss Tower - 深渊之塔
        new DailyDungeonData
        {
            Id = "abyss_easy",
            Name = "深渊之塔·简单",
            Type = DungeonType.AbyssTower,
            Difficulty = Difficulty.Easy,
            RecommendedLevel = 1,
            TotalFloors = 5,
            TimeLimit = 300,
            GoldReward = 100,
            ExpReward = 200
        },
        new DailyDungeonData
        {
            Id = "abyss_normal",
            Name = "深渊之塔·普通",
            Type = DungeonType.AbyssTower,
            Difficulty = Difficulty.Normal,
            RecommendedLevel = 20,
            TotalFloors = 10,
            TimeLimit = 600,
            GoldReward = 300,
            ExpReward = 500
        },
        new DailyDungeonData
        {
            Id = "abyss_hard",
            Name = "深渊之塔·困难",
            Type = DungeonType.AbyssTower,
            Difficulty = Difficulty.Hard,
            RecommendedLevel = 40,
            TotalFloors = 15,
            TimeLimit = 900,
            GoldReward = 800,
            ExpReward = 1200
        },
        new DailyDungeonData
        {
            Id = "abyss_epic",
            Name = "深渊之塔·史诗",
            Type = DungeonType.AbyssTower,
            Difficulty = Difficulty.Epic,
            RecommendedLevel = 60,
            TotalFloors = 20,
            TimeLimit = 1200,
            GoldReward = 2000,
            ExpReward = 3000
        },
        new DailyDungeonData
        {
            Id = "abyss_legendary",
            Name = "深渊之塔·传奇",
            Type = DungeonType.AbyssTower,
            Difficulty = Difficulty.Legendary,
            RecommendedLevel = 80,
            TotalFloors = 30,
            TimeLimit = 1800,
            GoldReward = 5000,
            ExpReward = 8000
        },

        // Dragon Lair - 龙穴
        new DailyDungeonData
        {
            Id = "dragon_easy",
            Name = "龙穴·简单",
            Type = DungeonType.DragonLair,
            Difficulty = Difficulty.Easy,
            RecommendedLevel = 15,
            TotalFloors = 3,
            TimeLimit = 180,
            GoldReward = 150,
            ExpReward = 300
        },
        new DailyDungeonData
        {
            Id = "dragon_normal",
            Name = "龙穴·普通",
            Type = DungeonType.DragonLair,
            Difficulty = Difficulty.Normal,
            RecommendedLevel = 30,
            TotalFloors = 5,
            TimeLimit = 360,
            GoldReward = 400,
            ExpReward = 700
        },
        new DailyDungeonData
        {
            Id = "dragon_hard",
            Name = "龙穴·困难",
            Type = DungeonType.DragonLair,
            Difficulty = Difficulty.Hard,
            RecommendedLevel = 50,
            TotalFloors = 8,
            TimeLimit = 540,
            GoldReward = 1000,
            ExpReward = 1500
        },
        new DailyDungeonData
        {
            Id = "dragon_epic",
            Name = "龙穴·史诗",
            Type = DungeonType.DragonLair,
            Difficulty = Difficulty.Epic,
            RecommendedLevel = 70,
            TotalFloors = 10,
            TimeLimit = 720,
            GoldReward = 2500,
            ExpReward = 4000
        },
        new DailyDungeonData
        {
            Id = "dragon_legendary",
            Name = "龙穴·传奇",
            Type = DungeonType.DragonLair,
            Difficulty = Difficulty.Legendary,
            RecommendedLevel = 90,
            TotalFloors = 15,
            TimeLimit = 1080,
            GoldReward = 6000,
            ExpReward = 10000
        },

        // Ancient Tomb - 远古墓穴
        new DailyDungeonData
        {
            Id = "tomb_easy",
            Name = "远古墓穴·简单",
            Type = DungeonType.AncientTomb,
            Difficulty = Difficulty.Easy,
            RecommendedLevel = 10,
            TotalFloors = 4,
            TimeLimit = 240,
            GoldReward = 120,
            ExpReward = 250
        },
        new DailyDungeonData
        {
            Id = "tomb_normal",
            Name = "远古墓穴·普通",
            Type = DungeonType.AncientTomb,
            Difficulty = Difficulty.Normal,
            RecommendedLevel = 25,
            TotalFloors = 8,
            TimeLimit = 480,
            GoldReward = 350,
            ExpReward = 600
        },
        new DailyDungeonData
        {
            Id = "tomb_hard",
            Name = "远古墓穴·困难",
            Type = DungeonType.AncientTomb,
            Difficulty = Difficulty.Hard,
            RecommendedLevel = 45,
            TotalFloors = 12,
            TimeLimit = 720,
            GoldReward = 900,
            ExpReward = 1400
        },
        new DailyDungeonData
        {
            Id = "tomb_epic",
            Name = "远古墓穴·史诗",
            Type = DungeonType.AncientTomb,
            Difficulty = Difficulty.Epic,
            RecommendedLevel = 65,
            TotalFloors = 16,
            TimeLimit = 960,
            GoldReward = 2200,
            ExpReward = 3500
        },
        new DailyDungeonData
        {
            Id = "tomb_legendary",
            Name = "远古墓穴·传奇",
            Type = DungeonType.AncientTomb,
            Difficulty = Difficulty.Legendary,
            RecommendedLevel = 85,
            TotalFloors = 25,
            TimeLimit = 1440,
            GoldReward = 5500,
            ExpReward = 9000
        },

        // Demon Castle - 恶魔城堡
        new DailyDungeonData
        {
            Id = "demon_easy",
            Name = "恶魔城堡·简单",
            Type = DungeonType.DemonCastle,
            Difficulty = Difficulty.Easy,
            RecommendedLevel = 20,
            TotalFloors = 5,
            TimeLimit = 300,
            GoldReward = 200,
            ExpReward = 400
        },
        new DailyDungeonData
        {
            Id = "demon_normal",
            Name = "恶魔城堡·普通",
            Type = DungeonType.DemonCastle,
            Difficulty = Difficulty.Normal,
            RecommendedLevel = 35,
            TotalFloors = 10,
            TimeLimit = 600,
            GoldReward = 500,
            ExpReward = 800
        },
        new DailyDungeonData
        {
            Id = "demon_hard",
            Name = "恶魔城堡·困难",
            Type = DungeonType.DemonCastle,
            Difficulty = Difficulty.Hard,
            RecommendedLevel = 55,
            TotalFloors = 15,
            TimeLimit = 900,
            GoldReward = 1200,
            ExpReward = 1800
        },
        new DailyDungeonData
        {
            Id = "demon_epic",
            Name = "恶魔城堡·史诗",
            Type = DungeonType.DemonCastle,
            Difficulty = Difficulty.Epic,
            RecommendedLevel = 75,
            TotalFloors = 20,
            TimeLimit = 1200,
            GoldReward = 3000,
            ExpReward = 5000
        },
        new DailyDungeonData
        {
            Id = "demon_legendary",
            Name = "恶魔城堡·传奇",
            Type = DungeonType.DemonCastle,
            Difficulty = Difficulty.Legendary,
            RecommendedLevel = 95,
            TotalFloors = 30,
            TimeLimit = 1800,
            GoldReward = 8000,
            ExpReward = 12000
        },

        // Sacred Ground - 神圣之地
        new DailyDungeonData
        {
            Id = "sacred_easy",
            Name = "神圣之地·简单",
            Type = DungeonType.SacredGround,
            Difficulty = Difficulty.Easy,
            RecommendedLevel = 25,
            TotalFloors = 3,
            TimeLimit = 200,
            GoldReward = 250,
            ExpReward = 500
        },
        new DailyDungeonData
        {
            Id = "sacred_normal",
            Name = "神圣之地·普通",
            Type = DungeonType.SacredGround,
            Difficulty = Difficulty.Normal,
            RecommendedLevel = 40,
            TotalFloors = 6,
            TimeLimit = 400,
            GoldReward = 600,
            ExpReward = 1000
        },
        new DailyDungeonData
        {
            Id = "sacred_hard",
            Name = "神圣之地·困难",
            Type = DungeonType.SacredGround,
            Difficulty = Difficulty.Hard,
            RecommendedLevel = 60,
            TotalFloors = 10,
            TimeLimit = 700,
            GoldReward = 1500,
            ExpReward = 2500
        },
        new DailyDungeonData
        {
            Id = "sacred_epic",
            Name = "神圣之地·史诗",
            Type = DungeonType.SacredGround,
            Difficulty = Difficulty.Epic,
            RecommendedLevel = 80,
            TotalFloors = 15,
            TimeLimit = 1000,
            GoldReward = 4000,
            ExpReward = 6000
        },
        new DailyDungeonData
        {
            Id = "sacred_legendary",
            Name = "神圣之地·传奇",
            Type = DungeonType.SacredGround,
            Difficulty = Difficulty.Legendary,
            RecommendedLevel = 100,
            TotalFloors = 20,
            TimeLimit = 1500,
            GoldReward = 10000,
            ExpReward = 15000
        }
    };

    public static List<DailyDungeonData> GetAllDungeons()
    {
        return new List<DailyDungeonData>(_dungeons);
    }

    public static DailyDungeonData GetDungeonById(string id)
    {
        return _dungeons.Find(d => d.Id == id);
    }

    public static List<DailyDungeonData> GetDungeonsByType(DungeonType type)
    {
        return _dungeons.FindAll(d => d.Type == type);
    }

    public static List<DailyDungeonData> GetDungeonsByDifficulty(Difficulty difficulty)
    {
        return _dungeons.FindAll(d => d.Difficulty == difficulty);
    }

    public static string GetDungeonTypeName(DungeonType type)
    {
        switch (type)
        {
            case DungeonType.AbyssTower: return "深渊之塔";
            case DungeonType.DragonLair: return "龙穴";
            case DungeonType.AncientTomb: return "远古墓穴";
            case DungeonType.DemonCastle: return "恶魔城堡";
            case DungeonType.SacredGround: return "神圣之地";
            default: return "未知";
        }
    }

    public static string GetDifficultyName(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return "简单";
            case Difficulty.Normal: return "普通";
            case Difficulty.Hard: return "困难";
            case Difficulty.Epic: return "史诗";
            case Difficulty.Legendary: return "传奇";
            default: return "未知";
        }
    }

    public static Color GetDifficultyColor(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return new Color(0.2f, 0.8f, 0.2f);
            case Difficulty.Normal: return new Color(0.2f, 0.6f, 1.0f);
            case Difficulty.Hard: return new Color(1.0f, 0.6f, 0.0f);
            case Difficulty.Epic: return new Color(0.6f, 0.2f, 1.0f);
            case Difficulty.Legendary: return new Color(1.0f, 0.2f, 0.2f);
            default: return Colors.White;
        }
    }

    public static Color GetDungeonTypeColor(DungeonType type)
    {
        switch (type)
        {
            case DungeonType.AbyssTower: return new Color(0.4f, 0.0f, 0.6f);
            case DungeonType.DragonLair: return new Color(1.0f, 0.3f, 0.0f);
            case DungeonType.AncientTomb: return new Color(0.5f, 0.4f, 0.2f);
            case DungeonType.DemonCastle: return new Color(0.6f, 0.0f, 0.0f);
            case DungeonType.SacredGround: return new Color(1.0f, 0.9f, 0.4f);
            default: return Colors.White;
        }
    }
}
