using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 战斗统计数据 - 存储玩家战斗相关统计信息
/// </summary>
public class BattleStatsData
{
    // 总战斗统计
    public int TotalBattles { get; set; } = 0;
    public int TotalVictories { get; set; } = 0;
    public int TotalDefeats { get; set; } = 0;
    public float TotalBattleTime { get; set; } = 0f;
    
    // 伤害统计
    public int TotalDamageDealt { get; set; } = 0;
    public int TotalDamageTaken { get; set; } = 0;
    public int TotalCriticalDamage { get; set; } = 0;
    public int TotalHealing { get; set; } = 0;
    
    // 击杀统计
    public int TotalEnemiesKilled { get; set; } = 0;
    public int TotalBossesKilled { get; set; } = 0;
    public int TotalEliteKilled { get; set; } = 0;
    
    // 技能使用统计
    public int TotalSkillsUsed { get; set; } = 0;
    public int TotalSkillsHit { get; set; } = 0;
    public int TotalSkillsMissed { get; set; } = 0;
    
    // 元素伤害统计
    public int FireDamage { get; set; } = 0;
    public int IceDamage { get; set; } = 0;
    public int LightningDamage { get; set; } = 0;
    public int DarkDamage { get; set; } = 0;
    public int HolyDamage { get; set; } = 0;
    public int PhysicalDamage { get; set; } = 0;
    
    // 敌人生物类型击杀统计
    public Dictionary<string, int> EnemyKillsByType { get; set; } = new Dictionary<string, int>();
    
    // 最近战斗记录（最后10场）
    public List<BattleRecord> RecentBattles { get; set; } = new List<BattleRecord>();
    
    // 会话统计
    public int SessionBattles { get; set; } = 0;
    public int SessionVictories { get; set; } = 0;
    public DateTime SessionStart { get; set; } = DateTime.Now;
}

/// <summary>
/// 单场战斗记录 - 存储单场战斗的详细信息
/// </summary>
public class BattleRecord
{
    public DateTime Timestamp { get; set; }
    public bool Victory { get; set; }
    public int DamageDealt { get; set; }
    public int DamageTaken { get; set; }
    public int EnemiesKilled { get; set; }
    public float Duration { get; set; }
    public string BattleType { get; set; } = "";
}

/// <summary>
/// 战斗统计系统 - 追踪和管理玩家战斗相关统计数据
/// 继承自 BaseSystem，实现数据持久化
/// </summary>
public class BattleStatisticsSystem : BaseSystem
{
    private BattleStatsData _stats = new BattleStatsData();
    private bool _battleActive = false;
    private DateTime _battleStartTime;
    private int _currentBattleDamageDealt = 0;
    private int _currentBattleDamageTaken = 0;
    private int _currentBattleEnemiesKilled = 0;
    private int _currentBattleSkillsUsed = 0;
    private int _currentBattleSkillsHit = 0;
    
    // Elemental tracking for current battle
    private int _currentFireDamage = 0;
    private int _currentIceDamage = 0;
    private int _currentLightningDamage = 0;
    private int _currentDarkDamage = 0;
    private int _currentHolyDamage = 0;
    private int _currentPhysicalDamage = 0;
    
    // Settings
    private int _maxRecentBattles = 10;
    
    protected override void Initialize()
    {
        LoadData();
    }

    public override void _Ready()
    {
        base._Ready();
    }
    
    /// <summary>
    /// 开始一场战斗
    /// </summary>
    /// <param name="battleType">战斗类型</param>
    public void StartBattle(string battleType = "Normal")
    {
        _battleActive = true;
        _battleStartTime = DateTime.Now;
        _currentBattleDamageDealt = 0;
        _currentBattleDamageTaken = 0;
        _currentBattleEnemiesKilled = 0;
        _currentBattleSkillsUsed = 0;
        _currentBattleSkillsHit = 0;
        
        // Reset elemental tracking
        _currentFireDamage = 0;
        _currentIceDamage = 0;
        _currentLightningDamage = 0;
        _currentDarkDamage = 0;
        _currentHolyDamage = 0;
        _currentPhysicalDamage = 0;
    }
    
