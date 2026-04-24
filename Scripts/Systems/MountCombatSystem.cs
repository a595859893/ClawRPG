using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Characters;

namespace ClawRPG.Scripts.Mounts {
    /// <summary>
    /// 坐骑战斗系统 - 管理坐骑战斗技能和战斗状态
    /// </summary>
    public partial class MountCombatSystem : BaseSystem {
        public static MountCombatSystem Instance { get; private set; }

        private MountCombatDatabase _database;
        private Dictionary<string, MountCombatData.MountCombatInstance> _mountCombatData = new Dictionary<string, MountCombatData.MountCombatInstance>();
        
        // 信号系统
        public delegate void OnMountSkillUsed(string mountId, string skillId, Vector2 targetPosition);
        public delegate void OnMountSkillReady(string mountId, string skillId);
        public delegate void OnMountCombatStart(string mountId);
        public delegate void OnMountCombatEnd(string mountId, int damageDealt, int damageTaken, int kills);
        public delegate void OnMountCombatStatsUpdated(string mountId, float damage, float attackSpeed);

        // 冷却更新计时
        private float _cooldownTimer = 0f;

        public override void _Ready() {
            Instance = this;
            _database = new MountCombatDatabase();
            GD.Print("[MountCombatSystem] Initialized");
        }

        public override void _Process(double delta) {
            // 更新冷却时间
            _cooldownTimer += delta;
            if (_cooldownTimer >= 1.0f) {
                _cooldownTimer = 0f;
                UpdateCooldowns();
            }
        }

        /// <summary>
        /// 更新所有技能冷却
        /// </summary>
        private void UpdateCooldowns() {
            foreach (var kvp in _mountCombatData) {
                var instance = kvp.Value;
                var keysToRemove = new List<string>();
                
                foreach (var cooldownKvp in instance.SkillCooldowns) {
                    if (cooldownKvp.Value > 0) {
                        instance.SkillCooldowns[cooldownKvp.Key] = cooldownKvp.Value - 1;
                        
                        // 技能冷却完成
                        if (instance.SkillCooldowns[cooldownKvp.Key] == 0) {
                            EmitSignal(nameof(OnMountSkillReady), kvp.Key, cooldownKvp.Key);
                        }
                    }
                    
                    if (cooldownKvp.Value <= 0) {
                        keysToRemove.Add(cooldownKvp.Key);
                    }
                }
                
                foreach (var key in keysToRemove) {
                    instance.SkillCooldowns.Remove(key);
                }
            }
        }

        /// <summary>
        /// 初始化坐骑战斗数据
        /// </summary>
        public void InitializeMountCombat(string mountId, int level) {
            if (_mountCombatData.ContainsKey(mountId)) return;

            var instance = new MountCombatData.MountCombatInstance {
                MountId = mountId,
                Level = level,
                Experience = 0,
                IsInCombat = false,
            };

            // 解锁初始技能
            var skills = _database.GetAllSkills(mountId, level);
            foreach (var skill in skills) {
                instance.UnlockedSkills.Add(skill.Id);
            }

            _mountCombatData[mountId] = instance;
            GD.Print($"[MountCombatSystem] Initialized combat data for {mountId} at level {level}");
        }

        /// <summary>
        /// 使用坐骑技能
        /// </summary>
        public bool UseMountSkill(string mountId, string skillId, Vector2 targetPosition, Player player) {
            if (!_mountCombatData.ContainsKey(mountId)) {
                GD.Warning($"[MountCombatSystem] No combat data for mount: {mountId}");
                return false;
            }

            var instance = _mountCombatData[mountId];
            var skill = GetSkill(mountId, skillId);
            
            if (skill == null) {
                GD.Warning($"[MountCombatSystem] Skill not found: {skillId}");
                return false;
            }

            // 检查冷却
            if (MountCombatData.GetSkillCooldownRemaining(instance, skillId) > 0) {
                GD.Print($"[MountCombatSystem] Skill on cooldown: {skillId}");
                return false;
            }

            // 检查魔法值
            if (player.CurrentMana < skill.ManaCost) {
                GD.Print($"[MountCombatSystem] Not enough mana: {player.CurrentMana}/{skill.ManaCost}");
                return false;
            }

            // 扣除魔法值
            player.AddMana(-skill.ManaCost);

            // 设置冷却
            instance.SkillCooldowns[skillId] = skill.Cooldown;

            // 开始战斗状态
            if (!instance.IsInCombat) {
                instance.IsInCombat = true;
                EmitSignal(nameof(OnMountCombatStart), mountId);
            }

            // 计算伤害
            float baseDamage = CalculateMountDamage(mountId, player);
            float skillDamage = baseDamage * skill.DamageMultiplier;

            // 应用技能效果
            ApplySkillEffects(skill, targetPosition, skillDamage, player);

            // 触发信号
            EmitSignal(nameof(OnMountSkillUsed), mountId, skillId, targetPosition);
            
            // 更新战斗统计
            instance.CombatDamageDealt += (int)skillDamage;

            return true;
        }

