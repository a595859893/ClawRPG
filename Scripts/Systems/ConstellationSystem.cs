using Godot;
using System;
using System.Collections.Generic;

public class ConstellationSystem
{
    private static ConstellationSystem _instance;
    public static ConstellationSystem Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ConstellationSystem();
            return _instance;
        }
    }

    // 星座类型
    public enum ConstellationType
    {
        Aries, Taurus, Gemini, Cancer, Leo, Virgo,
        Libra, Scorpio, Sagittarius, Capricorn, Aquarius, Pisces
    }

    // 星点数据
    private Dictionary<string, Dictionary<string, float>> _starData = new Dictionary<string, Dictionary<string, float>>();
    private HashSet<string> _unlockedStars = new HashSet<string>();
    private HashSet<string> _activatedStars = new HashSet<string>();
    private int _totalPointsSpent = 0;

    // 星座解锁需要的总星数
    private Dictionary<ConstellationType, int> _constellationUnlockRequirements = new Dictionary<ConstellationType, int>();

    public ConstellationSystem()
    {
        InitializeStarData();
        InitializeConstellationRequirements();
    }

    private void InitializeConstellationRequirements()
    {
        _constellationUnlockRequirements[ConstellationType.Aries] = 3;
        _constellationUnlockRequirements[ConstellationType.Taurus] = 3;
        _constellationUnlockRequirements[ConstellationType.Gemini] = 3;
        _constellationUnlockRequirements[ConstellationType.Cancer] = 3;
        _constellationUnlockRequirements[ConstellationType.Leo] = 3;
        _constellationUnlockRequirements[ConstellationType.Virgo] = 3;
        _constellationUnlockRequirements[ConstellationType.Libra] = 3;
        _constellationUnlockRequirements[ConstellationType.Scorpio] = 3;
        _constellationUnlockRequirements[ConstellationType.Sagittarius] = 3;
        _constellationUnlockRequirements[ConstellationType.Capricorn] = 3;
        _constellationUnlockRequirements[ConstellationType.Aquarius] = 3;
        _constellationUnlockRequirements[ConstellationType.Pisces] = 3;
    }

    private void InitializeStarData()
    {
        // Aries - Attack
        AddStar("aries_1", "Aries Spark", new Dictionary<string, float> { { "attack", 5 } });
        AddStar("aries_2", "Aries Fury", new Dictionary<string, float> { { "attack", 8 } });
        AddStar("aries_3", "Aries Strike", new Dictionary<string, float> { { "attack", 12 } });
        AddStar("aries_4", "Aries Power", new Dictionary<string, float> { { "attack", 15 }, { "crit_rate", 2 } });
        AddStar("aries_5", "Aries Champion", new Dictionary<string, float> { { "attack", 20 }, { "crit_rate", 3 }, { "crit_damage", 10 } });
        AddStar("aries_core", "Aries Core", new Dictionary<string, float> { { "attack", 30 }, { "crit_rate", 5 }, { "crit_damage", 15 } });

        // Taurus - Defense
        AddStar("taurus_1", "Taurus Stone", new Dictionary<string, float> { { "defense", 5 } });
        AddStar("taurus_2", "Taurus Wall", new Dictionary<string, float> { { "defense", 8 } });
        AddStar("taurus_3", "Taurus Shield", new Dictionary<string, float> { { "defense", 12 } });
        AddStar("taurus_4", "Taurus Iron", new Dictionary<string, float> { { "defense", 15 }, { "health", 50 } });
        AddStar("taurus_5", "Taurus Guardian", new Dictionary<string, float> { { "defense", 20 }, { "health", 80 }, { "dodge", 3 } });
        AddStar("taurus_core", "Taurus Core", new Dictionary<string, float> { { "defense", 30 }, { "health", 120 }, { "dodge", 5 } });

        // Gemini - Speed
        AddStar("gemini_1", "Gemini Swift", new Dictionary<string, float> { { "speed", 2 } });
        AddStar("gemini_2", "Gemini Wind", new Dictionary<string, float> { { "speed", 3 } });
        AddStar("gemini_3", "Gemini Breeze", new Dictionary<string, float> { { "speed", 5 } });
        AddStar("gemini_4", "Gemini Lightning", new Dictionary<string, float> { { "speed", 7 }, { "dodge", 3 } });
        AddStar("gemini_5", "Gemini Phantom", new Dictionary<string, float> { { "speed", 10 }, { "dodge", 5 }, { "crit_rate", 3 } });
        AddStar("gemini_core", "Gemini Core", new Dictionary<string, float> { { "speed", 15 }, { "dodge", 8 }, { "crit_rate", 5 } });

        // Cancer - Health
        AddStar("cancer_1", "Cancer Drop", new Dictionary<string, float> { { "health", 30 } });
        AddStar("cancer_2", "Cancer Stream", new Dictionary<string, float> { { "health", 50 } });
        AddStar("cancer_3", "Cancer River", new Dictionary<string, float> { { "health", 80 } });
        AddStar("cancer_4", "Cancer Sea", new Dictionary<string, float> { { "health", 100 }, { "health_regen", 2 } });
        AddStar("cancer_5", "Cancer Leviathan", new Dictionary<string, float> { { "health", 150 }, { "health_regen", 4 }, { "lifesteal", 3 } });
        AddStar("cancer_core", "Cancer Core", new Dictionary<string, float> { { "health", 200 }, { "health_regen", 6 }, { "lifesteal", 5 } });

        // Leo - Crit Rate
        AddStar("leo_1", "Leo Spark", new Dictionary<string, float> { { "crit_rate", 1 } });
        AddStar("leo_2", "Leo Flame", new Dictionary<string, float> { { "crit_rate", 2 } });
        AddStar("leo_3", "Leo Blaze", new Dictionary<string, float> { { "crit_rate", 3 } });
        AddStar("leo_4", "Leo Sun", new Dictionary<string, float> { { "crit_rate", 4 }, { "attack", 10 } });
        AddStar("leo_5", "Leo Radiant", new Dictionary<string, float> { { "crit_rate", 5 }, { "attack", 15 }, { "crit_damage", 10 } });
        AddStar("leo_core", "Leo Core", new Dictionary<string, float> { { "crit_rate", 8 }, { "attack", 25 }, { "crit_damage", 15 } });

        // Virgo - Healing
        AddStar("virgo_1", "Virgo Drop", new Dictionary<string, float> { { "healing", 5 } });
        AddStar("virgo_2", "Virgo Spring", new Dictionary<string, float> { { "healing", 8 } });
        AddStar("virgo_3", "Virgo Fountain", new Dictionary<string, float> { { "healing", 12 } });
        AddStar("virgo_4", "Virgo Lotus", new Dictionary<string, float> { { "healing", 15 }, { "health_regen", 3 } });
        AddStar("virgo_5", "Virgo Garden", new Dictionary<string, float> { { "healing", 20 }, { "health_regen", 5 }, { "lifesteal", 5 } });
        AddStar("virgo_core", "Virgo Core", new Dictionary<string, float> { { "healing", 30 }, { "health_regen", 8 }, { "lifesteal", 8 } });

        // Libra - Balance
        AddStar("libra_1", "Libra Balance", new Dictionary<string, float> { { "attack", 2 }, { "defense", 2 } });
        AddStar("libra_2", "Libra Harmony", new Dictionary<string, float> { { "attack", 3 }, { "defense", 3 } });
        AddStar("libra_3", "Libra Order", new Dictionary<string, float> { { "attack", 5 }, { "defense", 5 } });
        AddStar("libra_4", "Libra Scale", new Dictionary<string, float> { { "attack", 7 }, { "defense", 7 }, { "health", 30 } });
        AddStar("libra_5", "Libra Perfect", new Dictionary<string, float> { { "attack", 10 }, { "defense", 10 }, { "health", 50 }, { "speed", 2 } });
        AddStar("libra_core", "Libra Core", new Dictionary<string, float> { { "attack", 15 }, { "defense", 15 }, { "health", 80 }, { "speed", 3 } });

        // Scorpio - Crit Damage
        AddStar("scorpio_1", "Scorpio Sting", new Dictionary<string, float> { { "crit_damage", 5 } });
        AddStar("scorpio_2", "Scorpio Venom", new Dictionary<string, float> { { "crit_damage", 8 } });
        AddStar("scorpio_3", "Scorpio Poison", new Dictionary<string, float> { { "crit_damage", 12 } });
        AddStar("scorpio_4", "Scorpio Assassin", new Dictionary<string, float> { { "crit_damage", 15 }, { "crit_rate", 3 } });
        AddStar("scorpio_5", "Scorpio Night", new Dictionary<string, float> { { "crit_damage", 20 }, { "crit_rate", 4 }, { "lifesteal", 5 } });
        AddStar("scorpio_core", "Scorpio Core", new Dictionary<string, float> { { "crit_damage", 30 }, { "crit_rate", 6 }, { "lifesteal", 8 } });

        // Sagittarius - EXP
        AddStar("sagittarius_1", "Sagittarius Arrow", new Dictionary<string, float> { { "exp_bonus", 3 } });
        AddStar("sagittarius_2", "Sagittarius Bow", new Dictionary<string, float> { { "exp_bonus", 5 } });
        AddStar("sagittarius_3", "Sagittarius Hunter", new Dictionary<string, float> { { "exp_bonus", 8 } });
        AddStar("sagittarius_4", "Sagittarius Archer", new Dictionary<string, float> { { "exp_bonus", 10 }, { "attack", 8 } });
        AddStar("sagittarius_5", "Sagittarius Legend", new Dictionary<string, float> { { "exp_bonus", 15 }, { "attack", 15 }, { "speed", 3 } });
        AddStar("sagittarius_core", "Sagittarius Core", new Dictionary<string, float> { { "exp_bonus", 20 }, { "attack", 20 }, { "speed", 5 } });

        // Capricorn - Gold
        AddStar("capricorn_1", "Capricorn Coin", new Dictionary<string, float> { { "gold_bonus", 3 } });
        AddStar("capricorn_2", "Capricorn Silver", new Dictionary<string, float> { { "gold_bonus", 5 } });
        AddStar("capricorn_3", "Capricorn Gold", new Dictionary<string, float> { { "gold_bonus", 8 } });
        AddStar("capricorn_4", "Capricorn Rich", new Dictionary<string, float> { { "gold_bonus", 10 }, { "defense", 8 } });
        AddStar("capricorn_5", "Capricorn King", new Dictionary<string, float> { { "gold_bonus", 15 }, { "defense", 15 }, { "luck", 3 } });
        AddStar("capricorn_core", "Capricorn Core", new Dictionary<string, float> { { "gold_bonus", 20 }, { "defense", 20 }, { "luck", 5 } });

        // Aquarius - Magic
        AddStar("aquarius_1", "Aquarius Drop", new Dictionary<string, float> { { "magic", 5 } });
        AddStar("aquarius_2", "Aquarius Stream", new Dictionary<string, float> { { "magic", 8 } });
        AddStar("aquarius_3", "Aquarius River", new Dictionary<string, float> { { "magic", 12 } });
        AddStar("aquarius_4", "Aquarius Storm", new Dictionary<string, float> { { "magic", 15 }, { "mana", 30 } });
        AddStar("aquarius_5", "Aquarius Master", new Dictionary<string, float> { { "magic", 20 }, { "mana", 50 }, { "health_regen", 3 } });
        AddStar("aquarius_core", "Aquarius Core", new Dictionary<string, float> { { "magic", 30 }, { "mana", 80 }, { "health_regen", 5 } });

        // Pisces - Special
        AddStar("pisces_1", "Pisces Drop", new Dictionary<string, float> { { "lifesteal", 2 } });
        AddStar("pisces_2", "Pisces Stream", new Dictionary<string, float> { { "lifesteal", 3 } });
        AddStar("pisces_3", "Pisces River", new Dictionary<string, float> { { "lifesteal", 5 } });
        AddStar("pisces_4", "Pisces Ocean", new Dictionary<string, float> { { "lifesteal", 7 }, { "dodge", 3 } });
        AddStar("pisces_5", "Pisces Divine", new Dictionary<string, float> { { "lifesteal", 10 }, { "dodge", 5 }, { "luck", 3 } });
        AddStar("pisces_core", "Pisces Core", new Dictionary<string, float> { { "lifesteal", 15 }, { "dodge", 8 }, { "luck", 5 } });
    }

    private void AddStar(string id, string name, Dictionary<string, float> attributes)
    {
        _starData[id] = attributes;
    }

    public bool UnlockStar(string starId)
    {
        if (_unlockedStars.Contains(starId))
            return false;

        _unlockedStars.Add(starId);
        return true;
    }

    public bool ActivateStar(string starId)
    {
        if (!_unlockedStars.Contains(starId))
            return false;

        if (_activatedStars.Contains(starId))
            return false;

        _activatedStars.Add(starId);
        _totalPointsSpent++;
        return true;
    }

    public Dictionary<string, float> GetActivatedBonuses()
    {
        Dictionary<string, float> totalBonuses = new Dictionary<string, float>();

        foreach (string starId in _activatedStars)
        {
            if (_starData.ContainsKey(starId))
            {
                foreach (var kvp in _starData[starId])
                {
                    if (totalBonuses.ContainsKey(kvp.Key))
                        totalBonuses[kvp.Key] += kvp.Value;
                    else
                        totalBonuses[kvp.Key] = kvp.Value;
                }
            }
        }

        return totalBonuses;
    }

    public HashSet<string> GetUnlockedStars() { return _unlockedStars; }
    public HashSet<string> GetActivatedStars() { return _activatedStars; }
    public Dictionary<string, Dictionary<string, float>> GetAllStarData() { return _starData; }
    public int GetTotalPointsSpent() { return _totalPointsSpent; }

    public bool IsStarUnlocked(string starId) { return _unlockedStars.Contains(starId); }
    public bool IsStarActivated(string starId) { return _activatedStars.Contains(starId); }

    public void LoadData(HashSet<string> unlocked, HashSet<string> activated, int pointsSpent)
    {
        _unlockedStars = unlocked;
        _activatedStars = activated;
        _totalPointsSpent = pointsSpent;
    }

    public Dictionary<string, object> SaveData()
    {
        return new Dictionary<string, object>
        {
            { "unlocked", new List<string>(_unlockedStars) },
            { "activated", new List<string>(_activatedStars) },
            { "pointsSpent", _totalPointsSpent }
        };
    }
}
