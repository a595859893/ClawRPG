using Godot;
using System;
using System.Collections.Generic;
using Framework;
using ClawRPG.Systems.Meditation;

namespace ClawRPG.Systems.MeditationCheckpoint
{
    /// <summary>
    /// Meditation Checkpoint System — transforms meditation into a "chapter marker" save system.
    ///
    /// Every meditation automatically creates a checkpoint (no prompt, no action required).
    /// On resume, if the player meditated this run, they return to their last moment of reflection
    /// rather than the last auto-save.
    ///
    /// Design principle: "I stopped to think, and the world remembered."
    /// </summary>
    public partial class MeditationCheckpointSystem : BaseSystem
    {
        public static MeditationCheckpointSystem Instance { get; private set; }

        // Signals
        public MeditationCheckpointSignals Signals;

        public partial class MeditationCheckpointSignals : GodotObject
        {
            public delegate void CheckpointCreatedHandler(string runId, int meditationCount, string chapterLabel);
            public delegate void CheckpointLoadedHandler(string runId, Vector2 position, string zone);
            public delegate void CheckpointClearedHandler(string runId);

            public event CheckpointCreatedHandler CheckpointCreated;
            public event CheckpointLoadedHandler CheckpointLoaded;
            public event CheckpointClearedHandler CheckpointCleared;

            public void EmitCheckpointCreated(string runId, int meditationCount, string chapterLabel)
            {
                CheckpointCreated?.Invoke(runId, meditationCount, chapterLabel);
            }

            public void EmitCheckpointLoaded(string runId, Vector2 position, string zone)
            {
                CheckpointLoaded?.Invoke(runId, position, zone);
            }

            public void EmitCheckpointCleared(string runId)
            {
                CheckpointCleared?.Invoke(runId);
            }
        }

        // Internal state
        private string _currentRunId = "";
        private int _meditationCount = 0;
        private MeditationCheckpointData _currentCheckpoint;
        private bool _subscribedToMeditationSignals = false;

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            Signals = new MeditationCheckpointSignals();

            // Subscribe to MeditationCoreSystem signals
            _SubscribeToMeditationSignals();

            // Subscribe to game state signals for run lifecycle
            _SubscribeToGameStateSignals();
        }

        private void _SubscribeToMeditationSignals()
        {
            if (_subscribedToMeditationSignals)
                return;

            var meditation = MeditationCoreSystem.Instance;
            if (meditation != null)
            {
                meditation.Signals.MeditationStarted += _OnMeditationStarted;
                meditation.Signals.MeditationCompleted += _OnMeditationCompleted;
                _subscribedToMeditationSignals = true;
                GD.Print("[MeditationCheckpoint] Subscribed to MeditationCoreSystem signals");
            }
            else
            {
                // Retry next frame
                CallDeferred(nameof(_SubscribeToMeditationSignalsDeferred));
            }
        }

        private void _SubscribeToMeditationSignalsDeferred()
        {
            _SubscribeToMeditationSignals();
        }

        private void _SubscribeToGameStateSignals()
        {
            // Listen for run start / run end / prestige signals
            // These are emitted by various systems — we use TryGetNode to avoid hard coupling
            var eventBus = GetNodeOrNull<EventBusManager>("/root/EventBusManager");
            if (eventBus != null)
            {
                // Run ended or prestige cleared checkpoint data
                eventBus.CallDeferred("connect", "RunEnded", Callable.From(_OnRunEnded));
                eventBus.CallDeferred("connect", "PrestigeReset", Callable.From(_OnPrestigeReset));
            }

            // Game exit handler
            if (HasNode("/root"))
            {
                GetTree().Connect("idle_frame", Callable.From(_CheckAutoSave), Object.ConnectFlags.OneShot);
            }
        }

        private void _CheckAutoSave()
        {
            // This runs once — for game exit we connect to SceneTree quit signal
            if (GetTree() != null)
            {
                GetTree().Connect("quit_requested", Callable.From(_OnQuitRequested));
            }
        }

        #region Signal Handlers

