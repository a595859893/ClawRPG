using Godot;
using System;
using System.Collections.Generic;

public class ComboChainSystem : BaseSystem
{
    // 单例实例
    private static ComboChainSystem _instance;
    public static ComboChainSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ComboChainSystem();
            }
            return _instance;
        }
    }
    
    // 数据引用
    private ComboChainData _data;
    private ComboChainDatabase _database;
    
    // 连击时间限制（秒）
    private float _chainTimeLimit = 3.0f;
    
    // 当前活跃的加成效果
    private List<ChainBonusConfig> _activeBonuses = new List<ChainBonusConfig>();
    
    // 事件信号
    [Signal]
    public delegate void ChainStarted(int chainCount);
    
    [Signal]
    public delegate void ChainEnded(int maxChain, float totalDamage);
    
    [Signal]
    public delegate void ChainBonusActivated(int chainRequired, string effectName);
    
    [Signal]
    public delegate void ComboLevelUp(int newLevel, string levelName);
    
    public override void _Ready()
    {
        _database = ComboChainDatabase.Instance;
        
        // 获取数据节点
        _data = GetNode<ComboChainData>("/root/ComboChainData");
        if (_data == null)
        {
            GD.PrintErr("ComboChainData not found!");
        }
    }
    
    public override void _Process(float delta)
    {
        if (_data == null) return;
        
        // 更新连击计时器
        if (_data.IsChainActive && _data.CurrentChain > 0)
        {
            _data.ChainTimer -= delta;
            
            if (_data.ChainTimer <= 0)
            {
                EndChain();
            }
        }
    }
    
    // 触发连击
    public void TriggerChain(float damage, ComboChainDatabase.ComboType comboType = ComboChainDatabase.ComboType.Light)
    {
        if (_data == null || _database == null) return;
        
        bool isNewChain = !_data.IsChainActive || _data.CurrentChain == 0;
        
        if (isNewChain)
        {
            StartNewChain();
        }
        
        // 增加连击数
        _data.CurrentChain++;
        
        // 更新最大连击
        if (_data.CurrentChain > _data.MaxChain)
        {
            _data.MaxChain = _data.CurrentChain;
        }
        
        // 更新最大历史连击
        if (_data.MaxChain > _data.MaxChainEver)
        {
            _data.MaxChainEver = _data.CurrentChain;
        }
        
        // 计算伤害加成
        float bonusDamage = _database.CalculateChainDamage(damage, _data.CurrentChain, comboType) - damage;
        
        // 更新统计
        _data.TotalChainHits++;
        _data.TotalChainDamage += bonusDamage;
        _data.ChainDamageBonus += bonusDamage;
        
        // 重置计时器
        _data.ChainTimer = _chainTimeLimit;
        
        // 获取连击类型配置
        var typeConfig = _database.ComboTypeConfigs[comboType];
        if (typeConfig != null)
        {
            _data.ChainTimer = _chainTimeLimit * typeConfig.ChainTimeMultiplier;
        }
        
        // 检查连击等级提升
        int oldLevel = _database.GetComboLevel(_data.CurrentChain - 1);
        int newLevel = _database.GetComboLevel(_data.CurrentChain);
        
        if (newLevel > oldLevel)
        {
            EmitSignal(nameof(ComboLevelUp), newLevel, _database.GetComboLevelConfig(newLevel).Name);
        }
        
        // 检查连击加成激活
        CheckChainBonuses();
        
        // 添加到历史记录
        AddToHistory(damage, bonusDamage, (int)comboType);
        
        // 如果是新连击，发射信号
        if (isNewChain)
        {
            EmitSignal(nameof(ChainStarted), _data.CurrentChain);
        }
    }
    
    // 开始新连击
    private void StartNewChain()
    {
        _data.CurrentChain = 1;
        _data.IsChainActive = true;
        _data.ChainTimer = _chainTimeLimit;
        
        // 清空活跃加成
        _activeBonuses.Clear();
    }
    
    // 结束连击
    private void EndChain()
    {
        if (_data.CurrentChain > 0)
        {
            // 更新统计
            _data.TotalChains++;
            
            // 记录连击等级统计
            if (_data.CurrentChain >= 10) _data.Chain10Count++;
            if (_data.CurrentChain >= 25) _data.Chain25Count++;
            if (_data.CurrentChain >= 50) _data.Chain50Count++;
            if (_data.CurrentChain >= 100) _data.Chain100Count++;
            
            // 发射结束信号
            EmitSignal(nameof(ChainEnded), _data.MaxChain, _data.TotalChainDamage);
        }
        
        // 重置状态
        _data.CurrentChain = 0;
        _data.IsChainActive = false;
        _data.ChainTimer = 0;
        
        // 清空活跃加成
        _activeBonuses.Clear();
    }
    
    // 手动结束连击（玩家主动结束）
    public void ForceEndChain()
    {
        EndChain();
    }
    
    // 检查连击加成
    private void CheckChainBonuses()
    {
        foreach (var kvp in _database.ChainBonusConfigs)
        {
            if (_data.CurrentChain >= kvp.Key && !_activeBonuses.Contains(kvp.Value))
            {
                _activeBonuses.Add(kvp.Value);
                EmitSignal(nameof(ChainBonusActivated), kvp.Key, kvp.Value.EffectName);
            }
        }
    }
    
    // 添加到历史记录
    private void AddToHistory(float damage, float bonusDamage, int comboType)
    {
        var record = new ComboChainData.ChainRecord
        {
            ChainLevel = _database.GetComboLevel(_data.CurrentChain),
            Damage = damage,
            BonusDamage = bonusDamage,
            ComboType = comboType,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        
        _data.ChainHistory.Insert(0, record);
        
        // 保持历史记录数量在100条以内
        if (_data.ChainHistory.Count > 100)
        {
            _data.ChainHistory.RemoveAt(_data.ChainHistory.Count - 1);
        }
    }
    
    // 获取当前连击数
    public int GetCurrentChain()
    {
        return _data != null ? _data.CurrentChain : 0;
    }
    
    // 获取最大连击
    public int GetMaxChain()
    {
        return _data != null ? _data.MaxChain : 0;
    }
    
    // 获取当前连击等级
    public int GetCurrentComboLevel()
    {
        if (_data == null) return 1;
        return _database.GetComboLevel(_data.CurrentChain);
    }
    
    // 获取连击等级名称
    public string GetCurrentComboLevelName()
    {
        int level = GetCurrentComboLevel();
        return _database.GetComboLevelConfig(level).Name;
    }
    
    // 获取连击等级配置
    public ComboChainDatabase.ComboLevelConfig GetCurrentComboLevelConfig()
    {
        int level = GetCurrentComboLevel();
        return _database.GetComboLevelConfig(level);
    }
    
    // 获取连击伤害加成
    public float GetChainDamageBonus()
    {
        if (_data == null) return 0;
        
        var bonus = _database.GetChainBonus(_data.CurrentChain);
        return bonus != null ? bonus.DamageBonus : 0;
    }
    
    // 获取连击速度加成
    public float GetChainSpeedBonus()
    {
        if (_data == null) return 0;
        
        var bonus = _database.GetChainBonus(_data.CurrentChain);
        return bonus != null ? bonus.SpeedBonus : 0;
    }
    
    // 获取连击暴击加成
    public float GetChainCritBonus()
    {
        if (_data == null) return 0;
        
        var bonus = _database.GetChainBonus(_data.CurrentChain);
        return bonus != null ? bonus.CritBonus : 0;
    }
    
    // 获取当前活跃加成
    public List<ChainBonusConfig> GetActiveBonuses()
    {
        return _activeBonuses;
    }
    
    // 获取统计信息
    public Dictionary GetStatistics()
    {
        if (_data == null) return new Dictionary();
        
        var stats = new Dictionary();
        stats["totalChains"] = _data.TotalChains;
        stats["totalChainHits"] = _data.TotalChainHits;
        stats["maxChainEver"] = _data.MaxChainEver;
        stats["chain10Count"] = _data.Chain10Count;
        stats["chain25Count"] = _data.Chain25Count;
        stats["chain50Count"] = _data.Chain50Count;
        stats["chain100Count"] = _data.Chain100Count;
        stats["totalChainDamage"] = _data.TotalChainDamage;
        stats["chainDamageBonus"] = _data.ChainDamageBonus;
        
        return stats;
    }
    
    // 获取历史记录
    public List<ComboChainData.ChainRecord> GetHistory(int count = 10)
    {
        if (_data == null) return new List<ComboChainData.ChainRecord>();
        
        int actualCount = Math.Min(count, _data.ChainHistory.Count);
        return _data.ChainHistory.GetRange(0, actualCount);
    }
    
    // 设置连击时间限制
    public void SetChainTimeLimit(float seconds)
    {
        _chainTimeLimit = seconds;
    }
    
    // 获取连击时间限制
    public float GetChainTimeLimit()
    {
        return _chainTimeLimit;
    }
    
    // 获取剩余时间
    public float GetRemainingTime()
    {
        return _data != null ? _data.ChainTimer : 0;
    }
    
    // 检查连击是否活跃
    public bool IsChainActive()
    {
        return _data != null && _data.IsChainActive;
    }
    
    // 重置统计
    public void ResetStatistics()
    {
        if (_data == null) return;
        
        _data.TotalChains = 0;
        _data.TotalChainHits = 0;
        _data.MaxChainEver = 0;
        _data.Chain10Count = 0;
        _data.Chain25Count = 0;
        _data.Chain50Count = 0;
        _data.Chain100Count = 0;
        _data.TotalChainDamage = 0;
        _data.ChainDamageBonus = 0;
        _data.ChainHistory.Clear();
    }
}
