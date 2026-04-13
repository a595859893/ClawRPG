using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// REQ-180: Ghost Conviction System — Data Layer
    ///
    /// Tracks the player's conviction points earned by completing ghost combos,
    /// and manages the narrative fragment archive unlocked at milestones.
    /// </summary>
    public class GhostConvictionData
    {
        /// <summary>Total conviction points accumulated by completing ghost combos.</summary>
        public int TotalConvictionPoints { get; set; }

        /// <summary>Total number of ghost combos ever completed.</summary>
        public int CompletedGhostComboCount { get; set; }

        /// <summary>List of unlocked narrative fragment IDs.</summary>
        public List<string> UnlockedFragments { get; set; } = new List<string>();

        /// <summary>
        /// REQ-180: Add conviction points and check for new fragment unlocks.
        /// Returns list of newly unlocked fragment IDs.
        /// </summary>
        public List<string> AddConvictionPoints(int points)
        {
            var newlyUnlocked = new List<string>();
            TotalConvictionPoints += points;
            CompletedGhostComboCount++;

            // Check each threshold for new unlocks
            foreach (var threshold in GhostConvictionDatabase.FragmentThresholds)
            {
                if (TotalConvictionPoints >= threshold.PointsRequired &&
                    !UnlockedFragments.Contains(threshold.FragmentId))
                {
                    UnlockedFragments.Add(threshold.FragmentId);
                    newlyUnlocked.Add(threshold.FragmentId);
                }
            }

            return newlyUnlocked;
        }

        /// <summary>
        /// Check how many points needed for the next fragment.
        /// Returns 0 if all fragments are unlocked.
        /// </summary>
        public int PointsToNextFragment()
        {
            foreach (var threshold in GhostConvictionDatabase.FragmentThresholds)
            {
                if (!UnlockedFragments.Contains(threshold.FragmentId))
                    return threshold.PointsRequired - TotalConvictionPoints;
            }
            return 0; // All unlocked
        }
    }

    /// <summary>
    /// REQ-180: A single narrative fragment in the Ghost Archive.
    /// </summary>
    public class NarrativeFragment
    {
        public string Id { get; set; }
        public string Content { get; set; }        // 1-3 sentences
        public int Tier { get; set; }              // 1=10pts, 2=25pts, 3=50pts
    }

    /// <summary>
    /// REQ-180: Fragment threshold mapping — points required to unlock each fragment.
    /// </summary>
    public class FragmentThreshold
    {
        public string FragmentId { get; set; }
        public int PointsRequired { get; set; }
        public int Tier { get; set; }
    }

    /// <summary>
    /// REQ-180: Static database of all narrative fragments and thresholds.
    /// </summary>
    public static class GhostConvictionDatabase
    {
        /// <summary>
        /// Narrative fragments pool. Each tier has multiple fragments that can be randomly selected.
        /// </summary>
        public static readonly List<NarrativeFragment> FragmentPool = new List<NarrativeFragment>
        {
            // === Tier 1: 10 points ===
            new NarrativeFragment { Id = "ghost_001", Tier = 1, Content = "你在第 42 次轮回，曾在第 3 层放弃了这套连招...幽灵还记得。" },
            new NarrativeFragment { Id = "ghost_002", Tier = 1, Content = "那是一次仓促的撤退。连击数停在 7，连招名称已被遗忘。" },
            new NarrativeFragment { Id = "ghost_003", Tier = 1, Content = "你曾在 Boss 门前测试这套 build。数据已模糊，但幽灵没有忘记。" },

            // === Tier 2: 25 points ===
            new NarrativeFragment { Id = "ghost_010", Tier = 2, Content = "第 17 次轮回，你曾执着于一种后来被证明无效的组合技。放弃不是失败——是进化。" },
            new NarrativeFragment { Id = "ghost_011", Tier = 2, Content = "你的手指记得一个节奏。第七步，你犹豫了。幽灵替你完成了它。" },
            new NarrativeFragment { Id = "ghost_012", Tier = 2, Content = "连击点数的来源已被遗忘，但这套连招的结构从未真正消失。" },
            new NarrativeFragment { Id = "ghost_013", Tier = 2, Content = "你的幽灵不是一个复制品。它是你放弃的可能性，是另一种结局的残响。" },

            // === Tier 3: 50 points ===
            new NarrativeFragment { Id = "ghost_020", Tier = 3, Content = "你已经与幽灵和解。失败不是终点——它是另一个自己的起点。" },
            new NarrativeFragment { Id = "ghost_021", Tier = 3, Content = "第 3 层的连招，在第 50 次轮回的黎明，终于被完成了。幽灵见证了这一刻。" },
            new NarrativeFragment { Id = "ghost_022", Tier = 3, Content = "所有的执念都已沉淀。你和你的幽灵，终于站在了同一边。" },
        };

        /// <summary>
        /// Thresholds that trigger fragment unlocks.
        /// </summary>
        public static readonly List<FragmentThreshold> FragmentThresholds = new List<FragmentThreshold>
        {
            new FragmentThreshold { FragmentId = "ghost_001", PointsRequired = 10, Tier = 1 },
            new FragmentThreshold { FragmentId = "ghost_010", PointsRequired = 25, Tier = 2 },
            new FragmentThreshold { FragmentId = "ghost_020", PointsRequired = 50, Tier = 3 },
        };

        /// <summary>
        /// Get the content string for a given fragment ID.
        /// </summary>
        public static string GetFragmentContent(string fragmentId)
        {
            foreach (var f in FragmentPool)
            {
                if (f.Id == fragmentId) return f.Content;
            }
            return string.Empty;
        }
    }
}
