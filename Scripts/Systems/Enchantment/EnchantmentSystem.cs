using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Systems.Meditation;

namespace ClawRPG.Scripts.Systems.Enchantment
{
    /// <summary>
    /// 附魔系统核心类
    /// </summary>
    public class EnchantmentSystem : BaseSystem
    {
        // 单例
        private static EnchantmentSystem _instance;
        public static EnchantmentSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("EnchantmentSystem: Instance is null!");
                }
                return _instance;
            }
        }
        
        // 玩家附魔进度
        private Dictionary<string, EnchantmentProgress> _playerProgress;
        
        // 当前活跃的附魔会话
        private Dictionary<string, EnchantmentSession> _activeSessions;
        
        // 专注力加成 (从冥想系统获取)
        private float _focusBonus = 0f;
        
        // 事件信号
        public Action EnchantmentStarted;
        public Action EnchantmentCompleted;
        public Action EnchantmentSuccess;
        public Action EnchantmentFailed;
        public Action EnchantmentUnlocked;
        public Action FocusPointsChanged;
        
        public override void _Ready()
        {
            _instance = this;
            _playerProgress = new Dictionary<string, EnchantmentProgress>();
            _activeSessions = new Dictionary<string, EnchantmentSession>();
            
            // 初始化数据库
            EnchantmentDatabase.Instance.Initialize();
            
            GD.Print("EnchantmentSystem: Initialized with " + EnchantmentDatabase.Instance.GetTotalCount() + " enchantments");
        }
        
        /// <summary>
        /// 获取或创建玩家进度
        /// </summary>
        public EnchantmentProgress GetOrCreateProgress(string playerId)
        {
            if (!_playerProgress.ContainsKey(playerId))
            {
                _playerProgress[playerId] = new EnchantmentProgress(playerId);
            }
            return _playerProgress[playerId];
        }
        
        /// <summary>
        /// 开始附魔过程
        /// </summary>
        public EnchantmentSession StartEnchantment(string playerId, string equipmentId, string enchantmentId)
        {
            var progress = GetOrCreateProgress(playerId);
            var enchantment = EnchantmentDatabase.Instance.GetEnchantmentById(enchantmentId);
            
            if (enchantment == null)
            {
                GD.PrintErr("EnchantmentSystem: Enchantment not found - " + enchantmentId);
                return null;
            }
            
            // 检查是否已解锁该附魔
            bool isUnlocked = progress.UnlockedEnchantments.Any(e => e.EnchantmentId == enchantmentId);
            
            // 创建会话
            var session = new EnchantmentSession
            {
                PlayerId = playerId,
                EquipmentId = equipmentId,
                EnchantmentId = enchantmentId,
                AttemptLevel = 1
            };
            
            _activeSessions[session.SessionId] = session;
            
            // 检查玩家货币是否足够
            // 这里假设有经济系统，实际实现需要检查
            int cost = GetEnchantmentCost(enchantment, progress.CurrentFocusPoints);
            
            EmitSignal(SignalName.EnchantmentStarted);
            
            // 记录使用次数
            if (!progress.Statistics.EnchantmentUsageCount.ContainsKey(enchantmentId))
            {
                progress.Statistics.EnchantmentUsageCount[enchantmentId] = 0;
            }
            progress.Statistics.EnchantmentUsageCount[enchantmentId]++;
            progress.Statistics.TotalAttempts++;
            progress.Statistics.TotalExpenses += cost;
            
            // 解锁附魔（如果未解锁）
            if (!isUnlocked)
            {
                progress.UnlockedEnchantments.Add(new UnlockedEnchantment
                {
                    EnchantmentId = enchantmentId
                });
                EmitSignal(SignalName.EnchantmentUnlocked);
            }
            
            // 增加专注力点数
            progress.CurrentFocusPoints += 1;
            EmitSignal(SignalName.FocusPointsChanged);
            
            return session;
        }
        
        /// <summary>
        /// 执行附魔判定
        /// </summary>
        public bool PerformEnchantment(string sessionId)
        {
            if (!_activeSessions.ContainsKey(sessionId))
            {
                GD.PrintErr("EnchantmentSystem: Session not found - " + sessionId);
                return false;
            }
            
            var session = _activeSessions[sessionId];
            var progress = GetOrCreateProgress(session.PlayerId);
            var enchantment = EnchantmentDatabase.Instance.GetEnchantmentById(session.EnchantmentId);
            
            if (enchantment == null)
            {
                return false;
            }
            
            // 计算实际成功率
            float successRate = CalculateSuccessRate(enchantment, progress.CurrentFocusPoints);
            
            // 随机判定
            var random = new Random();
            float roll = (float)random.NextDouble() * 100f;
            bool success = roll <= successRate;
            
            session.IsCompleted = true;
            session.WasSuccessful = success;
            
            if (success)
            {
                // 应用附魔到装备
                ApplyEnchantmentToEquipment(session.PlayerId, session.EquipmentId, session.EnchantmentId, session.AttemptLevel);
                
                // 更新统计
                progress.Statistics.TotalSuccesses++;
                
                // 更新附魔使用记录
                var unlockedEnchantment = progress.UnlockedEnchantments.FirstOrDefault(e => e.EnchantmentId == session.EnchantmentId);
                if (unlockedEnchantment != null)
                {
                    unlockedEnchantment.UsageCount++;
                    unlockedEnchantment.SuccessCount++;
                }
                
                // 消耗专注力点数
                progress.CurrentFocusPoints = Mathf.Max(0, progress.CurrentFocusPoints - 1);
                
                EmitSignal(SignalName.EnchantmentSuccess);
            }
            else
            {
                progress.Statistics.TotalFailures++;
                
                // 更新附魔使用记录
                var unlockedEnchantment = progress.UnlockedEnchantments.FirstOrDefault(e => e.EnchantmentId == session.EnchantmentId);
                if (unlockedEnchantment != null)
                {
                    unlockedEnchantment.UsageCount++;
                }
                
                // 附魔失败不消耗专注力，但可能降级（可选功能）
                
                EmitSignal(SignalName.EnchantmentFailed);
            }
            
            progress.TotalEnchantmentsPerformed++;
            
            EmitSignal(SignalName.EnchantmentCompleted);
            
            return success;
        }
        
        /// <summary>
        /// 计算附魔成功率
        /// </summary>
        private float CalculateSuccessRate(EnchantmentRecord enchantment, int focusPoints)
        {
            float baseRate = enchantment.SuccessRate;
            
            // 专注力加成 (每点专注力增加1%成功率，上限20%)
            float focusBonus = Mathf.Min(focusPoints * 1.0f, 20f);
            
            // 附魔等级加成
            float tierBonus = 0f;
            switch (enchantment.Tier)
            {
                case EnchantmentTier.Common:
                    tierBonus = 5f;
                    break;
                case EnchantmentTier.Uncommon:
                    tierBonus = 0f;
                    break;
                case EnchantmentTier.Rare:
                    tierBonus = -5f;
                    break;
                case EnchantmentTier.Epic:
                    tierBonus = -10f;
                    break;
                case EnchantmentTier.Legendary:
                    tierBonus = -15f;
                    break;
            }
            
            return Mathf.Clamp(baseRate + focusBonus + tierBonus, 5f, 100f);
        }
        
        /// <summary>
        /// 获取附魔费用
        /// </summary>
        private int GetEnchantmentCost(EnchantmentRecord enchantment, int focusPoints)
        {
            int baseCost = enchantment.EnchantmentCost;
            
            // 专注力可以抵消部分费用
            int discount = Mathf.Min(focusPoints * 10, baseCost / 2);
            
            return Mathf.Max(baseCost - discount, 10);
        }
        
        /// <summary>
        /// 应用附魔到装备
        /// </summary>
        private void ApplyEnchantmentToEquipment(string playerId, string equipmentId, string enchantmentId, int level)
        {
            var progress = GetOrCreateProgress(playerId);
            
            // 检查装备是否已有附魔
            var existingEnchantment = progress.ActiveEnchantments.FirstOrDefault(e => e.EquipmentId == equipmentId);
            
            if (existingEnchantment != null)
            {
                // 更新现有附魔
                existingEnchantment.EnchantmentId = enchantmentId;
                existingEnchantment.EnchantmentLevel = level;
                existingEnchantment.EnchantedAt = DateTime.Now;
            }
            else
            {
                // 添加新附魔
                progress.ActiveEnchantments.Add(new EquipmentEnchantment
                {
                    EquipmentId = equipmentId,
                    EnchantmentId = enchantmentId,
                    EnchantmentLevel = level
                });
            }
        }
        
        /// <summary>
        /// 获取装备的附魔效果
        /// </summary>
        public Dictionary<EnchantmentEffect, float> GetEquipmentEnchantmentEffects(string playerId, string equipmentId)
        {
            var effects = new Dictionary<EnchantmentEffect, float>();
            var progress = GetOrCreateProgress(playerId);
            
            var equipmentEnchantment = progress.ActiveEnchantments.FirstOrDefault(e => e.EquipmentId == equipmentId);
            
            if (equipmentEnchantment != null)
            {
                var enchantment = EnchantmentDatabase.Instance.GetEnchantmentById(equipmentEnchantment.EnchantmentId);
                if (enchantment != null)
                {
                    effects[enchantment.PrimaryEffect] = enchantment.PrimaryEffectValue * equipmentEnchantment.EnchantmentLevel;
                    
                    if (enchantment.SecondaryEffect.HasValue)
                    {
                        effects[enchantment.SecondaryEffect.Value] = enchantment.SecondaryEffectValue * equipmentEnchantment.EnchantmentLevel;
                    }
                }
            }
            
            return effects;
        }
        
        /// <summary>
        /// 移除装备附魔
        /// </summary>
        public bool RemoveEnchantment(string playerId, string equipmentId)
        {
            var progress = GetOrCreateProgress(playerId);
            
            var enchantment = progress.ActiveEnchantments.FirstOrDefault(e => e.EquipmentId == equipmentId);
            
            if (enchantment != null)
            {
                progress.ActiveEnchantments.Remove(enchantment);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取玩家统计信息
        /// </summary>
        public EnchantmentStatistics GetStatistics(string playerId)
        {
            var progress = GetOrCreateProgress(playerId);
            return progress.Statistics;
        }
        
        /// <summary>
        /// 获取已解锁的附魔列表
        /// </summary>
        public List<EnchantmentRecord> GetUnlockedEnchantments(string playerId)
        {
            var progress = GetOrCreateProgress(playerId);
            var result = new List<EnchantmentRecord>();
            
            foreach (var unlocked in progress.UnlockedEnchantments)
            {
                var enchantment = EnchantmentDatabase.Instance.GetEnchantmentById(unlocked.EnchantmentId);
                if (enchantment != null)
                {
                    result.Add(enchantment);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取玩家专注力点数
        /// </summary>
        public int GetFocusPoints(string playerId)
        {
            var progress = GetOrCreateProgress(playerId);
            return progress.CurrentFocusPoints;
        }
        
        /// <summary>
        /// 从冥想系统同步专注力加成
        /// </summary>
        public void SyncFocusBonus()
        {
            if (MeditationUI.Instance != null)
            {
                // 假设冥想系统有专注力加成
                _focusBonus = 0f; // 简化实现
            }
        }
        
        /// <summary>
        /// 保存玩家进度
        /// </summary>
        public Dictionary<string, object> SaveProgress(string playerId)
        {
            var data = new Dictionary<string, object>();
            var progress = GetOrCreateProgress(playerId);
            
            data["player_id"] = progress.PlayerId;
            data["total_enchantments"] = progress.TotalEnchantmentsPerformed;
            data["focus_points"] = progress.CurrentFocusPoints;
            
            // 序列化已解锁附魔
            var unlockedList = new List<Dictionary<string, object>>();
            foreach (var unlocked in progress.UnlockedEnchantments)
            {
                unlockedList.Add(new Dictionary<string, object>
                {
                    {"enchantment_id", unlocked.EnchantmentId},
                    {"unlocked_at", unlocked.UnlockedAt.ToString("o")},
                    {"usage_count", unlocked.UsageCount},
                    {"success_count", unlocked.SuccessCount}
                });
            }
            data["unlocked_enchantments"] = unlockedList;
            
            // 序列化装备附魔
            var equipmentList = new List<Dictionary<string, object>>();
            foreach (var equip in progress.ActiveEnchantments)
            {
                equipmentList.Add(new Dictionary<string, object>
                {
                    {"equipment_id", equip.EquipmentId},
                    {"enchantment_id", equip.EnchantmentId},
                    {"level", equip.EnchantmentLevel},
                    {"enchanted_at", equip.EnchantedAt.ToString("o")},
                    {"is_permanent", equip.IsPermanent}
                });
            }
            data["equipment_enchantments"] = equipmentList;
            
            // 序列化统计
            data["statistics"] = new Dictionary<string, object>
            {
                {"total_attempts", progress.Statistics.TotalAttempts},
                {"total_successes", progress.Statistics.TotalSuccesses},
                {"total_failures", progress.Statistics.TotalFailures},
                {"total_expenses", progress.Statistics.TotalExpenses},
                {"highest_tier", progress.Statistics.HighestTierUnlocked}
            };
            
            return data;
        }
        
        /// <summary>
        /// 加载玩家进度
        /// </summary>
        public void LoadProgress(string playerId, Dictionary<string, object> data)
        {
            var progress = GetOrCreateProgress(playerId);
            
            if (data.ContainsKey("total_enchantments"))
                progress.TotalEnchantmentsPerformed = Convert.ToInt32(data["total_enchantments"]);
            
            if (data.ContainsKey("focus_points"))
                progress.CurrentFocusPoints = Convert.ToInt32(data["focus_points"]);
            
            // 加载已解锁附魔
            if (data.ContainsKey("unlocked_enchantments"))
            {
                var unlockedList = data["unlocked_enchantments"] as List<object>;
                foreach (var item in unlockedList)
                {
                    var dict = item as Dictionary<string, object>;
                    if (dict != null)
                    {
                        var unlocked = new UnlockedEnchantment
                        {
                            EnchantmentId = Convert.ToString(dict["enchantment_id"]),
                            UnlockedAt = DateTime.Parse(Convert.ToString(dict["unlocked_at"])),
                            UsageCount = Convert.ToInt32(dict["usage_count"]),
                            SuccessCount = Convert.ToInt32(dict["success_count"])
                        };
                        progress.UnlockedEnchantments.Add(unlocked);
                    }
                }
            }
            
            // 加载装备附魔
            if (data.ContainsKey("equipment_enchantments"))
            {
                var equipmentList = data["equipment_enchantments"] as List<object>;
                foreach (var item in equipmentList)
                {
                    var dict = item as Dictionary<string, object>;
                    if (dict != null)
                    {
                        var equip = new EquipmentEnchantment
                        {
                            EquipmentId = Convert.ToString(dict["equipment_id"]),
                            EnchantmentId = Convert.ToString(dict["enchantment_id"]),
                            EnchantmentLevel = Convert.ToInt32(dict["level"]),
                            EnchantedAt = DateTime.Parse(Convert.ToString(dict["enchanted_at"])),
                            IsPermanent = Convert.ToBoolean(dict["is_permanent"])
                        };
                        progress.ActiveEnchantments.Add(equip);
                    }
                }
            }
            
            // 加载统计
            if (data.ContainsKey("statistics"))
            {
                var stats = data["statistics"] as Dictionary<string, object>;
                if (stats != null)
                {
                    progress.Statistics.TotalAttempts = Convert.ToInt32(stats["total_attempts"]);
                    progress.Statistics.TotalSuccesses = Convert.ToInt32(stats["total_successes"]);
                    progress.Statistics.TotalFailures = Convert.ToInt32(stats["total_failures"]);
                    progress.Statistics.TotalExpenses = Convert.ToInt32(stats["total_expenses"]);
                    progress.Statistics.HighestTierUnlocked = Convert.ToInt32(stats["highest_tier"]);
                }
            }
        }
    }
    
}
