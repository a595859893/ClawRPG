using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Scripts.Fishing;
using Framework;

namespace ClawRPG.Scripts.Fishing
{
    /// <summary>
    /// 钓鱼系统核心逻辑
    /// </summary>
    public class FishingSystem : BaseSystem
    {
        // 单例
        private static FishingSystem _instance;
        public static FishingSystem Instance => _instance;
        
        // 玩家数据
        private PlayerFishingData _playerData = new PlayerFishingData();
        
        // 当前会话
        private FishingSession _currentSession = null;
        
        // 定时器
        private float _waitingTimer = 0f;
        private float _reelingTimer = 0f;
        
        // 信号
        public static readonly StringName FishingStartedSignal = "fishing_started";
        public static readonly StringName FishBitingSignal = "fish_biting";
        public static readonly StringName FishCaughtSignal = "fish_caught";
        public static readonly StringName FishEscapedSignal = "fish_escaped";
        public static readonly StringName LevelUpSignal = "level_up";
        
        public override void _Ready()
        {
            _instance = this;
            InitializeData();
        }
        
        private void InitializeData()
        {
            _playerData = new PlayerFishingData
            {
                TotalCatches = 0,
                TotalAttempts = 0,
                TotalValue = 0,
                TotalExperience = 0,
                CurrentLevel = 1,
                CurrentXP = 0,
                FishCaught = new Dictionary<string, int>(),
                LocationCatches = new Dictionary<FishingLocationType, int>(),
                RecentCatches = new List<FishingRecord>(),
                WeightRecords = new Dictionary<string, int>(),
                EquippedRod = RodType.Bamboo,
                PreferredBait = BaitType.Worm,
                UnlockedFish = new Dictionary<string, bool>(),
                BiggestCatchWeight = 0,
                BiggestCatchFish = "",
                PerfectCatches = 0
            };
            
            foreach (var loc in Enum.GetValues(typeof(FishingLocationType)))
            {
                _playerData.LocationCatches[(FishingLocationType)loc] = 0;
            }
            
            foreach (var fish in FishingDatabase.Fish.Keys)
            {
                _playerData.UnlockedFish[fish] = false;
                _playerData.FishCaught[fish] = 0;
                _playerData.WeightRecords[fish] = 0;
            }
        }
        
        #region 钓鱼操作
        
        /// <summary>
        /// 开始钓鱼
        /// </summary>
        public bool StartFishing(FishingLocationType location, RodType rod, BaitType bait)
        {
            if (_currentSession != null && _currentSession.CurrentState != FishingState.Idle)
            {
                GD.Print("当前正在进行钓鱼");
                return false;
            }
            
            _currentSession = new FishingSession
            {
                StartTime = DateTime.Now,
                Location = location,
                Rod = rod,
                Bait = bait,
                TotalAttempts = 0,
                SuccessfulCatches = 0,
                TotalValue = 0,
                TotalExperience = 0,
                Records = new List<FishingRecord>(),
                CurrentState = FishingState.Casting
            };
            
            // 触发抛竿动画/逻辑
            _currentSession.CurrentState = FishingState.Waiting;
            _waitingTimer = GetRandomWaitTime();
            
            EmitSignal(FishingStartedSignal, location.ToString());
            GD.Print($"开始钓鱼 - 地点: {location}, 鱼竿: {rod}, 鱼饵: {bait}");
            return true;
        }
        
        /// <summary>
        /// 提竿（咬钩时）
        /// </summary>
        public bool ReelIn()
        {
            if (_currentSession == null || _currentSession.CurrentState != FishingState.Biting)
            {
                return false;
            }
            
            // 计算成功几率
            float successChance = CalculateCatchSuccess();
            
            if (GD.Randf() < successChance)
            {
                // 钓到了！
                CompleteCatch(true);
                return true;
            }
            else
            {
                // 逃脱了
                CompleteCatch(false);
                return false;
            }
        }
        
