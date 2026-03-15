using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Database;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 宠物探险系统 - 宠物离线探险获取奖励
    /// </summary>
    public class PetExpeditionSystem : BaseSystem
    {
        public static PetExpeditionSystem Instance { get; private set; }
        
        private PetExpeditionData.PlayerExpeditionData _playerData;
        private Random _random = new Random();
        private List<ActiveExpedition> _activeExpeditions = new List<ActiveExpedition>();
        
        // 信号系统
        public static SignalExpeditionStarted OnExpeditionStarted { get; } = new SignalExpeditionStarted();
        public static SignalExpeditionCompleted OnExpeditionCompleted { get; } = new SignalExpeditionCompleted();
        public static SignalExpeditionFailed OnExpeditionFailed { get; } = new SignalExpeditionFailed();
        
        public class SignalExpeditionStarted : Godot.Signal {}
        public class SignalExpeditionCompleted : Godot.Signal {}
        public class SignalExpeditionFailed : Godot.Signal {}
        
        public override void _Ready()
        {
            Instance = this;
            _playerData = new PetExpeditionData.PlayerExpeditionData();
            LoadData();
        }
        
        public override void _Process(float delta)
        {
            CheckExpeditions();
        }
        
        /// <summary>
        /// 检查探险完成状态
        /// </summary>
        private void CheckExpeditions()
        {
            var now = DateTime.Now;
            var toComplete = _activeExpeditions
                .Where(e => !e.Completed && now >= e.StartTime.AddMinutes(e.DurationMinutes))
                .ToList();
            
            foreach (var expedition in toComplete)
            {
                CompleteExpedition(expedition);
            }
        }
        
        /// <summary>
        /// 完成探险
        /// </summary>
        private void CompleteExpedition(ActiveExpedition expedition)
        {
            expedition.Completed = true;
            
            var config = PetExpeditionDatabase.Expeditions[expedition.Type];
            
            // 计算成功率
            bool success = _random.NextDouble() < config.SuccessRate;
            
            if (success)
            {
                expedition.Success = true;
                expedition.GoldReward = _random.Next(config.GoldReward[0], config.GoldReward[1] + 1);
                expedition.ExpReward = _random.Next(config.ExpReward[0], config.ExpReward[1] + 1);
                
                // 随机物品
                int rarityIndex = GetRarityIndex(config.RarityWeights);
                expedition.ItemReward = config.ItemPool[_random.Next(config.ItemPool.Length)] + 
                    $" ({PetExpeditionDatabase.Rarities[rarityIndex]})";
                
                // 更新玩家数据
                _playerData.SuccessfulExpeditions++;
                _playerData.GoldEarned += expedition.GoldReward;
                _playerData.ExperienceGained += expedition.ExpReward;
                _playerData.ItemsEarned.Add(expedition.ItemReward);
                
                if (rarityIndex + 1 > _playerData.HighestRarityFound)
                {
                    _playerData.HighestRarityFound = rarityIndex + 1;
                }
                
                // 发送信号
                OnExpeditionCompleted.Emit();
                
                GD.Print($"[PetExpedition] Expedition completed! Pet: {expedition.PetName}, Gold: {expedition.GoldReward}, Item: {expedition.ItemReward}");
            }
            else
            {
                expedition.Success = false;
                _playerData.FailedExpeditions++;
                
                OnExpeditionFailed.Emit();
                
                GD.Print($"[PetExpedition] Expedition failed! Pet: {expedition.PetName}");
            }
            
            _playerData.TotalExpeditions++;
            SaveData();
        }
        
        /// <summary>
        /// 根据权重获取稀有度索引
        /// </summary>
        private int GetRarityIndex(float[] weights)
        {
            float total = weights.Sum();
            float roll = (float)_random.NextDouble() * total;
            
            float cumulative = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                {
                    return i;
                }
            }
            
            return weights.Length - 1;
        }
        
        /// <summary>
        /// 开始探险
        /// </summary>
        public bool StartExpedition(string petId, string petName, ExpeditionType type)
        {
            var config = PetExpeditionDatabase.Expeditions[type];
            
            // 检查是否有相同的宠物正在探险
            if (_activeExpeditions.Any(e => e.PetId == petId && !e.Completed))
            {
                GD.Print($"[PetExpedition] Pet {petName} is already on an expedition!");
                return false;
            }
            
            // 检查等级要求
            // Note: 这里需要从宠物系统获取等级，暂时简化处理
            
            var expedition = new ActiveExpedition
            {
                PetId = petId,
                PetName = petName,
                Type = type,
                StartTime = DateTime.Now,
                DurationMinutes = config.DurationMinutes,
                Completed = false
            };
            
            _activeExpeditions.Add(expedition);
            
            OnExpeditionStarted.Emit();
            
            GD.Print($"[PetExpedition] Started expedition: {petName} to {config.Name} for {config.DurationMinutes} minutes");
            
            return true;
        }
        
        /// <summary>
        /// 取消探险
        /// </summary>
        public bool CancelExpedition(string petId)
        {
            var expedition = _activeExpeditions.FirstOrDefault(e => e.PetId == petId && !e.Completed);
            
            if (expedition == null)
            {
                return false;
            }
            
            expedition.Completed = true;
            expedition.Success = false;
            _playerData.FailedExpeditions++;
            _playerData.TotalExpeditions++;
            
            SaveData();
            
            GD.Print($"[PetExpedition] Cancelled expedition for pet {petId}");
            
            return true;
        }
        
        /// <summary>
        /// 获取活跃探险列表
        /// </summary>
        public List<ActiveExpedition> GetActiveExpeditions()
        {
            return _activeExpeditions.Where(e => !e.Completed).ToList();
        }
        
        /// <summary>
        /// 获取探险历史
        /// </summary>
        public List<ActiveExpedition> GetExpeditionHistory()
        {
            return _activeExpeditions.Where(e => e.Completed).OrderByDescending(e => e.StartTime).Take(50).ToList();
        }
        
        /// <summary>
        /// 获取玩家统计
        /// </summary>
        public PetExpeditionData.PlayerExpeditionData GetPlayerStats()
        {
            return _playerData;
        }
        
        /// <summary>
        /// 获取探险详情
        /// </summary>
        public ActiveExpedition GetExpedition(string petId)
        {
            return _activeExpeditions.FirstOrDefault(e => e.PetId == petId && !e.Completed);
        }
        
        /// <summary>
        /// 获取剩余时间（分钟）
        /// </summary>
        public int GetRemainingMinutes(string petId)
        {
            var expedition = GetExpedition(petId);
            if (expedition == null) return 0;
            
            var remaining = expedition.StartTime.AddMinutes(expedition.DurationMinutes) - DateTime.Now;
            return Math.Max(0, (int)remaining.TotalMinutes);
        }
        
        /// <summary>
        /// 获取进度（0-1）
        /// </summary>
        public float GetProgress(string petId)
        {
            var expedition = GetExpedition(petId);
            if (expedition == null) return 0;
            
            var elapsed = DateTime.Now - expedition.StartTime;
            var total = TimeSpan.FromMinutes(expedition.DurationMinutes);
            
            return Math.Min(1f, (float)(elapsed.TotalSeconds / total.TotalSeconds));
        }
        
        /// <summary>
        /// 收集探险奖励
        /// </summary>
        public Dictionary<string, object> CollectReward(string petId)
        {
            var expedition = _activeExpeditions.FirstOrDefault(e => e.PetId == petId && e.Completed && !CollectedRewards.Contains(petId));
            
            if (expedition == null || !expedition.Success)
            {
                return null;
            }
            
            CollectedRewards.Add(petId);
            
            var reward = new Dictionary<string, object>
            {
                { "gold", expedition.GoldReward },
                { "experience", expedition.ExpReward },
                { "item", expedition.ItemReward }
            };
            
            // Note: 实际添加金币和经验需要调用经济系统
            
            return reward;
        }
        
        private HashSet<string> CollectedRewards = new HashSet<string>();
        
        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            // Note: 从存档加载数据
            // 简化处理：使用内存数据
        }
        
        /// <summary>
        /// 保存数据
        /// </summary>
        private void SaveData()
        {
            // Note: 保存到存档
            // 简化处理：使用内存数据
        }
        
        /// <summary>
        /// 重置数据（测试用）
        /// </summary>
        public void ResetData()
        {
            _playerData = new PetExpeditionData.PlayerExpeditionData();
            _activeExpeditions.Clear();
            CollectedRewards.Clear();
            SaveData();
        }
    }

        public override Dictionary ExportSaveData() => new();
        public override void ImportSaveData(Dictionary data) { }
}
