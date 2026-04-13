using Godot;
using System;

namespace ClawRPG.Scripts.Systems.TitleRecommendation
{
    /// <summary>
    /// Title recommendation data for the "Next Goal" panel (REQ-200).
    /// Contains everything needed to render a single recommendation card.
    /// </summary>
    [System.Serializable]
    public class TitleRecommendationData
    {
        /// <summary>Internal ID of the title.</summary>
        public string TitleId;

        /// <summary>Display name of the title.</summary>
        public string TitleName;

        /// <summary>Short description / unlock condition text.</summary>
        public string Description;

        /// <summary>Current progress as a 0.0-1.0 fraction.</summary>
        public float Progress;

        /// <summary>How many more units are needed to unlock (display only).</summary>
        public int Remaining;

        /// <summary>Absolute current value for the condition type.</summary>
        public int CurrentValue;

        /// <summary>Required value for full unlock.</summary>
        public int RequiredValue;

        /// <summary>Human-readable unit label: "怪物", "Boss", "金币", etc.</summary>
        public string UnitLabel;

        /// <summary>Rarity tier.</summary>
        public int Rarity; // maps to TitleRarity enum index

        /// <summary>Category for colour/icon coding.</summary>
        public int Category; // maps to TitleCategory enum index

        /// <summary>Whether this title is already unlocked.</summary>
        public bool IsUnlocked;

        public TitleRecommendationData() { }

        public TitleRecommendationData(
            string titleId,
            string titleName,
            string description,
            float progress,
            int remaining,
            int currentValue,
            int requiredValue,
            string unitLabel,
            int rarity,
            int category,
            bool isUnlocked)
        {
            TitleId = titleId;
            TitleName = titleName;
            Description = description;
            Progress = progress;
            Remaining = remaining;
            CurrentValue = currentValue;
            RequiredValue = requiredValue;
            UnitLabel = unitLabel;
            Rarity = rarity;
            Category = category;
            IsUnlocked = isUnlocked;
        }

        /// <summary>Convenience: formatted string for the "X% (还差 Y)" label.</summary>
        public string GetProgressLabel()
        {
            int pct = (int)Mathf.Round(Progress * 100f);
            return $"{pct}% (还差 {Remaining} {UnitLabel})";
        }
    }
}
