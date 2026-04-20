using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.RandomEvent;
using EventRarity = ClawRPG.Scripts.Systems.RandomEvent.EventRarity;

/// <summary>
/// Core random event system that manages event generation and effects
/// </summary>
public partial class RandomEventSystem : BaseSystem
{
    public static RandomEventSystem Instance { get; private set; }
    
    // Event database
    private Dictionary<string, RandomEventData> _eventDatabase = new();
    
    // Player stats
    private RandomEventStats _stats = new();
    
    // Configuration
    [Export] public float eventCheckInterval = 60f; // Check every 60 seconds
    [Export] public float eventProbability = 0.1f; // 10% chance per check
    [Export] public int maxActiveEffects = 5;
    [Export] public bool eventsEnabled = true;
    
    // Timer for event checks
    private Timer _eventTimer;
    
    // Current active event
    private RandomEventData _currentEvent;
    private DateTime _currentEventTime;
    private float _currentEventDuration;
    
    // Signal for UI updates
    public static void EventEnded(RandomEventData eventData) { }
    public static void EffectExpired(string effectType) { }
    
    public override void _Ready()
    {
        Instance = this;
        InitializeEventDatabase();
        SetupTimer();
        LoadStats();
    }
    
    private void InitializeEventDatabase()
    {
        // Positive Events
        CreateEvent("lucky_find", "Lucky Find", "You found some gold on the ground!", 
            RandomEventType.LuckyFind, EventRarity.Common, true, false,
            goldReward: 50, experienceReward: 10);
            
        CreateEvent("mysterious_blessing", "Mysterious Blessing", "A mysterious force grants you power!",
            RandomEventType.MysteriousBlessing, EventRarity.Uncommon, true, false,
            attackBonus: 0.1f, defenseBonus: 0.1f, effectDuration: 300f);
            
        CreateEvent("trader_visit", "Trader Visit", "A wandering trader appears with exotic goods!",
            RandomEventType.TraderVisit, EventRarity.Uncommon, true, false,
            goldReward: 100);
            
        CreateEvent("healing_spring", "Healing Spring", "You discovered a healing spring!",
            RandomEventType.HealingSpring, EventRarity.Uncommon, true, false,
            healthRestore: 0.3f);
            
        CreateEvent("treasure_chest", "Treasure Chest", "You found a hidden treasure chest!",
            RandomEventType.TreasureChest, EventRarity.Rare, true, false,
            goldReward: 200, experienceReward: 50);
            
        CreateEvent("friendly_encounter", "Friendly Encounter", "A friendly traveler shares their knowledge!",
            RandomEventType.FriendlyEncounter, EventRarity.Common, true, false,
            experienceReward: 30, luckBonus: 0.05f, effectDuration: 180f);
            
        CreateEvent("ancient_knowledge", "Ancient Knowledge", "You uncovered ancient secrets!",
            RandomEventType.AncientKnowledge, EventRarity.Rare, true, false,
            experienceReward: 100, attackBonus: 0.05f, defenseBonus: 0.05f, effectDuration: 600f);
            
        CreateEvent("windfall", "Windfall", "Fortune smiles upon you!",
            RandomEventType.Windfall, EventRarity.Legendary, true, false,
            goldReward: 500, experienceReward: 150, attackBonus: 0.15f, effectDuration: 600f);
            
        // Negative Events
        CreateEvent("ambush", "Ambush", "You've been ambushed!",
            RandomEventType.Ambush, EventRarity.Uncommon, false, true,
            healthPenalty: 0.2f, goldPenalty: 25);
            
        CreateEvent("trap", "Trap", "You triggered a trap!",
            RandomEventType.Trap, EventRarity.Uncommon, false, true,
            healthPenalty: 0.15f, speedPenalty: 0.1f, effectDuration: 120f);
            
        CreateEvent("curse", "Curse", "You've been cursed!",
            RandomEventType.Curse, EventRarity.Rare, false, true,
            attackPenalty: 0.15f, defensePenalty: 0.15f, effectDuration: 300f);
            
        CreateEvent("bandits", "Bandits", "Bandits attack you!",
            RandomEventType.Bandits, EventRarity.Uncommon, false, true,
            healthPenalty: 0.25f, goldPenalty: 50);
            
        CreateEvent("bad_weather", "Bad Weather", "A storm approaches...",
            RandomEventType.BadWeather, EventRarity.Common, false, true,
            speedPenalty: 0.2f, effectDuration: 180f);
            
        CreateEvent("plague", "Plague", "You feel ill...",
            RandomEventType.Plague, EventRarity.Rare, false, true,
            healthPenalty: 0.1f, attackPenalty: 0.1f, effectDuration: 600f);
            
        CreateEvent("theft", "Theft", "You've been robbed!",
            RandomEventType.Theft, EventRarity.Uncommon, false, true,
            goldPenalty: 100);
            
        CreateEvent("monster_attack", "Monster Attack", "A monster attacks!",
            RandomEventType.MonsterAttack, EventRarity.Rare, false, true,
            healthPenalty: 0.3f);
            
        // Neutral Events
        CreateEvent("traveler", "Traveler", "You met a fellow traveler.",
            RandomEventType.Traveler, EventRarity.Common, false, false,
            experienceReward: 10);
            
        CreateEvent("landmark", "Landmark", "You discovered a notable landmark.",
            RandomEventType.Landmark, EventRarity.Common, false, false,
            experienceReward: 20);
            
        CreateEvent("rest_site", "Rest Site", "You found a safe place to rest.",
            RandomEventType.RestSite, EventRarity.Uncommon, false, false,
            healthRestore: 0.15f);
            
        CreateEvent("puzzle", "Puzzle", "An ancient puzzle awaits...",
            RandomEventType.Puzzle, EventRarity.Uncommon, false, false,
            experienceReward: 40);
            
        CreateEvent("riddle", "Riddle", "A mysterious riddle appears!",
            RandomEventType.Riddle, EventRarity.Uncommon, false, false,
            experienceReward: 35, goldReward: 25);
            
        CreateEvent("omen", "Omen", "The spirits whisper...",
            RandomEventType.Omen, EventRarity.Rare, false, false,
            luckBonus: 0.1f, effectDuration: 300f);
            
        GD.Print($"[RandomEventSystem] Initialized with {_eventDatabase.Count} events");
    }
    
