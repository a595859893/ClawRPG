using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Combat;
using Framework;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat UI System - Manages all combat interface elements
    /// </summary>
    public class CombatUISystem : BaseSystem
    {
        private static CombatUISystem _instance;
        public static CombatUISystem Instance => _instance;
        
        // Database reference
        private CombatUIDatabase _database;
        
        // Active damage text instances
        private List<DamageTextData> _activeDamageTexts = new List<DamageTextData>();
        
        // Combat statistics
        private CombatStatistics _currentSessionStats = new CombatStatistics();
        
        // Combo tracking
        private ComboChainData _currentCombo = new ComboChainData();
        
        // UI preferences
        private UILayoutPreferences _uiPreferences = new UILayoutPreferences();
        
        // Combat state
        private PlayerCombatState _playerState = new PlayerCombatState();
        private List<EnemyCombatState> _enemyStates = new List<EnemyCombatState>();
        
        // Screen effects queue
        private Queue<ScreenEffectTrigger> _screenEffectQueue = new Queue<ScreenEffectTrigger>();
        
        // Signals
        public static string SignalDamageDealt = "damage_dealt";
        public static string SignalDamageTaken = "damage_taken";
        public static string SignalHealing = "healing";
        public static string SignalComboMilestone = "combo_milestone";
        public static string SignalKill = "enemy_kill";
        public static string SignalScreenEffect = "screen_effect";
        
        public override void _Ready()
        {
            _instance = this;
            _database = CombatUIDatabase.Instance;
            
            // Initialize default preferences
            _uiPreferences = new UILayoutPreferences
            {
                ShowDamageNumbers = true,
                ShowHealthBars = true,
                ShowComboCounter = true,
                ShowCombatIndicators = true,
                ShowDPS = false,
                UIScale = 1.0f,
                DamageNumberPosition = "above_target"
            };
            
            GD.Print("[CombatUISystem] Combat UI System initialized");
        }
        
        public override void _Process(float delta)
        {
            // Update combo timer
            if (_currentCombo.CurrentCombo > 0)
            {
                _currentCombo.ComboTimer -= delta;
                if (_currentCombo.ComboTimer <= 0)
                {
                    ResetCombo();
                }
            }
            
            // Update DPS calculation
            if (_currentSessionStats.SessionDuration > 0)
            {
                _currentSessionStats.DPS = _currentSessionStats.TotalDamageDealt / _currentSessionStats.SessionDuration;
            }
            
            // Process screen effects queue
            ProcessScreenEffects();
        }
        
        #region Damage Display
        
        /// <summary>
        /// Display damage number at target position
        /// </summary>
        public void ShowDamageNumber(float damage, DamageDisplayType type, Vector3 worldPosition, bool isPlayerSource = true)
        {
            if (!_uiPreferences.ShowDamageNumbers) return;
            
            var damageText = new DamageTextData
            {
                Amount = damage,
                DisplayType = type,
                Position = worldPosition,
                Lifetime = 1.0f,
                Scale = type == DamageDisplayType.Critical ? 1.5f : 1.0f,
                IsPlayerSource = isPlayerSource
            };
            
            _activeDamageTexts.Add(damageText);
            
            // Update statistics
            if (isPlayerSource)
            {
                _currentSessionStats.TotalDamageDealt += (int)damage;
                if (damage > _currentSessionStats.HighestDamage)
                {
                    _currentSessionStats.HighestDamage = damage;
                }
                
                if (type == DamageDisplayType.Critical)
                {
                    _currentSessionStats.CriticalHits++;
                    TriggerScreenEffect("critical_hit");
                }
                else
                {
                    TriggerScreenEffect("light_damage");
                }
                
                // Update combo
                if (damage > 0)
                {
                    AddComboHit(damage);
                }
            }
            else
            {
                _currentSessionStats.TotalDamageTaken += (int)damage;
                
                if (type == DamageDisplayType.Blocked)
                {
                    _currentSessionStats.Blocks++;
                    TriggerScreenEffect("perfect_block");
                }
                else if (damage > _playerState.CurrentHealth * 0.3f)
                {
                    TriggerScreenEffect("heavy_damage");
                }
                else
                {
                    TriggerScreenEffect("light_damage");
                }
            }
            
            EmitSignal(SignalDamageDealt, damage, type, worldPosition);
        }
        
        /// <summary>
        /// Display healing number
        /// </summary>
        public void ShowHealing(float amount, Vector3 worldPosition)
        {
            if (!_uiPreferences.ShowDamageNumbers) return;
            
            var healingText = new DamageTextData
            {
                Amount = amount,
                DisplayType = DamageDisplayType.Healing,
                Position = worldPosition,
                Lifetime = 1.2f,
                Scale = 1.2f,
                IsPlayerSource = true
            };
            
            _activeDamageTexts.Add(healingText);
            _currentSessionStats.TotalHealing += (int)amount;
            
            TriggerScreenEffect("healing");
            EmitSignal(SignalHealing, amount, worldPosition);
        }
        
        /// <summary>
        /// Display miss indicator
        /// </summary>
        public void ShowMiss(Vector3 worldPosition, bool isPlayerSource)
        {
            if (!_uiPreferences.ShowDamageNumbers) return;
            
            var missText = new DamageTextData
            {
                Amount = 0,
                DisplayType = DamageDisplayType.Miss,
                Position = worldPosition,
                Lifetime = 0.8f,
                Scale = 0.8f,
                IsPlayerSource = isPlayerSource
            };
            
            _activeDamageTexts.Add(missText);
            
            if (!isPlayerSource)
            {
                _currentSessionStats.Dodges++;
            }
        }
        
        #endregion
        
        #region Combat Indicators
        
        /// <summary>
        /// Show combat indicator message
        /// </summary>
        public void ShowCombatIndicator(CombatIndicatorType type, string message = "")
        {
            if (!_uiPreferences.ShowCombatIndicators) return;
            
            if (string.IsNullOrEmpty(message))
            {
                message = _database.GetRandomIndicatorMessage(type);
            }
            
            var indicator = new CombatIndicatorData
            {
                Type = type,
                Message = message,
                Duration = 2.0f,
                Priority = GetIndicatorPriority(type)
            };
            
            EmitSignal(SignalDamageDealt, indicator);
        }
        
        /// <summary>
        /// Show buff indicator
        /// </summary>
        public void ShowBuff(string buffName, bool isPositive = true)
        {
            var type = isPositive ? CombatIndicatorType.Buff : CombatIndicatorType.Debuff;
            ShowCombatIndicator(type, $"{buffName} {(isPositive ? "Applied!" : "Applied!")}");
        }
        
        /// <summary>
        /// Show kill indicator
        /// </summary>
        public void ShowKill(string enemyName)
        {
            _currentSessionStats.EnemiesKilled++;
            ShowCombatIndicator(CombatIndicatorType.Kill, $"Defeated {enemyName}!");
            TriggerScreenEffect("kill_streak");
            EmitSignal(SignalKill, enemyName);
        }
        
        /// <summary>
        /// Show boss phase change
        /// </summary>
        public void ShowBossPhase(int newPhase, int totalPhases)
        {
            ShowCombatIndicator(CombatIndicatorType.BossPhase, $"Phase {newPhase}/{totalPhases}!");
            TriggerScreenEffect("phase_change");
        }
        
        private int GetIndicatorPriority(CombatIndicatorType type)
        {
            switch (type)
            {
                case CombatIndicatorType.BossPhase:
                case CombatIndicatorType.Stun:
                    return 10;
                case CombatIndicatorType.Combo:
                case CombatIndicatorType.Kill:
                    return 8;
                case CombatIndicatorType.Critical:
                case CombatIndicatorType.Shield:
                    return 6;
                case CombatIndicatorType.Damage:
                    return 4;
                case CombatIndicatorType.Healing:
                case CombatIndicatorType.Buff:
                case CombatIndicatorType.Debuff:
                    return 2;
                default:
                    return 0;
            }
        }
        
        #endregion
        
        #region Combo System
        
        /// <summary>
        /// Add hit to combo chain
        /// </summary>
        public void AddComboHit(float damage)
        {
            _currentCombo.CurrentCombo++;
            _currentCombo.ComboHits++;
            _currentCombo.ComboDamage += damage;
            _currentCombo.ComboTimer = _currentCombo.MaxComboTime;
            
            if (_currentCombo.CurrentCombo > _currentCombo.MaxCombo)
            {
                _currentCombo.MaxCombo = _currentCombo.CurrentCombo;
            }
            
            // Check for milestone
            var milestone = _database.GetComboMilestone(_currentCombo.CurrentCombo);
            if (milestone != null)
            {
                ShowCombatIndicator(CombatIndicatorType.Combo, milestone.Message.Replace("{N}", _currentCombo.CurrentCombo.ToString()));
                EmitSignal(SignalComboMilestone, _currentCombo.CurrentCombo, milestone.Message);
                
                if (_currentCombo.CurrentCombo >= 10)
                {
                    TriggerScreenEffect("kill_streak");
                }
            }
        }
        
        /// <summary>
        /// Reset combo chain
        /// </summary>
        public void ResetCombo()
        {
            if (_currentCombo.CurrentCombo > 0)
            {
                GD.Print($"[CombatUI] Combo ended: {_currentCombo.MaxCombo} max hits, {_currentCombo.ComboDamage} total damage");
            }
            
            _currentCombo.CurrentCombo = 0;
            _currentCombo.ComboHits = 0;
            _currentCombo.ComboDamage = 0;
            _currentCombo.ComboTimer = 0;
        }
        
        /// <summary>
        /// Get current combo data
        /// </summary>
        public ComboChainData GetCurrentCombo()
        {
            return _currentCombo;
        }
        
        #endregion
        
        #region Screen Effects
        
        /// <summary>
        /// Trigger screen effect
        /// </summary>
        public void TriggerScreenEffect(string effectName, float intensity = 1.0f)
        {
            var effect = new ScreenEffectTrigger
            {
                EffectName = effectName,
                Intensity = intensity,
                Duration = 0.5f
            };
            
            _screenEffectQueue.Enqueue(effect);
            EmitSignal(SignalScreenEffect, effectName, intensity);
        }
        
        /// <summary>
        /// Process queued screen effects
        /// </summary>
        private void ProcessScreenEffects()
        {
            // Process one effect per frame
            if (_screenEffectQueue.Count > 0)
            {
                var effect = _screenEffectQueue.Dequeue();
                var config = _database.GetScreenEffect(effect.EffectName);
                
                if (config != null)
                {
                    ApplyScreenEffect(config, effect.Intensity);
                }
            }
        }
        
        private void ApplyScreenEffect(ScreenEffectConfig config, float intensity)
        {
            if (config.ScreenShake)
            {
                // Apply screen shake
                float shakeIntensity = config.ShakeIntensity * intensity;
                // Screen shake implementation would go here
            }
            
            if (config.FlashColor != null)
            {
                // Apply screen flash
                float flashAlpha = config.FlashAlpha * intensity;
                // Screen flash implementation would go here
            }
            
            if (config.SlowMotion)
            {
                // Apply slow motion
                float timeScale = config.SlowMotionScale;
                // Engine.time_scale = timeScale would be applied
            }
        }
        
        #endregion
        
        #region Player/Enemy State
        
        /// <summary>
        /// Update player combat state
        /// </summary>
        public void UpdatePlayerState(float currentHealth, float maxHealth, float shield = 0, float energy = 0, float maxEnergy = 100)
        {
            _playerState.CurrentHealth = currentHealth;
            _playerState.MaxHealth = maxHealth;
            _playerState.CurrentShield = shield;
            _playerState.CurrentEnergy = energy;
            _playerState.MaxEnergy = maxEnergy;
        }
        
        /// <summary>
        /// Get player combat state
        /// </summary>
        public PlayerCombatState GetPlayerState()
        {
            return _playerState;
        }
        
        /// <summary>
        /// Add enemy to tracking
        /// </summary>
        public void AddEnemy(string enemyId, string enemyName, float maxHealth, bool isBoss = false, int totalPhases = 1)
        {
            var enemy = new EnemyCombatState
            {
                EnemyId = enemyId,
                EnemyName = enemyName,
                CurrentHealth = maxHealth,
                MaxHealth = maxHealth,
                IsBoss = isBoss,
                CurrentPhase = 1,
                TotalPhases = totalPhases
            };
            
            _enemyStates.Add(enemy);
        }
        
        /// <summary>
        /// Update enemy health
        /// </summary>
        public void UpdateEnemyHealth(string enemyId, float currentHealth)
        {
            foreach (var enemy in _enemyStates)
            {
                if (enemy.EnemyId == enemyId)
                {
                    enemy.CurrentHealth = currentHealth;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Remove enemy from tracking
        /// </summary>
        public void RemoveEnemy(string enemyId)
        {
            _enemyStates.RemoveAll(e => e.EnemyId == enemyId);
        }
        
        /// <summary>
        /// Get all enemy states
        /// </summary>
        public List<EnemyCombatState> GetEnemyStates()
        {
            return _enemyStates;
        }
        
        #endregion
        
        #region Statistics
        
        /// <summary>
        /// Get current session statistics
        /// </summary>
        public CombatStatistics GetStatistics()
        {
            return _currentSessionStats;
        }
        
        /// <summary>
        /// Reset session statistics
        /// </summary>
        public void ResetStatistics()
        {
            _currentSessionStats = new CombatStatistics();
            ResetCombo();
        }
        
        /// <summary>
        /// Start new combat session
        /// </summary>
        public void StartCombatSession()
        {
            ResetStatistics();
            _currentSessionStats.SessionDuration = 0;
            _enemyStates.Clear();
        }
        
        /// <summary>
        /// End combat session
        /// </summary>
        public void EndCombatSession()
        {
            _currentSessionStats.SessionDuration += Time.GetTicksMsec() / 1000f;
        }
        
        #endregion
        
        #region UI Preferences
        
        /// <summary>
        /// Update UI preferences
        /// </summary>
        public void SetPreferences(UILayoutPreferences preferences)
        {
            _uiPreferences = preferences;
        }
        
        /// <summary>
        /// Get current UI preferences
        /// </summary>
        public UILayoutPreferences GetPreferences()
        {
            return _uiPreferences;
        }
        
        /// <summary>
        /// Toggle damage numbers
        /// </summary>
        public void ToggleDamageNumbers()
        {
            _uiPreferences.ShowDamageNumbers = !_uiPreferences.ShowDamageNumbers;
        }
        
        /// <summary>
        /// Toggle health bars
        /// </summary>
        public void ToggleHealthBars()
        {
            _uiPreferences.ShowHealthBars = !_uiPreferences.ShowHealthBars;
        }
        
        /// <summary>
        /// Toggle combo counter
        /// </summary>
        public void ToggleComboCounter()
        {
            _uiPreferences.ShowComboCounter = !_uiPreferences.ShowComboCounter;
        }
        
        #endregion
        
        #region Save/Load
        
        /// <summary>
        /// Save UI preferences
        /// </summary>
        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "preferences", _uiPreferences },
                { "lifetime_stats", new Dictionary<string, object>
                    {
                        { "total_damage_dealt", _currentSessionStats.TotalDamageDealt },
                        { "total_healing", _currentSessionStats.TotalHealing },
                        { "enemies_killed", _currentSessionStats.EnemiesKilled },
                        { "critical_hits", _currentSessionStats.CriticalHits }
                    }
                }
            };
        }
        
        /// <summary>
        /// Load UI preferences
        /// </summary>
        public void Deserialize(Dictionary<string, object> data)
        {
            if (data.ContainsKey("preferences"))
            {
                var prefs = data["preferences"] as Dictionary<string, object>;
                if (prefs != null)
                {
                    _uiPreferences.ShowDamageNumbers = prefs.ContainsKey("show_damage_numbers") ? (bool)prefs["show_damage_numbers"] : true;
                    _uiPreferences.ShowHealthBars = prefs.ContainsKey("show_health_bars") ? (bool)prefs["show_health_bars"] : true;
                    _uiPreferences.ShowComboCounter = prefs.ContainsKey("show_combo_counter") ? (bool)prefs["show_combo_counter"] : true;
                    _uiPreferences.ShowCombatIndicators = prefs.ContainsKey("show_combat_indicators") ? (bool)prefs["show_combat_indicators"] : true;
                    _uiPreferences.ShowDPS = prefs.ContainsKey("show_dps") ? (bool)prefs["show_dps"] : false;
                    _uiPreferences.UIScale = prefs.ContainsKey("ui_scale") ? (float)prefs["ui_scale"] : 1.0f;
                }
            }
        }
        
        #endregion

        #region BaseSystem Persistence

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();
            
            // 保存UI偏好设置
            data["showDamageNumbers"] = _uiPreferences.ShowDamageNumbers;
            data["showHealthBars"] = _uiPreferences.ShowHealthBars;
            data["showComboCounter"] = _uiPreferences.ShowComboCounter;
            data["showCombatIndicators"] = _uiPreferences.ShowCombatIndicators;
            data["showDPS"] = _uiPreferences.ShowDPS;
            data["uiScale"] = _uiPreferences.UIScale;
            data["damageNumberPosition"] = _uiPreferences.DamageNumberPosition;
            
            // 保存当前会话统计（作为累积数据）
            data["totalDamageDealt"] = _currentSessionStats.TotalDamageDealt;
            data["totalDamageTaken"] = _currentSessionStats.TotalDamageTaken;
            data["totalHealing"] = _currentSessionStats.TotalHealing;
            data["enemiesKilled"] = _currentSessionStats.EnemiesKilled;
            data["criticalHits"] = _currentSessionStats.CriticalHits;
            data["blocks"] = _currentSessionStats.Blocks;
            data["dodges"] = _currentSessionStats.Dodges;
            data["highestDamage"] = _currentSessionStats.HighestDamage;
            
            // 保存最大连击记录
            data["maxCombo"] = _currentCombo.MaxCombo;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 加载UI偏好设置
            if (data.TryGetValue("showDamageNumbers", out var showDamage))
                _uiPreferences.ShowDamageNumbers = (bool)showDamage;
            if (data.TryGetValue("showHealthBars", out var showHealth))
                _uiPreferences.ShowHealthBars = (bool)showHealth;
            if (data.TryGetValue("showComboCounter", out var showCombo))
                _uiPreferences.ShowComboCounter = (bool)showCombo;
            if (data.TryGetValue("showCombatIndicators", out var showIndicators))
                _uiPreferences.ShowCombatIndicators = (bool)showIndicators;
            if (data.TryGetValue("showDPS", out var showDps))
                _uiPreferences.ShowDPS = (bool)showDps;
            if (data.TryGetValue("uiScale", out var uiScale))
                _uiPreferences.UIScale = (float)uiScale;
            if (data.TryGetValue("damageNumberPosition", out var dmgPos))
                _uiPreferences.DamageNumberPosition = (string)dmgPos;
            
            // 加载统计累积数据
            if (data.TryGetValue("totalDamageDealt", out var totalDmg))
                _currentSessionStats.TotalDamageDealt = (int)totalDmg;
            if (data.TryGetValue("totalDamageTaken", out var takenDmg))
                _currentSessionStats.TotalDamageTaken = (int)takenDmg;
            if (data.TryGetValue("totalHealing", out var totalHeal))
                _currentSessionStats.TotalHealing = (int)totalHeal;
            if (data.TryGetValue("enemiesKilled", out var killed))
                _currentSessionStats.EnemiesKilled = (int)killed;
            if (data.TryGetValue("criticalHits", out var crits))
                _currentSessionStats.CriticalHits = (int)crits;
            if (data.TryGetValue("blocks", out var blocks))
                _currentSessionStats.Blocks = (int)blocks;
            if (data.TryGetValue("dodges", out var dodges))
                _currentSessionStats.Dodges = (int)dodges;
            if (data.TryGetValue("highestDamage", out var highest))
                _currentSessionStats.HighestDamage = (int)highest;
            
            // 加载最大连击记录
            if (data.TryGetValue("maxCombo", out var maxCombo))
                _currentCombo.MaxCombo = (int)maxCombo;
        }

        #endregion
    }
}
