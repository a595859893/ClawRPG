using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Pet AI Improvements System
/// Enhanced pet AI with personality, learning, and adaptive behaviors
/// </summary>
public partial class PetAIImprovementsSystem : BaseSystem
{
    // 信号定义 (C# 事件)
    public event Action<string> OnAiStateChanged;
    public event Action<string, Dictionary> OnAiDecisionMade;
    public event Action<int> OnAiLearningUpdate;
    public event Action<string, float> OnAiEmotionChanged;
    public event Action<int> OnAiLevelUp;

    // 常量
    private const string AI_STATE_CHANGED = "ai_state_changed";
    private const string AI_DECISION_MADE = "ai_decision_made";
    private const string AI_LEARNING_UPDATE = "ai_learning_update";
    private const string AI_EMOTION_CHANGED = "ai_emotion_changed";
    private const string AI_LEVEL_UP = "ai_level_up";

    // 内部类
    private class PetAIPersonality
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

    private class PetAIBehavior
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

        public string GetStateName()
        {
            return ((BehaviorState)currentState).ToString();
        }
    }

    private class PetAILearning
    {
        public Dictionary learningData = new Dictionary
        {
            { "total_battles", 0 },
            { "wins", 0 },
            { "losses", 0 },
            { "best_combo", 0 }
        };

        public Dictionary enemyTypeKills = new Dictionary<string, object>();
        public int adaptationLevel = 1;
        public int blockCount = 0;
        public int healCount = 0;
        public int dodgeCount = 0;

        public void RecordBattleResult(bool won)
        {
            int total = (int)learningData["total_battles"] + 1;
            learningData["total_battles"] = total;
            if (won)
            {
                learningData["wins"] = (int)learningData["wins"] + 1;
            }
            else
            {
                learningData["losses"] = (int)learningData["losses"] + 1;
            }
        }

        public void RecordEnemyKilled(string enemyType)
        {
            if (enemyTypeKills.ContainsKey(enemyType))
            {
                enemyTypeKills[enemyType] = (int)enemyTypeKills[enemyType] + 1;
            }
            else
            {
                enemyTypeKills[enemyType] = 1;
            }
        }

        public void RecordBlock()
        {
            blockCount++;
        }

        public void RecordHeal()
        {
            healCount++;
        }

        public void RecordDodge()
        {
            dodgeCount++;
        }

        public void RecordCombo(int combo)
        {
            learningData["best_combo"] = Mathf.Max((int)learningData["best_combo"], combo);
        }

        public float GetWinRate()
        {
            int total = (int)learningData["total_battles"];
            if (total == 0) return 0f;
            return (int)learningData["wins"] / (float)total;
        }

        public string GetMostKilledEnemy()
        {
            string maxEnemy = "";
            int maxCount = 0;
            foreach (var key in enemyTypeKills.Keys)
            {
                int count = (int)enemyTypeKills[key];
                if (count > maxCount)
                {
                    maxCount = count;
                    maxEnemy = key.ToString();
                }
            }
            return maxEnemy;
        }
    }

    private class PetAIDecision
    {
        private Random random = new Random();

        public string CalculateDecision(PetAIPersonality personality, PetAILearning learning, Dictionary situation)
        {
            float aggression = personality.aggressionModifier;
            int enemyCount = situation.ContainsKey("enemy_count") ? (int)situation["enemy_count"] : 0;
            float health = situation.ContainsKey("player_health") ? (float)situation["player_health"] : 1.0f;

            // 基于性格和情况做出决策
            if (enemyCount > 0 && health > 0.5f && aggression > 1.0f)
            {
                return "attack";
            }
            else if (health < 0.3f)
            {
                return "defend";
            }
            else if (random.NextDouble() > 0.5f)
            {
                return "support";
            }
            else
            {
                return "explore";
            }
        }
    }

    private class PetAIEmotionalState
    {
        public enum Emotion
        {
            HAPPY = 0,
            SAD = 1,
            ANGRY = 2,
            CALM = 3,
            EXCITED = 4,
            TIRED = 5
        }

        public Emotion currentEmotion = Emotion.CALM;
        public float emotionIntensity = 0.5f;
        public float moodTimer = 0f;

        public void UpdateEmotion(Emotion emotion, float intensity)
        {
            currentEmotion = emotion;
            emotionIntensity = intensity;
        }

        public void UpdateEmotionFromBattle(bool won, float damage)
        {
            if (won)
            {
                currentEmotion = Emotion.HAPPY;
                emotionIntensity = Mathf.Min(1.0f, emotionIntensity + 0.2f);
            }
            else
            {
                currentEmotion = Emotion.SAD;
                emotionIntensity = Mathf.Min(1.0f, emotionIntensity + 0.1f);
            }
        }

        public string GetEmotionName()
        {
            return currentEmotion.ToString();
        }
    }

    // 主类数据
    private PetAIPersonality personality;
    private PetAIBehavior behavior;
    private PetAILearning learning;
    private PetAIDecision decision;
    private PetAIEmotionalState emotion;

    private bool isActive = false;
    private Node currentTarget = null;
    private string ownerPetId = "";

    // 配置参数
    private float decisionInterval = 0.5f;
    private float emotionDecayRate = 0.1f;
    private int learningThreshold = 10;
    private int maxAiLevel = 15;

    // 状态追踪
    private float decisionTimer = 0f;
    private int lastAiLevel = 1;

    // 统计数据
    private float totalDamageDealt = 0f;
    private float totalDamagePrevented = 0f;
    private float totalHealingDone = 0f;
    private int criticalHits = 0;
    private int perfectDodges = 0;

    protected override void Initialize()
    {
        base.Initialize();
        personality = new PetAIPersonality();
        behavior = new PetAIBehavior();
        learning = new PetAILearning();
        decision = new PetAIDecision();
        emotion = new PetAIEmotionalState();
    }

    public void InitializePet(string petId, int personalityType = 0)
    {
        ownerPetId = petId;
        personality = new PetAIPersonality(personalityType);
        isActive = true;
        lastAiLevel = 1;
        GD.Print($"[PetAI] Initialized for pet: {petId} with personality: {personality.GetStateName()}");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (!isActive)
            return;

        // 更新决策计时器
        decisionTimer += delta;
        if (decisionTimer >= decisionInterval)
        {
            decisionTimer = 0f;
            MakeAiDecision();
        }

        // 更新情绪状态
        UpdateEmotionState(delta);
    }

    private void MakeAiDecision()
    {
        // 收集当前情况
        var situation = CollectSituation();

        // 计算最佳决策
        string decisionStr = decision.CalculateDecision(personality, learning, situation);

        // 执行决策
        ExecuteDecision(decisionStr, situation);

        // 发出信号
        OnAiDecisionMade?.Invoke(decisionStr, situation);
    }

    private Dictionary CollectSituation()
    {
        var situation = new Dictionary<string, object>();

        // 玩家血量
        var player = GetTree().GetFirstNodeInGroup("player");
        if (player != null && player.HasMethod("get_health_percent"))
        {
            situation["player_health"] = (float)player.Call("get_health_percent");
        }

        // 敌人数量
        var enemies = GetTree().GetNodesInGroup("enemy");
        situation["enemy_count"] = enemies.Count;

        // 最近敌人
        if (enemies.Count > 0)
        {
            situation["nearest_enemy_distance"] = 9999.0f;
            foreach (Node enemy in enemies)
            {
                if (enemy.HasMethod("get_global_position"))
                {
                    float dist = enemy.Call("get_global_position").As<Vector2>().DistanceTo(GlobalPosition);
                    if ((float)situation["nearest_enemy_distance"] > dist)
                    {
                        situation["nearest_enemy_distance"] = dist;
                    }
                }
            }
        }

        // 宠物血量
        situation["pet_health"] = 1.0f;

        // 能量水平
        situation["energy"] = personality.energyLevel;

        return situation;
    }

    private void ExecuteDecision(string decisionStr, Dictionary situation)
    {
        switch (decisionStr)
        {
            case "attack":
                behavior.currentState = (int)PetAIBehavior.BehaviorState.ATTACK;
                PerformAttackAction(situation);
                break;
            case "defend":
                behavior.currentState = (int)PetAIBehavior.BehaviorState.IDLE;
                PerformDefendAction(situation);
                break;
            case "support":
                behavior.currentState = (int)PetAIBehavior.BehaviorState.FOLLOW;
                PerformSupportAction(situation);
                break;
            case "retreat":
                behavior.currentState = (int)PetAIBehavior.BehaviorState.RETREAT;
                PerformRetreatAction(situation);
                break;
            case "explore":
                behavior.currentState = (int)PetAIBehavior.BehaviorState.EXPLORE;
                PerformExploreAction(situation);
                break;
        }

        OnAiStateChanged?.Invoke(behavior.GetStateName());
    }

    private void PerformAttackAction(Dictionary situation)
    {
        if (situation.ContainsKey("enemy_count") && (int)situation["enemy_count"] > 0)
        {
            learning.RecordBattleResult(true);
            personality.energyLevel = Mathf.Max(0f, personality.energyLevel - 0.05f);
        }
    }

    private void PerformDefendAction(Dictionary situation)
    {
        totalDamagePrevented += 10.0f;
        learning.RecordBlock();
        personality.energyLevel = Mathf.Min(1.0f, personality.energyLevel + 0.02f);
    }

    private void PerformSupportAction(Dictionary situation)
    {
        totalHealingDone += 5.0f;
        learning.RecordHeal();
        personality.energyLevel = Mathf.Max(0f, personality.energyLevel - 0.03f);
    }

    private void PerformRetreatAction(Dictionary situation)
    {
        learning.RecordDodge();
        personality.energyLevel = Mathf.Min(1.0f, personality.energyLevel + 0.05f);
    }

    private void PerformExploreAction(Dictionary situation)
    {
        // 探索行为逻辑 - 好奇心强的宠物
        if (personality.personalityType == (int)PetAIPersonality.PersonalityType.CURIOUS)
        {
            // 发现隐藏物品/区域
        }
    }

    private void UpdateEmotionState(float delta)
    {
        emotion.moodTimer += delta;

        // 情绪随时间衰减
        if (emotion.moodTimer > 10.0f)
        {
            emotion.UpdateEmotion(PetAIEmotionalState.Emotion.CALM, 0.5f);
        }
    }

    public void RecordBattleEvent(string enemyType, bool won, float damageDealt, 
                                   float damagePrevented, float healingDone)
    {
        learning.RecordBattleResult(won);
        learning.RecordEnemyKilled(enemyType);
        totalDamageDealt += damageDealt;
        totalDamagePrevented += damagePrevented;
        totalHealingDone += healingDone;

        // 更新情绪
        emotion.UpdateEmotionFromBattle(won, 0f);

        // 检查升级
        CheckAiLevelUp();
    }

    public void RecordCriticalHit()
    {
        learning.RecordCombo((int)learning.learningData["best_combo"] + 1);
        criticalHits++;
    }

    public void RecordPerfectDodge()
    {
        learning.RecordDodge();
        perfectDodges++;
    }

    private void CheckAiLevelUp()
    {
        int newLevel = GetAiLevel();
        if (newLevel > lastAiLevel)
        {
            lastAiLevel = newLevel;
            OnAiLevelUp?.Invoke(newLevel);
            GD.Print($"[PetAI] AI Level up! New level: {newLevel}");
        }
    }

    public void SetPersonalityType(int personalityType)
    {
        personality = new PetAIPersonality(personalityType);
    }

    public string GetAiState()
    {
        return behavior.GetStateName();
    }

    public int GetAiLevel()
    {
        int totalBattles = (int)learning.learningData["total_battles"];
        return Mathf.Min(maxAiLevel, 1 + totalBattles / learningThreshold);
    }

    public string GetCurrentEmotion()
    {
        return emotion.GetEmotionName();
    }

    public string GetPersonalityTypeStr()
    {
        switch (personality.personalityType)
        {
            case (int)PetAIPersonality.PersonalityType.AGGRESSIVE: return "Aggressive";
            case (int)PetAIPersonality.PersonalityType.DEFENSIVE: return "Defensive";
            case (int)PetAIPersonality.PersonalityType.SUPPORTIVE: return "Supportive";
            case (int)PetAIPersonality.PersonalityType.CURIOUS: return "Curious";
            case (int)PetAIPersonality.PersonalityType.LAZY: return "Lazy";
        }
        return "Unknown";
    }

    public Dictionary GetLearningStats()
    {
        return new Dictionary
        {
            { "adaptation_level", learning.adaptationLevel },
            { "win_rate", learning.GetWinRate() },
            { "total_battles", learning.learningData["total_battles"] },
            { "best_combo", learning.learningData["best_combo"] },
            { "most_killed_enemy", learning.GetMostKilledEnemy() }
        };
    }

    public Dictionary GetCombatStats()
    {
        return new Dictionary
        {
            { "total_damage_dealt", totalDamageDealt },
            { "total_damage_prevented", totalDamagePrevented },
            { "total_healing_done", totalHealingDone },
            { "critical_hits", criticalHits },
            { "perfect_dodges", perfectDodges }
        };
    }

    public override Dictionary<string, object> ExportSaveData()
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
            { "current_emotion", (int)emotion.currentEmotion },
            { "emotion_intensity", emotion.emotionIntensity },
            { "total_damage_dealt", totalDamageDealt },
            { "total_damage_prevented", totalDamagePrevented },
            { "total_healing_done", totalHealingDone },
            { "critical_hits", criticalHits },
            { "perfect_dodges", perfectDodges }
        };
    }

    public override void ImportSaveData(Dictionary<string, object> data)
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
            learning.adaptationLevel = (int)data["adaptation_level"];
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
            emotion.currentEmotion = (PetAIEmotionalState.Emotion)(int)data["current_emotion"];
            emotion.emotionIntensity = data.Contains("emotion_intensity") ? (float)data["emotion_intensity"] : 0.5f;
        }

        totalDamageDealt = data.Contains("total_damage_dealt") ? (float)data["total_damage_dealt"] : 0f;
        totalDamagePrevented = data.Contains("total_damage_prevented") ? (float)data["total_damage_prevented"] : 0f;
        totalHealingDone = data.Contains("total_healing_done") ? (float)data["total_healing_done"] : 0f;
        criticalHits = data.Contains("critical_hits") ? (int)data["critical_hits"] : 0;
        perfectDodges = data.Contains("perfect_dodges") ? (int)data["perfect_dodges"] : 0;

        lastAiLevel = GetAiLevel();
    }

    public void Deactivate()
    {
        isActive = false;
    }
}
