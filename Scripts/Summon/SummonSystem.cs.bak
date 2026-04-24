using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database;
using Godot;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// 召唤系统 - 管理玩家的召唤物
    /// </summary>
    public partial class SummonSystem : BaseSystem
    {
        private static SummonSystem _instance;
        public static new SummonSystem Instance
        {
            get => _instance;
            private set => _instance = value;
        }

        private PlayerSummonData _playerData;
        private SummonStatistics _statistics;
        private List<SummonSession> _activeSessions;
        private Dictionary<string, System.Timers.Timer> _summonTimers;

        public event Action<string> OnSummonUnlocked;
        public event Action<string> OnSummonActivated;
        public event Action<string> OnSummonDismissed;
        public event Action<string, int> OnSummonDamaged;
        public event Action<string> OnSummonLevelUp;
        public event Action<SummonSession> OnSessionEnded;

        public PlayerSummonData PlayerData => _playerData;
        public SummonStatistics Statistics => _statistics;

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            _playerData = new PlayerSummonData
            {
                UnlockedSummons = new List<UnlockedSummon>(),
                ActiveSummons = new List<ActiveSummon>(),
                MaxActiveSummons = 3,
                TotalSummons = 0,
                TotalDamageDealt = 0
            };
            _statistics = new SummonStatistics
            {
                TotalSummons = 0,
                TotalDamageDealt = 0,
                TotalKills = 0,
                TotalActiveTime = TimeSpan.Zero,
                SummonsByType = new Dictionary<SummonType, int>(),
                SummonsByRarity = new Dictionary<SummonRarity, int>(),
                HighestDamage = 0
            };
            _activeSessions = new List<SummonSession>();
            _summonTimers = new Dictionary<string, System.Timers.Timer>();
            LoadData();
        }
        
        protected override void Initialize()
        {
            GD.Print("[SummonSystem] Initialized");
        }

        /// <summary>
        /// 解锁召唤物
        /// </summary>
        public bool UnlockSummon(string summonId)
        {
            var summon = SummonDatabase.GetSummon(summonId);
            if (summon == null) return false;

            var existing = _playerData.UnlockedSummons.FirstOrDefault(s => s.SummonId == summonId);
            if (existing != null) return false;

            var unlocked = new UnlockedSummon
            {
                SummonId = summonId,
                UnlockTime = DateTime.Now,
                UseCount = 0,
                TotalDamage = 0,
                TotalKills = 0
            };
            _playerData.UnlockedSummons.Add(unlocked);

            // Update statistics
            if (!_statistics.SummonsByType.ContainsKey(summon.Type))
                _statistics.SummonsByType[summon.Type] = 0;
            _statistics.SummonsByType[summon.Type]++;

            if (!_statistics.SummonsByRarity.ContainsKey(summon.Rarity))
                _statistics.SummonsByRarity[summon.Rarity] = 0;
            _statistics.SummonsByRarity[summon.Rarity]++;

            OnSummonUnlocked?.Invoke(summonId);
            return true;
        }

        /// <summary>
        /// 激活召唤物
        /// </summary>
        public bool ActivateSummon(string summonId, int playerLevel)
        {
            var summon = SummonDatabase.GetSummon(summonId);
            if (summon == null) return false;

            if (summon.LevelRequirement > playerLevel)
    
            if (_playerData.ActiveSummons.Count >= _playerData.MaxActiveSummons)
    
            var unlocked = _playerData.UnlockedSummons.FirstOrDefault(s => s.SummonId == summonId);
            if (unlocked == null)
    
            var active = _playerData.ActiveSummons.FirstOrDefault(a => a.SummonId == summonId && a.State == SummonState.Active);
            if (active != null)
    
            // Check cooldown
            var onCooldown = _playerData.ActiveSummons.FirstOrDefault(a => a.SummonId == summonId && a.State == SummonState.Cooldown);
            if (onCooldown != null && DateTime.Now < onCooldown.CooldownEnd)
    
            var newActive = new ActiveSummon
            {
                SummonId = summonId,
                State = SummonState.Active,
                CurrentHealth = GetSummonMaxHealth(summon, playerLevel),
                Level = Math.Min(playerLevel / 5 + 1, 10),
                Experience = 0,
                ActiveTime = DateTime.Now,
                CooldownEnd = DateTime.MinValue
            };

            _playerData.ActiveSummons.Add(newActive);
            unlocked.UseCount++;
            _playerData.TotalSummons++;
            _statistics.TotalSummons++;

            // Create session
            var session = new SummonSession
            {
                StartTime = DateTime.Now,
                SummonIds = new List<string> { summonId },
                DamageDealt = 0,
                EnemiesKilled = 0
            };
            _activeSessions.Add(session);

            // Start duration timer
            StartSummonTimer(summonId, summon.Duration);

            OnSummonActivated?.Invoke(summonId);
            return true;
        }

        /// <summary>
        /// 解散召唤物
        /// </summary>
        public bool DismissSummon(string summonId)
        {
            var active = _playerData.ActiveSummons.FirstOrDefault(a => a.SummonId == summonId && a.State == SummonState.Active);
            if (active == null) return false;

            var summon = SummonDatabase.GetSummon(summonId);
            if (summon == null) return false;

            // Stop timer
            StopSummonTimer(summonId);

            // Set cooldown
            active.State = SummonState.Cooldown;
            active.CooldownEnd = DateTime.Now.AddSeconds(summon.Duration * 0.5);

            // Update statistics
            var session = _activeSessions.LastOrDefault(s => s.SummonIds.Contains(summonId) && s.EndTime == default);
            if (session != null)
            {
                session.EndTime = DateTime.Now;
                session.Duration = session.EndTime - session.StartTime;
                _statistics.TotalActiveTime += session.Duration;
                OnSessionEnded?.Invoke(session);
            }

            // Update unlocked summon stats
            var unlocked = _playerData.UnlockedSummons.FirstOrDefault(u => u.SummonId == summonId);
            if (unlocked != null && session != null)
            {
                unlocked.TotalDamage += session.DamageDealt;
                unlocked.TotalKills += session.EnemiesKilled;
            }

            OnSummonDismissed?.Invoke(summonId);
            return true;
        }

        /// <summary>
        /// 对召唤物造成伤害（敌人攻击召唤物）
        /// </summary>
        public void DamageSummon(string summonId, int damage)
        {
            var active = _playerData.ActiveSummons.FirstOrDefault(a => a.SummonId == summonId && a.State == SummonState.Active);
            if (active == null) return;

            active.CurrentHealth -= damage;

            if (active.CurrentHealth <= 0)
            {
                DismissSummon(summonId);
            }
        }

        /// <summary>
        /// 召唤物造成伤害
        /// </summary>
        public int SummonAttack(string summonId, int baseDamage)
        {
            var active = _playerData.ActiveSummons.FirstOrDefault(a => a.SummonId == summonId && a.State == SummonState.Active);
            if (active == null) return 0;

            var summon = SummonDatabase.GetSummon(summonId);
            if (summon == null) return 0;

            var playerData = GetPlayerData();
            var level = active.Level;

            // Calculate damage with rarity and level bonuses
            var rarityMultiplier = SummonDatabase.GetRarityStatMultiplier(summon.Rarity);
            var levelMultiplier = 1.0f + (level * 0.1f);
            var finalDamage = (int)(baseDamage * summon.BaseStats.Attack * rarityMultiplier * levelMultiplier * 0.1f);

            // Update statistics
            _playerData.TotalDamageDealt += finalDamage;
            _statistics.TotalDamageDealt += finalDamage;

            var session = _activeSessions.LastOrDefault(s => s.SummonIds.Contains(summonId) && s.EndTime == default);
            if (session != null)
            {
                session.DamageDealt += finalDamage;
            }

            if (finalDamage > _statistics.HighestDamage)
                _statistics.HighestDamage = finalDamage;

            OnSummonDamaged?.Invoke(summonId, finalDamage);
            return finalDamage;
        }

        /// <summary>
        /// 使用召唤物技能
        /// </summary>
        public int UseSkill(string summonId, string skillId, int baseDamage = 0)
        {
            var active = _playerData.ActiveSummons.FirstOrDefault(a => a.SummonId == summonId && a.State == SummonState.Active);
            if (active == null) return 0;

            var summon = SummonDatabase.GetSummon(summonId);
            if (summon == null) return 0;

            var skill = summon.Skills.FirstOrDefault(s => s.SkillId == skillId);
            if (skill == null) return 0;

            var damage = (int)(baseDamage * skill.DamageMultiplier);
            SummonAttack(summonId, damage);

            return damage;
        }

        /// <summary>
        /// 检查是否有活跃召唤物
        /// </summary>
        public bool HasActiveSummon(string summonId)
        {
            return _playerData.ActiveSummons.Any(a => a.SummonId == summonId && a.State == SummonState.Active);
        }

        /// <summary>
        /// 获取召唤物最大生命值
        /// </summary>
        public int GetSummonMaxHealth(Summon summon, int playerLevel)
        {
            var rarityMultiplier = SummonDatabase.GetRarityStatMultiplier(summon.Rarity);
            var levelMultiplier = 1.0f + (playerLevel / 5 + 1) * 0.1f;
            return (int)(summon.BaseStats.Health * rarityMultiplier * levelMultiplier);
        }

        /// <summary>
        /// 获取可用召唤物列表
        /// </summary>
        public List<Summon> GetAvailableSummons(int playerLevel)
        {
            return SummonDatabase.GetAvailableSummons(playerLevel);
        }

        /// <summary>
        /// 获取已解锁召唤物详情
        /// </summary>
        public List<Summon> GetUnlockedSummons()
        {
            var result = new List<Summon>();
            foreach (var unlocked in _playerData.UnlockedSummons)
            {
                var summon = SummonDatabase.GetSummon(unlocked.SummonId);
                if (summon != null)
                    result.Add(summon);
            }
            return result;
        }

        /// <summary>
        /// 获取活跃召唤物详情
        /// </summary>
        public List<Summon> GetActiveSummons()
        {
            var result = new List<Summon>();
            foreach (var active in _playerData.ActiveSummons.Where(a => a.State == SummonState.Active))
            {
                var summon = SummonDatabase.GetSummon(active.SummonId);
                if (summon != null)
                    result.Add(summon);
            }
            return result;
        }

        /// <summary>
        /// 获取召唤物统计数据
        /// </summary>
        public SummonStatistics GetStatistics()
        {
            // Update most used
            if (_playerData.UnlockedSummons.Count > 0)
            {
                _statistics.MostUsedSummonId = _playerData.UnlockedSummons
                    .OrderByDescending(u => u.UseCount)
                    .First().SummonId;
            }
            return _statistics;
        }

        /// <summary>
        /// 增加最大召唤数量
        /// </summary>
        public void IncreaseMaxSummons(int amount = 1)
        {
            _playerData.MaxActiveSummons = Math.Min(_playerData.MaxActiveSummons + amount, 6);
        }

        private void StartSummonTimer(string summonId, int durationSeconds)
        {
            var timer = new System.Timers.Timer(durationSeconds * 1000);
            timer.Elapsed += (s, e) =>
            {
                DismissSummon(summonId);
            };
            timer.AutoReset = false;
            timer.Start();

            _summonTimers[summonId] = timer;
        }

        private void StopSummonTimer(string summonId)
        {
            if (_summonTimers.ContainsKey(summonId))
            {
                _summonTimers[summonId].Stop();
                _summonTimers[summonId].Dispose();
                _summonTimers.Remove(summonId);
            }
        }

        private PlayerSummonData GetPlayerData()
        {
            return _playerData;
        }

        /// <summary>
        /// 导出存档数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>
            {
                { "unlocked_summons", _playerData.UnlockedSummons },
                { "active_summons", _playerData.ActiveSummons },
                { "max_active_summons", _playerData.MaxActiveSummons },
                { "total_summons", _playerData.TotalSummons },
                { "total_damage_dealt", _playerData.TotalDamageDealt },
                { "statistics", _statistics }
            };
        }

        /// <summary>
        /// 导入存档数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data.ContainsKey("unlocked_summons"))
                _playerData.UnlockedSummons = (List<UnlockedSummon>)data["unlocked_summons"];
            if (data.ContainsKey("active_summons"))
                _playerData.ActiveSummons = (List<ActiveSummon>)data["active_summons"];
            if (data.ContainsKey("max_active_summons"))
                _playerData.MaxActiveSummons = (int)data["max_active_summons"];
            if (data.ContainsKey("total_summons"))
                _playerData.TotalSummons = (int)data["total_summons"];
            if (data.ContainsKey("total_damage_dealt"))
                _playerData.TotalDamageDealt = (int)data["total_damage_dealt"];
            if (data.ContainsKey("statistics"))
                _statistics = (SummonStatistics)data["statistics"];
        }
    }
}
