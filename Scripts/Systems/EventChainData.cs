using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Event Chain System - 连接随机事件形成故事序列
    /// 应用 PCG 学习成果
    /// </summary>
    public partial class EventChainData : Resource {
        [Export] public string chainId = "";
        [Export] public string chainName = "";
        [Export] public string description = "";
        [Export] public int minChainLength = 2;
        [Export] public int maxChainLength = 5;
        [Export] public float triggerProbability = 0.3f;
        public List<String> requiredEvents = new List<String>();
        public List<String> followUpEvents = new List<String>();
        public EventChainReward reward = new EventChainReward();
        [Export] public EventChainCategory category = EventChainCategory.Adventure;
    }

    public class EventChainReward {
        [Export] public int goldBonus = 0;
        [Export] public int expBonus = 0;
        [Export] public float dropRateBonus = 1.0f;
        public List<String> bonusItems = new List<String>();
    }

    public enum EventChainCategory {
        Adventure,
        Combat,
        Mystery,
        Romance,
        Tragedy,
        Comedy,
        Legend
    }

    public class ActiveEventChain {
        public string chainId;
        public int currentStage;
        public int totalStages;
        public float progress;
        public bool isCompleted;
        public bool isFailed;
        public double startTime;
    }
}
