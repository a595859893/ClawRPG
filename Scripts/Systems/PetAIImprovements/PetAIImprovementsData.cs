using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Pet AI Personality
/// Defines the personality traits and modifiers for AI behavior
/// </summary>
public class PetAIPersonality
{
    public enum PersonalityType
    {
        Aggressive = 0,  // 主动攻击型
        Defensive = 1,   // 防御保护型
        Supportive = 2,   // 辅助支援型
        Curious = 3,      // 探索好奇型
        Lazy = 4          // 懒散休息型
    }

    public PersonalityType PersonalityTypeField { get; set; } = PersonalityType.Aggressive;
    public float CuriosityLevel { get; set; } = 0.5f;  // 0.0 - 1.0
    public float EnergyLevel { get; set; } = 1.0f;     // 0.0 - 1.0
    public float LoyaltyLevel { get; set; } = 0.5f;    // 0.0 - 1.0
    public float AggressionModifier { get; set; } = 1.0f;
    public float DefenseModifier { get; set; } = 1.0f;

    public PetAIPersonality(int type = (int)PersonalityType.Aggressive)
    {
        PersonalityTypeField = (PersonalityType)type;
        switch (PersonalityTypeField)
        {
            case PersonalityType.Aggressive:
                AggressionModifier = 1.5f;
                DefenseModifier = 0.8f;
                break;
            case PersonalityType.Defensive:
                AggressionModifier = 0.7f;
                DefenseModifier = 1.5f;
                break;
            case PersonalityType.Supportive:
                AggressionModifier = 0.8f;
                DefenseModifier = 1.0f;
                break;
            case PersonalityType.Curious:
                CuriosityLevel = 0.9f;
                AggressionModifier = 1.0f;
                break;
            case PersonalityType.Lazy:
                EnergyLevel = 0.3f;
                AggressionModifier = 0.5f;
                break;
        }
    }

    public string GetStateName()
    {
        return PersonalityTypeField.ToString();
    }
}

/// <summary>
/// Pet AI Behavior
/// Defines the behavior states and targets for AI
/// </summary>
public class PetAIBehavior
{
    public enum BehaviorState
    {
        Idle = 0,
        Patrol = 1,
        Chase = 2,
        Attack = 3,
        Retreat = 4,
        Follow = 5,
        Explore = 6,
        Heal = 7
    }

    public BehaviorState CurrentState { get; set; } = BehaviorState.Idle;
    public Vector3 TargetPosition { get; set; } = Vector3.Zero;
    public Node TargetEntity { get; set; } = null;
    public float StateTimer { get; set; } = 0.0f;
    public int BehaviorPriority { get; set; } = 0;

    public string GetStateName()
    {
        switch (CurrentState)
        {
            case BehaviorState.Idle: return "Idle";
            case BehaviorState.Patrol: return "Patrol";
            case BehaviorState.Chase: return "Chase";
            case BehaviorState.Attack: return "Attack";
            case BehaviorState.Retreat: return "Retreat";
            case BehaviorState.Follow: return "Follow";
            case BehaviorState.Explore: return "Explore";
            case BehaviorState.Heal: return "Heal";
        }
        return "Unknown";
    }
}

/// <summary>
/// Pet AI Learning
/// Tracks learning data and adapts AI behavior based on experience
/// </summary>
public class PetAILearning
{
    public Godot.Collections.Dictionary LearningData { get; set; }
    public Godot.Collections.Dictionary EnemyTypeKills { get; set; }  // EnemyType -> kill_count
    public List<object> PlayerActionMimic { get; set; }  // Record player actions for learning
    public float AdaptationLevel { get; set; } = 0.0f;  // 0.0 - 1.0
    public List<object> PreferredTactics { get; set; }  // Preferred tactics

