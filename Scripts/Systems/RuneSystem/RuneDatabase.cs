using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 符文数据库 - 管理所有符文数据
    /// </summary>
    public class RuneDatabase {
        private static RuneDatabase _instance;
        public static RuneDatabase Instance {
            get {
                if (_instance == null) {
                    _instance = new RuneDatabase();
                }
                return _instance;
            }
        }

        private Dictionary<string, Rune> _runes;

        public RuneDatabase() {
            _runes = new Dictionary<string, Rune>();
            InitializeRunes();
        }

        private void InitializeRunes() {
            // ===== 攻击符文 =====
            AddRune(new Rune {
                Id = "rune_strike",
                Name = "打击符文",
                Description = "增加基础攻击力",
                Type = RuneType.Attack,
                Rarity = RuneRarity.Common,
                LevelRequired = 1,
                Price = 100,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.Damage, 5 }
                }
            });

            AddRune(new Rune {
                Id = "rune_power",
                Name = "力量符文",
                Description = "提升力量属性",
                Type = RuneType.Attack,
                Rarity = RuneRarity.Uncommon,
                LevelRequired = 5,
                Price = 300,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.Damage, 12 }
                }
            });

            AddRune(new Rune {
                Id = "rune_fury",
                Name = "狂怒符文",
                Description = "增加暴击率和暴击伤害",
                Type = RuneType.Attack,
                Rarity = RuneRarity.Rare,
                LevelRequired = 10,
                Price = 800,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.CritChance, 5 },
                    { RuneAttribute.CritDamage, 15 }
                }
            });

            AddRune(new Rune {
                Id = "rune_slayer",
                Name = "杀手符文",
                Description = "致命攻击符文",
                Type = RuneType.Attack,
                Rarity = RuneRarity.Epic,
                LevelRequired = 20,
                Price = 2500,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.Damage, 25 },
                    { RuneAttribute.CritChance, 8 },
                    { RuneAttribute.CritDamage, 20 }
                }
            });

            AddRune(new Rune {
                Id = "rune_annihilation",
                Name = "毁灭符文",
                Description = "传说级攻击符文",
                Type = RuneType.Attack,
                Rarity = RuneRarity.Legendary,
                LevelRequired = 30,
                Price = 10000,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.Damage, 40 },
                    { RuneAttribute.CritChance, 12 },
                    { RuneAttribute.CritDamage, 30 }
                },
                UniquePassive = "攻击时有5%几率造成300%伤害"
            });

            // ===== 防御符文 =====
            AddRune(new Rune {
                Id = "rune_shield",
                Name = "护盾符文",
                Description = "增加基础防御",
                Type = RuneType.Defense,
                Rarity = RuneRarity.Common,
                LevelRequired = 1,
                Price = 100,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.Defense, 5 }
                }
            });

            AddRune(new Rune {
                Id = "rune_stone",
                Name = "岩石符文",
                Description = "提升生命值和防御",
                Type = RuneType.Defense,
                Rarity = RuneRarity.Uncommon,
                LevelRequired = 5,
                Price = 300,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.MaxHealth, 50 },
                    { RuneAttribute.Defense, 8 }
                }
            });

            AddRune(new Rune {
                Id = "rune_titan",
                Name = "泰坦符文",
                Description = "大幅提升生命值",
                Type = RuneType.Defense,
                Rarity = RuneRarity.Rare,
                LevelRequired = 10,
                Price = 800,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.MaxHealth, 120 },
                    { RuneAttribute.Defense, 15 }
                }
            });

            AddRune(new Rune {
                Id = "rune_immortal",
                Name = "不朽符文",
                Description = "生命恢复和抗性",
                Type = RuneType.Defense,
                Rarity = RuneRarity.Epic,
                LevelRequired = 20,
                Price = 2500,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.MaxHealth, 200 },
                    { RuneAttribute.Defense, 20 },
                    { RuneAttribute.HealthRegen, 3 },
                    { RuneAttribute.FireResistance, 10 }
                }
            });

            AddRune(new Rune {
                Id = "rune_eternal",
                Name = "永恒符文",
                Description = "传说级防御符文",
                Type = RuneType.Defense,
                Rarity = RuneRarity.Legendary,
                LevelRequired = 30,
                Price = 10000,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.MaxHealth, 350 },
                    { RuneAttribute.Defense, 30 },
                    { RuneAttribute.HealthRegen, 5 },
                    { RuneAttribute.FireResistance, 15 },
                    { RuneAttribute.IceResistance, 15 },
                    { RuneAttribute.DarkResistance, 15 }
                },
                UniquePassive = "生命值低于20%时获得30%伤害减免"
            });

            // ===== 魔法符文 =====
            AddRune(new Rune {
                Id = "rune_mana",
                Name = "法力符文",
                Description = "增加法力上限",
                Type = RuneType.Magic,
                Rarity = RuneRarity.Common,
                LevelRequired = 1,
                Price = 100,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.MaxMana, 20 }
                }
            });

            AddRune(new Rune {
                Id = "rune_wisdom",
                Name = "智慧符文",
                Description = "提升法力恢复",
                Type = RuneType.Magic,
                Rarity = RuneRarity.Uncommon,
                LevelRequired = 5,
                Price = 300,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.MaxMana, 40 },
                    { RuneAttribute.ManaRegen, 2 }
                }
            });

            AddRune(new Rune {
                Id = "rune_arcane",
                Name = "奥术符文",
                Description = "魔法增强符文",
                Type = RuneType.Magic,
                Rarity = RuneRarity.Rare,
                LevelRequired = 10,
                Price = 800,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.MaxMana, 80 },
                    { RuneAttribute.ManaRegen, 4 }
                }
            });

            AddRune(new Rune {
                Id = "rune_sorcerer",
                Name = "巫师符文",
                Description = "高级魔法符文",
                Type = RuneType.Magic,
                Rarity = RuneRarity.Epic,
                LevelRequired = 20,
                Price = 2500,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.MaxMana, 150 },
                    { RuneAttribute.ManaRegen, 6 },
                    { RuneAttribute.Damage, 15 }
                }
            });

            AddRune(new Rune {
                Id = "rune_ascendant",
                Name = "升华符文",
                Description = "传说级魔法符文",
                Type = RuneType.Magic,
                Rarity = RuneRarity.Legendary,
                LevelRequired = 30,
                Price = 10000,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.MaxMana, 250 },
                    { RuneAttribute.ManaRegen, 10 },
                    { RuneAttribute.Damage, 25 }
                },
                UniquePassive = "技能冷却速度提升15%"
            });

            // ===== 辅助符文 =====
            AddRune(new Rune {
                Id = "rune_swift",
                Name = "敏捷符文",
                Description = "提升移动速度",
                Type = RuneType.Utility,
                Rarity = RuneRarity.Common,
                LevelRequired = 1,
                Price = 100,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.MoveSpeed, 5 }
                }
            });

            AddRune(new Rune {
                Id = "rune_vitality",
                Name = "活力符文",
                Description = "生命恢复提升",
                Type = RuneType.Utility,
                Rarity = RuneRarity.Uncommon,
                LevelRequired = 5,
                Price = 300,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.HealthRegen, 2 },
                    { RuneAttribute.MoveSpeed, 3 }
                }
            });

            AddRune(new Rune {
                Id = "rune_agility",
                Name = "灵巧符文",
                Description = "攻击速度和闪避",
                Type = RuneType.Utility,
                Rarity = RuneRarity.Rare,
                LevelRequired = 10,
                Price = 800,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.AttackSpeed, 8 },
                    { RuneAttribute.MoveSpeed, 5 }
                }
            });

            AddRune(new Rune {
                Id = "rune_blessing",
                Name = "祝福符文",
                Description = "全面属性提升",
                Type = RuneType.Utility,
                Rarity = RuneRarity.Epic,
                LevelRequired = 20,
                Price = 2500,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.HealthRegen, 4 },
                    { RuneAttribute.ManaRegen, 3 },
                    { RuneAttribute.MoveSpeed, 8 },
                    { RuneAttribute.AttackSpeed, 5 }
                }
            });

            AddRune(new Rune {
                Id = "rune_divine",
                Name = "神圣符文",
                Description = "传说级辅助符文",
                Type = RuneType.Utility,
                Rarity = RuneRarity.Legendary,
                LevelRequired = 30,
                Price = 10000,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.HealthRegen, 6 },
                    { RuneAttribute.ManaRegen, 5 },
                    { RuneAttribute.MoveSpeed, 12 },
                    { RuneAttribute.AttackSpeed, 10 }
                },
                UniquePassive = "每秒恢复0.5%最大生命值和法力值"
            });

            // ===== 更多稀有符文 =====
            AddRune(new Rune {
                Id = "rune_dragon",
                Name = "巨龙符文",
                Description = "龙族之力",
                Type = RuneType.Legendary,
                Rarity = RuneRarity.Legendary,
                LevelRequired = 35,
                Price = 15000,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.Damage, 35 },
                    { RuneAttribute.MaxHealth, 300 },
                    { RuneAttribute.FireResistance, 25 }
                },
                UniquePassive = "对龙类敌人伤害提升30%"
            });

            AddRune(new Rune {
                Id = "rune_demon",
                Name = "恶魔符文",
                Description = "深渊之力",
                Type = RuneType.Legendary,
                Rarity = RuneRarity.Legendary,
                LevelRequired = 35,
                Price = 15000,
                Attributes = new Dictionary<RuneAttribute, float> {
                    { RuneAttribute.Damage, 30 },
                    { RuneAttribute.CritChance, 10 },
                    { RuneAttribute.DarkResistance, 25 }
                },
                UniquePassive = "生命值低于50%时伤害提升20%"
            });
        }

        private void AddRune(Rune rune) {
            _runes[rune.Id] = rune;
        }

        /// <summary>
        /// 获取符文
        /// </summary>
        public Rune GetRune(string id) {
            return _runes.TryGetValue(id, out var rune) ? rune : null;
        }

        /// <summary>
        /// 获取所有符文
        /// </summary>
        public Dictionary<string, Rune> GetAllRunes() {
            return new Dictionary<string, Rune>(_runes);
        }

        /// <summary>
        /// 按类型获取符文
        /// </summary>
        public List<Rune> GetRunesByType(RuneType type) {
            List<Rune> result = new List<Rune>();
            foreach (var rune in _runes.Values) {
                if (rune.Type == type) {
                    result.Add(rune);
                }
            }
            return result;
        }

        /// <summary>
        /// 按稀有度获取符文
        /// </summary>
        public List<Rune> GetRunesByRarity(RuneRarity rarity) {
            List<Rune> result = new List<Rune>();
            foreach (var rune in _runes.Values) {
                if (rune.Rarity == rarity) {
                    result.Add(rune);
                }
            }
            return result;
        }

        /// <summary>
        /// 按等级获取符文
        /// </summary>
        public List<Rune> GetRunesByLevel(int playerLevel) {
            List<Rune> result = new List<Rune>();
            foreach (var rune in _runes.Values) {
                if (rune.LevelRequired <= playerLevel) {
                    result.Add(rune);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取随机符文（用于掉落）
        /// </summary>
        public Rune GetRandomRune(RuneRarity minRarity, int playerLevel) {
            List<Rune> candidates = new List<Rune>();
            int minRarityIndex = (int)minRarity;
            
            foreach (var rune in _runes.Values) {
                if ((int)rune.Rarity >= minRarityIndex && rune.LevelRequired <= playerLevel) {
                    candidates.Add(rune);
                }
            }
            
            if (candidates.Count == 0) return null;
            return candidates[GD.RandI() % candidates.Count];
        }
    }
}