        /// <summary>
        /// 计算坐骑基础伤害
        /// </summary>
        private float CalculateMountDamage(string mountId, Player player) {
            var stats = _database.GetMountCombatStats(mountId);
            float baseDamage = stats.AttackDamage;
            
            // 加上玩家攻击力的一部分
            baseDamage += player.CurrentAttack * 0.3f;
            
            // 应用玩家装备的坐骑伤害加成
            baseDamage *= (1.0f + player.GetTotalStatMultiplier("MountDamage"));
            
            return baseDamage;
        }

        /// <summary>
        /// 应用技能效果
        /// </summary>
        private void ApplySkillEffects(MountCombatData.MountCombatSkill skill, Vector2 targetPosition, float damage, Player player) {
            var enemies = GetEnemiesInRange(targetPosition, skill.Range, skill.IsAOE ? skill.AOERadius : 0);
            
            foreach (var enemy in enemies) {
                // 计算最终伤害
                float finalDamage = CalculateFinalDamage(damage, player, enemy);
                
                // 造成伤害
                enemy.TakeDamage(finalDamage);
                
                // 击退效果
                if (skill.KnockbackForce > 0) {
                    Vector2 knockbackDir = (enemy.GlobalPosition - targetPosition).Normalized();
                    enemy.ApplyKnockback(knockbackDir * skill.KnockbackForce);
                }
                
                // 眩晕效果
                if (skill.StunDuration > 0) {
                    enemy.ApplyStun(skill.StunDuration);
                }
                
                // 减速效果
                if (skill.ApplySlow) {
                    enemy.ApplySlow(skill.SlowDuration, skill.SlowAmount);
                }
                
                // 流血效果
                if (skill.ApplyBleed) {
                    enemy.ApplyBleed(skill.BleedDuration, skill.BleedDamage);
                }
            }
            
            // 治疗效果
            if (skill.HealCaster) {
                player.Heal(skill.HealAmount);
            }
        }

        /// <summary>
        /// 获取范围内的敌人
        /// </summary>
        private List<Enemy> GetEnemiesInRange(Vector2 center, float range, float aoeRadius) {
            var enemies = new List<Enemy>();
            float actualRange = aoeRadius > 0 ? aoeRadius : range;
            
            var allEnemies = GetTree().GetNodesInGroup("Enemy");
            foreach (Node node in allEnemies) {
                if (node is Enemy enemy) {
                    float dist = enemy.GlobalPosition.DistanceTo(center);
                    if (dist <= actualRange) {
                        enemies.Add(enemy);
                    }
                }
            }
            
            return enemies;
        }

        /// <summary>
        /// 计算最终伤害
        /// </summary>
        private float CalculateFinalDamage(float baseDamage, Player player, Enemy enemy) {
            var stats = _database.GetMountCombatStats(MountManager.Instance.GetActiveMountId());
            
            // 暴击计算
            bool isCrit = GD.Randf() < stats.CritChance;
            float critMultiplier = isCrit ? stats.CritDamage : 1.0f;
            
            // 护甲穿透
            float armorPen = stats.ArmorPenetration;
            float enemyDefense = enemy.Defense * (1.0f - armorPen / 100f);
            
            // 最终伤害
            float finalDamage = (baseDamage * critMultiplier) - enemyDefense;
            finalDamage = Mathf.Max(1, finalDamage); // 最低1点伤害
            
            // 触发暴击特效
            if (isCrit) {
                // 这里可以添加暴击特效
            }
            
            return finalDamage;
        }

        /// <summary>
        /// 获取技能
        /// </summary>
        public MountCombatData.MountCombatSkill GetSkill(string mountId, string skillId) {
            var skills = _database.GetMountSkills(mountId);
            foreach (var skill in skills) {
                if (skill.Id == skillId) return skill;
            }
            return null;
        }

        /// <summary>
        /// 获取坐骑战斗实例
        /// </summary>
        public MountCombatData.MountCombatInstance GetMountCombatInstance(string mountId) {
            if (_mountCombatData.ContainsKey(mountId)) {
                return _mountCombatData[mountId];
            }
            return null;
        }

        /// <summary>
        /// 获取已解锁的技能列表
        /// </summary>
        public List<MountCombatData.MountCombatSkill> GetUnlockedSkills(string mountId) {
            if (!_mountCombatData.ContainsKey(mountId)) {
                return new List<MountCombatData.MountCombatSkill>();
            }
            
            var instance = _mountCombatData[mountId];
            var allSkills = _database.GetMountSkills(mountId);
            var unlocked = new List<MountCombatData.MountCombatSkill>();
            
            foreach (var skill in allSkills) {
                if (instance.UnlockedSkills.Contains(skill.Id)) {
                    unlocked.Add(skill);
                }
            }
            
            return unlocked;
        }

