using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Data.FateWeaving
{
    /// <summary>
    /// 命运路径类型枚举
    /// </summary>
    public enum FatePathType
    {
        Hero,
        AntiHero,
        Villain,
        Mercenary,
        Legend,
        Myth,
        Chaos,
        Order,
        Shadow,
        Light
    }

    /// <summary>
    /// 选择类型枚举
    /// </summary>
    public enum FateChoiceType
    {
        Moral,
        Combat,
        Social,
        Economic,
        Exploration,
        Mystery
    }

    /// <summary>
    /// 命运路径数据 - 包含路径的所有配置信息
    /// </summary>
    [Serializable]
    public class FatePathData
    {
        public FatePathType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, float> PathBonuses { get; set; }
        public List<string> ExclusiveChoices { get; set; }
        public int UnlockTier { get; set; }

        public FatePathData()
        {
            PathBonuses = new Dictionary<string, float>();
            ExclusiveChoices = new List<string>();
        }
    }

    /// <summary>
    /// 命运选择数据 - 包含选择的所有配置信息
    /// </summary>
    [Serializable]
    public class FateChoice
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public FateChoiceType ChoiceType { get; set; }
        public Dictionary<FatePathType, float> PathInfluence { get; set; }
        public Dictionary<string, float> StatBonuses { get; set; }
        public string ConsequenceDescription { get; set; }
        public bool IsSecret { get; set; }
        public int TierRequired { get; set; }

        public FateChoice()
        {
            PathInfluence = new Dictionary<FatePathType, float>();
            StatBonuses = new Dictionary<string, float>();
        }
    }

    /// <summary>
    /// 命运卡牌配置数据结构（JSON反序列化用）
    /// </summary>
    [Serializable]
    public class FatePathConfigData
    {
        public string type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Dictionary<string, float> pathBonuses { get; set; }
        public List<string> exclusiveChoices { get; set; }
        public int unlockTier { get; set; }
    }

    /// <summary>
    /// 命运选择配置数据结构（JSON反序列化用）
    /// </summary>
    [Serializable]
    public class FateChoiceConfigData
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string choiceType { get; set; }
        public Dictionary<string, float> pathInfluence { get; set; }
        public Dictionary<string, float> statBonuses { get; set; }
        public string consequenceDescription { get; set; }
        public bool isSecret { get; set; }
        public int tierRequired { get; set; }
    }

    /// <summary>
    /// 命运卡牌配置文件结构
    /// </summary>
    [Serializable]
    public class FateCardsConfigFile
    {
        public string version { get; set; }
        public List<FatePathConfigData> paths { get; set; }
        public List<FateChoiceConfigData> choices { get; set; }
    }
}
