using Godot;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Scripts.Crafting;

/// <summary>
/// 钓鱼系统管理器
/// </summary>
public class FishingSystem : BaseSystem
{
    private static FishingSystem _instance;
    public static FishingSystem Instance => _instance;
    
    // 钓鱼状态
    public FishingState State { get; private set; } = FishingState.Idle;
    
    // 当前使用的鱼竿
    public string CurrentRodId { get; private set; } = "rod_wooden";
    public int CurrentRodDurability { get; private set; } = 100;
    
    // 玩家钓鱼技能
    public FishingSkillData PlayerSkill { get; private set; } = new FishingSkillData();
    
    // 钓鱼位置
    public Vector2 FishingPosition { get; private set; }
    
    // 计时器
    private float _waitTimer;
    private float _biteTimer;
    private float _reelTimer;
    
    // 等待时间范围
    private const float MinWaitTime = 3.0f;
    private const float MaxWaitTime = 8.0f;
    
    // 咬钩时间窗口
    private const float BiteWindow = 1.5f;
    
    // 收线时间
    private const float ReelTime = 2.0f;
    
    // 信号
    public Action FishingStarted;
    public Action<FishingData> FishBit;
    public Action<FishingData, int> FishCaught;
    public Action FishMissed;
    public Action FishingEnded;
    public Action<FishingState> FishingStateChanged;
    public Action<int> LevelUp;
    public Action RodBroken;
    
    public override void _Ready()
    {
        _instance = this;
        LoadSkillData();
    }
    
    public override void _Process(float delta)
    {
        if (State == FishingState.Waiting)
        {
            _waitTimer -= delta;
            if (_waitTimer <= 0)
            {
                // 鱼咬钩了
                State = FishingState.Biting;
                _biteTimer = BiteWindow;
                FishingStateChanged?.Invoke(State);
                
                // 随机决定是否咬钩（考虑幸运加成）
                var rod = FishingDatabase.Instance.GetFishingRod(CurrentRodId);
                float catchChance = 0.7f * rod?.CatchBonus * PlayerSkill.CatchBonus;
                if (GD.RandFloat() < catchChance)
                {
                    // 随机选择一条鱼
                    var fish = FishingDatabase.Instance.RollFish(
                        PlayerSkill.Level, 
                        PlayerSkill.LuckBonus * 10
                    );
                    if (fish != null)
                    {
                        FishBit?.Invoke(fish);
                    }
                }
            }
        }
        else if (State == FishingState.Biting)
        {
            _biteTimer -= delta;
            if (_biteTimer <= 0)
            {
                // 错过了
                State = FishingState.Missed;
                FishMissed?.Invoke();
                EndFishing();
            }
        }
        else if (State == FishingState.Reeling)
        {
            _reelTimer -= delta;
            if (_reelTimer <= 0)
            {
                // 收线完成，检查是否成功
                TryCatchFish();
            }
        }
    }
    
    /// <summary>
    /// 开始钓鱼
    /// </summary>
    public void StartFishing(Vector2 position)
    {
        if (State != FishingState.Idle) return;
        
        var rod = FishingDatabase.Instance.GetFishingRod(CurrentRodId);
        if (rod == null) return;
        
        if (CurrentRodDurability <= 0)
        {
            RodBroken?.Invoke();
            return;
        }
        
        FishingPosition = position;
        State = FishingState.Casting;
        FishingStateChanged?.Invoke(State);
        
        // 抛竿动画时间
        GetTree().CreateTimer(0.5f).Timeout += () =>
        {
            State = FishingState.Waiting;
            _waitTimer = (float)GD.RandRange(MinWaitTime, MaxWaitTime);
            FishingStateChanged?.Invoke(State);
            FishingStarted?.Invoke();
        };
    }
    
    /// <summary>
    /// 提竿（鱼咬钩时）
    /// </summary>
    public void Reel()
    {
        if (State != FishingState.Biting) return;
        
        State = FishingState.Reeling;
        _reelTimer = ReelTime / (PlayerSkill.SpeedBonus * 
            (FishingDatabase.Instance.GetFishingRod(CurrentRodId)?.SpeedBonus ?? 1.0f));
        FishingStateChanged?.Invoke(State);
    }
    