        /// <summary>
        /// 取消钓鱼
        /// </summary>
        public void CancelFishing()
        {
            if (_currentSession == null) return;
            
            GD.Print($"钓鱼取消 - 尝试: {_currentSession.TotalAttempts}, 钓获: {_currentSession.SuccessfulCatches}");
            _currentSession = null;
        }
        
        #endregion
        
        #region 游戏循环
        
        public override void _Process(double delta)
        {
            if (_currentSession == null) return;
            
            switch (_currentSession.CurrentState)
            {
                case FishingState.Waiting:
                    ProcessWaiting(delta);
                    break;
                case FishingState.Reeling:
                    ProcessReeling(delta);
                    break;
            }
        }
        
        private void ProcessWaiting(float delta)
        {
            _waitingTimer -= delta;
            if (_waitingTimer <= 0)
            {
                // 鱼咬钩了
                _currentSession.CurrentState = FishingState.Biting;
                _reelingTimer = GetReelingTime();
                
                // 生成要咬钩的鱼
                _currentSession.CurrentFish = GenerateBitingFish();
                
                EmitSignal(FishBitingSignal, _currentSession.CurrentFish.Name);
                GD.Print($"鱼咬钩了！- {_currentSession.CurrentFish.Name}");
            }
        }
        
        private void ProcessReeling(float delta)
        {
            _reelingTimer -= delta;
            _currentSession.CurrentProgress = 1.0f - (_reelingTimer / GetReelingTime());
            
            if (_reelingTimer <= 0)
            {
                // 超时，鱼跑了
                CompleteCatch(false);
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        private float GetRandomWaitTime()
        {
            // 基础等待时间 3-8 秒
            float baseTime = GD.Randf() * 5.0f + 3.0f;
            
            // 鱼饵影响
            var baitConfig = FishingDatabase.Baits[_currentSession.Bait];
            
            // 环境影响
            var envState = GetCurrentEnvironment();
            baseTime *= (1.0f - envState.CatchBonus * 0.3f);
            
            return Mathf.Max(1.0f, baseTime);
        }
        
        private float GetReelingTime()
        {
            // 基础收线时间 2-4 秒
            float baseTime = GD.Randf() * 2.0f + 2.0f;
            
            // 鱼竿速度影响
            var rodConfig = FishingDatabase.Rods[_currentSession.Rod];
            baseTime /= rodConfig.ReelSpeed;
            
            return Mathf.Max(0.5f, baseTime);
        }
        
        private FishData GenerateBitingFish()
        {
            var location = _currentSession.Location;
            var locationConfig = FishingDatabase.Locations[location];
            
            // 根据概率决定是普通鱼还是稀有鱼
            float rareChance = 0.3f;
            var rodConfig = FishingDatabase.Rods[_currentSession.Rod];
            rareChance += rodConfig.RareBonus;
            
            var envState = GetCurrentEnvironment();
            rareChance += envState.RareBonus;
            
            FishType chosenType;
            if (GD.Randf() < rareChance)
            {
                chosenType = locationConfig.RareFishTypes[GD.Randi() % locationConfig.RareFishTypes.Count];
            }
            else
            {
                chosenType = locationConfig.BaseFishTypes[GD.Randi() % locationConfig.BaseFishTypes.Count];
            }
            
            // 筛选该地点该类型的鱼
            var candidates = FishingDatabase.Fish.Values
                .Where(f => f.Locations.Contains(location) && f.Rarity == chosenType)
                .ToList();
            
            if (candidates.Count == 0)
            {
                candidates = FishingDatabase.Fish.Values
                    .Where(f => f.Locations.Contains(location))
                    .ToList();
            }
            
            return candidates[GD.Randi() % candidates.Count];
        }
        
        private float CalculateCatchSuccess()
        {
            float baseChance = 0.5f;
            
            // 鱼竿加成
            var rodConfig = FishingDatabase.Rods[_currentSession.Rod];
            baseChance += rodConfig.CatchBonus;
            
            // 鱼饵加成
            var baitConfig = FishingDatabase.Baits[_currentSession.Bait];
            baseChance += (baitConfig.Attractiveness - 1.0f) * 0.2f;
            
            // 环境加成
            var envState = GetCurrentEnvironment();
            baseChance += envState.CatchBonus;
            
            // 难度调整
            var locationConfig = FishingDatabase.Locations[_currentSession.Location];
            baseChance /= locationConfig.Difficulty;
            
            // 当前鱼的稀有度影响
            if (_currentSession.CurrentFish != null)
            {
                float rarityPenalty = (float)_currentSession.CurrentFish.Rarity * 0.1f;
                baseChance -= rarityPenalty;
            }
            
            return Mathf.Clamp(baseChance, 0.1f, 0.95f);
        }
        
        private void CompleteCatch(bool success)
        {
            _currentSession.TotalAttempts++;
            _playerData.TotalAttempts++;
            
            if (success && _currentSession.CurrentFish != null)
            {
                var fish = _currentSession.CurrentFish;
                
                // 计算重量
                int weight = GD.Randi() % (fish.MaxWeight - fish.MinWeight + 1) + fish.MinWeight;
                
                // 计算价值和经验
                int value = fish.BaseValue;
                int xp = fish.ExperienceReward;
                
                // 稀有度加成
                float rarityMultiplier = 1.0f + ((int)fish.Rarity * 0.5f);
                value = (int)(value * rarityMultiplier);
                xp = (int)(xp * rarityMultiplier);
                
                // 重量加成
                float weightBonus = (float)weight / fish.MaxWeight;
                value = (int)(value * (1.0f + weightBonus));
                
                // 记录
                var record = new FishingRecord
                {
                    FishID = fish.ID,
                    CaughtAt = DateTime.Now,
                    Location = _currentSession.Location.ToString(),
                    Weight = weight,
                    IsNewRecord = weight > (_playerData.WeightRecords.ContainsKey(fish.ID) ? _playerData.WeightRecords[fish.ID] : 0),
                    AttemptNumber = _currentSession.TotalAttempts
                };
                
                if (record.IsNewRecord)
                {
                    _playerData.WeightRecords[fish.ID] = weight;
                    xp = (int)(xp * 1.5f); // 新纪录额外经验
                }
                
                // 更新统计
                _currentSession.Records.Add(record);
                _currentSession.SuccessfulCatches++;
                _currentSession.TotalValue += value;
                _currentSession.TotalExperience += xp;
                
                _playerData.TotalCatches++;
                _playerData.TotalValue += value;
                _playerData.TotalExperience += xp;
                
                if (!_playerData.FishCaught.ContainsKey(fish.ID))
                    _playerData.FishCaught[fish.ID] = 0;
                _playerData.FishCaught[fish.ID]++;
                
                if (!_playerData.LocationCatches.ContainsKey(_currentSession.Location))
                    _playerData.LocationCatches[_currentSession.Location] = 0;
                _playerData.LocationCatches[_currentSession.Location]++;
                
                // 更新最近钓获
                _playerData.RecentCatches.Add(record);
                if (_playerData.RecentCatches.Count > 50)
                    _playerData.RecentCatches.RemoveAt(0);
                
                // 解锁鱼类
                _playerData.UnlockedFish[fish.ID] = true;
                
                // 更新最大钓获
                if (weight > _playerData.BiggestCatchWeight)
                {
                    _playerData.BiggestCatchWeight = weight;
                    _playerData.BiggestCatchFish = fish.Name;
                }
                
                // 完美钓获判断
                if (weight >= fish.MaxWeight * 0.9f)
                {
                    _playerData.PerfectCatches++;
                    xp = (int)(xp * 1.2f);
                }
                
                // 升级检测
                int oldLevel = _playerData.CurrentLevel;
                _playerData.CurrentLevel = FishingDatabase.GetLevelForXP(_playerData.CurrentXP + xp);
                _playerData.CurrentXP += xp;
                
                if (_playerData.CurrentLevel > oldLevel)
                {
                    EmitSignal(LevelUpSignal, _playerData.CurrentLevel);
                    GD.Print($"钓鱼等级提升到 {_playerData.CurrentLevel}！");
                }
                
                // 稀有度统计
                if (!_playerData.RarityCatches.ContainsKey(fish.Rarity))
                    _playerData.RarityCatches[fish.Rarity] = 0;
                _playerData.RarityCatches[fish.Rarity]++;
                
                EmitSignal(FishCaughtSignal, fish.Name, value, weight);
                GD.Print($"钓到了！- {fish.Name}, 重量: {weight}g, 价值: {value}, 经验: {xp}");
            }
            else
            {
                EmitSignal(FishEscapedSignal);
                GD.Print("鱼逃脱了...");
            }
            
            // 重置状态
            _currentSession.CurrentState = FishingState.Idle;
            _currentSession.CurrentFish = null;
            
            // 可以选择继续钓鱼或结束
            // 这里自动开始下一轮
            _currentSession.CurrentState = FishingState.Waiting;
            _waitingTimer = GetRandomWaitTime();
        }
        
        private FishingEnvironmentState GetCurrentEnvironment()
        {
            // 这里可以集成天气系统或时间系统
            // 简化版本返回默认状态
            var state = new FishingEnvironmentState
            {
                Effect = FishingEnvironmentEffect.None,
                CatchBonus = 0.0f,
                RareBonus = 0.0f,
                ValueBonus = 0.0f,
                ActiveEvent = ""
            };
            
            // 检查时间
            var hour = DateTime.Now.Hour;
            if (hour >= 18 || hour < 6)
            {
                state.Effect = FishingEnvironmentEffect.Night;
                state.CatchBonus = 0.1f;
                state.RareBonus = 0.1f;
            }
            
            return state;
        }
        
        #endregion
        
        #region 数据访问
        
        public PlayerFishingData GetPlayerData() => _playerData;
        
        public FishingSession GetCurrentSession() => _currentSession;
        
        public FishingStatistics GetStatistics()
        {
            var stats = new FishingStatistics
            {
                TotalPlayTime = 0, // 需要计时器
                TotalCatches = _playerData.TotalCatches,
                TotalAttempts = _playerData.TotalAttempts,
                SuccessRate = _playerData.TotalAttempts > 0 ? (float)_playerData.TotalCatches / _playerData.TotalAttempts : 0,
                TotalValue = _playerData.TotalValue,
                TotalExperience = _playerData.TotalExperience,
                LongestStreak = 0,
                CurrentStreak = 0,
                RarityCatches = new Dictionary<FishType, int>(),
                LocationStats = new Dictionary<FishingLocationType, int>(),
                UniqueSpecies = _playerData.UnlockedFish.Values.Count(v => v),
                RarestFishCaught = ""
            };
            
            // 找出最稀有的鱼
            foreach (var fish in _playerData.FishCaught)
            {
                if (fish.Value > 0)
                {
                    var fishData = FishingDatabase.Fish[fish.Key];
                    if (stats.RarestFishCaught == "" || fishData.Rarity > FishingDatabase.Fish[stats.RarestFishCaught].Rarity)
                    {
                        stats.RarestFishCaught = fish.Key;
                    }
                }
            }
            
            foreach (var kvp in _playerData.RarityCatches)
            {
                stats.RarityCatches[kvp.Key] = kvp.Value;
            }
            
            foreach (var kvp in _playerData.LocationCatches)
            {
                stats.LocationStats[kvp.Key] = kvp.Value;
            }
            
            return stats;
        }
        
        public int GetUnlockedFishCount()
        {
            return _playerData.UnlockedFish.Values.Count(v => v);
        }
        
        public int GetTotalFishCount()
        {
            return FishingDatabase.Fish.Count;
        }
        
        #endregion
        
        #region 存档支持
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>
            {
                ["total_catches"] = _playerData.TotalCatches,
                ["total_attempts"] = _playerData.TotalAttempts,
                ["total_value"] = _playerData.TotalValue,
                ["total_experience"] = _playerData.TotalExperience,
                ["current_level"] = _playerData.CurrentLevel,
                ["current_xp"] = _playerData.CurrentXP,
                ["equipped_rod"] = (int)_playerData.EquippedRod,
                ["preferred_bait"] = (int)_playerData.PreferredBait,
                ["biggest_catch_weight"] = _playerData.BiggestCatchWeight,
                ["biggest_catch_fish"] = _playerData.BiggestCatchFish,
                ["perfect_catches"] = _playerData.PerfectCatches,
                ["fish_caught"] = _playerData.FishCaught,
                ["unlocked_fish"] = _playerData.UnlockedFish,
                ["weight_records"] = _playerData.WeightRecords,
                ["recent_catches"] = new List<Dictionary<string, object>>()
            };
            
            foreach (var record in _playerData.RecentCatches)
            {
                data["recent_catches"].Add(new Dictionary<string, object>
                {
                    ["fish_id"] = record.FishID,
                    ["caught_at"] = record.CaughtAt.ToString("o"),
                    ["location"] = record.Location,
                    ["weight"] = record.Weight,
                    ["is_new_record"] = record.IsNewRecord
                });
            }
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _playerData.TotalCatches = data.ContainsKey("total_catches") ? Convert.ToInt32(data["total_catches"]) : 0;
            _playerData.TotalAttempts = data.ContainsKey("total_attempts") ? Convert.ToInt32(data["total_attempts"]) : 0;
            _playerData.TotalValue = data.ContainsKey("total_value") ? Convert.ToInt32(data["total_value"]) : 0;
            _playerData.TotalExperience = data.ContainsKey("total_experience") ? Convert.ToInt32(data["total_experience"]) : 0;
            _playerData.CurrentLevel = data.ContainsKey("current_level") ? Convert.ToInt32(data["current_level"]) : 1;
            _playerData.CurrentXP = data.ContainsKey("current_xp") ? Convert.ToInt32(data["current_xp"]) : 0;
            _playerData.EquippedRod = data.ContainsKey("equipped_rod") ? (RodType)Convert.ToInt32(data["equipped_rod"]) : RodType.Bamboo;
            _playerData.PreferredBait = data.ContainsKey("preferred_bait") ? (BaitType)Convert.ToInt32(data["preferred_bait"]) : BaitType.Worm;
            _playerData.BiggestCatchWeight = data.ContainsKey("biggest_catch_weight") ? Convert.ToInt32(data["biggest_catch_weight"]) : 0;
            _playerData.BiggestCatchFish = data.ContainsKey("biggest_catch_fish") ? data["biggest_catch_fish"].ToString() : "";
            _playerData.PerfectCatches = data.ContainsKey("perfect_catches") ? Convert.ToInt32(data["perfect_catches"]) : 0;
            
            if (data.ContainsKey("fish_caught"))
            {
                var dict = data["fish_caught"] as Dictionary<string, object>;
                if (dict != null)
                {
                    _playerData.FishCaught = new Dictionary<string, int>();
                    foreach (var kvp in dict)
                    {
                        _playerData.FishCaught[kvp.Key] = Convert.ToInt32(kvp.Value);
                    }
                }
            }
            
            if (data.ContainsKey("unlocked_fish"))
            {
                var dict = data["unlocked_fish"] as Dictionary<string, object>;
                if (dict != null)
                {
                    _playerData.UnlockedFish = new Dictionary<string, bool>();
                    foreach (var kvp in dict)
                    {
                        _playerData.UnlockedFish[kvp.Key] = Convert.ToBoolean(kvp.Value);
                    }
                }
            }
            
            if (data.ContainsKey("weight_records"))
            {
                var dict = data["weight_records"] as Dictionary<string, object>;
                if (dict != null)
                {
                    _playerData.WeightRecords = new Dictionary<string, int>();
                    foreach (var kvp in dict)
                    {
                        _playerData.WeightRecords[kvp.Key] = Convert.ToInt32(kvp.Value);
                    }
                }
            }
        }
        
        #endregion
    }
}