        /// <summary>
        /// 获取技能冷却剩余时间
        /// </summary>
        public int GetSkillCooldown(string mountId, string skillId) {
            if (!_mountCombatData.ContainsKey(mountId)) return 0;
            return MountCombatData.GetSkillCooldownRemaining(_mountCombatData[mountId], skillId);
        }

        /// <summary>
        /// 坐骑普通攻击
        /// </summary>
        public void PerformMountAttack(Player player, Vector2 direction) {
            string mountId = MountManager.Instance.GetActiveMountId();
            if (mountId == null) return;

            var instance = GetMountCombatInstance(mountId);
            if (instance == null) {
                InitializeMountCombat(mountId, 1);
                instance = GetMountCombatInstance(mountId);
            }

            var stats = _database.GetMountCombatStats(mountId);
            
            // 检查攻击间隔
            float currentTime = (float)OS.GetTicksMsec() / 1000f;
            float attackInterval = 1.0f / stats.AttackSpeed;
            
            if (currentTime - instance.LastAttackTime < attackInterval) {
                return;
            }
            
            instance.LastAttackTime = currentTime;

            // 开始战斗状态
            if (!instance.IsInCombat) {
                instance.IsInCombat = true;
                EmitSignal(nameof(OnMountCombatStart), mountId);
            }

            // 计算伤害
            float baseDamage = CalculateMountDamage(mountId, player);
            
            // 查找攻击范围内的敌人
            var enemies = GetEnemiesInRange(player.GlobalPosition + direction * 50f, 80f, 0);
            if (enemies.Count > 0) {
                // 攻击最近的敌人
                Enemy target = enemies[0];
                float minDist = float.MaxValue;
                foreach (var enemy in enemies) {
                    float dist = enemy.GlobalPosition.DistanceTo(player.GlobalPosition);
                    if (dist < minDist) {
                        minDist = dist;
                        target = enemy;
                    }
                }
                
                float damage = CalculateFinalDamage(baseDamage, player, target);
                target.TakeDamage(damage);
                
                // 触发音效
                if (SoundEffectSystem.Instance != null) {
                    SoundEffectSystem.Instance.PlayEnemyHit();
                }
                
                // 更新统计
                instance.CombatDamageDealt += (int)damage;
            }
        }

        /// <summary>
        /// 结束坐骑战斗
        /// </summary>
        public void EndMountCombat(string mountId) {
            if (!_mountCombatData.ContainsKey(mountId)) return;
            
            var instance = _mountCombatData[mountId];
            if (!instance.IsInCombat) return;
            
            instance.IsInCombat = false; 
            
            EmitSignal(nameof(OnMountCombatEnd), mountId, 
                instance.CombatDamageDealt, 
                instance.CombatDamageTaken, 
                instance.CombatKills);
            
            // 重置统计
            instance.CombatDamageDealt = 0;
            instance.CombatDamageTaken = 0;
            instance.CombatKills = 0;
        }

        /// <summary>
        /// 记录被击中
        /// </summary>
        public void RecordMountDamageTaken(int damage) {
            string mountId = MountManager.Instance.GetActiveMountId();
            if (mountId == null || !_mountCombatData.ContainsKey(mountId)) return;
            
            var instance = _mountCombatData[mountId];
            instance.CombatDamageTaken += damage;
            
            if (!instance.IsInCombat) {
                instance.IsInCombat = true;
                EmitSignal(nameof(OnMountCombatStart), mountId);
            }
        }

        /// <summary>
        /// 记录击杀
        /// </summary>
        public void RecordMountKill() {
            string mountId = MountManager.Instance.GetActiveMountId();
            if (mountId == null || !_mountCombatData.ContainsKey(mountId)) return;
            
            _mountCombatData[mountId].CombatKills++;
        }

        /// <summary>
        /// 获取坐骑战斗属性
        /// </summary>
        public MountCombatData.MountCombatStats GetMountCombatStats(string mountId) {
            return _database.GetMountCombatStats(mountId);
        }

        /// <summary>
        /// 坐骑是否有战斗能力
        /// </summary>
        public bool HasCombatAbility() {
            string mountId = MountManager.Instance.GetActiveMountId();
            if (mountId == null) return false;
            return _database.HasCombatAbility(mountId);
        }