    public PetAILearning()
    {
        EnemyTypeKills = new Godot.Collections.Dictionary();
        PlayerActionMimic = new List<object>();
        PreferredTactics = new List<object>();
        
        LearningData = new Godot.Collections.Dictionary
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
            { "preferred_enemy_types", new List<object>() },
            { "weak_against", new List<object>() }
        };
    }

    public void RecordBattleResult(bool win)
    {
        LearningData["total_battles"] = (int)LearningData["total_battles"] + 1;
        if (win)
        {
            LearningData["wins"] = (int)LearningData["wins"] + 1;
        }
        else
        {
            LearningData["losses"] = (int)LearningData["losses"] + 1;
        }
        UpdateAdaptationLevel();
    }

    public void RecordEnemyKilled(string enemyType)
    {
        if (!EnemyTypeKills.Contains(enemyType))
        {
            EnemyTypeKills[enemyType] = 0;
        }
        EnemyTypeKills[enemyType] = (int)EnemyTypeKills[enemyType] + 1;
    }

    public void RecordDodge()
    {
        LearningData["dodge_count"] = (int)LearningData["dodge_count"] + 1;
    }

    public void RecordBlock()
    {
        LearningData["block_count"] = (int)LearningData["block_count"] + 1;
    }

    public void RecordHeal()
    {
        LearningData["heal_count"] = (int)LearningData["heal_count"] + 1;
    }

    public void RecordCombo(int comboSize)
    {
        LearningData["combo_count"] = (int)LearningData["combo_count"] + 1;
        if (comboSize > (int)LearningData["best_combo"])
        {
            LearningData["best_combo"] = comboSize;
        }
    }

    public void UpdateAdaptationLevel()
    {
        float total = (int)LearningData["total_battles"];
        if (total > 0)
        {
            AdaptationLevel = Math.Min(1.0f, total / 100.0f);
        }
    }

    public float GetWinRate()
    {
        int total = (int)LearningData["total_battles"];
        if (total == 0)
            return 0.0f;
        return (int)LearningData["wins"] / (float)total;
    }

    public string GetMostKilledEnemy()
    {
        int maxKills = 0;
        string result = "";
        foreach (string enemyType in EnemyTypeKills.Keys)
        {
            if ((int)EnemyTypeKills[enemyType] > maxKills)
            {
                maxKills = (int)EnemyTypeKills[enemyType];
                result = enemyType;
            }
        }
        return result;
    }
}

/// <summary>
/// Pet AI Decision
/// Calculates and makes decisions based on personality, learning, and situation
/// </summary>
public class PetAIDecision
{
    public Godot.Collections.Dictionary DecisionWeights { get; set; }
    public float PersonalityInfluence { get; set; } = 0.5f;
    public float LearningInfluence { get; set; } = 0.3f;
    public float SituationInfluence { get; set; } = 0.2f;

    public PetAIDecision()
    {
        DecisionWeights = new Godot.Collections.Dictionary
        {
            { "attack", 1.0 },
            { "defend", 1.0 },
            { "support", 1.0 },
            { "retreat", 1.0 },
            { "explore", 1.0 }
        };
    }

