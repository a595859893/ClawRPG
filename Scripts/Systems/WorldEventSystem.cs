using Godot;
using System;
using System.Collections.Generic;

public class WorldEventSystem : Node
{
    // Event types
    public enum EventType
    {
        Festival,           // 节日庆典
        DragonAttack,       // 巨龙袭击
        MerchantCaravan,    // 商队到来
        GoblinRaid,         // 哥布林突袭
        BountyHunt,        // 赏金猎杀
        Tournament,         // 竞技大赛
        Eclipse,            // 日食现象
        HarvestFestival,    // 丰收祭
        Blizzard,          // 暴风雪
        Plague,             // 瘟疫蔓延
        TreasureDiscovery,  // 宝藏发现
        AncientAwakening   // 远古苏醒
    }

    // Event status
    public enum EventStatus
    {
        Inactive,
        Announced,    // 即将发生
        Active,       // 进行中
        Concluding,   // 即将结束
        Completed     // 已完成
    }

    // Event data structure
    public class WorldEvent
    {
        public string id;
        public string name;
        public string description;
        public EventType type;
        public EventStatus status;
        public int duration;        // 持续时间(秒)
        public int timeRemaining;   // 剩余时间
        public int announceTime;    // 提前预告时间
        public float spawnChance;   // 触发概率
        public List<string> rewards; // 奖励列表
        public Dictionary<string, int> rewardAmounts;
        public bool playerParticipated;
        public int participantCount;
        public float completionProgress; // 0.0 - 1.0

        public WorldEvent(string eventId, string eventName, EventType eventType)
        {
            id = eventId;
            name = eventName;
            type = eventType;
            status = EventStatus.Inactive;
            rewards = new List<string>();
            rewardAmounts = new Dictionary<string, int>();
            playerParticipated = false;
            participantCount = 0;
            completionProgress = 0f;
        }
    }

