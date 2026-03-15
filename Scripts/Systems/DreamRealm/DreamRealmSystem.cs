using Godot;
using System;
using System.Collections.Generic;

public partial class DreamRealmSystem : BaseSystem
{
    public static DreamRealmSystem Instance { get; private set; }

    // 梦境领域状态
    public enum DreamState
    {
        Inactive,
        Entering,
        Active,
        Exiting
    }

    // 梦境区域类型
    public enum RealmType
    {
        NightmareForest,    // 噩梦森林
        AstralPlane,        // 星界位面
        VoidRealm,          // 虚空领域
        MemoryPalace,       // 记忆宫殿
        DreamOcean,         // 梦境海洋
        ChaosDimension,     // 混沌维度
        ShadowRealm,        // 暗影领域
        CelestialGarden    //  Celestial花园
    }

    // 敌人类型
    public enum DreamEnemyType
    {
        ShadowWraith,       // 暗影幽灵
        NightmareBeast,     // 噩梦野兽
        AstralSpirit,       // 星界精灵
        VoidWalker,         // 虚空行者
        MemoryEcho,         // 记忆回声
        ChaosSpawn,         // 混沌产物
        DreamEater,         // 梦境吞噬者
        CelestialGuard      // 天界守卫
    }

    [Export]
    private DreamState _currentState = DreamState.Inactive;

    [Export]
    private RealmType _currentRealm = RealmType.NightmareForest;

    [Export]
    private int _dreamLevel = 1;

    [Export]
    private int _maxDreamLevel = 100;

    [Export]
    private float _timeInDream = 0f;

    [Export]
    private int _enemiesDefeated = 0;

    [Export]
    private int _treasuresFound = 0;

    [Export]
    private float _dreamPowerMultiplier = 1.0f;

    // 区域解锁状态
    private Dictionary<RealmType, bool> _unlockedRealms = new Dictionary<RealmType, bool>
    {
        { RealmType.NightmareForest, true },
        { RealmType.AstralPlane, false },
        { RealmType.VoidRealm, false },
        { RealmType.MemoryPalace, false },
        { RealmType.DreamOcean, false },
        { RealmType.ChaosDimension, false },
        { RealmType.ShadowRealm, false },
        { RealmType.CelestialGarden, false }
    };

    // 区域统计数据
    private Dictionary<RealmType, RealmStats> _realmStats = new Dictionary<RealmType, RealmStats>();

    // 梦境之力加成
    private Dictionary<string, float> _dreamBuffs = new Dictionary<string, float>
    {
        { "attack", 1.0f },
        { "defense", 1.0f },
        { "speed", 1.0f },
        { "luck", 1.0f },
        { "experience", 1.0f }
    };

    // 信号
    [Signal]
    public void DreamEntered(RealmType realm);

    [Signal]
    public void DreamExited();

    [Signal]
    public void RealmUnlocked(RealmType realm);

    [Signal]
    public void LevelUp(int newLevel);

    [Signal]
    public void TreasureFound(DreamTreasure treasure);

    public class RealmStats
    {
        public int timesVisited;
        public int enemiesDefeated;
        public int treasuresFound;
        public float timeSpent;
        public int highestLevel;
    }

    public class DreamTreasure
    {
        public string name;
        public string description;
        public int goldReward;
        public float expReward;
        public string itemId;
        public Rarity rarity;
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public override void _Ready()
    {
        Instance = this;
        InitializeRealms();
    }

    private void InitializeRealms()
    {
        foreach (RealmType realm in Enum.GetValues(typeof(RealmType)))
        {
            _realmStats[realm] = new RealmStats();
        }
    }

    public override void _Process(double delta)
    {
        if (_currentState == DreamState.Active)
        {
            _timeInDream += (float)delta;
            UpdateDreamBuffs();
        }
    }

    // 进入梦境
    public void EnterDream(RealmType realm = RealmType.NightmareForest)
    {
        if (_unlockedRealms[realm])
        {
            _currentState = DreamState.Entering;
            _currentRealm = realm;
            _dreamLevel = Math.Min(_dreamLevel, _maxDreamLevel);

            // 记录访问
            if (_realmStats.ContainsKey(realm))
            {
                _realmStats[realm].timesVisited++;
            }

            // 延迟进入
            GetTree().CreateTimer(1.0f).Timeout += () =>
            {
                _currentState = DreamState.Active;
                EmitSignal(SignalName.DreamEntered, (int)realm);
            };
        }
    }

    // 退出梦境
    public void ExitDream()
    {
        if (_currentState == DreamState.Active || _currentState == DreamState.Entering)
        {
            _currentState = DreamState.Exiting;

            // 记录统计
            if (_realmStats.ContainsKey(_currentRealm))
            {
                _realmStats[_currentRealm].timeSpent += _timeInDream;
                _realmStats[_currentRealm].enemiesDefeated += _enemiesDefeated;
                _realmStats[_currentRealm].treasuresFound += _treasuresFound;
            }

            GetTree().CreateTimer(0.5f).Timeout += () =>
            {
                _currentState = DreamState.Inactive;
                ResetDreamState();
                EmitSignal(SignalName.DreamExited);
            };
        }
    }

