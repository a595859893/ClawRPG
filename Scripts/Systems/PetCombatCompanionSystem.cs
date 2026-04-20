using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Framework;
using ClawRPG.Scripts.Systems.Pets;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Pet Combat Companion System - coordinates pet behavior with player combat
    /// Features: Combo system, learning, sync with player
    /// </summary>
    public partial class PetCombatCompanionSystem : BaseSystem
    {
        public static PetCombatCompanionSystem Instance { get; private set; }

        private PetCombatCompanionData _companionData = new PetCombatCompanionData();
        
        // Signals for UI and game integration (Godot 4 compatible)
        [Signal]
        public delegate void ComboChainChangedEventHandler(string petId, int chain);
        [Signal]
        public delegate void RoleChangedEventHandler(string petId, string role);
        [Signal]
        public delegate void SyncLevelChangedEventHandler(string petId, float syncLevel);
        [Signal]
        public delegate void ComboExecutedEventHandler(string petId, ComboType comboType, float syncLevel);
        [Signal]
        public delegate void LearningUpdatedEventHandler(string petId, string learning);
        [Signal]
        public delegate void PositionRecommendationEventHandler(string petId, Vector2 position);
        /// <summary>
        /// 宠物协同攻击触发（REQ-136）：玩家combo触发后，宠物根据syncLevel概率发动协战
        /// </summary>
        [Signal]
        public delegate void SynergyAttackTriggeredEventHandler(string petId, string attackType, float syncLevel);

        public override void _Ready()
        {
            Instance = this;
            LoadData();
        }

        public override void _Process(double delta)
        {
            UpdateComboChains((float)delta);
        }

        #region Initialization

        public void InitializePetCompanion(string petId)
        {
            if (!_companionData.PetStates.ContainsKey(petId))
            {
                _companionData.PetStates[petId] = new PetCompanionState
                {
                    PetId = petId,
                    CurrentRole = "Attacker",
                    ComboChain = 0,
                    ComboWindow = 2.0f,
                    SyncLevel = 0.5f
                };
            }

            if (!_companionData.LearningData.ContainsKey(petId))
            {
                _companionData.LearningData[petId] = new PetLearningData
                {
                    PetId = petId,
                    AdaptationLevel = 0f,
                    LastLearningUpdate = DateTime.Now
                };
            }

            if (!_companionData.ComboHistory.ContainsKey(petId))
            {
                _companionData.ComboHistory[petId] = new List<CombatComboRecord>();
            }
        }

        #endregion

        #region Role Management

        public void SetPetRole(string petId, string role)
        {
            if (_companionData.PetStates.TryGetValue(petId, out var state))
            {
                string oldRole = state.CurrentRole;
                state.CurrentRole = role;
                
                // Update learning data
                if (_companionData.LearningData.TryGetValue(petId, out var learning))
                {
                    if (!learning.PreferredBehaviors.Contains(role))
                    {
                        learning.PreferredBehaviors.Add(role);
                    }
                }

                RoleChanged?.Emit(petId, role);
            }
        }

        public string GetPetRole(string petId)
        {
            if (_companionData.PetStates.TryGetValue(petId, out var state))
            {
                return state.CurrentRole;
            }
            return "Attacker";
        }

        public string GetRecommendedRole(string petId, float petHealthPercent, int nearbyEnemies, bool ownerNeedsHealing)
        {
            if (ownerNeedsHealing)
                return "Support";
            
            if (petHealthPercent < 0.3f)
                return "Scout";
            
            if (nearbyEnemies > 3)
                return "Tank";
            
            return "Attacker";
        }

        #endregion

        #region Combo System

        public void RecordPlayerAttack(string petId, string attackType, Vector2 playerPosition)
        {
            if (!_companionData.PetStates.TryGetValue(petId, out var state))
                return;

            float currentTime = (float)GetTimeSinceStart();
            float timeSinceLastAction = currentTime - state.LastAttackTime;

            // Update player's attack pattern for learning
            if (_companionData.LearningData.TryGetValue(petId, out var learning))
            {
                learning.PlayerAttackPattern[attackType] = timeSinceLastAction;
                
                // Update average interval
                float total = 0;
                int count = 0;
                foreach (var interval in learning.PlayerAttackPattern.Values)
                {
                    total += interval;
                    count++;
                }
                if (count > 0)
                    learning.AveragePlayerAttackInterval = total / count;
            }

            // Check if within combo window
            if (timeSinceLastAction <= state.ComboWindow && state.IsInCombo)
            {
                state.ComboChain++;
                _companionData.TotalCombos++;
                
                if (state.ComboChain > _companionData.HighestComboChain)
                    _companionData.HighestComboChain = state.ComboChain;

                ComboChainChanged?.Emit(petId, state.ComboChain);
            }
            else
            {
                // Reset combo if window expired
                if (state.ComboChain > 0)
                {
                    ExecuteCombo(petId, state.ComboChain);
                }
                state.ComboChain = 1;
                state.IsInCombo = true;
            }

            state.LastAttackTime = currentTime;
            state.LastPlayerAction = attackType;
            state.LastPlayerPosition = playerPosition;

            // Update sync level based on timing
            UpdateSyncLevel(petId, timeSinceLastAction);

            // REQ-136: 协同攻击概率触发
            // attackChance = 0.3f + syncLevel * 0.5f
            float attackChance = 0.3f + state.SyncLevel * 0.5f;
            if (Random.Shared.NextFloat() < attackChance && state.ComboChain >= 1)
            {
                ExecuteCombo(petId, state.ComboChain);
                SynergyAttackTriggered?.Emit(petId, attackType, state.SyncLevel);
            }

            // REQ-189: Record skill usage for departure profile card
            try
            {
                ClawRPG.Systems.PetDeparture.PetDepartureSystem.Instance?.RecordSkillUsed(petId, attackType);
            }
            catch (Exception)
            {
                // Silently ignore if PetDepartureSystem is not available
            }

            // REQ-190: Record first skill of each battle for memory guidance
            try
            {
                ClawRPG.Systems.PetBattleMemory.PetBattleMemorySystem.Instance?.RecordPlayerSkillUse(petId, attackType, "");
            }
            catch (Exception)
            {
                // Silently ignore if PetBattleMemorySystem is not available
            }
        }

        private void ExecuteCombo(string petId, int chainLength)
        {
            if (!_companionData.PetStates.TryGetValue(petId, out var state))
                return;

            ComboType comboType = DetermineComboType(chainLength, state.CurrentRole);
            float damage = CalculateComboDamage(chainLength, state.SyncLevel);
            int hitCount = Mathf.Min(chainLength, 10);

            // Record combo
            var record = new CombatComboRecord
            {
                PetId = petId,
                ComboType = comboType.ToString(),
                Damage = damage,
                Duration = state.ComboWindow,
                HitCount = hitCount,
                Timestamp = DateTime.Now
            };

            if (_companionData.ComboHistory.ContainsKey(petId))
            {
                _companionData.ComboHistory[petId].Add(record);
                
                // Keep only last 100 combos
                if (_companionData.ComboHistory[petId].Count > 100)
                    _companionData.ComboHistory[petId].RemoveAt(0);
            }

            _companionData.TotalComboDamage += damage;
            
            // Apply actual damage to enemy target(s)
            ApplyComboDamage(petId, damage, chainLength);
            
            ComboExecuted?.Emit(petId, comboType, damage);
        }
        
        /// <summary>
        /// 将 combo 伤害实际施加到敌人（REQ-134 核心修复）
        /// </summary>
        private void ApplyComboDamage(string petId, float damage, int chainLength)
        {
            // 获取 PetCombatAI 实例
            var petAI = PetCombatAI.Instance;
            if (petAI == null)
            {
                GD.PrintErr("[PetCombatCompanionSystem] PetCombatAI.Instance is null, cannot apply combo damage");
                return;
            }
            
            // 获取当前目标
            var currentTarget = petAI.GetCurrentTarget();
            
            if (chainLength <= 1 || currentTarget == null)
            {
                // 单目标攻击
                if (currentTarget != null)
                {
                    petAI.ApplyComboDamageToTarget(currentTarget, damage);
                }
                return;
            }
            
            // 多目标：分裂伤害
            var enemies = petAI.GetNearbyEnemies();
            if (enemies.Count == 0)
            {
                if (currentTarget != null)
                {
                    petAI.ApplyComboDamageToTarget(currentTarget, damage);
                }
                return;
            }
            
            int targetCount = Mathf.Min(chainLength, enemies.Count);
            float damagePerTarget = damage / targetCount;
            
            for (int i = 0; i < targetCount; i++)
            {
                var enemy = enemies[i];
                if (IsInstanceValid(enemy))
                {
                    petAI.ApplyComboDamageToTarget(enemy, damagePerTarget);
                }
            }
            
            GD.Print($"[PetCombatCompanionSystem] Multi-target combo: {targetCount} enemies, {damagePerTarget:F1} damage each");
        }

        private ComboType DetermineComboType(int chainLength, string role)
        {
            if (chainLength >= 8)
                return ComboType.Ultimate;
            if (chainLength >= 5)
                return ComboType.Support;
            if (role == "Support")
                return ComboType.Support;
            if (chainLength >= 3)
                return ComboType.Chain;
            return ComboType.Basic;
        }

        private float CalculateComboDamage(int chainLength, float syncLevel)
        {
            float baseDamage = 10f;
            float chainMultiplier = 1f + (chainLength - 1) * 0.2f;
            float syncMultiplier = 1f + syncLevel * 0.5f;
            return baseDamage * chainMultiplier * syncMultiplier;
        }

        private void UpdateComboChains(float delta)
        {
            float currentTime = (float)GetTimeSinceStart();

            foreach (var state in _companionData.PetStates.Values)
            {
                if (state.IsInCombo)
                {
                    float timeSinceLastAttack = currentTime - state.LastAttackTime;
                    
                    if (timeSinceLastAttack > state.ComboWindow)
                    {
                        // Combo expired
                        if (state.ComboChain > 0)
                        {
                            ExecuteCombo(state.PetId, state.ComboChain);
                        }
                        state.ComboChain = 0;
                        state.IsInCombo = false;
                    }
                }
            }
        }

        #endregion

        #region Sync Level

        private void UpdateSyncLevel(string petId, float timeSinceLastAction)
        {
            if (!_companionData.PetStates.TryGetValue(petId, out var state))
                return;

            if (!_companionData.LearningData.TryGetValue(petId, out var learning))
                return;

            // Sync improves when pet attacks close to player's attack timing
            float idealInterval = learning.AveragePlayerAttackInterval;
            float deviation = Mathf.Abs(timeSinceLastAction - idealInterval);
            
            float syncChange = 0.01f;
            if (deviation < 0.3f)
            {
                state.SyncLevel = Mathf.Min(1f, state.SyncLevel + syncChange);
            }
            else
            {
                state.SyncLevel = Mathf.Max(0f, state.SyncLevel - syncChange * 0.5f);
            }

            SyncLevelChanged?.Emit(petId, state.SyncLevel);
        }

        public float GetSyncLevel(string petId)
        {
            if (_companionData.PetStates.TryGetValue(petId, out var state))
            {
                return state.SyncLevel;
            }
            return 0.5f;
        }

        /// <summary>
        /// 获取当前激活宠物的同步等级
        /// </summary>
        public float GetCurrentSyncLevel()
        {
            string activeId = GetActivePetId();
            if (!string.IsNullOrEmpty(activeId))
            {
                return GetSyncLevel(activeId);
            }
            return 0.5f;
        }

        /// <summary>
        /// 获取当前激活的宠物ID（REQ-136）
        /// </summary>
        public string GetActivePetId()
        {
            if (!string.IsNullOrEmpty(_companionData.ActivePetId) &&
                _companionData.PetStates.ContainsKey(_companionData.ActivePetId))
            {
                return _companionData.ActivePetId;
            }
            // Fallback: return first registered pet
            foreach (var petId in _companionData.PetStates.Keys)
            {
                return petId;
            }
            return "";
        }

        #endregion

        #region Learning System

        public void RecordEnemyKill(string petId, string enemyType)
        {
            if (_companionData.LearningData.TryGetValue(petId, out var learning))
            {
                if (learning.EnemyTypeKills.ContainsKey(enemyType))
                    learning.EnemyTypeKills[enemyType]++;
                else
                    learning.EnemyTypeKills[enemyType] = 1;

                // Increase adaptation level
                learning.AdaptationLevel = Mathf.Min(100f, learning.AdaptationLevel + 0.5f);
                learning.LastLearningUpdate = DateTime.Now;

                LearningUpdated?.Emit(petId, "enemy_kill");
            }
        }

        public void RecordDodgeResult(string petId, bool success)
        {
            if (_companionData.LearningData.TryGetValue(petId, out var learning))
            {
                if (success)
                    learning.SuccessfulDodges++;
                else
                    learning.FailedDodges++;

                int total = learning.SuccessfulDodges + learning.FailedDodges;
                if (total > 0)
                    learning.DodgeSuccessRate = (float)learning.SuccessfulDodges / total;

                LearningUpdated?.Emit(petId, "dodge");
            }
        }

        public string GetMostKilledEnemyType(string petId)
        {
            if (_companionData.LearningData.TryGetValue(petId, out var learning))
            {
                string maxEnemy = "";
                int maxKills = 0;
                
                foreach (var kvp in learning.EnemyTypeKills)
                {
                    if (kvp.Value > maxKills)
                    {
                        maxKills = kvp.Value;
                        maxEnemy = kvp.Key;
                    }
                }
                
                return maxEnemy;
            }
            return "";
        }

        public Dictionary<string, object> GetLearningReport(string petId)
        {
            var report = new Dictionary<string, object>();
            
            if (_companionData.LearningData.TryGetValue(petId, out var learning))
            {
                report["most_killed_enemy"] = GetMostKilledEnemyType(petId);
                report["total_enemies_killed"] = learning.SuccessfulDodges + learning.FailedDodges; // Approximation
                report["dodge_success_rate"] = learning.DodgeSuccessRate;
                report["adaptation_level"] = learning.AdaptationLevel;
                report["preferred_role"] = learning.PreferredBehaviors.Count > 0 ? learning.PreferredBehaviors[0] : "Attacker";
                report["avg_attack_interval"] = learning.AveragePlayerAttackInterval;
            }

            return report;
        }

        #endregion

        #region Position Recommendations

        public Vector2 GetRecommendedPosition(string petId, Vector2 playerPosition, Vector2 enemyPosition, int enemyCount)
        {
            if (!_companionData.PetStates.TryGetValue(petId, out var state))
                return playerPosition;

            Vector2 directionToEnemy = (enemyPosition - playerPosition).Normalized();
            float optimalDistance = state.CurrentRole switch
            {
                "Tank" => 80f,      // Close to protect
                "Attacker" => 150f, // Medium range
                "Support" => 200f,   // Back line
                "Scout" => 300f,    // Far to flank
                _ => 150f
            };

            // Add some randomness based on sync level
            float randomOffset = (1f - state.SyncLevel) * 50f * (Random.Shared.NextFloat() - 0.5f);
            
            Vector2 recommended = playerPosition + directionToEnemy * optimalDistance;
            recommended.x += randomOffset;
            
            PositionRecommendation?.Emit(petId, recommended);
            return recommended;
        }

        #endregion

        #region Statistics

        public Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>();
            
            stats["total_combos"] = _companionData.TotalCombos;
            stats["total_combo_damage"] = _companionData.TotalComboDamage;
            stats["highest_combo_chain"] = _companionData.HighestComboChain;
            stats["pet_count"] = _companionData.PetStates.Count;

            return stats;
        }

        public Dictionary<string, object> GetPetStatistics(string petId)
        {
            var stats = new Dictionary<string, object>();
            
            if (_companionData.PetStates.TryGetValue(petId, out var state))
            {
                stats["role"] = state.CurrentRole;
                stats["current_combo_chain"] = state.ComboChain;
                stats["sync_level"] = state.SyncLevel;
            }

            if (_companionData.ComboHistory.TryGetValue(petId, out var history))
            {
                float totalDamage = 0;
                int totalHits = 0;
                foreach (var record in history)
                {
                    totalDamage += record.Damage;
                    totalHits += record.HitCount;
                }
                stats["total_combos"] = history.Count;
                stats["total_combo_damage"] = totalDamage;
                stats["total_combo_hits"] = totalHits;
            }

            // Add learning data
            foreach (var kvp in GetLearningReport(petId))
            {
                stats[kvp.Key] = kvp.Value;
            }

            return stats;
        }

        #endregion

        #region Save/Load

        public void SaveData()
        {
            GameDataManager.SetData("pet_combat_companion", _companionData);
        }

        public void LoadData()
        {
            if (GameDataManager.HasData("pet_combat_companion"))
            {
                _companionData = GameDataManager.GetData<PetCombatCompanionData>("pet_combat_companion");
            }
        }

        #endregion

        private double GetTimeSinceStart()
        {
            return Time.GetTicksMsec() / 1000.0;
        }
        
        // ===== 持久化 =====
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            data["active_pet_id"] = _companionData.ActivePetId ?? "";
            data["current_role"] = _companionData.CurrentRole ?? "";
            data["sync_level"] = _companionData.SyncLevel;
            data["combo_count"] = _companionData.ComboCount;
            data["max_combo_count"] = _companionData.MaxComboCount;
            data["total_combos_executed"] = _companionData.TotalCombosExecuted;
            data["total_attacks_assisted"] = _companionData.TotalAttacksAssisted;
            data["total_damage_dealt"] = _companionData.TotalDamageDealt;
            data["total_enemies_defeated"] = _companionData.TotalEnemiesDefeated;
            
            // 保存学习技能
            var learnedSkillsData = new Godot.Collections.Array();
            foreach (var skill in _companionData.LearnedSkills)
            {
                learnedSkillsData.Add(skill);
            }
            data["learned_skills"] = learnedSkillsData;

            // 保存讣告数据 (REQ-191)
            var obituaryDataList = new List<Dictionary<string, object>>();
            foreach (var kvp in _companionData.ObituaryData)
            {
                obituaryDataList.Add(new Dictionary<string, object>
                {
                    ["pet_id"] = kvp.Key,
                    ["total_battles"] = kvp.Value.TotalBattles,
                    ["most_used_combo"] = kvp.Value.MostUsedCombo ?? "",
                    ["most_used_combo_count"] = kvp.Value.MostUsedComboCount,
                    ["total_enemies_killed"] = kvp.Value.TotalEnemiesKilled,
                    ["total_battle_time_seconds"] = kvp.Value.TotalBattleTimeSeconds,
                    ["peak_streak"] = kvp.Value.PeakStreak,
                    ["first_battle_timestamp"] = kvp.Value.FirstBattleTimestamp,
                    ["last_battle_timestamp"] = kvp.Value.LastBattleTimestamp
                });
            }
            data["obituary_data"] = obituaryDataList;

            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("active_pet_id"))
                _companionData.ActivePetId = data["active_pet_id"].ToString();
            if (data.ContainsKey("current_role"))
                _companionData.CurrentRole = data["current_role"].ToString();
            if (data.ContainsKey("sync_level"))
                _companionData.SyncLevel = Convert.ToSingle(data["sync_level"]);
            if (data.ContainsKey("combo_count"))
                _companionData.ComboCount = Convert.ToInt32(data["combo_count"]);
            if (data.ContainsKey("max_combo_count"))
                _companionData.MaxComboCount = Convert.ToInt32(data["max_combo_count"]);
            if (data.ContainsKey("total_combos_executed"))
                _companionData.TotalCombosExecuted = Convert.ToInt32(data["total_combos_executed"]);
            if (data.ContainsKey("total_attacks_assisted"))
                _companionData.TotalAttacksAssisted = Convert.ToInt32(data["total_attacks_assisted"]);
            if (data.ContainsKey("total_damage_dealt"))
                _companionData.TotalDamageDealt = Convert.ToInt32(data["total_damage_dealt"]);
            if (data.ContainsKey("total_enemies_defeated"))
                _companionData.TotalEnemiesDefeated = Convert.ToInt32(data["total_enemies_defeated"]);
            
            if (data.ContainsKey("learned_skills"))
            {
                _companionData.LearnedSkills.Clear();
                var skills = (Array)data["learned_skills"];
                foreach (string skill in skills)
                {
                    _companionData.LearnedSkills.Add(skill);
                }
            }

            // 恢复讣告数据 (REQ-191)
            if (data.ContainsKey("obituary_data"))
            {
                try
                {
                    var obituaryList = (Godot.Collections.Array)data["obituary_data"];
                    _companionData.ObituaryData.Clear();
                    foreach (var item in obituaryList)
                    {
                        var dict = (Dictionary<string, object>)item;
                        string petId = dict.GetValueOrDefault("pet_id", "").ToString();
                        if (!string.IsNullOrEmpty(petId))
                        {
                            var obituary = new PetObituaryData
                            {
                                TotalBattles = Convert.ToInt32(dict.GetValueOrDefault("total_battles", 0)),
                                MostUsedCombo = dict.GetValueOrDefault("most_used_combo", "").ToString(),
                                MostUsedComboCount = Convert.ToInt32(dict.GetValueOrDefault("most_used_combo_count", 0)),
                                TotalEnemiesKilled = Convert.ToInt32(dict.GetValueOrDefault("total_enemies_killed", 0)),
                                TotalBattleTimeSeconds = Convert.ToDouble(dict.GetValueOrDefault("total_battle_time_seconds", 0.0)),
                                PeakStreak = Convert.ToInt32(dict.GetValueOrDefault("peak_streak", 0)),
                                FirstBattleTimestamp = Convert.ToInt64(dict.GetValueOrDefault("first_battle_timestamp", 0L)),
                                LastBattleTimestamp = Convert.ToInt64(dict.GetValueOrDefault("last_battle_timestamp", 0L))
                            };
                            _companionData.ObituaryData[petId] = obituary;
                        }
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[PetCombatCompanion] Failed to restore obituary data: {ex.Message}");
                }
            }
        }

        #region Obituary System Integration (REQ-191)

        /// <summary>
        /// 战斗开始时由 PetObituarySystem 调用（通过 CombatManager.CombatStarted 信号）
        /// </summary>
        public void OnBattleStarted()
        {
            string activeId = _companionData.ActivePetId;
            if (string.IsNullOrEmpty(activeId)) return;

            EnsureObituaryData(activeId);
            _companionData.ObituaryData[activeId].OnBattleStarted();
        }

        /// <summary>
        /// 战斗结束时由 PetObituarySystem 调用（通过 CombatManager.CombatEnded 信号）
        /// </summary>
        public void OnBattleEnded(string firstCombo)
        {
            string activeId = _companionData.ActivePetId;
            if (string.IsNullOrEmpty(activeId)) return;

            EnsureObituaryData(activeId);
            _companionData.ObituaryData[activeId].OnBattleEnded(firstCombo);

            // 同步 TotalEnemiesDefeated 到讣告数据
            _companionData.ObituaryData[activeId].TotalEnemiesKilled = _companionData.TotalEnemiesDefeated;

            // 同步 PeakStreak
            if (_companionData.MaxComboCount > _companionData.ObituaryData[activeId].PeakStreak)
            {
                _companionData.ObituaryData[activeId].PeakStreak = _companionData.MaxComboCount;
            }
        }

        /// <summary>
        /// 获取宠物讣告数据（供 PetObituarySystem 使用）
        /// </summary>
        public PetObituaryData GetObituaryData(string petId)
        {
            if (string.IsNullOrEmpty(petId)) return null;
            EnsureObituaryData(petId);
            return _companionData.ObituaryData[petId];
        }

        private void EnsureObituaryData(string petId)
        {
            if (!_companionData.ObituaryData.ContainsKey(petId))
            {
                _companionData.ObituaryData[petId] = new PetObituaryData();
            }
        }

        #endregion
    }
}