        /// <summary>
        /// 序列化 - 保存数据
        /// </summary>
        public Dictionary<string, Dictionary<string, object>> Serialize() {
            Dictionary<string, Dictionary<string, object>> data = new Dictionary<string, Dictionary<string, object>>();
            
            foreach (var kvp in _mountCombatData) {
                Dictionary<string, object> mountData = new Dictionary<string, object>();
                mountData["mountId"] = kvp.Value.MountId;
                mountData["level"] = kvp.Value.Level;
                mountData["experience"] = kvp.Value.Experience;
                mountData["unlockedSkills"] = string.Join(",", kvp.Value.UnlockedSkills);
                
                // 序列化冷却
                Dictionary<string, int> cooldowns = new Dictionary<string, int>();
                foreach (var cd in kvp.Value.SkillCooldowns) {
                    cooldowns[cd.Key] = cd.Value;
                }
                mountData["cooldowns"] = cooldowns;
                
                data[kvp.Key] = mountData;
            }
            
            return data;
        }

        /// <summary>
        /// 反序列化 - 加载数据
        /// </summary>
        public void Deserialize(Dictionary<string, Dictionary<string, object>> data) {
            if (data == null) return;
            
            _mountCombatData.Clear();
            
            foreach (var kvp in data) {
                var mountData = kvp.Value;
                var instance = new MountCombatData.MountCombatInstance();
                instance.MountId = mountData["mountId"].ToString();
                instance.Level = Convert.ToInt32(mountData["level"]);
                instance.Experience = Convert.ToInt32(mountData["experience"]);
                
                // 解锁技能
                string skillsStr = mountData["unlockedSkills"].ToString();
                if (!string.IsNullOrEmpty(skillsStr)) {
                    instance.UnlockedSkills = new List<string>(skillsStr.Split(','));
                }
                
                // 加载冷却
                if (mountData.ContainsKey("cooldowns")) {
                    var cdData = mountData["cooldowns"] as Dictionary<string, int>;
                    if (cdData != null) {
                        instance.SkillCooldowns = cdData;
                    }
                }
                
                _mountCombatData[kvp.Key] = instance;
            }
            
            GD.Print($"[MountCombatSystem] Loaded {_mountCombatData.Count} mount combat data");
        }
        
        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            var mountCombatList = new Godot.Collections.Array();
            
            foreach (var kvp in _mountCombatData)
            {
                var mountData = new Dictionary
                {
                    { "mount_id", kvp.Value.MountId },
                    { "level", kvp.Value.Level },
                    { "experience", kvp.Value.Experience },
                    { "is_in_combat", kvp.Value.IsInCombat },
                    { "combat_damage_dealt", kvp.Value.CombatDamageDealt },
                    { "combat_damage_taken", kvp.Value.CombatDamageTaken },
                    { "combat_kills", kvp.Value.CombatKills },
                    { "unlocked_skills", new Godot.Collections.Array(kvp.Value.UnlockedSkills) }
                };
                
                // 序列化冷却
                var cooldowns = new Dictionary<string, object>();
                foreach (var cd in kvp.Value.SkillCooldowns)
                {
                    cooldowns[cd.Key] = cd.Value;
                }
                mountData["cooldowns"] = cooldowns;
                
                mountCombatList.Add(mountData);
            }
            
            data["mount_combat_data"] = mountCombatList;
            return data;
        }
        
        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _mountCombatData.Clear();
            
            if (data.ContainsKey("mount_combat_data"))
            {
                var mountCombatList = (Array)data["mount_combat_data"];
                foreach (Dictionary mountData in mountCombatList)
                {
                    var instance = new MountCombatData.MountCombatInstance
                    {
                        MountId = (string)mountData["mount_id"],
                        Level = (int)mountData["level"],
                        Experience = (int)mountData["experience"],
                        IsInCombat = (bool)mountData["is_in_combat"],
                        CombatDamageDealt = (int)mountData["combat_damage_dealt"],
                        CombatDamageTaken = (int)mountData["combat_damage_taken"],
                        CombatKills = (int)mountData["combat_kills"]
                    };
                    
                    // 解锁技能
                    if (mountData.ContainsKey("unlocked_skills"))
                    {
                        var skills = (Array)mountData["unlocked_skills"];
                        instance.UnlockedSkills = new List<string>();
                        foreach (string skill in skills)
                        {
                            instance.UnlockedSkills.Add(skill);
                        }
                    }
                    
                    // 冷却
                    if (mountData.ContainsKey("cooldowns"))
                    {
                        var cdData = (Dictionary)mountData["cooldowns"];
                        instance.SkillCooldowns = new Dictionary<string, int>();
                        foreach (var cd in cdData)
                        {
                            instance.SkillCooldowns[cd.Key] = (int)cd.Value;
                        }
                    }
                    
                    _mountCombatData[instance.MountId] = instance;
                }
            }
            
            GD.Print($"[MountCombatSystem] Loaded {_mountCombatData.Count} mount combat data");
        }
    }
}
