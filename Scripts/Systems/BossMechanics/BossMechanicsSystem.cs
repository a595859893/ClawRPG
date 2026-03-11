using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.BossMechanics {
    /// <summary>
    /// Boss 机制系统 - 管理 Boss 战斗中的阶段转换、狂暴和特殊机制
    /// </summary>
    public class BossMechanicsSystem {
        private static BossMechanicsSystem _instance;
        public static BossMechanicsSystem Instance {
            get {
                if (_instance == null) {
                    _instance = new BossMechanicsSystem();
                }
                return _instance;
            }
        }

        // 当前活跃的 Boss 战斗
        private Dictionary<string, ActiveBossFight> _activeBossFights = new Dictionary<string, ActiveBossFight>();
        
        // Boss 配置缓存
        private Dictionary<string, List<BossPhaseConfig>> _bossPhases = new Dictionary<string, List<BossPhaseConfig>>();
        private Dictionary<string, List<EnrageConfig>> _bossEnrages = new Dictionary<string, List<EnrageConfig>>();
        private Dictionary<string, List<BossSpecialMechanic>> _bossSpecialMechanics = new Dictionary<string, List<BossSpecialMechanic>>();
        
        // 玩家记录
        private Dictionary<string, PlayerBossRecord> _playerRecords = new Dictionary<string, PlayerBossRecord>();
        
        // 信号
        public static Signal0 BossPhaseChanged { get; private set; } = new Signal0();
        public static Signal1<string> BossEnraged { get; private set; } = new Signal1<string>();
        public static Signal1<string> BossSpecialMechanicTriggered { get; private set; } = new Signal1<string>();
        public static Signal1<Dictionary<string, Variant>> BossMechanicActivated { get; private set; } = new Signal1<Dictionary<string, Variant>>();

        private BossMechanicsSystem() {
            InitializeDatabase();
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private void InitializeDatabase() {
            var bossIds = BossMechanicsDatabase.GetAllConfiguredBossIds();
            foreach (var bossId in bossIds) {
                _bossPhases[bossId] = BossMechanicsDatabase.GetBossPhases(bossId);
                _bossEnrages[bossId] = BossMechanicsDatabase.GetBossEnrages(bossId);
                _bossSpecialMechanics[bossId] = BossMechanicsDatabase.GetBossSpecialMechanics(bossId);
            }
        }

        /// <summary>
        /// 开始 Boss 战斗
        /// </summary>
        public void StartBossFight(string bossId, string bossName, float maxHealth) {
            if (_activeBossFights.ContainsKey(bossId)) {
                // 已有战斗，先结束
                EndBossFight(bossId);
            }

            var fight = new ActiveBossFight {
                bossId = bossId,
                bossName = bossName,
                maxHealth = maxHealth,
                currentHealth = maxHealth,
                currentPhase = 0,
                timeInCombat = 0f,
                totalDamageDealt = 0f,
                totalDamageTaken = 0f,
                currentCombo = 0,
                isEnraged = false,
                isInvincible = false
            };

            // 初始化特殊机制冷却
            if (_bossSpecialMechanics.ContainsKey(bossId)) {
                foreach (var mechanic in _bossSpecialMechanics[bossId]) {
                    fight.mechanicCooldowns[mechanic.mechanicName] = 0f;
                }
            }

            _activeBossFights[bossId] = fight;
            
            // 初始化玩家记录
            InitializePlayerRecord(bossId);
        }

        /// <summary>
        /// 初始化玩家记录
        /// </summary>
        private void InitializePlayerRecord(string bossId) {
            if (!_playerRecords.ContainsKey(bossId)) {
                _playerRecords[bossId] = new PlayerBossRecord {
                    bossId = bossId,
                    timesFought = 0,
                    timesDefeated = 0,
                    bestTime = float.MaxValue,
                    totalDamageDealt = 0,
                    totalDamageTaken = 0,
                    bestCombo = 0
                };
            }
            _playerRecords[bossId].timesFought++;
        }

        /// <summary>
        /// 更新 Boss 战斗状态 (每帧调用)
        /// </summary>
        public void _Process(float delta) {
            var bossesToRemove = new List<string>();
            
            foreach (var fight in _activeBossFights.Values) {
                fight.timeInCombat += delta;
                
                // 更新特殊机制冷却
                foreach (var kvp in fight.mechanicCooldowns) {
                    if (fight.mechanicCooldowns[kvp.Key] > 0) {
                        fight.mechanicCooldowns[kvp.Key] -= delta;
                        if (fight.mechanicCooldowns[kvp.Key] < 0) {
                            fight.mechanicCooldowns[kvp.Key] = 0;
                        }
                    }
                }

                // 检查阶段转换
                CheckPhaseTransition(fight);
                
                // 检查狂暴触发
                CheckEnrageTriggers(fight);
                
                // 检查特殊机制触发
                CheckSpecialMechanics(fight);
                
                // 检查战斗是否结束
                if (fight.currentHealth <= 0) {
                    bossesToRemove.Add(fight.bossId);
                }
            }

            // 移除已结束的 Boss 战斗
            foreach (var bossId in bossesToRemove) {
                EndBossFight(bossId);
            }
        }

        /// <summary>
        /// 检查阶段转换
        /// </summary>
        private void CheckPhaseTransition(ActiveBossFight fight) {
            if (!_bossPhases.ContainsKey(fight.bossId)) return;
            
            var phases = _bossPhases[fight.bossId];
            float healthPercent = (fight.currentHealth / fight.maxHealth) * 100f;
            
            for (int i = fight.currentPhase + 1; i < phases.Count; i++) {
                if (healthPercent <= phases[i].healthPercent) {
                    // 进入新阶段
                    fight.currentPhase = i;
                    
                    // 应用阶段效果
                    ApplyPhaseEffects(fight, phases[i]);
                    
                    // 发送信号
                    var phaseData = new Dictionary<string, Variant> {
                        { "boss_id", fight.bossId },
                        { "boss_name", fight.bossName },
                        { "phase_index", i },
                        { "phase_name", phases[i].phaseName },
                        { "phase_type", (int)phases[i].phaseType },
                        { "damage_multiplier", phases[i].damageMultiplier },
                        { "speed_multiplier", phases[i].speedMultiplier },
                        { "show_warning", phases[i].showWarning },
                        { "warning_message", phases[i].warningMessage }
                    };
                    BossPhaseChanged.Emit();
                    BossMechanicActivated.Emit(phaseData);
                    
                    GD.Print($"[BossMechanics] {fight.bossName} 进入 {phases[i].phaseName}!");
                    break;
                }
            }
        }

        /// <summary>
        /// 应用阶段效果
        /// </summary>
        private void ApplyPhaseEffects(ActiveBossFight fight, BossPhaseConfig phase) {
            // 清除旧的效果
            fight.activeEffects.Clear();
            
            // 应用新的伤害/速度乘数
            // 注意: 实际应用需要与战斗系统集成
            // 这里只记录状态
            fight.activeEffects.Add($"damage_{phase.damageMultiplier}");
            fight.activeEffects.Add($"speed_{phase.speedMultiplier}");
            fight.activeEffects.Add($"attack_speed_{phase.attackSpeedMultiplier}");
        }

        /// <summary>
        /// 检查狂暴触发
        /// </summary>
        private void CheckEnrageTriggers(ActiveBossFight fight) {
            if (!_bossEnrages.ContainsKey(fight.bossId) || fight.isEnraged) return;
            
            var enrages = _bossEnrages[fight.bossId];
            float healthPercent = (fight.currentHealth / fight.maxHealth) * 100f;
            
            foreach (var enrage in enrages) {
                bool shouldEnrage = false;
                
                switch (enrage.triggerType) {
                    case EnrageTriggerType.TimeBased:
                        shouldEnrage = fight.timeInCombat >= enrage.triggerValue;
                        break;
                    case EnrageTriggerType.HealthBased:
                        shouldEnrage = healthPercent <= enrage.triggerValue;
                        break;
                    case EnrageTriggerType.DamageBased:
                        shouldEnrage = fight.totalDamageDealt >= enrage.triggerValue;
                        break;
                }
                
                if (shouldEnrage) {
                    TriggerEnrage(fight, enrage);
                    break;
                }
            }
        }

        /// <summary>
        /// 触发狂暴
        /// </summary>
        private void TriggerEnrage(ActiveBossFight fight, EnrageConfig enrage) {
            fight.isEnraged = true;
            
            // 应用狂暴效果
            fight.activeEffects.Add($"enrage_damage_{enrage.damageBonus}");
            fight.activeEffects.Add($"enrage_speed_{enrage.speedBonus}");
            
            // 发送信号
            var enrageData = new Dictionary<string, Variant> {
                { "boss_id", fight.bossId },
                { "boss_name", fight.bossName },
                { "trigger_name", enrage.triggerName },
                { "enrage_message", enrage.enrageMessage },
                { "damage_bonus", enrage.damageBonus },
                { "speed_bonus", enrage.speedBonus }
            };
            BossEnraged.Emit(enrage.enrageMessage);
            BossMechanicActivated.Emit(enrageData);
            
            GD.Print($"[BossMechanics] {fight.bossName} 狂暴了! {enrage.enrageMessage}");
        }

        /// <summary>
        /// 检查特殊机制触发
        /// </summary>
        private void CheckSpecialMechanics(ActiveBossFight fight) {
            if (!_bossSpecialMechanics.ContainsKey(fight.bossId)) return;
            
            var mechanics = _bossSpecialMechanics[fight.bossId];
            var random = new Random();
            
            foreach (var mechanic in mechanics) {
                // 检查冷却
                if (fight.mechanicCooldowns[mechanic.mechanicName] > 0) continue;
                
                // 检查触发几率
                if (random.NextDouble() > mechanic.triggerChance) continue;
                
                // 触发特殊机制
                TriggerSpecialMechanic(fight, mechanic);
                
                // 设置冷却
                fight.mechanicCooldowns[mechanic.mechanicName] = mechanic.cooldown;
            }
        }

        /// <summary>
        /// 触发特殊机制
        /// </summary>
        private void TriggerSpecialMechanic(ActiveBossFight fight, BossSpecialMechanic mechanic) {
            var mechanicData = new Dictionary<string, Variant> {
                { "boss_id", fight.bossId },
                { "boss_name", fight.bossName },
                { "mechanic_name", mechanic.mechanicName },
                { "description", mechanic.description },
                { "mechanic_type", (int)mechanic.mechanicType }
            };
            
            // 添加效果参数
            if (mechanic.effects != null) {
                foreach (var kvp in mechanic.effects) {
                    mechanicData[kvp.Key] = kvp.Value;
                }
            }
            
            BossSpecialMechanicTriggered.Emit(mechanic.mechanicName);
            BossMechanicActivated.Emit(mechanicData);
            
            GD.Print($"[BossMechanics] {fight.bossName} 使用 {mechanic.mechanicName}!");
        }

        /// <summary>
        /// Boss 受到伤害
        /// </summary>
        public void BossTakeDamage(string bossId, float damage) {
            if (!_activeBossFights.ContainsKey(bossId)) return;
            
            var fight = _activeBossFights[bossId];
            fight.currentHealth -= damage;
            if (fight.currentHealth < 0) fight.currentHealth = 0;
            
            // 更新玩家记录
            if (_playerRecords.ContainsKey(bossId)) {
                _playerRecords[bossId].totalDamageDealt += damage;
            }
        }

        /// <summary>
        /// Boss 造成伤害
        /// </summary>
        public void BossDealDamage(string bossId, float damage) {
            if (!_activeBossFights.ContainsKey(bossId)) return;
            
            var fight = _activeBossFights[bossId];
            fight.totalDamageTaken += damage;
            
            // 更新玩家记录
            if (_playerRecords.ContainsKey(bossId)) {
                _playerRecords[bossId].totalDamageTaken += damage;
            }
        }

        /// <summary>
        /// 增加连击数
        /// </summary>
        public void AddCombo(string bossId) {
            if (!_activeBossFights.ContainsKey(bossId)) return;
            
            var fight = _activeBossFights[bossId];
            fight.currentCombo++;
            
            // 更新最佳连击
            if (_playerRecords.ContainsKey(bossId)) {
                if (fight.currentCombo > _playerRecords[bossId].bestCombo) {
                    _playerRecords[bossId].bestCombo = fight.currentCombo;
                }
            }
        }

        /// <summary>
        /// 重置连击数
        /// </summary>
        public void ResetCombo(string bossId) {
            if (!_activeBossFights.ContainsKey(bossId)) return;
            _activeBossFights[bossId].currentCombo = 0;
        }

        /// <summary>
        /// 结束 Boss 战斗
        /// </summary>
        public void EndBossFight(string bossId) {
            if (!_activeBossFights.ContainsKey(bossId)) return;
            
            var fight = _activeBossFights[bossId];
            
            // 更新玩家记录
            if (_playerRecords.ContainsKey(bossId)) {
                var record = _playerRecords[bossId];
                if (fight.currentHealth <= 0) {
                    record.timesDefeated++;
                    if (fight.timeInCombat < record.bestTime) {
                        record.bestTime = fight.timeInCombat;
                    }
                }
                record.lastFightTime = DateTime.Now;
            }
            
            _activeBossFights.Remove(bossId);
        }

        /// <summary>
        /// 获取 Boss 当前战斗状态
        /// </summary>
        public ActiveBossFight GetBossFightStatus(string bossId) {
            if (_activeBossFights.ContainsKey(bossId)) {
                return _activeBossFights[bossId];
            }
            return null;
        }

        /// <summary>
        /// 获取当前阶段配置
        /// </summary>
        public BossPhaseConfig GetCurrentPhaseConfig(string bossId) {
            if (!_activeBossFights.ContainsKey(bossId) || !_bossPhases.ContainsKey(bossId)) {
                return null;
            }
            
            var fight = _activeBossFights[bossId];
            var phases = _bossPhases[bossId];
            
            if (fight.currentPhase < phases.Count) {
                return phases[fight.currentPhase];
            }
            return null;
        }

        /// <summary>
        /// 获取玩家记录
        /// </summary>
        public PlayerBossRecord GetPlayerRecord(string bossId) {
            if (_playerRecords.ContainsKey(bossId)) {
                return _playerRecords[bossId];
            }
            return null;
        }

        /// <summary>
        /// 获取所有玩家记录
        /// </summary>
        public Dictionary<string, PlayerBossRecord> GetAllPlayerRecords() {
            return new Dictionary<string, PlayerBossRecord>(_playerRecords);
        }

        /// <summary>
        /// 获取伤害乘数
        /// </summary>
        public float GetDamageMultiplier(string bossId) {
            var phase = GetCurrentPhaseConfig(bossId);
            if (phase == null) return 1f;
            
            float multiplier = phase.damageMultiplier;
            
            // 如果狂暴了，添加额外加成
            if (_activeBossFights.ContainsKey(bossId) && _activeBossFights[bossId].isEnraged) {
                if (_bossEnrages.ContainsKey(bossId)) {
                    foreach (var enrage in _bossEnrages[bossId]) {
                        if (_activeBossFights[bossId].activeEffects.Exists(e => e.Contains("enrage_damage"))) {
                            multiplier += enrage.damageBonus;
                            break;
                        }
                    }
                }
            }
            
            return multiplier;
        }

        /// <summary>
        /// 获取速度乘数
        /// </summary>
        public float GetSpeedMultiplier(string bossId) {
            var phase = GetCurrentPhaseConfig(bossId);
            if (phase == null) return 1f;
            
            float multiplier = phase.speedMultiplier;
            
            // 如果狂暴了，添加额外加成
            if (_activeBossFights.ContainsKey(bossId) && _activeBossFights[bossId].isEnraged) {
                if (_bossEnrages.ContainsKey(bossId)) {
                    foreach (var enrage in _bossEnrages[bossId]) {
                        if (_activeBossFights[bossId].activeEffects.Exists(e => e.Contains("enrage_speed"))) {
                            multiplier += enrage.speedBonus;
                            break;
                        }
                    }
                }
            }
            
            return multiplier;
        }

        /// <summary>
        /// 存档数据
        /// </summary>
        public Dictionary<string, Variant> GetSaveData() {
            var data = new Dictionary<string, Variant>();
            
            // 玩家记录
            var records = new Dictionary<string, Dictionary<string, Variant>>();
            foreach (var kvp in _playerRecords) {
                records[kvp.Key] = new Dictionary<string, Variant> {
                    { "boss_id", kvp.Value.bossId },
                    { "times_fought", kvp.Value.timesFought },
                    { "times_defeated", kvp.Value.timesDefeated },
                    { "best_time", kvp.Value.bestTime },
                    { "total_damage_dealt", kvp.Value.totalDamageDealt },
                    { "total_damage_taken", kvp.Value.totalDamageTaken },
                    { "best_combo", kvp.Value.bestCombo }
                };
            }
            data["player_records"] = records;
            
            return data;
        }

        /// <summary>
        /// 加载存档
        /// </summary>
        public void LoadSaveData(Dictionary<string, Variant> data) {
            if (!data.ContainsKey("player_records")) return;
            
            var records = (Dictionary<string, Dictionary<string, Variant>>)data["player_records"];
            _playerRecords.Clear();
            
            foreach (var kvp in records) {
                var record = new PlayerBossRecord {
                    bossId = (string)kvp.Value["boss_id"],
                    timesFought = (int)kvp.Value["times_fought"],
                    timesDefeated = (int)kvp.Value["times_defeated"],
                    bestTime = (float)kvp.Value["best_time"],
                    totalDamageDealt = (float)kvp.Value["total_damage_dealt"],
                    totalDamageTaken = (float)kvp.Value["total_damage_taken"],
                    bestCombo = (int)kvp.Value["best_combo"]
                };
                _playerRecords[kvp.Key] = record;
            }
        }
    }
}
