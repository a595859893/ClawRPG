using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Database;
using ClawRPG.Systems;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 坐骑远征系统 - 坐骑离线探险获取奖励
    /// </summary>
    public class MountExpeditionSystem : Node
    {
        public static MountExpeditionSystem Instance { get; private set; }
        
        private MountExpeditionData.PlayerExpeditionData _playerData;
        private Random _random = new Random();
        
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
            _playerData = new MountExpeditionData.PlayerExpeditionData();
        }
        
        public override void _Process(float delta)
        {
            CheckExpeditions();
        }
        
        /// <summary>
        /// 检查远征完成状态
        /// </summary>
        private void CheckExpeditions()
        {
            var completedExpeditions = _playerData.ActiveExpeditions
                .Where(e => !e.Completed && DateTime.Now >= e.StartTime.AddMinutes(e.DurationMinutes))
                .ToList();
            
            foreach (var expedition in completedExpeditions)
            {
                expedition.Completed = true;
                ProcessExpeditionResult(expedition);
            }
        }
        
        /// <summary>
        /// 处理远征结果
        /// </summary>
        private void ProcessExpeditionResult(MountExpeditionData.ActiveExpedition expedition)
        {
            var zone = MountExpeditionDatabase.GetZone(expedition.ZoneId);
            if (zone == null) return;
            
            // 计算成功率
            float successRate = CalculateSuccessRate(expedition.MountId, zone);
            bool success = _random.NextDouble() < successRate;
            
            var result = new MountExpeditionData.ExpeditionResult
            {
                ZoneId = expedition.ZoneId,
                Success = success,
                MountId = expedition.MountId,
                CompletedAt = DateTime.Now
            };
            
            if (success)
            {
                // 成功奖励
                result.GoldReward = _random.Next(zone.MinGoldReward, zone.MaxGoldReward + 1);
                result.ExpReward = _random.Next(zone.MinExpReward, zone.MaxExpReward + 1);
                
                // 物品掉落
                foreach (var itemId in zone.ItemRewards)
                {
                    if (_random.NextDouble() < 0.3f)
                    {
                        result.ItemRewards.Add(itemId);
                    }
                }
                
                OnExpeditionCompleted.Emit();
            }
            else
            {
                // 失败安慰奖
                result.GoldReward = zone.MinGoldReward / 5;
                result.ExpReward = zone.MinExpReward / 5;
                
                OnExpeditionFailed.Emit();
            }
            
            // 更新统计
            _playerData.History.Insert(0, result);
            _playerData.TotalExpeditions++;
            _playerData.TotalGoldEarned += result.GoldReward;
            _playerData.TotalExpEarned += result.ExpReward;
            
            if (_playerData.ZoneCompletions.ContainsKey(expedition.ZoneId))
                _playerData.ZoneCompletions[expedition.ZoneId]++;
            else
                _playerData.ZoneCompletions[expedition.ZoneId] = 1;
            
            // 发放奖励
            GrantReward(result);
        }
        
        /// <summary>
        /// 计算远征成功率
        /// </summary>
        private float CalculateSuccessRate(string mountId, MountExpeditionData.ExpeditionZone zone)
        {
            var mount = GetMountById(mountId);
            if (mount == null) return zone.BaseSuccessRate;
            
            // 基于坐骑等级和区域推荐等级计算
            int mountLevel = mount.Get("level", 1);
            int levelDiff = mountLevel - zone.RecommendedLevel;
            
            float successRate = zone.BaseSuccessRate;
            
            // 每高于推荐等级1级，成功率+2%
            if (levelDiff > 0)
                successRate += levelDiff * 0.02f;
            // 每低于推荐等级1级，成功率-5%
            else if (levelDiff < 0)
                successRate += levelDiff * 0.05f;
            
            return Mathf.Clamp(successRate, 0.1f, 0.95f);
        }
        
        /// <summary>
        /// 获取坐骑数据（模拟）
        /// </summary>
        private Godot.Collections.Dictionary GetMountById(string mountId)
        {
            // 这里应该从 MountManager 获取坐骑数据
            // 暂时返回默认数据
            return new Godot.Collections.Dictionary { { "level", 1 } };
        }
        
        /// <summary>
        /// 发放奖励
        /// </summary>
        private void GrantReward(MountExpeditionData.ExpeditionResult result)
        {
            var player = GetTree().CurrentScene?.GetNode<Player>("Player");
            if (player != null)
            {
                player.AddGold(result.GoldReward);
                player.AddExperience(result.ExpReward);
            }
            
            // 添加物品到背包
            foreach (var itemId in result.ItemRewards)
            {
                InventoryManager.Instance?.AddItem(itemId, 1);
            }
        }
        
        /// <summary>
        /// 开始远征
        /// </summary>
        public bool StartExpedition(string zoneId, string mountId)
        {
            var zone = MountExpeditionDatabase.GetZone(zoneId);
            if (zone == null)
            {
                GD.PrintErr($"[MountExpedition] Invalid zone: {zoneId}");
                return false;
            }
            
            // 检查是否有空闲坐骑
            var mount = GetMountById(mountId);
            if (mount == null)
            {
                GD.PrintErr($"[MountExpedition] Mount not found: {mountId}");
                return false;
            }
            
            // 检查是否已有远征进行
            if (_playerData.ActiveExpeditions.Count >= 3)
            {
                GD.PrintErr("[MountExpedition] Max active expeditions reached");
                return false;
            }
            
            // 创建新远征
            var expedition = new MountExpeditionData.ActiveExpedition
            {
                ExpeditionId = Guid.NewGuid().ToString(),
                ZoneId = zoneId,
                MountId = mountId,
                StartTime = DateTime.Now,
                DurationMinutes = zone.DurationMinutes,
                Completed = false,
                Claimed = false
            };
            
            _playerData.ActiveExpeditions.Add(expedition);
            OnExpeditionStarted.Emit();
            
            GD.Print($"[MountExpedition] Started expedition: {zone.Name} for {mountId}");
            return true;
        }
        
        /// <summary>
        /// 取消远征
        /// </summary>
        public bool CancelExpedition(string expeditionId)
        {
            var expedition = _playerData.ActiveExpeditions
                .FirstOrDefault(e => e.ExpeditionId == expeditionId);
            
            if (expedition == null || expedition.Completed)
                return false;
            
            var zone = MountExpeditionDatabase.GetZone(expedition.ZoneId);
            int refund = zone != null ? zone.MinGoldReward / 10 : 10;
            
            // 返还部分金币
            var player = GetTree().CurrentScene?.GetNode<Player>("Player");
            if (player != null)
                player.AddGold(refund);
            
            _playerData.ActiveExpeditions.Remove(expedition);
            GD.Print($"[MountExpedition] Cancelled expedition: {expeditionId}, refund: {refund}");
            return true;
        }
        
        /// <summary>
        /// 领取远征奖励
        /// </summary>
        public bool ClaimReward(string expeditionId)
        {
            var expedition = _playerData.ActiveExpeditions
                .FirstOrDefault(e => e.ExpeditionId == expeditionId);
            
            if (expedition == null || !expedition.Completed || expedition.Claimed)
                return false;
            
            // 查找对应结果
            var result = _playerData.History
                .FirstOrDefault(r => r.MountId == expedition.MountId && 
                    r.CompletedAt >= expedition.StartTime && 
                    r.CompletedAt <= expedition.StartTime.AddMinutes(expedition.DurationMinutes + 1));
            
            if (result != null)
            {
                GrantReward(result);
            }
            
            expedition.Claimed = true;
            _playerData.ActiveExpeditions.Remove(expedition);
            
            GD.Print($"[MountExpedition] Claimed reward for: {expeditionId}");
            return true;
        }
        
        /// <summary>
        /// 获取活跃远征进度
        /// </summary>
        public List<Godot.Collections.Dictionary> GetExpeditionProgress()
        {
            var progress = new List<Godot.Collections.Dictionary>();
            
            foreach (var expedition in _playerData.ActiveExpeditions)
            {
                var zone = MountExpeditionDatabase.GetZone(expedition.ZoneId);
                if (zone == null) continue;
                
                var endTime = expedition.StartTime.AddMinutes(expedition.DurationMinutes);
                float progressPercent = (float)(DateTime.Now - expedition.StartTime).TotalMinutes / expedition.DurationMinutes;
                progressPercent = Mathf.Clamp(progressPercent, 0f, 1f);
                
                progress.Add(new Godot.Collections.Dictionary
                {
                    { "expedition_id", expedition.ExpeditionId },
                    { "zone_name", zone.Name },
                    { "mount_id", expedition.MountId },
                    { "progress", progressPercent },
                    { "completed", expedition.Completed },
                    { "claimed", expedition.Claimed },
                    { "remaining_minutes", Mathf.Max(0, (int)(endTime - DateTime.Now).TotalMinutes) }
                });
            }
            
            return progress;
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Godot.Collections.Dictionary GetStatistics()
        {
            return new Godot.Collections.Dictionary
            {
                { "total_expeditions", _playerData.TotalExpeditions },
                { "total_gold_earned", _playerData.TotalGoldEarned },
                { "total_exp_earned", _playerData.TotalExpEarned },
                { "active_count", _playerData.ActiveExpeditions.Count },
                { "history_count", _playerData.History.Count }
            };
        }
        
        /// <summary>
        /// 获取存档数据
        /// </summary>
        public Godot.Collections.Dictionary GetSaveData()
        {
            var activeList = new List<Godot.Collections.Dictionary>();
            foreach (var exp in _playerData.ActiveExpeditions)
            {
                activeList.Add(new Godot.Collections.Dictionary
                {
                    { "expedition_id", exp.ExpeditionId },
                    { "zone_id", exp.ZoneId },
                    { "mount_id", exp.MountId },
                    { "start_time", exp.StartTime.ToString("o") },
                    { "duration_minutes", exp.DurationMinutes },
                    { "completed", exp.Completed },
                    { "claimed", exp.Claimed }
                });
            }
            
            var historyList = new List<Godot.Collections.Dictionary>();
            foreach (var result in _playerData.History)
            {
                historyList.Add(new Godot.Collections.Dictionary
                {
                    { "zone_id", result.ZoneId },
                    { "success", result.Success },
                    { "gold_reward", result.GoldReward },
                    { "exp_reward", result.ExpReward },
                    { "item_rewards", new Godot.Collections.Array(result.ItemRewards) },
                    { "mount_id", result.MountId },
                    { "completed_at", result.CompletedAt.ToString("o") }
                });
            }
            
            var zoneCompletions = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.ZoneCompletions)
            {
                zoneCompletions[kvp.Key] = kvp.Value;
            }
            
            return new Godot.Collections.Dictionary
            {
                { "active_expeditions", new Godot.Collections.Array(activeList) },
                { "history", new Godot.Collections.Array(historyList) },
                { "total_expeditions", _playerData.TotalExpeditions },
                { "total_gold_earned", _playerData.TotalGoldEarned },
                { "total_exp_earned", _playerData.TotalExpEarned },
                { "zone_completions", zoneCompletions }
            };
        }
        
        /// <summary>
        /// 加载存档数据
        /// </summary>
        public void LoadSaveData(Godot.Collections.Dictionary data)
        {
            _playerData = new MountExpeditionData.PlayerExpeditionData();
            
            if (data.Contains("total_expeditions"))
                _playerData.TotalExpeditions = (int)data["total_expeditions"];
            if (data.Contains("total_gold_earned"))
                _playerData.TotalGoldEarned = (int)data["total_gold_earned"];
            if (data.Contains("total_exp_earned"))
                _playerData.TotalExpEarned = (int)data["total_exp_earned"];
            
            // 加载活跃远征
            if (data.Contains("active_expeditions"))
            {
                var activeList = (Godot.Collections.Array)data["active_expeditions"];
                foreach (Godot.Collections.Dictionary expData in activeList)
                {
                    var exp = new MountExpeditionData.ActiveExpedition
                    {
                        ExpeditionId = (string)expData["expedition_id"],
                        ZoneId = (string)expData["zone_id"],
                        MountId = (string)expData["mount_id"],
                        StartTime = DateTime.Parse((string)expData["start_time"]),
                        DurationMinutes = (int)expData["duration_minutes"],
                        Completed = (bool)expData["completed"],
                        Claimed = (bool)expData["claimed"]
                    };
                    _playerData.ActiveExpeditions.Add(exp);
                }
            }
            
            // 加载历史记录
            if (data.Contains("history"))
            {
                var historyList = (Godot.Collections.Array)data["history"];
                foreach (Godot.Collections.Dictionary resultData in historyList)
                {
                    var result = new MountExpeditionData.ExpeditionResult
                    {
                        ZoneId = (string)resultData["zone_id"],
                        Success = (bool)resultData["success"],
                        GoldReward = (int)resultData["gold_reward"],
                        ExpReward = (int)resultData["exp_reward"],
                        MountId = (string)resultData["mount_id"],
                        CompletedAt = DateTime.Parse((string)resultData["completed_at"])
                    };
                    
                    if (resultData.Contains("item_rewards"))
                    {
                        var items = (Godot.Collections.Array)resultData["item_rewards"];
                        foreach (string item in items)
                            result.ItemRewards.Add(item);
                    }
                    
                    _playerData.History.Add(result);
                }
            }
            
            // 加载区域完成次数
            if (data.Contains("zone_completions"))
            {
                var zoneCompletions = (Godot.Collections.Dictionary)data["zone_completions"];
                foreach (var key in zoneCompletions.Keys)
                {
                    _playerData.ZoneCompletions[(string)key] = (int)zoneCompletions[key];
                }
            }
        }
    }
}
