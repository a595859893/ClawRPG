using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.MountBattle {
    /// <summary>
    /// 坐骑战斗系统 - Mount Battle System
    /// 允许玩家在坐骑上进行战斗，参与各种战斗模式
    /// </summary>
    public class MountBattleSystem : BaseSystem {
        private MountBattleData _data;
        private MountBattleState _currentState = MountBattleState.Idle;
        private float _skillCooldownTimer = 0f;
        private Dictionary<string, float> _skillCooldowns = new Dictionary<string, float>();
        private Dictionary<string, float> _activeEffects = new Dictionary<string, float>();
        
        // 战斗属性
        private float _currentMountHealth = 100f;
        private float _maxMountHealth = 100f;
        private float _currentMountMana = 100f;
        private float _maxMountMana = 100f;
        
        // 战斗事件
        public event Action<MountBattleRecord> OnBattleEnd;
        public event Action<float, float> OnHealthChange;
        public event Action<float, float> OnManaChange;
        public event Action<string> OnSkillUsed;
        public event Action<int> OnLevelUp;
        public event Action<string> OnRankChange;
        
        public override void _Ready() {
            InitializeData();
        }
        
        private void InitializeData() {
            _data = new MountBattleData();
            // 初始化技能冷却
            foreach (var skill in MountBattleDatabase.MountSkills.Values) {
                _skillCooldowns[skill.Id] = 0f;
            }
        }
        
        /// <summary>
        /// 启用坐骑战斗模式
        /// </summary>
        public void EnableMountBattle() {
            _data.IsMountBattleEnabled = true;
            _currentState = MountBattleState.Idle;
            GD.Print("[MountBattle] 坐骑战斗模式已启用");
        }
        
        /// <summary>
        /// 禁用坐骑战斗模式
        /// </summary>
        public void DisableMountBattle() {
            _data.IsMountBattleEnabled = false;
            _currentState = MountBattleState.Idle;
            GD.Print("[MountBattle] 坐骑战斗模式已禁用");
        }
        
        /// <summary>
        /// 开始一场坐骑战斗
        /// </summary>
        public void StartBattle(MountBattleType battleType) {
            if (!_data.IsMountBattleEnabled) {
                GD.Print("[MountBattle] 请先启用坐骑战斗模式");
                return;
            }
            
            _currentState = MountBattleState.Preparing;
            _currentMountHealth = GetMaxMountHealth();
            _currentMountMana = GetMaxMountMana();
            
            GD.Print($"[MountBattle] 开始{battleType}战斗");
        }
        
        /// <summary>
        /// 使用坐骑战斗技能
        /// </summary>
        public bool UseSkill(string skillId, float baseDamage = 0) {
            if (_currentState != MountBattleState.InBattle) {
                return false;
            }
            
            if (!_data.UnlockedMountSkills.Contains(skillId)) {
                GD.Print($"[MountBattle] 技能 {skillId} 未解锁");
                return false;
            }
            
            // 检查冷却
            if (_skillCooldowns.ContainsKey(skillId) && _skillCooldowns[skillId] > 0) {
                GD.Print($"[MountBattle] 技能 {skillId} 冷却中");
                return false;
            }
            
            // 检查魔法值
            var skill = MountBattleDatabase.MountSkills[skillId];
            if (_currentMountMana < skill.ManaCost) {
                GD.Print($"[MountBattle] 魔法值不足");
                return false;
            }
            
            // 使用魔法值
            _currentMountMana -= skill.ManaCost;
            OnManaChange?.Invoke(_currentMountMana, _maxMountMana);
            
            // 应用技能效果
            float actualDamage = ApplySkillEffect(skill, baseDamage);
            
            // 设置冷却
            _skillCooldowns[skillId] = skill.Cooldown;
            
            OnSkillUsed?.Invoke(skillId);
            GD.Print($"[MountBattle] 使用技能 {skill.Name}, 伤害: {actualDamage}");
            
            return true;
        }
        
        private float ApplySkillEffect(MountBattleSkill skill, float baseDamage) {
            float result = 0f;
            
            // 获取技能等级
            int skillLevel = _data.SkillLevels.ContainsKey(skill.Id) ? _data.SkillLevels[skill.Id] : 1;
            float scaling = 1f + (skillLevel - 1) * skill.ScalingPerLevel;
            
            // 伤害技能
            if (skill.BaseDamage > 0) {
                result = baseDamage * skill.BaseDamage * scaling / 100f;
                
                // 忽略防御
                if (skill.IgnoreDefense > 0) {
                    // 忽略防御的逻辑在伤害计算中应用
                }
                
                // 区域效果
                if (skill.AreaEffect) {
                    GD.Print($"[MountBattle] 区域伤害 {result} 半径 {skill.EffectRadius}");
                }
                
                _data.TotalMountDamageDealt += (int)result;
            }
            
            // 治疗技能
            if (skill.BaseHeal > 0) {
                float healAmount = skill.BaseHeal * scaling;
                _currentMountHealth = Mathf.Min(_currentMountHealth + healAmount, _maxMountHealth);
                OnHealthChange?.Invoke(_currentMountHealth, _maxMountHealth);
                result = -healAmount; // 负数表示治疗
            }
            
            // 护盾技能
            if (skill.BaseShield > 0) {
                float shieldAmount = skill.BaseShield * scaling;
                _activeEffects["shield"] = _activeEffects.GetValueOrDefault("shield", 0) + shieldAmount;
                result = shieldAmount;
            }
            
            // 格挡
            if (skill.DamageReduction > 0) {
                _activeEffects["block"] = skill.DamageReduction;
                _activeEffects["block_duration"] = 1.5f; // 格挡持续1.5秒
            }
            
            // 闪避
            if (skill.DodgeChance > 0) {
                _activeEffects["dodge"] = skill.DodgeChance;
            }
            
            // 速度加成
            if (skill.SpeedBoost > 0 || skill.AttackSpeedBoost > 0) {
                _activeEffects["speed_boost"] = skill.SpeedBoost;
                _activeEffects["attack_speed_boost"] = skill.AttackSpeedBoost;
                _activeEffects["speed_duration"] = skill.Duration;
            }
            
            return result;
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage) {
            float finalDamage = damage;
            
            // 应用格挡
            if (_activeEffects.ContainsKey("block") && _activeEffects["block_duration"] > 0) {
                finalDamage *= (1f - _activeEffects["block"]);
            }
            
            // 应用护盾
            if (_activeEffects.ContainsKey("shield") && _activeEffects["shield"] > 0) {
                float shield = _activeEffects["shield"];
                if (shield >= finalDamage) {
                    _activeEffects["shield"] -= finalDamage;
                    finalDamage = 0;
                } else {
                    finalDamage -= shield;
                    _activeEffects["shield"] = 0;
                }
            }
            
            _currentMountHealth -= finalDamage;
            _data.TotalMountDamageTaken += (int)finalDamage;
            OnHealthChange?.Invoke(_currentMountHealth, _maxMountHealth);
            
            if (_currentMountHealth <= 0) {
                EndBattle(false);
            }
        }
        
        /// <summary>
        /// 造成伤害（包含各种加成计算）
        /// </summary>
        public float DealDamage(float baseDamage) {
            float finalDamage = baseDamage;
            
            // 坐骑类型加成
            string mountType = GetCurrentMountType();
            if (MountBattleDatabase.MountTypeBonuses.ContainsKey(mountType)) {
                var bonuses = MountBattleDatabase.MountTypeBonuses[mountType];
                finalDamage *= (1f + bonuses.AttackBonus);
            }
            
            // 技能速度加成
            if (_activeEffects.ContainsKey("attack_speed_boost")) {
                finalDamage *= (1f + _activeEffects["attack_speed_boost"]);
            }
            
            _data.TotalMountDamageDealt += (int)finalDamage;
            return finalDamage;
        }
        
        /// <summary>
        /// 结束战斗
        /// </summary>
        public void EndBattle(bool victory) {
            _currentState = victory ? MountBattleState.Victory : MountBattleState.Defeated;
            
            var record = new MountBattleRecord {
                Victory = victory,
                DamageDealt = _data.TotalMountDamageDealt,
                DamageTaken = _data.TotalMountDamageTaken,
                EarnedPoints = CalculateEarnedPoints(victory),
                EarnedExp = CalculateEarnedExp(victory)
            };
            
            // 更新统计数据
            if (victory) {
                _data.Wins++;
                _data.CurrentStreak++;
                if (_data.CurrentStreak > _data.BestStreak) {
                    _data.BestStreak = _data.CurrentStreak;
                }
                // 连胜加成
                record.EarnedPoints += _data.CurrentStreak * 5;
            } else {
                _data.Losses++;
                _data.CurrentStreak = 0;
            }
            
            // 更新经验
            int oldLevel = _data.CurrentMountCombatLevel;
            _data.TotalMountKills++;
            CheckLevelUp(record.EarnedExp);
            
            // 更新段位
            CheckRankChange();
            
            // 添加战斗记录
            _data.BattleHistory.Add(record);
            if (_data.BattleHistory.Count > 100) {
                _data.BattleHistory.RemoveAt(0);
            }
            
            OnBattleEnd?.Invoke(record);
            _currentState = MountBattleState.Idle;
            
            GD.Print($"[MountBattle] 战斗结束: {(victory ? "胜利" : "失败")}, 获得 {record.EarnedPoints} 点, {record.EarnedExp} 经验");
        }
        
        private int CalculateEarnedPoints(bool victory) {
            if (!victory) return 5;
            
            int basePoints = 20;
            int streakBonus = Mathf.Min(_data.CurrentStreak * 5, 50);
            return basePoints + streakBonus;
        }
        
        private int CalculateEarnedExp(bool victory) {
            if (!victory) return 10;
            return 50 + _data.CurrentMountCombatLevel * 5;
        }
        
        private void CheckLevelUp(int expGained) {
            int currentExp = GetCurrentExp();
            int requiredExp = GetRequiredExp(_data.CurrentMountCombatLevel);
            
            while (currentExp >= requiredExp && _data.CurrentMountCombatLevel < 20) {
                _data.CurrentMountCombatLevel++;
                currentExp -= requiredExp;
                requiredExp = GetRequiredExp(_data.CurrentMountCombatLevel);
                
                // 解锁新技能
                UnlockSkillsForLevel(_data.CurrentMountCombatLevel);
                
                OnLevelUp?.Invoke(_data.CurrentMountCombatLevel);
                GD.Print($"[MountBattle] 坐骑战斗等级提升到 {_data.CurrentMountCombatLevel}");
            }
        }
        
        private void UnlockSkillsForLevel(int level) {
            foreach (var skill in MountBattleDatabase.MountSkills.Values) {
                if (skill.UnlockLevel == level && !_data.UnlockedMountSkills.Contains(skill.Id)) {
                    _data.UnlockedMountSkills.Add(skill.Id);
                    _data.SkillLevels[skill.Id] = 1;
                    GD.Print($"[MountBattle] 解锁新技能: {skill.Name}");
                }
            }
        }
        
        private void CheckRankChange() {
            _data.SeasonPoints += _data.Wins > 0 ? 20 : 0;
            
            string newRank = "Bronze";
            foreach (var rank in MountBattleDatabase.Ranks) {
                if (_data.SeasonPoints >= rank.Value.MinPoints) {
                    newRank = rank.Key;
                }
            }
            
            if (newRank != _data.SeasonRank) {
                _data.SeasonRank = newRank;
                OnRankChange?.Invoke(newRank);
                GD.Print($"[MountBattle] 段位提升到 {newRank}");
            }
        }
        
        /// <summary>
        /// 获取最大生命值
        /// </summary>
        public float GetMaxMountHealth() {
            float baseHealth = 100f;
            float levelBonus = (_data.CurrentMountCombatLevel - 1) * 10f;
            
            // 坐骑类型加成
            string mountType = GetCurrentMountType();
            if (MountBattleDatabase.MountTypeBonuses.ContainsKey(mountType)) {
                var bonuses = MountBattleDatabase.MountTypeBonuses[mountType];
                levelBonus *= (1f + bonuses.HealthBonus);
            }
            
            return baseHealth + levelBonus;
        }
        
        /// <summary>
        /// 获取最大魔法值
        /// </summary>
        public float GetMaxMountMana() {
            return 100f + (_data.CurrentMountCombatLevel - 1) * 5f;
        }
        
        /// <summary>
        /// 获取当前生命值
        /// </summary>
        public float GetCurrentMountHealth() => _currentMountHealth;
        
        /// <summary>
        /// 获取当前魔法值
        /// </summary>
        public float GetCurrentMountMana() => _currentMountMana;
        
        /// <summary>
        /// 获取当前坐骑类型
        /// </summary>
        public string GetCurrentMountType() {
            // 从坐骑系统获取当前坐骑类型
            return "horse"; // 默认
        }
        
        /// <summary>
        /// 获取当前等级经验
        /// </summary>
        public int GetCurrentExp() {
            // 简化实现
            return 0;
        }
        
        /// <summary>
        /// 获取升级所需经验
        /// </summary>
        public int GetRequiredExp(int level) {
            return MountBattleDatabase.LevelExpRequirements.GetValueOrDefault(level, 10000);
        }
        
        /// <summary>
        /// 获取战斗统计
        /// </summary>
        public MountBattleData GetData() => _data;
        
        /// <summary>
        /// 获取当前状态
        /// </summary>
        public MountBattleState GetCurrentState() => _currentState;
        
        /// <summary>
        /// 获取战斗属性加成
        /// </summary>
        public MountCombatStats GetCombatStats() {
            var stats = new MountCombatStats();
            
            string mountType = GetCurrentMountType();
            if (MountBattleDatabase.MountTypeBonuses.ContainsKey(mountType)) {
                var bonuses = MountBattleDatabase.MountTypeBonuses[mountType];
                stats.AttackBonus += bonuses.AttackBonus;
                stats.DefenseBonus += bonuses.DefenseBonus;
                stats.SpeedBonus += bonuses.SpeedBonus;
                stats.HealthBonus += bonuses.HealthBonus;
                stats.CritChance += bonuses.CritChance;
                stats.CritDamage += bonuses.CritDamage;
                stats.DodgeChance += bonuses.DodgeChance;
            }
            
            // 速度加成
            if (_activeEffects.ContainsKey("speed_boost")) {
                stats.SpeedBonus += _activeEffects["speed_boost"];
            }
            
            return stats;
        }
        
        /// <summary>
        /// 获取技能冷却时间
        /// </summary>
        public float GetSkillCooldown(string skillId) {
            return _skillCooldowns.GetValueOrDefault(skillId, 0f);
        }
        
        /// <summary>
        /// 获取所有技能
        /// </summary>
        public Dictionary<string, MountBattleSkill> GetAllSkills() {
            return MountBattleDatabase.MountSkills;
        }
        
        /// <summary>
        /// 获取已解锁的技能
        /// </summary>
        public List<string> GetUnlockedSkills() => _data.UnlockedMountSkills;
        
        /// <summary>
        /// 升级技能
        /// </summary>
        public bool UpgradeSkill(string skillId) {
            if (!_data.UnlockedMountSkills.Contains(skillId)) {
                return false;
            }
            
            var skill = MountBattleDatabase.MountSkills[skillId];
            int currentLevel = _data.SkillLevels.GetValueOrDefault(skillId, 1);
            
            if (currentLevel >= skill.MaxLevel) {
                return false;
            }
            
            int upgradeCost = currentLevel * 100;
            _data.SkillLevels[skillId] = currentLevel + 1;
            
            GD.Print($"[MountBattle] 技能 {skill.Name} 升级到 {currentLevel + 1}");
            return true;
        }
        
        public override void _Process(float delta) {
            // 更新技能冷却
            foreach (var skillId in new List<string>(_skillCooldowns.Keys)) {
                if (_skillCooldowns[skillId] > 0) {
                    _skillCooldowns[skillId] -= delta;
                    if (_skillCooldowns[skillId] < 0) {
                        _skillCooldowns[skillId] = 0;
                    }
                }
            }
            
            // 更新持续效果
            var expiredEffects = new List<string>();
            foreach (var effect in _activeEffects) {
                if (effect.Key.EndsWith("_duration")) {
                    _activeEffects[effect.Key] -= delta;
                    if (_activeEffects[effect.Key] <= 0) {
                        expiredEffects.Add(effect.Key.Replace("_duration", ""));
                    }
                }
            }
            
            foreach (var effect in expiredEffects) {
                _activeEffects.Remove(effect);
                _activeEffects.Remove(effect + "_duration");
            }
            
            // 魔法值恢复
            if (_currentState == MountBattleState.InBattle) {
                _currentMountMana = Mathf.Min(_currentMountMana + delta * 2f, _maxMountMana);
                OnManaChange?.Invoke(_currentMountMana, _maxMountMana);
            }
        }
    }
}