    private void CreateEvent(string id, string name, string description, 
        RandomEventType type, EventRarity rarity, bool isPositive, bool isNegative,
        int goldReward = 0, int experienceReward = 0, float healthRestore = 0,
        float attackBonus = 0, float defenseBonus = 0, float speedBonus = 0, float luckBonus = 0,
        int goldPenalty = 0, float healthPenalty = 0, float attackPenalty = 0, 
        float defensePenalty = 0, float speedPenalty = 0, float effectDuration = -1)
    {
        var evt = new RandomEventData
        {
            eventId = id,
            eventName = name,
            description = description,
            eventType = type,
            rarity = rarity,
            isPositive = isPositive,
            isNegative = isNegative,
            goldReward = goldReward,
            experienceReward = experienceReward,
            healthRestore = healthRestore,
            attackBonus = attackBonus,
            defenseBonus = defenseBonus,
            speedBonus = speedBonus,
            luckBonus = luckBonus,
            goldPenalty = goldPenalty,
            healthPenalty = healthPenalty,
            attackPenalty = attackPenalty,
            defenseBonus = defensePenalty,
            speedPenalty = speedPenalty,
            effectDuration = effectDuration
        };
        
        _eventDatabase[id] = evt;
    }
    
    private void SetupTimer()
    {
        _eventTimer = new Timer();
        _eventTimer.WaitTime = eventCheckInterval;
        _eventTimer.Autostart = true;
        _eventTimer.Timeout += OnEventCheckTimer;
        AddChild(_eventTimer);
    }
    
    private void OnEventCheckTimer()
    {
        if (!eventsEnabled || _currentEvent != null) return;
        
        // Random chance to trigger an event
        if (GD.Randf() < eventProbability)
        {
            TriggerRandomEvent();
        }
    }
    
    /// <summary>
    /// Manually trigger a random event
    /// </summary>
    public void TriggerRandomEvent()
    {
        if (_eventDatabase.Count == 0) return;
        
        // Select event based on rarity weights
        var selectedEvent = SelectEventByRarity();
        if (selectedEvent == null) return;
        
        ApplyEvent(selectedEvent);
    }
    
