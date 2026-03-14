// BossMechanicsSystem.cs - Boss 机制系统核心
using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.BossMechanics;

namespace ClawRPG.Scripts.BossMechanics {
    
    public class BossMechanicsSystem : Node {
        
        private static BossMechanicsSystem _instance;
        public static BossMechanicsSystem Instance {
            get { return _instance; }
        }
        
        // 当前战斗状态
        private BossState _currentBoss;
        private BossBattleRecord _currentBattle;
        private BossAIConfig _currentAIConfig;
        
        // 系统数据
        private BossStatistics _statistics;
        private BossProgress _progress;
        
        // 技能冷却
        private Dictionary<string, float> _skillCooldowns;
        
        // 战斗计时
        private float _battleTimer;
        private float _phaseTimer;
        
        // 信号
        [Signal]
        public delegate void BossDamaged(float damage, string damageType);
        
        [Signal]
        public delegate void PhaseChanged(int newPhase, BattlePhaseType phaseType);
        
        [Signal]
        public delegate void BossDefeated(string bossId, bool isVictory, int starsEarned);
        
        [Signal]
        public delegate void SkillUsed(string skillId, string skillName);
        
        [Signal]
        public delegate void LootDropped(string lootId, string itemName, int quantity);
        
        [Signal]
        public delegate void EnrageActivated();
        
        [Signal]
        public delegate void BattleStarted(string bossId, string bossName);
        
        public override void _Ready() {
            base._Ready();
            _instance = this;
            
            _skillCooldowns = new Dictionary<string, float>();
            _statistics = new BossStatistics();
            _progress = new BossProgress();
            
            LoadData();
        }
        
        public override void _Process(float delta) {
            base._Process(delta);
            
            if (_currentBoss != null && _currentBoss.IsAlive && _currentBattle != null) {
                _battleTimer += delta;
                _phaseTimer += delta;
                
                // 更新技能冷却
                UpdateSkillCooldowns(delta);
                
                // 检查狂暴计时器
                if (_currentBoss.EnrageTimer > 0) {
                    _currentBoss.EnrageTimer -= delta;
                    if (_currentBoss.EnrageTimer <= 0 && !_currentBoss.IsEnraged) {
                        ActivateEnrage();
                    }
                }
                
                // AI 决策
                ProcessAI(delta);
                
                // 检查阶段转换
                CheckPhaseTransition();
            }
        }
        
        // 开始 Boss 战斗
        public void StartBattle(string bossId) {
            var db = BossMechanicsDatabase.Instance;
            _currentBoss = db.GetBossConfig(bossId);
            
            if (_currentBoss == null) {
                GD.PrintErr($"Boss not found: {bossId}");
                return;
            }
            
            // 获取 AI 配置
            var bossConfig = db.BossConfigs[bossId];
            _currentAIConfig = db.AIBehaviorConfigs[BossAIBehavior.Balanced];
            
            // 初始化战斗记录
            _currentBattle = new BossBattleRecord {
                RecordId = Guid.NewGuid().ToString(),
                BossId = bossId,
                BossName = _currentBoss.BossName,
                StartTime = DateTime.Now,
                IsVictory = false,
                PhaseReached = 1
            };
            
            // 初始化技能冷却
            _skillCooldowns.Clear();
            var phases = db.GetBossPhases(bossId);
            foreach (var phase in phases) {
                foreach (var skillId in phase.UnlockedSkills) {
                    _skillCooldowns[skillId] = 0;
                }
            }
            
            _battleTimer = 0;
            _phaseTimer = 0;
            
            // 解锁 Boss
            if (!_progress.UnlockedBosses.ContainsKey(bossId)) {
                _progress.UnlockedBosses[bossId] = true;
            }
            
            EmitSignal(nameof(BattleStarted), bossId, _currentBoss.BossName);
            GD.Print($"Boss battle started: {_currentBoss.BossName}");
        }
        
