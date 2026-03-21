using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Meditation
{
    /// <summary>
    /// Meditation System - Allows players to meditate for buffs and benefits
    /// </summary>
    public class MeditationSystem : BaseSystem
    {
        public static MeditationSystem Instance { get; private set; }
        
        // Player meditation data
        private Dictionary<string, MeditationProgress> _playerProgress = new Dictionary<string, MeditationProgress>();
        
        // Active buffs on players
        private Dictionary<string, List<MeditationBuff>> _activeBuffs = new Dictionary<string, List<MeditationBuff>>();
        
        // Current meditation sessions
        private Dictionary<string, MeditationSession> _activeSessions = new Dictionary<string, MeditationSession>();
        
        // Cooldown tracking
        private Dictionary<string, Dictionary<MeditationType, DateTime>> _cooldowns = new Dictionary<string, Dictionary<MeditationType, DateTime>>();
        
        // Signals
        public Signals Signals;
        
        public class Signals : Godot.Object
        {
            public delegate void MeditationStartedHandler(string playerId, MeditationType type);
            public delegate void MeditationCompletedHandler(string playerId, MeditationType type, List<string> benefits);
            public delegate void BuffAppliedHandler(string playerId, MeditationType type, string statAffected, float value);
            public delegate void BuffExpiredHandler(string playerId, MeditationType type);
            public delegate void FocusGainedHandler(string playerId, int focusAmount);
            public delegate void AbilityUnlockedHandler(string playerId, string abilityId);
            
            public event MeditationStartedHandler MeditationStarted;
            public event MeditationCompletedHandler MeditationCompleted;
            public event BuffAppliedHandler BuffApplied;
            public event BuffExpiredHandler BuffExpired;
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
            
            public void EmitBuffApplied(string playerId, MeditationType type, string statAffected, float value)
            {
                BuffApplied?.Invoke(playerId, type, statAffected, value);
            }
            
            public void EmitBuffExpired(string playerId, MeditationType type)
            {
                BuffExpired?.Invoke(playerId, type);
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
            Instance = this;
            Signals = new Signals();
            
            // Initialize timer for buff expiration checking
            var timer = new Timer();
            timer.Name = "MeditationBuffTimer";
            timer.WaitTime = 1.0f;
            timer.Autostart = true;
            timer.Timeout += _OnBuffCheckTimer;
            AddChild(timer);
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
        /// Check if meditation type is on cooldown
        /// </summary>
        public bool IsOnCooldown(string playerId, MeditationType type)
        {
            if (!_cooldowns.ContainsKey(playerId))
                return false;
            
            if (!_cooldowns[playerId].ContainsKey(type))
                return false;
            
            return DateTime.Now < _cooldowns[playerId][type];
        }
        
        /// <summary>
        /// Get remaining cooldown time
        /// </summary>
        public int GetCooldownRemaining(string playerId, MeditationType type)
        {
            if (!IsOnCooldown(playerId, type))
                return 0;
            
            var cooldownEnd = _cooldowns[playerId][type];
            return (int)(cooldownEnd - DateTime.Now).TotalSeconds;
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
        /// Get active buffs
        /// </summary>
        public List<MeditationBuff> GetActiveBuffs(string playerId)
        {
            return _activeBuffs.ContainsKey(playerId) ? _activeBuffs[playerId] : new List<MeditationBuff>();
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
        /// Check and remove expired buffs
        /// </summary>
        private void _OnBuffCheckTimer()
        {
            var now = DateTime.Now;
            var expiredBuffs = new List<Tuple<string, MeditationBuff>>();
            
            foreach (var kvp in _activeBuffs)
            {
                var playerId = kvp.Key;
                var buffs = kvp.Value;
                
                for (int i = buffs.Count - 1; i >= 0; i--)
                {
                    var buff = buffs[i];
                    if (!buff.IsPermanent)
                    {
                        var elapsed = (now - buff.StartTime).TotalSeconds;
                        if (elapsed >= buff.Duration)
                        {
                            expiredBuffs.Add(Tuple.Create(playerId, buff));
                            buffs.RemoveAt(i);
                        }
                    }
                }
            }
            
            foreach (var expired in expiredBuffs)
            {
                Signals.EmitBuffExpired(expired.Item1, expired.Item2.Type);
            }
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
        /// Apply a meditation benefit
        /// </summary>
        private void ApplyBenefit(string playerId, MeditationType type, MeditationBenefit benefit)
        {
            float value = benefit.BaseValue * benefit.EffectMultiplier;
            
            if (benefit.Duration == -1)
            {
                // Permanent buff - apply directly to player stats
                ApplyPermanentBuff(playerId, benefit.StatAffected, value);
            }
            else
            {
                // Temporary buff
                ApplyTemporaryBuff(playerId, type, benefit.StatAffected, value, benefit.Duration);
            }
            
            Signals.EmitBuffApplied(playerId, type, benefit.StatAffected, value);
        }
        
        /// <summary>
        /// Apply permanent stat bonus
        /// </summary>
        private void ApplyPermanentBuff(string playerId, string stat, float value)
        {
            // This would integrate with the player's stat system
            GD.Print($"[Meditation] Applying permanent buff to {playerId}: {stat} + {value}");
        }
        
        /// <summary>
        /// Apply temporary buff
        /// </summary>
        private void ApplyTemporaryBuff(string playerId, MeditationType type, string stat, float value, int duration)
        {
            if (!_activeBuffs.ContainsKey(playerId))
                _activeBuffs[playerId] = new List<MeditationBuff>();
            
            var buff = new MeditationBuff
            {
                BuffId = Guid.NewGuid().ToString(),
                Type = type,
                StatAffected = stat,
                Value = value,
                StartTime = DateTime.Now,
                Duration = duration,
                IsPermanent = false
            };
            
            _activeBuffs[playerId].Add(buff);
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
        /// Set cooldown for meditation type
        /// </summary>
        private void SetCooldown(string playerId, MeditationType type, int cooldownSeconds)
        {
            if (!_cooldowns.ContainsKey(playerId))
                _cooldowns[playerId] = new Dictionary<MeditationType, DateTime>();
            
            _cooldowns[playerId][type] = DateTime.Now.AddSeconds(cooldownSeconds);
        }
        
        /// <summary>
        /// Get meditation bonus for stat
        /// </summary>
        public float GetStatBonus(string playerId, string stat)
        {
            float totalBonus = 0f;
            
            if (!_activeBuffs.ContainsKey(playerId))
                return 0f;
            
            foreach (var buff in _activeBuffs[playerId])
            {
                if (buff.StatAffected == stat || buff.StatAffected == "AllStats")
                {
                    totalBonus += buff.Value;
                }
            }
            
            return totalBonus;
        }
        
        /// <summary>
        /// Save meditation data
        /// </summary>
        public Dictionary<string, object> SaveData(string playerId)
        {
            var data = new Dictionary<string, object>();
            
            if (_playerProgress.ContainsKey(playerId))
            {
                data["progress"] = _playerProgress[playerId];
            }
            
            if (_activeBuffs.ContainsKey(playerId))
            {
                data["buffs"] = _activeBuffs[playerId];
            }
            
            return data;
        }
        
        /// <summary>
        /// Load meditation data
        /// </summary>
        public void LoadData(string playerId, Dictionary<string, object> data)
        {
            if (data == null)
                return;
            
            if (data.ContainsKey("progress"))
            {
                _playerProgress[playerId] = (MeditationProgress)data["progress"];
            }
            
            if (data.ContainsKey("buffs"))
            {
                _activeBuffs[playerId] = (List<MeditationBuff>)data["buffs"];
            }
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
            
            // 保存活跃buff（需要恢复过期时间）
            var buffsData = new Dictionary<string, Variant>();
            foreach (var kvp in _activeBuffs)
            {
                var playerBuffs = new List<Dictionary<string, Variant>>();
                foreach (var buff in kvp.Value)
                {
                    playerBuffs.Add(new Dictionary<string, Variant>
                    {
                        { "buffId", buff.BuffId },
                        { "type", (int)buff.Type },
                        { "statAffected", buff.StatAffected },
                        { "value", buff.Value },
                        { "startTime", buff.StartTime.Ticks },
                        { "duration", buff.Duration },
                        { "isPermanent", buff.IsPermanent }
                    });
                }
                buffsData[kvp.Key] = playerBuffs;
            }
            data["buffs"] = buffsData;
            
            // 保存冷却时间
            var cooldownsData = new Dictionary<string, Variant>();
            foreach (var playerKvp in _cooldowns)
            {
                var playerCooldowns = new Dictionary<string, Variant>();
                foreach (var cdKvp in playerKvp.Value)
                {
                    playerCooldowns[cdKvp.Key.ToString()] = cdKvp.Value.Ticks;
                }
                cooldownsData[playerKvp.Key] = playerCooldowns;
            }
            data["cooldowns"] = cooldownsData;
            
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
            
            // 加载活跃buff
            if (data.TryGetValue("buffs", out var buffsData))
            {
                var buffsDict = (Dictionary<string, Variant>)buffsData;
                foreach (var kvp in buffsDict)
                {
                    var playerId = kvp.Key;
                    var playerBuffs = new List<MeditationBuff>();
                    
                    var buffsList = (List<Variant>)kvp.Value;
                    foreach (var buffVar in buffsList)
                    {
                        var buffDict = (Dictionary<string, Variant>)buffVar;
                        var buff = new MeditationBuff
                        {
                            BuffId = (string)buffDict["buffId"],
                            Type = (MeditationType)(int)buffDict["type"],
                            StatAffected = (string)buffDict["statAffected"],
                            Value = (float)buffDict["value"],
                            StartTime = new DateTime((long)buffDict["startTime"]),
                            Duration = (int)buffDict["duration"],
                            IsPermanent = (bool)buffDict["isPermanent"]
                        };
                        playerBuffs.Add(buff);
                    }
                    
                    _activeBuffs[playerId] = playerBuffs;
                }
            }
            
            // 加载冷却时间
            if (data.TryGetValue("cooldowns", out var cooldownsData))
            {
                var cdDict = (Dictionary<string, Variant>)cooldownsData;
                foreach (var playerKvp in cdDict)
                {
                    var playerId = playerKvp.Key;
                    var playerCooldowns = new Dictionary<MeditationType, DateTime>();
                    
                    var pcDict = (Dictionary<string, Variant>)playerKvp.Value;
                    foreach (var cd in pcDict)
                    {
                        if (Enum.TryParse<MeditationType>(cd.Key, out var meditationType))
                            playerCooldowns[meditationType] = new DateTime((long)cd.Value);
                    }
                    
                    _cooldowns[playerId] = playerCooldowns;
                }
            }
        }
}
