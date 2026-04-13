using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// REQ-180: Ghost Conviction System
    ///
    /// Meta-reward layer for ComboGhostSystem (REQ-174).
    ///
    /// Every time a player completes a ghost combo (via AdvanceGhost()),
    /// they earn 1 Conviction Point. At 10/25/50 points, narrative
    /// fragments unlock in the Ghost Archive.
    ///
    /// This is purely a narrative/meta layer — no gameplay balance impact.
    ///
    /// Integration: ComboGhostSystem.AdvanceGhost() → AddConvictionPoint()
    /// </summary>
    public partial class GhostConvictionSystem : BaseSystem
    {
        public new static GhostConvictionSystem Instance { get; private set; }

        // REQ-180: Core data
        private GhostConvictionData _convictionData = new GhostConvictionData();

        // Signals
        /// <summary>Fired when conviction points change. arg0=new total.</summary>
        public static Action<int> OnConvictionPointsChanged;

        /// <summary>Fired when one or more fragments are unlocked. arg0=list of fragment IDs.</summary>
        public static Action<List<string>> OnFragmentsUnlocked;

        /// <summary>Fired when all fragments are unlocked (max tier reached).</summary>
        public static Action OnAllFragmentsUnlocked;

        protected override void Initialize()
        {
            Instance = this;
            GD.Print("[GhostConvictionSystem] Initialized");
        }

        /// <summary>
        /// REQ-180: Called by ComboGhostSystem.AdvanceGhost() when a ghost combo is completed.
        /// Awards 1 conviction point and triggers unlocks.
        /// </summary>
        public void AddConvictionPoint()
        {
            int oldPoints = _convictionData.TotalConvictionPoints;
            var newlyUnlocked = _convictionData.AddConvictionPoints(1);

            GD.Print($"[GhostConvictionSystem] +1 conviction point (total: {_convictionData.TotalConvictionPoints})");

            OnConvictionPointsChanged?.Invoke(_convictionData.TotalConvictionPoints);

            if (newlyUnlocked.Count > 0)
            {
                GD.Print($"[GhostConvictionSystem] Unlocked fragments: {string.Join(", ", newlyUnlocked)}");
                OnFragmentsUnlocked?.Invoke(newlyUnlocked);

                // Check if all fragments are now unlocked
                if (_convictionData.UnlockedFragments.Count >= GhostConvictionDatabase.FragmentPool.Count)
                {
                    OnAllFragmentsUnlocked?.Invoke();
                    GD.Print("[GhostConvictionSystem] All fragments unlocked — player has reconciled with their ghosts.");
                }
            }
        }

        // === Public API ===

        /// <summary>Current total conviction points.</summary>
        public int GetConvictionPoints() => _convictionData.TotalConvictionPoints;

        /// <summary>Total ghost combos ever completed.</summary>
        public int GetCompletedGhostComboCount() => _convictionData.CompletedGhostComboCount;

        /// <summary>List of all unlocked fragment IDs.</summary>
        public List<string> GetUnlockedFragments() => new List<string>(_convictionData.UnlockedFragments);

        /// <summary>Points needed for the next fragment. 0 if all unlocked.</summary>
        public int GetPointsToNextFragment() => _convictionData.PointsToNextFragment();

        /// <summary>Get the content for a specific fragment ID.</summary>
        public string GetFragmentContent(string fragmentId)
            => GhostConvictionDatabase.GetFragmentContent(fragmentId);

        /// <summary>Get all unlocked fragments with their content.</summary>
        public List<(string id, string content, int tier)> GetUnlockedFragmentsWithContent()
        {
            var result = new List<(string, string, int)>();
            foreach (var id in _convictionData.UnlockedFragments)
            {
                var content = GhostConvictionDatabase.GetFragmentContent(id);
                // Find tier
                int tier = 0;
                foreach (var f in GhostConvictionDatabase.FragmentPool)
                {
                    if (f.Id == id) { tier = f.Tier; break; }
                }
                result.Add((id, content, tier));
            }
            return result;
        }

        // === Persistence ===

        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>
            {
                ["totalConvictionPoints"] = _convictionData.TotalConvictionPoints,
                ["completedGhostComboCount"] = _convictionData.CompletedGhostComboCount,
                ["unlockedFragments"] = _convictionData.UnlockedFragments,
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.TryGetValue("totalConvictionPoints", out var p) && p is int points)
                _convictionData.TotalConvictionPoints = points;

            if (data.TryGetValue("completedGhostComboCount", out var c) && c is int count)
                _convictionData.CompletedGhostComboCount = count;

            if (data.TryGetValue("unlockedFragments", out var f) && f is List<object> fragments)
            {
                _convictionData.UnlockedFragments.Clear();
                foreach (var item in fragments)
                    if (item is string s) _convictionData.UnlockedFragments.Add(s);
            }

            GD.Print($"[GhostConvictionSystem] Loaded { _convictionData.TotalConvictionPoints} conviction points, " +
                     $"{_convictionData.UnlockedFragments.Count} fragments unlocked.");
        }
    }
}
