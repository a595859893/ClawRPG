using Godot;
using System;
using System.Collections.Generic;

namespace GameSystems {
    /// <summary>
    /// 公会数据库 - 配置所有公会相关数据
    /// </summary>
    // 公会数据库 - 配置所有公会相关数据
    public static class GuildDatabase {
        // 等级名称
        private static readonly string[] LevelNames = {
            "无", "新手", "成员", "长老", "副会长", "会长"
        };

        // 等级权限
        private static readonly GuildPermission[] LevelPermissions = {
            GuildPermission.None,
            GuildPermission.Invite,
            GuildPermission.Invite | GuildPermission.AcceptQuest,
            GuildPermission.Invite | GuildPermission.Kick | GuildPermission.Promote | GuildPermission.AcceptQuest,
            GuildPermission.Invite | GuildPermission.Kick | GuildPermission.Promote | GuildPermission.Demote | GuildPermission.ManageNotice | GuildPermission.ManageBank | GuildPermission.AcceptQuest,
            GuildPermission.All
        };

        // 公会等级经验需求
        private static readonly int[] LevelExpRequirements = {
            0, 1000, 5000, 20000, 50000, 100000
        };

        // 公会等级最大成员数
        private static readonly int[] LevelMaxMembers = {
            20, 30, 40, 50, 60, 100
        };

        // 获取等级名称
        public static string GetLevelName(GuildLevel level) {
            int index = (int)level;
            if (index >= 0 && index < LevelNames.Length) {
                return LevelNames[index];
            }
            return "未知";
        }

        // 获取等级权限
        public static GuildPermission GetLevelPermissions(GuildLevel level) {
            int index = (int)level;
            if (index >= 0 && index < LevelPermissions.Length) {
                return LevelPermissions[index];
            }
            return GuildPermission.None;
        }

        // 获取等级经验需求
        public static int GetLevelExpRequirement(int level) {
            if (level >= 0 && level < LevelExpRequirements.Length) {
                return LevelExpRequirements[level];
            }
            return 999999;
        }

        // 获取等级最大成员数
        public static int GetLevelMaxMembers(int level) {
            if (level >= 0 && level < LevelMaxMembers.Length) {
                return LevelMaxMembers[level];
            }
            return 20;
        }

        // 默认建筑配置
        public static Dictionary<string, GuildBuilding> GetDefaultBuildings() {
            var buildings = new Dictionary<string, GuildBuilding>();
            
            // 大厅 - 公会核心
            buildings["hall"] = new GuildBuilding {
                BuildingId = "hall",
                Name = "公会大厅",
                Description = "公会核心建筑，解锁更多功能",
                Level = 1,
                MaxLevel = 10,
                UpgradeCost = 1000,
                UpgradeRequirement = 100,
                Bonuses = new Dictionary<string, int> {
                    { "max_members", 5 },
                    { "exp_bonus", 5 }
                }
            };

            // 仓库 - 存储物品
            buildings["warehouse"] = new GuildBuilding {
                BuildingId = "warehouse",
                Name = "公会仓库",
                Description = "公会共享物品仓库",
                Level = 1,
                MaxLevel = 10,
                UpgradeCost = 800,
                UpgradeRequirement = 80,
                Bonuses = new Dictionary<string, int> {
                    { "storage_slots", 10 },
                    { "item_quality", 1 }
                }
            };

            // 训练场 - 提升经验
            buildings["training"] = new GuildBuilding {
                BuildingId = "training",
                Name = "训练场",
                Description = "成员经验加成",
                Level = 1,
                MaxLevel = 10,
                UpgradeCost = 600,
                UpgradeRequirement = 60,
                Bonuses = new Dictionary<string, int> {
                    { "exp_bonus", 10 }
                }
            };

            // 商店 - 公会专属商店
            buildings["shop"] = new GuildBuilding {
                BuildingId = "shop",
                Name = "公会商店",
                Description = "公会专属物品",
                Level = 1,
                MaxLevel = 5,
                UpgradeCost = 500,
                UpgradeRequirement = 50,
                Bonuses = new Dictionary<string, int> {
                    { "discount", 5 }
                }
            };

            // 祭坛 - 增益效果
            buildings["altar"] = new GuildBuilding {
                BuildingId = "altar",
                Name = "公会祭坛",
                Description = "全员属性加成",
                Level = 1,
                MaxLevel = 5,
                UpgradeCost = 1200,
                UpgradeRequirement = 120,
                Bonuses = new Dictionary<string, int> {
                    { "attack_bonus", 2 },
                    { "defense_bonus", 2 }
                }
            };

            return buildings;
        }

