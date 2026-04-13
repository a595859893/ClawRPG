using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.TitleBiography
{
    /// <summary>
    /// 称号传记数据 - 记录单个称号的传记条目
    /// </summary>
    public class TitleBiographyData
    {
        /// <summary>对应称号ID</summary>
        public string TitleId { get; set; }
        /// <summary>传记标题（复用TitleData.TitleName）</summary>
        public string TitleName { get; set; }
        /// <summary>动态生成的传记正文</summary>
        public string BiographyText { get; set; }
        /// <summary>解锁时间</summary>
        public DateTime UnlockTime { get; set; }
        /// <summary>称号稀有度（用于颜色）</summary>
        public string Rarity { get; set; }
        /// <summary>称号分类</summary>
        public string Category { get; set; }

        public TitleBiographyData()
        {
            TitleId = "";
            TitleName = "";
            BiographyText = "";
            UnlockTime = DateTime.MinValue;
            Rarity = "Common";
            Category = "Combat";
        }

        public TitleBiographyData(string titleId, string titleName, string bioText, DateTime unlockTime, string rarity, string category)
        {
            TitleId = titleId;
            TitleName = titleName;
            BiographyText = bioText;
            UnlockTime = unlockTime;
            Rarity = rarity;
            Category = category;
        }
    }

    /// <summary>
    /// 传记条目（用于UI展示）
    /// </summary>
    public class BiographyEntry
    {
        public string TitleId { get; set; }
        public string TitleName { get; set; }
        public string BiographyText { get; set; }
        public DateTime UnlockTime { get; set; }
        public string Rarity { get; set; }
        public string Category { get; set; }
        public int Progress { get; set; }       // 传记进度（如果有）
        public int RequiredValue { get; set; }  // 目标值（如果有）

        public BiographyEntry() { }

        public BiographyEntry(TitleBiographyData data)
        {
            TitleId = data.TitleId;
            TitleName = data.TitleName;
            BiographyText = data.BiographyText;
            UnlockTime = data.UnlockTime;
            Rarity = data.Rarity;
            Category = data.Category;
        }
    }

    /// <summary>
    /// 传记解锁状态（用于持久化）
    /// </summary>
    public class TitleBiographySaveData
    {
        public List<TitleBiographyData> UnlockedBiographies { get; set; } = new List<TitleBiographyData>();
        public int TotalBiographiesUnlocked { get; set; }
    }
}
