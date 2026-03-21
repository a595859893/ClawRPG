using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat status tracking system - tracks real-time combat statistics
    /// </summary>
    public class CombatStatusSystem : BaseSystem
    {
        private static CombatStatusSystem _instance;
        public static new CombatStatusSystem Instance
        {
            get => _instance;
            private set => _instance = value;
        }

        // Current combat status
        private CombatStatusData.PlayerCombatStatus _currentCombat;
        
        // Session statistics
        private CombatStatusData.SessionStats _sessionStats;
        
        // Signals for UI updates
        public static event Action OnCombatStarted;
        public static event Action OnCombatEnded;
        public static event Action OnStatsUpdated;
        public static event Action OnComboChanged;
        public static event Action<CombatStatusData.CombatEvent> OnCombatEvent;
        
        // Combat timeout (seconds)
        private const float COMBAT_TIMEOUT = 5.0f;
        private float _combatTimer;

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            _currentCombat = new CombatStatusData.PlayerCombatStatus();
            _sessionStats = new CombatStatusData.SessionStats();
            LoadData();
        }
        
        protected override void Initialize()
        {
            GD.Print("[CombatStatusSystem] Initialized");
        }
        
        // Max recent events to keep
        private const int MAX_RECENT_EVENTS = 20;
        
        /// <summary>
        /// Export save data
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return GetSaveData();
        }
        
        /// <summary>
        /// Import save data
        /// </summary>
        public override bool ImportSaveData(Dictionary<string, object> data)
        {
            if (data != null)
            {
                LoadSaveData(data);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Start tracking a new combat
        /// </summary>
        public void StartCombat()
        {
            if (!_currentCombat.IsInCombat)
            {
                _currentCombat = new CombatStatusData.PlayerCombatStatus
                {
                    CombatStartTime = Time.GetUnixTimeFromSystem(),
                    IsInCombat = true
                };
                _sessionStats.TotalCombats++;
                OnCombatStarted?.Invoke();
            }
            _combatTimer = COMBAT_TIMEOUT;
        }

        /// <summary>
        /// End the current combat
        /// </summary>
        public void EndCombat()
        {
            if (_currentCombat.IsInCombat)
            {
                _currentCombat.IsInCombat = false; 
                double combatDuration = Time.GetUnixTimeFromSystem() - _currentCombat.CombatStartTime;
                _sessionStats.TotalCombatTime += combatDuration;
                
                // Update session stats
                _sessionStats.TotalDamageDealt += _currentCombat.TotalDamageDealt;
                _sessionStats.TotalDamageTaken += _currentCombat.TotalDamageTaken;
                _sessionStats.TotalHealingDone += _currentCombat.TotalHealingDone;
                _sessionStats.TotalCriticalHits += _currentCombat.CriticalHits;
                _sessionStats.TotalEnemiesKilled += _currentCombat.EnemiesKilled;
                
                if (_currentCombat.MaxCombo > _sessionStats.BestCombo)
                    _sessionStats.BestCombo = _currentCombat.MaxCombo;
                
                OnCombatEnded?.Invoke();
            }
        }

        /// <summary>
        /// Process called every frame
        /// </summary>
        public override void _Process(double delta)
        {
            if (_currentCombat.IsInCombat)
            {
                _combatTimer -= (float)delta;
                if (_combatTimer <= 0)
                {
                    EndCombat();
                }
            }
        }

        /// <summary>
        /// Record damage dealt to enemy
        /// </summary>
        public void RecordDamageDealt(float damage, bool isCritical = false, 
            CombatStatusData.DamageBreakdown.DamageType damageType = CombatStatusData.DamageBreakdown.DamageType.Physical)
        {
            StartCombat();
            
            _currentCombat.TotalDamageDealt += damage;
            _currentCombat.SkillsUsed++;
            
            // Add to damage breakdown
            switch (damageType)
            {
                case CombatStatusData.DamageBreakdown.DamageType.Physical:
                    _currentCombat.DamageDealtBreakdown.PhysicalDamage += damage;
                    break;
                case CombatStatusData.DamageBreakdown.DamageType.Magic:
                    _currentCombat.DamageDealtBreakdown.MagicDamage += damage;
                    break;
                case CombatStatusData.DamageBreakdown.DamageType.Fire:
                    _currentCombat.DamageDealtBreakdown.FireDamage += damage;
                    break;
                case CombatStatusData.DamageBreakdown.DamageType.Ice:
                    _currentCombat.DamageDealtBreakdown.IceDamage += damage;
                    break;
                case CombatStatusData.DamageBreakdown.DamageType.Lightning:
                    _currentCombat.DamageDealtBreakdown.LightningDamage += damage;
                    break;
                case CombatStatusData.DamageBreakdown.DamageType.Dark:
                    _currentCombat.DamageDealtBreakdown.DarkDamage += damage;
                    break;
                case CombatStatusData.DamageBreakdown.DamageType.Holy:
                    _currentCombat.DamageDealtBreakdown.HolyDamage += damage;
                    break;
                case CombatStatusData.DamageBreakdown.DamageType.Poison:
                    _currentCombat.DamageDealtBreakdown.PoisonDamage += damage;
                    break;
            }

            if (isCritical)
            {
                _currentCombat.CriticalHits++;
                AddCombatEvent(CombatStatusData.CombatEventType.CriticalHit, damage, 
                    $"暴击! {damage:F0}", true);
            }
            else
            {
                AddCombatEvent(CombatStatusData.CombatEventType.DamageDealt, damage, 
                    $"造成 {damage:F0} 伤害");
            }

            OnStatsUpdated?.Invoke();
        }

        /// <summary>
        /// Record damage taken from enemy
        /// </summary>
        public void RecordDamageTaken(float damage, bool isCritical = false)
        {
            StartCombat();
            
            _currentCombat.TotalDamageTaken += damage;
            
            if (isCritical)
            {
                AddCombatEvent(CombatStatusData.CombatEventType.DamageTaken, damage, 
                    $"暴击受伤! {damage:F0}", true);
            }
            else
            {
                AddCombatEvent(CombatStatusData.CombatEventType.DamageTaken, damage, 
                    $"受到 {damage:F0} 伤害");
            }
            
            OnStatsUpdated?.Invoke();
        }

        /// <summary>
        /// Record healing done
        /// </summary>
        public void RecordHealingDone(float amount)
        {
            StartCombat();
            
            _currentCombat.TotalHealingDone += amount;
            AddCombatEvent(CombatStatusData.CombatEventType.HealingDone, amount, 
                $"恢复 {amount:F0} 生命");
            
            OnStatsUpdated?.Invoke();
        }

        /// <summary>
        /// Record healing received
        /// </summary>
        public void RecordHealingReceived(float amount)
        {
            StartCombat();
            
            _currentCombat.TotalHealingReceived += amount;
            OnStatsUpdated?.Invoke();
        }

        /// <summary>
        /// Record a block
        /// </summary>
        public void RecordBlock()
        {
            StartCombat();
            
            _currentCombat.Blocks++;
            AddCombatEvent(CombatStatusData.CombatEventType.Block, 0, "格挡成功");
            
            OnStatsUpdated?.Invoke();
        }

        /// <summary>
        /// Record a dodge
        /// </summary>
        public void RecordDodge()
        {
            StartCombat();
            
            _currentCombat.Dodges++;
            AddCombatEvent(CombatStatusData.CombatEventType.Dodge, 0, "闪避成功");
            
            OnStatsUpdated?.Invoke();
        }

        /// <summary>
        /// Record enemy killed
        /// </summary>
        public void RecordEnemyKilled(bool isBoss = false)
        {
            StartCombat();
            
            _currentCombat.EnemiesKilled++;
            
            if (isBoss)
            {
                AddCombatEvent(CombatStatusData.CombatEventType.BossDamage, 0, "Boss击败!");
            }
            else
            {
                AddCombatEvent(CombatStatusData.CombatEventType.EnemyKilled, 0, "敌人击败");
            }
            
            OnStatsUpdated?.Invoke();
        }

        /// <summary>
        /// Increment combo counter
        /// </summary>
        public void IncrementCombo()
        {
            if (!_currentCombat.IsInCombat)
                StartCombat();
                
            _currentCombat.CurrentCombo++;
            
            if (_currentCombat.CurrentCombo > _currentCombat.MaxCombo)
                _currentCombat.MaxCombo = _currentCombat.CurrentCombo;
            
            OnComboChanged?.Invoke();
        }

        /// <summary>
        /// Reset combo counter
        /// </summary>
        public void ResetCombo()
        {
            _currentCombat.CurrentCombo = 0;
            OnComboChanged?.Invoke();
        }

        /// <summary>
        /// Update buff/debuff counts
        /// </summary>
        public void UpdateBuffStatus(int buffs, int debuffs)
        {
            _currentCombat.ActiveBuffs = buffs;
            _currentCombat.ActiveDebuffs = debuffs;
            OnStatsUpdated?.Invoke();
        }

        /// <summary>
        /// Add a combat event to recent events
        /// </summary>
        private void AddCombatEvent(CombatStatusData.CombatEventType type, float value, 
            string description, bool isCritical = false)
        {
            var combatEvent = new CombatStatusData.CombatEvent
            {
                Type = type,
                Value = value,
                Description = description,
                Timestamp = Time.GetUnixTimeFromSystem(),
                IsCritical = isCritical
            };
            
            _currentCombat.RecentEvents.Add(combatEvent);
            
            // Keep only recent events
            while (_currentCombat.RecentEvents.Count > MAX_RECENT_EVENTS)
            {
                _currentCombat.RecentEvents.RemoveAt(0);
            }
            
            OnCombatEvent?.Invoke(combatEvent);
        }

        /// <summary>
        /// Get current combat status
        /// </summary>
        public CombatStatusData.PlayerCombatStatus GetCurrentCombatStatus()
        {
            return _currentCombat;
        }

        /// <summary>
        /// Get session statistics
        /// </summary>
        public CombatStatusData.SessionStats GetSessionStats()
        {
            return _sessionStats;
        }

        /// <summary>
        /// Calculate current DPS
        /// </summary>
        public float GetCurrentDPS()
        {
            if (!_currentCombat.IsInCombat || _currentCombat.CombatStartTime == 0)
                return 0;
                
            double elapsed = Time.GetUnixTimeFromSystem() - _currentCombat.CombatStartTime;
            if (elapsed <= 0)
                return 0;
                
            return (float)(_currentCombat.TotalDamageDealt / elapsed);
        }

        /// <summary>
        /// Calculate combat grade based on performance
        /// </summary>
        public CombatStatusData.CombatGrade CalculateCombatGrade()
        {
            if (_currentCombat.TotalDamageDealt == 0)
                return CombatStatusData.CombatGrade.D;
                
            float efficiency = _currentCombat.TotalDamageDealt / Math.Max(_currentCombat.TotalDamageTaken, 1);
            float survival = _currentCombat.TotalDamageTaken == 0 ? 1.0f : 
                Math.Min(1.0f, _currentCombat.TotalHealingDone / _currentCombat.TotalDamageTaken);
            float critRate = _currentCombat.TotalDamageDealt > 0 ? 
                (float)_currentCombat.CriticalHits / Math.Max(1, _currentCombat.SkillsUsed) : 0;
            
            float score = efficiency * 0.4f + survival * 0.3f + critRate * 0.3f;
            
            if (score >= 2.5f) return CombatStatusData.CombatGrade.S;
            if (score >= 1.5f) return CombatStatusData.CombatGrade.A;
            if (score >= 1.0f) return CombatStatusData.CombatGrade.B;
            if (score >= 0.5f) return CombatStatusData.CombatGrade.C;
            return CombatStatusData.CombatGrade.D;
        }

        /// <summary>
        /// Reset session statistics
        /// </summary>
        public void ResetSessionStats()
        {
            _sessionStats = new CombatStatusData.SessionStats();
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                { "session_stats", new Dictionary<string, object>
                    {
                        { "total_combats", _sessionStats.TotalCombats },
                        { "total_damage_dealt", _sessionStats.TotalDamageDealt },
                        { "total_damage_taken", _sessionStats.TotalDamageTaken },
                        { "total_healing_done", _sessionStats.TotalHealingDone },
                        { "total_critical_hits", _sessionStats.TotalCriticalHits },
                        { "total_enemies_killed", _sessionStats.TotalEnemiesKilled },
                        { "total_combat_time", _sessionStats.TotalCombatTime },
                        { "best_combo", _sessionStats.BestCombo },
                        { "highest_dps", _sessionStats.HighestDPS }
                    }
                }
            };
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("session_stats"))
            {
                var stats = data["session_stats"] as Dictionary<string, object>;
                if (stats != null)
                {
                    _sessionStats.TotalCombats = stats.ContainsKey("total_combats") ? 
                        Convert.ToInt32(stats["total_combats"]) : 0;
                    _sessionStats.TotalDamageDealt = stats.ContainsKey("total_damage_dealt") ? 
                        Convert.ToSingle(stats["total_damage_dealt"]) : 0;
                    _sessionStats.TotalDamageTaken = stats.ContainsKey("total_damage_taken") ? 
                        Convert.ToSingle(stats["total_damage_taken"]) : 0;
                    _sessionStats.TotalHealingDone = stats.ContainsKey("total_healing_done") ? 
                        Convert.ToSingle(stats["total_healing_done"]) : 0;
                    _sessionStats.TotalCriticalHits = stats.ContainsKey("total_critical_hits") ? 
                        Convert.ToInt32(stats["total_critical_hits"]) : 0;
                    _sessionStats.TotalEnemiesKilled = stats.ContainsKey("total_enemies_killed") ? 
                        Convert.ToInt32(stats["total_enemies_killed"]) : 0;
                    _sessionStats.TotalCombatTime = stats.ContainsKey("total_combat_time") ? 
                        Convert.ToDouble(stats["total_combat_time"]) : 0;
                    _sessionStats.BestCombo = stats.ContainsKey("best_combo") ? 
                        Convert.ToInt32(stats["best_combo"]) : 0;
                    _sessionStats.HighestDPS = stats.ContainsKey("highest_dps") ? 
                        Convert.ToSingle(stats["highest_dps"]) : 0;
                }
            }
        }
    }
}

// Damage type enum extension
public partial class CombatStatusData
{
    public partial class DamageBreakdown
    {
        public enum DamageType
        {
            Physical,
            Magic,
            Fire,
            Ice,
            Lightning,
            Dark,
            Holy,
            Poison
        }
    }
}
