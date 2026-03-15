using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Pet AI Improvements Data
/// Enhanced pet AI behaviors and learning systems
/// </summary>
public class PetAIImprovementsData : BaseSystem
{
    // 内部类定义
    public class PetAIPersonality
    {
        public enum PersonalityType
        {
            AGGRESSIVE = 0,
            DEFENSIVE = 1,
            SUPPORTIVE = 2,
            CURIOUS = 3,
            LAZY = 4
        }

        public int personalityType = (int)PersonalityType.AGGRESSIVE;
        public float curiosityLevel = 0.5f;
        public float energyLevel = 1.0f;
        public float loyaltyLevel = 0.5f;
        public float aggressionModifier = 1.0f;
        public float defenseModifier = 1.0f;

        public PetAIPersonality(int type = (int)PersonalityType.AGGRESSIVE)
        {
            personalityType = type;
            switch (type)
            {
                case (int)PersonalityType.AGGRESSIVE:
                    aggressionModifier = 1.5f;
                    defenseModifier = 0.8f;
                    break;
                case (int)PersonalityType.DEFENSIVE:
                    aggressionModifier = 0.7f;
                    defenseModifier = 1.5f;
                    break;
                case (int)PersonalityType.SUPPORTIVE:
                    aggressionModifier = 0.8f;
                    defenseModifier = 1.0f;
                    break;
                case (int)PersonalityType.CURIOUS:
                    curiosityLevel = 0.9f;
                    aggressionModifier = 1.0f;
                    break;
                case (int)PersonalityType.LAZY:
                    energyLevel = 0.3f;
                    aggressionModifier = 0.5f;
                    break;
            }
        }

        public string GetStateName()
        {
            return ((PersonalityType)personalityType).ToString();
        }
    }

    public class PetAIBehavior
    {
        public enum BehaviorState
        {
            IDLE = 0,
            PATROL = 1,
            CHASE = 2,
            ATTACK = 3,
            RETREAT = 4,
            FOLLOW = 5,
            EXPLORE = 6,
            HEAL = 7
        }

        public int currentState = (int)BehaviorState.IDLE;
        public Vector2 targetPosition = Vector2.Zero;
        public Node targetEntity = null;
        public float stateTimer = 0f;
        public int behaviorPriority = 0;

        public string GetStateName()
        {
            return ((BehaviorState)currentState).ToString();
        }
    }

    public class PetAILearning
    {
        public Dictionary learningData = new Dictionary();
        public Dictionary enemyTypeKills = new Dictionary();
        public Array playerActionMimic = new Array();
        public float adaptationLevel = 0f;
        public Array preferredTactics = new Array();

        public PetAILearning()
        {
            learningData = new Dictionary
            {
                { "total_battles", 0 },
                { "wins", 0 },
                { "losses", 0 },
                { "dodge_count", 0 },
                { "block_count", 0 },
                { "heal_count", 0 },
                { "combo_count", 0 },
                { "best_combo", 0 },
                { "average_response_time", 0.0 },
                { "preferred_enemy_types", new Array() },
                { "weak_against", new Array() }
            };
        }

        public void RecordBattleResult(bool won)
        {
            learningData["total_battles"] = (int)learningData["total_battles"] + 1;
            if (won)
            {
                learningData["wins"] = (int)learningData["wins"] + 1;
            }
            else
            {
                learningData["losses"] = (int)learningData["losses"] + 1;
            }
            UpdateAdaptationLevel();
        }

        public void RecordEnemyKilled(string enemyType)
        {
            if (!enemyTypeKills.ContainsKey(enemyType))
            {
                enemyTypeKills[enemyType] = 0;
            }
            enemyTypeKills[enemyType] = (int)enemyTypeKills[enemyType] + 1;
        }

        public void RecordDodge()
        {
            learningData["dodge_count"] = (int)learningData["dodge_count"] + 1;
        }

        public void RecordBlock()
        {
            learningData["block_count"] = (int)learningData["block_count"] + 1;
        }

        public void RecordHeal()
        {
            learningData["heal_count"] = (int)learningData["heal_count"] + 1;
        }

        public void RecordCombo(int comboSize)
        {
            learningData["combo_count"] = (int)learningData["combo_count"] + 1;
            if (comboSize > (int)learningData["best_combo"])
            {
                learningData["best_combo"] = comboSize;
            }
        }

        public void UpdateAdaptationLevel()
        {
            float total = (int)learningData["total_battles"];
            if (total > 0)
            {
                adaptationLevel = Mathf.Min(1.0f, total / 100.0f);
            }
        }

        public float GetWinRate()
        {
            int total = (int)learningData["total_battles"];
            if (total == 0) return 0f;
            return (int)learningData["wins"] / (float)total;
        }