        // 玩家攻击 Boss
        public void PlayerAttackBoss(float damage, string damageType = "physical") {
            if (_currentBoss == null || !_currentBoss.IsAlive) return;
            
            // 计算实际伤害
            float defenseReduction = _currentBoss.Defense * 0.1f;
            float actualDamage = Math.Max(1, damage - defenseReduction);
            
            _currentBoss.CurrentHealth -= actualDamage;
            _currentBattle.DamageDealt += (int)actualDamage;
            
            EmitSignal(nameof(BossDamaged), actualDamage, damageType);
            
            // 检查 Boss 是否死亡
            if (!_currentBoss.IsAlive) {
                EndBattle(true);
            }
        }
        
        // Boss 攻击玩家
        public void BossAttackPlayer(float damage) {
            if (_currentBoss == null || !_currentBoss.IsAlive) return;
            
            _currentBattle.DamageTaken += (int)damage;
            _currentBoss.TotalDamageDealt += (int)damage;
        }
        
        // 使用 Boss 技能
        public void UseSkill(string skillId) {
            if (_currentBoss == null || !_currentBoss.IsAlive) return;
            
            var db = BossMechanicsDatabase.Instance;
            var skill = db.GetSkill(skillId);
            
            if (skill == null) return;
            
            // 检查冷却
            if (_skillCooldowns.ContainsKey(skillId) && _skillCooldowns[skillId] > 0) return;
            
            // 检查技能是否在当前阶段解锁
            var phases = db.GetBossPhases(_currentBoss.BossId);
            bool skillUnlocked = false;
            foreach (var phase in phases) {
                if (phase.PhaseNumber <= _currentBoss.CurrentPhase && 
                    phase.UnlockedSkills.Contains(skillId)) {
                    skillUnlocked = true;
                    break;
                }
            }
            
            if (!skillUnlocked) return;
            
            // 设置冷却
            _skillCooldowns[skillId] = skill.Cooldown;
            
            // 记录使用的技能
            _currentBattle.SkillsUsed.Add(skillId);
            
            EmitSignal(nameof(SkillUsed), skillId, skill.SkillName);
            GD.Print($"Boss used skill: {skill.SkillName}");
        }
        
        // 更新技能冷却
        private void UpdateSkillCooldowns(float delta) {
            foreach (var key in _skillCooldowns.Keys) {
                if (_skillCooldowns[key] > 0) {
                    _skillCooldowns[key] -= delta;
                }
            }
        }
        
        // AI 决策处理
        private void ProcessAI(float delta) {
            if (_currentBoss == null || _currentAIConfig == null) return;
            
            // 根据 AI 行为类型做出决策
            var db = BossMechanicsDatabase.Instance;
            var phases = db.GetBossPhases(_currentBoss.BossId);
            
            BattlePhase currentPhase = null;
            foreach (var phase in phases) {
                if (phase.PhaseNumber == _currentBoss.CurrentPhase) {
                    currentPhase = phase;
                    break;
                }
            }
            
            if (currentPhase == null) return;
            
            // 随机选择可用技能
            var availableSkills = new List<string>();
            foreach (var skillId in currentPhase.UnlockedSkills) {
                if (_skillCooldowns.ContainsKey(skillId) && _skillCooldowns[skillId] <= 0) {
                    var skill = db.GetSkill(skillId);
                    if (skill != null) {
                        // 根据权重随机选择
                        for (int i = 0; i < (int)(skill.Weight * 100); i++) {
                            availableSkills.Add(skillId);
                        }
                    }
                }
            }
            
            if (availableSkills.Count > 0 && GD.Randf() < _currentAIConfig.SkillUsageRate * delta) {
                var randomIndex = (int)(GD.Randf() * availableSkills.Count);
                UseSkill(availableSkills[randomIndex]);
            }
            
            // 检查是否需要治疗
            if (_currentBoss.CurrentHealth / _currentBoss.MaxHealth < _currentAIConfig.PriorityHealThreshold) {
                UseSkill("self_heal");
            }
            
            // 检查是否使用增益
            if (GD.Randf() < 0.01f * delta) {
                UseSkill("power_up");
            }
        }
        
