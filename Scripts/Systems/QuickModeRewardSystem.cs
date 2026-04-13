using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 快速模式专属奖励系统
/// 提供快速模式的专属奖励加成和成就追踪
/// </summary>
public partial class QuickModeRewardSystem : BaseSystem
{
    public static QuickModeRewardSystem Instance { get; private set; }

    // 快速模式统计
    private int _quickModeWins = 0;
    private int _quickModePlays = 0;
    private int _quickModePerfectRuns = 0;  // 无伤通关
    private int _quickModeSpeedRuns = 0;    // 速通（低于目标时间）
    private float _bestQuickModeTime = float.MaxValue;
    private int _consecutiveQuickWins = 0;

    // 信号系统
    public delegate void QuickModeRewardGranted(int gold, int exp, string rewardType);
    public delegate void QuickModeAchievementUnlocked(string achievementId);
    public delegate void QuickModeStatsUpdated(string statName, int value);

    public override void _Ready()
    {
        Instance = this;
        base._Ready();
        LoadData();
    }

    /// <summary>
    /// 系统名称
    /// </summary>
    protected override string SystemName => "QuickModeReward";

    /// <summary>
    /// 初始化系统
    /// </summary>
    protected override void Initialize()
    {
        // 监听快速模式启用事件
        GameModeManager.QuickModeEnabled += OnQuickModeEnabled;
        
        // 监听游戏结束事件（需要集成到游戏结束流程）
        GD.Print("[QuickModeRewardSystem] Initialized - Quick Mode rewards ready");
    }

    /// <summary>
    /// 当快速模式启用时
    /// </summary>
    private void OnQuickModeEnabled()
    {
        _quickModePlays++;
        EmitSignal(SignalName.QuickModeStatsUpdated, "plays", _quickModePlays);
        SaveData();
    }

    /// <summary>
    /// 获取快速模式奖励乘数
    /// </summary>
    public float GetQuickModeBonusMultiplier()
    {
        if (!GameModeManager.Instance.IsQuickMode())
        {
            return 1.0f;
        }

        // 基础加成（来自配置）
        float baseBonus = GameModeManager.Instance.GetXPMultiplier();
        
        // 连胜加成（每连胜3场，额外+5%，最高+25%）
        float streakBonus = Math.Min(0.25f, (_consecutiveQuickWins / 3) * 0.05f);
        
        // 速通加成（如果打破了个人记录）
        float speedBonus = 0f;
        if (_bestQuickModeTime < GameModeManager.Instance.GetTargetDurationMinutes() * 60)
        {
            speedBonus = 0.1f;
        }

        return baseBonus + streakBonus + speedBonus;
    }

    /// <summary>
    /// 获取快速模式金币加成乘数
    /// </summary>
    public float GetQuickModeGoldMultiplier()
    {
        if (!GameModeManager.Instance.IsQuickMode())
        {
            return 1.0f;
        }

        // 基础加成（来自配置）
        float baseBonus = GameModeManager.Instance.GetGoldMultiplier();
        
        // 完美通关加成（无伤）
        float perfectBonus = _quickModePerfectRuns > 0 ? 0.1f : 0f;

        return baseBonus + perfectBonus;
    }

    /// <summary>
    /// 快速模式游戏胜利
    /// </summary>
    public void OnQuickModeVictory(float completionTimeSeconds, bool wasPerfect)
    {
        if (!GameModeManager.Instance.IsQuickMode())
        {
            return;
        }

        _quickModeWins++;
        _consecutiveQuickWins++;

        // 检查速通
        int targetSeconds = GameModeManager.Instance.GetTargetDurationMinutes() * 60;
        if (completionTimeSeconds < targetSeconds)
        {
            _quickModeSpeedRuns++;
            
            // 更新最佳时间
            if (completionTimeSeconds < _bestQuickModeTime)
            {
                _bestQuickModeTime = completionTimeSeconds;
            }
        }

        // 检查完美通关
        if (wasPerfect)
        {
            _quickModePerfectRuns++;
        }

        // 计算并发放奖励
        GrantQuickModeRewards(completionTimeSeconds, wasPerfect);

        // 检查成就
        CheckQuickModeAchievements();

        // 通知成就系统
        var achievementSystem = GetNode<AchievementSystem>("/root/AchievementSystem");
        if (achievementSystem != null)
        {
            achievementSystem.TrackQuickModeWin(_quickModeWins);
            achievementSystem.TrackQuickModePlay(_quickModePlays);
        }

        EmitSignal(SignalName.QuickModeStatsUpdated, "wins", _quickModeWins);
        SaveData();
        
        GD.Print($"[QuickModeReward] Victory! Wins: {_quickModeWins}, Streak: {_consecutiveQuickWins}, Time: {completionTimeSeconds:F1}s");
    }

    /// <summary>
    /// 快速模式游戏失败
    /// </summary>
    public void OnQuickModeDefeat()
    {
        if (!GameModeManager.Instance.IsQuickMode())
        {
            return;
        }

        _consecutiveQuickWins = 0;
        SaveData();
        
        GD.Print($"[QuickModeReward] Defeat! Streak reset.");
    }