        public string GetMostKilledEnemy()
        {
            int maxKills = 0;
            string result = "";
            foreach (string enemyType in enemyTypeKills.Keys)
            {
                if ((int)enemyTypeKills[enemyType] > maxKills)
                {
                    maxKills = (int)enemyTypeKills[enemyType];
                    result = enemyType;
                }
            }
            return result;
        }
    }

    public class PetAIDecision
    {
        public Dictionary decisionWeights = new Dictionary();
        public float personalityInfluence = 0.5f;
        public float learningInfluence = 0.3f;
        public float situationInfluence = 0.2f;

        public PetAIDecision()
        {
            decisionWeights = new Dictionary
            {
                { "attack", 1.0 },
                { "defend", 1.0 },
                { "support", 1.0 },
                { "retreat", 1.0 },
                { "explore", 1.0 }
            };
        }

        public string CalculateDecision(PetAIPersonality personality, PetAILearning learning, Dictionary situation)
        {
            var weights = new Dictionary(decisionWeights);

            // 性格影响
            switch (personality.personalityType)
            {
                case (int)PetAIPersonality.PersonalityType.AGGRESSIVE:
                    weights["attack"] = (float)weights["attack"] * personality.aggressionModifier * personalityInfluence * 2.0;
                    weights["retreat"] = (float)weights["retreat"] * 0.5;
                    break;
                case (int)PetAIPersonality.PersonalityType.DEFENSIVE:
                    weights["defend"] = (float)weights["defend"] * personality.defenseModifier * personalityInfluence * 2.0;
                    weights["attack"] = (float)weights["attack"] * 0.7;
                    break;
                case (int)PetAIPersonality.PersonalityType.SUPPORTIVE:
                    weights["support"] = (float)weights["support"] * personalityInfluence * 2.0;
                    weights["attack"] = (float)weights["attack"] * 0.8;
                    break;
                case (int)PetAIPersonality.PersonalityType.CURIOUS:
                    weights["explore"] = (float)weights["explore"] * personality.curiosityLevel * personalityInfluence * 2.0;
                    break;
            }

            // 学习影响
            float winRate = learning.GetWinRate();
            if (winRate > 0.7f)
            {
                weights["attack"] = (float)weights["attack"] * (1.0 + learningInfluence);
            }
            else if (winRate < 0.4f)
            {
                weights["defend"] = (float)weights["defend"] * (1.0 + learningInfluence);
                weights["retreat"] = (float)weights["retreat"] * (1.0 + learningInfluence * 0.5);
            }

            // 情况影响
            if (situation.ContainsKey("player_health"))
            {
                float playerHealth = (float)situation["player_health"];
                if (playerHealth < 0.3f)
                {
                    weights["support"] = (float)weights["support"] * situationInfluence * 3.0;
                    weights["defend"] = (float)weights["defend"] * situationInfluence * 2.0;
                }
            }

            if (situation.ContainsKey("enemy_count"))
            {
                int enemyCount = (int)situation["enemy_count"];
                if (enemyCount > 3)
                {
                    weights["attack"] = (float)weights["attack"] * 0.7;
                    weights["defend"] = (float)weights["defend"] * 1.5;
                }
            }

            // 选择最高权重
            string bestDecision = "attack";
            float bestWeight = 0f;
            foreach (string decision in weights.Keys)
            {
                if ((float)weights[decision] > bestWeight)
                {
                    bestWeight = (float)weights[decision];
                    bestDecision = decision;
                }
            }

            return bestDecision;
        }
    }

    public class PetAIEmotionalState
    {
        public enum Emotion
        {
            HAPPY = 0,
            SAD = 1,
            ANGRY = 2,
            EXCITED = 3,
            SCARED = 4,
            CALM = 5
        }

        public int currentEmotion = (int)Emotion.HAPPY;
        public float emotionIntensity = 0.5f;
        public float moodTimer = 0f;
        public Array emotionHistory = new Array();

        public PetAIEmotionalState()
        {
            emotionHistory = new Array();
        }

        public void UpdateEmotion(int newEmotion, float intensity = 0.5f)
        {
            currentEmotion = newEmotion;
            emotionIntensity = Mathf.Clamp(intensity, 0f, 1f);
            moodTimer = 0f;

            var entry = new Dictionary
            {
                { "emotion", newEmotion },
                { "intensity", intensity },
                { "time", Time.GetUnixTimeFromSystem() }
            };
            emotionHistory.Add(entry);

            // 保持历史在最近20条
            if (emotionHistory.Count > 20)
            {
                emotionHistory.RemoveAt(0);
            }
        }

        public string GetEmotionName()
        {
            switch (currentEmotion)
            {
                case (int)Emotion.HAPPY: return "Happy";
                case (int)Emotion.SAD: return "Sad";
                case (int)Emotion.ANGRY: return "Angry";
                case (int)Emotion.EXCITED: return "Excited";
                case (int)Emotion.SCARED: return "Scared";
                case (int)Emotion.CALM: return "Calm";
            }
            return "Unknown";
        }

