using Godot;
using System;
using System.Collections.Generic;

public class ConstellationData
{
    public enum ConstellationType
    {
        Aries,         // 白羊座 - 攻击
        Taurus,        // 金牛座 - 防御
        Gemini,        // 双子座 - 速度
        Cancer,        // 巨蟹座 - 生命
        Leo,           // 狮子座 - 暴击
        Virgo,         // 处女座 - 治疗
        Libra,        // 天秤座 - 平衡
        Scorpio,       // 天蝎座 - 暴击伤害
        Sagittarius,   // 射手座 - 经验
        Capricorn,    // 摩羯座 - 金币
        Aquarius,      // 水瓶座 - 魔法
        Pisces         // 双鱼座 - 特殊
    }

    public enum StarTier
    {
        Common,    // 普通星
        Uncommon,  // 优秀星
        Rare,      // 稀有星
        Epic,      // 史诗星
        Legendary  // 传奇星 - 星座核心
    }

    [System.Serializable]
    public class ConstellationStar
    {
        public string Id;
        public string Name;
        public string Description;
        public StarTier Tier;
        public ConstellationType Constellation;
        public int StarIndex;        // 在星座中的位置 0-8
        public int RequiredPoints;   // 需要的天赋点数
        public string RequiredItem; // 需要的物品（可选）
        public Dictionary<string, float> Attributes; // 属性加成
        public bool IsCore;         // 是否是星座核心星
    }

    [System.Serializable]
    public class ConstellationInfo
    {
        public ConstellationType Type;
        public string Name;
        public string Description;
        public string ChineseName;
        public int TotalStars;
        public float[] CoreBonus;   // 激活核心星的额外加成
    }

    [System.Serializable]
    public class PlayerConstellationData
    {
        public HashSet<string> UnlockedStars = new HashSet<string>();
        public HashSet<string> ActivatedStars = new HashSet<string>();
        public ConstellationType[] ActivatedConstellations = new ConstellationType[0];
        public int TotalPointsSpent = 0;
    }
}