    public string CalculateDecision(PetAIPersonality personality, PetAILearning learning,
                                    Godot.Collections.Dictionary situation)
    {
        var weights = new Godot.Collections.Dictionary(DecisionWeights);

        // Personality influence
        switch (personality.PersonalityTypeField)
        {
            case PetAIPersonality.PersonalityType.Aggressive:
                weights["attack"] = (float)weights["attack"] * personality.AggressionModifier * PersonalityInfluence * 2.0;
                weights["retreat"] = (float)weights["retreat"] * 0.5;
                break;
            case PetAIPersonality.PersonalityType.Defensive:
                weights["defend"] = (float)weights["defend"] * personality.DefenseModifier * PersonalityInfluence * 2.0;
                weights["attack"] = (float)weights["attack"] * 0.7;
                break;
            case PetAIPersonality.PersonalityType.Supportive:
                weights["support"] = (float)weights["support"] * PersonalityInfluence * 2.0;
                weights["attack"] = (float)weights["attack"] * 0.8;
                break;
            case PetAIPersonality.PersonalityType.Curious:
                weights["explore"] = (float)weights["explore"] * personality.CuriosityLevel * PersonalityInfluence * 2.0;
                break;
        }

        // Learning influence
        float winRate = learning.GetWinRate();
        if (winRate > 0.7f)
        {
            weights["attack"] = (float)weights["attack"] * (1.0 + LearningInfluence);
        }
        else if (winRate < 0.4f)
        {
            weights["defend"] = (float)weights["defend"] * (1.0 + LearningInfluence);
            weights["retreat"] = (float)weights["retreat"] * (1.0 + LearningInfluence * 0.5);
        }

        // Situation influence
        if (situation.ContainsKey("player_health"))
        {
            float playerHealth = (float)situation["player_health"];
            if (playerHealth < 0.3f)
            {
                weights["support"] = (float)weights["support"] * SituationInfluence * 3.0;
                weights["defend"] = (float)weights["defend"] * SituationInfluence * 2.0;
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

        // Choose highest weight
        string bestDecision = "attack";
        float bestWeight = 0.0f;
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

/// <summary>
/// Pet AI Emotional State
/// Tracks and manages the emotional state of the AI
/// </summary>
public class PetAIEmotionalState
{
    public enum Emotion
    {
        Happy = 0,
        Sad = 1,
        Angry = 2,
        Excited = 3,
        Scared = 4,
        Calm = 5
    }

    public int CurrentEmotion { get; set; } = (int)Emotion.Happy;
    public float EmotionIntensity { get; set; } = 0.5f;  // 0.0 - 1.0
    public float MoodTimer { get; set; } = 0.0f;
    public List<Godot.Collections.Dictionary> EmotionHistory { get; set; }

    public PetAIEmotionalState()
    {
        EmotionHistory = new List<Godot.Collections.Dictionary>();
    }

    public void UpdateEmotion(int newEmotion, float intensity = 0.5f)
    {
        CurrentEmotion = newEmotion;
        EmotionIntensity = Mathf.Clamp(intensity, 0.0f, 1.0f);
        MoodTimer = 0.0f;

        EmotionHistory.Add(new Godot.Collections.Dictionary
        {
            { "emotion", newEmotion },
            { "intensity", intensity },
            { "time", Time.GetUnixTimeFromSystem() }
        });

        // Keep history to last 20 entries
        if (EmotionHistory.Count > 20)
        {
            EmotionHistory.RemoveAt(0);
        }
    }

    public string GetEmotionName()
    {
        switch (CurrentEmotion)
        {
            case (int)Emotion.Happy: return "Happy";
            case (int)Emotion.Sad: return "Sad";
            case (int)Emotion.Angry: return "Angry";
            case (int)Emotion.Excited: return "Excited";
            case (int)Emotion.Scared: return "Scared";
            case (int)Emotion.Calm: return "Calm";
        }
        return "Unknown";
    }

    public void UpdateEmotionFromBattle(bool win, float playerHealthChange)
    {
        if (win)
        {
            UpdateEmotion((int)Emotion.Excited, 0.8f);
        }
        else if (playerHealthChange < -0.3f)
        {
            UpdateEmotion((int)Emotion.Scared, 0.7f);
        }
        else if (playerHealthChange > 0.3f)
        {
            UpdateEmotion((int)Emotion.Happy, 0.6f);
        }
        else
        {
            UpdateEmotion((int)Emotion.Calm, 0.5f);
        }
    }
}

/// <summary>
/// Pet AI Improvements Data
/// Container for all AI-related data
/// </summary>
public class PetAIImprovementsData
{
    public PetAIPersonality Personality { get; set; }
    public PetAIBehavior Behavior { get; set; }
    public PetAILearning Learning { get; set; }
    public PetAIDecision Decision { get; set; }
    public PetAIEmotionalState Emotion { get; set; }

    // Combat statistics
    public float TotalDamageDealt { get; set; } = 0.0f;
    public float TotalDamagePrevented { get; set; } = 0.0f;
    public float TotalHealingDone { get; set; } = 0.0f;
    public int CriticalHits { get; set; } = 0;
    public int PerfectDodges { get; set; } = 0;

    public PetAIImprovementsData()
    {
        Personality = new PetAIPersonality();
        Behavior = new PetAIBehavior();
        Learning = new PetAILearning();
        Decision = new PetAIDecision();
        Emotion = new PetAIEmotionalState();
    }

    public void Reset()
    {
        TotalDamageDealt = 0.0f;
        TotalDamagePrevented = 0.0f;
        TotalHealingDone = 0.0f;
        CriticalHits = 0;
        PerfectDodges = 0;
    }

    public int GetAiLevel()
    {
        // Calculate AI level based on learning data and adaptation level
        float level = 1.0f;
        level += Learning.AdaptationLevel * 9.0f;  // Level 1-10
        level += Learning.GetWinRate() * 5.0f;     // Extra points
        return (int)Mathf.Clamp(level, 1, 15);
    }
}
