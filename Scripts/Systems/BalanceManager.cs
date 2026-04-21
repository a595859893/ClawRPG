using Godot;
using System;
using System.IO;
using System.Text.Json;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 游戏平衡管理器 - 加载和应用游戏平衡配置
    /// </summary>
    public partial class BalanceManager : BaseSystem {
        public static BalanceManager Instance { get; private set; }

        private BalanceConfig _config;
        private string _configPath = "user://balance_config.json";
        private bool _configLoaded = false; 

        // 信号系统
        

        public override void _Ready() {
            Instance = this;
            LoadConfig();
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "Balance";

        /// <summary>
        /// 加载平衡配置文件
        /// </summary>
        public void LoadConfig() {
            try {
                if (System.IO.File.Exists(_configPath)) {
                    string json = System.IO.File.ReadAllText(_configPath);
                    _config = JsonSerializer.Deserialize<BalanceConfig>(json);
                    GD.Print("[BalanceManager] 配置已加载");
                } else {
                    // 创建默认配置
                    _config = new BalanceConfig();
                    SaveConfig();
                    GD.Print("[BalanceManager] 默认配置已创建");
                }
                _configLoaded = true;
                ApplyConfig();
            } catch (Exception e) {
                GD.PrintErr($"[BalanceManager] 配置加载失败: {e.Message}");
                _config = new BalanceConfig();
                _configLoaded = true;
            }
        }

        /// <summary>
        /// 保存平衡配置文件
        /// </summary>
        public void SaveConfig() {
            try {
                var options = new JsonSerializerOptions {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(_config, options);
                System.IO.File.WriteAllText(_configPath, json);
                GD.Print("[BalanceManager] 配置已保存");
            } catch (Exception e) {
                GD.PrintErr($"[BalanceManager] 配置保存失败: {e.Message}");
            }
        }

        /// <summary>
        /// 应用配置到游戏
        /// </summary>
        public void ApplyConfig() {
            GD.Print("[BalanceManager] 平衡配置已应用到游戏");
            EmitSignal(SignalName.ConfigReloaded);
        }

        /// <summary>
        /// 重载配置
        /// </summary>
        public void ReloadConfig() {
            LoadConfig();
        }

        /// <summary>
        /// 获取当前配置
        /// </summary>
        public BalanceConfig GetConfig() {
            return _config;
        }

        // ============ 玩家平衡 ============
        
        /// <summary>
        /// 获取玩家生命值乘数
        /// </summary>
        public float GetPlayerHealthMultiplier() => _config?.Player.HealthMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取玩家法力值乘数
        /// </summary>
        public float GetPlayerManaMultiplier() => _config?.Player.ManaMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取玩家攻击力乘数
        /// </summary>
        public float GetPlayerAttackMultiplier() => _config?.Player.AttackMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取玩家防御力乘数
        /// </summary>
        public float GetPlayerDefenseMultiplier() => _config?.Player.DefenseMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取玩家暴击率乘数
        /// </summary>
        public float GetPlayerCritChanceMultiplier() => _config?.Player.CritChanceMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取玩家暴击伤害乘数
        /// </summary>
        public float GetPlayerCritDamageMultiplier() => _config?.Player.CritDamageMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取玩家闪避率乘数
        /// </summary>
        public float GetPlayerDodgeMultiplier() => _config?.Player.DodgeMultiplier ?? 1.0f;

        // ============ 敌人平衡 ============
        
        /// <summary>
        /// 获取敌人生命值乘数
        /// </summary>
        public float GetEnemyHealthMultiplier() => _config?.Enemy.HealthMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取敌人伤害乘数
        /// </summary>
        public float GetEnemyDamageMultiplier() => _config?.Enemy.DamageMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取敌人掉落经验乘数
        /// </summary>
        public float GetEnemyXPMultiplier() => _config?.Enemy.XPMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取敌人掉落率乘数
        /// </summary>
        public float GetEnemyDropRateMultiplier() => _config?.Enemy.DropRateMultiplier ?? 1.0f;

        // ============ 战斗平衡 ============
        
        /// <summary>
        /// 获取基础伤害乘数
        /// </summary>
        public float GetBaseDamageMultiplier() => _config?.Combat.BaseDamageMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取技能伤害乘数
        /// </summary>
        public float GetSkillDamageMultiplier() => _config?.Combat.SkillDamageMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取基础暴击率
        /// </summary>
        public float GetCritBaseChance() => _config?.Combat.CritBaseChance ?? 0.05f;
        
        /// <summary>
        /// 获取暴击额外伤害
        /// </summary>
        public float GetCritBonusDamage() => _config?.Combat.CritBonusDamage ?? 0.5f;
        
        /// <summary>
        /// 获取闪避基础几率
        /// </summary>
        public float GetDodgeBaseChance() => _config?.Combat.DodgeBaseChance ?? 0.05f;
        
        /// <summary>
        /// 获取格挡减伤比例
        /// </summary>
        public float GetBlockBaseReduction() => _config?.Combat.BlockBaseReduction ?? 0.5f;
        
        /// <summary>
        /// 获取完美格挡减伤比例
        /// </summary>
        public float GetPerfectBlockReduction() => _config?.Combat.PerfectBlockReduction ?? 1.0f;
        
        /// <summary>
        /// 获取反击伤害倍数
        /// </summary>
        public float GetCounterAttackDamage() => _config?.Combat.CounterAttackDamage ?? 1.5f;
        
        /// <summary>
        /// 获取连击伤害加成
        /// </summary>
        public float GetComboDamageBonus() => _config?.Combat.ComboDamageBonus ?? 0.1f;
        
        /// <summary>
        /// 获取最大连击数
        /// </summary>
        public float GetMaxCombo() => _config?.Combat.MaxCombo ?? 10;

        // ============ 物品平衡 ============
        
        /// <summary>
        /// 获取物品掉落率乘数
        /// </summary>
        public float GetItemDropRateMultiplier() => _config?.Item.DropRateMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取附魔成本乘数
        /// </summary>
        public float GetEnchantCostMultiplier() => _config?.Item.EnchantCostMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取强化成本乘数
        /// </summary>
        public float GetEnhancementCostMultiplier() => _config?.Item.EnhancementCostMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取套装效果乘数
        /// </summary>
        public float GetSetEffectMultiplier() => _config?.Item.SetEffectMultiplier ?? 1.0f;

        // ============ 技能平衡 ============
        
        /// <summary>
        /// 获取技能冷却乘数
        /// </summary>
        public float GetSkillCooldownMultiplier() => _config?.Skill.CooldownMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取技能法力消耗乘数
        /// </summary>
        public float GetSkillManaCostMultiplier() => _config?.Skill.ManaCostMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取技能伤害乘数
        /// </summary>
        public float GetSkillEffectMultiplier() => _config?.Skill.DamageMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取治疗效果乘数
        /// </summary>
        public float GetHealMultiplier() => _config?.Skill.HealMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取技能范围乘数
        /// </summary>
        public float GetAoERadiusMultiplier() => _config?.Skill.AoERadiusMultiplier ?? 1.0f;

        // ============ Boss平衡 ============
        
        /// <summary>
        /// 获取Boss生命值乘数
        /// </summary>
        public float GetBossHealthMultiplier() => _config?.Boss.HealthMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取Boss伤害乘数
        /// </summary>
        public float GetBossDamageMultiplier() => _config?.Boss.DamageMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取Boss狂暴时间乘数
        /// </summary>
        public float GetBossEnrageTimeMultiplier() => _config?.Boss.EnrageTimeMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取Boss狂暴伤害倍数
        /// </summary>
        public float GetBossEnrageDamageMultiplier() => _config?.Boss.EnrageDamageMultiplier ?? 1.5f;

        // ============ 经验平衡 ============
        
        /// <summary>
        /// 获取击杀经验乘数
        /// </summary>
        public float GetKillXPMultiplier() => _config?.XP.KillXPMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取任务经验乘数
        /// </summary>
        public float GetQuestXPMultiplier() => _config?.XP.QuestXPMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取Boss经验乘数
        /// </summary>
        public float GetBossXPMultiplier() => _config?.XP.BossXPMultiplier ?? 1.0f;

        // ============ 经济平衡 ============
        
        /// <summary>
        /// 获取金币掉落乘数
        /// </summary>
        public float GetGoldDropMultiplier() => _config?.Economy.GoldDropMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取物品价格乘数
        /// </summary>
        public float GetItemPriceMultiplier() => _config?.Economy.ItemPriceMultiplier ?? 1.0f;
        
        /// <summary>
        /// 获取任务奖励乘数
        /// </summary>
        public float GetQuestRewardMultiplier() => _config?.Economy.QuestRewardMultiplier ?? 1.0f;

        // ============ 动态调整 ============
        
        /// <summary>
        /// 动态调整玩家属性乘数
        /// </summary>
        public void SetPlayerMultiplier(string stat, float value) {
            if (_config == null) return;
            
            float oldValue = 1.0f;
            switch (stat.ToLower()) {
                case "health": oldValue = _config.Player.HealthMultiplier; _config.Player.HealthMultiplier = value; break;
                case "mana": oldValue = _config.Player.ManaMultiplier; _config.Player.ManaMultiplier = value; break;
                case "attack": oldValue = _config.Player.AttackMultiplier; _config.Player.AttackMultiplier = value; break;
                case "defense": oldValue = _config.Player.DefenseMultiplier; _config.Player.DefenseMultiplier = value; break;
                case "crit": oldValue = _config.Player.CritChanceMultiplier; _config.Player.CritChanceMultiplier = value; break;
                case "critdamage": oldValue = _config.Player.CritDamageMultiplier; _config.Player.CritDamageMultiplier = value; break;
                case "dodge": oldValue = _config.Player.DodgeMultiplier; _config.Player.DodgeMultiplier = value; break;
            }
            
            EmitSignal(SignalName.BalanceChanged, "player", stat, oldValue, value);
            ApplyConfig();
        }

        /// <summary>
        /// 动态调整敌人属性乘数
        /// </summary>
        public void SetEnemyMultiplier(string stat, float value) {
            if (_config == null) return;
            
            float oldValue = 1.0f;
            switch (stat.ToLower()) {
                case "health": oldValue = _config.Enemy.HealthMultiplier; _config.Enemy.HealthMultiplier = value; break;
                case "damage": oldValue = _config.Enemy.DamageMultiplier; _config.Enemy.DamageMultiplier = value; break;
                case "xp": oldValue = _config.Enemy.XPMultiplier; _config.Enemy.XPMultiplier = value; break;
                case "droprate": oldValue = _config.Enemy.DropRateMultiplier; _config.Enemy.DropRateMultiplier = value; break;
            }
            
            EmitSignal(SignalName.BalanceChanged, "enemy", stat, oldValue, value);
            ApplyConfig();
        }

        /// <summary>
        /// 创建难度预设
        /// </summary>
        public void ApplyDifficultyPreset(string preset) {
            if (_config == null) return;
            
            switch (preset.ToLower()) {
                case "easy":
                    _config.Player.HealthMultiplier = 1.5f;
                    _config.Player.DefenseMultiplier = 1.5f;
                    _config.Enemy.HealthMultiplier = 0.7f;
                    _config.Enemy.DamageMultiplier = 0.7f;
                    _config.Combat.CritBaseChance = 0.1f;
                    _config.XP.KillXPMultiplier = 1.5f;
                    _config.Economy.GoldDropMultiplier = 1.5f;
                    break;
                    
                case "hard":
                    _config.Player.HealthMultiplier = 0.7f;
                    _config.Player.DefenseMultiplier = 0.8f;
                    _config.Enemy.HealthMultiplier = 1.5f;
                    _config.Enemy.DamageMultiplier = 1.5f;
                    _config.Combat.CritBaseChance = 0.03f;
                    _config.XP.KillXPMultiplier = 1.2f;
                    _config.Economy.GoldDropMultiplier = 1.2f;
                    break;
                    
                case "nightmare":
                    _config.Player.HealthMultiplier = 0.5f;
                    _config.Player.DefenseMultiplier = 0.6f;
                    _config.Enemy.HealthMultiplier = 2.0f;
                    _config.Enemy.DamageMultiplier = 2.0f;
                    _config.Combat.CritBaseChance = 0.02f;
                    _config.XP.KillXPMultiplier = 2.0f;
                    _config.Economy.GoldDropMultiplier = 2.0f;
                    break;
                    
                case "normal":
                default:
                    _config = new BalanceConfig();
                    break;
            }
            
            SaveConfig();
            ApplyConfig();
            GD.Print($"[BalanceManager] 已应用难度预设: {preset}");
        }

        /// <summary>
        /// 导出保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            if (_config != null)
            {
                data["config_json"] = ExportConfigAsJson();
            }
            data["config_loaded"] = _configLoaded;
            return data;
        }

        /// <summary>
        /// 导入保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.ContainsKey("config_json"))
            {
                ImportConfigFromJson(data["config_json"].ToString());
            }
            if (data.ContainsKey("config_loaded"))
            {
                _configLoaded = Convert.ToBoolean(data["config_loaded"]);
            }
        }

        /// <summary>
        /// 导出配置为JSON字符串
        /// </summary>
        public string ExportConfigAsJson() {
            if (_config == null) return "{}";
            
            var options = new JsonSerializerOptions {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(_config, options);
        }

        /// <summary>
        /// 从JSON字符串导入配置
        /// </summary>
        public void ImportConfigFromJson(string json) {
            try {
                _config = JsonSerializer.Deserialize<BalanceConfig>(json);
                SaveConfig();
                ApplyConfig();
                GD.Print("[BalanceManager] 配置已从JSON导入");
            } catch (Exception e) {
                GD.PrintErr($"[BalanceManager] 配置导入失败: {e.Message}");
            }
        }
    }
}