    private void ResetDreamState()
    {
        _enemiesDefeated = 0;
        _treasuresFound = 0;
        _timeInDream = 0f;
        _dreamPowerMultiplier = 1.0f;
        ResetDreamBuffs();
    }

    // 更新梦境加成
    private void UpdateDreamBuffs()
    {
        float levelBonus = 1.0f + (_dreamLevel * 0.05f);
        float timeBonus = Math.Min(_timeInDream / 300f, 0.5f); // 最多50%加成
        float enemyBonus = 1.0f + (_enemiesDefeated * 0.02f);

        _dreamPowerMultiplier = levelBonus + timeBonus + enemyBonus - 2.0f;

        _dreamBuffs["attack"] = _dreamPowerMultiplier;
        _dreamBuffs["defense"] = _dreamPowerMultiplier * 0.8f;
        _dreamBuffs["speed"] = _dreamPowerMultiplier * 0.9f;
        _dreamBuffs["luck"] = _dreamPowerMultiplier * 1.2f;
        _dreamBuffs["experience"] = _dreamPowerMultiplier * 1.5f;
    }

    private void ResetDreamBuffs()
    {
        _dreamBuffs["attack"] = 1.0f;
        _dreamBuffs["defense"] = 1.0f;
        _dreamBuffs["speed"] = 1.0f;
        _dreamBuffs["luck"] = 1.0f;
        _dreamBuffs["experience"] = 1.0f;
    }

    // 击败敌人
    public void OnEnemyDefeated()
    {
        _enemiesDefeated++;

        // 检查升级
        int expGained = (int)(100 * _dreamBuffs["experience"]);
        CheckLevelUp(expGained);
    }

    // 检查升级
    private void CheckLevelUp(int exp)
    {
        int expNeeded = _dreamLevel * 500;
        if (exp >= expNeeded && _dreamLevel < _maxDreamLevel)
        {
            _dreamLevel++;
            EmitSignal(SignalName.LevelUp, _dreamLevel);
        }
    }

    // 发现宝藏
    public DreamTreasure GenerateTreasure()
    {
        Random rng = new Random();
        float roll = (float)rng.NextDouble();

        Rarity rarity;
        if (roll < 0.5f) rarity = Rarity.Common;
        else if (roll < 0.75f) rarity = Rarity.Uncommon;
        else if (roll < 0.9f) rarity = Rarity.Rare;
        else if (roll < 0.97f) rarity = Rarity.Epic;
        else rarity = Rarity.Legendary;

        var treasure = CreateTreasure(rarity);
        _treasuresFound++;

        EmitSignal(SignalName.TreasureFound, treasure);
        return treasure;
    }

    private DreamTreasure CreateTreasure(Rarity rarity)
    {
        var treasure = new DreamTreasure
        {
            rarity = rarity,
            goldReward = GetGoldReward(rarity),
            expReward = GetExpReward(rarity),
            name = GetTreasureName(rarity),
            description = GetTreasureDescription(rarity),
            itemId = GetTreasureItem(rarity)
        };
        return treasure;
    }

    private string GetTreasureName(Rarity rarity)
    {
        string[] names = GetTreasureNames(rarity);
        return names[new Random().Next(names.Length)];
    }

    private string[] GetTreasureNames(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => new[] { "Dream Shard", "Mystic Essence", "Shadow Dust" },
            Rarity.Uncommon => new[] { "Starlight Crystal", "Nightmare Fragment", "Astral Gem" },
            Rarity.Rare => new[] { "Void Crystal", "Memory Stone", "Chaos Ember" },
            Rarity.Epic => new[] { "Dream Crown", "Astral Crown", "Eternal Flame" },
            Rarity.Legendary => new[] { "Cosmic Egg", "Dreamweaver's Staff", "Chronos Pendant" },
            _ => new[] { "Unknown Artifact" }
        };
    }

