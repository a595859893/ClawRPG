using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    // 使用 Rune.cs 中定义的枚举: RuneAttribute, RuneRarity, RuneSet

    /// <summary>
    /// 符文集合效果
    /// </summary>
    [System.Serializable]
    public class RuneSetBonus {
        public int SetCount { get; set; }           // 需要的符文数量
        public Dictionary<RuneAttribute, float> Attributes { get; set; }  // 激活的属性加成
        public string Description { get; set; }      // 效果描述

        public RuneSetBonus() {
            Attributes = new Dictionary<RuneAttribute, float>();
        }
    }

    /// <summary>
    /// 符文集合数据
    /// </summary>
    [System.Serializable]
    public class RuneSetData {
        public RuneSet Set { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RuneSetBonus> Bonuses { get; set; }  // 2件/4件效果等
        public string IconName { get; set; }

        public RuneSetData() {
            Bonuses = new List<RuneSetBonus>();
        }

        /// <summary>
        /// 获取指定数量的激活效果
        /// </summary>
        public RuneSetBonus GetBonusForCount(int count) {
            RuneSetBonus bestBonus = null;
            foreach (var bonus in Bonuses) {
                if (count >= bonus.SetCount) {
                    bestBonus = bonus;
                }
            }
            return bestBonus;
        }
    }

    /// <summary>
    /// 符文集合管理器
    /// </summary>
    public class RuneSetManager {
        private static RuneSetManager _instance;
        public static RuneSetManager Instance {
            get {
                if (_instance == null) {
                    _instance = new RuneSetManager();
                }
                return _instance;
            }
        }

        // 符文集合数据库
        private Dictionary<RuneSet, RuneSetData> _setDatabase;

        public RuneSetManager() {
            InitializeSetDatabase();
        }

        /// <summary>
        /// 初始化符文集合数据库
        /// </summary>
        private void InitializeSetDatabase() {
            _setDatabase = new Dictionary<RuneSet, RuneSetData>();

            // 攻击套装 - 2件+攻击 4件+暴击伤害
            RuneSetData attackSet = new RuneSetData {
                Set = RuneSet.Attack,
                Name = "攻击",
                Description = "提升攻击力和暴击伤害",
                IconName = "sword",
                Bonuses = new List<RuneSetBonus> {
                    new RuneSetBonus {
                        SetCount = 2,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.Damage, 15f }
                        },
                        Description = "攻击+15"
                    },
                    new RuneSetBonus {
                        SetCount = 4,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.Damage, 30f },
                            { RuneAttribute.CritDamage, 0.15f }
                        },
                        Description = "攻击+30, 暴击伤害+15%"
                    }
                }
            };
            _setDatabase[RuneSet.Attack] = attackSet;

            // 防御套装 - 2件+防御 4件+生命
            RuneSetData defenseSet = new RuneSetData {
                Set = RuneSet.Defense,
                Name = "防御",
                Description = "提升防御力和生命值",
                IconName = "shield",
                Bonuses = new List<RuneSetBonus> {
                    new RuneSetBonus {
                        SetCount = 2,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.Defense, 15f }
                        },
                        Description = "防御+15"
                    },
                    new RuneSetBonus {
                        SetCount = 4,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.Defense, 30f },
                            { RuneAttribute.MaxHealth, 200f }
                        },
                        Description = "防御+30, 生命+200"
                    }
                }
            };
            _setDatabase[RuneSet.Defense] = defenseSet;

            // 生命套装 - 2件+生命 4件+生命恢复
            RuneSetData lifeSet = new RuneSetData {
                Set = RuneSet.Life,
                Name = "生命",
                Description = "大幅提升生命值",
                IconName = "heart",
                Bonuses = new List<RuneSetBonus> {
                    new RuneSetBonus {
                        SetCount = 2,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.MaxHealth, 100f }
                        },
                        Description = "生命+100"
                    },
                    new RuneSetBonus {
                        SetCount = 4,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.MaxHealth, 250f },
                            { RuneAttribute.HealthRegen, 5f }
                        },
                        Description = "生命+250, 生命恢复+5"
                    }
                }
            };
            _setDatabase[RuneSet.Life] = lifeSet;

            // 魔法套装 - 2件+魔法 4件+法力恢复
            RuneSetData magicSet = new RuneSetData {
                Set = RuneSet.Magic,
                Name = "魔法",
                Description = "提升魔法攻击和技能效率",
                IconName = "magic",
                Bonuses = new List<RuneSetBonus> {
                    new RuneSetBonus {
                        SetCount = 2,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.Damage, 15f }
                        },
                        Description = "魔法+15"
                    },
                    new RuneSetBonus {
                        SetCount = 4,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.Damage, 30f },
                            { RuneAttribute.ManaRegen, 5f }
                        },
                        Description = "魔法+30, 法力恢复+5"
                    }
                }
            };
            _setDatabase[RuneSet.Magic] = magicSet;

            // 速度套装 - 2件+速度 4件+闪避
            RuneSetData speedSet = new RuneSetData {
                Set = RuneSet.Speed,
                Name = "速度",
                Description = "提升速度和闪避",
                IconName = "wind",
                Bonuses = new List<RuneSetBonus> {
                    new RuneSetBonus {
                        SetCount = 2,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.MoveSpeed, 0.05f }
                        },
                        Description = "移动速度+5%"
                    },
                    new RuneSetBonus {
                        SetCount = 4,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.MoveSpeed, 0.10f },
                            { RuneAttribute.AttackSpeed, 0.08f }
                        },
                        Description = "移动速度+10%, 攻击速度+8%"
                    }
                }
            };
            _setDatabase[RuneSet.Speed] = speedSet;

            // 暴击套装 - 2件+暴击率 4件+暴击伤害
            RuneSetData criticalSet = new RuneSetData {
                Set = RuneSet.Critical,
                Name = "暴击",
                Description = "提升暴击能力",
                IconName = "lightning",
                Bonuses = new List<RuneSetBonus> {
                    new RuneSetBonus {
                        SetCount = 2,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.CritChance, 0.05f }
                        },
                        Description = "暴击率+5%"
                    },
                    new RuneSetBonus {
                        SetCount = 4,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.CritChance, 0.10f },
                            { RuneAttribute.CritDamage, 0.25f }
                        },
                        Description = "暴击率+10%, 暴击伤害+25%"
                    }
                }
            };
            _setDatabase[RuneSet.Critical] = criticalSet;

            // 均衡套装 - 2件+全属性 4件+战斗续航
            RuneSetData balanceSet = new RuneSetData {
                Set = RuneSet.Balance,
                Name = "均衡",
                Description = "全面提升属性",
                IconName = "balance",
                Bonuses = new List<RuneSetBonus> {
                    new RuneSetBonus {
                        SetCount = 2,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.MaxHealth, 50f },
                            { RuneAttribute.Damage, 5f },
                            { RuneAttribute.Defense, 5f }
                        },
                        Description = "生命+50, 攻击+5, 防御+5"
                    },
                    new RuneSetBonus {
                        SetCount = 4,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.MaxHealth, 100f },
                            { RuneAttribute.Damage, 10f },
                            { RuneAttribute.Defense, 10f }
                        },
                        Description = "生命+100, 攻击+10, 防御+10"
                    }
                }
            };
            _setDatabase[RuneSet.Balance] = balanceSet;

            // 龙之套装 - 传说套装
            RuneSetData dragonSet = new RuneSetData {
                Set = RuneSet.Dragon,
                Name = "龙之",
                Description = "传说龙族之力",
                IconName = "dragon",
                Bonuses = new List<RuneSetBonus> {
                    new RuneSetBonus {
                        SetCount = 2,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.Damage, 25f },
                            { RuneAttribute.CritChance, 0.05f }
                        },
                        Description = "攻击+25, 暴击率+5%"
                    },
                    new RuneSetBonus {
                        SetCount = 4,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.Damage, 50f },
                            { RuneAttribute.CritChance, 0.10f },
                            { RuneAttribute.CritDamage, 0.30f }
                        },
                        Description = "攻击+50, 暴击率+10%, 暴击伤害+30%"
                    }
                }
            };
            _setDatabase[RuneSet.Dragon] = dragonSet;

            // 凤凰套装 - 传说套装
            RuneSetData phoenixSet = new RuneSetData {
                Set = RuneSet.Phoenix,
                Name = "凤凰",
                Description = "凤凰涅槃之力",
                IconName = "phoenix",
                Bonuses = new List<RuneSetBonus> {
                    new RuneSetBonus {
                        SetCount = 2,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.MaxHealth, 150f },
                            { RuneAttribute.HealthRegen, 3f }
                        },
                        Description = "生命+150, 生命恢复+3"
                    },
                    new RuneSetBonus {
                        SetCount = 4,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.MaxHealth, 300f },
                            { RuneAttribute.HealthRegen, 8f },
                            { RuneAttribute.Defense, 20f }
                        },
                        Description = "生命+300, 生命恢复+8, 防御+20"
                    }
                }
            };
            _setDatabase[RuneSet.Phoenix] = phoenixSet;

            // 暗影套装 - 传说套装
            RuneSetData shadowSet = new RuneSetData {
                Set = RuneSet.Shadow,
                Name = "暗影",
                Description = "暗影刺客之力",
                IconName = "shadow",
                Bonuses = new List<RuneSetBonus> {
                    new RuneSetBonus {
                        SetCount = 2,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.Damage, 20f },
                            { RuneAttribute.AttackSpeed, 0.05f }
                        },
                        Description = "攻击+20, 攻击速度+5%"
                    },
                    new RuneSetBonus {
                        SetCount = 4,
                        Attributes = new Dictionary<RuneAttribute, float> {
                            { RuneAttribute.Damage, 40f },
                            { RuneAttribute.CritChance, 0.08f },
                            { RuneAttribute.CritDamage, 0.20f }
                        },
                        Description = "攻击+40, 暴击率+8%, 暴击伤害+20%"
                    }
                }
            };
            _setDatabase[RuneSet.Shadow] = shadowSet;
        }

        /// <summary>
        /// 获取符文集合数据
        /// </summary>
        public RuneSetData GetSetData(RuneSet set) {
            if (_setDatabase.ContainsKey(set)) {
                return _setDatabase[set];
            }
            return null;
        }

        /// <summary>
        /// 计算套装属性加成
        /// </summary>
        public Dictionary<RuneAttribute, float> CalculateSetBonuses(List<Rune> equippedRunes) {
            Dictionary<RuneAttribute, float> totalBonus = new Dictionary<RuneAttribute, float>();
            
            // 统计每个套装的有效符文数量
            Dictionary<RuneSet, int> setCounts = new Dictionary<RuneSet, int>();
            
            foreach (Rune rune in equippedRunes) {
                if (rune.Set != RuneSet.None) {
                    if (!setCounts.ContainsKey(rune.Set)) {
                        setCounts[rune.Set] = 0;
                    }
                    setCounts[rune.Set]++;
                }
            }

            // 计算每个套装的激活效果
            foreach (var kvp in setCounts) {
                RuneSet set = kvp.Key;
                int count = kvp.Value;
                
                RuneSetData setData = GetSetData(set);
                if (setData == null) continue;

                RuneSetBonus bonus = setData.GetBonusForCount(count);
                if (bonus != null && bonus.Attributes != null) {
                    foreach (var attr in bonus.Attributes) {
                        if (totalBonus.ContainsKey(attr.Key)) {
                            totalBonus[attr.Key] += attr.Value;
                        } else {
                            totalBonus[attr.Key] = attr.Value;
                        }
                    }
                }
            }

            return totalBonus;
        }

        /// <summary>
        /// 获取激活的套装信息
        /// </summary>
        public List<RuneSetActivationInfo> GetActiveSetInfo(List<Rune> equippedRunes) {
            List<RuneSetActivationInfo> activeSets = new List<RuneSetActivationInfo>();
            
            // 统计每个套装的有效符文数量
            Dictionary<RuneSet, int> setCounts = new Dictionary<RuneSet, int>();
            
            foreach (Rune rune in equippedRunes) {
                if (rune.Set != RuneSet.None) {
                    if (!setCounts.ContainsKey(rune.Set)) {
                        setCounts[rune.Set] = 0;
                    }
                    setCounts[rune.Set]++;
                }
            }

            // 获取激活的套装
            foreach (var kvp in setCounts) {
                RuneSet set = kvp.Key;
                int count = kvp.Value;
                
                RuneSetData setData = GetSetData(set);
                if (setData == null) continue;

                // 至少需要2件才能激活套装效果
                if (count >= 2) {
                    RuneSetBonus activeBonus = setData.GetBonusForCount(count);
                    if (activeBonus != null) {
                        activeSets.Add(new RuneSetActivationInfo {
                            Set = set,
                            SetName = setData.Name,
                            RuneCount = count,
                            ActiveBonus = activeBonus
                        });
                    }
                }
            }

            return activeSets;
        }

        /// <summary>
        /// 获取所有套装列表
        /// </summary>
        public List<RuneSetData> GetAllSets() {
            return new List<RuneSetData>(_setDatabase.Values);
        }
    }

    /// <summary>
    /// 符文套装激活信息
    /// </summary>
    public class RuneSetActivationInfo {
        public RuneSet Set { get; set; }
        public string SetName { get; set; }
        public int RuneCount { get; set; }
        public RuneSetBonus ActiveBonus { get; set; }
    }
}
