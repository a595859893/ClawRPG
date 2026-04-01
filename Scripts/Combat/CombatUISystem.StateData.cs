using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.Combat
{
    public partial class CombatUISystem
    {
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

        public override Dictionary<string, object> ExportSaveData()
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
        
        public override void ImportSaveData(Dictionary<string, object> data)
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
