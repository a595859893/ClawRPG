using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetInteraction {
    /// <summary>
    /// 宠物互动类型枚举
    /// </summary>
    public enum InteractionType {
        Pet,           // 抚摸
        Play,          // 玩耍
        Talk,          // 对话
        Feed,          // 喂食
        Groom,         // 梳理
        Train,         // 训练
        Cuddle,        // 抱抱
        Heal           // 治疗
    }

    /// <summary>
    /// 互动结果枚举
    /// </summary>
    public enum InteractionResult {
        Success,
        Failed,
        Special,
        Critical
    }

    /// <summary>
    /// 宠物互动数据
    /// </summary>
    [System.Serializable]
    public class PetInteractionData {
        public Dictionary<string, PetInteractionRecord> petInteractions = new Dictionary<string, PetInteractionRecord>();
        public int totalInteractions = 0;
        public int specialInteractions = 0;
        public Dictionary<InteractionType, int> interactionTypeCount = new Dictionary<InteractionType, int>();
        public DateTime lastInteractionTime = DateTime.MinValue;
    }

    /// <summary>
    /// 单个宠物的互动记录
    /// </summary>
    [System.Serializable]
    public class PetInteractionRecord {
        public string petId;
        public string petName;
        public int totalInteractions = 0;
        public int favoriteInteraction = 0; // 最喜欢的互动类型次数
        public InteractionType favoriteType = InteractionType.Pet;
        public DateTime lastInteractionTime = DateTime.MinValue;
        public int happinessGained = 0;
        public int affectionGained = 0;
        public List<InteractionHistory> history = new List<InteractionHistory>();
    }

    /// <summary>
    /// 互动历史记录
    /// </summary>
    [System.Serializable]
    public class InteractionHistory {
        public InteractionType type;
        public InteractionResult result;
        public int happinessGained;
        public int affectionGained;
        public DateTime timestamp;
        public string soundPlayed;
    }

    /// <summary>
    /// 互动效果配置
    /// </summary>
    [System.Serializable]
    public class InteractionEffect {
        public InteractionType type;
        public string name;
        public string description;
        public int happinessGain;
        public int affectionGain;
        public float cooldown; // 秒
        public string soundEffect;
        public string particleEffect;
        public float duration; // 持续时间（秒）
    }

    /// <summary>
    /// 对话内容配置
    /// </summary>
    [System.Serializable]
    public class DialogueContent {
        public string dialogueId;
        public InteractionType triggerType;
        public List<string> responses = new List<string>();
        public int happinessBonus;
        public int affectionBonus;
    }
}