        // 检查阶段转换
        private void CheckPhaseTransition() {
            if (_currentBoss == null) return;
            
            var db = BossMechanicsDatabase.Instance;
            var phases = db.GetBossPhases(_currentBoss.BossId);
            
            foreach (var phase in phases) {
                if (phase.PhaseNumber > _currentBoss.CurrentPhase) {
                    if (_currentBoss.HealthPercentage <= phase.HealthThreshold) {
                        _currentBoss.CurrentPhase = phase.PhaseNumber;
                        _phaseTimer = 0;
                        
                        // 应用阶段效果
                        _currentBoss.AttackDamage *= phase.DamageMultiplier;
                        _currentBoss.Defense *= phase.DefenseMultiplier;
                        _currentBoss.MoveSpeed *= phase.SpeedMultiplier;
                        
                        _currentBattle.PhaseReached = phase.PhaseNumber;
                        
                        EmitSignal(nameof(PhaseChanged), phase.PhaseNumber, phase.Type);
                        GD.Print($"Boss entered phase: {phase.PhaseName}");
                        
                        break;
                    }
                }
            }
        }
        
        // 激活狂暴
        private void ActivateEnrage() {
            if (_currentBoss == null) return;
            
            _currentBoss.IsEnraged = true;
            _currentBoss.AttackDamage *= 1.5f;
            
            EmitSignal(nameof(EnrageActivated));
            GD.Print("Boss is enraged!");
        }
        
        // 结束战斗
        private void EndBattle(bool isVictory) {
            if (_currentBattle == null) return;
            
            _currentBattle.EndTime = DateTime.Now;
            _currentBattle.IsVictory = isVictory;
            
            // 计算星级
            int stars = CalculateStars();
            _currentBattle.StarsEarned = stars;
            
            // 更新统计
            _statistics.TotalBattles++;
            if (isVictory) {
                _statistics.Victories++;
                if (_statistics.BossKills.ContainsKey(_currentBoss.BossId)) {
                    _statistics.BossKills[_currentBoss.BossId]++;
                } else {
                    _statistics.BossKills[_currentBoss.BossId] = 1;
                }
                
                // 生成掉落
                GenerateLoot();
            } else {
                _statistics.Defeats++;
                if (_statistics.BossDeaths.ContainsKey(_currentBoss.BossId)) {
                    _statistics.BossDeaths[_currentBoss.BossId]++;
                } else {
                    _statistics.BossDeaths[_currentBoss.BossId] = 1;
                }
            }
            
            _statistics.TotalDamageDealt += _currentBattle.DamageDealt;
            _statistics.TotalDamageTaken += _currentBattle.DamageTaken;
            
            // 更新进度
            if (_progress.BestPhases.ContainsKey(_currentBoss.BossId)) {
                if (_currentBattle.PhaseReached > _progress.BestPhases[_currentBoss.BossId]) {
                    _progress.BestPhases[_currentBoss.BossId] = _currentBattle.PhaseReached;
                }
            } else {
                _progress.BestPhases[_currentBoss.BossId] = _currentBattle.PhaseReached;
            }
            
            if (_progress.BestStars.ContainsKey(_currentBoss.BossId)) {
                if (stars > _progress.BestStars[_currentBoss.BossId]) {
                    _progress.BestStars[_currentBoss.BossId] = stars;
                }
            } else {
                _progress.BestStars[_currentBoss.BossId] = stars;
            }
            
            if (_currentBattle.PhaseReached > _statistics.HighestPhaseReached) {
                _statistics.HighestPhaseReached = _currentBattle.PhaseReached;
            }
            
            // 发送信号
            EmitSignal(nameof(BossDefeated), _currentBoss.BossId, isVictory, stars);
            
            // 保存数据
            SaveData();
            
            GD.Print($"Boss battle ended: {(isVictory ? "Victory" : "Defeat")} - Stars: {stars}");
            
            // 清理
            _currentBoss = null;
            _currentBattle = null;
        }
        