    /// <summary>
    /// 结束一场战斗
    /// </summary>
    /// <param name="victory">是否胜利</param>
    public void EndBattle(bool victory)
    {
        if (!_battleActive) return;
        
        float battleDuration = (float)(DateTime.Now - _battleStartTime).TotalSeconds;
        
        // Update totals
        _stats.TotalBattles++;
        _stats.SessionBattles++;
        
        if (victory)
        {
            _stats.TotalVictories++;
            _stats.SessionVictories++;
        }
        else
        {
            _stats.TotalDefeats++;
        }
        
        _stats.TotalBattleTime += battleDuration;
        _stats.TotalDamageDealt += _currentBattleDamageDealt;
        _stats.TotalDamageTaken += _currentBattleDamageTaken;
        _stats.TotalSkillsUsed += _currentBattleSkillsUsed;
        
        // Elemental totals
        _stats.FireDamage += _currentFireDamage;
        _stats.IceDamage += _currentIceDamage;
        _stats.LightningDamage += _currentLightningDamage;
        _stats.DarkDamage += _currentDarkDamage;
        _stats.HolyDamage += _currentHolyDamage;
        _stats.PhysicalDamage += _currentPhysicalDamage;
        
        // Add recent battle record
        BattleRecord record = new BattleRecord
        {
            Timestamp = DateTime.Now,
            Victory = victory,
            DamageDealt = _currentBattleDamageDealt,
            DamageTaken = _currentBattleDamageTaken,
            EnemiesKilled = _currentBattleEnemiesKilled,
            Duration = battleDuration,
            BattleType = "Normal"
        };
        
        _stats.RecentBattles.Insert(0, record);
        
        // Keep only last N battles
        if (_stats.RecentBattles.Count > _maxRecentBattles)
        {
            _stats.RecentBattles.RemoveAt(_stats.RecentBattles.Count - 1);
        }
        
        _battleActive = false;
        SaveData();
    }
    
    /// <summary>
    /// 记录造成的伤害
    /// </param>
    /// <param name="damage">伤害值</param>
    /// <param name="element">元素类型</param>
    /// <param name="isCritical">是否暴击</param>
    public void RecordDamageDealt(int damage, string element = "Physical", bool isCritical = false)
    {
        if (!_battleActive) return;
        
        _currentBattleDamageDealt += damage;
        
        // Track elemental damage
        switch (element.ToLower())
        {
            case "fire":
                _currentFireDamage += damage;
                break;
            case "ice":
                _currentIceDamage += damage;
                break;
            case "lightning":
                _currentLightningDamage += damage;
                break;
            case "dark":
                _currentDarkDamage += damage;
                break;
            case "holy":
                _currentHolyDamage += damage;
                break;
            default:
                _currentPhysicalDamage += damage;
                break;
        }
        
        if (isCritical)
        {
            _stats.TotalCriticalDamage += damage;
        }
    }
    
    /// <summary>
    /// 记录受到的伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void RecordDamageTaken(int damage)
    {
        if (!_battleActive) return;
        _currentBattleDamageTaken += damage;
    }
    
    /// <summary>
    /// 记录击杀敌人
    /// </summary>
    /// <param name="enemyType">敌人类型</param>
    /// <param name="isBoss">是否Boss</param>
    /// <param name="isElite">是否精英怪</param>
    public void RecordEnemyKilled(string enemyType, bool isBoss = false, bool isElite = false)
    {
        if (!_battleActive) return;
        
        _currentBattleEnemiesKilled++;
        _stats.TotalEnemiesKilled++;
        
        if (isBoss)
        {
            _stats.TotalBossesKilled++;
        }
        else if (isElite)
        {
            _stats.TotalEliteKilled++;
        }
        
        // Track by type
        if (!_stats.EnemyKillsByType.ContainsKey(enemyType))
        {
            _stats.EnemyKillsByType[enemyType] = 0;
        }
        _stats.EnemyKillsByType[enemyType]++;
    }
    
    /// <summary>
    /// 记录技能使用
    /// </summary>
    /// <param name="hit">是否命中</param>
    public void RecordSkillUsed(bool hit)
    {
        if (!_battleActive) return;
        
        _currentBattleSkillsUsed++;
        
        if (hit)
        {
            _currentBattleSkillsHit++;
            _stats.TotalSkillsHit++;
        }
        else
        {
            _stats.TotalSkillsMissed++;
        }
    }
    
    public void RecordHealing(int amount)
    {
        _stats.TotalHealing += amount;
    }
    
    // Statistics Accessors
    public float GetWinRate()
    {
        if (_stats.TotalBattles == 0) return 0f;
        return (float)_stats.TotalVictories / _stats.TotalBattles * 100f;
    }
    
    public float GetAverageDamagePerBattle()
    {
        if (_stats.TotalBattles == 0) return 0f;
        return (float)_stats.TotalDamageDealt / _stats.TotalBattles;
    }
    
    public float GetAverageBattleDuration()
    {
        if (_stats.TotalBattles == 0) return 0f;
        return _stats.TotalBattleTime / _stats.TotalBattles;
    }
    
    public float GetSkillAccuracy()
    {
        if (_stats.TotalSkillsUsed == 0) return 0f;
        return (float)_stats.TotalSkillsHit / _stats.TotalSkillsUsed * 100f;
    }
    