    private string GetTreasureDescription(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => "A common dream essence found in the realm.",
            Rarity.Uncommon => "An uncommon artifact imbued with dream energy.",
            Rarity.Rare => "A rare relic with powerful dream-weaving properties.",
            Rarity.Epic => "An epic artifact of immense dream power.",
            Rarity.Legendary => "A legendary artifact that exists beyond mortal dreams.",
            _ => "An artifact of unknown origin."
        };
    }

    private int GetGoldReward(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 50,
            Rarity.Uncommon => 150,
            Rarity.Rare => 400,
            Rarity.Epic => 1000,
            Rarity.Legendary => 3000,
            _ => 0
        };
    }

    private float GetExpReward(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 100,
            Rarity.Uncommon => 250,
            Rarity.Rare => 500,
            Rarity.Epic => 1000,
            Rarity.Legendary => 2500,
            _ => 0
        };
    }

    private string GetTreasureItem(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => "dream_shard",
            Rarity.Uncommon => "starlight_crystal",
            Rarity.Rare => "void_crystal",
            Rarity.Epic => "dream_crown",
            Rarity.Legendary => "cosmic_egg",
            _ => ""
        };
    }

    // 解锁区域
    public void UnlockRealm(RealmType realm)
    {
        if (!_unlockedRealms[realm])
        {
            _unlockedRealms[realm] = true;
            EmitSignal(SignalName.RealmUnlocked, (int)realm);
        }
    }

    // 生成随机敌人
    public DreamEnemyType GenerateRandomEnemy()
    {
        Array enemies = Enum.GetValues(typeof(DreamEnemyType));
        return (DreamEnemyType)enemies.GetValue(new Random().Next(enemies.Length));
    }

    // 获取区域信息
    public Dictionary GetRealmInfo(RealmType realm)
    {
        return new Dictionary
        {
            { "name", GetRealmName(realm) },
            { "description", GetRealmDescription(realm) },
            { "unlocked", _unlockedRealms[realm] },
            { "difficulty", GetRealmDifficulty(realm) },
            { "timesVisited", _realmStats[realm].timesVisited }
        };
    }

    private string GetRealmName(RealmType realm)
    {
        return realm switch
        {
            RealmType.NightmareForest => "Nightmare Forest",
            RealmType.AstralPlane => "Astral Plane",
            RealmType.VoidRealm => "Void Realm",
            RealmType.MemoryPalace => "Memory Palace",
            RealmType.DreamOcean => "Dream Ocean",
            RealmType.ChaosDimension => "Chaos Dimension",
            RealmType.ShadowRealm => "Shadow Realm",
            RealmType.CelestialGarden => "Celestial Garden",
            _ => "Unknown Realm"
        };
    }

    private string GetRealmDescription(RealmType realm)
    {
        return realm switch
        {
            RealmType.NightmareForest => "A dark forest filled with nightmares and shadow creatures.",
            RealmType.AstralPlane => "A ethereal plane where stars are born and die.",
            RealmType.VoidRealm => "A primordial void beyond mortal comprehension.",
            RealmType.MemoryPalace => "A palace built from forgotten memories.",
            RealmType.DreamOcean => "An endless ocean of pure dream energy.",
            RealmType.ChaosDimension => "A dimension of pure chaos and entropy.",
            RealmType.ShadowRealm => "A realm of eternal shadow and silence.",
            RealmType.CelestialGarden => "A heavenly garden of eternal beauty.",
            _ => "An unknown dream realm."
        };
    }

    private int GetRealmDifficulty(RealmType realm)
    {
        return realm switch
        {
            RealmType.NightmareForest => 1,
            RealmType.AstralPlane => 2,
            RealmType.MemoryPalace => 3,
            RealmType.DreamOcean => 4,
            RealmType.ShadowRealm => 5,
            RealmType.VoidRealm => 6,
            RealmType.ChaosDimension => 7,
            RealmType.CelestialGarden => 8,
            _ => 1
        };
    }

    // 获取属性加成
    public float GetDreamBuff(string buffType)
    {
        if (_dreamBuffs.ContainsKey(buffType))
            return _dreamBuffs[buffType];
        return 1.0f;
    }

    // 存档支持
    public Dictionary SaveData()
    {
        return new Dictionary
        {
            { "currentRealm", (int)_currentRealm },
            { "dreamLevel", _dreamLevel },
            { "enemiesDefeated", _enemiesDefeated },
            { "treasuresFound", _treasuresFound },
            { "timeInDream", _timeInDream },
            { "unlockedRealms", new List<int>() }
        };
    }

    public void LoadData(Dictionary data)
    {
        if (data.Contains("dreamLevel"))
            _dreamLevel = (int)data["dreamLevel"];
        if (data.Contains("enemiesDefeated"))
            _enemiesDefeated = (int)data["enemiesDefeated"];
        if (data.Contains("treasuresFound"))
            _treasuresFound = (int)data["treasuresFound"];
        if (data.Contains("timeInDream"))
            _timeInDream = (float)data["timeInDream"];
    }

    // 属性访问
    public DreamState CurrentState => _currentState;
    public RealmType CurrentRealm => _currentRealm;
    public int DreamLevel => _dreamLevel;
    public int MaxDreamLevel => _maxDreamLevel;
    public float TimeInDream => _timeInDream;
    public int EnemiesDefeated => _enemiesDefeated;
    public int TreasuresFound => _treasuresFound;
    public float DreamPowerMultiplier => _dreamPowerMultiplier;
    public bool IsInDream => _currentState == DreamState.Active;
    public Dictionary<RealmType, bool> UnlockedRealms => _unlockedRealms;
    public Dictionary<RealmType, RealmStats> RealmStats => _realmStats;
}