        // 默认技能配置
        public static Dictionary<string, GuildSkill> GetDefaultSkills() {
            var skills = new Dictionary<string, GuildSkill>();

            // 战斗加成
            skills["battle_buff"] = new GuildSkill {
                SkillId = "battle_buff",
                Name = "战斗增益",
                Description = "全体成员战斗属性提升",
                Level = 0,
                MaxLevel = 5,
                CostPerLevel = 200,
                Bonuses = new Dictionary<string, int> {
                    { "attack", 5 },
                    { "defense", 5 }
                }
            };

            // 采集加成
            skills["gather_buff"] = new GuildSkill {
                SkillId = "gather_buff",
                Name = "采集增益",
                Description = "采集获得额外产出",
                Level = 0,
                MaxLevel = 5,
                CostPerLevel = 150,
                Bonuses = new Dictionary<string, int> {
                    { "gather_amount", 10 }
                }
            };

            // 经验加成
            skills["exp_buff"] = new GuildSkill {
                SkillId = "exp_buff",
                Name = "经验加成",
                Description = "获得更多经验",
                Level = 0,
                MaxLevel = 5,
                CostPerLevel = 250,
                Bonuses = new Dictionary<string, int> {
                    { "exp_rate", 5 }
                }
            };

            // 掉落加成
            skills["drop_buff"] = new GuildSkill {
                SkillId = "drop_buff",
                Name = "掉落增益",
                Description = "掉落率提升",
                Level = 0,
                MaxLevel = 5,
                CostPerLevel = 300,
                Bonuses = new Dictionary<string, int> {
                    { "drop_rate", 3 }
                }
            };

            // 治疗加成
            skills["heal_buff"] = new GuildSkill {
                SkillId = "heal_buff",
                Name = "治疗增益",
                Description = "治疗效果提升",
                Level = 0,
                MaxLevel = 5,
                CostPerLevel = 180,
                Bonuses = new Dictionary<string, int> {
                    { "heal_bonus", 10 }
                }
            };

            // 移动速度
            skills["speed_buff"] = new GuildSkill {
                SkillId = "speed_buff",
                Name = "移动增益",
                Description = "移动速度提升",
                Level = 0,
                MaxLevel = 3,
                CostPerLevel = 120,
                Bonuses = new Dictionary<string, int> {
                    { "move_speed", 5 }
                }
            };

            return skills;
        }

        // 创建新公会
        public static GuildData CreateNewGuild(string guildId, string name, string leaderId, string leaderName) {
            var guild = new GuildData {
                GuildId = guildId,
                Name = name,
                LeaderId = leaderId,
                LeaderName = leaderName,
                Description = "新成立的公会",
                Notice = "欢迎加入我们的公会！",
                Level = 1,
                MaxMembers = GetLevelMaxMembers(1),
                CreateTime = DateTime.Now,
                LastActivity = DateTime.Now,
                Buildings = GetDefaultBuildings(),
                Skills = GetDefaultSkills()
            };

            // 添加会长为第一个成员
            var leader = new GuildMember {
                PlayerId = leaderId,
                PlayerName = leaderName,
                Level = GuildLevel.Leader,
                Permissions = GuildPermission.All,
                JoinDate = DateTime.Now,
                LastActive = DateTime.Now,
                IsOnline = true
            };
            guild.Members.Add(leader);
            guild.CurrentMembers = 1;

            return guild;
        }

        // 计算升级所需经验
        public static int GetUpgradeExp(int currentLevel) {
            if (currentLevel >= LevelExpRequirements.Length - 1) {
                return 999999; // 已满级
            }
            return LevelExpRequirements[currentLevel + 1] - LevelExpRequirements[currentLevel];
        }

        // 获取等级名称
        public static string GetGuildLevelName(int level) {
            if (level <= 0) return "无";
            if (level >= LevelNames.Length) return "传奇";
            return level + "级";
        }
    }
}