    public string GetMostKilledEnemy()
    {
        string mostKilled = "";
        int maxKills = 0;
        
        foreach (var kvp in _stats.EnemyKillsByType)
        {
            if (kvp.Value > maxKills)
            {
                maxKills = kvp.Value;
                mostKilled = kvp.Key;
            }
        }
        
        return mostKilled;
    }
    
    public string GetDominantElement()
    {
        int maxDamage = 0;
        string dominantElement = "Physical";
        
        if (_stats.FireDamage > maxDamage) { maxDamage = _stats.FireDamage; dominantElement = "Fire"; }
        if (_stats.IceDamage > maxDamage) { maxDamage = _stats.IceDamage; dominantElement = "Ice"; }
        if (_stats.LightningDamage > maxDamage) { maxDamage = _stats.LightningDamage; dominantElement = "Lightning"; }
        if (_stats.DarkDamage > maxDamage) { maxDamage = _stats.DarkDamage; dominantElement = "Dark"; }
        if (_stats.HolyDamage > maxDamage) { maxDamage = _stats.HolyDamage; dominantElement = "Holy"; }
        if (_stats.PhysicalDamage > maxDamage) { dominantElement = "Physical"; }
        
        return dominantElement;
    }
    
    public BattleStatsData GetStats() => _stats;
    
    public bool IsBattleActive() => _battleActive;
    
    // Data Persistence
    private string GetSavePath()
    {
        return "user://battle_stats.save";
    }
    
    public override Dictionary ExportSaveData()
    {
        return _stats.ToDictionary();
    }
    
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("TotalBattles")) _stats.TotalBattles = Convert.ToInt32(data["TotalBattles"]);
        if (data.Contains("TotalVictories")) _stats.TotalVictories = Convert.ToInt32(data["TotalVictories"]);
        if (data.Contains("TotalDefeats")) _stats.TotalDefeats = Convert.ToInt32(data["TotalDefeats"]);
        if (data.Contains("TotalBattleTime")) _stats.TotalBattleTime = (float)Convert.ToDouble(data["TotalBattleTime"]);
        if (data.Contains("TotalDamageDealt")) _stats.TotalDamageDealt = Convert.ToInt32(data["TotalDamageDealt"]);
        if (data.Contains("TotalDamageTaken")) _stats.TotalDamageTaken = Convert.ToInt32(data["TotalDamageTaken"]);
        if (data.Contains("TotalCriticalDamage")) _stats.TotalCriticalDamage = Convert.ToInt32(data["TotalCriticalDamage"]);
        if (data.Contains("TotalHealing")) _stats.TotalHealing = Convert.ToInt32(data["TotalHealing"]);
        if (data.Contains("TotalEnemiesKilled")) _stats.TotalEnemiesKilled = Convert.ToInt32(data["TotalEnemiesKilled"]);
        if (data.Contains("TotalBossesKilled")) _stats.TotalBossesKilled = Convert.ToInt32(data["TotalBossesKilled"]);
        if (data.Contains("TotalEliteKilled")) _stats.TotalEliteKilled = Convert.ToInt32(data["TotalEliteKilled"]);
        if (data.Contains("TotalSkillsUsed")) _stats.TotalSkillsUsed = Convert.ToInt32(data["TotalSkillsUsed"]);
        if (data.Contains("TotalSkillsHit")) _stats.TotalSkillsHit = Convert.ToInt32(data["TotalSkillsHit"]);
        if (data.Contains("TotalSkillsMissed")) _stats.TotalSkillsMissed = Convert.ToInt32(data["TotalSkillsMissed"]);
        if (data.Contains("FireDamage")) _stats.FireDamage = Convert.ToInt32(data["FireDamage"]);
        if (data.Contains("IceDamage")) _stats.IceDamage = Convert.ToInt32(data["IceDamage"]);
        if (data.Contains("LightningDamage")) _stats.LightningDamage = Convert.ToInt32(data["LightningDamage"]);
        if (data.Contains("DarkDamage")) _stats.DarkDamage = Convert.ToInt32(data["DarkDamage"]);
        if (data.Contains("HolyDamage")) _stats.HolyDamage = Convert.ToInt32(data["HolyDamage"]);
        if (data.Contains("PhysicalDamage")) _stats.PhysicalDamage = Convert.ToInt32(data["PhysicalDamage"]);
        if (data.Contains("SessionBattles")) _stats.SessionBattles = Convert.ToInt32(data["SessionBattles"]);
        if (data.Contains("SessionVictories")) _stats.SessionVictories = Convert.ToInt32(data["SessionVictories"]);
    }
    
    public void ResetStats()
    {
        _stats = new BattleStatsData();
        SaveData();
    }
}
