using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 多人表情配置数据库
/// </summary>
public class MultiplayerEmoteDatabase
{
    private static MultiplayerEmoteDatabase _instance;
    public static MultiplayerEmoteDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MultiplayerEmoteDatabase();
            return _instance;
        }
    }

    // 表情配置映射
    public Dictionary<EmoteType, EmoteConfig> EmoteConfigs { get; private set; }

    // 分类表情映射
    public Dictionary<EmoteCategory, List<EmoteType>> CategoryEmotes { get; private set; }

    private MultiplayerEmoteDatabase()
    {
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        EmoteConfigs = new Dictionary<EmoteType, EmoteConfig>();
        CategoryEmotes = new Dictionary<EmoteCategory, List<EmoteType>>();

        // 初始化所有表情配置
        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Wave,
            Name = "挥手",
            Description = "向其他玩家挥手致意",
            Category = EmoteCategory.Social,
            IconPath = "res://icons/emote_wave.png",
            AnimationName = "emote_wave",
            Duration = 2.0f,
            UnlockLevel = 1
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Laugh,
            Name = "大笑",
            Description = "开怀大笑",
            Category = EmoteCategory.Emotion,
            IconPath = "res://icons/emote_laugh.png",
            AnimationName = "emote_laugh",
            Duration = 2.5f,
            UnlockLevel = 1
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Cry,
            Name = "哭泣",
            Description = "伤心落泪",
            Category = EmoteCategory.Emotion,
            IconPath = "res://icons/emote_cry.png",
            AnimationName = "emote_cry",
            Duration = 3.0f,
            UnlockLevel = 3
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Dance,
            Name = "跳舞",
            Description = "欢快地跳舞",
            Category = EmoteCategory.Celebration,
            IconPath = "res://icons/emote_dance.png",
            AnimationName = "emote_dance",
            Duration = 4.0f,
            UnlockLevel = 5
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Clap,
            Name = "鼓掌",
            Description = "为他人鼓掌",
            Category = EmoteCategory.Social,
            IconPath = "res://icons/emote_clap.png",
            AnimationName = "emote_clap",
            Duration = 2.0f,
            UnlockLevel = 1
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.ThumbsUp,
            Name = "点赞",
            Description = "给出一个赞",
            Category = EmoteCategory.Social,
            IconPath = "res://icons/emote_thumbsup.png",
            AnimationName = "emote_thumbsup",
            Duration = 1.5f,
            UnlockLevel = 1
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Shrug,
            Name = "耸肩",
            Description = "表示无奈",
            Category = EmoteCategory.Emotion,
            IconPath = "res://icons/emote_shrug.png",
            AnimationName = "emote_shrug",
            Duration = 2.0f,
            UnlockLevel = 2
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Point,
            Name = "指向",
            Description = "指向某个方向",
            Category = EmoteCategory.Action,
            IconPath = "res://icons/emote_point.png",
            AnimationName = "emote_point",
            Duration = 1.5f,
            UnlockLevel = 1
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Bow,
            Name = "鞠躬",
            Description = "恭敬地鞠躬",
            Category = EmoteCategory.Social,
            IconPath = "res://icons/emote_bow.png",
            AnimationName = "emote_bow",
            Duration = 2.5f,
            UnlockLevel = 3
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Cheer,
            Name = "欢呼",
            Description = "兴奋地欢呼",
            Category = EmoteCategory.Celebration,
            IconPath = "res://icons/emote_cheer.png",
            AnimationName = "emote_cheer",
            Duration = 3.0f,
            UnlockLevel = 2
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Angry,
            Name = "生气",
            Description = "表达愤怒",
            Category = EmoteCategory.Emotion,
            IconPath = "res://icons/emote_angry.png",
            AnimationName = "emote_angry",
            Duration = 2.0f,
            UnlockLevel = 4
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Love,
            Name = "爱心",
            Description = "发送爱心",
            Category = EmoteCategory.Emotion,
            IconPath = "res://icons/emote_love.png",
            AnimationName = "emote_love",
            Duration = 2.0f,
            UnlockLevel = 2
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Think,
            Name = "思考",
            Description = "陷入沉思",
            Category = EmoteCategory.Emotion,
            IconPath = "res://icons/emote_think.png",
            AnimationName = "emote_think",
            Duration = 3.0f,
            UnlockLevel = 3
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Sleep,
            Name = "睡觉",
            Description = "打盹",
            Category = EmoteCategory.Emotion,
            IconPath = "res://icons/emote_sleep.png",
            AnimationName = "emote_sleep",
            Duration = 4.0f,
            UnlockLevel = 5
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Attack,
            Name = "攻击姿势",
            Description = "摆出攻击姿势",
            Category = EmoteCategory.Combat,
            IconPath = "res://icons/emote_attack.png",
            AnimationName = "emote_attack",
            Duration = 2.0f,
            UnlockLevel = 3
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Defend,
            Name = "防御姿势",
            Description = "摆出防御姿势",
            Category = EmoteCategory.Combat,
            IconPath = "res://icons/emote_defend.png",
            AnimationName = "emote_defend",
            Duration = 2.5f,
            UnlockLevel = 3
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Celebrate,
            Name = "庆祝",
            Description = "庆祝胜利",
            Category = EmoteCategory.Celebration,
            IconPath = "res://icons/emote_celebrate.png",
            AnimationName = "emote_celebrate",
            Duration = 3.5f,
            UnlockLevel = 4
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Sad,
            Name = "悲伤",
            Description = "表达悲伤",
            Category = EmoteCategory.Emotion,
            IconPath = "res://icons/emote_sad.png",
            AnimationName = "emote_sad",
            Duration = 2.5f,
            UnlockLevel = 4
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Surprise,
            Name = "惊讶",
            Description = "表示惊讶",
            Category = EmoteCategory.Emotion,
            IconPath = "res://icons/emote_surprise.png",
            AnimationName = "emote_surprise",
            Duration = 2.0f,
            UnlockLevel = 2
        });

        AddEmote(new EmoteConfig
        {
            Type = EmoteType.Welcome,
            Name = "欢迎",
            Description = "热情欢迎",
            Category = EmoteCategory.Social,
            IconPath = "res://icons/emote_welcome.png",
            AnimationName = "emote_welcome",
            Duration = 2.5f,
            UnlockLevel = 1
        });
    }

    private void AddEmote(EmoteConfig config)
    {
        EmoteConfigs[config.Type] = config;

        if (!CategoryEmotes.ContainsKey(config.Category))
        {
            CategoryEmotes[config.Category] = new List<EmoteType>();
        }
        CategoryEmotes[config.Category].Add(config.Type);
    }

    /// <summary>
    /// 获取表情配置
    /// </summary>
    public EmoteConfig GetEmoteConfig(EmoteType type)
    {
        return EmoteConfigs.ContainsKey(type) ? EmoteConfigs[type] : null;
    }

    /// <summary>
    /// 获取分类下的所有表情
    /// </summary>
    public List<EmoteType> GetEmotesByCategory(EmoteCategory category)
    {
        return CategoryEmotes.ContainsKey(category) ? new List<EmoteType>(CategoryEmotes[category]) : new List<EmoteType>();
    }

    /// <summary>
    /// 根据等级获取可解锁的表情
    /// </summary>
    public List<EmoteType> GetUnlockedEmotesByLevel(int level)
    {
        List<EmoteType> result = new List<EmoteType>();
        foreach (var kvp in EmoteConfigs)
        {
            if (kvp.Value.UnlockLevel <= level)
            {
                result.Add(kvp.Key);
            }
        }
        return result;
    }

    /// <summary>
    /// 获取所有表情
    /// </summary>
    public List<EmoteType> GetAllEmotes()
    {
        return new List<EmoteType>(EmoteConfigs.Keys);
    }
}