    private RandomEventData SelectEventByRarity()
    {
        // Rarity weights: Common 50%, Uncommon 30%, Rare 15%, Legendary 5%
        float roll = GD.Randf();
        EventRarity targetRarity;
        
        if (roll < 0.5f) targetRarity = EventRarity.Common;
        else if (roll < 0.8f) targetRarity = EventRarity.Uncommon;
        else if (roll < 0.95f) targetRarity = EventRarity.Rare;
        else targetRarity = EventRarity.Legendary;
        
        // Filter events by rarity
        var candidates = new List<RandomEventData>();
        foreach (var evt in _eventDatabase.Values)
        {
            if (evt.rarity == targetRarity)
            {
                candidates.Add(evt);
            }
        }
        
        if (candidates.Count == 0)
        {
            // Fallback to any event
            candidates.AddRange(_eventDatabase.Values);
        }
        
        if (candidates.Count == 0) return null;
        
        return candidates[GD.Randi() % candidates.Count];
    }
    
    private void ApplyEvent(RandomEventData evt)
    {
        _currentEvent = evt;
        _currentEventTime = DateTime.Now;
        _currentEventDuration = evt.effectDuration;
        
        // Update stats
        _stats.eventsEncountered++;
        _stats.recentEvents.Add(evt.eventId);
        if (_stats.recentEvents.Count > 10) _stats.recentEvents.RemoveAt(0);
        
        if (!_stats.eventCounts.ContainsKey(evt.eventId))
            _stats.eventCounts[evt.eventId] = 0;
        _stats.eventCounts[evt.eventId]++;
        
        if (evt.isPositive)
        {
            _stats.positiveEvents++;
            ApplyRewards(evt);
        }
        else if (evt.isNegative)
        {
            _stats.negativeEvents++;
            ApplyPenalties(evt);
        }
        else
        {
            _stats.neutralEvents++;
            ApplyRewards(evt);
        }
        
        EmitSignal(SignalName.EventTriggered, evt);
        SaveStats();
        
        GD.Print($"[RandomEventSystem] Event triggered: {evt.eventName} ({evt.rarity})");
    }
    
    private void ApplyRewards(RandomEventData evt)
    {
        // Gold reward
        if (evt.goldReward > 0)
        {
            // Add gold to player (placeholder - integrate with actual player gold system)
            _stats.totalGoldGained += evt.goldReward;
            GD.Print($"[RandomEventSystem] Received {evt.goldReward} gold!");
        }
        
        // Experience reward
        if (evt.experienceReward > 0)
        {
            _stats.totalExperienceGained += evt.experienceReward;
            GD.Print($"[RandomEventSystem] Gained {evt.experienceReward} experience!");
        }
        
        // Health restore
        if (evt.healthRestore > 0)
        {
            // Restore player health (placeholder)
            GD.Print($"[RandomEventSystem] Restored {evt.healthRestore * 100}% health!");
        }
        
        // Apply temporary bonuses
        if (evt.attackBonus > 0 || evt.defenseBonus > 0 || evt.speedBonus > 0 || evt.luckBonus > 0)
        {
            ApplyTemporaryBonus("positive", evt.effectDuration);
        }
    }
    
    private void ApplyPenalties(RandomEventData evt)
    {
        // Gold penalty
        if (evt.goldPenalty > 0)
        {
            _stats.totalGoldLost += evt.goldPenalty;
            GD.Print($"[RandomEventSystem] Lost {evt.goldPenalty} gold!");
        }
        
        // Health penalty
        if (evt.healthPenalty > 0)
        {
            GD.Print($"[RandomEventSystem] Lost {evt.healthPenalty * 100}% health!");
        }
        
        // Apply temporary debuffs
        if (evt.attackPenalty > 0 || evt.defensePenalty > 0 || evt.speedPenalty > 0)
        {
            ApplyTemporaryBonus("negative", evt.effectDuration);
        }
    }
    
    private void ApplyTemporaryBonus(string bonusType, float duration)
    {
        if (duration <= 0) return; // Permanent effect
        
        var effectKey = $"{bonusType}_{_currentEvent.eventId}";
        _stats.activeEffects[effectKey] = DateTime.Now.AddSeconds(duration);
        
        if (_stats.activeEffects.Count > maxActiveEffects)
        {
            // Remove oldest effect
            var oldest = "";
            var oldestTime = DateTime.MaxValue;
            foreach (var kvp in _stats.activeEffects)
            {
                if (kvp.Value < oldestTime)
                {
                    oldestTime = kvp.Value;
                    oldest = kvp.Key;
                }
            }
            if (oldest != "")
            {
                _stats.activeEffects.Remove(oldest);
                EmitSignal(SignalName.EffectExpired, oldest);
            }
        }
        
        EmitSignal(SignalName.EffectApplied, bonusType, duration);
    }
    