    /// <summary>
    /// 发放快速模式奖励
    /// </summary>
    private void GrantQuickModeRewards(float completionTimeSeconds, bool wasPerfect)
    {
        // 基础奖励（根据完成时间）
        int baseGold = 100;
        int baseExp = 50;

        // 应用乘数
        float goldMultiplier = GetQuickModeGoldMultiplier();
        float expMultiplier = GetQuickModeBonusMultiplier();

        int goldReward = (int)(baseGold * goldMultiplier);
        int expReward = (int)(baseExp * expMultiplier);

        // 速通额外奖励
        if (completionTimeSeconds < GameModeManager.Instance.GetTargetDurationMinutes() * 60)
        {
            goldReward += (int)(baseGold * 0.2f);
            expReward += (int)(baseExp * 0.2f);
        }

        // 完美通关额外奖励
        if (wasPerfect)
        {
            goldReward += (int)(baseGold * 0.3f);
            expReward += (int)(baseExp * 0.3f);
        }

        // 发放奖励给玩家
        var player = GetPlayer();
        if (player != null)
        {
            player.AddGold(goldReward);
            player.AddExperience(expReward);
        }

        EmitSignal(SignalName.QuickModeRewardGranted, goldReward, expReward, "victory");
        
        GD.Print($"[QuickModeReward] Granted {goldReward} gold, {expReward} exp");
    }

    /// <summary>
    /// 检查快速模式成就
    /// </summary>
    private void CheckQuickModeAchievements()
    {
        // 这里可以添加检查快速模式专属成就的逻辑
        // 成就系统会根据统计自动解锁
        
        // 触发成就检查
        var achievementSystem = GetNode<AchievementSystem>("/root/AchievementSystem");
        if (achievementSystem != null)
        {
            // 首次胜利
            if (_quickModeWins == 1)
            {
                EmitSignal(SignalName.QuickModeAchievementUnlocked, "quick_first_win");
            }
            
            // 10连胜
            if (_consecutiveQuickWins >= 10)
            {
                EmitSignal(SignalName.QuickModeAchievementUnlocked, "quick_streak_10");
            }
            
            // 速通10次
            if (_quickModeSpeedRuns >= 10)
            {
                EmitSignal(SignalName.QuickModeAchievementUnlocked, "quick_speed_10");
            }
            
            // 完美通关5次
            if (_quickModePerfectRuns >= 5)
            {
                EmitSignal(SignalName.QuickModeAchievementUnlocked, "quick_perfect_5");
            }
        }
    }

    /// <summary>
    /// 获取玩家节点
    /// </summary>
    private Player GetPlayer()
    {
        // 尝试从主场景获取玩家
        var main = GetTree().CurrentScene;
        if (main != null)
        {
            var player = main.GetNode<Player>("Player");
            return player;
        }
        return null;
    }

    // ============ 数据持久化 ============

    /// <summary>
    /// 保存数据
    /// </summary>
    private void SaveData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = new Godot.Collections.Dictionary
        {
            ["quick_mode_wins"] = _quickModeWins,
            ["quick_mode_plays"] = _quickModePlays,
            ["quick_mode_perfect_runs"] = _quickModePerfectRuns,
            ["quick_mode_speed_runs"] = _quickModeSpeedRuns,
            ["best_quick_mode_time"] = _bestQuickModeTime,
            ["consecutive_quick_wins"] = _consecutiveQuickWins
        };

        saveSystem.SaveQuickModeData(data);
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    private void LoadData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadQuickModeData();
        if (data == null) return;

        _quickModeWins = (int)data.Get("quick_mode_wins", 0);
        _quickModePlays = (int)data.Get("quick_mode_plays", 0);
        _quickModePerfectRuns = (int)data.Get("quick_mode_perfect_runs", 0);
        _quickModeSpeedRuns = (int)data.Get("quick_mode_speed_runs", 0);
        _bestQuickModeTime = (float)data.Get("best_quick_mode_time", float.MaxValue);
        _consecutiveQuickWins = (int)data.Get("consecutive_quick_wins", 0);

        GD.Print($"[QuickModeReward] Data loaded: {_quickModeWins} wins, {_quickModePlays} plays");
    }

    // ============ 公开属性 ============

    public int QuickModeWins => _quickModeWins;
    public int QuickModePlays => _quickModePlays;
    public int QuickModePerfectRuns => _quickModePerfectRuns;
    public int QuickModeSpeedRuns => _quickModeSpeedRuns;
    public float BestQuickModeTime => _bestQuickModeTime == float.MaxValue ? 0 : _bestQuickModeTime;
    public int ConsecutiveQuickWins => _consecutiveQuickWins;

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary<string, object>
        {
            ["quick_mode_wins"] = _quickModeWins,
            ["quick_mode_plays"] = _quickModePlays,
            ["quick_mode_perfect_runs"] = _quickModePerfectRuns,
            ["quick_mode_speed_runs"] = _quickModeSpeedRuns,
            ["best_quick_mode_time"] = _bestQuickModeTime,
            ["consecutive_quick_wins"] = _consecutiveQuickWins
        };
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        _quickModeWins = (int)data.Get("quick_mode_wins", 0);
        _quickModePlays = (int)data.Get("quick_mode_plays", 0);
        _quickModePerfectRuns = (int)data.Get("quick_mode_perfect_runs", 0);
        _quickModeSpeedRuns = (int)data.Get("quick_mode_speed_runs", 0);
        _bestQuickModeTime = (float)data.Get("best_quick_mode_time", float.MaxValue);
        _consecutiveQuickWins = (int)data.Get("consecutive_quick_wins", 0);
    }

    /// <summary>
    /// 重置数据
    /// </summary>
    public override void Reset()
    {
        _quickModeWins = 0;
        _quickModePlays = 0;
        _quickModePerfectRuns = 0;
        _quickModeSpeedRuns = 0;
        _bestQuickModeTime = float.MaxValue;
        _consecutiveQuickWins = 0;
        SaveData();
    }
}
