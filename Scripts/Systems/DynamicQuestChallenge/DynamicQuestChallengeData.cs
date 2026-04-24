using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.DynamicQuestChallenge
{
    /// <summary>
    /// DynamicQuestChallenge data container
    /// Stores active challenges, completed challenges, and statistics
    /// </summary>
    public class DynamicQuestChallengeData
    {
        /// <summary>
        /// Active challenges currently in progress
        /// </summary>
        public List<object> ActiveChallenges { get; set; } = new List<object>();

        /// <summary>
        /// Completed challenges history
        /// </summary>
        public List<object> CompletedChallenges { get; set; } = new List<object>();

        /// <summary>
        /// Statistics for quest challenge system
        /// </summary>
        public Dictionary<string, object> Statistics { get; set; } = new Dictionary<string, object>
        {
            { "total_generated", 0 },
            { "total_completed", 0 },
            { "total_abandoned", 0 },
            { "current_streak", 0 },
            { "longest_streak", 0 },
            { "total_gold_earned", 0 },
            { "total_experience_earned", 0 }
        };

        /// <summary>
        /// Convert data to dictionary for serialization
        /// </summary>
        public Dictionary ToDict()
        {
            return new Dictionary
            {
                { "active_challenges", ActiveChallenges },
                { "completed_challenges", CompletedChallenges },
                { "statistics", Statistics }
            };
        }

        /// <summary>
        /// Load data from dictionary
        /// </summary>
        public void FromDict(Dictionary dict)
        {
            if (dict.ContainsKey("active_challenges"))
            {
                ActiveChallenges = new List<object>((Godot.Collections.Array)dict["active_challenges"]);
            }
            if (dict.ContainsKey("completed_challenges"))
            {
                CompletedChallenges = new List<object>((Godot.Collections.Array)dict["completed_challenges"]);
            }
            if (dict.ContainsKey("statistics"))
            {
                Statistics = new Dictionary<string, object>((Godot.Collections.Dictionary)dict["statistics"]);
            }
        }
    }
}