    /// <summary>
    /// 尝试捕获鱼
    /// </summary>
    private void TryCatchFish()
    {
        var rod = FishingDatabase.Instance.GetFishingRod(CurrentRodId);
        
        // 消耗耐久度
        CurrentRodDurability -= rod?.DurabilityPerCast ?? 1;
        if (CurrentRodDurability <= 0)
        {
            RodBroken?.Invoke();
        }
        
        // 随机选择一条鱼（基于等级和幸运）
        var fish = FishingDatabase.Instance.RollFish(
            PlayerSkill.Level, 
            PlayerSkill.LuckBonus * 10
        );
        
        if (fish != null)
        {
            // 计算数量
            int quantity = (int)GD.RandRange(fish.MinQuantity, fish.MaxQuantity);
            
            // 添加到背包
            if (ItemSystem.Instance.AddItem(fish.ItemId, quantity))
            {
                State = FishingState.Caught;
                FishCaught?.Invoke(fish, quantity);
                
                // 获得经验
                AddExperience(fish.ExperienceReward);
            }
        }
        else
        {
            State = FishingState.Missed;
            FishMissed?.Invoke();
        }
        
        EndFishing();
    }
    
    /// <summary>
    /// 结束钓鱼
    /// </summary>
    public void EndFishing()
    {
        FishingEnded?.Invoke();
        State = FishingState.Idle;
        FishingStateChanged?.Invoke(State);
    }
    
    /// <summary>
    /// 取消钓鱼
    /// </summary>
    public void CancelFishing()
    {
        if (State == FishingState.Idle) return;
        
        State = FishingState.Idle;
        FishingStateChanged?.Invoke(State);
        FishingEnded?.Invoke();
    }
    
    /// <summary>
    /// 切换鱼竿
    /// </summary>
    public void ChangeRod(string rodId)
    {
        var rod = FishingDatabase.Instance.GetFishingRod(rodId);
        if (rod != null && PlayerSkill.Level >= rod.RequiredLevel)
        {
            CurrentRodId = rodId;
            CurrentRodDurability = rod.Durability;
        }
    }
    
    /// <summary>
    /// 修理鱼竿
    /// </summary>
    public void RepairRod(int amount)
    {
        var rod = FishingDatabase.Instance.GetFishingRod(CurrentRodId);
        if (rod != null)
        {
            CurrentRodDurability = Mathf.Min(CurrentRodDurability + amount, rod.Durability);
        }
    }
    
    /// <summary>
    /// 添加经验
    /// </summary>
    public void AddExperience(int amount)
    {
        if (PlayerSkill.Level >= 100) return;
        
        PlayerSkill.Experience += amount;
        
        while (PlayerSkill.Experience >= PlayerSkill.ExperienceToNextLevel && PlayerSkill.Level < 100)
        {
            PlayerSkill.Experience -= PlayerSkill.ExperienceToNextLevel;
            PlayerSkill.Level++;
            PlayerSkill.ExperienceToNextLevel = CalculateExpToNextLevel(PlayerSkill.Level);
            
            // 更新技能加成
            PlayerSkill.CatchBonus = 1.0f + (PlayerSkill.Level * 0.02f);
            PlayerSkill.LuckBonus = 1.0f + (PlayerSkill.Level * 0.015f);
            PlayerSkill.SpeedBonus = 1.0f + (PlayerSkill.Level * 0.01f);
            
            LevelUp?.Invoke(PlayerSkill.Level);
        }
        
        SaveSkillData();
    }
    
    private int CalculateExpToNextLevel(int level)
    {
        return (int)(100 * Mathf.Pow(1.1f, level - 1));
    }
    
