using Godot;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Systems.Meditation
{
    /// <summary>
    /// Meditation Core System - Handles core meditation session logic
    /// </summary>
    public partial class MeditationCoreSystem : BaseSystem
    {
        public static MeditationCoreSystem Instance { get; private set; }

        // Player meditation data
        private Dictionary<string, MeditationProgress> _playerProgress = new Dictionary<string, MeditationProgress>();

        // Current meditation sessions
        private Dictionary<string, MeditationSession> _activeSessions = new Dictionary<string, MeditationSession>();

        // Weak references to sibling systems for decoupling
        private WeakReference<MeditationBuffSystem> _buffSystemRef;
        private WeakReference<MeditationCooldownSystem> _cooldownSystemRef;

        // Signals
        public Signals Signals;

        public class Signals : Godot.Object
        {
            public delegate void MeditationStartedHandler(string playerId, MeditationType type);
            public delegate void MeditationCompletedHandler(string playerId, MeditationType type, List<string> benefits);
            public delegate void FocusGainedHandler(string playerId, int focusAmount);
            public delegate void AbilityUnlockedHandler(string playerId, string abilityId);

            public event MeditationStartedHandler MeditationStarted;
            public event MeditationCompletedHandler MeditationCompleted;
            public event FocusGainedHandler FocusGained;
            public event AbilityUnlockedHandler AbilityUnlocked;

            public void EmitMeditationStarted(string playerId, MeditationType type)
            {
                MeditationStarted?.Invoke(playerId, type);
            }

            public void EmitMeditationCompleted(string playerId, MeditationType type, List<string> benefits)
            {
                MeditationCompleted?.Invoke(playerId, type, benefits);
            }

            public void EmitFocusGained(string playerId, int focusAmount)
            {
                FocusGained?.Invoke(playerId, focusAmount);
            }

            public void EmitAbilityUnlocked(string playerId, string abilityId)
            {
                AbilityUnlocked?.Invoke(playerId, abilityId);
            }
        }

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            Signals = new Signals();
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            // Get references to sibling systems
            var buffNode = GetNodeOrNull<MeditationBuffSystem>("/root/MeditationBuffSystem");
            if (buffNode != null)
                _buffSystemRef = new WeakReference<MeditationBuffSystem>(buffNode);

            var cdNode = GetNodeOrNull<MeditationCooldownSystem>("/root/MeditationCooldownSystem");
            if (cdNode != null)
                _cooldownSystemRef = new WeakReference<MeditationCooldownSystem>(cdNode);
        }

        /// <summary>
        /// Start a meditation session
        /// </summary>
        public bool StartMeditation(string playerId, MeditationType type, int duration)
        {
            if (!CanMeditate(playerId, type, duration))
                return false;

            var config = MeditationDatabase.Instance.GetTypeConfig(type);
            if (config == null)
                return false;

            // Clamp duration
            duration = Mathf.Clamp(duration, config.MinDuration, config.MaxDuration);

            // Create session
            var session = new MeditationSession
            {
                PlayerId = playerId,
                Type = type,
                Duration = duration
            };

            _activeSessions[playerId] = session;

            // Set cooldown
            SetCooldown(playerId, type, config.Cooldown);

            // Emit signal
            Signals.EmitMeditationStarted(playerId, type);

            GD.Print($"[Meditation] Player {playerId} started {type} meditation for {duration} seconds");
            return true;
        }

        /// <summary>
        /// Complete a meditation session
        /// </summary>
        public void CompleteMeditation(string playerId)
        {
            if (!_activeSessions.ContainsKey(playerId))
                return;

            var session = _activeSessions[playerId];
            if (session.Completed)
                return;

            session.Completed = true;
            session.EndTime = DateTime.Now;

            var config = MeditationDatabase.Instance.GetTypeConfig(session.Type);
            if (config == null)
                return;

            // Calculate focus gain
            float durationMultiplier = (float)session.Duration / config.MinDuration;
            int focusGain = (int)(config.BaseFocusGain * durationMultiplier);

            // Apply focus
            AddFocus(playerId, focusGain);
            session.FocusGained = focusGain;

            // Get and apply benefits
            var benefits = MeditationDatabase.Instance.GetBenefitsForType(session.Type, session.Duration);
            foreach (var benefit in benefits)
            {
                ApplyBenefit(playerId, session.Type, benefit);
                session.AchievedBenefits.Add(benefit.BenefitName);
            }

            // Update progress
            UpdateProgress(playerId, session);

            // Check for unlocks
            CheckUnlocks(playerId);

            // Emit signal
            Signals.EmitMeditationCompleted(playerId, session.Type, session.AchievedBenefits);

            GD.Print($"[Meditation] Player {playerId} completed {session.Type} meditation, gained {focusGain} focus");

            _activeSessions.Remove(playerId);
        }

        /// <summary>
        /// Cancel a meditation session
        /// </summary>
        public void CancelMeditation(string playerId)
        {
            if (_activeSessions.ContainsKey(playerId))
            {
                var session = _activeSessions[playerId];

                // Award partial focus for time spent
                var elapsed = (DateTime.Now - session.StartTime).TotalSeconds;
                if (elapsed >= 10) // Minimum 10 seconds for partial benefit
                {
                    var config = MeditationDatabase.Instance.GetTypeConfig(session.Type);
                    if (config != null)
                    {
                        int partialFocus = (int)(config.BaseFocusGain * 0.1f * (elapsed / 30.0f));
                        AddFocus(playerId, partialFocus);
                    }
                }

                _activeSessions.Remove(playerId);
            }
        }

        /// <summary>
        /// Check if player can meditate
        /// </summary>
        public bool CanMeditate(string playerId, MeditationType type, int duration)
        {
            // Check if already meditating
            if (_activeSessions.ContainsKey(playerId))
                return false;

            // Check cooldown
            if (IsOnCooldown(playerId, type))
                return false;

            // Check if unlocked
            var progress = GetOrCreateProgress(playerId);
            if (!MeditationDatabase.Instance.IsMeditationUnlocked(type, progress.CurrentFocus))
                return false;

            var config = MeditationDatabase.Instance.GetTypeConfig(type);
            if (config == null)
                return false;

            // Check duration bounds
            if (duration < config.MinDuration || duration > config.MaxDuration)
                return false;

            return true;
        }

        /// <summary>
        /// Get current meditation session
        /// </summary>
        public MeditationSession GetCurrentSession(string playerId)
        {
            return _activeSessions.ContainsKey(playerId) ? _activeSessions[playerId] : null;
        }

        /// <summary>
        /// Get meditation progress
        /// </summary>
        public MeditationProgress GetProgress(string playerId)
        {
            return _playerProgress.ContainsKey(playerId) ? _playerProgress[playerId] : null;
        }

        /// <summary>
        /// Get unlocked meditation types
        /// </summary>
        public List<MeditationType> GetUnlockedTypes(string playerId)
        {
            var progress = GetOrCreateProgress(playerId);
            var result = new List<MeditationType>();

            foreach (MeditationType type in Enum.GetValues(typeof(MeditationType)))
            {
                if (MeditationDatabase.Instance.IsMeditationUnlocked(type, progress.CurrentFocus))
                    result.Add(type);
            }

            return result;
        }

        /// <summary>
        /// Add focus points
        /// </summary>
        private void AddFocus(string playerId, int amount)
        {
            var progress = GetOrCreateProgress(playerId);
            progress.CurrentFocus = Mathf.Min(progress.CurrentFocus + amount, progress.MaxFocus);
            Signals.EmitFocusGained(playerId, amount);
        }

        /// <summary>
        /// Update player meditation progress
        /// </summary>
        private void UpdateProgress(string playerId, MeditationSession session)
        {
            var progress = GetOrCreateProgress(playerId);

            progress.TotalSessions++;
            progress.TotalMeditationTime += session.Duration;

            if (progress.SessionsByType.ContainsKey(session.Type))
                progress.SessionsByType[session.Type]++;

            progress.LastMeditationTime = DateTime.Now;

            // Check daily reset
            if (DateTime.Now >= progress.DailyResetTime)
            {
                progress.DailySessions = 0;
                progress.DailyResetTime = DateTime.Today.AddDays(1);
            }
            progress.DailySessions++;

            // Add to recent sessions
            progress.RecentSessions.Add(session);
            if (progress.RecentSessions.Count > 10)
                progress.RecentSessions.RemoveAt(0);
        }

        /// <summary>
        /// Check for newly unlocked abilities
        /// </summary>
        private void CheckUnlocks(string playerId)
        {
            var progress = GetOrCreateProgress(playerId);

            // Check each meditation type for unlock
            foreach (var kvp in MeditationDatabase.Instance.FocusToUnlock)
            {
                string abilityId = kvp.Key;
                int requiredFocus = kvp.Value;

                if (progress.CurrentFocus >= requiredFocus && !progress.UnlockedAbilities.Contains(abilityId))
                {
                    progress.UnlockedAbilities.Add(abilityId);
                    Signals.EmitAbilityUnlocked(playerId, abilityId);
                    GD.Print($"[Meditation] Player {playerId} unlocked {abilityId} meditation!");
                }
            }
        }

        /// <summary>
        /// Get or create player progress
        /// </summary>
        private MeditationProgress GetOrCreateProgress(string playerId)
        {
            if (!_playerProgress.ContainsKey(playerId))
            {
                _playerProgress[playerId] = new MeditationProgress { PlayerId = playerId };
            }
            return _playerProgress[playerId];
        }

        /// <summary>
        /// Apply a meditation benefit
        /// </summary>
        private void ApplyBenefit(string playerId, MeditationType type, MeditationBenefit benefit)
        {
            if (_buffSystemRef != null && _buffSystemRef.TryGetTarget(out var buffSystem))
            {
                buffSystem.ApplyBenefit(playerId, type, benefit);
            }
        }

        /// <summary>
        /// Check if meditation type is on cooldown
        /// </summary>
        private bool IsOnCooldown(string playerId, MeditationType type)
        {
            if (_cooldownSystemRef != null && _cooldownSystemRef.TryGetTarget(out var cdSystem))
            {
                return cdSystem.IsOnCooldown(playerId, type);
            }
            return false;
        }

        /// <summary>
        /// Set cooldown for meditation type
        /// </summary>
        private void SetCooldown(string playerId, MeditationType type, int cooldownSeconds)
        {
            if (_cooldownSystemRef != null && _cooldownSystemRef.TryGetTarget(out var cdSystem))
            {
                cdSystem.SetCooldown(playerId, type, TimeSpan.FromSeconds(cooldownSeconds));
            }
        }

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            // 保存所有玩家的冥想进度
            var progressData = new Dictionary<string, Variant>();
            foreach (var kvp in _playerProgress)
            {
                var progress = kvp.Value;
                progressData[kvp.Key] = new Dictionary<string, Variant>
                {
                    { "currentFocus", progress.CurrentFocus },
                    { "maxFocus", progress.MaxFocus },
                    { "totalSessions", progress.TotalSessions },
                    { "totalMeditationTime", progress.TotalMeditationTime },
                    { "dailySessions", progress.DailySessions },
                    { "dailyResetTime", progress.DailyResetTime.Ticks },
                    { "lastMeditationTime", progress.LastMeditationTime.Ticks },
                    { "unlockedAbilities", new List<Variant>(progress.UnlockedAbilities) }
                };

                // 保存各类型会话计数
                var sessionsByType = new Dictionary<string, Variant>();
                foreach (var st in progress.SessionsByType)
                {
                    sessionsByType[st.Key.ToString()] = st.Value;
                }
                ((Dictionary<string, Variant>)progressData[kvp.Key])["sessionsByType"] = sessionsByType;
            }
            data["progress"] = progressData;

            // 保存活跃会话（如果有的话）
            var sessionsData = new Dictionary<string, Variant>();
            foreach (var kvp in _activeSessions)
            {
                var session = kvp.Value;
                sessionsData[kvp.Key] = new Dictionary<string, Variant>
                {
                    { "sessionId", session.SessionId },
                    { "playerId", session.PlayerId },
                    { "type", (int)session.Type },
                    { "startTime", session.StartTime.Ticks },
                    { "duration", session.Duration },
                    { "completed", session.Completed },
                    { "achievedBenefits", new List<Variant>(session.AchievedBenefits) },
                    { "focusGained", session.FocusGained }
                };
            }
            data["sessions"] = sessionsData;

            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            // 加载玩家冥想进度
            if (data.TryGetValue("progress", out var progressData))
            {
                var progressDict = (Dictionary<string, Variant>)progressData;
                foreach (var kvp in progressDict)
                {
                    var playerId = kvp.Key;
                    var pData = (Dictionary<string, Variant>)kvp.Value;

                    var progress = new MeditationProgress { PlayerId = playerId };

                    if (pData.TryGetValue("currentFocus", out var currentFocus))
                        progress.CurrentFocus = (int)currentFocus;
                    if (pData.TryGetValue("maxFocus", out var maxFocus))
                        progress.MaxFocus = (int)maxFocus;
                    if (pData.TryGetValue("totalSessions", out var totalSessions))
                        progress.TotalSessions = (int)totalSessions;
                    if (pData.TryGetValue("totalMeditationTime", out var totalTime))
                        progress.TotalMeditationTime = (int)totalTime;
                    if (pData.TryGetValue("dailySessions", out var dailySessions))
                        progress.DailySessions = (int)dailySessions;
                    if (pData.TryGetValue("dailyResetTime", out var dailyReset))
                        progress.DailyResetTime = new DateTime((long)dailyReset);
                    if (pData.TryGetValue("lastMeditationTime", out var lastMed))
                        progress.LastMeditationTime = new DateTime((long)lastMed);
                    if (pData.TryGetValue("unlockedAbilities", out var abilities))
                        progress.UnlockedAbilities = new List<string>((IEnumerable<string>)abilities);

                    if (pData.TryGetValue("sessionsByType", out var sessionsByType))
                    {
                        var sbt = (Dictionary<string, Variant>)sessionsByType;
                        foreach (var st in sbt)
                        {
                            if (Enum.TryParse<MeditationType>(st.Key, out var meditationType))
                                progress.SessionsByType[meditationType] = (int)st.Value;
                        }
                    }

                    _playerProgress[playerId] = progress;
                }
            }

            // 加载活跃会话
            if (data.TryGetValue("sessions", out var sessionsData))
            {
                var sessionsDict = (Dictionary<string, Variant>)sessionsData;
                foreach (var kvp in sessionsDict)
                {
                    var playerId = kvp.Key;
                    var sData = (Dictionary<string, Variant>)kvp.Value;

                    var session = new MeditationSession
                    {
                        PlayerId = (string)sData["playerId"],
                        Type = (MeditationType)(int)sData["type"],
                        StartTime = new DateTime((long)sData["startTime"]),
                        Duration = (int)sData["duration"],
                        Completed = (bool)sData["completed"],
                        FocusGained = (int)sData["focusGained"]
                    };

                    if (sData.TryGetValue("sessionId", out var sessionId))
                        session.SessionId = (string)sessionId;
                    if (sData.TryGetValue("achievedBenefits", out var benefits))
                        session.AchievedBenefits = new List<string>((IEnumerable<string>)benefits);

                    _activeSessions[playerId] = session;
                }
            }
        }
    }
}