        public void UpdateEmotionFromBattle(bool win, float playerHealthChange)
        {
            if (win)
            {
                UpdateEmotion((int)Emotion.EXCITED, 0.8f);
            }
            else if (playerHealthChange < -0.3f)
            {
                UpdateEmotion((int)Emotion.SCARED, 0.7f);
            }
            else if (playerHealthChange > 0.3f)
            {
                UpdateEmotion((int)Emotion.HAPPY, 0.6f);
            }
            else
            {
                UpdateEmotion((int)Emotion.CALM, 0.5f);
            }
        }
    }

    // 主数据类字段
    public PetAIPersonality personality;
    public PetAIBehavior behavior;
    public PetAILearning learning;
    public PetAIDecision decision;
    public PetAIEmotionalState emotion;

    // 战斗统计
    public float totalDamageDealt = 0f;
    public float totalDamagePrevented = 0f;
    public float totalHealingDone = 0f;
    public int criticalHits = 0;
    public int perfectDodges = 0;

    protected override void Initialize()
    {
        base.Initialize();
        personality = new PetAIPersonality();
        behavior = new PetAIBehavior();
        learning = new PetAILearning();
        decision = new PetAIDecision();
        emotion = new PetAIEmotionalState();
    }

    public void Reset()
    {
        totalDamageDealt = 0f;
        totalDamagePrevented = 0f;
        totalHealingDone = 0f;
        criticalHits = 0;
        perfectDodges = 0;
    }

    public int GetAiLevel()
    {
        // 基于学习数据和适应等级计算AI等级
        float level = 1.0f;
        level += learning.adaptationLevel * 9.0f;  // 1-10级
        level += learning.GetWinRate() * 5.0f;    // 额外加分
        return (int)Mathf.Clamp(level, 1f, 15f);
    }

    public override Dictionary ExportSaveData()
    {
        return new Dictionary
        {
            { "personality_type", personality.personalityType },
            { "curiosity_level", personality.curiosityLevel },
            { "energy_level", personality.energyLevel },
            { "loyalty_level", personality.loyaltyLevel },
            { "adaptation_level", learning.adaptationLevel },
            { "total_battles", learning.learningData["total_battles"] },
            { "wins", learning.learningData["wins"] },
            { "losses", learning.learningData["losses"] },
            { "best_combo", learning.learningData["best_combo"] },
            { "enemy_type_kills", learning.enemyTypeKills },
            { "current_emotion", emotion.currentEmotion },
            { "emotion_intensity", emotion.emotionIntensity },
            { "total_damage_dealt", totalDamageDealt },
            { "total_damage_prevented", totalDamagePrevented },
            { "total_healing_done", totalHealingDone },
            { "critical_hits", criticalHits },
            { "perfect_dodges", perfectDodges }
        };
    }

    public override void ImportSaveData(Dictionary data)
    {
        base.ImportSaveData(data);

        if (data.Contains("personality_type"))
        {
            personality = new PetAIPersonality((int)data["personality_type"]);
            personality.curiosityLevel = data.Contains("curiosity_level") ? (float)data["curiosity_level"] : 0.5f;
            personality.energyLevel = data.Contains("energy_level") ? (float)data["energy_level"] : 1.0f;
            personality.loyaltyLevel = data.Contains("loyalty_level") ? (float)data["loyalty_level"] : 0.5f;
        }

        if (data.Contains("adaptation_level"))
        {
            learning.adaptationLevel = (float)data["adaptation_level"];
            learning.learningData["total_battles"] = data.Contains("total_battles") ? data["total_battles"] : 0;
            learning.learningData["wins"] = data.Contains("wins") ? data["wins"] : 0;
            learning.learningData["losses"] = data.Contains("losses") ? data["losses"] : 0;
            learning.learningData["best_combo"] = data.Contains("best_combo") ? data["best_combo"] : 0;
        }

        if (data.Contains("enemy_type_kills"))
        {
            learning.enemyTypeKills = (Dictionary)data["enemy_type_kills"];
        }

        if (data.Contains("current_emotion"))
        {
            emotion.currentEmotion = (int)data["current_emotion"];
            emotion.emotionIntensity = data.Contains("emotion_intensity") ? (float)data["emotion_intensity"] : 0.5f;
        }

        totalDamageDealt = data.Contains("total_damage_dealt") ? (float)data["total_damage_dealt"] : 0f;
        totalDamagePrevented = data.Contains("total_damage_prevented") ? (float)data["total_damage_prevented"] : 0f;
        totalHealingDone = data.Contains("total_healing_done") ? (float)data["total_healing_done"] : 0f;
        criticalHits = data.Contains("critical_hits") ? (int)data["critical_hits"] : 0;
        perfectDodges = data.Contains("perfect_dodges") ? (int)data["perfect_dodges"] : 0;
    }
}
