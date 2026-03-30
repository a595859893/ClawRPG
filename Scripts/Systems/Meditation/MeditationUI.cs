using Godot;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Systems.Meditation
{
    /// <summary>
    /// Meditation UI - Coordinator for the Meditation System
    /// Orchestrates MeditationCoreSystem, MeditationBuffSystem, and MeditationCooldownSystem
    /// Provides unified meditation entry point and aggregates signals from all subsystems
    /// </summary>
    public partial class MeditationUI : BaseSystem
    {
        public static MeditationUI Instance { get; private set; }

        // Subsystem references
        private MeditationCoreSystem _coreSystem;
        private MeditationBuffSystem _buffSystem;
        private MeditationCooldownSystem _cooldownSystem;

        // Aggregated signals - combines all subsystem signals
        public Signals Signals;

        /// <summary>
        /// Aggregated signals from all meditation subsystems
        /// </summary>
        public class Signals : GodotObject
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
            base._Ready();
            Instance = this;
            Signals = new Signals();

            // Get references to sibling systems
            InitializeSubsystemReferences();

            // Connect to subsystem signals
            ConnectSubsystemSignals();

            GD.Print($"[MeditationUI] MeditationUI coordinator initialized");
        }

        private void InitializeSubsystemReferences()
        {
            // Try to get references via node paths
            // In the actual game, these would be autoloaded or accessible via proper paths
            _coreSystem = GetNodeOrNull<MeditationCoreSystem>("/root/MeditationCoreSystem");
            _buffSystem = GetNodeOrNull<MeditationBuffSystem>("/root/MeditationBuffSystem");
            _cooldownSystem = GetNodeOrNull<MeditationCooldownSystem>("/root/MeditationCooldownSystem");

            // Fallback: Try Common autoload path
            if (_coreSystem == null)
                _coreSystem = GetNodeOrNull<MeditationCoreSystem>("/root/MeditationCoreSystem");
            if (_buffSystem == null)
                _buffSystem = GetNodeOrNull<MeditationBuffSystem>("/root/MeditationBuffSystem");
            if (_cooldownSystem == null)
                _cooldownSystem = GetNodeOrNull<MeditationCooldownSystem>("/root/MeditationCooldownSystem");

            if (_coreSystem == null || _buffSystem == null || _cooldownSystem == null)
            {
                GD.PushWarning("[MeditationUI] Some subsystem references could not be resolved. " +
                    $"Core: {_coreSystem != null}, Buff: {_buffSystem != null}, Cooldown: {_cooldownSystem != null}");
            }
        }

        private void ConnectSubsystemSignals()
        {
            // Connect to Core System signals
            if (_coreSystem?.Signals != null)
            {
                _coreSystem.Signals.MeditationStarted += (playerId, type) =>
                    Signals.EmitMeditationStarted(playerId, type);
                _coreSystem.Signals.MeditationCompleted += (playerId, type, benefits) =>
                    Signals.EmitMeditationCompleted(playerId, type, benefits);
                _coreSystem.Signals.FocusGained += (playerId, amount) =>
                    Signals.EmitFocusGained(playerId, amount);
                _coreSystem.Signals.AbilityUnlocked += (playerId, abilityId) =>
                    Signals.EmitAbilityUnlocked(playerId, abilityId);
            }

            // Connect to Buff System signals
            if (_buffSystem?.Signals != null)
            {
                _buffSystem.Signals.BuffApplied += (playerId, type, stat, value) =>
                    Signals.EmitBuffApplied(playerId, type, stat, value);
                _buffSystem.Signals.BuffExpired += (playerId, type) =>
                    Signals.EmitBuffExpired(playerId, type);
            }
        }

        /// <summary>
        /// Start a meditation session (coordinator entry point)
        /// </summary>
        public bool StartMeditation(string playerId, MeditationType type, int duration)
        {
            if (_coreSystem != null)
            {
                return _coreSystem.StartMeditation(playerId, type, duration);
            }
            GD.PushWarning("[MeditationUI] Cannot start meditation: CoreSystem not available");
            return false;
        }

        /// <summary>
        /// Complete a meditation session
        /// </summary>
        public void CompleteMeditation(string playerId)
        {
            if (_coreSystem != null)
            {
                _coreSystem.CompleteMeditation(playerId);
            }
        }

        /// <summary>
        /// Cancel a meditation session
        /// </summary>
        public void CancelMeditation(string playerId)
        {
            if (_coreSystem != null)
            {
                _coreSystem.CancelMeditation(playerId);
            }
        }

        /// <summary>
        /// Check if player can meditate
        /// </summary>
        public bool CanMeditate(string playerId, MeditationType type, int duration)
        {
            if (_coreSystem != null)
            {
                return _coreSystem.CanMeditate(playerId, type, duration);
            }
            return false;
        }

        /// <summary>
        /// Check if meditation type is on cooldown
        /// </summary>
        public bool IsOnCooldown(string playerId, MeditationType type)
        {
            if (_cooldownSystem != null)
            {
                return _cooldownSystem.IsOnCooldown(playerId, type);
            }
            return false;
        }

        /// <summary>
        /// Get remaining cooldown time
        /// </summary>
        public int GetCooldownRemaining(string playerId, MeditationType type)
        {
            if (_cooldownSystem != null)
            {
                return _cooldownSystem.GetCooldownRemaining(playerId, type);
            }
            return 0;
        }

        /// <summary>
        /// Get current meditation session
        /// </summary>
        public MeditationSession GetCurrentSession(string playerId)
        {
            if (_coreSystem != null)
            {
                return _coreSystem.GetCurrentSession(playerId);
            }
            return null;
        }

        /// <summary>
        /// Get meditation progress
        /// </summary>
        public MeditationProgress GetProgress(string playerId)
        {
            if (_coreSystem != null)
            {
                return _coreSystem.GetProgress(playerId);
            }
            return null;
        }

        /// <summary>
        /// Get active buffs
        /// </summary>
        public List<MeditationBuff> GetActiveBuffs(string playerId)
        {
            if (_buffSystem != null)
            {
                return _buffSystem.GetActiveBuffs(playerId);
            }
            return new List<MeditationBuff>();
        }

        /// <summary>
        /// Get unlocked meditation types
        /// </summary>
        public List<MeditationType> GetUnlockedTypes(string playerId)
        {
            if (_coreSystem != null)
            {
                return _coreSystem.GetUnlockedTypes(playerId);
            }
            return new List<MeditationType>();
        }

        /// <summary>
        /// Get meditation bonus for stat
        /// </summary>
        public float GetStatBonus(string playerId, string stat)
        {
            if (_buffSystem != null)
            {
                return _buffSystem.GetStatBonus(playerId, stat);
            }
            return 0f;
        }

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            // Delegate to each subsystem
            if (_coreSystem != null)
            {
                data["core"] = _coreSystem.ExportSaveData();
            }
            if (_buffSystem != null)
            {
                data["buff"] = _buffSystem.ExportSaveData();
            }
            if (_cooldownSystem != null)
            {
                data["cooldown"] = _cooldownSystem.ExportSaveData();
            }

            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            // Delegate to each subsystem
            if (data.TryGetValue("core", out var coreData) && _coreSystem != null)
            {
                _coreSystem.ImportSaveData((Dictionary)coreData);
            }
            if (data.TryGetValue("buff", out var buffData) && _buffSystem != null)
            {
                _buffSystem.ImportSaveData((Dictionary)buffData);
            }
            if (data.TryGetValue("cooldown", out var cooldownData) && _cooldownSystem != null)
            {
                _cooldownSystem.ImportSaveData((Dictionary)cooldownData);
            }
        }

        public override void Reset()
        {
            base.Reset();
            Instance = null;
        }

        public override string GetId()
        {
            return "MeditationUI";
        }
    }
}
