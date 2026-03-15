using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 公会节日系统 - 管理公会节日活动和庆典
/// </summary>
public class GuildFestivalSystem : Node
{
    // 节日类型
    public enum FestivalType
    {
        SpringFestival,    // 春节庆典
        HarvestFestival,   // 丰收祭
        MidSummerFestival, // 仲夏节
        MoonFestival,      // 中秋节
        WinterSolstice,    // 冬至节
        Anniversary,       // 周年庆
        HeroCommemoration, // 英雄纪念日
        TradeFair,        // 贸易博览会
        DragonRacing,     // 龙舟赛
        KnightTournament  // 骑士锦标赛
    }

    // 节日状态
    public enum FestivalState
    {
        Inactive,
        Preparation,
        Active,
        Completed
    }

    // 活动类型
    public enum ActivityType
    {
        MiniGame,      // 小游戏
        Competition,   // 竞赛
        Quest,         // 任务
        Collection,    // 收集
        Crafting,      // 制作
        Social         // 社交
    }

    private Dictionary<int, FestivalData> _festivals = new Dictionary<int, FestivalData>();
    private int _nextFestivalId = 1;
    private int _currentFestivalId = -1;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        LoadFestivals();
        CheckAutoStart();
    }

    private void LoadFestivals()
    {
        // 初始化预设节日
        var presetFestivals = new[]
        {
            new FestivalData { Id = _nextFestivalId++, Type = FestivalType.SpringFestival, Name = "春节庆典", Description = "欢度新春，开启一年好运", MinGuildLevel = 1, Duration = 300, PreparationTime = 120, BonusGold = 1.5, BonusExp = 1.3, RewardPoints = 100 },
            new FestivalData { Id = _nextFestivalId++, Type = FestivalType.HarvestFestival, Name = "丰收祭", Description = "庆祝丰收，感谢大地馈赠", MinGuildLevel = 2, Duration = 360, PreparationTime = 180, BonusGold = 1.8, BonusExp = 1.2, RewardPoints = 120 },
            new FestivalData { Id = _nextFestivalId++, Type = FestivalType.MidSummerFestival, Name = "仲夏节", Description = "夏日狂欢，热情似火", MinGuildLevel = 2, Duration = 300, PreparationTime = 120, BonusGold = 1.4, BonusExp = 1.5, RewardPoints = 110 },
            new FestivalData { Id = _nextFestivalId++, Type = FestivalType.MoonFestival, Name = "中秋节", Description = "月圆之夜，团圆之时", MinGuildLevel = 3, Duration = 420, PreparationTime = 180, BonusGold = 1.6, BonusExp = 1.4, RewardPoints = 130 },
            new FestivalData { Id = _nextFestivalId++, Type = FestivalType.WinterSolstice, Name = "冬至节", Description = "寒冬暖意，温馨相聚", MinGuildLevel = 3, Duration = 360, PreparationTime = 150, BonusGold = 1.5, BonusExp = 1.6, RewardPoints = 125 },
            new FestivalData { Id = _nextFestivalId++, Type = FestivalType.Anniversary, Name = "周年庆", Description = "庆祝公会成立周年", MinGuildLevel = 5, Duration = 480, PreparationTime = 240, BonusGold = 2.0, BonusExp = 2.0, RewardPoints = 200 },
            new FestivalData { Id = _nextFestivalId++, Type = FestivalType.HeroCommemoration, Name = "英雄纪念日", Description = "纪念公会英雄的牺牲与荣耀", MinGuildLevel = 4, Duration = 360, PreparationTime = 180, BonusGold = 1.7, BonusExp = 1.8, RewardPoints = 150 },
            new FestivalData { Id = _nextFestivalId++, Type = FestivalType.TradeFair, Name = "贸易博览会", Description = "商人云集，交易盛会", MinGuildLevel = 3, Duration = 420, PreparationTime = 180, BonusGold = 2.5, BonusExp = 1.1, RewardPoints = 100 },
            new FestivalData { Id = _nextFestivalId++, Type = FestivalType.DragonRacing, Name = "龙舟赛", Description = "划船竞速，团队协作", MinGuildLevel = 2, Duration = 300, PreparationTime = 120, BonusGold = 1.3, BonusExp = 1.4, RewardPoints = 115 },
            new FestivalData { Id = _nextFestivalId++, Type = FestivalType.KnightTournament, Name = "骑士锦标赛", Description = "骑士荣耀之战", MinGuildLevel = 4, Duration = 360, PreparationTime = 180, BonusGold = 1.4, BonusExp = 1.9, RewardPoints = 140 }
        };

        foreach (var festival in presetFestivals)
        {
            _festivals[festival.Id] = festival;
        }
    }

    private void CheckAutoStart()
    {
        // 检查是否需要自动开始节日
        var now = OS.GetUnixTime();
        // 简单实现：随机开始一个节日
        if (_rng.Randf() < 0.05) // 5%概率
        {
            var activeFestivals = GetActiveFestivals();
            if (activeFestivals.Count == 0)
            {
                // 随机选择一个节日开始
                var keys = new List<int>(_festivals.Keys);
                if (keys.Count > 0)
                {
                    var randomKey = keys[_rng.Randi() % keys.Count];
                    var festival = _festivals[randomKey];
                    if (festival.State == FestivalState.Inactive)
                    {
                        StartFestival(randomKey);
                    }
                }
            }
        }
    }

    public List<FestivalData> GetActiveFestivals()
    {
        var active = new List<FestivalData>();
        foreach (var f in _festivals.Values)
        {
            if (f.State == FestivalState.Active)
                active.Add(f);
        }
        return active;
    }

    public void StartFestival(int festivalId)
    {
        if (!_festivals.ContainsKey(festivalId))
            return;

        var festival = _festivals[festivalId];
        festival.State = FestivalState.Preparation;
        festival.StartTime = OS.GetUnixTime();
        festival.PreparationEndTime = festival.StartTime + festival.PreparationTime;
        
        GD.Print($"[GuildFestival] Festival {festival.Name} is now in preparation!");
    }

    public void ActivateFestival(int festivalId)
    {
        if (!_festivals.ContainsKey(festivalId))
            return;

        var festival = _festivals[festivalId];
        festival.State = FestivalState.Active;
        festival.ActiveStartTime = OS.GetUnixTime();
        
        GD.Print($"[GuildFestival] Festival {festival.Name} is now active!");
    }

    public void CompleteFestival(int festivalId)
    {
        if (!_festivals.ContainsKey(festivalId))
            return;

        var festival = _festivals[festivalId];
        festival.State = FestivalState.Completed;
        
        GD.Print($"[GuildFestival] Festival {festival.Name} has completed!");
    }

    public FestivalData GetFestival(int festivalId)
    {
        return _festivals.ContainsKey(festivalId) ? _festivals[festivalId] : null;
    }

    public Dictionary<int, FestivalData> GetAllFestivals() => new Dictionary<int, FestivalData>(_festivals);

    public bool IsFestivalActive()
    {
        foreach (var f in _festivals.Values)
        {
            if (f.State == FestivalState.Active)
                return true;
        }
        return false;
    }

    public float GetCurrentBonusGold()
    {
        foreach (var f in _festivals.Values)
        {
            if (f.State == FestivalState.Active)
                return f.BonusGold;
        }
        return 1.0f;
    }

    public float GetCurrentBonusExp()
    {
        foreach (var f in _festivals.Values)
        {
            if (f.State == FestivalState.Active)
                return f.BonusExp;
        }
        return 1.0f;
    }

    public override void _Process(float delta)
    {
        // 检查节日状态转换
        long now = OS.GetUnixTime();
        
        foreach (var f in _festivals.Values)
        {
            if (f.State == FestivalState.Preparation && now >= f.PreparationEndTime)
            {
                ActivateFestival(f.Id);
            }
            else if (f.State == FestivalState.Active && f.ActiveStartTime > 0)
            {
                long elapsed = now - f.ActiveStartTime;
                if (elapsed >= f.Duration)
                {
                    CompleteFestival(f.Id);
                }
            }
        }
    }
}

public class FestivalData
{
    public int Id { get; set; }
    public GuildFestivalSystem.FestivalType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int MinGuildLevel { get; set; }
    public int Duration { get; set; } = 300; // 5分钟
    public int PreparationTime { get; set; } = 120; // 2分钟
    public float BonusGold { get; set; } = 1.5f;
    public float BonusExp { get; set; } = 1.3f;
    public int RewardPoints { get; set; } = 100;
    
    public GuildFestivalSystem.FestivalState State { get; set; } = GuildFestivalSystem.FestivalState.Inactive;
    public long StartTime { get; set; }
    public long PreparationEndTime { get; set; }
    public long ActiveStartTime { get; set; }
    
    // 参与统计
    public int ParticipantCount { get; set; }
    public int ActivityCompletions { get; set; }
}