        /// <summary>
        /// Triggered when player enters meditation — create checkpoint automatically
        /// </summary>
        private void _OnMeditationStarted(string playerId, MeditationType type)
        {
            // Only checkpoint in valid zones
            string currentZone = _GetCurrentZone();
            if (!MeditationCheckpointDatabase.AllowCombatMeditationCheckpoint &&
                !MeditationCheckpointDatabase.IsCheckpointZone(currentZone))
            {
                GD.Print($"[MeditationCheckpoint] Skipping checkpoint — not in meditation zone (current: {currentZone})");
                return;
            }

            _CreateCheckpoint(playerId, type, currentZone);
        }

        /// <summary>
        /// Triggered when meditation completes — enhance with chapter label
        /// </summary>
        private void _OnMeditationCompleted(string playerId, MeditationType type, List<string> benefits)
        {
            if (_currentCheckpoint != null && !string.IsNullOrEmpty(_currentCheckpoint.RunId))
            {
                GD.Print($"[MeditationCheckpoint] Meditation complete — checkpoint saved: {_currentCheckpoint.ChapterLabel}");
            }
        }

        private void _OnRunEnded()
        {
            // Run ended — checkpoint stays valid until new run starts
            GD.Print("[MeditationCheckpoint] Run ended, checkpoint preserved for resume");
        }

        private void _OnPrestigeReset()
        {
            // Prestige/reset — clear all checkpoints for this run
            if (!string.IsNullOrEmpty(_currentRunId))
            {
                _ClearCheckpointForRun(_currentRunId);
            }
        }

        private void _OnQuitRequested()
        {
            // Game is exiting — ensure checkpoint is set as resume point
            if (_currentCheckpoint != null && !string.IsNullOrEmpty(_currentCheckpoint.RunId))
            {
                _SetResumePoint(_currentCheckpoint);
                GD.Print($"[MeditationCheckpoint] Quit requested — resume point set at {_currentCheckpoint.ChapterLabel}");
            }
        }

        #endregion

        #region Core Checkpoint Logic

        /// <summary>
        /// Create a checkpoint at the current meditation point
        /// </summary>
        private void _CreateCheckpoint(string playerId, MeditationType type, string zone)
        {
            // Get or create run ID
            if (string.IsNullOrEmpty(_currentRunId))
            {
                _currentRunId = _GetOrCreateRunId();
            }

            _meditationCount++;

            // Get player position
            Vector2 playerPos = _GetPlayerPosition();

            // Create checkpoint data
            _currentCheckpoint = new MeditationCheckpointData
            {
                RunId = _currentRunId,
                MeditationCount = _meditationCount,
                Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                LastMeditationPosition = playerPos,
                LastMeditationZone = zone,
                ScenePath = _GetCurrentScenePath(),
                MeditationType = type.ToString(),
                WorldStateSnapshot = _CaptureLightweightWorldState()
            };

            // Emit signal
            string chapterLabel = MeditationCheckpointDatabase.GetChapterLabel(_meditationCount);
            Signals.EmitCheckpointCreated(_currentRunId, _meditationCount, chapterLabel);

            // Set this as the active resume point
            _SetResumePoint(_currentCheckpoint);

            GD.Print($"[MeditationCheckpoint] Checkpoint created: {chapterLabel} at {zone} ({playerPos})");
        }

