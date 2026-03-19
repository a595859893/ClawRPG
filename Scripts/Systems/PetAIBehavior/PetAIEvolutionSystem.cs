using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Pet AI Evolution System - pets learn and evolve based on battle experience
    /// </summary>
    public partial class PetAIEvolutionSystem : BaseSystem
    {
        public static PetAIEvolutionSystem Instance { get; private set; }

        private PlayerPetAIEvolutionData _playerEvolutionData = new PlayerPetAIEvolutionData();
        
        // Signals
        public Signal<string, PetAIEvolutionType> EvolutionUnlocked { get; }
        public Signal<string, float> ProgressUpdated { get; }
        public Signal<string, PetAIEvolutionType, float> BonusActivated { get; }
        public Signal<string, int> ComboUpdated { get; }

        public override void _Ready()
        {
            Instance = this;
            LoadData();
        }

        public void InitializePetEvolution(string petId)
        {
            if (!_playerEvolutionData.PetEvolutions.ContainsKey(petId))
            {
                _playerEvolutionData.PetEvolutions[petId] = new PetAIEvolutionData
                {
                    PetId = petId,
                    LastEvolutionTime = DateTime.MinValue
                };
            }
        }

        /// <summary>
        /// Record battle results for learning
        /// </summary>
        public void RecordBattleResult(string petId, bool won, float damageDealt, float damageTaken, 
            float healingDone, int enemiesDefeated, float survivalRate)
        {
            if (!_playerEvolutionData.PetEvolutions.ContainsKey(petId))
                InitializePetEvolution(petId);

            var data = _playerEvolutionData.PetEvolutions[petId];
            
            data.TotalBattlesFought++;
            if (won) data.BattlesWon++;
            data.TotalDamageDealt += (int)damageDealt;
            data.TotalDamageTaken += (int)damageTaken;
            data.TotalHealingDone += (int)healingDone;
            data.EnemiesDefeated += enemiesDefeated;
            
            if (survivalRate > data.BestSurvivalRate)
                data.BestSurvivalRate = survivalRate;

            // Learn from defeat
            if (!won && !data.HasLearnedFromDefeat)
            {
                data.HasLearnedFromDefeat = true;
                AddProgress(petId, PetAIEvolutionType.DefenseExpert, 20f);
            }

            // Update progress for all evolutions
            UpdateEvolutionProgress(petId, won, damageDealt, damageTaken, healingDone);
            
            // Check for new evolution unlocks
            CheckEvolutionUnlock(petId);
            
            _playerEvolutionData.LastBattleTime = DateTime.Now;
            SaveData();
        }

        private void UpdateEvolutionProgress(string petId, bool won, float damageDealt, 
            float damageTaken, float healingDone)
        {
            var data = _playerEvolutionData.PetEvolutions[petId];
            
            // AggressionMaster progress
            float aggressionProgress = won ? damageDealt * 0.1f : damageDealt * 0.05f;
            AddProgress(petId, PetAIEvolutionType.AggressionMaster, aggressionProgress);
            
            // DefenseExpert progress
            float defenseProgress = damageTaken > 0 ? (1f - damageDealt / damageTaken) * 10f : 0f;
            AddProgress(petId, PetAIEvolutionType.DefenseExpert, defenseProgress);
            
            // SpeedDemon progress (based on quick kills)
            if (won && damageDealt > 100)
                AddProgress(petId, PetAIEvolutionType.SpeedDemon, 5f);
            
            // SupportMaster progress
            float supportProgress = healingDone * 0.2f;
            AddProgress(petId, PetAIEvolutionType.SupportMaster, supportProgress);
            
            // TacticalGenius progress (win with balanced stats)
            if (won && damageDealt > 0 && damageTaken > 0)
            {
                float ratio = damageDealt / damageTaken;
                if (ratio >= 0.8f && ratio <= 1.5f)
                    AddProgress(petId, PetAIEvolutionType.TacticalGenius, 10f);
            }
            
            // Survivalist progress (survive with low HP)
            if (won && data.BestSurvivalRate < 0.5f)
                AddProgress(petId, PetAIEvolutionType.Survivalist, 15f);
            
            // Berserker progress (win with high damage dealt)
            if (damageDealt > 500)
                AddProgress(petId, PetAIEvolutionType.Berserker, 10f);
            
            // Guardian progress (defend and protect)
            if (won && healingDone > 0)
                AddProgress(petId, PetAIEvolutionType.Guardian, 8f);
        }

        private void AddProgress(string petId, PetAIEvolutionType type, float amount)
        {
            var data = _playerEvolutionData.PetEvolutions[petId];
            
            if (!data.EvolutionProgress.ContainsKey(type))
                data.EvolutionProgress[type] = 0f;
            
            // Only add progress if not already unlocked
            if (!data.UnlockedEvolutions.Contains(type))
            {
                data.EvolutionProgress[type] += amount;
                ProgressUpdated?.Emit(petId, data.EvolutionProgress[type]);
            }
        }

        private void CheckEvolutionUnlock(string petId)
        {
            var data = _playerEvolutionData.PetEvolutions[petId];
            
            // Calculate best evolution based on stats
            var recommendedEvolution = PetAIEvolutionDatabase.CalculateBestEvolution(data);
            
            // Check if any evolution threshold is met
            foreach (var progress in data.EvolutionProgress)
            {
                if (data.UnlockedEvolutions.Contains(progress.Key))
                    continue;
                
                if (progress.Value >= PetAIEvolutionData.EvolutionThreshold)
                {
                    // Unlock the recommended evolution
                    if (progress.Key == recommendedEvolution || data.UnlockedEvolutions.Count >= 2)
                    {
                        UnlockEvolution(petId, progress.Key);
                    }
                }
            }
        }

        public void UnlockEvolution(string petId, PetAIEvolutionType type)
        {
            if (!_playerEvolutionData.PetEvolutions.ContainsKey(petId))
                return;

            var data = _playerEvolutionData.PetEvolutions[petId];
            
            if (!data.UnlockedEvolutions.Contains(type))
            {
                data.UnlockedEvolutions.Add(type);
                data.EvolutionProgress[type] = PetAIEvolutionData.EvolutionThreshold;
                data.LastEvolutionTime = DateTime.Now;
                _playerEvolutionData.TotalEvolutionsUnlocked++;
                
                EvolutionUnlocked?.Emit(petId, type);
            }
        }

        /// <summary>
        /// Get evolution bonuses based on current battle state
        /// </summary>
        public AIEvolutionBonus GetActiveBonus(string petId, float currentHp, float maxHp, int currentCombo)
        {
            if (!_playerEvolutionData.PetEvolutions.ContainsKey(petId))
                return null;

            var data = _playerEvolutionData.PetEvolutions[petId];
            float hpPercent = currentHp / Mathf.Max(maxHp, 1f);
            
            // Find the best matching evolution
            foreach (var unlockedType in data.UnlockedEvolutions)
            {
                var bonus = PetAIEvolutionDatabase.GetEvolution(unlockedType);
                if (bonus == null) continue;
                
                // Check activation conditions
                bool hpCondition = hpPercent <= bonus.ActivationHPThreshold;
                bool comboCondition = currentCombo >= bonus.ActivationComboThreshold;
                
                if (hpCondition || comboCondition)
                {
                    BonusActivated?.Emit(petId, unlockedType, hpPercent);
                    return bonus;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Record combo kill for bonus calculation
        /// </summary>
        public void RecordComboKill(string petId)
        {
            if (!_playerEvolutionData.PetEvolutions.ContainsKey(petId))
                InitializePetEvolution(petId);

            var data = _playerEvolutionData.PetEvolutions[petId];
            data.ComboKills++;
            data.HighestCombo = Mathf.Max(data.HighestCombo, data.ComboKills);
            
            ComboUpdated?.Emit(petId, data.ComboKills);
        }

        /// <summary>
        /// Reset combo when time passes without kill
        /// </summary>
        public void ResetCombo(string petId)
        {
            if (_playerEvolutionData.PetEvolutions.TryGetValue(petId, out var data))
            {
                data.ComboKills = 0;
            }
        }

        public PetAIEvolutionData GetEvolutionData(string petId)
        {
            if (_playerEvolutionData.PetEvolutions.TryGetValue(petId, out var data))
                return data;
            return null;
        }

        public List<PetAIEvolutionType> GetUnlockedEvolutions(string petId)
        {
            if (_playerEvolutionData.PetEvolutions.TryGetValue(petId, out var data))
                return data.UnlockedEvolutions;
            return new List<PetAIEvolutionType>();
        }

        public float GetEvolutionProgress(string petId, PetAIEvolutionType type)
        {
            if (_playerEvolutionData.PetEvolutions.TryGetValue(petId, out var data))
            {
                if (data.EvolutionProgress.TryGetValue(type, out var progress))
                    return progress;
            }
            return 0f;
        }

        public Dictionary<string, object> GetStatistics(string petId)
        {
            var stats = new Dictionary<string, object>();
            
            if (_playerEvolutionData.PetEvolutions.TryGetValue(petId, out var data))
            {
                stats["battles_fought"] = data.TotalBattlesFought;
                stats["battles_won"] = data.BattlesWon;
                stats["win_rate"] = data.TotalBattlesFought > 0 
                    ? (float)data.BattlesWon / data.TotalBattlesFought 
                    : 0f;
                stats["total_damage_dealt"] = data.TotalDamageDealt;
                stats["total_damage_taken"] = data.TotalDamageTaken;
                stats["total_healing_done"] = data.TotalHealingDone;
                stats["enemies_defeated"] = data.EnemiesDefeated;
                stats["best_survival_rate"] = data.BestSurvivalRate;
                stats["highest_combo"] = data.HighestCombo;
                stats["unlocked_evolution_count"] = data.UnlockedEvolutions.Count;
            }
            
            stats["total_evolution_unlocks"] = _playerEvolutionData.TotalEvolutionsUnlocked;
            
            return stats;
        }

        public void SaveData()
        {
            GameDataManager.SetData("pet_ai_evolution_data", _playerEvolutionData);
        }

        public void LoadData()
        {
            if (GameDataManager.HasData("pet_ai_evolution_data"))
            {
                _playerEvolutionData = GameDataManager.GetData<PlayerPetAIEvolutionData>("pet_ai_evolution_data");
            }
        }

        public void ResetEvolution(string petId)
        {
            if (_playerEvolutionData.PetEvolutions.ContainsKey(petId))
            {
                _playerEvolutionData.PetEvolutions[petId] = new PetAIEvolutionData
                {
                    PetId = petId,
                    LastEvolutionTime = DateTime.MinValue
                };
            }
        }
    }

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            if (_playerEvolutionData == null) return data;

            // 保存宠物进化数据
            var petEvolutionsData = new Dictionary<string, Dictionary<string, Variant>>();
            foreach (var kvp in _playerEvolutionData.PetEvolutions)
            {
                var petData = new Dictionary<string, Variant>();
                petData["pet_id"] = kvp.Value.PetId;
                petData["total_battles"] = kvp.Value.TotalBattlesFought;
                petData["battles_won"] = kvp.Value.BattlesWon;
                petData["total_damage_dealt"] = kvp.Value.TotalDamageDealt;
                petData["total_damage_taken"] = kvp.Value.TotalDamageTaken;
                petData["total_healing_done"] = kvp.Value.TotalHealingDone;
                petData["enemies_defeated"] = kvp.Value.EnemiesDefeated;
                petData["best_survival_rate"] = kvp.Value.BestSurvivalRate;
                petData["highest_combo"] = kvp.Value.HighestCombo;
                petData["has_learned_from_defeat"] = kvp.Value.HasLearnedFromDefeat;

                // 序列化解锁的进化类型
                var unlockedList = new List<int>();
                foreach (var evoType in kvp.Value.UnlockedEvolutions)
                {
                    unlockedList.Add((int)evoType);
                }
                petData["unlocked_evolution_types"] = unlockedList;

                // 序列化进化进度
                var progressDict = new Dictionary<string, float>();
                foreach (var progKvp in kvp.Value.EvolutionProgress)
                {
                    progressDict[progKvp.Key.ToString()] = progKvp.Value;
                }
                petData["evolution_progress"] = progressDict;

                petEvolutionsData[kvp.Key] = petData;
            }
            data["pet_evolutions"] = petEvolutionsData;

            data["total_evolutions_unlocked"] = _playerEvolutionData.TotalEvolutionsUnlocked;
            data["last_battle_time"] = _playerEvolutionData.LastBattleTime.ToString("o");

            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            if (data.TryGetValue("pet_evolutions", out var petEvolutionsData))
            {
                var petEvolutions = new Dictionary<string, PetAIEvolutionData>();
                var petDict = (Dictionary<string, Variant>)petEvolutionsData;

                foreach (var kvp in petDict)
                {
                    var petData = (Dictionary<string, Variant>)kvp.Value;
                    var evolutionData = new PetAIEvolutionData();

                    if (petData.TryGetValue("pet_id", out var petId))
                        evolutionData.PetId = (string)petId;
                    if (petData.TryGetValue("total_battles", out var totalBattles))
                        evolutionData.TotalBattlesFought = (int)totalBattles;
                    if (petData.TryGetValue("battles_won", out var battlesWon))
                        evolutionData.BattlesWon = (int)battlesWon;
                    if (petData.TryGetValue("total_damage_dealt", out var damageDealt))
                        evolutionData.TotalDamageDealt = (int)damageDealt;
                    if (petData.TryGetValue("total_damage_taken", out var damageTaken))
                        evolutionData.TotalDamageTaken = (int)damageTaken;
                    if (petData.TryGetValue("total_healing_done", out var healingDone))
                        evolutionData.TotalHealingDone = (int)healingDone;
                    if (petData.TryGetValue("enemies_defeated", out var enemiesDefeated))
                        evolutionData.EnemiesDefeated = (int)enemiesDefeated;
                    if (petData.TryGetValue("best_survival_rate", out var survivalRate))
                        evolutionData.BestSurvivalRate = (float)survivalRate;
                    if (petData.TryGetValue("highest_combo", out var highestCombo))
                        evolutionData.HighestCombo = (int)highestCombo;
                    if (petData.TryGetValue("has_learned_from_defeat", out var hasLearned))
                        evolutionData.HasLearnedFromDefeat = (bool)hasLearned;

                    // 反序列化解锁的进化类型
                    if (petData.TryGetValue("unlocked_evolution_types", out var unlockedData))
                    {
                        evolutionData.UnlockedEvolutions = new List<PetAIEvolutionType>();
                        var unlockedList = (List<Variant>)unlockedData;
                        foreach (var typeInt in unlockedList)
                        {
                            evolutionData.UnlockedEvolutions.Add((PetAIEvolutionType)(int)typeInt);
                        }
                    }

                    // 反序列化进化进度
                    if (petData.TryGetValue("evolution_progress", out var progressData))
                    {
                        evolutionData.EvolutionProgress = new Dictionary<PetAIEvolutionType, float>();
                        var progressDict = (Dictionary<string, Variant>)progressData;
                        foreach (var progKvp in progressDict)
                        {
                            if (Enum.TryParse<PetAIEvolutionType>(progKvp.Key, out var evoType))
                            {
                                evolutionData.EvolutionProgress[evoType] = (float)progKvp.Value;
                            }
                        }
                    }

                    petEvolutions[kvp.Key] = evolutionData;
                }

                _playerEvolutionData.PetEvolutions = petEvolutions;
            }

            if (data.TryGetValue("total_evolutions_unlocked", out var totalUnlocked))
                _playerEvolutionData.TotalEvolutionsUnlocked = (int)totalUnlocked;
            if (data.TryGetValue("last_battle_time", out var lastBattleTimeStr) && DateTime.TryParse((string)lastBattleTimeStr, out var parsedTime))
                _playerEvolutionData.LastBattleTime = parsedTime;
        }
}
