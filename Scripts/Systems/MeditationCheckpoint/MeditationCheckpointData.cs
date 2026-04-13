using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.MeditationCheckpoint
{
    /// <summary>
    /// Checkpoint data recorded at each meditation session.
    /// This is the "chapter marker" — where the player chose to pause and reflect.
    /// </summary>
    public class MeditationCheckpointData
    {
        /// <summary>
        /// Current roguelike run identifier
        /// </summary>
        public string RunId { get; set; }

        /// <summary>
        /// How many times player meditated this run (chapter number)
        /// </summary>
        public int MeditationCount { get; set; }

        /// <summary>
        /// Unix timestamp of this meditation checkpoint
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Player position at last meditation
        /// </summary>
        public Vector2 LastMeditationPosition { get; set; }

        /// <summary>
        /// Zone/dungeon at last meditation
        /// </summary>
        public string LastMeditationZone { get; set; }

        /// <summary>
        /// Scene path to resume to
        /// </summary>
        public string ScenePath { get; set; }

        /// <summary>
        /// Lightweight world state snapshot (run-specific keys only)
        /// </summary>
        public Dictionary<string, object> WorldStateSnapshot { get; set; }

        /// <summary>
        /// Meditation type used at this checkpoint
        /// </summary>
        public string MeditationType { get; set; }

        /// <summary>
        /// Formatted chapter label for UI display
        /// </summary>
        public string ChapterLabel => $"Chapter {MeditationCount}";

        public MeditationCheckpointData()
        {
            WorldStateSnapshot = new Dictionary<string, object>();
            Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        }
    }

    /// <summary>
    /// Persisted save data for MeditationCheckpointSystem
    /// </summary>
    public class MeditationCheckpointSaveData
    {
        /// <summary>
        /// All checkpoints keyed by run ID
        /// </summary>
        public Dictionary<string, MeditationCheckpointData> Checkpoints { get; set; }

        /// <summary>
        /// Run ID of the currently tracked run
        /// </summary>
        public string CurrentRunId { get; set; }

        public MeditationCheckpointSaveData()
        {
            Checkpoints = new Dictionary<string, MeditationCheckpointData>();
        }
    }

    /// <summary>
    /// Static database providing default configuration and utility methods.
    /// </summary>
    public static class MeditationCheckpointDatabase
    {
        /// <summary>
        /// Maximum checkpoints to retain per run (older ones are淘汰)
        /// </summary>
        public const int MaxCheckpointsPerRun = 20;

        /// <summary>
        /// Whether checkpoint on combat meditation is allowed (default: false)
        /// Only meditation in Safe House or designated zones triggers checkpoint.
        /// </summary>
        public static bool AllowCombatMeditationCheckpoint => false;

        /// <summary>
        /// Zones where meditation triggers a checkpoint
        /// </summary>
        private static readonly HashSet<string> CheckpointZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SafeHouse",
            "MeditationShrine",
            "Temple",
            "Shrine",
            "RestArea"
        };

        /// <summary>
        /// Check if the given zone name allows checkpoint saving
        /// </summary>
        public static bool IsCheckpointZone(string zoneName)
        {
            if (string.IsNullOrEmpty(zoneName))
                return false;
            return CheckpointZones.Contains(zoneName);
        }

        /// <summary>
        /// Generate a chapter label for display
        /// </summary>
        public static string GetChapterLabel(int meditationCount)
        {
            return $"Chapter {meditationCount}";
        }
    }
}