        // 计算星级
        private int CalculateStars() {
            if (_currentBattle == null || _currentBoss == null) return 0;
            
            int stars = 0;
            
            // 胜利获得 1 星
            if (_currentBattle.IsVictory) {
                stars = 1;
                
                // 无死亡额外 1 星
                if (_currentBattle.DamageTaken == 0) {
                    stars++;
                }
                
                // 达到最高阶段额外 1 星
                var db = BossMechanicsDatabase.Instance;
                var phases = db.GetBossPhases(_currentBoss.BossId);
                if (_currentBattle.PhaseReached >= phases.Count) {
                    stars++;
                }
            }
            
            return Math.Min(3, stars);
        }
        
        // 生成掉落
        private void GenerateLoot() {
            if (_currentBoss == null) return;
            
            var db = BossMechanicsDatabase.Instance;
            var lootTable = db.GetBossLootTable(_currentBoss.BossId);
            
            foreach (var loot in lootTable) {
                bool dropped = false;
                
                if (loot.IsGuaranteed) {
                    dropped = true;
                } else {
                    if (GD.Randf() < loot.DropRate) {
                        dropped = true;
                    }
                }
                
                if (dropped) {
                    int quantity = (int)(GD.Randf() * (loot.MaxQuantity - loot.MinQuantity + 1)) + loot.MinQuantity;
                    
                    _currentBattle.LootReceived.Add($"{loot.ItemName} x{quantity}");
                    _statistics.TotalLootCollected += quantity;
                    
                    EmitSignal(nameof(LootDropped), loot.LootId, loot.ItemName, quantity);
                    GD.Print($"Loot dropped: {loot.ItemName} x{quantity}");
                }
            }
        }
        
        // 获取当前 Boss 状态
        public BossState GetCurrentBoss() {
            return _currentBoss;
        }
        
        // 获取当前战斗记录
        public BossBattleRecord GetCurrentBattle() {
            return _currentBattle;
        }
        
        // 获取统计信息
        public BossStatistics GetStatistics() {
            return _statistics;
        }
        
        // 获取进度
        public BossProgress GetProgress() {
            return _progress;
        }
        
        // 获取所有可用的 Boss
        public List<string> GetAvailableBosses() {
            return BossMechanicsDatabase.Instance.GetAllBossIds();
        }
        
        // 获取技能冷却
        public float GetSkillCooldown(string skillId) {
            if (_skillCooldowns.ContainsKey(skillId)) {
                return _skillCooldowns[skillId];
            }
            return 0;
        }
        
        // 获取战斗时间
        public float GetBattleTime() {
            return _battleTimer;
        }
        
        // 获取战斗阶段
        public int GetCurrentPhase() {
            if (_currentBoss != null) {
                return _currentBoss.CurrentPhase;
            }
            return 0;
        }
        
        // 存档数据
        public Dictionary<string, object> GetSaveData() {
            var data = new Dictionary<string, object>();
            
            data["statistics"] = _statistics;
            data["progress"] = _progress;
            
            return data;
        }
        
        // 加载数据
        public void LoadData() {
            // 从存档加载统计数据和进度
            // 实际实现需要与游戏存档系统集成
            if (HasNode("/root/SaveSystem")) {
                var saveSystem = GetNode("/root/SaveSystem");
                // 存档加载功能待与SaveSystem集成后实现
            }
        }
        
        // 保存数据
        public void SaveData() {
            // 保存统计数据和进度到存档
            // 实际实现需要与游戏存档系统集成
            if (HasNode("/root/SaveSystem")) {
                var saveSystem = GetNode("/root/SaveSystem");
                // 存档保存功能待与SaveSystem集成后实现
            }
        }
        
        // 放弃战斗
        public void ForfeitBattle() {
            if (_currentBattle != null) {
                EndBattle(false);
            }
        }
    }
}
