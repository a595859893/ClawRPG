using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物训练数据库
    /// </summary>
    public static class PetTrainingDatabase
    {
        // 训练项目列表
        public static List<PetTrainingData.TrainingProject> TrainingProjects { get; private set; } = new();

        // 初始化数据库
        public static void Initialize()
        {
            TrainingProjects.Clear();

            // 攻击训练项目
            AddProject("train_attack_1", "基础攻击训练", "提升宠物基础攻击力", 
                PetTrainingData.TrainingType.Attack, 1, 1, 50, 10, 
                5, 0, 0, 0, 0, 0, 0, 60, new int[] { }, new int[] { });
            AddProject("train_attack_2", "进阶攻击训练", "显著提升宠物攻击力", 
                PetTrainingData.TrainingType.Attack, 2, 5, 150, 25, 
                12, 0, 0, 0, 0, 0, 0, 120, new int[] { }, new int[] { });
            AddProject("train_attack_3", "高级攻击训练", "大幅提升宠物攻击力", 
                PetTrainingData.TrainingType.Attack, 3, 10, 400, 50, 
                20, 0, 0, 0, 0, 0, 0, 180, new int[] { }, new int[] { });
            AddProject("train_attack_4", "专家攻击训练", "专家级攻击能力提升", 
                PetTrainingData.TrainingType.Attack, 4, 20, 1000, 100, 
                30, 0, 0, 0, 0, 5, 0, 240, new int[] { }, new int[] { });
            AddProject("train_attack_5", "大师攻击训练", "大师级攻击精通", 
                PetTrainingData.TrainingType.Attack, 5, 30, 2500, 200, 
                45, 0, 0, 0, 0, 8, 0, 300, new int[] { }, new int[] { });

            // 防御训练项目
            AddProject("train_defense_1", "基础防御训练", "提升宠物基础防御力", 
                PetTrainingData.TrainingType.Defense, 1, 1, 50, 10, 
                0, 5, 0, 0, 0, 0, 0, 60, new int[] { }, new int[] { });
            AddProject("train_defense_2", "进阶防御训练", "显著提升宠物防御力", 
                PetTrainingData.TrainingType.Defense, 2, 5, 150, 25, 
                0, 12, 0, 0, 0, 0, 0, 120, new int[] { }, new int[] { });
            AddProject("train_defense_3", "高级防御训练", "大幅提升宠物防御力", 
                PetTrainingData.TrainingType.Defense, 3, 10, 400, 50, 
                0, 20, 0, 0, 0, 0, 0, 180, new int[] { }, new int[] { });
            AddProject("train_defense_4", "专家防御训练", "专家级防御能力提升", 
                PetTrainingData.TrainingType.Defense, 4, 20, 1000, 100, 
                0, 30, 0, 0, 5, 0, 0, 240, new int[] { }, new int[] { });
            AddProject("train_defense_5", "大师防御训练", "大师级防御精通", 
                PetTrainingData.TrainingType.Defense, 5, 30, 2500, 200, 
                0, 45, 0, 0, 8, 0, 0, 300, new int[] { }, new int[] { });

            // 速度训练项目
            AddProject("train_speed_1", "基础速度训练", "提升宠物移动速度", 
                PetTrainingData.TrainingType.Speed, 1, 1, 50, 10, 
                0, 0, 0, 5, 0, 0, 0, 60, new int[] { }, new int[] { });
            AddProject("train_speed_2", "进阶速度训练", "显著提升宠物速度", 
                PetTrainingData.TrainingType.Speed, 2, 5, 150, 25, 
                0, 0, 0, 12, 0, 0, 0, 120, new int[] { }, new int[] { });
            AddProject("train_speed_3", "高级速度训练", "大幅提升宠物速度", 
                PetTrainingData.TrainingType.Speed, 3, 10, 400, 50, 
                0, 0, 0, 20, 0, 0, 0, 180, new int[] { }, new int[] { });
            AddProject("train_speed_4", "专家速度训练", "专家级速度提升", 
                PetTrainingData.TrainingType.Speed, 4, 20, 1000, 100, 
                0, 0, 0, 30, 0, 0, 5, 240, new int[] { }, new int[] { });
            AddProject("train_speed_5", "大师速度训练", "大师级速度精通", 
                PetTrainingData.TrainingType.Speed, 5, 30, 2500, 200, 
                0, 0, 0, 45, 0, 0, 8, 300, new int[] { }, new int[] { });

            // 生命训练项目
            AddProject("train_health_1", "基础生命训练", "提升宠物最大生命值", 
                PetTrainingData.TrainingType.Health, 1, 1, 50, 10, 
                0, 0, 20, 0, 0, 0, 0, 60, new int[] { }, new int[] { });
            AddProject("train_health_2", "进阶生命训练", "显著提升宠物生命值", 
                PetTrainingData.TrainingType.Health, 2, 5, 150, 25, 
                0, 0, 50, 0, 0, 0, 0, 120, new int[] { }, new int[] { });
            AddProject("train_health_3", "高级生命训练", "大幅提升宠物生命值", 
                PetTrainingData.TrainingType.Health, 3, 10, 400, 50, 
                0, 0, 100, 0, 0, 0, 0, 180, new int[] { }, new int[] { });
            AddProject("train_health_4", "专家生命训练", "专家级生命提升", 
                PetTrainingData.TrainingType.Health, 4, 20, 1000, 100, 
                0, 0, 150, 0, 0, 0, 8, 240, new int[] { }, new int[] { });
            AddProject("train_health_5", "大师生命训练", "大师级生命精通", 
                PetTrainingData.TrainingType.Health, 5, 30, 2500, 200, 
                0, 0, 200, 0, 0, 0, 12, 300, new int[] { }, new int[] { });

            // 暴击训练项目
            AddProject("train_critical_1", "基础暴击训练", "提升宠物暴击率", 
                PetTrainingData.TrainingType.Critical, 1, 1, 80, 15, 
                0, 0, 0, 0, 3, 0, 0, 90, new int[] { }, new int[] { });
            AddProject("train_critical_2", "进阶暴击训练", "显著提升宠物暴击率", 
                PetTrainingData.TrainingType.Critical, 2, 8, 250, 40, 
                0, 0, 0, 0, 6, 5, 0, 150, new int[] { }, new int[] { });
            AddProject("train_critical_3", "高级暴击训练", "大幅提升宠物暴击", 
                PetTrainingData.TrainingType.Critical, 3, 15, 600, 80, 
                0, 0, 0, 0, 10, 10, 0, 210, new int[] { }, new int[] { });
            AddProject("train_critical_4", "专家暴击训练", "专家级暴击精通", 
                PetTrainingData.TrainingType.Critical, 4, 25, 1500, 150, 
                0, 0, 0, 0, 15, 15, 10, 270, new int[] { }, new int[] { });
            AddProject("train_critical_5", "大师暴击训练", "大师级暴击掌握", 
                PetTrainingData.TrainingType.Critical, 5, 35, 3500, 250, 
                0, 0, 0, 0, 20, 20, 15, 330, new int[] { }, new int[] { });

            // 特殊训练项目
            AddProject("train_special_1", "战斗本能", "激发宠物战斗本能", 
                PetTrainingData.TrainingType.Special, 1, 5, 200, 30, 
                5, 5, 10, 2, 2, 2, 0, 180, new int[] { }, new int[] { });
            AddProject("train_special_2", "战斗精通", "精通战斗技巧", 
                PetTrainingData.TrainingType.Special, 2, 12, 500, 60, 
                10, 10, 20, 4, 4, 4, 0, 240, new int[] { }, new int[] { });
            AddProject("train_special_3", "战斗大师", "成为战斗大师", 
                PetTrainingData.TrainingType.Special, 3, 20, 1200, 120, 
                15, 15, 30, 6, 6, 6, 5, 300, new int[] { }, new int[] { });
            AddProject("train_special_4", "战斗传奇", "传奇战斗能力", 
                PetTrainingData.TrainingType.Special, 4, 30, 3000, 200, 
                25, 25, 50, 10, 10, 10, 10, 360, new int[] { }, new int[] { });
            AddProject("train_special_5", "战斗神话", "神话级战斗能力", 
                PetTrainingData.TrainingType.Special, 5, 40, 8000, 350, 
                40, 40, 80, 15, 15, 15, 15, 420, new int[] { }, new int[] { });

            GD.Print($"宠物训练数据库已初始化: {TrainingProjects.Count} 个训练项目");
        }

        private static void AddProject(string id, string name, string description, 
            PetTrainingData.TrainingType type, int level, int requiredLevel,
            int goldCost, int trainingPoints, float attack, float defense, 
            float health, float speed, float critRate, float critDamage, float lifeSteal,
            int duration, int[] materials, int[] materialCounts)
        {
            var project = new PetTrainingData.TrainingProject
            {
                Id = id,
                Name = name,
                Description = description,
                Type = type,
                Level = level,
                RequiredLevel = requiredLevel,
                GoldCost = goldCost,
                TrainingPoints = trainingPoints,
                AttackBonus = attack,
                DefenseBonus = defense,
                HealthBonus = health,
                SpeedBonus = speed,
                CriticalRateBonus = critRate,
                CriticalDamageBonus = critDamage,
                LifeStealBonus = lifeSteal,
                Duration = duration,
                RequiredMaterials = materials,
                MaterialCounts = materialCounts
            };
            TrainingProjects.Add(project);
        }

        // 获取指定类型的训练项目
        public static List<PetTrainingData.TrainingProject> GetProjectsByType(PetTrainingData.TrainingType type)
        {
            var result = new List<PetTrainingData.TrainingProject>();
            foreach (var project in TrainingProjects)
            {
                if (project.Type == type)
                    result.Add(project);
            }
            return result;
        }

        // 获取指定等级的训练项目
        public static List<PetTrainingData.TrainingProject> GetProjectsByLevel(int level)
        {
            var result = new List<PetTrainingData.TrainingProject>();
            foreach (var project in TrainingProjects)
            {
                if (project.Level == level)
                    result.Add(project);
            }
            return result;
        }

        // 根据ID获取训练项目
        public static PetTrainingData.TrainingProject GetProject(string id)
        {
            foreach (var project in TrainingProjects)
            {
                if (project.Id == id)
                    return project;
            }
            return null;
        }
    }
}