    /// <summary>
    /// 购买鱼竿
    /// </summary>
    public bool BuyRod(string rodId)
    {
        var rod = FishingDatabase.Instance.GetFishingRod(rodId);
        if (rod == null) return false;
        
        if (PlayerSkill.Level < rod.RequiredLevel) return false;
        
        if (GameStats.Instance.Gold >= rod.Price)
        {
            GameStats.Instance.Gold -= rod.Price;
            ChangeRod(rodId);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取钓鱼统计
    /// </summary>
    public Dictionary<string, int> GetFishingStats()
    {
        return new Dictionary<string, int>
        {
            { "TotalCatches", _totalCatches },
            { "TotalFish", _totalFish },
            { "TotalExp", PlayerSkill.Experience },
            { "CurrentLevel", PlayerSkill.Level },
            { "RarestFish", _rarestFishCaught }
        };
    }
    
    private int _totalCatches;
    private int _totalFish;
    private int _rarestFishCaught;
    
    public void RecordCatch(ItemRarity rarity)
    {
        _totalCatches++;
        _rarestFishCaught = Math.Max(_rarestFishCaught, (int)rarity);
    }
    
    // 存档支持
    private const string SaveKeyFishing = "fishing_system";
    
    public Dictionary<string, object> Save()
    {
        return new Dictionary<string, object>
        {
            { "current_rod", CurrentRodId },
            { "rod_durability", CurrentRodDurability },
            { "skill_level", PlayerSkill.Level },
            { "skill_exp", PlayerSkill.Experience },
            { "total_catches", _totalCatches },
            { "total_fish", _totalFish },
            { "rarest_fish", _rarestFishCaught }
        };
    }
    
    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.TryGetValue("current_rod", out var rodId))
            CurrentRodId = rodId.ToString();
        
        if (data.TryGetValue("rod_durability", out var durability))
            CurrentRodDurability = (int)durability;
        
        if (data.TryGetValue("skill_level", out var level))
            PlayerSkill.Level = (int)level;
        
        if (data.TryGetValue("skill_exp", out var exp))
            PlayerSkill.Experience = (int)exp;
        
        if (data.TryGetValue("total_catches", out var catches))
            _totalCatches = (int)catches;
        
        if (data.TryGetValue("total_fish", out var fish))
            _totalFish = (int)fish;
        
        if (data.TryGetValue("rarest_fish", out var rarest))
            _rarestFishCaught = (int)rarest;
        
        // 重新计算技能加成
        PlayerSkill.CatchBonus = 1.0f + (PlayerSkill.Level * 0.02f);
        PlayerSkill.LuckBonus = 1.0f + (PlayerSkill.Level * 0.015f);
        PlayerSkill.SpeedBonus = 1.0f + (PlayerSkill.Level * 0.01f);
        PlayerSkill.ExperienceToNextLevel = CalculateExpToNextLevel(PlayerSkill.Level);
    }

    /// <summary>
    /// 导出保存数据 - 实现 BaseSystem 持久化接口
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary
        {
            { "currentRodId", CurrentRodId },
            { "currentRodDurability", CurrentRodDurability },
            { "skillLevel", PlayerSkill.Level },
            { "skillExperience", PlayerSkill.Experience },
            { "skillExpToNextLevel", PlayerSkill.ExperienceToNextLevel },
            { "totalCatches", _totalCatches },
            { "totalFish", _totalFish },
            { "rarestFishCaught", _rarestFishCaught }
        };
    }

    /// <summary>
    /// 导入保存数据 - 实现 BaseSystem 持久化接口
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;

        if (data.Contains("currentRodId"))
            CurrentRodId = data["currentRodId"].ToString();

        if (data.Contains("currentRodDurability"))
            CurrentRodDurability = Convert.ToInt32(data["currentRodDurability"]);

        if (data.Contains("skillLevel"))
            PlayerSkill.Level = Convert.ToInt32(data["skillLevel"]);

        if (data.Contains("skillExperience"))
            PlayerSkill.Experience = Convert.ToInt32(data["skillExperience"]);

        if (data.Contains("skillExpToNextLevel"))
            PlayerSkill.ExperienceToNextLevel = Convert.ToInt32(data["skillExpToNextLevel"]);

        if (data.Contains("totalCatches"))
            _totalCatches = Convert.ToInt32(data["totalCatches"]);

        if (data.Contains("totalFish"))
            _totalFish = Convert.ToInt32(data["totalFish"]);

        if (data.Contains("rarestFishCaught"))
            _rarestFishCaught = Convert.ToInt32(data["rarestFishCaught"]);

        // 重新计算技能加成
        PlayerSkill.CatchBonus = 1.0f + (PlayerSkill.Level * 0.02f);
        PlayerSkill.LuckBonus = 1.0f + (PlayerSkill.Level * 0.015f);
        PlayerSkill.SpeedBonus = 1.0f + (PlayerSkill.Level * 0.01f);
    }

    private void SaveSkillData()
    {
        // 保存到玩家数据
    }
    
    private void LoadSkillData()
    {
        // 从玩家数据加载
    }
}
