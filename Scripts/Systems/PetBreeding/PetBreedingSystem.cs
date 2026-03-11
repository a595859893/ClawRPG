using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 宠物繁殖系统 - 管理宠物繁殖功能
    /// </summary>
    public partial class PetBreedingSystem : Node
    {
        public static PetBreedingSystem Instance { get; private set; }
        
        // 玩家繁殖数据
        private PetBreedingData.PlayerBreedingData _playerData = new PetBreedingData.PlayerBreedingData();
        
        // 信号
        public static Signal<PetBreedingData.BreedingInstance> BreedingStarted { get; } = new Signal<PetBreedingData.BreedingInstance>();
        public static Signal<PetBreedingData.BreedingInstance> BreedingCompleted { get; } = new Signal<PetBreedingData.BreedingInstance>();
        public static Signal<PetBreedingData.BreedingInstance> BreedingFailed { get; } = new Signal<PetBreedingData.BreedingInstance>();
        
        public override void _Ready()
        {
            Instance = this;
        }
        
        /// <summary>
        /// 开始繁殖
        /// </summary>
        public bool StartBreeding(string parent1Id, string parent2Id, PetBreedingData.BreedingType type)
        {
            var parent1 = GetPetInfo(parent1Id);
            var parent2 = GetPetInfo(parent2Id);
            
            if (parent1 == null || parent2 == null)
            {
                GD.PrintErr("[PetBreeding] Invalid parent pets");
                return false;
            }
            
            // 检查等级要求
            var config = PetBreedingDatabase.GetConfig(type);
            if (parent1.Level < config.MinParentLevel || parent2.Level < config.MinParentLevel)
            {
                GD.PrintErr($"[PetBreeding] Parent pets must be level {config.MinParentLevel} or higher");
                return false;
            }
            
            // 检查金币
            if (!HasEnoughGold(config.GoldCost))
            {
                GD.PrintErr("[PetBreeding] Not enough gold");
                return false;
            }
            
            // 扣除金币
            DeductGold(config.GoldCost);
            
            // 创建繁殖实例
            var breeding = new PetBreedingData.BreedingInstance
            {
                Parent1Id = parent1Id,
                Parent2Id = parent2Id,
                Parent1 = parent1,
                Parent2 = parent2,
                Type = type,
                StartTime = DateTime.Now,
                DurationSeconds = config.BaseDuration,
                State = PetBreedingData.BreedingState.InProgress
            };
            
            _playerData.ActiveBreedings[breeding.InstanceId] = breeding;
            _playerData.TotalBreedings++;
            
            BreedingStarted.Emit(breeding);
            SaveBreedingData();
            
            GD.Print($"[PetBreeding] Started breeding: {parent1.PetName} x {parent2.PetName}");
            return true;
        }
        
        /// <summary>
        /// 处理繁殖进度
        /// </summary>
        public void _Process(double delta)
        {
            var completed = new List<string>();
            
            foreach (var kvp in _playerData.ActiveBreedings)
            {
                var breeding = kvp.Value;
                if (breeding.State != PetBreedingData.BreedingState.InProgress)
                    continue;
                
                var elapsed = (DateTime.Now - breeding.StartTime).TotalSeconds;
                if (elapsed >= breeding.DurationSeconds)
                {
                    // 完成繁殖
                    CompleteBreeding(breeding);
                    completed.Add(kvp.Key);
                }
            }
            
            foreach (var id in completed)
            {
                _playerData.ActiveBreedings.Remove(id);
            }
        }
        
        /// <summary>
        /// 完成繁殖
        /// </summary>
        private void CompleteBreeding(PetBreedingData.BreedingInstance breeding)
        {
            var config = PetBreedingDatabase.GetConfig(breeding.Type);
            
            // 计算成功率
            float successRate = PetBreedingDatabase.CalculateSuccessRate(
                breeding.Type, 
                breeding.Parent1.Level, 
                breeding.Parent2.Level
            );
            
            bool success = new Random().NextDouble() < successRate;
            
            breeding.State = PetBreedingData.BreedingState.Completed;
            breeding.Success = success;
            
            if (success)
            {
                // 生成后代
                string offspringRarity = PetBreedingDatabase.SelectOffspringRarity(
                    breeding.Parent1.Rarity, 
                    breeding.Parent2.Rarity,
                    config.LegendaryChance
                );
                
                int attack, defense, health, speed;
                PetBreedingDatabase.CalculateOffspringAttributes(
                    breeding.Parent1, 
                    breeding.Parent2, 
                    out attack, out defense, out health, out speed
                );
                
                // 创建后代宠物
                string offspringId = CreateOffspringPet(
                    breeding.Parent1.PetName + " & " + breeding.Parent2.PetName,
                    offspringRarity,
                    config.OffspringMinLevel,
                    attack, defense, health, speed
                );
                
                breeding.OffspringId = offspringId;
                _playerData.SuccessfulBreedings++;
                
                if (offspringRarity == "Legendary")
                {
                    _playerData.LegendaryBreedings++;
                }
                
                // 添加历史记录
                _playerData.History.Add(new PetBreedingData.BreedingRecord
                {
                    Parent1Name = breeding.Parent1.PetName,
                    Parent2Name = breeding.Parent2.PetName,
                    OffspringName = breeding.Parent1.PetName + " Child",
                    OffspringRarity = offspringRarity,
                    BreedingTime = DateTime.Now,
                    Success = true,
                    GoldCost = config.GoldCost
                });
                
                BreedingCompleted.Emit(breeding);
                GD.Print($"[PetBreeding] Breeding successful! Offspring: {offspringRarity}");
            }
            else
            {
                // 繁殖失败
                _playerData.History.Add(new PetBreedingData.BreedingRecord
                {
                    Parent1Name = breeding.Parent1.PetName,
                    Parent2Name = breeding.Parent2.PetName,
                    OffspringName = "Failed",
                    OffspringRarity = "None",
                    BreedingTime = DateTime.Now,
                    Success = false,
                    GoldCost = config.GoldCost
                });
                
                BreedingFailed.Emit(breeding);
                GD.Print("[PetBreeding] Breeding failed!");
            }
            
            SaveBreedingData();
        }
        
        /// <summary>
        /// 取消繁殖
        /// </summary>
        public bool CancelBreeding(string instanceId)
        {
            if (!_playerData.ActiveBreedings.ContainsKey(instanceId))
                return false;
            
            var breeding = _playerData.ActiveBreedings[instanceId];
            var config = PetBreedingDatabase.GetConfig(breeding.Type);
            
            // 返还部分金币 (50%)
            int refund = config.GoldCost / 2;
            AddGold(refund);
            
            breeding.State = PetBreedingData.BreedingState.Cancelled;
            _playerData.ActiveBreedings.Remove(instanceId);
            
            SaveBreedingData();
            GD.Print($"[PetBreeding] Breeding cancelled, refunded {refund} gold");
            return true;
        }
        
        /// <summary>
        /// 获取活跃繁殖列表
        /// </summary>
        public Array<PetBreedingData.BreedingInstance> GetActiveBreedings()
        {
            return new Array<PetBreedingData.BreedingInstance>(_playerData.ActiveBreedings.Values.ToList());
        }
        
        /// <summary>
        /// 获取繁殖历史
        /// </summary>
        public Array<PetBreedingData.BreedingRecord> GetBreedingHistory()
        {
            return new Array<PetBreedingData.BreedingRecord>(_playerData.History.OrderByDescending(r => r.BreedingTime).ToList());
        }
        
        /// <summary>
        /// 获取繁殖统计
        /// </summary>
        public Dictionary GetStatistics()
        {
            return new Dictionary
            {
                { "total_breedings", _playerData.TotalBreedings },
                { "successful_breedings", _playerData.SuccessfulBreedings },
                { "legendary_breedings", _playerData.LegendaryBreedings },
                { "success_rate", _playerData.TotalBreedings > 0 ? 
                    (float)_playerData.SuccessfulBreedings / _playerData.TotalBreedings : 0 }
            };
        }
        
        /// <summary>
        /// 获取繁殖进度
        /// </summary>
        public float GetBreedingProgress(string instanceId)
        {
            if (!_playerData.ActiveBreedings.ContainsKey(instanceId))
                return 0;
            
            var breeding = _playerData.ActiveBreedings[instanceId];
            var elapsed = (DateTime.Now - breeding.StartTime).TotalSeconds;
            return (float)Math.Min(elapsed / breeding.DurationSeconds, 1.0);
        }
        
        /// <summary>
        /// 剩余时间（秒）
        /// </summary>
        public int GetRemainingTime(string instanceId)
        {
            if (!_playerData.ActiveBreedings.ContainsKey(instanceId))
                return 0;
            
            var breeding = _playerData.ActiveBreedings[instanceId];
            var elapsed = (DateTime.Now - breeding.StartTime).TotalSeconds;
            return Math.Max(0, breeding.DurationSeconds - (int)elapsed);
        }
        
        #region 宠物系统集成
        
        private PetBreedingData.ParentPet GetPetInfo(string petId)
        {
            // 从宠物管理器获取宠物信息
            if (PetManager.Instance == null) return null;
            
            var pets = PetManager.Instance.GetPets();
            var pet = pets.FirstOrDefault(p => p.Id == petId);
            
            if (pet == null) return null;
            
            return new PetBreedingData.ParentPet
            {
                PetId = pet.Id,
                PetName = pet.Name,
                Level = pet.Level,
                Attack = pet.Attack,
                Defense = pet.Defense,
                Health = pet.Health,
                Speed = pet.Speed,
                CritRate = pet.CritRate,
                CritDamage = pet.CritDamage,
                Rarity = pet.Rarity,
                Element = pet.Element
            };
        }
        
        private string CreateOffspringPet(string name, string rarity, int level, int attack, int defense, int health, int speed)
        {
            // 通过宠物管理器创建新宠物
            // 这里需要集成宠物系统
            GD.Print($"[PetBreeding] Creating offspring: {name}, Rarity: {rarity}, Level: {level}");
            return Guid.NewGuid().ToString();
        }
        
        private bool HasEnoughGold(int amount)
        {
            // 需要集成玩家金币系统
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player == null) return false;
            
            var goldProperty = player.Get("Gold");
            if (goldProperty == null) return false;
            
            return (int)goldProperty >= amount;
        }
        
        private void DeductGold(int amount)
        {
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player == null) return;
            
            var goldProperty = player.Get("Gold");
            if (goldProperty != null)
            {
                player.Set("Gold", (int)goldProperty - amount);
            }
        }
        
        private void AddGold(int amount)
        {
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player == null) return;
            
            var goldProperty = player.Get("Gold");
            if (goldProperty != null)
            {
                player.Set("Gold", (int)goldProperty + amount);
            }
        }
        
        #endregion
        
        #region 存档支持
        
        public Dictionary GetSaveData()
        {
            var breedingList = new Array();
            foreach (var kvp in _playerData.ActiveBreedings)
            {
                var data = new Dictionary
                {
                    { "instance_id", kvp.Value.InstanceId },
                    { "parent1_id", kvp.Value.Parent1Id },
                    { "parent2_id", kvp.Value.Parent2Id },
                    { "type", (int)kvp.Value.Type },
                    { "start_time", kvp.Value.StartTime.ToString("o") },
                    { "duration", kvp.Value.DurationSeconds },
                    { "state", (int)kvp.Value.State }
                };
                breedingList.Add(data);
            }
            
            var historyList = new Array();
            foreach (var record in _playerData.History)
            {
                var data = new Dictionary
                {
                    { "parent1_name", record.Parent1Name },
                    { "parent2_name", record.Parent2Name },
                    { "offspring_name", record.OffspringName },
                    { "offspring_rarity", record.OffspringRarity },
                    { "time", record.BreedingTime.ToString("o") },
                    { "success", record.Success },
                    { "gold_cost", record.GoldCost }
                };
                historyList.Add(data);
            }
            
            return new Dictionary
            {
                { "active_breedings", breedingList },
                { "history", historyList },
                { "total_breedings", _playerData.TotalBreedings },
                { "successful_breedings", _playerData.SuccessfulBreedings },
                { "legendary_breedings", _playerData.LegendaryBreedings }
            };
        }
        
        public void LoadSaveData(Dictionary data)
        {
            if (data == null) return;
            
            _playerData = new PetBreedingData.PlayerBreedingData();
            
            // 加载活跃繁殖
            if (data.Contains("active_breedings"))
            {
                var breedingList = (Array)data["active_breedings"];
                foreach (Dictionary breedingData in breedingList)
                {
                    // 这里需要重新构建繁殖实例
                    // 简化处理：跳过正在进行的繁殖
                }
            }
            
            // 加载统计
            if (data.Contains("total_breedings"))
                _playerData.TotalBreedings = (int)data["total_breedings"];
            if (data.Contains("successful_breedings"))
                _playerData.SuccessfulBreedings = (int)data["successful_breedings"];
            if (data.Contains("legendary_breedings"))
                _playerData.LegendaryBreedings = (int)data["legendary_breedings"];
        }
        
        private void SaveBreedingData()
        {
            // 触发保存
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveGame();
            }
        }
        
        #endregion
    }
}