    // Singleton instance
    private static WorldEventSystem _instance;
    public static WorldEventSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WorldEventSystem();
            }
            return _instance;
        }
    }

    // Event database
    private Dictionary<string, WorldEvent> _events = new Dictionary<string, WorldEvent>();
    private List<string> _activeEventIds = new List<string>();
    private List<string> _eventHistory = new List<string>();
    private Random _random = new Random();

    // Configuration
    private int _minEventInterval = 300;   // 最小事件间隔(5分钟)
    private int _maxEventInterval = 900;  // 最大事件间隔(15分钟)
    private int _nextEventTime;
    private bool _eventSystemEnabled = true;

    // Statistics
    private int _totalEventsTriggered;
    private int _totalPlayerParticipations;
    private int _totalRewardsClaimed;

    public override void _Ready()
    {
        _instance = this;
        InitializeEventDatabase();
        ScheduleNextEvent();
        GD.Print("[WorldEventSystem] World Event System initialized");
    }

    public override void _Process(float delta)
    {
        UpdateEventTimers(delta);
    }

    private void InitializeEventDatabase()
    {
        // Festival - 节日庆典
        var festival = new WorldEvent("festival", "春季庆典", EventType.Festival);
        festival.description = "村庄举办盛大的春季庆典，所有参与者都能获得经验加成！";
        festival.duration = 180;
        festival.announceTime = 60;
        festival.spawnChance = 0.15f;
        festival.rewards.Add("experience");
        festival.rewardAmounts["experience"] = 50;
        festival.rewards.Add("gold");
        festival.rewardAmounts["gold"] = 100;
        _events["festival"] = festival;

        // DragonAttack - 巨龙袭击
        var dragonAttack = new WorldEvent("dragon_attack", "巨龙来袭", EventType.DragonAttack);
        dragonAttack.description = "一条巨龙出现在村庄附近，需要勇敢的冒险者将其击退！";
        dragonAttack.duration = 300;
        dragonAttack.announceTime = 120;
        dragonAttack.spawnChance = 0.08f;
        dragonAttack.rewards.Add("gold");
        dragonAttack.rewardAmounts["gold"] = 500;
        dragonAttack.rewards.Add("dragon_scale");
        dragonAttack.rewardAmounts["dragon_scale"] = 3;
        _events["dragon_attack"] = dragonAttack;

        // MerchantCaravan - 商队到来
        var merchant = new WorldEvent("merchant_caravan", "商队抵达", EventType.MerchantCaravan);
        merchant.description = "远方商队带来了稀有货物，商店商品打折出售！";
        merchant.duration = 240;
        merchant.announceTime = 90;
        merchant.spawnChance = 0.18f;
        merchant.rewards.Add("discount_token");
        merchant.rewardAmounts["discount_token"] = 1;
        _events["merchant_caravan"] = merchant;

        // GoblinRaid - 哥布林突袭
        var goblin = new WorldEvent("goblin_raid", "哥布林突袭", EventType.GoblinRaid);
        goblin.description = "大批哥布林正在袭击农场，需要帮助击退它们！";
        goblin.duration = 180;
        goblin.announceTime = 60;
        goblin.spawnChance = 0.12f;
        goblin.rewards.Add("gold");
        goblin.rewardAmounts["gold"] = 150;
        goblin.rewards.Add("goblin_ear");
        goblin.rewardAmounts["goblin_ear"] = 5;
        _events["goblin_raid"] = goblin;

        // BountyHunt - 赏金猎杀
        var bounty = new WorldEvent("bounty_hunt", "赏金任务", EventType.BountyHunt);
        bounty.description = "一名危险的逃犯正在附近出没，赏金猎人集合！";
        bounty.duration = 360;
        bounty.announceTime = 120;
        bounty.spawnChance = 0.10f;
        bounty.rewards.Add("gold");
        bounty.rewardAmounts["gold"] = 300;
        bounty.rewards.Add("bounty_token");
        bounty.rewardAmounts["bounty_token"] = 2;
        _events["bounty_hunt"] = bounty;

        // Tournament - 竞技大赛
        var tournament = new WorldEvent("tournament", "竞技大赛", EventType.Tournament);
        tournament.description = "一年一度的竞技大赛开始了，展示你实力的时候到了！";
        tournament.duration = 420;
        tournament.announceTime = 180;
        tournament.spawnChance = 0.07f;
        tournament.rewards.Add("gold");
        tournament.rewardAmounts["gold"] = 400;
        tournament.rewards.Add("trophy");
        tournament.rewardAmounts["trophy"] = 1;
        _events["tournament"] = tournament;

        // Eclipse - 日食现象
        var eclipse = new WorldEvent("eclipse", "日食奇观", EventType.Eclipse);
        eclipse.description = "天空出现了罕见的日食现象，传说此时蕴含着神秘力量...";
        eclipse.duration = 150;
        eclipse.announceTime = 60;
        eclipse.spawnChance = 0.05f;
        eclipse.rewards.Add("mystic_essence");
        eclipse.rewardAmounts["mystic_essence"] = 5;
        _events["eclipse"] = eclipse;

        // HarvestFestival - 丰收祭
        var harvest = new WorldEvent("harvest_festival", "丰收祭", EventType.HarvestFestival);
        harvest.description = "秋天到了，村民们庆祝丰收，分享美食和喜悦！";
        harvest.duration = 200;
        harvest.announceTime = 90;
        harvest.spawnChance = 0.12f;
        harvest.rewards.Add("experience");
        harvest.rewardAmounts["experience"] = 75;
        harvest.rewards.Add("food_pack");
        harvest.rewardAmounts["food_pack"] = 3;
        _events["harvest_festival"] = harvest;

        // Blizzard - 暴风雪
        var blizzard = new WorldEvent("blizzard", "暴风雪", EventType.Blizzard);
        blizzard.description = "一场猛烈的暴风雪来临，村庄需要帮助清理积雪！";
        blizzard.duration = 240;
        blizzard.announceTime = 120;
        blizzard.spawnChance = 0.08f;
        blizzard.rewards.Add("gold");
        blizzard.rewardAmounts["gold"] = 100;
        blizzard.rewards.Add("ice_crystal");
        blizzard.rewardAmounts["ice_crystal"] = 2;
        _events["blizzard"] = blizzard;

        // Plague - 瘟疫蔓延
        var plague = new WorldEvent("plague", "瘟疫蔓延", EventType.Plague);
        plague.description = "一种奇怪的疾病在村庄蔓延，需要收集药材制作解药！";
        plague.duration = 300;
        plague.announceTime = 150;
        plague.spawnChance = 0.06f;
        plague.rewards.Add("gold");
        plague.rewardAmounts["gold"] = 200;
        plague.rewards.Add("herbal_medicine");
        plague.rewardAmounts["herbal_medicine"] = 5;
        _events["plague"] = plague;

        // TreasureDiscovery - 宝藏发现
        var treasure = new WorldEvent("treasure_discovery", "宝藏发现", EventType.TreasureDiscovery);
        treasure.description = "探险者在附近发现了古代宝藏的线索！";
        treasure.duration = 180;
        treasure.announceTime = 90;
        treasure.spawnChance = 0.10f;
        treasure.rewards.Add("gold");
        treasure.rewardAmounts["gold"] = 250;
        treasure.rewards.Add("ancient_coin");
        treasure.rewardAmounts["ancient_coin"] = 3;
        _events["treasure_discovery"] = treasure;

        // AncientAwakening - 远古苏醒
        var ancient = new WorldEvent("ancient_awakening", "远古苏醒", EventType.AncientAwakening);
        ancient.description = "沉睡已久的远古巨兽即将苏醒，世界需要英雄！";
        ancient.duration = 480;
        ancient.announceTime = 240;
        ancient.spawnChance = 0.04f;
        ancient.rewards.Add("gold");
        ancient.rewardAmounts["gold"] = 1000;
        ancient.rewards.Add("ancient_relic");
        ancient.rewardAmounts["ancient_relic"] = 1;
        _events["ancient_awakening"] = ancient;
    }

    private void ScheduleNextEvent()
    {
        int interval = _random.Next(_minEventInterval, _maxEventInterval + 1);
        _nextEventTime = OS.GetSystemTimeMsecs() / 1000 + interval;
    }

    private void UpdateEventTimers(float delta)
    {
        long currentTime = OS.GetSystemTimeMsecs() / 1000;

        // Check if it's time to trigger a new event
        if (currentTime >= _nextEventTime && _eventSystemEnabled)
        {
            TryTriggerRandomEvent();
            ScheduleNextEvent();
        }

        // Update active event timers
        for (int i = _activeEventIds.Count - 1; i >= 0; i--)
        {
            string eventId = _activeEventIds[i];
            if (_events.ContainsKey(eventId))
            {
                WorldEvent evt = _events[eventId];
                if (evt.status == EventStatus.Active || evt.status == EventStatus.Announced)
                {
                    evt.timeRemaining -= (int)(delta);

                    if (evt.status == EventStatus.Announced && evt.timeRemaining <= 0)
                    {
                        evt.status = EventStatus.Active;
                        evt.timeRemaining = evt.duration;
                        EmitSignal(nameof(EventStarted), eventId);
                    }
                    else if (evt.status == EventStatus.Active && evt.timeRemaining <= 0)
                    {
                        CompleteEvent(eventId);
                    }
                }
            }
        }
    }

    private void TryTriggerRandomEvent()
    {
        // Filter events that can spawn
        List<string> availableEvents = new List<string>();
        foreach (var kvp in _events)
        {
            if (kvp.Value.status == EventStatus.Inactive)
            {
                if (_random.NextDouble() < kvp.Value.spawnChance)
                {
                    availableEvents.Add(kvp.Key);
                }
            }
        }

        // Pick one random event from available
        if (availableEvents.Count > 0)
        {
            string selectedEvent = availableEvents[_random.Next(availableEvents.Count)];
            StartEvent(selectedEvent);
        }
    }

    public void StartEvent(string eventId)
    {
        if (!_events.ContainsKey(eventId)) return;

        WorldEvent evt = _events[eventId];
        evt.status = EventStatus.Announced;
        evt.timeRemaining = evt.announceTime;
        evt.playerParticipated = false;
        evt.participantCount = 0;
        evt.completionProgress = 0f;

        if (!_activeEventIds.Contains(eventId))
        {
            _activeEventIds.Add(eventId);
        }

        _totalEventsTriggered++;
        EmitSignal(nameof(EventAnnounced), eventId, evt.name, evt.description);
        GD.Print($"[WorldEventSystem] Event announced: {evt.name}");
    }

    public void CompleteEvent(string eventId)
    {
        if (!_events.ContainsKey(eventId)) return;

        WorldEvent evt = _events[eventId];
        evt.status = EventStatus.Completed;

        if (_activeEventIds.Contains(eventId))
        {
            _activeEventIds.Remove(eventId);
        }

        _eventHistory.Add(eventId);
        if (_eventHistory.Count > 50)
        {
            _eventHistory.RemoveAt(0);
        }

        EmitSignal(nameof(EventCompleted), eventId, evt.name);
        GD.Print($"[WorldEventSystem] Event completed: {evt.name}");

        // Reset event after some time
        evt.status = EventStatus.Inactive;
    }

    public bool ParticipateInEvent(string eventId)
    {
        if (!_events.ContainsKey(eventId)) return false;

        WorldEvent evt = _events[eventId];
        if (evt.status != EventStatus.Active && evt.status != EventStatus.Announced) return false;
        if (evt.playerParticipated) return false;

        evt.playerParticipated = true;
        evt.participantCount++;
        _totalPlayerParticipations++;

        // Award rewards
        foreach (string reward in evt.rewards)
        {
            int amount = evt.rewardAmounts.ContainsKey(reward) ? evt.rewardAmounts[reward] : 1;
            AwardReward(reward, amount);
        }

        _totalRewardsClaimed += evt.rewards.Count;
        EmitSignal(nameof(PlayerParticipated), eventId, evt.name);
        GD.Print($"[WorldEventSystem] Player participated in event: {evt.name}");

        return true;
    }

    private void AwardReward(string rewardType, int amount)
    {
        // This would integrate with the game's reward system
        switch (rewardType)
        {
            case "gold":
                // Award gold - would integrate with game economy
                GD.Print($"[WorldEventSystem] Awarded {amount} gold");
                break;
            case "experience":
                GD.Print($"[WorldEventSystem] Awarded {amount} experience");
                break;
            case "dragon_scale":
            case "goblin_ear":
            case "bounty_token":
            case "trophy":
            case "discount_token":
            case "mystic_essence":
            case "food_pack":
            case "ice_crystal":
            case "herbal_medicine":
            case "ancient_coin":
            case "ancient_relic":
                GD.Print($"[WorldEventSystem] Awarded {amount} {rewardType}");
                break;
        }
    }

    // Public API
    public Dictionary<string, WorldEvent> GetActiveEvents()
    {
        Dictionary<string, WorldEvent> activeEvents = new Dictionary<string, WorldEvent>();
        foreach (string eventId in _activeEventIds)
        {
            if (_events.ContainsKey(eventId))
            {
                activeEvents[eventId] = _events[eventId];
            }
        }
        return activeEvents;
    }

    public WorldEvent GetEvent(string eventId)
    {
        return _events.ContainsKey(eventId) ? _events[eventId] : null;
    }

    public List<string> GetEventHistory()
    {
        return new List<string>(_eventHistory);
    }

    public void SetEventSystemEnabled(bool enabled)
    {
        _eventSystemEnabled = enabled;
    }

    public int GetTotalEventsTriggered() => _totalEventsTriggered;
    public int GetTotalParticipations() => _totalPlayerParticipations;
    public int GetTotalRewardsClaimed() => _totalRewardsClaimed;

    // Signals
    [Signal]
    public delegate void EventAnnounced(string eventId, string eventName, string description);

    [Signal]
    public delegate void EventStarted(string eventId);

    [Signal]
    public delegate void EventCompleted(string eventId, string eventName);

    [Signal]
    public delegate void PlayerParticipated(string eventId, string eventName);
}
