using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 多人表情数据
/// </summary>
namespace MultiplayerEmoteSystem
{
    /// <summary>
    /// 表情类型
    /// </summary>
    public enum EmoteType
    {
        Wave,           // 挥手
        Laugh,          // 大笑
        Cry,            // 哭泣
        Dance,          // 跳舞
        Clap,           // 鼓掌
        ThumbsUp,       // 点赞
        Shrug,          // 耸肩
        Point,          // 指向
        Bow,            // 鞠躬
        Cheer,          // 欢呼
        Angry,          // 生气
        Love,           // 爱心
        Think,          // 思考
        Sleep,          // 睡觉
        Attack,         // 攻击姿势
        Defend,         // 防御姿势
        Celebrate,      // 庆祝
        Sad,            // 悲伤
        Surprise,       // 惊讶
        Welcome         // 欢迎
    }

    /// <summary>
    /// 表情分类
    /// </summary>
    public enum EmoteCategory
    {
        Social,         // 社交
        Emotion,         // 情感
        Action,         // 动作
        Combat,         // 战斗
        Celebration     // 庆祝
    }

    /// <summary>
    /// 单个表情配置
    /// </summary>
    [Serializable]
    public class EmoteConfig
    {
        public EmoteType Type;
        public string Name;
        public string Description;
        public EmoteCategory Category;
        public string IconPath;
        public string AnimationName;
        public float Duration = 2.0f;
        public bool IsUnlocked = true;
        public int UnlockLevel = 1;
    }

    /// <summary>
    /// 玩家表情数据
    /// </summary>
    [Serializable]
    public class PlayerEmoteData
    {
        public int PlayerId;
        public List<EmoteType> UnlockedEmotes = new List<EmoteType>();
        public Dictionary<EmoteType, int> EmoteUsageCount = new Dictionary<EmoteType, int>();
        public EmoteType? LastEmote = null;
        public float LastEmoteTime = 0;
    }

    /// <summary>
    /// 表情使用记录
    /// </summary>
    [Serializable]
    public class EmoteRecord
    {
        public int PlayerId;
        public string PlayerName;
        public EmoteType Emote;
        public Vector2 Position;
        public float Timestamp;
    }

    /// <summary>
    /// 表情统计数据
    /// </summary>
    [Serializable]
    public class EmoteStatistics
    {
        public int TotalEmotesUsed;
        public Dictionary<EmoteCategory, int> CategoryUsage = new Dictionary<EmoteCategory, int>();
        public Dictionary<EmoteType, int> EmoteUsage = new Dictionary<EmoteType, int>();
        public EmoteType MostUsedEmote;
        public int MaxComboEmotes;
        public int CurrentCombo;
    }
}
