using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 随机挑战数据库 - 存储和管理程序生成挑战的模板
/// 包含挑战类型定义、奖励配置等
/// </summary>
public class ProceduralChallengeDatabase
{
    private static ProceduralChallengeData.ChallengeTemplate[] _templates;
    private static Dictionary<string, ProceduralChallengeData.ChallengeTemplate> _templateMap;
    private static Random _random = new Random();

    public static void Initialize()
    {
        _templates = new ProceduralChallengeData.ChallengeTemplate[]
        {
            // Kill Enemies Challenges
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "kill_goblins",
                Name = "Goblin Hunt",
                Description = "Defeat {count} goblins",
                Type = ProceduralChallengeData.ChallengeType.KillEnemies,
                Rarity = ProceduralChallengeData.ChallengeRarity.Common,
                BaseRequirement = 20,
                BaseTimeLimit = 300,
                BaseGoldReward = 100,
                BaseExpReward = 50
            },
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "kill_elites",
                Name = "Elite Slayer",
                Description = "Defeat {count} elite enemies",
                Type = ProceduralChallengeData.ChallengeType.KillEnemies,
                Rarity = ProceduralChallengeData.ChallengeRarity.Rare,
                BaseRequirement = 10,
                BaseTimeLimit = 600,
                BaseGoldReward = 500,
                BaseExpReward = 250
            },
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "kill_bosses",
                Name = "Boss Hunter",
                Description = "Defeat {count} bosses",
                Type = ProceduralChallengeData.ChallengeType.DefeatBoss,
                Rarity = ProceduralChallengeData.ChallengeRarity.Epic,
                BaseRequirement = 3,
                BaseTimeLimit = 900,
                BaseGoldReward = 1000,
                BaseExpReward = 500
            },

            // Survive Waves Challenges
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "survive_waves_1",
                Name = "Wave Survivor",
                Description = "Survive {count} waves of enemies",
                Type = ProceduralChallengeData.ChallengeType.SurviveWaves,
                Rarity = ProceduralChallengeData.ChallengeRarity.Uncommon,
                BaseRequirement = 5,
                BaseTimeLimit = 600,
                BaseGoldReward = 200,
                BaseExpReward = 100
            },
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "survive_waves_2",
                Name = "Endless Wave",
                Description = "Survive {count} waves without dying",
                Type = ProceduralChallengeData.ChallengeType.SurviveWaves,
                Rarity = ProceduralChallengeData.ChallengeRarity.Epic,
                BaseRequirement = 10,
                BaseTimeLimit = 1200,
                BaseGoldReward = 800,
                BaseExpReward = 400
            },

            // Collect Items Challenges
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "collect_gold",
                Name = "Gold Collector",
                Description = "Collect {count} gold coins",
                Type = ProceduralChallengeData.ChallengeType.CollectItems,
                Rarity = ProceduralChallengeData.ChallengeRarity.Common,
                BaseRequirement = 500,
                BaseTimeLimit = 300,
                BaseGoldReward = 50,
                BaseExpReward = 25
            },
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "collect_herbs",
                Name = "Herbalist",
                Description = "Collect {count} herbs",
                Type = ProceduralChallengeData.ChallengeType.CollectItems,
                Rarity = ProceduralChallengeData.ChallengeRarity.Uncommon,
                BaseRequirement = 15,
                BaseTimeLimit = 600,
                BaseGoldReward = 150,
                BaseExpReward = 75
            },

            // Time Trial Challenges
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "time_trial_easy",
                Name = "Speed Run",
                Description = "Complete the challenge within {time} seconds",
                Type = ProceduralChallengeData.ChallengeType.TimeTrial,
                Rarity = ProceduralChallengeData.ChallengeRarity.Uncommon,
                BaseRequirement = 120,
                BaseTimeLimit = 120,
                BaseGoldReward = 200,
                BaseExpReward = 100
            },
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "time_trial_hard",
                Name = "Lightning Run",
                Description = "Complete the challenge within {time} seconds",
                Type = ProceduralChallengeData.ChallengeType.TimeTrial,
                Rarity = ProceduralChallengeData.ChallengeRarity.Legendary,
                BaseRequirement = 60,
                BaseTimeLimit = 60,
                BaseGoldReward = 2000,
                BaseExpReward = 1000
            },

            // No Damage Challenges
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "no_damage_easy",
                Name = "Perfect Defense",
                Description = "Complete the challenge without taking damage",
                Type = ProceduralChallengeData.ChallengeType.NoDamage,
                Rarity = ProceduralChallengeData.ChallengeRarity.Rare,
                BaseRequirement = 1,
                BaseTimeLimit = 600,
                BaseGoldReward = 400,
                BaseExpReward = 200
            },
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "no_damage_hard",
                Name = "Invincible",
                Description = "Defeat {count} enemies without taking damage",
                Type = ProceduralChallengeData.ChallengeType.NoDamage,
                Rarity = ProceduralChallengeData.ChallengeRarity.Legendary,
                BaseRequirement = 10,
                BaseTimeLimit = 900,
                BaseGoldReward = 3000,
                BaseExpReward = 1500
            },

            // Limited Resources Challenges
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "no_potions",
                Name = "Survivor",
                Description = "Complete the challenge without using potions",
                Type = ProceduralChallengeData.ChallengeType.LimitedResources,
                Rarity = ProceduralChallengeData.ChallengeRarity.Rare,
                BaseRequirement = 1,
                BaseTimeLimit = 600,
                BaseGoldReward = 350,
                BaseExpReward = 175
            },
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "no_skills",
                Name = "Bare Hands",
                Description = "Complete the challenge without using skills",
                Type = ProceduralChallengeData.ChallengeType.LimitedResources,
                Rarity = ProceduralChallengeData.ChallengeRarity.Epic,
                BaseRequirement = 1,
                BaseTimeLimit = 600,
                BaseGoldReward = 600,
                BaseExpReward = 300
            },

            // Solo Challenge
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "solo_easy",
                Name = "Lone Wolf",
                Description = "Complete the challenge without any companions",
                Type = ProceduralChallengeData.ChallengeType.SoloChallenge,
                Rarity = ProceduralChallengeData.ChallengeRarity.Uncommon,
                BaseRequirement = 1,
                BaseTimeLimit = 600,
                BaseGoldReward = 250,
                BaseExpReward = 125
            },

            // Endurance Challenges
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "endurance_1",
                Name = "Marathon",
                Description = "Survive for {time} seconds continuously",
                Type = ProceduralChallengeData.ChallengeType.Endurance,
                Rarity = ProceduralChallengeData.ChallengeRarity.Rare,
                BaseRequirement = 180,
                BaseTimeLimit = 200,
                BaseGoldReward = 400,
                BaseExpReward = 200
            },
            new ProceduralChallengeData.ChallengeTemplate
            {
                Id = "endurance_2",
                Name = "Iron Will",
                Description = "Survive for {time} seconds without healing",
                Type = ProceduralChallengeData.ChallengeType.Endurance,
                Rarity = ProceduralChallengeData.ChallengeRarity.Epic,
                BaseRequirement = 120,
                BaseTimeLimit = 130,
                BaseGoldReward = 700,
                BaseExpReward = 350
            }
        };

        _templateMap = new Dictionary<string, ProceduralChallengeData.ChallengeTemplate>();
        foreach (var template in _templates)
        {
            _templateMap[template.Id] = template;
        }
    }

    public static ProceduralChallengeData.ChallengeTemplate GetTemplate(string id)
    {
        if (_templateMap == null) Initialize();
        return _templateMap.ContainsKey(id) ? _templateMap[id] : null;
    }

    public static ProceduralChallengeData.ChallengeTemplate[] GetAllTemplates()
    {
        if (_templateMap == null) Initialize();
        return _templates;
    }

    public static ProceduralChallengeData.ChallengeTemplate[] GetTemplatesByRarity(ProceduralChallengeData.ChallengeRarity rarity)
    {
        if (_templateMap == null) Initialize();
        List<ProceduralChallengeData.ChallengeTemplate> result = new List<ProceduralChallengeData.ChallengeTemplate>();
        foreach (var template in _templates)
        {
            if (template.Rarity == rarity)
                result.Add(template);
        }
        return result.ToArray();
    }

    public static ProceduralChallengeData.ChallengeTemplate GenerateRandomChallenge(int playerLevel)
    {
        if (_templateMap == null) Initialize();
        
        // Weight towards higher rarities for higher level players
        float[] rarityWeights = GetRarityWeights(playerLevel);
        float totalWeight = 0;
        foreach (float w in rarityWeights) totalWeight += w;

        float roll = (float)(_random.NextDouble() * totalWeight);
        ProceduralChallengeData.ChallengeRarity selectedRarity = ProceduralChallengeData.ChallengeRarity.Common;
        
        float cumulative = 0;
        for (int i = 0; i < rarityWeights.Length; i++)
        {
            cumulative += rarityWeights[i];
            if (roll <= cumulative)
            {
                selectedRarity = (ProceduralChallengeData.ChallengeRarity)i;
                break;
            }
        }

        // Get templates of selected rarity
        var templates = GetTemplatesByRarity(selectedRarity);
        if (templates.Length == 0)
            templates = _templates;

        return templates[_random.Next(templates.Length)];
    }

    private static float[] GetRarityWeights(int playerLevel)
    {
        // Weights shift towards higher rarities as player levels up
        float baseCommon = Math.Max(30 - playerLevel * 0.5f, 10);
        float baseUncommon = Math.Max(30 - playerLevel * 0.3f, 15);
        float baseRare = Math.Max(20 + playerLevel * 0.2f, 15);
        float baseEpic = Math.Max(15 + playerLevel * 0.4f, 8);
        float baseLegendary = Math.Max(5 + playerLevel * 0.2f, 2);

        return new float[] { baseCommon, baseUncommon, baseRare, baseEpic, baseLegendary };
    }

    public static string GetRarityColor(ProceduralChallengeData.ChallengeRarity rarity)
    {
        switch (rarity)
        {
            case ProceduralChallengeData.ChallengeRarity.Common: return "#9E9E9E";
            case ProceduralChallengeData.ChallengeRarity.Uncommon: return "#4CAF50";
            case ProceduralChallengeData.ChallengeRarity.Rare: return "#2196F3";
            case ProceduralChallengeData.ChallengeRarity.Epic: return "#9C27B0";
            case ProceduralChallengeData.ChallengeRarity.Legendary: return "#FF9800";
            default: return "#FFFFFF";
        }
    }
}