    /// <summary>
    /// Skip or dismiss the current event
    /// </summary>
    public void DismissCurrentEvent()
    {
        if (_currentEvent != null)
        {
            EmitSignal(SignalName.EventEnded, _currentEvent);
            _currentEvent = null;
            SaveStats();
        }
    }
    
    /// <summary>
    /// Force trigger a specific event by ID
    /// </summary>
    public void TriggerSpecificEvent(string eventId)
    {
        if (_eventDatabase.TryGetValue(eventId, out var evt))
        {
            ApplyEvent(evt);
        }
    }
    
    /// <summary>
    /// Get current active event
    /// </summary>
    public RandomEventData GetCurrentEvent() => _currentEvent;
    
    /// <summary>
    /// Get event statistics
    /// </summary>
    public RandomEventStats GetStats() => _stats;
    
    /// <summary>
    /// Get all available events
    /// </summary>
    public Dictionary<string, RandomEventData> GetAllEvents() => new(_eventDatabase);
    
    /// <summary>
    /// Enable or disable random events
    /// </summary>
    public void SetEventsEnabled(bool enabled)
    {
        eventsEnabled = enabled;
    }
    
    private void LoadStats()
    {
        // Placeholder for loading stats from save
    }
    
    private void SaveStats()
    {
        // Placeholder for saving stats to save
    }
    
    public override void _Process(double delta)
    {
        // Check for expired effects
        var now = DateTime.Now;
        var expiredEffects = new List<string>();
        
        foreach (var kvp in _stats.activeEffects)
        {
            if (kvp.Value < now)
            {
                expiredEffects.Add(kvp.Key);
            }
        }
        
        foreach (var effect in expiredEffects)
        {
            _stats.activeEffects.Remove(effect);
            EmitSignal(SignalName.EffectExpired, effect);
        }
        
        // Check for event duration expiration
        if (_currentEvent != null && _currentEvent.effectDuration > 0)
        {
            var elapsed = (DateTime.Now - _currentEventTime).TotalSeconds;
            if (elapsed >= _currentEventDuration)
            {
                DismissCurrentEvent();
            }
        }
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 玩家统计
        data["events_triggered"] = _stats.eventsTriggered;
        data["events_accepted"] = _stats.eventsAccepted;
        data["events_dismissed"] = _stats.eventsDismissed;
        data["total_gold_from_events"] = _stats.totalGoldFromEvents;
        data["total_exp_from_events"] = _stats.totalExpFromEvents;
        
        // 活跃效果
        var activeEffects = new Godot.Collections.Array();
        foreach (var effect in _stats.activeEffects)
        {
            activeEffects.Add(effect);
        }
        data["active_effects"] = activeEffects;
        
        // 当前事件
        if (_currentEvent != null)
        {
            data["current_event_id"] = _currentEvent.eventId;
            data["current_event_time"] = _currentEventTime.ToString("o");
        }
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 玩家统计
        _stats.eventsTriggered = (int)data.GetValueOrDefault("events_triggered", 0);
        _stats.eventsAccepted = (int)data.GetValueOrDefault("events_accepted", 0);
        _stats.eventsDismissed = (int)data.GetValueOrDefault("events_dismissed", 0);
        _stats.totalGoldFromEvents = (int)data.GetValueOrDefault("total_gold_from_events", 0);
        _stats.totalExpFromEvents = (int)data.GetValueOrDefault("total_exp_from_events", 0);
        
        // 活跃效果
        if (data.Contains("active_effects"))
        {
            var effectsArray = (Array)data["active_effects"];
            _stats.activeEffects = new List<string>();
            foreach (string effect in effectsArray)
            {
                _stats.activeEffects.Add(effect);
            }
        }
        
        // 当前事件
        if (data.Contains("current_event_id") && _eventDatabase.ContainsKey((string)data["current_event_id"]))
        {
            _currentEvent = _eventDatabase[(string)data["current_event_id"]];
            if (data.Contains("current_event_time"))
            {
                DateTime.TryParse(data["current_event_time"].ToString(), out _currentEventTime);
            }
        }
    }
}