        /// <summary>
        /// Set a checkpoint as the active resume point (called on quit)
        /// </summary>
        private void _SetResumePoint(MeditationCheckpointData checkpoint)
        {
            // Store the resume point in a way that SaveSerializer / GameStateManager can read
            // We use a dedicated file: user://meditation_resume_point.json
            try
            {
                var savePath = "user://meditation_resume_point.json";
                using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);

                if (file != null)
                {
                    var saveDict = new Dictionary<string, Variant>
                    {
                        { "runId", checkpoint.RunId },
                        { "meditationCount", checkpoint.MeditationCount },
                        { "timestamp", checkpoint.Timestamp },
                        { "positionX", checkpoint.LastMeditationPosition.X },
                        { "positionY", checkpoint.LastMeditationPosition.Y },
                        { "zone", checkpoint.LastMeditationZone },
                        { "scenePath", checkpoint.ScenePath },
                        { "meditationType", checkpoint.MeditationType },
                        { "chapterLabel", checkpoint.ChapterLabel }
                    };

                    string json = Json.Stringify(saveDict);
                    file.StoreString(json);
                    file.Close();
                    GD.Print($"[MeditationCheckpoint] Resume point saved to {savePath}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[MeditationCheckpoint] Failed to save resume point: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a meditation checkpoint exists for the current run
        /// </summary>
        public bool HasCheckpointForCurrentRun()
        {
            if (string.IsNullOrEmpty(_currentRunId))
                return false;

            // Check if resume point file exists
            try
            {
                var savePath = "user://meditation_resume_point.json";
                return FileAccess.FileExists(savePath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the chapter label for the last checkpoint
        /// </summary>
        public string GetLastChapterLabel()
        {
            return _currentCheckpoint != null
                ? _currentCheckpoint.ChapterLabel
                : "Chapter 0";
        }

        /// <summary>
        /// Get meditation count for current run
        /// </summary>
        public int GetMeditationCount() => _meditationCount;

        /// <summary>
        /// Load the last checkpoint data (for resume flow integration)
        /// </summary>
        public MeditationCheckpointData LoadLastCheckpoint()
        {
            try
            {
                var savePath = "user://meditation_resume_point.json";
                if (!FileAccess.FileExists(savePath))
                    return null;

                using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
                if (file == null)
                    return null;

                string json = file.GetAsText();
                file.Close();

                var parsed = Json.ParseString(json) as Dictionary<string, Variant>;
                if (parsed == null)
                    return null;

                var data = new MeditationCheckpointData
                {
                    RunId = parsed.TryGetValue("runId", out var runId) ? (string)runId : "",
                    MeditationCount = parsed.TryGetValue("meditationCount", out var mc) ? (int)mc : 0,
                    Timestamp = parsed.TryGetValue("timestamp", out var ts) ? (long)ts : 0,
                    LastMeditationPosition = new Vector2(
                        parsed.TryGetValue("positionX", out var px) ? (float)px : 0,
                        parsed.TryGetValue("positionY", out var py) ? (float)py : 0),
                    LastMeditationZone = parsed.TryGetValue("zone", out var z) ? (string)z : "",
                    ScenePath = parsed.TryGetValue("scenePath", out var sp) ? (string)sp : "",
                    MeditationType = parsed.TryGetValue("meditationType", out var mt) ? (string)mt : ""
                };

                Signals.EmitCheckpointLoaded(data.RunId, data.LastMeditationPosition, data.LastMeditationZone);
                return data;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[MeditationCheckpoint] Failed to load checkpoint: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Clear checkpoint for a specific run (called on prestige/reset)
        /// </summary>
        public void ClearCheckpointForRun(string runId)
        {
            _ClearCheckpointForRun(runId);
        }

        private void _ClearCheckpointForRun(string runId)
        {
            // Clear the resume point file
            try
            {
                var savePath = "user://meditation_resume_point.json";
                if (FileAccess.FileExists(savePath))
                {
                    DirAccess.Remove(savePath);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[MeditationCheckpoint] Failed to clear checkpoint: {ex.Message}");
            }

            _currentCheckpoint = null;
            _meditationCount = 0;
            Signals.EmitCheckpointCleared(runId);
            GD.Print($"[MeditationCheckpoint] Checkpoint cleared for run {runId}");
        }

        #endregion

        #region Utility Methods

        private string _GetCurrentZone()
        {
            // Try to get current zone from GameStateManager or similar
            // This is a soft dependency — returns empty string if not available
            try
            {
                if (HasNode("/root/GameStateManager"))
                {
                    var gsm = GetNode("/root/GameStateManager");
                    if (gsm.HasMethod("GetCurrentZoneName"))
                        return (string)gsm.Call("GetCurrentZoneName");
                }
            }
            catch { }

            // Fallback: check if current scene name matches known zones
            if (GetTree()?.CurrentScene != null)
            {
                string sceneName = GetTree().CurrentScene.Name;
                if (MeditationCheckpointDatabase.IsCheckpointZone(sceneName))
                    return sceneName;
            }

            return "";
        }

        private Vector2 _GetPlayerPosition()
        {
            try
            {
                // Try to get player position from Player or GameStateManager
                if (HasNode("/root/Player"))
                {
                    var player = GetNode("/root/Player");
                    if (player.HasMethod("GetGlobalPosition"))
                        return (Vector2)player.Call("GetGlobalPosition");
                    if (player is Node2D node2d)
                        return node2d.GlobalPosition;
                }
                if (HasNode("/root/GameStateManager"))
                {
                    var gsm = GetNode("/root/GameStateManager");
                    if (gsm.HasMethod("GetPlayerPosition"))
                        return (Vector2)gsm.Call("GetPlayerPosition");
                }
            }
            catch { }

            return new Vector2(0, 0);
        }

        private string _GetCurrentScenePath()
        {
            return GetTree()?.CurrentScene?.SceneFilePath ?? "";
        }

        private string _GetOrCreateRunId()
        {
            // Get from GameStateManager if available
            try
            {
                if (HasNode("/root/GameStateManager"))
                {
                    var gsm = GetNode("/root/GameStateManager");
                    if (gsm.HasMethod("GetCurrentRunId"))
                        return (string)gsm.Call("GetCurrentRunId");
                }
            }
            catch { }

            // Fallback: generate from timestamp
            return $"run_{DateTimeOffset.Now.ToUnixTimeSeconds()}";
        }

        private Dictionary<string, object> _CaptureLightweightWorldState()
        {
            // Capture only meditation-relevant state, not full game state
            // This is intentionally minimal — just what's needed for narrative framing
            var snapshot = new Dictionary<string, object>
            {
                { "meditationCount", _meditationCount },
                { "timestamp", DateTimeOffset.Now.ToUnixTimeSeconds() }
            };

            // Add current zone if available
            string zone = _GetCurrentZone();
            if (!string.IsNullOrEmpty(zone))
                snapshot["zone"] = zone;

            return snapshot;
        }

        #endregion

        #region Persistence

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, Variant>
            {
                { "currentRunId", _currentRunId },
                { "meditationCount", _meditationCount }
            };

            if (_currentCheckpoint != null)
            {
                data["checkpoint"] = new Dictionary<string, Variant>
                {
                    { "runId", _currentCheckpoint.RunId },
                    { "meditationCount", _currentCheckpoint.MeditationCount },
                    { "timestamp", _currentCheckpoint.Timestamp },
                    { "positionX", _currentCheckpoint.LastMeditationPosition.X },
                    { "positionY", _currentCheckpoint.LastMeditationPosition.Y },
                    { "zone", _currentCheckpoint.LastMeditationZone },
                    { "scenePath", _currentCheckpoint.ScenePath },
                    { "meditationType", _currentCheckpoint.MeditationType }
                };
            }

            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.TryGetValue("currentRunId", out var runId))
                _currentRunId = (string)runId;
            if (data.TryGetValue("meditationCount", out var mc))
                _meditationCount = (int)mc;

            if (data.TryGetValue("checkpoint", out var cp) && cp is Dictionary<string, object> cpDict)
            {
                _currentCheckpoint = new MeditationCheckpointData
                {
                    RunId = cpDict.TryGetValue("runId", out var cprId) ? (string)cprId : "",
                    MeditationCount = cpDict.TryGetValue("meditationCount", out var cpmc) ? (int)cpmc : 0,
                    Timestamp = cpDict.TryGetValue("timestamp", out var ts) ? (long)ts : 0,
                    LastMeditationPosition = new Vector2(
                        cpDict.TryGetValue("positionX", out var px) ? (float)px : 0,
                        cpDict.TryGetValue("positionY", out var py) ? (float)py : 0),
                    LastMeditationZone = cpDict.TryGetValue("zone", out var z) ? (string)z : "",
                    ScenePath = cpDict.TryGetValue("scenePath", out var sp) ? (string)sp : "",
                    MeditationType = cpDict.TryGetValue("meditationType", out var mt) ? (string)mt : ""
                };
            }

            GD.Print($"[MeditationCheckpoint] Loaded checkpoint data: run={_currentRunId}, meditationCount={_meditationCount}");
        }

        #endregion
    }
}
